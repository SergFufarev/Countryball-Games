using System.Collections.Generic;
using UnityEngine;

namespace SPSDigital.Metrica
{
    public class AppMetricaBridge : MonoBehaviour
    {
        /*
        public static IDictionary<string, object> SetParametrs(
            string ad_type = ""
            , string placement = ""
            , string result = ""
            , string connection = ""
            , string level_number = ""
            , string level_name = ""
            , string level_count = ""
            , string level_diff = ""
            , string level_loop = ""
            , string level_random = ""
            , string level_type = ""
            , string inapp_id = ""
            , string currency = ""
            , string price = ""
            , string inapp_type = ""
            , string show_reason = ""
            , string rate_result = ""
            , string game_mode = ""
            , string time = ""
            , string progress = ""
            , string _continue = ""
            , string step_name = ""
        )
        
        {
            IDictionary<string, object> parametrs = new Dictionary<string, object>();

            if(!string.IsNullOrEmpty(ad_type)) parametrs.Add("ad_type",ad_type);
            if(!string.IsNullOrEmpty(placement)) parametrs.Add("placement",placement);
            if(!string.IsNullOrEmpty(result)) parametrs.Add("result",result);
            if(!string.IsNullOrEmpty(connection)) parametrs.Add("connection",connection);
            if(!string.IsNullOrEmpty(level_number)) parametrs.Add("level_number",level_number);
            if(!string.IsNullOrEmpty(level_name)) parametrs.Add("level_name",level_name);
            if(!string.IsNullOrEmpty(level_count)) parametrs.Add("level_count",level_count);
            if(!string.IsNullOrEmpty(level_diff)) parametrs.Add("level_diff",level_diff);
            if(!string.IsNullOrEmpty(level_loop)) parametrs.Add("level_loop",level_loop);
            if(!string.IsNullOrEmpty(level_random)) parametrs.Add("level_random",level_random);
            if(!string.IsNullOrEmpty(level_type)) parametrs.Add("level_type",level_type);
            if(!string.IsNullOrEmpty(inapp_id)) parametrs.Add("inapp_id",inapp_id);
            if(!string.IsNullOrEmpty(currency)) parametrs.Add("currency",currency);
            if(!string.IsNullOrEmpty(price)) parametrs.Add("price",price);
            if(!string.IsNullOrEmpty(inapp_type)) parametrs.Add("inapp_type",inapp_type);
            if(!string.IsNullOrEmpty(show_reason)) parametrs.Add("show_reason",show_reason);
            if(!string.IsNullOrEmpty(rate_result)) parametrs.Add("rate_result",rate_result);
            if(!string.IsNullOrEmpty(game_mode)) parametrs.Add("game_mode",game_mode);
            if(!string.IsNullOrEmpty(time)) parametrs.Add("time",time);
            if(!string.IsNullOrEmpty(progress)) parametrs.Add("progress",progress);
            if(!string.IsNullOrEmpty(_continue)) parametrs.Add("continue",_continue);
            if(!string.IsNullOrEmpty(step_name)) parametrs.Add("step_name",step_name);

            return parametrs;
        }
        */

        #region REPORT EVENT

        public static void ReportEvent(string message, bool sendEventsBuffer = false)
        {
#if SPSDIGITAL_METRICA
            AppMetrica.Instance.ReportEvent(message);
            
            if(sendEventsBuffer) AppMetrica.Instance.SendEventsBuffer();

#endif
        }

        public static void ReportEvent(string message, IDictionary<string, object> parameters, bool sendEventsBuffer = false)
        {
#if SPSDIGITAL_METRICA
            AppMetrica.Instance.ReportEvent(message, parameters);
            
            if(sendEventsBuffer) AppMetrica.Instance.SendEventsBuffer();

#endif
        }

        public static void ReportEvent(string message, string json, bool sendEventsBuffer = false)
        {
#if SPSDIGITAL_METRICA
            AppMetrica.Instance.ReportEvent(message, json);
            
            if(sendEventsBuffer) AppMetrica.Instance.SendEventsBuffer();

#endif
        }

        #endregion
    }
}