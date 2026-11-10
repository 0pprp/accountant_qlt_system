import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class DecliningTab extends StatefulWidget {
  const DecliningTab({super.key});

  @override
  State<DecliningTab> createState() => _DecliningTabState();
}

class _DecliningTabState extends State<DecliningTab> {
  List<Map<String, dynamic>> _rows = const [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final rows = await Injection.committee.declining();
      if (!mounted) return;
      setState(() {
        _rows = rows;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      body: AnimatedWavesBackground(
        child: SafeArea(
          bottom: false,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Padding(
                padding: EdgeInsets.fromLTRB(16, 12, 16, 8),
                child: Text('الزبائن المتدنية', style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800)),
              ),
              const Padding(
                padding: EdgeInsets.fromLTRB(16, 0, 16, 8),
                child: Text(
                  'هبوط درجتين خلال 7 أيام (ممتاز→ضعيف، جيد→متلكئ، ضعيف→سيء)',
                  style: TextStyle(fontSize: 13, height: 1.4),
                ),
              ),
              Expanded(
                child: RefreshIndicator(
                  onRefresh: _load,
                  child: _loading
                      ? ListView(children: const [SizedBox(height: 120), Center(child: CircularProgressIndicator())])
                      : _error != null
                          ? ListView(children: [Padding(padding: const EdgeInsets.all(24), child: Text(_error!))])
                          : _rows.isEmpty
                              ? ListView(children: const [SizedBox(height: 120), Center(child: Text('لا زبائن متدنية حالياً'))])
                              : ListView.separated(
                                  padding: EdgeInsets.fromLTRB(16, 8, 16, 100 + bottom),
                                  itemCount: _rows.length,
                                  separatorBuilder: (_, __) => const SizedBox(height: 8),
                                  itemBuilder: (_, i) {
                                    final r = _rows[i];
                                    final from = r['fromLabel'] ?? r['FromLabel'] ?? '';
                                    final to = r['toLabel'] ?? r['ToLabel'] ?? r['riskLabel'] ?? r['RiskLabel'] ?? '';
                                    final days = r['daysBetween'] ?? r['DaysBetween'] ?? r['missedDays'] ?? r['MissedDays'] ?? 0;
                                    final list = r['listName'] ?? r['ListName'] ?? '';
                                    return Material(
                                      color: Colors.white.withValues(alpha: 0.92),
                                      borderRadius: BorderRadius.circular(14),
                                      child: ListTile(
                                        title: Text('${r['fullName'] ?? r['FullName'] ?? ''}',
                                            style: const TextStyle(fontWeight: FontWeight.w700)),
                                        subtitle: Text('$list — $from → $to · $days يوم'),
                                        trailing: Text(
                                          '$to',
                                          style: const TextStyle(color: AppColors.warning, fontWeight: FontWeight.w700),
                                        ),
                                      ),
                                    );
                                  },
                                ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
