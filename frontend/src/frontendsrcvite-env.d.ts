/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_BYPASS_AUTH?: string
  readonly VITE_USE_MOCK_USER?: string
  readonly VITE_USE_MOCK_CHAT?: string
  readonly VITE_USE_MOCK_AUTH?:string
  readonly VITE_USE_MOCK_GOODS ?:string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
