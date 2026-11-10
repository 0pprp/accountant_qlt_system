function m(t){return t.split("T")[0].replace(/-/g,"/")}function c(t){const[e,r]=t.split("T");if(!e)return"";const[n,o,s]=e.split("-"),a=`${Number(s)}/${Number(o)}/${n}`;if(!r)return a;const[i,u]=r.split(":");return`${a} - ${i}:${u}`}export{c as a,m as f};
//# sourceMappingURL=time-B5LnI0aS.js.map
