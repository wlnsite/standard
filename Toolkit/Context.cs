using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlniao;
using Wlniao.Log;

namespace Logistic
{
    public class Context
    {
        #region 基础方法开始
        internal String LogisticHost = Wlniao.Config.GetSetting("LogisticHost", "https://logistic.wlniao.net");
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
                this.LogisticCertSn = Wlniao.Config.GetSetting("LogisticCertSn");
                this.LogisticServerPub = Wlniao.Crypto.Helper.Decode(Wlniao.Config.GetSetting("LogisticServerPub"));
                this.LogisticPublicKey = Wlniao.Crypto.Helper.Decode(Wlniao.Config.GetSetting("LogisticPublicKey"));
                this.LogisticPrivateKey = Wlniao.Crypto.Helper.Decode(Wlniao.Config.GetSetting("LogisticPrivateKey"));
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
            var rlt = new Wlniao.ApiResult<T>();
            if (string.IsNullOrEmpty(LogisticCertSn))
            {
                rlt.message = "配置异常，参数“LogisticCertSn”未配置";
            }
            else if (LogisticPublicKey == null || LogisticPublicKey.Length < 20)
            {
                rlt.message = "配置异常，参数“LogisticPublicKey”未配置";
            }
            else if (LogisticPrivateKey == null || LogisticPrivateKey.Length < 20)
            {
                rlt.message = "配置异常，参数“LogisticPrivateKey”未配置";
            }
            else
            {
                var token = strUtil.CreateRndStrE(16);
                var utime = DateTools.GetUnix();
                var plain = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var sm4data = Wlniao.Encryptor.SM4EncryptECBToHex(plain, token);
                var sm4key = Wlniao.Encryptor.SM2EncryptByPublicKey(UTF8Encoding.UTF8.GetBytes(token), LogisticServerPub);
                var sm2key = new Wlniao.Crypto.SM2(LogisticPublicKey, LogisticPrivateKey, Wlniao.Crypto.SM2Mode.C1C3C2);
                var signtxt = Wlniao.Crypto.Helper.Encode(sm2key.SignWithRsAsn1(UTF8Encoding.UTF8.GetBytes($"{utime}:{LogisticCertSn}:{sm4data}")));
                if (string.IsNullOrEmpty(msgid))
                {
                    msgid = strUtil.CreateLongId();
                }
                var start = DateTime.Now;
                var resStr = "";
                var useTime = "";
                var apiServer = LogisticHost + path;
                try
                {
                    var reqStr = Newtonsoft.Json.JsonConvert.SerializeObject(new { sn = LogisticCertSn, time = utime, data = sm4data, sign = signtxt, sm4key });
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
                var logDebug = $"msgid:{msgid},{apiServer}[usetime:{useTime}]{Environment.NewLine} >>> {plain}";
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
                                logDebug += "\n <<< " + json;
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


        public ApiResult<Dictionary<string, object>> RoutePush(string contract, long time, string state, string biz_no, string out_biz_no, string phone, string content, string geocoord)
        {
            var result = RequestApi<Dictionary<string, object>>("/v1/s2b_route_push", new Dictionary<string, object>
            {
                { "contract", contract },
                { "time", time },
                { "state", state },
                { "biz_no", biz_no },
                { "out_biz_no", out_biz_no },
                { "phone", phone },
                { "content", content },
                { "geocoord", geocoord }
            }, strUtil.CreateLongId(), true);

            return result;
        }
    }
}