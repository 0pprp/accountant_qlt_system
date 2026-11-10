import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:delegate_app/features/sales/data/submitted_requests_remote.dart';
import 'package:delegate_app/features/sales/presentation/submitted_sales_requests_page.dart';
class FakeRemote extends SubmittedRequestsRemote {
 FakeRemote():super(Dio(), 'Delegate');
 final pending=<Completer<SubmittedRequestsPageData>>[];
 final tabs=<String>[];
 @override Future<SubmittedRequestsPageData> load(String tab,int page,CancelToken token){tabs.add(tab);final c=Completer<SubmittedRequestsPageData>();pending.add(c);return c.future;}
}
SubmittedRequestsPageData row(String name)=>SubmittedRequestsPageData([SubmittedRequest.fromJson({'requestId':1,'customerName':name,'statusLabel':'مرفوض','address':'الكرادة','requestedProduct':'ثلاجة','rejectionReason':'رقم غير صحيح'})],false);
void main(){
 testWidgets('latest tab wins even if cancelled network response arrives late',(tester)async{
 final remote=FakeRemote();
 await tester.pumpWidget(MaterialApp(home:SubmittedSalesRequestsPage(remote:remote)));
 await tester.tap(find.text('مرفوض'));await tester.pump();
 remote.pending[1].complete(row('الطلب الحالي'));await tester.pumpAndSettle();
 remote.pending[0].complete(row('طلب قديم'));await tester.pumpAndSettle();
 expect(find.text('الطلب الحالي'),findsOneWidget);expect(find.text('طلب قديم'),findsNothing);
 expect(remote.tabs,['all','rejected']);
 expect(find.text('سبب الرفض: رقم غير صحيح'),findsOneWidget);
 expect(find.text('المنتج المطلوب: ثلاجة'),findsOneWidget);
 });
 testWidgets('failed request is not presented as an empty inbox and can retry',(tester)async{
 final remote=FakeRemote();await tester.pumpWidget(MaterialApp(home:SubmittedSalesRequestsPage(remote:remote)));
 remote.pending[0].completeError(Exception('offline'));await tester.pumpAndSettle();
 expect(find.text('لا توجد طلبات ضمن هذا التبويب'),findsNothing);
 await tester.tap(find.text('إعادة المحاولة'));await tester.pump();
 remote.pending[1].complete(const SubmittedRequestsPageData([],false));await tester.pumpAndSettle();
 expect(find.text('لا توجد طلبات ضمن هذا التبويب'),findsOneWidget);
 });
}
