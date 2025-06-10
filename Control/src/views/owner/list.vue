<template>
    <div>
        <el-input clearable v-model="query.key" placeholder="Enter 进行筛选" style="width:270px" v-on:keydown.enter.native="getlist(0)"></el-input>
        <el-button type="info" icon="refresh" v-on:click="refresh()">重新加载</el-button>
        <el-button type="primary" icon="plus" v-on:click="modify('')">新增</el-button>
    </div>
    <div class="wln-line"></div>
    <el-table :data="pager.rows" :empty-text="pager.message" style="width:100%">
        <el-table-column width="120" label="类型" align="center" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18"><span v-if="scope.row.role == 'buyer'">发运方</span><span v-else-if="scope.row.role == 'supplier'">承运方</span></div>
            </template>
        </el-table-column>
        <el-table-column width="168" label="机构编码" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">{{scope.row.id}}</div>
            </template>
        </el-table-column>
        <el-table-column width="320" label="机构名称" fixed="left">
            <template #default="scope">
                <div class="fs15 lh18">{{scope.row.name}}</div>
            </template>
        </el-table-column>
        <el-table-column width="320" label="证件号码">
            <template #default="scope">
                <div class="fs15 lh18"><span style="color:#409eff" v-if="scope.row.certificate">{{scope.row.certificate}}</span><span v-else>未录入</span></div>
            </template>
        </el-table-column>
        <el-table-column></el-table-column>
        <el-table-column width="168" label="登记时间">
            <template #default="scope">{{scope.row.time_create.showTime()}}</template>
        </el-table-column>
        <el-table-column width="108" align="center" fixed="right">
            <template #default="scope">
                <el-dropdown placement="bottom-end">
                    <el-button size="small" type="primary">操作<el-icon class="el-icon--right"><arrow-down /></el-icon></el-button>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item v-on:click="modify(scope.row)" icon="edit">查看/编辑</el-dropdown-item>
                            <el-dropdown-item v-on:click="wln.ext.tabto('操作员管理', `owner_operator?owner=${scope.row.id}&name=${scope.row.name}`)" icon="document">操作员管理</el-dropdown-item>
                            <el-dropdown-item v-on:click="wln.ext.tabto('证书密钥管理', `owner_certkey?owner=${scope.row.id}&name=${scope.row.name}`)" icon="document">证书密钥管理</el-dropdown-item>
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
            <div class="wln-title">机构信息</div>
            <el-form label-width="120px">
                <el-form-item label="机构名称">
                    <el-input v-model="form.name" style="width: 360px" placeholder="商户名称、说明备注等"></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="默认身份">
                    <el-select v-model="form.role" placeholder="请选择机构默认登录身份" style="width: 360px">
                        <el-option label="发运方" value="buyer"></el-option>
                        <el-option label="承运方" value="supplier"></el-option>
                    </el-select><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="证件号码">
                    <el-input v-model="form.certificate" style="width:360px" placeholder="机构证件号码"></el-input><span class="tips notnull"></span>
                </el-form-item>
                <el-form-item label="证件照片">
                    <div>
                        <el-input v-model="form.cert_picture" disabled placeholder="图片最大尺寸2M" style="width: 360px"></el-input> <el-button type="primary" v-on:click="toUpload('cert_picture')">点击上传</el-button>
                        <div v-if="form.cert_picture" style="margin-top:10px"><img :src="form.cert_picture " style="max-height:120px" /></div><span class="wln-tips"></span>
                    </div>
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
        owner: '',
        id: '',
        role: '',
        name: '',
        certificate: '',
        cert_picture: '',
        drawer: false
    }
    import { ref, reactive, onMounted, getCurrentInstance } from 'vue'
    import { mQuery, mPager } from "@/wlnapp/model.js"
    const { ctx, proxy: { wln } } = getCurrentInstance()
    const form = reactive({ ...mForm })
    const pager = reactive({ ...mPager })
    const query = reactive({ ...mQuery })
    const refresh = () => {
        pager.rows = []
        pager.message = pager.loadMsg
        getlist(0)
    }

    const getlist = (page) => {
        form.drawer = false
        query.page = parseInt(page) || 0
        pager.message = pager.loadMsg
        wln.api('/owner/pager', (res) => {
            pager.rows = res.data.rows || []
            pager.total = res.data.total || 0
            pager.message = res.success ? res.data.message : res.message
        }, query, true, true, (res) => { pager.message = pager.errMsg })
    }

    const submit = () => {
        wln.api('/owner/submit', (res) => {
            wln.toast(res.message, res.success ? 'success' : 'error')
            if (res.success) {
                getlist()
            }
        }, form)
    }

    const remove = (row) => {
        wln.confirm('数据一经删除将无法恢复，是否继续？', () => {
            wln.api('/owner/remove', (res) => {
                wln.toast(res.message, res.success)
                if (res.success) {
                    getlist()
                }
            }, { id: row.id })
        })
    }

    const modify = (row) => {
        for (let i in mForm) { form[i] = mForm[i] }
        if (row) {
            wln.api('/owner/modify', (res) => {
                if (res.success) {
                    for (let i in mForm) { form[i] = res.data[i] }
                    form.drawer = true
                } else {
                    wln.toast(res.message)
                }
            }, { id: row.id })
        } else {
            form.drawer = true
        }
    }

    const toUpload = (key) => {
        wln.ext.uploader('/upload', 'jpg,png,gif,jpeg', (res) => {
            if (res.success) {
                form[key] = res.url
            } else {
                wln.alert(res.message)
            }
        })
    }

    onMounted(() => {
        refresh()
    })
</script>