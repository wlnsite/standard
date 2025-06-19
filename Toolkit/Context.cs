using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlniao;
using Wlniao.Crypto;
using Wlniao.Log;
using Wlniao.OpenApi;

namespace Logistic
{
    public class Context
    {
        #region 基础方法开始
        internal String LogisticHost { get; set; } = string.Empty;
        internal String LogisticCertSn { get; set; } = string.Empty;
        private byte[] LogisticServerPub { get; set; } = [];
        private byte[] LogisticPublicKey { get; set; } = [];
        private byte[] LogisticPrivateKey { get; set; } = [];

        /// <summary>
        /// 
        /// </summary>
        public Context()
        {
            try
            {
                this.LogisticHost = Settings.LogisticHost;
                this.LogisticCertSn = Settings.LogisticCertSn;
                this.LogisticServerPub = Wlniao.Crypto.Helper.Decode(Settings.LogisticServerPub);
                this.LogisticPublicKey = Wlniao.Crypto.Helper.Decode(Settings.LogisticPublicKey);
                this.LogisticPrivateKey = Wlniao.Crypto.Helper.Decode(Settings.LogisticPrivateKey);
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certSn"></param>
        /// <param name="serverPub"></param>
        /// <param name="certPublicKey"></param>
        /// <param name="privateKey"></param>
        /// <param name="host"></param>
        public Context(string certSn, string serverPub, string certPublicKey, string privateKey, string host = "")
        {
            try
            {
                this.LogisticHost = Settings.LogisticHost;
                this.LogisticCertSn = certSn;
                this.LogisticServerPub = Wlniao.Crypto.Helper.Decode(serverPub);
                this.LogisticPublicKey = Wlniao.Crypto.Helper.Decode(certPublicKey);
                this.LogisticPrivateKey = Wlniao.Crypto.Helper.Decode(privateKey);
                if (!string.IsNullOrEmpty(host))
                {
                    LogisticHost = host;
                }
            }
            catch { }
        }

        private static Context? instance = null;
        public static Context Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Context();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// 校验请求内容，并取出调用方法并解密业务数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ApiResult<object> Invoke(Dictionary<string, object> msg, Func<String, Dictionary<String, Object>, ApiResult<Object>> func)
        {
            var time = msg.GetInt64("time");
            var owner = msg.GetString("owner");
            var msgid = msg.GetString("msgid");
            var action = msg.GetString("action");
            var data = msg.GetString("data");
            var sign = msg.GetString("sign");
            var result = new ApiResult<object> { node = XCore.WebNode, message = "未知错误" };
            try
            {
                if (string.IsNullOrEmpty(owner))
                {
                    result.code = "501";
                    result.message = "缺少参数“owner”，请输入接收机构";
                }
                else if (string.IsNullOrEmpty(action))
                {
                    result.code = "501";
                    result.message = "缺少参数“action”，请输入调用的操作名称";
                }
                else if (time < DateTools.GetUnix() - 7200)
                {
                    result.code = "113";
                    result.message = time > 0 ? "请求已过有效期，请重新发起" : "缺少参数“time”，请指定请求时间";
                }
                else if (string.IsNullOrEmpty(data))
                {
                    result.code = "501";
                    result.message = "缺少参数“data”，请输入请求数据";
                }
                else if (string.IsNullOrEmpty(sign))
                {
                    result.code = "115";
                    result.message = "缺少参数“sign”，请对当前请求进行签名";
                }
                else if (!new Wlniao.Crypto.SM2(Settings.LogisticServerPub, String.Empty, KeyType.Generate, Wlniao.Crypto.SM2Mode.C1C3C2).VerifySign(UTF8Encoding.UTF8.GetBytes($"{time}\n{owner}\n{action}\n{data}"), Wlniao.Crypto.Helper.Decode(sign)))
                {
                    result.code = "115";
                    result.message = "参数“sign”无效，当前请求签名验证失败";
                }
                else
                {
                    var plainData = Encryptor.SM4DecryptECBFromHex(data, Settings.LogisticToken);
                    if (string.IsNullOrEmpty(plainData))
                    {
                        result.code = "403";
                        result.message = "数据解密失败，请重新发起";
                    }
                    else
                    {
                        var req = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(plainData);
                        var res = func.Invoke(action, req ?? new Dictionary<string, object>());
                        var logReq = Newtonsoft.Json.JsonConvert.SerializeObject(new { time, owner, action, data = req ?? new Dictionary<string, object>() });
                        var logRes = Newtonsoft.Json.JsonConvert.SerializeObject(new { res.success, res.message, res.code, data = res.data ?? new { } });
                        result = new ApiResult<object> { success = res.success, message = res.message, tips = res.tips, code = res.code, data = Wlniao.Encryptor.SM4EncryptECBToHex(Newtonsoft.Json.JsonConvert.SerializeObject(res.data ?? new { }), Logistic.Settings.LogisticToken) };
                        Loger.Topic("agent", $"msgid:{msgid}{Environment.NewLine} >>> {logReq}{Environment.NewLine} <<< {logRes}", Wlniao.Log.LogLevel.Debug, true);
                    }
                }
            }
            catch (Exception ex)
            {
                result.code = "500";
                result.message = ex.Message;
                Wlniao.Log.Loger.Error(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 调用API接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="msgid"></param>
        /// <param name="storeLog"></param>
        /// <returns></returns>
        public Wlniao.ApiResult<T> RequestApi<T>(String path, Object data, String? msgid = null, Boolean storeLog = true)
        {
            if (string.IsNullOrEmpty(msgid))
            {
                msgid = strUtil.CreateLongId();
            }
            var rlt = new Wlniao.ApiResult<T> { traceid = msgid };
            if (string.IsNullOrEmpty(LogisticCertSn))
            {
                rlt.message = "配置异常，参数“LogisticCertSn”未配置";
            }
            else if (LogisticServerPub == null || LogisticServerPub.Length < 20)
            {
                rlt.message = "配置异常，参数“LogisticServerPub”未配置";
            }
            else if (LogisticPrivateKey == null || LogisticPrivateKey.Length < 20)
            {
                rlt.message = "配置异常，参数“LogisticPrivateKey”未配置";
            }
            else
            {
                var token = string.IsNullOrEmpty(Settings.LogisticToken) ? strUtil.CreateRndStrE(16) : Settings.LogisticToken;
                var utime = DateTools.GetUnix();
                var plain = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var sm4data = Wlniao.Encryptor.SM4EncryptECBToHex(plain, token);
                var sm2key = new Wlniao.Crypto.SM2(new byte[0], LogisticPrivateKey, Wlniao.Crypto.SM2Mode.C1C3C2);
                var signtxt = Wlniao.Crypto.Helper.Encode(sm2key.Sign(UTF8Encoding.UTF8.GetBytes($"{utime}\n{path}\n{LogisticCertSn}\n{sm4data}")));
                var start = DateTime.Now;
                var resStr = "";
                var useTime = "";
                var apiServer = LogisticHost + path;
                try
                {
                    var body = new Dictionary<string, object> {
                        {"sn", LogisticCertSn },
                        {"time", utime },
                        {"data", sm4data },
                        {"sign", signtxt }
                    };
                    if (token != Settings.LogisticToken)
                    {
                        body.TryAdd("sm4key", Wlniao.Encryptor.SM2EncryptByPublicKey(UTF8Encoding.UTF8.GetBytes(token), LogisticServerPub));
                    }
                    var reqStr = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(reqStr)))
                    {
                        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = XCore.ServerCertificateCustomValidationCallback };
                        using (var client = new System.Net.Http.HttpClient(handler))
                        {
                            if (storeLog)
                            {
                                Loger.Topic("logistic", $"msgid:{msgid},{apiServer}{Environment.NewLine} >>> {reqStr}", Wlniao.Log.LogLevel.Information, false);
                            }
                            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, apiServer);
                            request.Content = new System.Net.Http.StreamContent(stream);
                            request.Content.Headers.Add("Content-Type", "application/json");
                            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Logistic.Toolkit");
                            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Wlniao-Trace", msgid);
                            var response = client.Send(request);
                            resStr = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Content.ReadAsStringAsync().Result : "";
                            if (response.Headers.Contains("X-Wlniao-Trace"))
                            {
                                rlt.traceid = response.Headers.GetValues("X-Wlniao-Trace").FirstOrDefault();
                            }
                            if (response.Headers.Contains("X-Wlniao-UseTime"))
                            {
                                useTime = response.Headers.GetValues("X-Wlniao-UseTime").FirstOrDefault();
                            }
                            else
                            {
                                useTime = DateTime.Now.Subtract(start).TotalMilliseconds.ToString("F2") + "ms";
                            }
                            if (storeLog)
                            {
                                Loger.Topic("logistic", $"msgid:{msgid},{apiServer}{Environment.NewLine} <<< {resStr}", Wlniao.Log.LogLevel.Information, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    useTime = DateTime.Now.Subtract(start).TotalMilliseconds.ToString("F2") + "ms";
                    if (storeLog)
                    {
                        Loger.Topic("logistic", $"msgid:{msgid},{apiServer}{Environment.NewLine}请确认接口访问是否正常： => {ex.Message}", Wlniao.Log.LogLevel.Error, true);
                    }
                }
                var logDebug = $"traceid:{rlt.traceid},{apiServer}[usetime:{useTime}]{Environment.NewLine} >>> {plain}";
                try
                {
                    var resObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Wlniao.ApiResult<String>>(resStr);
                    if (resObj != null)
                    {
                        rlt.node = resObj.node;
                        rlt.code = resObj.code;
                        rlt.message = resObj.message;
                        rlt.success = resObj.success;
                        if (!string.IsNullOrEmpty(resObj.data))
                        {
                            var json = Wlniao.Encryptor.SM4DecryptECBFromHex(resObj.data, token);
                            if (string.IsNullOrEmpty(json))
                            {
                                logDebug += "\n <<< RESPONSE EMPTY";
                                rlt.code = "116";
                                rlt.message = $"网络错误，返回内容无法解密[{XCore.WebNode}]";
                            }
                            else
                            {
                                try
                                {
                                    if (typeof(T) == typeof(string))
                                    {
                                        rlt.data = (T)System.Convert.ChangeType(json, typeof(T));
                                    }
                                    else
                                    {
                                        rlt.data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                                    }
                                }
                                catch { }
                                logDebug += "\n <<< " + Newtonsoft.Json.JsonConvert.SerializeObject(new { rlt.success, rlt.message, rlt.code, rlt.data });
                            }
                        }
                        else
                        {
                            logDebug += "\n <<< " + resStr;
                        }
                    }
                    else
                    {
                        rlt.code = "127";
                        rlt.message = $"网络错误，返回异常[{XCore.WebNode}]";
                    }
                }
                catch (System.Net.Http.HttpRequestException ex)
                {
                    logDebug += "\n <<< Exception: " + ex.Message;
                    rlt.code = "111";
                    rlt.message = $"网络错误，数据发送异常[{XCore.WebNode}]";
                }
                catch (System.Net.Http.HttpIOException ex)
                {
                    logDebug += "\n <<< Exception: " + ex.Message;
                    rlt.code = "121";
                    rlt.message = $"网络错误，数据读写异常[{XCore.WebNode}]";
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    logDebug += "\n <<< Exception: " + ex.Message;
                    rlt.code = "127";
                    rlt.message = $"网络错误，数据返回异常[{XCore.WebNode}]";
                }
                catch (Exception ex)
                {
                    logDebug += "\n <<< Exception: " + ex.Message;
                    rlt.message = $"Exception: {ex.Message}[{XCore.WebNode}]";
                }
                if (storeLog)
                {
                    Loger.Topic("logistic", logDebug, Wlniao.Log.LogLevel.Debug, true);
                }
            }
            return rlt;
        }
        #endregion 基础方法结束


        public ApiResult<Dictionary<string, object>> RoutePush(string contract, long time, string state, string bill_no, string out_bill_no, string phone, string content, string geocoord)
        {
            var result = RequestApi<Dictionary<string, object>>("/v1/bill_route_push", new Dictionary<string, object>
            {
                { "contract", contract },
                { "time", time },
                { "state", state },
                { "bill_no", bill_no },
                { "out_bill_no", out_bill_no },
                { "phone", phone },
                { "content", content },
                { "geocoord", geocoord }
            }, System.Guid.NewGuid().ToString(), true);

            return result;
        }
    
    
    }
}