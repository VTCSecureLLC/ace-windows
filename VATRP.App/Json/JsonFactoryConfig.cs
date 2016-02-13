using System;
using System.Net;
using HockeyApp;
using System.Threading.Tasks;
using VATRP.Core.Model;
using com.vtcsecure.ace.windows.Utilities;

namespace com.vtcsecure.ace.windows.Json
{
    // note - Config.cs moved into Core.Model.ACEConfig
    public class JsonFactoryConfig
    {

        public readonly static string defaultConfigString = "{\"version\":1,\"expiration_time\":280," +
            "\"configuration_auth_password\":null," +
            "\"configuration_auth_expiration\":-1," +
            "\"sip_registration_maximum_threshold\":10," +
            "\"sip_register_usernames\":[]," + 
            "\"sip_auth_username\":null," +
            "\"sip_auth_password\":null," +
            "\"sip_register_domain\":\"bc1.vatrp.net\"," +
            "\"sip_register_port\":25060," +
            "\"sip_register_transport\":\"tcp\"," +
            "\"enable_echo_cancellation\":true," +
            "\"enable_video\":true," +
            "\"enable_rtt\":true," +
            "\"enable_adaptive_rate\":true," +
            "\"enabled_codecs\":[\"H.264\",\"H.263\",\"VP8\",\"G.722\",\"G.711\"]," +
            "\"bwLimit\":\"high-fps\"," +
            "\"upload_bandwidth\":1500," +"\"download_bandwidth\":1500," +
            "\"enable_stun\":false," +
            "\"stun_server\":\"bc1.vatrp.net\"," +
            "\"enable_ice\":true," +
            "\"logging\":\"info\"," +
            "\"sip_mwi_uri\":null," +
            "\"sip_videomail_uri\":null," +"\"video_resolution_maximum\":\"cif\"}";
/* @"{
  'version': 1,
  'expiration_time': 3600,
  'configuration_auth_password': '""',
  'configuration_auth_expiration': -1,
  'sip_registration_maximum_threshold': 10,
  'sip_register_usernames': [],
  'sip_auth_username': '""',
  'sip_auth_password': '""',
  'sip_register_domain': 'bc1.vatrp.com',
  'sip_register_port': 5060,
  'sip_register_transport': 'tcp',
  'enable_echo_cancellation': 'true',
  'enable_video': 'true',
  'enable_rtt': 'true',
  'enable_adaptive_rate': 'true',
  'enabled_codecs': ['H.264','H.263','VP8','G.722','G.711'],
  'bwLimit': 'high-fps',
  'upload_bandwidth': 660,
  'download_bandwidth': 660,
  'enable_stun': 'false',
  'stun_server': '""',
  'enable_ice': 'false',
  'logging': 'info',
  'sip_mwi_uri': '""',
  'sip_videomail_uri': '""',
  'video_resolution_maximum': 'cif'
}";*/
        // returns the default json config values
        public static ACEConfig defaultConfig()
        {
            return JsonDeserializer.JsonDeserialize<ACEConfig>(defaultConfigString);
        }
        public static ACEConfig defaultConfig(ACEConfigStatusType configStatus)
        {
            ACEConfig config = (ACEConfig)JsonDeserializer.JsonDeserialize<ACEConfig>(defaultConfigString);
            config.configStatus = configStatus;
            return config;
        }

        // VATRP-1271: Liz E. - condense this down and use the JSON handler that we already have
        public static ACEConfig createConfigFromURL(string url, string userName, string password)
        {
            //ACEConfig defaultval = defaultConfig();
            IFeedbackThread feedbackThread = HockeyClient.Current.CreateFeedbackThread();
            string stacktrace = null;
            try
            {
                ACEConfig aceConfig = JsonWebRequest.MakeJsonWebRequestAuthenticated<ACEConfig>(url, userName, password);
                // aceConfig should never be null at this point - throwing JsonException in a failure event. If it is null, there is a problem - 
                //    but let's log it and handle it
                if (aceConfig == null)
                {
                    return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
                }
                aceConfig.configStatus = ACEConfigStatusType.LOGIN_SUCCEESSFUL;
                return aceConfig;
            }
            catch (JsonException ex)
            {
                // Once the codes that are sent back from the server are managed we can manage them here. For now, look
                //   for unauthorized in the message string as we know that this is returned currently.
                if ((ex.InnerException != null) && !string.IsNullOrEmpty(ex.InnerException.Message) &&
                    ex.InnerException.Message.ToLower().Contains("unauthorized"))
                {
                    return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.LOGIN_UNAUTHORIZED);
                }
                else 
                {
                    switch (ex.jsonExceptionType)
                    {
                        case JsonExceptionType.DESERIALIZATION_FAILED: return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNABLE_TO_PARSE);
                            break;
                        case JsonExceptionType.CONNECTION_FAILED: return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.CONNECTION_FAILED);
                            break;
                        default: 
                            return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                stacktrace = ex.StackTrace;
                return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
            }

            if ((feedbackThread != null) && !string.IsNullOrEmpty(stacktrace))
            {
                feedbackThread.PostFeedbackMessageAsync(url + "\n\n" + stacktrace, "noreply@ace.com", "json failed to deseralized", "Ace Logcat");
            }
            // note - this may be an invalid login - need to look for the correct unauthorized response assuming that there is one.
            //  Otherwise the app will use null response for now to assume unauthorized
            ACEConfig defaultConfig = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.SRV_RECORD_NOT_FOUND);
            return defaultConfig;
        }

    }
}

