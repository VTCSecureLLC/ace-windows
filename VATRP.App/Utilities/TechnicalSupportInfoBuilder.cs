using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class TechnicalSupportInfoBuilder
    {

        public static string GetStringForTechnicalSupprtString(bool verbose)
        {
            StringBuilder configString = new StringBuilder();
            if (App.CurrentAccount != null)
            {
                ACEConfig config = App.CurrentAccount.configuration;
                if (config == null)
                {
                    return "";
                }
                // Liz E. Note: Commenting out the items that are not yet being stored above. Uncomment and access correct information as needed
                //  (rather than printing items that are not yet in use stored)
                //        public int version { get; set; }
                configString.AppendLine("Configuration Version: " + config.version);
                //         public int expiration_time { get; set; }
                configString.AppendLine("Expiration Time: " + config.expiration_time); // this should be converted to date time, I am sure

                // not sure we want auth information printed in the technical spec sheet. name maybe, but not password.
                //       public string configuration_auth_password { get; set; }
                //       public int configuration_auth_expiration { get; set; }

                //       public int sip_registration_maximum_threshold { get; set; }
                //configString.AppendLine("SIP Registration Maximum Threshold: " + config.sip_registration_maximum_threshold);
                //       public List<string> sip_register_usernames { get; set; }
                configString.AppendLine("SIP Register Usernames: " + string.Join(", ", config.sip_register_usernames.ToArray()));
                //       public string sip_auth_username { get; set; }
                configString.AppendLine("Username: " + App.CurrentAccount.Username);
                //       public string sip_auth_password { get; set; }
                //configString.AppendLine("SIP Auth Password: " + sip_auth_password);
                //       public string sip_register_domain { get; set; }
                configString.AppendLine("Domain: " + App.CurrentAccount.ProxyHostname);
                //       public int sip_register_port { get; set; }
                configString.AppendLine("Port: " + App.CurrentAccount.ProxyPort);
                //       public string sip_register_transport { get; set; }
                configString.AppendLine("Transport: " + App.CurrentAccount.Transport);

                //       public bool enable_echo_cancellation { get; set; }
                configString.AppendLine("Enable Echo Cancellation: " + App.CurrentAccount.EchoCancel.ToString());
                //       public bool enable_video { get; set; }
                configString.AppendLine("Enable Video: " + App.CurrentAccount.EnableVideo.ToString());
                //       public bool enable_rtt { get; set; }
                bool enable_rtt = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.USE_RTT, true);
                configString.AppendLine("Enable RTT: " + enable_rtt.ToString());
                //       public bool enable_adaptive_rate { get; set; }
                configString.AppendLine("Enable Adaptive Rate: " + App.CurrentAccount.EnableAVPF.ToString()); // is this correct?
                //       public bool enable_stun { get; set; }
                configString.AppendLine("Enable STUN: " + App.CurrentAccount.EnableSTUN.ToString());
                //       public string stun_server { get; set; }
                configString.AppendLine("STUN Server: " + App.CurrentAccount.STUNAddress);
                //       public bool enable_ice { get; set; }
                configString.AppendLine("Enable ICE: " + App.CurrentAccount.EnableICE.ToString());

                //       public List<string> enabled_codecs { get; set; }
                //           configString.AppendLine("Enabled Codecs: " + string.Join(", ", enabled_codecs.ToArray()));
                //       public string bwLimit { get; set; }

                //       public int upload_bandwidth { get; set; }
                //       public int download_bandwidth { get; set; }
                //       public string logging { get; set; }
                //       public string sip_mwi_uri { get; set; }
                //       public string video_resolution_maximum { get; set; }

                //       public string sip_videomail_uri { get; set; }
                configString.AppendLine("Video Mail URI: " + App.CurrentAccount.VideoMailUri);


                //        public bool user_is_agent { get; set; } // do not include this yet - this is a POC feature at the moment
            }
            return configString.ToString();
        }

    }
}
