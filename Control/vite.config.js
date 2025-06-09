// Plugins
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'
import { fileURLToPath, URL } from 'node:url'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        vue(),
        // https://element-plus.gitee.io/zh-CN/guide/quickstart.html#%E6%8C%89%E9%9C%80%E5%AF%BC%E5%85%A5
        AutoImport({
            resolvers: [ElementPlusResolver()],
        }),
        Components({
            resolvers: [ElementPlusResolver()],
        }),
    ],
    base: process.env.NODE_ENV === 'development' ? '': '/',
    //base: process.env.NODE_ENV === 'development' ? '': '/vapp',
    build: {
        outDir: "../Server/wwwroot", 
        // outDir: "../Server/wwwroot/vapp", 
        emptyOutDir: true 
    },
    define: { 'process.env': {} },
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        },
        extensions: [
            '.js',
            '.json',
            '.jsx',
            '.mjs',
            '.ts',
            '.tsx',
            '.vue',
        ],
    },
    server: {
        port: 7800,
        cors: false,
        proxy: {
            "/control/service": { target: "http://127.0.0.1:8008", changeOrigin: true, ws: false }
        }
    },
})