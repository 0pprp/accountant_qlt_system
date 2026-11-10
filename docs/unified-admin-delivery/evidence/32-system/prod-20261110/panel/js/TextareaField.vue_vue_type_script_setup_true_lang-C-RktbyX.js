import{j as b}from"./primevue-components-DT8-55m5.js";import{B as m,f as h}from"./primevue-core-T01DRH7V.js";import{m as p,c,d as l,R as x,a4 as d,a5 as v,e as g,l as y,h as z,i as $,x as k,t as s,I as u,v as w,p as V}from"./vue-vendor-DWzzIv9s.js";import{s as S}from"./index-CodRd4Kx.js";var P=`
    .p-textarea {
        font-family: inherit;
        font-feature-settings: inherit;
        font-size: 1rem;
        color: dt('textarea.color');
        background: dt('textarea.background');
        padding-block: dt('textarea.padding.y');
        padding-inline: dt('textarea.padding.x');
        border: 1px solid dt('textarea.border.color');
        transition:
            background dt('textarea.transition.duration'),
            color dt('textarea.transition.duration'),
            border-color dt('textarea.transition.duration'),
            outline-color dt('textarea.transition.duration'),
            box-shadow dt('textarea.transition.duration');
        appearance: none;
        border-radius: dt('textarea.border.radius');
        outline-color: transparent;
        box-shadow: dt('textarea.shadow');
    }

    .p-textarea:enabled:hover {
        border-color: dt('textarea.hover.border.color');
    }

    .p-textarea:enabled:focus {
        border-color: dt('textarea.focus.border.color');
        box-shadow: dt('textarea.focus.ring.shadow');
        outline: dt('textarea.focus.ring.width') dt('textarea.focus.ring.style') dt('textarea.focus.ring.color');
        outline-offset: dt('textarea.focus.ring.offset');
    }

    .p-textarea.p-invalid {
        border-color: dt('textarea.invalid.border.color');
    }

    .p-textarea.p-variant-filled {
        background: dt('textarea.filled.background');
    }

    .p-textarea.p-variant-filled:enabled:hover {
        background: dt('textarea.filled.hover.background');
    }

    .p-textarea.p-variant-filled:enabled:focus {
        background: dt('textarea.filled.focus.background');
    }

    .p-textarea:disabled {
        opacity: 1;
        background: dt('textarea.disabled.background');
        color: dt('textarea.disabled.color');
    }

    .p-textarea::placeholder {
        color: dt('textarea.placeholder.color');
    }

    .p-textarea.p-invalid::placeholder {
        color: dt('textarea.invalid.placeholder.color');
    }

    .p-textarea-fluid {
        width: 100%;
    }

    .p-textarea-resizable {
        overflow: hidden;
        resize: none;
    }

    .p-textarea-sm {
        font-size: dt('textarea.sm.font.size');
        padding-block: dt('textarea.sm.padding.y');
        padding-inline: dt('textarea.sm.padding.x');
    }

    .p-textarea-lg {
        font-size: dt('textarea.lg.font.size');
        padding-block: dt('textarea.lg.padding.y');
        padding-inline: dt('textarea.lg.padding.x');
    }
`,q={root:function(e){var a=e.instance,r=e.props;return["p-textarea p-component",{"p-filled":a.$filled,"p-textarea-resizable ":r.autoResize,"p-textarea-sm p-inputfield-sm":r.size==="small","p-textarea-lg p-inputfield-lg":r.size==="large","p-invalid":a.$invalid,"p-variant-filled":a.$variant==="filled","p-textarea-fluid":a.$fluid}]}},B=m.extend({name:"textarea",style:P,classes:q}),I={name:"BaseTextarea",extends:b,props:{autoResize:Boolean},style:B,provide:function(){return{$pcTextarea:this,$parentInstance:this}}};function i(t){"@babel/helpers - typeof";return i=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(e){return typeof e}:function(e){return e&&typeof Symbol=="function"&&e.constructor===Symbol&&e!==Symbol.prototype?"symbol":typeof e},i(t)}function R(t,e,a){return(e=T(e))in t?Object.defineProperty(t,e,{value:a,enumerable:!0,configurable:!0,writable:!0}):t[e]=a,t}function T(t){var e=N(t,"string");return i(e)=="symbol"?e:e+""}function N(t,e){if(i(t)!="object"||!t)return t;var a=t[Symbol.toPrimitive];if(a!==void 0){var r=a.call(t,e);if(i(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(e==="string"?String:Number)(t)}var f={name:"Textarea",extends:I,inheritAttrs:!1,observer:null,mounted:function(){var e=this;this.autoResize&&(this.observer=new ResizeObserver(function(){requestAnimationFrame(function(){e.resize()})}),this.observer.observe(this.$el))},updated:function(){this.autoResize&&this.resize()},beforeUnmount:function(){this.observer&&this.observer.disconnect()},methods:{resize:function(){if(this.$el.offsetParent){var e=this.$el.style.height,a=parseInt(e)||0,r=this.$el.scrollHeight,o=!a||r>a,n=a&&r<a;n?(this.$el.style.height="auto",this.$el.style.height="".concat(this.$el.scrollHeight,"px")):o&&(this.$el.style.height="".concat(r,"px"))}},onInput:function(e){this.autoResize&&this.resize(),this.writeValue(e.target.value,e)}},computed:{attrs:function(){return p(this.ptmi("root",{context:{filled:this.$filled,disabled:this.disabled}}),this.formField)},dataP:function(){return h(R({invalid:this.$invalid,fluid:this.$fluid,filled:this.$variant==="filled"},this.size,this.size))}}},H=["value","name","disabled","aria-invalid","data-p"];function j(t,e,a,r,o,n){return l(),c("textarea",p({class:t.cx("root"),value:t.d_value,name:t.name,disabled:t.disabled,"aria-invalid":t.invalid||void 0,"data-p":n.dataP,onInput:e[0]||(e[0]=function(){return n.onInput&&n.onInput.apply(n,arguments)})},n.attrs),null,16,H)}f.render=j;const C=["aria-required","aria-label"],A=x({__name:"TextareaField",props:d({modelValue:{},error:{},placeholder:{},label:{},required:{type:Boolean},readonly:{type:Boolean},rows:{}},{modelValue:{},modelModifiers:{}}),emits:d(["update:modelValue"],["update:modelValue"]),setup(t){const e=v(t,"modelValue");return(a,r)=>(l(),c("div",null,[g("span",{class:k(["input__label",{required:a.required}]),"aria-required":a.required,"aria-label":a.$t(a.label)},s(a.$t(a.label)),11,C),y(u(f),{modelValue:e.value,"onUpdate:modelValue":r[0]||(r[0]=o=>e.value=o),placeholder:a.$t(a.placeholder),name:"name",rows:a.rows,readonly:a.readonly,required:a.required},null,8,["modelValue","placeholder","rows","readonly","required"]),a.error?(l(),z(u(S),{key:0,severity:"error",class:"text-error text-xs py-2",variant:"simple"},{default:w(()=>[V(s(a.error),1)]),_:1})):$("",!0)]))}});export{A as _};
//# sourceMappingURL=TextareaField.vue_vue_type_script_setup_true_lang-C-RktbyX.js.map
