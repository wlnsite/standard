using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;

namespace Controllers.Control
{
    /// <summary>
    /// 机构证书管理
    /// </summary>
    [ApiController, ApiGroup("Control")]
    [Route(RoutePrefix + "[controller]/[action]")]
    public class CertKeyController : BaseController
    {
        /// <summary>
        /// 分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<ApiResult<Wlniao.Pager<Dto.User>>>(0)]
        [WlniaoQueryParameter(Name = "size", Description = "每页返回数量长度，默认：10", Required = false)]
        [WlniaoQueryParameter(Name = "page", Description = "当前页码：默认为1", Required = false)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        [WlniaoQueryParameter(Name = "owner", Description = "所属机构", Required = true)]
        public IActionResult pager()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var page = obj.GetInt32("page");
                var size = obj.GetInt32("size", 10);
                var owner = obj.GetInt32("owner");
                var result = new ApiResult<Wlniao.Pager<Dto.CertKey>>
                {
                    code = "-1",
                    tips = true,
                    data = new Wlniao.Pager<Dto.CertKey> { size = size, page = page }
                };
                try
                {
                    var db = new SqlContext();
                    var exp = Expressionable.Create<Models.CertKey>().And(o => o.state >= 0 && o.owner == owner);
                    var key = obj.GetString("key");
                    if (key.IsNotNullAndEmpty())
                    {
                        exp = exp.And(o => o.sn.Contains(key));
                    }
                    var query = db.Queryable<Models.CertKey>().Where(exp.ToExpression());
                    var rows = query.OrderBy(o => o.time_expire).Skip(result.data.skip).Take(result.data.size).ToList();
                    foreach (var row in rows)
                    {
                        result.data.rows.Add(new Dto.CertKey
                        {
                            sn = row.sn,
                            state = row.state,
                            time_create = row.time_create,
                            time_expire = row.time_expire,
                        });
                    }
                    result.data.total = query.Count();
                    result.code = "0";
                    result.message = "查询完成，数据已返回";
                }
                catch (Exception ex)
                {
                    result.message = ex.Message;
                }
                return OutputSerialize(result);
            });
        }

        /// <summary>
        /// 提交保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [WlniaoBodyParameter(Type = typeof(Dto.User))]
        [ProducesResponseType<ApiResult<string>>(0)]
        [WlniaoQueryParameter(Name = "sid", Description = "用户标识", Required = true)]
        [WlniaoQueryParameter(Name = "role", Description = "授权角色 manger/operator", Required = true)]
        [WlniaoQueryParameter(Name = "owner", Description = "所属机构", Required = true)]
        public IActionResult submit()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var sn = obj.GetString("sn");
                var owner = obj.GetInt32("owner");
                var public_key = obj.GetString("public_key");
                var private_key = obj.GetString("private_key");
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (owner < 0)
                {
                    return OutputMessage(result, "所属机构未输入，请输入", "101");
                }
                if (string.IsNullOrEmpty(sn) && string.IsNullOrEmpty(public_key))
                {
                    return OutputMessage(result, "证书公钥未输入，请输入", "101");
                }
                else if (!string.IsNullOrEmpty(public_key))
                {
                    var sm4key = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(Wlniao.Crypto.Helper.Decode(public_key)), 4, 8).Replace("-", "");
                    sn = Wlniao.Encryptor.SM4EncryptECBToHex(owner.ToString(), sm4key);
                }
                var db = new SqlContext();
                var row = string.IsNullOrEmpty(sn) ? null : db.Queryable<Models.CertKey>().Where(o => o.sn == sn && o.owner == owner).First();
                if (row == null)
                {
                    row = new Models.CertKey { sn = sn, owner = owner, time_create = DateTools.GetUnix() };
                }
                row.state = obj.GetInt32("state");
                row.public_key = obj.GetString("public_key");
                row.private_key = obj.GetString("private_key");
                row.memo = obj.GetString("memo");
                row.only_to = obj.GetString("only_to");

                if (db.Storageable<Models.CertKey>(row).ExecuteCommand() > 0)
                {
                    result.code = "200";
                    result.data = row.sn;
                    result.success = true;
                    result.message = "提交成功，操作已保存";
                }
                else
                {
                    result.code = "1";
                    result.message = "提交失败，数据未保存";
                }
                return OutputSerialize(result);
            });
        }



        /// <summary>
        /// 数据删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [WlniaoQueryParameter(Name = "sn", Description = "证书序列号", Required = true)]
        [ProducesResponseType<ApiResult<string>>(0)]
        public IActionResult remove()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var key = obj.GetString("sn");
                var row = string.IsNullOrEmpty(key) ? null : db.Queryable<Models.CertKey>().Where(o => o.sn == key).First();
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (row == null)
                {
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    row.state = -1;
                    if (db.Updateable<Models.CertKey>(row).ExecuteCommand() > 0)
                    {
                        result.code = "200";
                        result.data = key;
                        result.success = true;
                        result.message = "提交成功，操作已删除";
                    }
                    else
                    {
                        result.code = "1";
                        result.message = "提交失败，数据未删除";
                    }
                }
                return OutputSerialize(result);
            });
        }

        /// <summary>
        /// 修改查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [WlniaoQueryParameter(Name = "sn", Description = "证书序列号", Required = true)]
        [ProducesResponseType<ApiResult<Dto.CertKey>>(0)]
        public IActionResult modify()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var key = obj.GetString("sn");
                var row = string.IsNullOrEmpty(key) ? null : db.Queryable<Models.CertKey>().Where(o => o.sn == key && o.state >= 0).First();
                var result = new ApiResult<Dto.CertKey> { code = "0", tips = true };
                if (row == null)
                {
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    result.code = "200";
                    result.data = new Dto.CertKey
                    {
                        sn = row.sn,
                        state = row.state,
                        owner = row.owner,
                        only_to = row.only_to,
                        public_key = row.public_key,
                        private_key = row.private_key
                    };
                    result.success = true;
                    result.message = "查询成功，数据已返回";
                }
                return OutputSerialize(result);
            });
        }
    
    
    }
}