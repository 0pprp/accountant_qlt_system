<template>
  <Message
    :unstyled="false"
    severity="secondary"
    variant="simple"
    icon="pi pi-ban"
    class="py-2 px-4 w-50"
  >
    {{ $t('noData') }}
  </Message>
</template>

<script setup lang="ts">
import { Message } from 'primevue'
</script>
