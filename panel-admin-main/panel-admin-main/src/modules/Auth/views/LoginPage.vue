<template>
  <div
    class="entry"
    dir="rtl"
  >
    <!-- انترو تطبيق الفريق -->
    <div
      v-if="showIntro"
      class="entry-intro"
      aria-hidden="true"
    >
      <img
        class="entry-intro__lines-tl"
        src="/brand/lines_top_left.svg"
        alt=""
      >
      <img
        class="entry-intro__lines-br"
        src="/brand/lines_bottom_right.svg"
        alt=""
      >
      <div
        class="entry-intro__blob"
        :class="{ 'entry-intro__blob--run': introReady }"
      />
      <img
        class="entry-intro__logo"
        :class="{ 'entry-intro__logo--in': introReady }"
        src="/brand/logo_green.svg"
        width="140"
        height="102"
        alt=""
      >
      <p
        class="entry-intro__title"
        :class="{ 'entry-intro__title--in': introReady }"
      >
        قلعة الضمان
      </p>
    </div>

    <div
      class="entry-page"
      :class="{ 'entry-page--visible': !showIntro }"
    >
      <div class="entry-stage">
        <div
          class="entry-wave"
          aria-hidden="true"
        >
          <svg
            class="entry-wave__svg"
            viewBox="0 0 1440 420"
            preserveAspectRatio="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M0 0H1440V268C1338 312 1210 340 1048 328C820 310 780 250 560 220C340 190 220 230 0 268V0Z"
              fill="#006A64"
            />
            <path
              d="M0 248C180 210 320 188 520 210C760 236 820 290 1040 302C1200 312 1340 290 1440 262V288C1330 318 1200 340 1040 328C820 312 760 258 520 234C320 212 180 232 0 272V248Z"
              fill="#005752"
              opacity="0.35"
            />
          </svg>
          <img
            class="entry-wave__lines"
            src="/brand/lines_top_left.svg"
            alt=""
          >
          <div class="entry-brand">
            <img
              src="/brand/logo_white.svg"
              class="entry-brand__logo"
              alt=""
            >
            <h1>قلعة الضمان</h1>
          </div>
        </div>

        <img
          class="entry-stage__lines"
          src="/brand/lines_bottom_right.svg"
          alt=""
        >

        <div class="entry-body">
          <header class="entry-hello">
            <p class="entry-hello__title">
              مرحبا بك
            </p>
            <p class="entry-hello__sub">
              يرجى تسجيل الدخول
            </p>
          </header>

          <div class="entry-form">
            <LoginForm
              :loading="isPending"
              @submit="onSubmit"
            />

            <AppButton
              type="submit"
              form="login-form"
              variant="primary"
              block
              class="entry-form__submit"
              :loading="isPending"
              :disabled="isPending"
            >
              تسجيل دخول
            </AppButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import LoginForm from '../components/LoginForm/LoginForm.vue'
import type { AuthLoginForm } from '../types/model'
import { useAuthLoginMutation } from '@/modules/Auth/requests/mutations'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton } from '@/modules/Core/components/ui'

const router = useRouter()
const { mutateAsync, isPending } = useAuthLoginMutation()
const authStore = useAuthStore()

const showIntro = ref(true)
const introReady = ref(false)
let introTimer: ReturnType<typeof setTimeout> | null = null
let readyTimer: ReturnType<typeof setTimeout> | null = null

onMounted(() => {
  readyTimer = setTimeout(() => {
    introReady.value = true
  }, 40)
  introTimer = setTimeout(() => {
    showIntro.value = false
  }, 3800)
})

onUnmounted(() => {
  if (introTimer) clearTimeout(introTimer)
  if (readyTimer) clearTimeout(readyTimer)
})

async function onSubmit(values: AuthLoginForm) {
  const { data } = await mutateAsync(values)
  authStore.replaceSession(data)
  router.push({ name: 'HomeMainRoute' })
}
</script>

<style scoped>
.entry {
  --brand: #006a64;
  --brand-deep: #004f4b;
  --brand-glow: rgba(0, 106, 100, 0.45);
  --brand-glow-soft: rgba(0, 106, 100, 0.18);
  --entry-wave-h: clamp(200px, 28vw, 300px);
  min-height: 100dvh;
  background: #f4f7f7;
  color: #00201e;
  position: relative;
  overflow: hidden;
}

/* —— Intro —— */
.entry-intro {
  position: fixed;
  inset: 0;
  z-index: 40;
  background: #f8f8f8;
  display: grid;
  place-items: center;
}
.entry-intro__lines-tl {
  position: absolute;
  top: 0;
  inset-inline-start: 0;
  width: min(55vw, 360px);
  pointer-events: none;
}
.entry-intro__lines-br {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.5;
  pointer-events: none;
}
.entry-intro__blob {
  position: absolute;
  width: 140vmax;
  height: 140vmax;
  border-radius: 50%;
  background: var(--brand);
  transform: scale(4);
}
.entry-intro__blob--run {
  animation:
    blob-in 500ms 500ms cubic-bezier(0.23, 1, 0.32, 1) forwards,
    blob-out 800ms 1500ms cubic-bezier(0.23, 1, 0.32, 1) forwards;
}
.entry-intro__logo {
  position: relative;
  z-index: 2;
  opacity: 0;
  transform: scale(0.5);
}
.entry-intro__logo--in {
  animation: logo-in 500ms 1600ms cubic-bezier(0.23, 1, 0.32, 1) forwards;
}
.entry-intro__title {
  position: absolute;
  z-index: 2;
  margin: 0;
  font-size: clamp(1.6rem, 4vw, 2.2rem);
  font-weight: 700;
  color: var(--brand);
  opacity: 0;
  transform: translateY(35px);
}
.entry-intro__title--in {
  animation: title-in 400ms 2500ms ease-in-out forwards;
}

@keyframes blob-in {
  to {
    transform: scale(0.25);
  }
}
@keyframes blob-out {
  to {
    transform: scale(2.5);
    opacity: 0;
  }
}
@keyframes logo-in {
  to {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes title-in {
  to {
    opacity: 1;
    transform: translateY(85px);
  }
}

.entry-page {
  min-height: 100dvh;
  opacity: 0;
  pointer-events: none;
  transition: opacity 280ms ease;
}
.entry-page--visible {
  opacity: 1;
  pointer-events: auto;
}

.entry-stage {
  position: relative;
  min-height: 100dvh;
  width: 100%;
  display: flex;
  flex-direction: column;
}

.entry-wave {
  position: relative;
  z-index: 1;
  width: 100%;
  height: var(--entry-wave-h);
  flex-shrink: 0;
  overflow: hidden;
}
.entry-wave__svg {
  position: absolute;
  inset: 0;
  display: block;
  width: 100%;
  height: 118%;
}
.entry-wave__lines {
  position: absolute;
  top: 0;
  inset-inline-start: 0;
  width: min(46vw, 440px);
  opacity: 0.85;
  pointer-events: none;
  animation: settle 700ms ease-out;
  transform-origin: top left;
}

.entry-brand {
  position: relative;
  z-index: 2;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px 16px 40px;
  color: #fff;
  box-sizing: border-box;
}
.entry-brand__logo {
  width: clamp(72px, 10vw, 108px);
  height: auto;
}
.entry-brand h1 {
  margin: 0;
  font-size: clamp(1.5rem, 3.2vw, 2.1rem);
  font-weight: 700;
  color: #fff;
}

.entry-stage__lines {
  position: absolute;
  left: 0;
  right: 0;
  top: calc(var(--entry-wave-h) - 24px);
  bottom: 0;
  width: 100%;
  object-fit: cover;
  opacity: 0.28;
  pointer-events: none;
  z-index: 0;
  animation: settle 700ms ease-out;
}

/* الجسم تحت الموجة — بدون كونتينر يغلّف الحقول */
.entry-body {
  position: relative;
  z-index: 2;
  flex: 1;
  width: min(100% - 40px, 560px);
  margin: 0 auto;
  padding: clamp(36px, 5.5vw, 64px) 0 clamp(40px, 5vw, 64px);
  display: flex;
  flex-direction: column;
  gap: clamp(28px, 4vw, 40px);
}

.entry-hello {
  margin: 0;
  text-align: start;
}
.entry-hello__title {
  margin: 0;
  font-size: clamp(1.55rem, 2.8vw, 1.95rem);
  font-weight: 700;
  color: var(--brand);
}
.entry-hello__sub {
  margin: 10px 0 0;
  font-size: clamp(0.95rem, 1.5vw, 1.08rem);
  color: #5f7270;
  line-height: 1.5;
}

.entry-form {
  display: flex;
  flex-direction: column;
  gap: clamp(24px, 3.2vw, 32px);
}
.entry-form__submit {
  margin-top: 4px;
}
.entry-form :deep(.app-button),
.entry-form :deep(button.app-button),
.entry-form :deep(.q-btn) {
  min-height: 54px;
  border-radius: 14px;
  font-size: 1.08rem;
  font-weight: 700;
  box-shadow:
    0 0 0 1px rgba(0, 106, 100, 0.25),
    0 0 16px var(--brand-glow-soft),
    0 10px 22px rgba(0, 79, 75, 0.2);
}

/* النيون على الحقل نفسه فقط */
.entry-form :deep(.login-form) {
  display: grid;
  gap: clamp(22px, 3vw, 28px);
}
.entry-form :deep(.q-field),
.entry-form :deep(.app-field) {
  gap: 10px;
}
.entry-form :deep(label),
.entry-form :deep(.app-field__label),
.entry-form :deep(.q-field__label) {
  color: #3d5552;
  font-size: 0.98rem;
  font-weight: 600;
}
.entry-form :deep(.q-input),
.entry-form :deep(input.q-input) {
  min-height: 54px !important;
  border-radius: 14px !important;
  background: #fff !important;
  border: 1.5px solid rgba(0, 106, 100, 0.5) !important;
  box-shadow:
    0 0 0 1px rgba(0, 106, 100, 0.1),
    0 0 16px rgba(0, 106, 100, 0.16) !important;
  padding-block: 12px !important;
  padding-inline: 16px !important;
  transition:
    border-color 0.18s ease,
    box-shadow 0.18s ease;
}
.entry-form :deep(.q-input:hover),
.entry-form :deep(input.q-input:hover) {
  border-color: rgba(0, 106, 100, 0.78) !important;
  box-shadow:
    0 0 0 1px rgba(0, 106, 100, 0.16),
    0 0 20px rgba(0, 106, 100, 0.28) !important;
}
.entry-form :deep(.q-input:focus),
.entry-form :deep(.q-input:focus-visible),
.entry-form :deep(input.q-input:focus),
.entry-form :deep(input.q-input:focus-visible) {
  outline: none !important;
  border-color: var(--brand) !important;
  box-shadow:
    0 0 0 3px rgba(0, 106, 100, 0.2),
    0 0 24px var(--brand-glow) !important;
}
.entry-form :deep(.q-control-shell),
.entry-form :deep(.q-control-shell--password) {
  background: transparent !important;
  border: 0 !important;
  box-shadow: none !important;
  padding: 0 !important;
  min-height: 0 !important;
}
.entry-form :deep(.q-control-shell--password .q-input--has-action) {
  padding-inline-end: 52px !important;
}
.entry-form :deep(.q-pw-toggle) {
  color: var(--brand);
  width: 42px;
  height: 42px;
  background: transparent !important;
  border: 0 !important;
  box-shadow: none !important;
}
.entry-form :deep(.q-pw-toggle:hover) {
  background: rgba(0, 106, 100, 0.1) !important;
  color: var(--brand-deep);
}

@keyframes settle {
  from {
    transform: scale(1.25);
  }
  to {
    transform: scale(1);
  }
}

@media (min-width: 768px) {
  .entry {
    --entry-wave-h: clamp(220px, 26vw, 320px);
  }
  .entry-body {
    width: min(100% - 56px, 580px);
    padding-top: clamp(44px, 6vw, 68px);
  }
}

@media (min-width: 1024px) {
  .entry {
    --entry-wave-h: min(32vh, 340px);
  }
  .entry-body {
    width: min(100% - 72px, 600px);
    padding-top: 60px;
  }
}

@media (min-width: 1400px) {
  .entry {
    --entry-wave-h: min(30vh, 360px);
  }
  .entry-body {
    width: min(100% - 88px, 640px);
    padding-top: 72px;
  }
}
</style>
