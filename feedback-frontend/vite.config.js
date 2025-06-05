import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7185', // backend api
        changeOrigin: true,
        secure: false, // gereksiz sebebiyle false -sanırım-
      }
    }
  }
})