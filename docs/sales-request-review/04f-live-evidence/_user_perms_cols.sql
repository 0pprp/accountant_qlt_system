SELECT column_name FROM information_schema.columns WHERE table_name='UserPermissions' ORDER BY 1;
SELECT "Id","Name" FROM "Permissions" WHERE "Name" IN (
  'SalesRequest.ExceptionDecide','SalesRequest.TabExceptions','SalesRequest.ReadUnassigned','SalesRequest.SoftDelete','SalesRequest.Restore','SalesRequest.TimelineRead'
);
