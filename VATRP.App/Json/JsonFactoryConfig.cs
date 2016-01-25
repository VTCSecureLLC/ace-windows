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
        public readonly static string config =
 @"{
  'version': 1,
  'expiration_time': 3600,
  'configuration_auth_password': '',
  'configuration_auth_expiration': -1,
  'sip_registration_maximum_threshold': 10,
  'sip_register_usernames': [],
  'sip_auth_username': '',
  'sip_auth_password': '',
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
  'stun_server': '',
  'enable_ice': 'false',
  'logging': 'info',
  'sip_mwi_uri': '',
  'sip_videomail_uri': '',
  'video_resolution_maximum': 'cif'
}";
        // returns the default json config values
        public static ACEConfig defaultConfig()
        {
            return JsonDeserializer.JsonDeserialize<ACEConfig>(config);
        }

        // VATRP-1271: Liz E. - condense this down and use the JSON handler that we already have
        public static ACEConfig createConfigFromURL(string url, string userName, string password)
        {
            IFeedbackThread feedbackThread = HockeyClient.Current.CreateFeedbackThread();
            string stacktrace = null;
            try
            {
                ACEConfig aceConfig = JsonWebRequest.MakeJsonWebRequestAuthenticated<ACEConfig>(url, userName, password);
                return aceConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                stacktrace = ex.StackTrace;
            }

            if ((feedbackThread != null) && !string.IsNullOrEmpty(stacktrace))
            {
                feedbackThread.PostFeedbackMessageAsync(url + "\n\n" + stacktrace, "noreply@ace.com", "json failed to deseralized", "Ace Logcat");
            }
            // liz e- maybe this should return default config instead of null?
            return null;
        }

    }
}

