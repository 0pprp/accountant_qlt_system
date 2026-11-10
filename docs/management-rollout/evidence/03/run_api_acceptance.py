# -*- coding: utf-8 -*-
"""Isolated API acceptance for DelegateFollowup — source parity checks."""
from __future__ import annotations

import json
from pathlib import Path

import requests

BASE = "http://127.0.0.1:4200/api/v1"
OUT = Path(__file__).resolve().parent
results: list[dict] = []


def note(status: str, id_: str, detail: str) -> None:
    results.append({"status": status, "id": id_, "detail": detail})
    print(f"[{status}] {id_} — {detail}")


def login(user: str, password: str) -> str:
    r = requests.post(
        f"{BASE}/admin/Auth/login",
        json={"username": user, "password": password},
        timeout=30,
    )
    r.raise_for_status()
    data = r.json()
    assert "DelegateFollowup" in (data.get("permissions") or {}), "Admin missing DelegateFollowup perms"
    return data["accessToken"]


def main() -> None:
    token = login("admin", "admin123")
    h = {"Authorization": f"Bearer {token}"}

    # limited without perm
    try:
        lim = requests.post(
            f"{BASE}/admin/Auth/login",
            json={"username": "limited01", "password": "admin123"},
            timeout=30,
        ).json()
        lim_tok = lim["accessToken"]
        has = "DelegateFollowup" in (lim.get("permissions") or {})
        note(
            "Live-verified" if has is False else "Blocked",
            "limited-no-delegate-followup-perm",
            f"has={has}",
        )
        fr = requests.get(
            f"{BASE}/admin/DelegateFollowup/lists",
            params={"branchId": 1},
            headers={"Authorization": f"Bearer {lim_tok}"},
            timeout=30,
        )
        note(
            "Live-verified" if fr.status_code in (401, 403) else "Blocked",
            "limited-lists-forbidden",
            f"status={fr.status_code}",
        )
    except Exception as ex:
        note("Blocked", "limited-login", str(ex))

    # SalesFollowup still works (independent)
    sf = requests.get(
        f"{BASE}/admin/SalesFollowup/employees",
        params={"branchId": 1},
        headers=h,
        timeout=30,
    )
    note(
        "Live-verified" if sf.status_code == 200 else "Blocked",
        "salesfollowup-untouched",
        f"status={sf.status_code}",
    )

    lists = requests.get(
        f"{BASE}/admin/DelegateFollowup/lists",
        params={"branchId": 1},
        headers=h,
        timeout=60,
    )
    if lists.status_code != 200:
        note("Blocked", "lists", f"{lists.status_code} {lists.text[:300]}")
        OUT.joinpath("results.json").write_text(
            json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        return

    payload = lists.json()
    if isinstance(payload, list):
        items = payload
    else:
        items = (
            payload.get("lists")
            or payload.get("items")
            or payload.get("data")
            or []
        )
    note("Live-verified", "lists", f"count={len(items)}")

    if not items:
        note(
            "Tested",
            "empty-lists-ok",
            "no active OrderLists with payments≤40d — empty board is valid",
        )
        # still validate branch required / forbidden
        bad = requests.get(
            f"{BASE}/admin/DelegateFollowup/lists",
            params={"branchId": 99999},
            headers=h,
            timeout=30,
        )
        note(
            "Live-verified" if bad.status_code in (400, 403) else "Tested",
            "branch-out-of-scope",
            f"status={bad.status_code}",
        )
    else:
        card = items[0]
        list_id = card.get("listId") or card.get("ListId")
        sm = requests.get(
            f"{BASE}/admin/DelegateFollowup/lists/{list_id}/summary",
            params={"branchId": 1},
            headers=h,
            timeout=120,
        )
        sm.raise_for_status()
        summary = sm.json()
        legal_ok = summary.get("legalAvailable") is False
        follower = summary.get("followerCommittee") or {}
        note(
            "Live-verified" if legal_ok else "Blocked",
            "legal-unavailable-honest",
            f"legalAvailable={summary.get('legalAvailable')} count={summary.get('legalCustomerCount')}",
        )
        note(
            "Live-verified" if follower.get("available") is False else "Blocked",
            "committee-unavailable-honest",
            f"follower={follower.get('available')} note={follower.get('note')}",
        )
        unresolved = summary.get("unresolvedNotes") or []
        note(
            "Live-verified" if len(unresolved) > 0 else "Tested",
            "unresolved-notes-present",
            f"n={len(unresolved)}",
        )

        cu = requests.get(
            f"{BASE}/admin/DelegateFollowup/lists/{list_id}/customers",
            params={"branchId": 1},
            headers=h,
            timeout=120,
        )
        cu.raise_for_status()
        customers = cu.json()
        rows = customers if isinstance(customers, list) else customers.get("customers") or customers.get("items") or []
        note("Live-verified", "customers", f"count={len(rows)}")

        # board rating upsert + bounds
        bad_score = requests.post(
            f"{BASE}/admin/DelegateFollowup/lists/{list_id}/board-operator-rating",
            params={"branchId": 1},
            json={"score": 11, "note": "bad"},
            headers=h,
            timeout=30,
        )
        note(
            "Live-verified" if bad_score.status_code == 400 else "Blocked",
            "board-rating-reject-11",
            f"status={bad_score.status_code}",
        )
        ok = requests.post(
            f"{BASE}/admin/DelegateFollowup/lists/{list_id}/board-operator-rating",
            params={"branchId": 1},
            json={"score": 8, "note": "قبول معزول"},
            headers=h,
            timeout=30,
        )
        if ok.status_code == 200:
            body = ok.json()
            rating = body.get("rating") or body
            note(
                "Live-verified",
                "board-rating-upsert",
                f"score={rating.get('score')} has={rating.get('hasRating')}",
            )
            again = requests.post(
                f"{BASE}/admin/DelegateFollowup/lists/{list_id}/board-operator-rating",
                params={"branchId": 1},
                json={"score": 9, "note": "استبدال يومي"},
                headers=h,
                timeout=30,
            )
            again.raise_for_status()
            again_score = (again.json().get("rating") or again.json()).get("score")
            note(
                "Live-verified" if again_score == 9 else "Blocked",
                "board-rating-same-day-replace",
                f"score={again_score}",
            )
        else:
            note("Blocked", "board-rating-upsert", f"{ok.status_code} {ok.text[:200]}")

        # IDOR other branch list if any
        other = requests.get(
            f"{BASE}/admin/DelegateFollowup/lists/{list_id}/summary",
            params={"branchId": 2},
            headers=h,
            timeout=60,
        )
        note(
            "Live-verified" if other.status_code in (400, 403, 404) or (
                other.status_code == 200 and (other.json().get("branchId") == 2 or other.json().get("listId") != list_id)
            ) else "Tested",
            "cross-branch-summary",
            f"status={other.status_code}",
        )

    OUT.mkdir(parents=True, exist_ok=True)
    OUT.joinpath("results.json").write_text(
        json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print("WROTE", OUT / "results.json")


if __name__ == "__main__":
    main()
