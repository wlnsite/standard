using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Wlniao;
using Wlniao.Crypto;

namespace Logistic
{
    public class LogisticController:XCoreController
    {
        /// <summary>
        /// 
        /// </summary>
        protected ApiResult<Object> result = new ApiResult<Object> { node = XCore.WebNode, message = "未知错误" };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult? Invoke(Func<String, Dictionary<String, Object>, ApiResult<object>> func)
        {
            result = Context.Invoke(new Dictionary<string, object>
            {
                { "time", cvt.ToLong(PostRequest("time")) },
                { "owner", PostRequest("owner") },
                { "action", PostRequest("action") },
                { "data", PostRequest("data") },
                { "sign", PostRequest("sign") },
                { "wid", GetRequest("wid") }
            }, func);
            return OutDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public IActionResult OutDefault()
        {
            var txt = string.Empty;
            var dic = new Dictionary<string, object>();
            if (result.data == null)
            {
                txt = "";
            }
            else if (result.data is string)
            {
                txt = result.data.ToString();
            }
            else
            {
                txt = Newtonsoft.Json.JsonConvert.SerializeObject(result.data);
            }
            dic.Add("code", string.IsNullOrEmpty(result.code) ? "" : result.code);
            dic.Add("data", Encryptor.SM4EncryptECBToHex(txt, Settings.LogisticToken));
            dic.Add("success", result.success);
            dic.Add("message", result.message);
            if (result.tips)
            {
                dic.Add("tips", result.tips);
            }
            return Json(dic);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult OutMessage(string message, string code = "")
        {
            result.code = code;
            result.success = false;
            result.message = message;
            return OutDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult OutSuccess(Object obj)
        {
            result.code = "200";
            result.data = obj;
            result.success = true;
            result.message = "success";
            return OutDefault();
        }

    }
}
