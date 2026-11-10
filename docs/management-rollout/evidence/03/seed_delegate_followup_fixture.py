# -*- coding: utf-8 -*-
"""One-off fixture: minimal data so DelegateFollowup lists returns >= 1 active list.

Only INSERTs. Re-running reuses the uniquely named fixture list/customer and does
not duplicate them. Existing data is never modified or deleted.
"""
import sys
from datetime import date, datetime, timedelta, timezone

import psycopg

CONN = "host=localhost dbname=qalaat_local user=postgres password=postgres"
LIST_NAME = "قائمة اختبار متابعة مندوبين"
CUSTOMER_NATIONAL_CODE = "FIXTURE-DF-0001"
USER_ID = 1
BRANCH_ID = 1

APPROVAL_APPROVED = 1  # OrderApprovalStatus.Approved
STEP_COMPLETED = 2  # OrderStep.Completed
EXEC_IN_PROGRESS = 1  # OrderExecutionStatus.InProgress
WORKFLOW_SOLD = 5  # SalesWorkflowStatus.Sold


def counts(cur):
    out = {}
    for t in ("OrderLists", "Customers", "Orders", "InstallmentPayments"):
        cur.execute(f'SELECT COUNT(*) FROM "{t}"')
        out[t] = cur.fetchone()[0]
    return out


def main():
    now = datetime.now(timezone.utc)
    today = date.today()
    sale_date = today - timedelta(days=30)

    with psycopg.connect(CONN) as conn:
        with conn.cursor() as cur:
            print("before:", counts(cur))

            cur.execute('SELECT 1 FROM "Users" WHERE "Id"=%s', (USER_ID,))
            if cur.fetchone() is None:
                sys.exit(f"User Id={USER_ID} not found")
            cur.execute('SELECT 1 FROM "Branches" WHERE "Id"=%s', (BRANCH_ID,))
            if cur.fetchone() is None:
                sys.exit(f"Branch Id={BRANCH_ID} not found")

            cur.execute(
                'SELECT "Id" FROM "OrderLists" WHERE "Name"=%s AND "BranchId"=%s',
                (LIST_NAME, BRANCH_ID),
            )
            row = cur.fetchone()
            if row:
                list_id = row[0]
                print("reusing OrderList", list_id)
            else:
                cur.execute(
                    """INSERT INTO "OrderLists"
                       ("Name","MandobId","MotabaId","BranchId",
                        "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
                       VALUES (%s,%s,%s,%s,%s,%s,%s,%s) RETURNING "Id" """,
                    (LIST_NAME, USER_ID, USER_ID, BRANCH_ID, now, USER_ID, now, USER_ID),
                )
                list_id = cur.fetchone()[0]

            cur.execute(
                'SELECT "Id" FROM "Customers" WHERE "NationalCode"=%s AND "BranchId"=%s',
                (CUSTOMER_NATIONAL_CODE, BRANCH_ID),
            )
            row = cur.fetchone()
            if row:
                customer_id = row[0]
                print("reusing Customer", customer_id)
            else:
                cur.execute(
                    """INSERT INTO "Customers"
                       ("FullName","MotherName","NationalCode","BirthDate","PhoneNumber",
                        "WhatsAppPhoneNumber","Business_Name","Business_Address",
                        "Business_NearestKnownLocation","BranchId",
                        "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
                       VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s) RETURNING "Id" """,
                    (
                        "عميل اختبار متابعة",
                        "والدة اختبار",
                        CUSTOMER_NATIONAL_CODE,
                        date(1990, 1, 1),
                        "07700000001",
                        "07700000001",
                        "محل اختبار",
                        "عنوان اختبار",
                        "أقرب نقطة دالة اختبار",
                        BRANCH_ID,
                        now,
                        USER_ID,
                        now,
                        USER_ID,
                    ),
                )
                customer_id = cur.fetchone()[0]

            cur.execute(
                'SELECT "Id" FROM "Orders" WHERE "OrderListId"=%s AND "CustomerId"=%s',
                (list_id, customer_id),
            )
            row = cur.fetchone()
            if row:
                order_id = row[0]
                print("reusing Order", order_id)
            else:
                cur.execute(
                    """INSERT INTO "Orders"
                       ("BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
                        "CreationAddress","SaleDate","SaleTime","Step","ExecutionStatus",
                        "ApprovalStatus","WorkflowStatus","SellerId","CustomerId","BranchId",
                        "OrderListId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
                       VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
                       RETURNING "Id" """,
                    (
                        1000000, 1500000, 0, 10000,
                        "عنوان اختبار", sale_date, "10:00:00",
                        STEP_COMPLETED, EXEC_IN_PROGRESS, APPROVAL_APPROVED, WORKFLOW_SOLD,
                        USER_ID, customer_id, BRANCH_ID, list_id,
                        now, USER_ID, now, USER_ID,
                    ),
                )
                order_id = cur.fetchone()[0]

            cur.execute(
                'SELECT "Id" FROM "InstallmentPayments" WHERE "OrderId"=%s AND "Date"=%s',
                (order_id, today),
            )
            row = cur.fetchone()
            if row:
                payment_id = row[0]
                print("reusing InstallmentPayment", payment_id)
            else:
                cur.execute(
                    """INSERT INTO "InstallmentPayments"
                       ("Amount","Date","Description","OrderId",
                        "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
                       VALUES (%s,%s,%s,%s,%s,%s,%s,%s) RETURNING "Id" """,
                    (50000, today, "fixture", order_id, now, USER_ID, now, USER_ID),
                )
                payment_id = cur.fetchone()[0]

            print("after:", counts(cur))
            print(
                f"IDs: OrderListId={list_id} CustomerId={customer_id} "
                f"OrderId={order_id} InstallmentPaymentId={payment_id}"
            )
        conn.commit()


if __name__ == "__main__":
    main()
