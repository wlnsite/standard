using Microsoft.AspNetCore.Mvc;
using Wlniao;
using Wlniao.XServer;

namespace Controllers.Control
{
    /// <summary>
    /// 基础控制器：管理面板系统
    /// </summary>
    public class BaseController : Wlniao.XCenter.EmiController
    {
        /// <summary>
        /// 路由前缀
        /// </summary>
        internal const string RoutePrefix = "control/service/";
        /// <summary>
        /// 
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        public BaseController()
        {
            base.AutoSetEmiCookie = false;
        }

        /// <summary>
        /// 管理端静态发布页【仅匹配非control/service/以外的目录】
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("control/{p1:regex(^(?!service))}/{p2?}/{p3?}/{p4?}")]
        public IActionResult appx(string? p1, string? p2, string? p3, string? p4)
        {
            return Content("empty page", "text/html", System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// OAuth授权处理
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(RoutePrefix + "authx")]
        public IActionResult authx()
        {
            return BuildTicket();
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(RoutePrefix + "upload")]
        public IActionResult upload()
        {
            return CheckSession((xsession, ctx) =>
            {
                var rlt = new ApiResult<String>();
                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    var filter = GetRequestNoSecurity("filter");
                    var file = Request.Form.Files[0];
                    var ext = file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower();
                    var key = "control";
                    if (string.IsNullOrEmpty(filter) || filter.Contains(ext))
                    {
                        var saveName = key + "/" + DateTools.GetNow().ToString("yyyyMM") + "/" + strUtil.CreateRndStrE(8) + ext;
                        if (XStorage.Upload(saveName, file.OpenReadStream()))
                        {
                            return Json(new
                            {
                                success = true,
                                state = "SUCCESS",
                                fileName = file.FileName,
                                size = file.Length.ToString(),
                                type = ext,
                                path = "/" + saveName,
                                url = XStorage.ConvertUrlToFullUrl(saveName)
                            });
                        }
                        else if (XStorage.XStorageType == "local")
                        {
                            rlt.message = "请先设置参数“UploadPath”";
                        }
                        else
                        {
                            rlt.message = "文件上传失败，请检查上传配置是否正确";
                        }
                    }
                    else
                    {
                        rlt.message = "只能上传" + filter + "格式的文件";
                    }
                }
                else
                {
                    rlt.message = "请先选择要上传的文件";
                }
                return Json(rlt);
            });
        }


    }

}