using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Model
{
    public class ACEConfig
    {
        public int version { get; set; }
        public int expiration_time { get; set; }

        public string configuration_auth_password { get; set; }
        public int configuration_auth_expiration { get; set; }
        
        public int sip_registration_maximum_threshold { get; set; }
        public List<string> sip_register_usernames { get; set; }
        public string sip_auth_username { get; set; }
        public string sip_auth_password { get; set; }
        public string sip_register_domain { get; set; }
        public int sip_register_port { get; set; }
        public string sip_register_transport { get; set; }
        
        public bool enable_echo_cancellation { get; set; }
        public bool enable_video { get; set; }
        public bool enable_rtt { get; set; }
        public bool enable_adaptive_rate { get; set; }
        public bool enable_stun { get; set; }
        public string stun_server { get; set; }
        public bool enable_ice { get; set; }
        
        public List<string> enabled_codecs { get; set; }
        public string bwLimit { get; set; }
        public int upload_bandwidth { get; set; }
        public int download_bandwidth { get; set; }
        public string logging { get; set; }
        public string sip_mwi_uri { get; set; }
        public string sip_videomail_uri { get; set; }
        public string video_resolution_maximum { get; set; }

        public ACEConfig()
        {

        }

        public void UpdateVATRPAccountFromACEConfig(VATRPAccount accountToUpdate)
        {
            accountToUpdate.EchoCancel = this.enable_echo_cancellation;
            accountToUpdate.EnableAVPF = this.enable_adaptive_rate;
            accountToUpdate.EnableICE = this.enable_ice;
            accountToUpdate.EnableSTUN = this.enable_stun;
            accountToUpdate.EnableVideo = this.enable_video;
            accountToUpdate.VideoAutomaticallyStart = this.enable_video;
            accountToUpdate.STUNAddress = this.stun_server;

            var stunServer = this.stun_server.Split(':');
            if (stunServer.Length > 1)
            {
                accountToUpdate.STUNAddress = stunServer[0];
                accountToUpdate.STUNPort = Convert.ToUInt16(stunServer[1]);
            }
            var username = this.sip_auth_username;
            if (!string.IsNullOrWhiteSpace(username))
            {
                accountToUpdate.RegistrationUser = username;
                accountToUpdate.Username = username;
            }

            var password = this.sip_auth_password;
            if (!string.IsNullOrWhiteSpace(password))
            {
                accountToUpdate.RegistrationPassword = password;
                accountToUpdate.Password = password;
            }
            var domain = this.sip_register_domain;
            if (!string.IsNullOrWhiteSpace(domain))
            {
                accountToUpdate.ProxyHostname = domain;
            }

            var port = this.sip_register_port;
            if (port > 0)
            {
                accountToUpdate.ProxyPort = (UInt16)port;
            }
            var transport = this.sip_register_transport;
            if (!string.IsNullOrWhiteSpace(transport))
            {
                accountToUpdate.Transport = transport;
            }

            // on successful login, we need to update the following in config: (list in progress)
            // this.enable_rtt;
            
            


            //implimment codec selection support

            /*
            newAccount.MuteMicrophone //missing
             *  public bool  enable_ice { get; set; } missing
             *         public bool  enable_rtt { get; set; }     //missing
             *         
             *         public int version { get; set; }// missing
             *          public string logging { get; set; } missing
             *          public bool AutoLogin { get; set; //missing
             *          sip_register_usernames //missing
            */

        }

    }
}
