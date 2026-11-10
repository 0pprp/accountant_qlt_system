async () => {
  const t = await (await fetch('/packages/sales_employee_app/core/config/app_env.dart.lib.js')).text();
  const urls = t.match(/https?:[^"']+/g);
  return {
    urls,
    has4200: t.includes('4200'),
    hasProd: t.includes('qalataldhaman'),
    around: t.includes('apiBaseUrl')
      ? t.slice(t.indexOf('apiBaseUrl'), t.indexOf('apiBaseUrl') + 500)
      : null,
  };
}
