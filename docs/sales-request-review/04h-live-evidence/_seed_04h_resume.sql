INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","IsDeleted","ConcurrencyStamp","CreatedBy","CreatedAt"
) VALUES
('04H Flutter Complete Customer','07704445566',1,0,1,4,false,0,1,NOW()),
('04H Reassign Block Customer','07705556677',1,0,1,4,false,0,1,NOW())
RETURNING "Id","CustomerName","Status";
