using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;
using Wlniao.XServer;

namespace Controllers.Control
{
    /// <summary>
    /// 机构信息管理
    /// </summary>
    [ApiController, ApiGroup("Control")]
    [Route(RoutePrefix + "[controller]/[action]")]
    public class OwnerController : BaseController
    {
        /// <summary>
        /// 下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<ApiResult<List<Dto.Option>>>(0)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        public IActionResult list()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var result = new ApiResult<List<Dto.Option>>
                {
                    code = "-1",
                    tips = true,
                    data = new List<Dto.Option>()
                };
                try
                {
                    var db = new SqlContext();
                    var exp = Expressionable.Create<Models.Owner>().And(o => true);
                    var key = obj.GetString("key");
                    var role = obj.GetString("role");
                    if (key.IsNotNullAndEmpty())
                    {
                        exp = exp.And(o => o.name.Contains(key));
                    }
                    if (role.IsNotNullAndEmpty())
                    {
                        exp = exp.And(o => o.role == role);
                    }
                    var query = db.Queryable<Models.Owner>().Where(exp.ToExpression());
                    var rows = query.OrderBy(o => o.time_create).ToList();
                    foreach (var row in rows)
                    {
                        result.data.Add(new Dto.Option
                        {
                            value = row.id.ToString(),
                            label = row.name
                        });
                    }
                    result.code = "0";
                    result.success = true;
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
        /// 分页列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<ApiResult<Wlniao.Pager<Dto.Owner>>>(0)]
        [WlniaoQueryParameter(Name = "size", Description = "每页返回数量长度，默认：10", Required = false)]
        [WlniaoQueryParameter(Name = "page", Description = "当前页码：默认为1", Required = false)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        public IActionResult pager()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var page = obj.GetInt32("page");
                var size = obj.GetInt32("size", 10);
                var result = new ApiResult<Wlniao.Pager<Dto.Owner>>
                {
                    code = "-1",
                    tips = true,
                    data = new Wlniao.Pager<Dto.Owner> { size = size, page = page }
                };
                try
                {
                    var db = new SqlContext();
                    var exp = Expressionable.Create<Models.Owner>().And(o => true);
                    var key = obj.GetString("key");
                    if (key.IsNotNullAndEmpty())
                    {
                        exp = exp.And(o => o.name.Contains(key));
                    }
                    var query = db.Queryable<Models.Owner>().Where(exp.ToExpression());
                    var rows = query.OrderBy(o => o.time_create).Skip(result.data.skip).Take(result.data.size).ToList();
                    foreach (var row in rows)
                    {
                        result.data.rows.Add(row.ConvertToDto());
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
        [WlniaoBodyParameter(Type = typeof(Dto.Owner))]
        [ProducesResponseType<ApiResult<string>>(0)]
        public IActionResult submit()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var key = obj.GetInt32("id");
                var row = key <= 0 ? null : db.Queryable<Models.Owner>().Where(o => o.id == key).First();
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (row == null)
                {
                    if (key > 0)
                    {
                        result.code = "404";
                        result.message = "商户ID无效";
                        return OutputSerialize(result);
                    }
                    else
                    {
                        row = new Models.Owner { id = Models.Owner.NewId(), time_create = DateTools.GetUnix() };
                    }
                }
                row.role = obj.GetString("role");
                row.name = obj.GetString("name");
                row.certificate = obj.GetString("certificate");
                row.cert_picture = XStorage.ConvertUrlToStorage(obj.GetString("cert_picture"));
                if (string.IsNullOrEmpty(row.role))
                {
                    result.message = "默认角色未输入，请输入";
                }
                else if (string.IsNullOrEmpty(row.name))
                {
                    result.message = "机构名称未输入，请输入";
                }
                else
                {
                    try
                    {
                        if (db.Storageable<Models.Owner>(row).ExecuteCommand() > 0)
                        {
                            result.code = "200";
                            result.data = row.id.ToString();
                            result.success = true;
                            result.message = "提交成功，操作已保存";
                        }
                        else
                        {
                            result.code = "500";
                            result.message = "提交失败，数据未保存";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.message = ex.Message;
                    }
                }
                return OutputSerialize(result);
            });
        }



        /// <summary>
        /// 数据删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [WlniaoQueryParameter(Name = "id", Description = "记录ID", Required = true)]
        [ProducesResponseType<ApiResult<string>>(0)]
        public IActionResult remove()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var id = obj.GetInt32("id");
                var row = id <= 0 ? null : db.Queryable<Models.Owner>().Where(o => o.id == id).First();
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (row == null)
                {
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    if (db.Deleteable<Models.Owner>(row).ExecuteCommand() > 0)
                    {
                        result.code = "200";
                        result.data = id.ToString();
                        result.success = true;
                        result.message = "提交成功，数据已删除";
                    }
                    else
                    {
                        result.code = "500";
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
        [WlniaoQueryParameter(Name = "id", Description = "记录ID", Required = true)]
        [ProducesResponseType<ApiResult<Dto.Owner>>(0)]
        public IActionResult modify()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var id = obj.GetInt32("id");
                var row = id <= 0 ? null : db.Queryable<Models.Owner>().Where(o => o.id == id).First();
                var result = new ApiResult<Dto.Owner> { code = "0", tips = true };
                if (row == null)
                {
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    result.code = "200";
                    result.data = new Dto.Owner
                    {
                        id = row.id,
                        role = row.role,
                        name = row.name,
                        certificate = row.certificate,
                        cert_picture = XStorage.ConvertUrlToFullUrl(row.cert_picture)
                    };
                    result.success = true;
                    result.message = "查询成功，数据已返回";
                }
                return OutputSerialize(result);
            });
        }
    
    
    }
}