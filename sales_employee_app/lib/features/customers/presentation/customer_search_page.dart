import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/customers/domain/entities/customer_lite.dart';
import 'package:sales_employee_app/injection.dart';

class CustomerSearchPage extends StatefulWidget {
  const CustomerSearchPage({super.key});

  @override
  State<CustomerSearchPage> createState() => _CustomerSearchPageState();
}

class _CustomerSearchPageState extends State<CustomerSearchPage> {
  final _ctrl = TextEditingController();
  List<CustomerLite> _items = const [];
  bool _loading = false;

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final list = await Injection.customersRemote.search(query: _ctrl.text);
      if (!mounted) return;
      setState(() {
        _items = list;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'بحث الزبائن', message: DioClient.mapError(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('بحث الزبائن')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _ctrl,
                    decoration: const InputDecoration(
                      hintText: 'اسم أو هاتف',
                      prefixIcon: Icon(Icons.search),
                    ),
                    onSubmitted: (_) => _search(),
                  ),
                ),
                const SizedBox(width: 8),
                ElevatedButton(onPressed: _search, child: const Text('بحث')),
              ],
            ),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : ListView.builder(
                    itemCount: _items.length,
                    itemBuilder: (_, i) {
                      final c = _items[i];
                      return ListTile(
                        title: Text(c.fullName),
                        subtitle: Text([
                          if (c.phone != null && c.phone!.isNotEmpty) c.phone!,
                          if (c.address != null && c.address!.isNotEmpty) c.address!,
                        ].join(' • ')),
                        leading: const CircleAvatar(
                          backgroundColor: AppColors.primary,
                          child: Icon(Icons.person, color: Colors.white),
                        ),
                        onTap: () async {
                          final full = await Injection.customersRemote.getById(c.id);
                          if (!context.mounted) return;
                          await showAppMessage(
                            context,
                            title: full?.fullName ?? c.fullName,
                            message: 'الهاتف: ${full?.phone ?? c.phone ?? '-'}\nالعنوان: ${full?.address ?? c.address ?? '-'}',
                          );
                        },
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }
}
