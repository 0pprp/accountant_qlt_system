# -*- coding: utf-8 -*-
import psycopg

with psycopg.connect(
    "host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres"
) as c:
    with c.cursor() as cur:
        cur.execute('SELECT COUNT(*) FROM "OrderLists"')
        print("OrderLists", cur.fetchone()[0])
        cur.execute(
            'SELECT "Id","Name","BranchId","MandobId","MotabaId" FROM "OrderLists" LIMIT 10'
        )
        print("rows", cur.fetchall())
        cur.execute(
            """
            SELECT o."OrderListId", COUNT(*),
                   MAX(ip."Date") AS last_pay,
                   SUM(o."SellAmount") AS sell
            FROM "Orders" o
            LEFT JOIN "InstallmentPayments" ip ON ip."OrderId"=o."Id"
            WHERE o."OrderListId" IS NOT NULL
            GROUP BY o."OrderListId"
            LIMIT 20
            """
        )
        print("orders_by_list", cur.fetchall())
