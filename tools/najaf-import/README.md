# مستورد فرع النجف (sandbox)

يقرأ `source_stage` (Postgres معزول من `stage_najaf.py`) ويكتب إلى sandbox فرع النجف فقط.

## قواعد

- Idempotent عبر `ImportKey = najaf|{table}|{id}`
- لا يمس مستخدم `ahmed` ولا صلاحيات Admin
- لا يكتب أسرارًا أو بيانات زبائن في Git
- `state.json` و`exceptions.jsonl` محليًا/على السيرفر خارج Git إن حويا مسارات حساسة

## تشغيل (بعد stage)

```bash
export NAJAF_STAGE_DSN='postgresql://postgres:...@localhost:5433/postgres'
export NAJAF_TARGET_DSN='postgresql://...@sandbox/qalaat'
export NAJAF_BRANCH_NAME='النجف'
python import_najaf.py --dry-run
python import_najaf.py
```

الحالة: `./.local/state.json` (مُستثناة من Git عبر `.gitignore` في المجلد).
