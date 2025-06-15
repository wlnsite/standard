using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Wlniao;
using Wlniao.Caching;
using Wlniao.Crypto;
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
        #region
        /// <summary>
        /// 
        /// </summary>
        protected String ctxOwner = "";
        /// <summary>
        /// 
        /// </summary>
        protected String ctxToken = "";
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
        public IActionResult Check(Func<Dictionary<String, Object>, IActionResult> func)
        {
            try
            {
                var sn = PostRequest("sn");
                var time = cvt.ToLong(PostRequest("time"));
                var data = PostRequest("data");
                var sign = PostRequest("sign");
                var sm4key = PostRequest("sm4key");
                if (string.IsNullOrEmpty(sn))
                {
                    return OutMessage("缺少参数“sn”，请输入证书序列号", "113");
                }
                if (time < DateTools.GetUnix() + 7200)
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
                var cert = Models.CertKey.GetCache(sn);
                if (cert == null || cert.server_key == null || cert.server_key.Length == 0 || cert.public_key == null || cert.public_key.Length == 0)
                {
                    return OutMessage("参数“sn”无效，请确认证书序列号是否正确", "113");
                }
                else
                {
                    // 从证书序号中解析出当前请求来源的OwnerId
                    var tmpkey = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(cert.public_key), 4, 8).Replace("-", "");
                    ctxOwner = Wlniao.Encryptor.SM4DecryptECBFromHex(sn, tmpkey);
                }
                if (!new Wlniao.Crypto.SM2(cert.public_key, cert.private_key, Wlniao.Crypto.SM2Mode.C1C3C2).VerifySignWithRsAsn1(UTF8Encoding.UTF8.GetBytes($"{time}:{sn}:{data}"), Wlniao.Crypto.Helper.Decode(sign)))
                {
                    return OutMessage("参数“sign”无效，当前请求签名验证失败", "113");
                }
                if (!string.IsNullOrEmpty(sm4key))
                {
                    try
                    {
                        var bytes = new Wlniao.Crypto.SM2(null, cert.server_key, Wlniao.Crypto.SM2Mode.C1C3C2).Decrypt(Helper.Decode(sm4key));
                        ctxToken = UTF8Encoding.UTF8.GetString(bytes);
                    }
                    catch { }
                    if (string.IsNullOrEmpty(ctxToken))
                    {
                        return OutMessage("数据解密失败，请确认平台公钥是否正确", "403");
                    }
                }
                var plainData = string.IsNullOrEmpty(ctxToken) ? data: Encryptor.SM4DecryptECBFromHex(data, ctxToken);
                if (string.IsNullOrEmpty(plainData))
                {
                    return OutMessage("数据解密失败，请重新发起", "403");
                }
                else
                {
                    return func?.Invoke(Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, Object>>(plainData));
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
            dic.Add("node", result.node);
            dic.Add("code", string.IsNullOrEmpty(result.code) ? "1" : result.code);
            dic.Add("data", string.IsNullOrEmpty(ctxToken) ? txt : Encryptor.SM4EncryptECBToHex(txt, ctxToken));
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
        #endregion


        /// <summary>
        /// 交易路由节点推送
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType<ApiResult<List<Dto.Option>>>(0)]
        [WlniaoQueryParameter(Name = "key", Description = "查询筛选关键字", Required = false)]
        public IActionResult s2b_route_push()
        {
            return Check(obj =>
            {
                var time = obj.GetString("time");   //路由产生
                var state = obj.GetString("state");
                var bizno = obj.GetString("biz_no");
                var outno = obj.GetString("out_biz_no");
                var phone = obj.GetString("phone");
                var content = obj.GetString("content");
                var geocoord = obj.GetString("geocoord");
                var contract = obj.GetString("contract");
                if (!string.IsNullOrEmpty(contract))
                {
                    return OutMessage("缺少参数，单据所属合约未输入");
                }
                if (cvt.ToLong(time) < 0)
                {
                    return OutMessage(string.IsNullOrEmpty(time) ? "缺少参数，路由产生时间未输入" : "参数无效，请检查路由产生时间是否正确");
                }
                if (string.IsNullOrEmpty(state))
                {
                    return OutMessage("缺少参数，路由状态值未输入");
                }
                else if (new[] { "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "98", "99" }.Contains(state) == false)
                {
                    return OutMessage("参数无效，请检查路由状态值是否正确");
                }
                if (string.IsNullOrEmpty(bizno) && string.IsNullOrEmpty(outno))
                {
                    return OutMessage("缺少参数，承运方单号未输入");
                }

                return Content("debug");
            });
        }

    }
}