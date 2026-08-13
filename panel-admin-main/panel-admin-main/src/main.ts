import { createApp } from 'vue'
import './assets/styles/main.css'
import App from './App.vue'
import { registerPlugins } from '@/modules/Core/plugins'
import '@/registerModules'

const app = createApp(App)

registerPlugins(app)

app.mount('#app')
