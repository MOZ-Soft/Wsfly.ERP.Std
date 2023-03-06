using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.API
{
    /// <summary>
    /// 支付辅助
    /// </summary>
    [Serializable]
    public class CallMZAPI_Pay
    {
        /// <summary>
        /// 生成支付二维码
        /// </summary>
        public static Dictionary<string, string> BuildPayEWM(string orderNumber, decimal money, PayMethod method, ref string payNo)
        {
            //收款帐户编号
            string zhbh = AppGlobal.GetSysConfigReturnString("System_MZAPI_ZHBH");
            if (string.IsNullOrWhiteSpace(zhbh)) return null;

            //支付方式
            string paymethod = method.ToString();
            if (method == PayMethod.AlipayAndWeiXin) paymethod = "Alipay|WeiXin";

            //调用API支付接口参数
            Dictionary<string, object> dicParams = new Dictionary<string, object>()
            {
                { "ZHBH", zhbh },
                { "DH", orderNumber },
                { "JE", money.ToString("f") },
                { "FS", paymethod },
                { "BT", "支付：" + money.ToString("f") + "元" },
                { "HD", "" }
            };

            string apiName = "Pay";

            try
            {
                //获取支付二维码
                ReceiveJson receiveJson = CallMZAPI.Post(apiName, dicParams, Security.AES);
                if (receiveJson.success)
                {
                    //{"AlipayPayUrl":"","WeiXinPayUrl":""}
                    string data = receiveJson.data;
                    payNo = receiveJson.message;

                    Dictionary<string, string> dicData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                    string alipayEWMData = dicData["AlipayPayUrl"];
                    string weixinEWMData = dicData["WeiXinPayUrl"];

                    return dicData;
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "生成支付二维码异常");
            }

            return null;
        }
        /// <summary>
        /// 是否支付
        /// </summary>
        /// <returns></returns>
        public static bool IsPayed(string payNo)
        {
            if (string.IsNullOrWhiteSpace(payNo)) return false;

            try
            {
                Dictionary<string, object> dicParams = new Dictionary<string, object>()
                {
                    { "DH", payNo },
                };
                ReceiveJson receiveJson = CallMZAPI.Post("IsPayed", dicParams, Security.AES);
                if (receiveJson.success) return true;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "查询是否支付异常");
            }

            return false;
        }
        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="payNo"></param>
        /// <param name="je"></param>
        /// <returns></returns>
        public static bool Refund(string payNo, decimal je)
        {
            if (string.IsNullOrWhiteSpace(payNo)) return false;

            try
            {
                Dictionary<string, object> dicParams = new Dictionary<string, object>()
                {
                    { "DH", payNo },
                    { "JE", je },
                };
                ReceiveJson receiveJson = CallMZAPI.Post("ApplyRefund", dicParams, Security.AES);
                if (receiveJson.success) return true;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "申请退款异常");
            }

            return false;
        }
        /// <summary>
        /// 支付方式
        /// </summary>
        public enum PayMethod
        {
            Alipay,
            WeiXin,
            AlipayAndWeiXin
        }
    }
}
