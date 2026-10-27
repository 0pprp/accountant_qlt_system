# سكربتات المشروع

كل عمليات التنظيف والنشر محصورة هنا حتى تبقى سهلة ومكانها واحد.

| السكربت | الوظيفة |
|---|---|
| `clean.ps1` | يحذف مخرجات البناء والكاش (`bin`/`obj`/`dist`/`build`/`publish-out`) |
| `publish-api.ps1` | ينشر الـ API إلى `publish-out/api` |
| `publish-panel.ps1` | يبني لوحة الإدارة إلى `publish-out/panel` |
| `publish-all.ps1` | تنظيف ثم نشر API + Panel |
| `push-github.ps1` | يرفع الفرع الحالي إلى GitHub (`origin`) |
| `deploy-server.ps1` | سحب `development` على السيرفر الحي وإعادة بناء API + Panel |

## أمثلة

```powershell
# من جذر المستودع
.\scripts\clean.ps1
.\scripts\publish-all.ps1
.\scripts\push-github.ps1
```

مجلد `publish-out/` مخرجات مؤقتة وموجود في `.gitignore` — احذفه بأي وقت بـ `clean.ps1`.
