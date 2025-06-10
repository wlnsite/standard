<template>
    <div>
        <el-input clearable v-model="query.key" placeholder="Enter 进行筛选" style="width:270px" v-on:keydown.enter.native="getlist(0)"></el-input>
        <el-button type="info" icon="refresh" v-on:click="refresh()">重新加载</el-button>
        <el-button type="primary" icon="plus" v-on:click="modify('')">新增</el-button>
    </div>
    <div class="wln-line"></div>
    <el-table :data="pager.rows" :empty-text="pager.message" style="width:100%">
        <el-table-column width="80" align="center" label="状态" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">
                    <span style="color:#67C23A" v-if="scope.row.state > 0">启用</span>
                    <span style="color:#909399" v-else>停用</span>
                </div>
            </template>
        </el-table-column>
        <el-table-column width="320" label="终端名称/标识" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">{{scope.row.name}}</div>
                <div class="fs12 lh14 c99">ID:{{scope.row.sid}}</div>
            </template>
        </el-table-column>
        <el-table-column width="320" label="终端通讯密钥" fixed="left">
            <template #default="scope">
                <div class="fs14 lh16 c99">公钥：<span class="c-success" v-if="scope.row.public_key == '已录入'">已录入</span><span class="c-primary" v-else>未录入</span></div>
                <div class="fs14 lh16 c99">私钥：<span class="c-success" v-if="scope.row.private_key == '已录入'">已录入</span><span class="c-primary" v-else>未录入</span></div>
            </template>
        </el-table-column>
        <el-table-column></el-table-column>
        <el-table-column width="108" align="center" fixed="right">
            <template #default="scope">
                <el-dropdown placement="bottom-end">
                    <el-button size="small" type="primary">操作<el-icon class="el-icon--right"><arrow-down /></el-icon></el-button>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item v-on:click="modify(scope.row)" icon="edit">查看/编辑</el-dropdown-item>
                            <el-dropdown-item v-on:click="remove(scope.row)" icon="delete">删除</el-dropdown-item>
                        </el-dropdown-menu>
                    </template>
                </el-dropdown>
            </template>
        </el-table-column>
    </el-table>
     <!--弹出框内容-->
    <el-pagination v-on:current-change="getlist" layout="total, prev, pager, next, jumper" :total="pager.total" :current-page="query.page" :page-size="query.size"></el-pagination>
    <div class="wln-mask-layout" v-if="form.drawer">
        <div class="wln-mask-form" style="width:580px;">
            <div class="wln-title">收款终端信息</div>
            <el-form label-width="120px">
                <el-form-item label="终端标识">
                    <el-input v-model="form.sid" style="width: 256px" placeholder="15位收款终端标识ID" :disabled="form.create_time > 0"></el-input>
                    <el-select v-model="form.state" placeholder="状态" style="width:100px">
                        <el-option label="启用" :value="1"></el-option>
                        <el-option label="停用" :value="0"></el-option>
                    </el-select><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="终端名称">
                    <el-input v-model="form.name" style="width: 360px" placeholder="如：张三、停车场等"></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="终端公钥">
                    <el-input v-model="form.public_key" style="width:360px" placeholder="应用端验签、加密用公钥"></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="终端私钥 ">
                    <el-input v-model="form.private_key" style="width:360px" placeholder="终端发起API请求签名、解密私钥"></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item class="el-form-btns">
                    <el-button icon="check" type="primary" v-on:click="submit">保存</el-button>
                    <el-button icon="close" v-on:click="form.drawer=false">取消/关闭窗口</el-button>
                </el-form-item>
            </el-form>
        </div>
    </div>
</template>

<script setup>
    let mForm = {
        sn: '',
        sid: '',
        name: '',
        state: 1,
        public_key: '',
        private_key: '',
        create_time: 0,
        drawer: false
    }
    import { ref, reactive, onMounted, getCurrentInstance } from 'vue'
    import { mQuery, mPager } from "@/wlnapp/model.js"
    const { ctx, proxy: { wln } } = getCurrentInstance()
    const form = reactive({ ...mForm })
    const pager = reactive({ ...mPager })
    const query = reactive({ ...mQuery })
    function refresh() {
        pager.rows = []
        pager.message = pager.loadMsg
        getlist(0)
    }
    function getlist(page) {
        form.drawer = false
        query.page = parseInt(page) || 0
        pager.message = pager.loadMsg
        wln.api('/certkey/pager', (res) => {
            pager.rows = res.data.rows || []
            pager.total = res.data.total || 0
            pager.message = res.success ? res.data.message : res.message
        }, query, true, true, (res) => { pager.message = pager.errMsg })
    }
    function submit() {
        wln.api('/certkey/submit', (res) => {
            wln.toast(res.message, res.success ? 'success' : 'error')
            if (res.success) {
                getlist()
            }
        }, form)
    }
    function remove(row) {
        wln.confirm('数据一经删除将无法恢复，是否继续？', () => {
            wln.api('/certkey/remove', (res) => {
                wln.toast(res.message, res.success)
                if (res.success) {
                    getlist()
                }
            }, { sn: row.sn })
        })
    }
    function modify(row) {
        for (let i in mForm) { form[i] = mForm[i] }
        if (row) {
            wln.api('/certkey/modify', (res) => {
                if (res.success) {
                    for (let i in mForm) { form[i] = res.data[i] }
                    form.drawer = true
                } else {
                    wln.toast(res.message)
                }
            }, { sn: row.sn ,sid: row.sid })
        } else {
            form.drawer = true
        }
    }
    onMounted(() => {
        refresh()
    })
</script>