<template>
  <section
    class="min-h-[50vh] flex flex-col items-center justify-center gap-3 px-6 text-[#00201E]"
    role="status"
  >
    <h1 class="text-lg font-bold m-0">{{ $t('access.notFoundTitle') }}</h1>
    <p class="text-sm m-0 text-center max-w-md leading-6">
      {{ $t('access.notFoundMessage') }}
    </p>
    <router-link
      class="min-h-11 inline-flex items-center px-4 rounded-lg bg-primary text-white font-medium"
      :to="{ name: 'HomeMainRoute' }"
    >
      {{ $t('sidebar.home') }}
    </router-link>
  </section>
</template>
