import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:committee_officer_application/features/home/presentation/widgets/delegate_note_button.dart';
void main(){
 testWidgets('cancel does not write a delegate note',(tester)async{
 var calls=0;
 await tester.pumpWidget(MaterialApp(home:Scaffold(body:DelegateNoteButton(listName:'القائمة الأولى',save:(_)async{calls++;}))));
 await tester.tap(find.text('ملاحظة المندوب'));await tester.pumpAndSettle();
 expect(find.text('ملاحظة على مندوب القائمة الأولى'),findsOneWidget);
 await tester.tap(find.text('إلغاء'));await tester.pumpAndSettle();expect(calls,0);
 });
 testWidgets('save sends trimmed text for the selected list only',(tester)async{
 String? saved;
 await tester.pumpWidget(MaterialApp(home:Scaffold(body:DelegateNoteButton(listName:'قائمة الاختبار',save:(text)async{saved=text;}))));
 await tester.tap(find.text('ملاحظة المندوب'));await tester.pumpAndSettle();
 await tester.enterText(find.byType(TextField),'  ملاحظة ميدانية  ');
 await tester.tap(find.text('إرسال'));await tester.pumpAndSettle();
 expect(saved,'ملاحظة ميدانية');expect(find.text('حُفظت ملاحظة مندوب القائمة.'),findsOneWidget);
 });}

