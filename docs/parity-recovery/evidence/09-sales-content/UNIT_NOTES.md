# Unit 09

```
dotnet test --filter "FullyQualifiedName~SalesExcelSearch|FullyQualifiedName~TutorialVideos"
→ Passed 11 (exit 0)

dotnet test --filter "FullyQualifiedName~ContentAppAudience|FullyQualifiedName~TutorialVideos|FullyQualifiedName~AnnouncementDomain"
→ Passed 21 (exit 0) — جمهور التطبيق مربوط بهوية الجلسة

dotnet test --filter "FullyQualifiedName~SalesOverview|...|PriceBoard|Announcements|AppNotes|CommitteeTasks|PermissionOverride|SalesRequestHub"
→ Passed 77 earlier in session (prior suite)
```

Excel search: scope isolation + name match rules.
Tutorial: rejects javascript / example.com / http youtube; concurrency with allowlisted HTTPS.
ContentAppAudience: Mandob↛follower announcements; Motaba↛delegate; Mandob↛sales-employee tutorials.
