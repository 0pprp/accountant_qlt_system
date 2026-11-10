SELECT "Id", "Name" FROM "Roles" ORDER BY "Id";
SELECT "Id", "Username", "CreationStep" FROM "Users" ORDER BY "Id" LIMIT 30;
SELECT "Id", "Name" FROM "Permissions" WHERE "Name" LIKE 'SalesRequest.App%' OR "Name" = 'Complaint.Submit' ORDER BY 1;
SELECT * FROM "SalesEmployeeAssignments";
