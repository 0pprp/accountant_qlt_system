import type { App, DefineComponent } from 'vue'

export function registerLayouts(app: App<Element>) {
  const layouts = import.meta.glob('../layouts/*.vue', { eager: true })

  Object.entries(layouts).forEach(([path, layout]) => {
    const match = path.match(/\/([^/]+)\.vue$/)
    const layoutName = match ? match[1] : 'Default'

    app.component(layoutName, (layout as { default: DefineComponent }).default)
  })
}
