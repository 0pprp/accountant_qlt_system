SELECT "Id","RequestId","ChangeType",
  CASE WHEN "PublishedAtUtc" IS NULL THEN 'null' ELSE 'pub' END AS pub,
  "RetryCount"
FROM "SalesRequestChangeOutbox" ORDER BY "Id";
