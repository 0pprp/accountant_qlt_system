import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import ViteSvgLoader from 'vite-svg-loader'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), tailwindcss(), ViteSvgLoader()],
  server: {
    host: '0.0.0.0',
    port: 8080,
  },
  preview: {
    allowedHosts : ['panel.qalataldhaman.com'],
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      'primevue/api': 'primevue/api/api.esm.js',
    },
  },
  build: {
    minify: 'esbuild',
    sourcemap: true,
    rollupOptions: {
      output: {
        dir: 'dist',
        entryFileNames: `js/[name]-[hash].js`,
        chunkFileNames: `js/[name]-[hash].js`,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        assetFileNames: (assetInfo: any) => {
          const extensionToDir = {
            '.css': 'css',
            '.svg': 'svgs',
            '.ttf': 'fonts',
          }

          const extension = assetInfo.name.substring(
            assetInfo.name.lastIndexOf('.')
          )

          const dir =
            extension in extensionToDir
              ? extensionToDir[extension as keyof typeof extensionToDir]
              : 'media'

          return `assets/${dir}/[hash][extname]`
        },

        manualChunks: {
          // Framework core
          'vue-vendor': ['vue', 'vue-router', 'pinia'],
          'primevue-core': ['primevue/config'],
          'primevue-components': [
            'primevue/datatable',
            'primevue/column',
            'primevue/button',
            'primevue/inputtext',
          ],
          utils: ['axios', '@tanstack/vue-query'],
          i18n: ['vue-i18n'],
        },
      },
    },
    chunkSizeWarningLimit: 1000,
  },

  optimizeDeps: {
    include: ['vue', 'vue-router', 'pinia', 'vue-i18n', '@tanstack/vue-query'],
    force: false,
  },
  css: {
    devSourcemap: false,
  },
})
