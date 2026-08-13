import pluginJs from '@eslint/js'
import typescriptEslint from 'typescript-eslint'
import pluginVue from 'eslint-plugin-vue'
import vueEslintParser from 'vue-eslint-parser'
import importPlugin from 'eslint-plugin-import'
import eslintConfigPrettier from 'eslint-config-prettier'

export default typescriptEslint.config([
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,mts,tsx,vue}'],
  },
  {
    name: 'app/files-to-ignore',
    ignores: [
      '**/dist/**',
      '**/node_modules/**',
      '**/dist-ssr/**',
      '**/coverage/**',
    ],
  },
  {
    files: ['**/*.{ts,vue}'],
    languageOptions: {
      parser: vueEslintParser,
      parserOptions: {
        parser: '@typescript-eslint/parser',
        sourceType: 'module',
      },
      globals: {
        defineProps: 'readonly',
        defineEmits: 'readonly',
        defineExpose: 'readonly',
        withDefaults: 'readonly',
      },
    },
    plugins: {
      import: importPlugin,
      vue: pluginVue,
    },
    extends: [
      ...typescriptEslint.configs.recommended,
      ...pluginVue.configs['flat/essential'],
      ...pluginVue.configs['flat/strongly-recommended'],
    ],
    rules: {
      ...pluginJs.configs.recommended.rules,
      ...importPlugin.configs.recommended.rules,
      'import/no-unresolved': 'off',
      'import/named': 'error',
      'import/namespace': 'error',
      'import/default': 'error',
      'import/export': 'error',
      'import/no-unused-modules': ['warn'],
      'vue/attribute-hyphenation': [
        'error',
        'never',
        {
          ignore: [],
        },
      ],
      'import/order': [
        'error',
        {
          groups: [
            'builtin',
            'external',
            'internal',
            ['parent', 'sibling', 'index'],
          ],
        },
      ],
      'vue/padding-line-between-blocks': ['error', 'always'],
      'vue/no-undef-components': [
        'error',
        {
          ignorePatterns: ['RouterLink', 'RouterView', 'i18n-t'],
        },
      ],
      'vue/singleline-html-element-content-newline': [
        'error',
        {
          normals: 'always',
          exceptions: ['pre', 'textarea'],
        },
      ],
      'vue/define-macros-order': [
        'error',
        {
          order: [
            'defineOptions',
            'defineProps',
            'defineEmits',
            'defineModel',
            'defineExpose',
          ],
          defineExposeLast: false,
        },
      ],
      'vue/no-unused-properties': 'error',
      'vue/comma-dangle': ['error', 'never'],
      'no-eval': 'error',
      'no-console': 'error',
      'vue/component-name-in-template-casing': ['error', 'PascalCase'],
      'vue/return-in-computed-property': [
        'error',
        {
          treatUndefinedAsUnspecified: true,
        },
      ],
      'vue/custom-event-name-casing': [
        'error',
        'camelCase',
        {
          ignores: ['/^[a-z]+(?:-[a-z]+)*:[a-z]+$/u'],
        },
      ],
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': ['error'],
      'vue/no-unused-vars': ['warn'],
      'no-undef': 'off',
      'vue/valid-v-slot': 'off',
    },
    settings: {
      'import/extensions': [
        'error',
        'ignorePackages',
        { '': 'never', js: 'never', jsx: 'never', ts: 'never', tsx: 'never' },
      ],
    },
  },
  {
    files: ['**/*.d.ts'],
    rules: {
      'no-unused-vars': 'off',
    },
  },
  {
    files: ['**/*.ts', '**/*.js', '**/*.tsx', '**/*.vue'],
    rules: {
      'func-style': ['error', 'declaration'],
    },
  },
  eslintConfigPrettier,
])
