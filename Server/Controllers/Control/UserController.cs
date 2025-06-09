using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;

namespace Controllers.Control
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiController, ApiGroup("Control")]
    [Route(RoutePrefix + "[controller]/[action]")]
    public class UserController : BaseController
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
                    var exp = Expressionable.Create<Models.User>().And(o => true);
                    var key = obj.GetString("key");
                    if (key.IsNotNullAndEmpty())
                    {
                        exp = exp.And(o => o.mobile.Contains(key));
                    }
                    var query = db.Queryable<Models.User>().Where(exp.ToExpression());
                    var rows = query.OrderBy(o => o.time_create).ToList();
                    foreach (var row in rows)
                    {
                        result.data.Add(new Dto.Option
                        {
                            value = row.sid,
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
        [ProducesResponseType<ApiResult<Wlniao.Pager<Dto.User>>>(0)]
        [WlniaoQueryParameter(Name = "size", Description = "每页返回数量长度，默认：10", Required = false)]
        [WlniaoQueryParameter(Name = "page", Description = "当前页码：默认为1", Required = false)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        public IActionResult pager()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var key = obj.GetString("key");
                var page = obj.GetInt32("page");
                var size = obj.GetInt32("size", 10);
                var result = new ApiResult<Wlniao.Pager<Dto.User>>
                {
                    code = "-1",
                    tips = true,
                    data = new Wlniao.Pager<Dto.User> { size = size, page = page }
                };
                try
                {
                    var db = new SqlContext();
                    var exp = Expressionable.Create<Models.User>();
                    if (!string.IsNullOrEmpty(key))
                    {
                        exp = exp.And(o => o.name.Contains(key) || o.mobile.Contains(key));                    }

                    var query = db.Queryable<Models.User>().Where(exp.ToExpression());
                    var rows = query.OrderBy(o => o.time_create).Skip(result.data.skip).Take(result.data.size).ToList();
                    var userSids = rows.Select(o => o.sid).ToArray();
                    var userRows = db.Queryable<Models.User>().Where(o => userSids.Contains(o.sid)).ToList();
                    foreach (var row in rows)
                    {
                        var user = userRows.Where(o => o.sid == row.sid).FirstOrDefault();
                        result.data.rows.Add(new Dto.User
                        {
                            sid = row.sid,
                            name = user?.name,
                            mobile = user?.mobile
                        });
                    }
                    result.data.total = query.Count();
                    result.code = "200";
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
        public IActionResult submit()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var sid = obj.GetString("sid");
                var name = obj.GetString("name");
                var mobile = obj.GetString("mobile");
                var result = new ApiResult<string> { code = "-1", tips = true };
                if (!strUtil.IsMobile(mobile))
                {
                    return OutputMessage(result, "手机号码无效，请重新输入", "101");
                }
                var row = string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.User>().Where(o => o.sid == sid).First();
                if (row == null && string.IsNullOrEmpty(sid))
                {
                    //sid=
                    row = new Models.User { sid = sid, time_create = DateTools.GetUnix() };
                }
                if (row == null)
                {
                    result.code = "404";
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    row.mobile = mobile;
                    row.name = obj.GetString("name");
                    if (db.Storageable<Models.User>(row).ExecuteCommand() > 0)
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
        [WlniaoQueryParameter(Name = "sid", Description = "标识ID", Required = true)]
        [ProducesResponseType<ApiResult<Dto.User>>(0)]
        public IActionResult modify()
        {
            return CheckSession((xsession, ctx) =>
            {
                var db = new SqlContext();
                var obj = InputDeserialize();
                var sid = obj.GetString("sid");
                var row = string.IsNullOrEmpty(sid) ? null : db.Queryable<Models.User>().Where(o => o.sid == sid).First();
                var result = new ApiResult<Dto.User> { code = "-1", tips = true };
                if (row == null)
                {
                    result.code = "404";
                    result.message = "所选记录无效，请重新选择";
                }
                else
                {
                    result.code = "200";
                    result.data = new Dto.User
                    {
                        sid = row.sid,
                        name = row.name,
                        mobile = row.mobile
                    };
                    result.success = true;
                    result.message = "提交成功，操作已保存";
                }
                return OutputSerialize(result);
            });
        }


    }
}