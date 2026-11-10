SELECT current_database() AS db, inet_server_addr() AS addr, inet_server_port() AS port;
SELECT c.relname
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public' AND c.relkind = 'r'
  AND c.relname IN ('Branches','Provinces','Customers','Orders','Users','Permissions','Roles')
ORDER BY 1;
SELECT COUNT(*) AS branch_count FROM "Branches";
SELECT COUNT(*) AS user_count FROM "Users";
SELECT "Name" FROM "Roles" ORDER BY 1 LIMIT 20;
