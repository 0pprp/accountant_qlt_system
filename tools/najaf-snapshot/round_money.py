"""تدوير كل مبالغ القاعدة إلى دينار صحيح.

الدينار العراقي بلا كسور. الكسور الظاهرة ناتجة عن حساب عائم (double) عند
الاستيراد والتسوية، لا عن بيانات المصدر. يُدوَّر كل عمود مالي مرة واحدة،
ثم يُعاد حساب BalanceAfter ورصيد كل خزينة ليبقى ثابت الدفتر سليماً.
"""
import argparse
import psycopg

# الجدول -> أعمدته المالية
MONEY = {
    "Orders": ["BuyAmount", "SellAmount", "PrepaymentAmount", "DailyInstallmentAmount",
               "SellAmountDiscountApplied", "DailyInstallmentDiscountApplied"],
    "OrderItem": ["BuyAmount", "SellAmount", "PrepaymentAmount", "DailyInstallmentAmount"],
    "InstallmentPayments": ["Amount"],
    "Products": ["BuyAmount", "SellAmount", "DailyInstallmentAmount"],
    "Customers": ["SellAmountDiscount", "DailyInstallmentDiscount"],
    "Users": ["UndeliveredCashAmount"],
    "Expenses": ["TotalAmount"],
    "ExpenseItems": ["Amount"],
    "Purchases": ["TotalAmount"],
    "PurchaseItems": ["Amount"],
    "SafeMovements": ["Amount", "BalanceAfter"],
    "Safes": ["Balance"],
    "CustomerShopProfiles": ["StockEstimatedValue", "EstimatedDailyRevenue"],
}


def main(dsn):
    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local", "الهدف لازم qalaat_local"

    total = 0
    for table, cols in MONEY.items():
        c.execute("SELECT to_regclass(%s)", (f'public."{table}"',))
        if c.fetchone()[0] is None:
            continue
        present = []
        for col in cols:
            c.execute("""SELECT 1 FROM information_schema.columns
                         WHERE table_name=%s AND column_name=%s""", (table, col))
            if c.fetchone():
                present.append(col)
        if not present:
            continue
        where = " OR ".join(f'"{col}" <> round("{col}"::numeric)' for col in present)
        sets = ", ".join(f'"{col}" = round("{col}"::numeric)' for col in present)
        c.execute(f'UPDATE "{table}" SET {sets} WHERE {where}')
        if c.rowcount:
            print(f"  {table}: {c.rowcount:,} صف")
            total += c.rowcount

    # إعادة بناء الرصيد الجاري والرصيد المخزون بعد التدوير
    c.execute("""WITH o AS (SELECT "Id", SUM("Amount") OVER (PARTITION BY "SafeId"
                 ORDER BY "CreatedAt" NULLS FIRST, "Id"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) r FROM "SafeMovements")
                 UPDATE "SafeMovements" m SET "BalanceAfter"=o.r FROM o WHERE o."Id"=m."Id";""")
    c.execute("""UPDATE "Safes" s SET "Balance"=COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0);""")
    cn.commit()
    print(f"المجموع المُدوَّر: {total:,} صف")

    # تحقق: لا كسر باقٍ، والدفتر متوازن
    leftovers = 0
    for table, cols in MONEY.items():
        c.execute("SELECT to_regclass(%s)", (f'public."{table}"',))
        if c.fetchone()[0] is None:
            continue
        for col in cols:
            c.execute("""SELECT 1 FROM information_schema.columns
                         WHERE table_name=%s AND column_name=%s""", (table, col))
            if not c.fetchone():
                continue
            c.execute(f'SELECT count(*) FROM "{table}" WHERE "{col}" <> round("{col}"::numeric)')
            n = c.fetchone()[0]
            if n:
                print(f"  !! باقٍ كسر في {table}.{col}: {n}")
                leftovers += n
    print(f"كسور متبقية: {leftovers}")

    c.execute("""SELECT count(*) FROM "Safes" s WHERE abs(s."Balance"-COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0))>0.001""")
    print(f"خزائن غير متوازنة: {c.fetchone()[0]}")
    c.execute('SELECT SUM("Balance") FROM "Safes"')
    print(f"مجموع الخزائن: {float(c.fetchone()[0]):,.0f}")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    main(ap.parse_args().dsn)
