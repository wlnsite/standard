using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;

namespace Controllers.Control
{
    /// <summary>
    /// 合约记录管理
    /// </summary>
    [ApiController, ApiGroup("Control")]
    [Route(RoutePrefix + "[controller]/[action]")]
    public class ContractController : BaseController
    {
        /// <summary>
        /// 合约记录列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<ApiResult<Wlniao.Pager<Dto.Contract>>>(0)]
        [WlniaoQueryParameter(Name = "size", Description = "每页返回数量长度，默认：10", Required = false)]
        [WlniaoQueryParameter(Name = "page", Description = "当前页码：默认为1", Required = false)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        [WlniaoQueryParameter(Name = "owner", Description = "所属机构", Required = true)]
        public IActionResult list()
        {
            return CheckSession((xsession, ctx) =>
            {
                var obj = InputDeserialize();
                var min = PostRequest("min");
                var max = PostRequest("max");
                var page = obj.GetInt32("page");
                var size = obj.GetInt32("size", 10);
                var result = new ApiResult<Wlniao.Pager<Dto.Contract>>
                {
                    code = "-1",
                    tips = true,
                    data = new Wlniao.Pager<Dto.Contract> { size = size, page = page }
                };
                try
                {
                    var exp = Expressionable.Create<Models.Contract>().And(o => true);
                    var key = obj.GetString("key");
                    var type = obj.GetString("type");
                    var owner = obj.GetInt32("owner");
                    if (owner <= 0)
                    {
                        result.message = "未指定合约所属机构，请指定";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(min))
                        {
                            exp = exp.And(o => o.time_expire >= DateTools.GetUnix(DateTools.Convert(min)));
                        }
                        if (!string.IsNullOrEmpty(max))
                        {
                            exp = exp.And(o => o.time_expire <= DateTools.GetUnix(DateTools.Convert(max)));
                        }
                        if (type == "buyer")
                        {
                            exp = exp.And(o => o.buyer == owner);
                        }
                        else if (type == "supplier")
                        {
                            exp = exp.And(o => o.supplier == owner);
                        }
                        else
                        {
                            exp = exp.And(o => o.buyer == owner || o.supplier == owner);
                        }
                        if (key.IsNotNullAndEmpty())
                        {
                            exp = exp.And(o => o.id.Contains(key));
                        }
                        var db = new SqlContext();
                        var query = db.Queryable<Models.Contract>().Where(exp.ToExpression());
                        var rows = query.OrderByDescending(o => o.time_create).Skip(result.data.skip).Take(result.data.size).ToList();
                        var ownerKeys = rows.Where(o => o.buyer != owner).Select(o => o.buyer).Distinct().ToArray();
                        ownerKeys = ownerKeys.Concat(rows.Where(o => o.supplier != owner).Select(o => o.supplier).Distinct().ToArray()).ToArray();
                        var ownerRows = db.Queryable<Models.Owner>().Where(o => ownerKeys.Contains(o.id)).Select(o => new { o.id, o.name }).ToList();
                        foreach (var row in rows)
                        {
                            var dto = row.ConvertToDto();
                            dto.buyer_name = ownerRows.Where(o => o.id == dto.buyer).Select(o => o.name).FirstOrDefault();
                            dto.supplier_name = ownerRows.Where(o => o.id == dto.supplier).Select(o => o.name).FirstOrDefault();
                            result.data.rows.Add(dto);
                        }
                        result.data.total = query.Count();
                        result.code = "200";
                        result.message = result.data.total > 0 ? "查询完成，数据已返回" : "查询完成，暂无数据";
                        result.success = true;
                    }
                }
                catch (Exception ex)
                {
                    result.message = ex.Message;
                }
                return OutputSerialize(result);
            });
        }

    }
}