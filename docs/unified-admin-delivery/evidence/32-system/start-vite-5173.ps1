$env:VITE_API_BASE_URL='http://127.0.0.1:4200/api/v1/'
$env:VITE_FILE_BASE_URL='http://127.0.0.1:4200/'
$env:VITE_API_PROXY_TARGET='http://127.0.0.1:4200'
Set-Location 'C:\GitHub\accountant_qlt_system\panel-admin-main\panel-admin-main'
npx vite --host 127.0.0.1 --port 5173
