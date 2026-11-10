<template>
  <div class="permission-control">
    <button type="button" role="switch" :aria-checked="enabled" :aria-label="label" :disabled="disabled" class="permission-switch" :class="{enabled}" @click="$emit('toggle', !enabled)">
      <span class="switch-track" aria-hidden="true"><span /></span>
      <span>{{ enabled ? 'تشغيل' : 'إيقاف' }}</span>
    </button>
    <button type="button" class="inherit-button" :disabled="disabled || inherited" :aria-label="`حسب الدور: ${label}`" @click="$emit('inherit')">{{ inherited ? 'موروثة من الدور' : 'حسب الدور' }}</button>
  </div>
</template>
<script setup lang="ts">
defineProps<{label:string;enabled:boolean;inherited:boolean;disabled:boolean}>()
defineEmits<{toggle:[boolean];inherit:[]}>()
</script>
<style scoped>
.permission-control{display:flex;align-items:center;gap:12px;flex-wrap:wrap}
.permission-switch{display:inline-flex;align-items:center;gap:10px;min-height:44px;padding:6px 10px;border:1px solid #d7e2e0;border-radius:10px;font:inherit;color:#52625f;background:#f5f7f6;cursor:pointer}
.permission-switch.enabled{color:#006a64;background:#edf7f4;border-color:#a1ccc5}
.switch-track{display:block;width:42px;height:24px;border-radius:20px;background:#7e8986;padding:3px;direction:ltr}
.switch-track span{display:block;width:18px;height:18px;background:white;border-radius:50%;transition:transform .15s}
.enabled .switch-track{background:#006a64}.enabled .switch-track span{transform:translateX(18px)}
.inherit-button{min-height:44px;font:inherit;font-size:12px;text-decoration:underline;color:#526b67}
button:disabled{opacity:.6;cursor:default}button:focus-visible{outline:2px solid #006a64;outline-offset:3px}
@media(prefers-reduced-motion:reduce){.switch-track span{transition:none}}
</style>
