using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;

namespace Controllers.Control
{
    /// <summary>
    /// 操作员管理
    /// </summary>
    [ApiController, ApiGroup("Control")]
    [Route(RoutePrefix + "[controller]/[action]")]
    public class OperatorController : BaseController
    {
        /// <summary>
        /// 操作员列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<ApiResult<List<Dto.Option>>>(0)]
        [WlniaoQueryParameter(Name = "owner", Description = "操作员所属机构", Required = false)]
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
                    var owner = obj.GetInt32("owner");
                    if (owner <= 0)
                    {
                        result.message = "未指定操作员所属机构，请指定";
                    }
                    else
                    {
                        var db = new SqlContext();
                        var exp = Expressionable.Create<Models.Operator>().And(o => o.owner == owner && o.state >= 0);
                        var query = db.Queryable<Models.Operator>().Where(exp.ToExpression());
                        var rows = query.OrderBy(o => o.time_create).ToList();
                        var userSids = rows.Select(o => o.sid).ToArray();
                        var userRows = db.Queryable<Models.User>().Where(o => userSids.Contains(o.sid)).ToList();
                        foreach (var row in rows)
                        {
                            var user = userRows.Where(o => o.sid == row.sid).FirstOrDefault();
                            if (user == null)
                            {
                                continue;
                            }
                            result.data.Add(new Dto.Option
                            {
                                value = user.sid,
                                label = user.name
                            });
                        }
                        result.code = "0";
                        result.success = true;
                        result.message = "查询完成，数据已返回";
                    }
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
        [ProducesResponseType<ApiResult<Wlniao.Pager<Dto.Operator>>>(0)]
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
                var result = new ApiResult<Wlniao.Pager<Dto.Operator>>
                {
                    code = "-1",
                    tips = true,
                    data = new Wlniao.Pager<Dto.Operator> { size = size, page = page }
                };
                try
                {
                    if (owner <= 0)
                    {
                        result.message = "未指定操作员所属机构，请指定";
                    }
                    else
                    {
                        var db = new SqlContext();
                        var exp = Expressionable.Create<Models.Operator>().And(o => o.owner == owner && o.state >= 0);
                        var query = db.Queryable<Models.Operator>().Where(exp.ToExpression());
                        var rows = query.OrderBy(o => o.time_create).Skip(result.data.skip).Take(result.data.size).ToList();
                        var userSids = rows.Select(o => o.sid).ToArray();
                        var userRows = db.Queryable<Models.User>().Where(o => userSids.Contains(o.sid)).ToList();
                        foreach (var row in rows)
                        {
                            var user = userRows.Where(o => o.sid == row.sid).FirstOrDefault();
                            result.data.rows.Add(new Dto.Operator
                            {
                                sid = row.sid,
                                role = row.role,
                                state = row.state,
                                name = user?.name,
                                mobile = user?.mobile
                            });
                        }
                        result.data.total = query.Count();
                        result.code = "200";
                        result.message = "查询完成，数据已返回";
                    }
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
        [WlniaoBodyParameter(Type = typeof(Dto.Operator))]
        [ProducesResponseType<ApiResult<string>>(0)]
        public IActionResult submit()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var sid = obj.GetString("sid");
                var owner = obj.GetInt32("owner");
                var row = owner <= 0 || string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.Operator>().Where(o => o.sid == sid && o.owner == owner).First();
                if (row == null && owner > 0 && !string.IsNullOrEmpty(sid))
                {
                    row = new Models.Operator { sid = sid, owner = owner, time_create = DateTools.GetUnix() };
                }
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (row == null)
                {
                    result.code = "404";
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    row.state = obj.GetInt32("state");
                    if (db.Storageable<Models.Operator>(row).ExecuteCommand() > 0)
                    {
                        result.code = "200";
                        result.data = row.sid;
                        result.success = true;
                        result.message = "提交成功，操作已保存";
                    }
                    else
                    {
                        result.code = "500";
                        result.message = "提交失败，数据未保存";
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
        [WlniaoQueryParameter(Name = "key", Description = "标识ID", Required = true)]
        [ProducesResponseType<ApiResult<string>>(0)]
        public IActionResult remove()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var sid = obj.GetString("sid");
                var owner = obj.GetInt32("owner");
                var row = owner <= 0 || string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.Operator>().Where(o => o.sid == sid && o.owner == owner).First();
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (row == null)
                {
                    result.code = "404";
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    row.state = -1;
                    if (db.Storageable<Models.Operator>(row).ExecuteCommand() > 0)
                    {
                        result.code = "200";
                        result.data = row.sid;
                        result.success = true;
                        result.message = "提交成功，操作已保存";
                    }
                    else
                    {
                        result.code = "500";
                        result.message = "提交失败，数据未保存";
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
        [WlniaoQueryParameter(Name = "key", Description = "标识ID", Required = true)]
        [ProducesResponseType<ApiResult<Dto.Operator>>(0)]
        public IActionResult modify()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var sid = obj.GetString("sid");
                var owner = obj.GetInt32("owner");
                var row = owner <= 0 || string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.Operator>().Where(o => o.sid == sid && o.owner == owner).First();
                var user = string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.User>().Where(o => o.sid == sid).First();
                var result = new ApiResult<Dto.Operator> { code = "-1", tips = true };
                if (row == null || user == null || row.state < 0)
                {
                    result.code = "404";
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    result.code = "200";
                    result.data = new Dto.Operator
                    {
                        sid = row.sid,
                        role = row.role,
                        state = row.state,
                        name = user.name,
                        mobile = user.mobile
                    };
                    result.success = true;
                    result.message = "提交成功，操作已保存";
                }
                return OutputSerialize(result);
            });
        }


    }
}