using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logistic
{
    public class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticHost = Wlniao.Config.GetSetting("LogisticHost", "https://lmsapi.wlniao.net");
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticToken = Wlniao.Config.GetSetting("LogisticToken");
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticCertSn = Wlniao.Config.GetSetting("LogisticCertSn");
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticServerPub = Wlniao.Config.GetSetting("LogisticServerPub");
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticPublicKey = Wlniao.Config.GetSetting("LogisticPublicKey");
        /// <summary>
        /// 
        /// </summary>
        public static String LogisticPrivateKey = Wlniao.Config.GetSetting("LogisticPrivateKey");
    }
}
