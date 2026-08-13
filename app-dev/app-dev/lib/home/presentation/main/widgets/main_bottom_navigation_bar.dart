part of '../main_page.dart';

class MainBottomNavigationBar extends StatelessWidget {
  final ValueNotifier<int> currentPage;
  final PageController pageController;
  final ValueNotifier<bool> isAnimating;
  MainBottomNavigationBar({
    super.key,
    required this.currentPage,
    required this.pageController,
    required this.isAnimating,
  });
  final pages = ['clients', 'sales', 'home', 'payments', 'user'];
  final pagesNames = ['عملاء', 'مبيعات', 'الرئيسية', 'تسديدات', 'المستخدم'];

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        boxShadow: [
          BoxShadow(
            color: Color(0xffD3D1D8).withValues(alpha: 0.15),
            blurRadius: 20,
            spreadRadius: 0,
            offset: Offset(0, -8),
          ),
        ],
      ),
      child: CurvedNavigationBar(
        backgroundColor: Colors.transparent,
        color: AppColor.surface2,
        index: currentPage.value,
        animationCurve: Curves.easeOutQuad,
        animationDuration: Duration(milliseconds: 400),
        buttonBackgroundColor: AppColor.primary,
        height: 72,
        items: <Widget>[
          for (int i = 0; i < 5; i++)
            Column(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                Padding(
                  padding: const EdgeInsets.all(4.0),
                  child: VectorGraphic(
                    loader: AssetBytesLoader(
                      'assets/svg/${i == currentPage.value ? '${pages[i]}_selected_icon.svg' : '${pages[i]}_icon.svg'}',
                    ),
                    width: i == currentPage.value ? 30 : 24,
                    height: i == currentPage.value ? 30 : 24,
                  ),
                ),
                if (i != currentPage.value)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 2),
                    child: Text(pagesNames[i], style: AppTextStyle.bodySmall.withColor(AppColor.text2)),
                  ),
              ],
            ),
        ],
        onTap: (index) async {
          isAnimating.value = true;
          currentPage.value = index;
          await pageController.animateToPage(index, duration: Duration(milliseconds: 500), curve: Curves.easeOut);
          isAnimating.value = false;
        },
      ),
    );
  }
}
