import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';
import 'package:sales_employee_app/features/sales/presentation/widgets/sales_request_card.dart';
void main(){
 testWidgets('request card retains contact, product, notes and detail action on a narrow screen',(tester)async{
 tester.view.physicalSize=const Size(360,800);tester.view.devicePixelRatio=1;
 addTearDown(tester.view.resetPhysicalSize);addTearDown(tester.view.resetDevicePixelRatio);
 var opened=false;
 await tester.pumpWidget(MaterialApp(home:Scaffold(body:SingleChildScrollView(child:SalesRequestCard(
 item:const SalesRequestItem(requestId:8,customerName:'زبون طلب البيع',customerPhone:'07800000000',address:'بغداد الكرادة',requestedProduct:'ثلاجة',submissionNote:'اتصل قبل الوصول',lastStatusNote:'جاهز للكشف',status:1,statusLabel:'مسند',tabKey:'incoming'),onOpen:()=>opened=true)))));
 expect(find.text('07800000000'),findsOneWidget);expect(find.text('بغداد الكرادة'),findsOneWidget);expect(find.text('ثلاجة'),findsOneWidget);expect(find.text('اتصل قبل الوصول'),findsOneWidget);expect(find.text('جاهز للكشف'),findsOneWidget);
 await tester.ensureVisible(find.text('فتح الطلب والإجراءات'));await tester.tap(find.text('فتح الطلب والإجراءات'));
 expect(opened,isTrue);expect(tester.takeException(),isNull);
 });
}
