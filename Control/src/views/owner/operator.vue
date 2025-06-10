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
        <el-table-column width="168" label="用户sid" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">{{scope.row.sid}}</div>
            </template>
        </el-table-column>
        <el-table-column width="168" label="员工姓名" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">{{scope.row.name}}</div>
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
            <div class="wln-title">操作人员信息</div>
            <el-form label-width="120px">
                <el-form-item label="用户id">
                    <el-input v-model="form.sid" style="width: 256px" placeholder="" ></el-input>
                    <el-select v-model="form.state" placeholder="状态" style="width:100px">
                        <el-option label="启用" :value="1"></el-option>
                        <el-option label="停用" :value="0"></el-option>
                    </el-select><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="员工角色">
                    <el-select v-model="form.role" placeholder="请选择员工角色身份" style="width: 360px">
                        <el-option label="管理人员" value="manger"></el-option>
                        <el-option label="普通人员" value="operator"></el-option>
                    </el-select><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="员工姓名">
                    <el-input v-model="form.name" style="width:360px" placeholder=""></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="手机号码 ">
                    <el-input v-model="form.mobile" style="width:360px" placeholder=""></el-input><span class="tips notnull"></span>
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
        sid	: '',
        name: '',
        role: '',
        state: 1,
        value: '',
        label: '',
        mobile: '',
        owner: '',
        create_time: '',
        time_create: '',
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
        wln.api('/operator/pager', (res) => {
            pager.rows = res.data.rows || []
            pager.total = res.data.total || 0
            pager.message = res.success ? res.data.message : res.message
        }, query, true, true, (res) => { pager.message = pager.errMsg })
    }
    function submit() {
        wln.api('/operator/submit', (res) => {
            wln.toast(res.message, res.success ? 'success' : 'error')
            if (res.success) {
                getlist()
            }
        }, form )
    }
    function remove(row) {
        wln.confirm('数据一经删除将无法恢复，是否继续？', () => {
            wln.api('/operator/remove', (res) => {
                wln.toast(res.message, res.success)
                if (res.success) {
                    getlist()
                }
            }, { sid: row.sid , owner: row.owner})
        })
    }
    function modify(row) {
        for (let i in mForm) { form[i] = mForm[i] }
        if (row) {
            wln.api('/operator/modify', (res) => {
                if (res.success) {
                    for (let i in mForm) { form[i] = res.data[i] }
                    form.drawer = true
                } else {
                    wln.toast(res.message)
                }
            }, {  sid: row.sid , owner: row.owner })
        } else {
            form.drawer = true
        }
    }
    onMounted(() => {
        refresh()
    })
</script>