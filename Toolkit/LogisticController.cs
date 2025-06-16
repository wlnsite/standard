using System.Text;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult? Check(Func<String, Dictionary<String, Object>, IActionResult> func)
        {
            try
            {
                var time = cvt.ToLong(PostRequest("time"));
                var owner = PostRequest("owner");
                var action = PostRequest("action");
                var data = PostRequest("data");
                var sign = PostRequest("sign");
                if (string.IsNullOrEmpty(owner))
                {
                    return OutMessage("缺少参数“owner”，请输入接收机构", "113");
                }
                if (string.IsNullOrEmpty(action))
                {
                    return OutMessage("缺少参数“action”，请输入调用的操作名称", "113");
                }
                if (time < DateTools.GetUnix() - 7200)
                {
                    return OutMessage(time > 0 ? "请求已过有效期，请重新发起" : "缺少参数“time”，请指定请求时间", "113");
                }
                if (string.IsNullOrEmpty(data))
                {
                    return OutMessage("缺少参数“data”，请输入请求数据", "113");
                }
                if (string.IsNullOrEmpty(sign))
                {
                    return OutMessage("缺少参数“sign”，请对当前请求进行签名", "113");
                }
                if (!new Wlniao.Crypto.SM2(Settings.LogisticServerPub, String.Empty, KeyType.Generate, Wlniao.Crypto.SM2Mode.C1C3C2).VerifySign(UTF8Encoding.UTF8.GetBytes($"{time}\n{owner}\n{action}\n{data}"), Wlniao.Crypto.Helper.Decode(sign)))
                {
                    return OutMessage("参数“sign”无效，当前请求签名验证失败", "113");
                }
                var plainData = Encryptor.SM4DecryptECBFromHex(data, Settings.LogisticToken);
                if (string.IsNullOrEmpty(plainData))
                {
                    return OutMessage("数据解密失败，请重新发起", "403");
                }
                else
                {
                    var input = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(plainData);
                    return func?.Invoke(action, input ?? new Dictionary<string, object>());
                }
            }
            catch (Exception ex)
            {
                Wlniao.Log.Loger.Error(ex.Message);
                return OutMessage(ex.Message, "401");
            }
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
