<template>
  <div v-if="activeTab !== undefined">
    <Tabs v-model:value="activeTab" :unstyled="false">
      <TabList
        class="flex flex-row flex-auto gap-2 !justify-center !bg-background"
        :unstyled="false"
      >
        <Tab
          :unstyled="false"
          v-for="tab in tabs"
          class="bg-background"
          color="primary"
          :key="tab.title"
          :value="tab.value"
          >{{ tab.title }}
        </Tab>
      </TabList>
    </Tabs>

    <TabPanels>
      <TabPanel :value="activeTab">
        <slot name="content" />
      </TabPanel>
    </TabPanels>
  </div>
</template>

<script setup lang="ts">
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primevue'
import type { Props } from './CustomTab.types'

defineProps<Props>()

const activeTab = defineModel<number | string>('modelValue')
</script>

<style scoped>
.p-tabpanels {
  padding: 0px !important;
}
</style>
