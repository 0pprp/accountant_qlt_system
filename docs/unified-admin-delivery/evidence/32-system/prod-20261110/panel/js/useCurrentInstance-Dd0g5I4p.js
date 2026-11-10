import{g as s}from"./vue-vendor-DWzzIv9s.js";function u(e,n){const r=s();if(!r)throw new Error(`[QalaatAlDahman] ${e} must be called from inside a setup function`);return r}let a=0,t=new WeakMap;function i(){const e=u("useUid");if(t.has(e))return t.get(e);const n=a++;return t.set(e,n),n}i.reset=()=>{a=0,t=new WeakMap};export{i as u};
//# sourceMappingURL=useCurrentInstance-Dd0g5I4p.js.map
