"""Local-only, deterministic two-process payment lock verification.

The named ISO fixture orders are mutated; never run against production.
Tokens stay in memory. PAYMENT_VERIFY_DSN is required, not written to evidence.
"""
import concurrent.futures
import datetime
import json
import ipaddress
import os
import threading
import time
import urllib.error
import urllib.request
import uuid
from pathlib import Path
import psycopg2

DSN = os.environ['PAYMENT_VERIFY_DSN']
BASES = ['http://127.0.0.1:4202', 'http://127.0.0.1:4203']
OUT = Path(os.environ['PAYMENT_VERIFY_OUTPUT'])
def db():
    connection = psycopg2.connect(DSN)
    with connection.cursor() as cur:
        cur.execute('select current_database(), inet_server_addr()::text')
        name, host = cur.fetchone()
        assert name == 'qalaat_22_23_iso' and ipaddress.ip_interface(host).ip.is_loopback, (name, host)
    connection.commit()
    return connection

def http(base, path, body, token=None):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    req = urllib.request.Request(base + path, json.dumps(body).encode(), headers)
    try:
        with urllib.request.urlopen(req, timeout=30) as res:
            return res.status, json.load(res)
    except urllib.error.HTTPError as err:
        return err.code, json.loads(err.read())

def sql(query, args=(), fetch=False):
    with db() as conn:
        with conn.cursor() as cur:
            cur.execute(query, args)
            return cur.fetchall() if fetch else None

result = {'database': 'qalaat_22_23_iso', 'api_ports': [4202,4203], 'checks': []}
try:
    processes = sql("select distinct application_name from pg_stat_activity where application_name in ('PaymentLockVerify4202','PaymentLockVerify4203')", fetch=True)
    assert len(processes) == 2, processes
    status, login = http(BASES[0], '/api/v1/app/Auth/login', {
        'username': os.environ['PAYMENT_VERIFY_USER'], 'password': os.environ['PAYMENT_VERIFY_PASSWORD']})
    assert status == 200, status
    token = login['accessToken']
    date = datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=3))).date().isoformat()
    orders = sql('SELECT "Id", "SellerId", "OrderListId" FROM "Orders" WHERE "Id" IN (1,3) ORDER BY "Id"', fetch=True)
    assert len(orders)==2 and orders[0][1] == orders[1][1] and orders[0][1] is not None, orders
    seller = orders[0][1]
    sql('UPDATE "OrderLists" SET "FollowerCollectionEnabled"=false WHERE "Id" IN (%s,%s)', (orders[0][2],orders[1][2]))

    def ready_orders():
        sql('UPDATE "Orders" SET "SellAmount"=500000, "ExecutionStatus"=1 WHERE "Id" IN (1,3)')

    def body(order_id, key, sync=False):
        row = {'amount':1000, 'date':date, 'clientPaymentId':key}
        if sync:
            row['orderId'] = order_id
            return '/api/v1/app/InstallmentPayments/sync', {'installmentPayments':[row]}
        return f'/api/v1/app/Orders/{order_id}/installment-payments', row

    def pair(namespace, entity_id, calls):
        # Hold the exact application lock until both independent API database
        # sessions are demonstrably waiting for it. Not merely two fast tasks.
        blocker = db()
        with blocker.cursor() as cur:
            cur.execute('SELECT pg_advisory_xact_lock(%s,%s)',(namespace,entity_id))
        barrier = threading.Barrier(3)
        def run(index):
            barrier.wait(timeout=10)
            path, payload = calls[index]
            return http(BASES[index],path,payload,token)
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
            futures = [pool.submit(run,i) for i in range(2)]
            barrier.wait(timeout=10)
            waiters = []
            try:
                until = time.monotonic()+15
                while time.monotonic()<until:
                    waiters=sql("""select distinct a.application_name from pg_locks l
                        join pg_stat_activity a on a.pid=l.pid
                        where l.locktype='advisory' and not l.granted
                        and l.classid=%s and l.objid=%s and l.objsubid=2
                        and a.application_name in ('PaymentLockVerify4202','PaymentLockVerify4203')""",
                        (namespace,entity_id),True)
                    if len(waiters)==2: break
                    time.sleep(.05)
            finally:
                blocker.commit()
                blocker.close()
            responses=[f.result(timeout=30) for f in futures]
        assert len(waiters)==2, {'waiting_processes':waiters,'statuses':[x[0] for x in responses]}
        return responses, sorted(x[0] for x in waiters)

    ready_orders()
    key=str(uuid.uuid4())
    responses,waiters=pair(791902,1,[body(1,key),body(1,key,True)])
    assert all(s==200 for s,_ in responses), responses
    a,b=responses[0][1],responses[1][1]['items'][0]
    count=sql('SELECT COUNT(*) FROM "InstallmentPayments" WHERE "ClientPaymentId"=%s',(key,),True)[0][0]
    assert count==1 and a['paymentId']==b['paymentId'], (a,b,count)
    assert sorted([a['alreadyApplied'],b['alreadyApplied']])==[False,True]
    result['checks'].append({'name':'concurrent_create_sync','waiting_processes':waiters,'key_count':count,'same_payment_id':True,'passed':True})

    paid=sql('SELECT COALESCE(SUM("Amount"),0) FROM "InstallmentPayments" WHERE "OrderId"=1',fetch=True)[0][0]
    sql('UPDATE "Orders" SET "SellAmount"=%s,"ExecutionStatus"=1 WHERE "Id"=1',(paid+1500,))
    responses,waiters=pair(791902,1,[body(1,str(uuid.uuid4())),body(1,str(uuid.uuid4()))])
    assert sorted(s for s,_ in responses)==[200,400],responses
    after=sql('SELECT COALESCE(SUM("Amount"),0) FROM "InstallmentPayments" WHERE "OrderId"=1',fetch=True)[0][0]
    assert after-paid==1000
    result['checks'].append({'name':'over_remaining','waiting_processes':waiters,'statuses':sorted(s for s,_ in responses),'paid_delta':after-paid,'passed':True})

    ready_orders()
    cash0=sql('SELECT "UndeliveredCashAmount" FROM "Users" WHERE "Id"=%s',(seller,),True)[0][0]
    responses,waiters=pair(791903,seller,[body(1,str(uuid.uuid4())),body(3,str(uuid.uuid4()))])
    assert all(s==200 for s,_ in responses),responses
    cash1=sql('SELECT "UndeliveredCashAmount" FROM "Users" WHERE "Id"=%s',(seller,),True)[0][0]
    assert cash1-cash0==2000,(cash0,cash1)
    result['checks'].append({'name':'different_orders_same_cash_holder','waiting_processes':waiters,'cash_delta':cash1-cash0,'passed':True})
    result['passed']=True
finally:
    result['recorded_at_utc']=datetime.datetime.now(datetime.timezone.utc).isoformat()
    OUT.parent.mkdir(parents=True,exist_ok=True)
    OUT.write_text(json.dumps(result,ensure_ascii=False,indent=2,default=str),encoding='utf-8')
    print(json.dumps(result,ensure_ascii=False,default=str))
