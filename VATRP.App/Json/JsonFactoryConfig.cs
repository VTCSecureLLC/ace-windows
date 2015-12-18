﻿using Newtonsoft.Json;
using System;
using System.Net;
using HockeyApp;
namespace com.vtcsecure.ace.windows.Json
{
    class JsonFactoryConfig
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
        public static Config defaultConfig()
        {
            return JsonConvert.DeserializeObject<Config>(config);
        }


        public static Config createConfigFromURL(string url)
        {
            if (url != null)
            {

                string s;
                using (WebClient client = new WebClient())
                {
                    s = client.DownloadString(url);
                }
                try
                {
                    return JsonConvert.DeserializeObject<Config>(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return null;
                }

            }
            return null;
        }
    }

}
