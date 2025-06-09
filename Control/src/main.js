import { createApp } from 'vue'
import { createRouter, createWebHistory, createWebHashHistory } from 'vue-router'

import App from './App.vue'
import Wln from '@/wlnapp/wln.js'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import 'element-plus/dist/index.css'
import { ElMessage, ElMessageBox} from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
const app = createApp(App)
const router = createRouter({ history: createWebHistory('/'), routes: [{ path: "/:pathMatch(.*)*", component: () => import("@/wlnapp/404.vue")}] })
const cfgs = { api: (localStorage.getItem('control_service') || '') + '/control/service', debug: localStorage.getItem('debug') == 'true', pk: localStorage.getItem('pub') || '' }
const wln = Wln(cfgs, {
    noauth: (obj) => {
        if(obj && obj.message) {
            wln.alert(obj.message)
        } else {
            wln.alert('未登录，请先登录',() => {console.log('by noauth')})
        }
    },
    toast: (msg, type) => {
        if(typeof type == 'boolean' && type) {
            type = 'success'
        }
        ElMessage({ message: msg, grouping: true, duration: 3000, type: type || 'info' })
    },
    alert: (msg, fnOk) => {
        ElMessageBox.alert(msg, '提示', { callback: fnOk, confirmButtonText: '确定' })
    },
    prompt: (msg, fnYes, fnNot, txtYes, txtNot, inputTips) => {
        ElMessageBox.prompt(msg, '请输入', { confirmButtonText: txtYes, cancelButtonText: txtNot, inputErrorMessage: inputTips }).then((res) => {
            if(res.action == 'confirm') { fnYes(res.value) }
        }).catch(fnNot)
    },
    confirm: (msg, fnYes, fnNot, txtYes, txtNot) => {
        ElMessageBox.confirm(msg, '操作确认', { confirmButtonText: txtYes, cancelButtonText: txtNot }).then(fnYes).catch(fnNot)
    },
	goback: (delta) => {
		router.back(delta)
	},
	gourl: (url, type) => {
		router.push(url)
    },
    tabto: (title, urlto) => {
        if (urlto.indexOf('/') < 0) {
            let pathto = location.pathname.substr(0, location.pathname.lastIndexOf('/') + 1) + urlto
            urlto = `${location.origin}${pathto}`
        }
        window.emiOpenTab(urlto, title)
    },
    uploader: (path, accept, fn) => {
        let _up = document.createElement('input');
        let _attType = document.createAttribute("type");
        _attType.nodeValue = "file"
        _up.setAttributeNode(_attType)

        if (accept) {
            var _attAccept = document.createAttribute("accept");
            _attAccept.nodeValue = accept
            _up.setAttributeNode(_attAccept)
        }
        _up.addEventListener('change', function () {
            if (!this.files || this.files.length !== 1) {
                return;
            } else {
                let file = this.files[0]
                wln.upload(path, file, fn, accept)
            }
        })
        _up.click()
    }
})
app.use(router)
app.use(ElementPlus, { locale: zhCn })
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}
app.config.globalProperties.wln = wln
if(location.search && location.search.indexOf('ehost') > 0 && location.search.indexOf('xsession') > 0) {
	wln.api('/authx' + location.search, (res) => {
		if(res.success) {
			localStorage.setItem('ticket', res.ticket)
			localStorage.setItem('x-domain', res.domain)
		    app.mount('#app')
		} else {
            wln.alert(res.message)
        }
	}, {}, false, true)
} else {
	app.mount('#app')
}
router.isReady().then(() => {
    let base = '/control'
    router.addRoute({
        path: `${base}/utility`,
        children: [
            { path: 'user', component: () => import('@/views/user/list.vue') },
            { path: 'owner', component: () => import('@/views/owner/list.vue') },
            { path: 'owner_certkey', component: () => import('@/views/owner/certkey.vue') }
        ]
    })
    router.addRoute({
        path: `${base}/contract`,
        children: [
            { path: 'merchant', component: () => import('@/views/payset/merchant.vue') },
            { path: 'terminal', component: () => import('@/views/payset/terminal.vue') },
            { path: 'operator', component: () => import('@/views/payset/operator.vue') }
        ]
    })
    router.replace(router.currentRoute.value.fullPath)
})