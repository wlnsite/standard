using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Wlniao;
using Wlniao.Swagger;

namespace Controllers.Openapi
{
    /// <summary>
    /// 第一版开放接口服务
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]/[action]")]
    public class V1Controller : XCoreController
    {
        /// <summary>
        /// 下拉列表
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType<ApiResult<List<Dto.Option>>>(0)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        public IActionResult list()
        {
            return Content("v1");
        }

    }
}