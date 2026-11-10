SELECT current_database() AS db,
       inet_server_addr() AS host_addr,
       inet_server_port() AS host_port;
SELECT c.relname
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public'
  AND c.relkind = 'r'
  AND c.relname IN (
    'SalesRequests',
    'SalesRequestTimelineEntries',
    'SalesRequestChangeOutbox',
    'Permissions',
    'RolePermissions',
    'Roles',
    'Users',
    'Branches'
  )
ORDER BY 1;
