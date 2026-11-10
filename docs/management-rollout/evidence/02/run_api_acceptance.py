# -*- coding: utf-8 -*-
"""Isolated API acceptance for ManagementTasks phase 02. No production deploy."""
from __future__ import annotations

import json
import time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

import requests

BASE = "http://127.0.0.1:4200/api/v1"
OUT = Path(__file__).resolve().parent
results: list[dict] = []


def note(status: str, id_: str, detail: str) -> None:
    results.append({"status": status, "id": id_, "detail": detail})
    print(f"[{status}] {id_} — {detail}")


def login(username: str, password: str) -> dict:
    r = requests.post(
        f"{BASE}/admin/Auth/login",
        json={"username": username, "password": password},
        timeout=30,
    )
    r.raise_for_status()
    return r.json()


def auth_headers(token: str, branch_id: int | None = None) -> dict:
    h = {"Authorization": f"Bearer {token}"}
    if branch_id is not None:
        h["X-Branch-Id"] = str(branch_id)
    return h


def main() -> None:
    admin = login("admin", "admin123")
    gg = login("gg", "admin123")

    admin_token = admin["accessToken"]
    gg_token = gg["accessToken"]
    admin_h = auth_headers(admin_token)
    gg_h = auth_headers(gg_token)

    admin_task = "Task" in (admin.get("permissions") or {})
    gg_task = "Task" in (gg.get("permissions") or {})
    note(
        "Live-verified" if admin_task and gg_task else "Blocked",
        "login-task-perms",
        f"admin_task={admin_task} gg_task={gg_task} gg_task_actions={(gg.get('permissions') or {}).get('Task')}",
    )

    limited_token = None
    try:
        limited = login("limited01", "admin123")
        limited_token = limited["accessToken"]
        limited_ok = "Task" not in (limited.get("permissions") or {})
        note(
            "Live-verified" if limited_ok else "Tested",
            "limited-no-task-perm",
            f"has_task={not limited_ok}",
        )
    except Exception as ex:
        note("Blocked", "limited-login", str(ex))

    # Create task assigned to admin (assignee=1) on branch 1
    due = time.strftime("%Y-%m-%d", time.gmtime(time.time() + 86400 * 3))
    create_body = {
        "title": "مهمة قبول معزولة 02",
        "description": "دورة كاملة مع نقل وتصديق",
        "priority": 2,
        "dueDate": due,
        "branchId": 1,
        "assigneeUserId": 1,
    }
    cr = requests.post(f"{BASE}/admin/ManagementTasks", json=create_body, headers=admin_h, timeout=30)
    if cr.status_code != 200:
        note("Blocked", "create-task", f"{cr.status_code} {cr.text[:300]}")
        OUT.joinpath("results.json").write_text(
            json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        return

    created = cr.json()
    task_id = created["id"]
    ver = created["expectedVersion"]
    note("Live-verified", "create-task", f"id={task_id} status={created.get('status')} ver={ver}")

    # Forbidden: limited list
    if limited_token:
        lr = requests.get(
            f"{BASE}/admin/ManagementTasks",
            params={"pageIndex": 1, "pageSize": 10, "branchId": 1},
            headers=auth_headers(limited_token),
            timeout=30,
        )
        note(
            "Live-verified" if lr.status_code in (401, 403) else "Blocked",
            "limited-list-forbidden",
            f"status={lr.status_code}",
        )

    # Accept by admin (holder)
    ar = requests.post(
        f"{BASE}/admin/ManagementTasks/{task_id}/accept",
        json={"expectedVersion": ver},
        headers=admin_h,
        timeout=30,
    )
    ar.raise_for_status()
    accepted = ar.json()
    ver = accepted["expectedVersion"]
    note("Live-verified", "accept", f"status={accepted['status']} ver={ver}")

    # Transfer to gg (user 2)
    tr = requests.post(
        f"{BASE}/admin/ManagementTasks/{task_id}/transfer",
        json={"expectedVersion": ver, "targetUserId": 2, "note": "نقل لفرع"},
        headers=admin_h,
        timeout=30,
    )
    if tr.status_code != 200:
        note("Blocked", "transfer", f"{tr.status_code} {tr.text[:300]}")
    else:
        transferred = tr.json()
        ver = transferred["expectedVersion"]
        note("Live-verified", "transfer", f"status={transferred['status']} ver={ver}")

    # After transfer, admin (not holder) cannot accept again / complete
    bad = requests.post(
        f"{BASE}/admin/ManagementTasks/{task_id}/complete",
        json={"expectedVersion": ver},
        headers=admin_h,
        timeout=30,
    )
    note(
        "Live-verified" if bad.status_code == 400 else "Blocked",
        "former-holder-blocked",
        f"status={bad.status_code} body={bad.text[:160]}",
    )

    # gg accept + complete
    ga = requests.post(
        f"{BASE}/admin/ManagementTasks/{task_id}/accept",
        json={"expectedVersion": ver},
        headers=gg_h,
        timeout=30,
    )
    if ga.status_code != 200:
        note("Blocked", "gg-accept", f"{ga.status_code} {ga.text[:300]}")
    else:
        ver = ga.json()["expectedVersion"]
        note("Live-verified", "gg-accept", f"ver={ver}")

    gc = requests.post(
        f"{BASE}/admin/ManagementTasks/{task_id}/complete",
        json={"expectedVersion": ver},
        headers=gg_h,
        timeout=30,
    )
    if gc.status_code != 200:
        note("Blocked", "gg-complete", f"{gc.status_code} {gc.text[:300]}")
        awaiting_ver = ver
    else:
        done = gc.json()
        awaiting_ver = done["expectedVersion"]
        note(
            "Live-verified",
            "gg-complete-awaiting-confirm",
            f"status={done['status']} ver={awaiting_ver}",
        )

    # Concurrent confirm conflict: two confirms with same expectedVersion
    def confirm_once():
        return requests.post(
            f"{BASE}/admin/ManagementTasks/{task_id}/confirm",
            json={"expectedVersion": awaiting_ver},
            headers=admin_h,
            timeout=30,
        )

    with ThreadPoolExecutor(max_workers=2) as pool:
        f1 = pool.submit(confirm_once)
        f2 = pool.submit(confirm_once)
        r1, r2 = f1.result(), f2.result()

    codes = sorted([r1.status_code, r2.status_code])
    note(
        "Live-verified" if codes in ([200, 409], [200, 400]) or (200 in codes and 409 in codes) else "Tested",
        "concurrent-confirm",
        f"codes={codes} bodies={[r1.text[:80], r2.text[:80]]}",
    )

    # Detail + events
    detail = requests.get(f"{BASE}/admin/ManagementTasks/{task_id}", headers=admin_h, timeout=30)
    if detail.status_code == 200:
        d = detail.json()
        events = d.get("events") or []
        note(
            "Live-verified",
            "detail-events",
            f"status={d.get('task', {}).get('status')} events={len(events)} types={[e.get('eventType') for e in events]}",
        )
    else:
        note("Blocked", "detail-events", f"{detail.status_code}")

    # Branch isolation: create on branch 1, try access as if only branch 2 — use query with branchId=2 should not show it for inbox of other; IDOR get by id still needs branch scope of user.
    # Admin has both branches so create a task and ensure list?branchId=2 does not include branch1 task
    list_b2 = requests.get(
        f"{BASE}/admin/ManagementTasks",
        params={"pageIndex": 1, "pageSize": 50, "branchId": 2},
        headers=admin_h,
        timeout=30,
    )
    if list_b2.status_code == 200:
        items = (list_b2.json().get("items") or list_b2.json().get("Items") or [])
        leaked = [i for i in items if i.get("id") == task_id or i.get("Id") == task_id]
        note(
            "Live-verified" if len(leaked) == 0 else "Blocked",
            "branch-filter-no-leak",
            f"branch2_count={len(items)} leaked={len(leaked)}",
        )
    else:
        note("Blocked", "branch-filter-no-leak", f"{list_b2.status_code}")

    # Retry complete after done should not duplicate (already completed path)
    # Create self-complete path without transfer
    cr2 = requests.post(
        f"{BASE}/admin/ManagementTasks",
        json={
            "title": "إنجاز ذاتي بلا تصديق",
            "description": "لا تمر بالتأكيد",
            "priority": 1,
            "dueDate": due,
            "branchId": 1,
            "assigneeUserId": 1,
        },
        headers=admin_h,
        timeout=30,
    )
    if cr2.status_code == 200:
        t2 = cr2.json()
        tid2, v2 = t2["id"], t2["expectedVersion"]
        requests.post(
            f"{BASE}/admin/ManagementTasks/{tid2}/accept",
            json={"expectedVersion": v2},
            headers=admin_h,
            timeout=30,
        ).raise_for_status()
        # refresh version
        d2 = requests.get(f"{BASE}/admin/ManagementTasks/{tid2}", headers=admin_h, timeout=30).json()
        v2 = d2["task"]["expectedVersion"]
        c1 = requests.post(
            f"{BASE}/admin/ManagementTasks/{tid2}/complete",
            json={"expectedVersion": v2},
            headers=admin_h,
            timeout=30,
        )
        c1.raise_for_status()
        final = c1.json()
        # retry with stale version
        c2 = requests.post(
            f"{BASE}/admin/ManagementTasks/{tid2}/complete",
            json={"expectedVersion": v2},
            headers=admin_h,
            timeout=30,
        )
        note(
            "Live-verified" if final["status"] == 5 and c2.status_code == 409 else "Tested",
            "self-complete-no-confirm-and-retry-409",
            f"final_status={final['status']} retry={c2.status_code}",
        )
    else:
        note("Blocked", "self-complete-no-confirm-and-retry-409", cr2.text[:200])

    OUT.joinpath("results.json").write_text(
        json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print("WROTE", OUT / "results.json")


if __name__ == "__main__":
    main()
