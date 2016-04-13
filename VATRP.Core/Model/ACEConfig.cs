using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Model
{
    public enum ACEConfigStatusType
    {
        LOGIN_SUCCEESSFUL,
        LOGIN_UNAUTHORIZED,
        SRV_RECORD_NOT_FOUND,
        UNABLE_TO_PARSE,
        CONNECTION_FAILED,
        UNKNOWN
    }
    public class ACEConfig
    {

        public ACEConfigStatusType configStatus { get; set; }

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
        public bool user_is_agent { get; set; }

        public ACEConfig()
        {
            configStatus = ACEConfigStatusType.UNKNOWN;
            sip_register_usernames = new List<string>();
            enabled_codecs = new List<string>();
        }

        public void NormalizeValues()
        {
            configuration_auth_password = NormalizeValue(configuration_auth_password);
            //        public List<string> sip_register_usernames { get; set; }
            sip_register_usernames = NormalizeValues(sip_register_usernames);
            //        public string sip_auth_username { get; set; }
            sip_auth_username = NormalizeValue(sip_auth_username);
            //        public string sip_auth_password { get; set; }
            sip_auth_password = NormalizeValue(sip_auth_password);
            //        public string sip_register_domain { get; set; }
            sip_register_domain = NormalizeValue(sip_register_domain);
            //        public string sip_register_transport { get; set; }
            sip_register_transport = NormalizeValue(sip_register_transport);

            //        public string stun_server { get; set; }
            stun_server = NormalizeValue(stun_server);
        
            //        public List<string> enabled_codecs { get; set; }
            enabled_codecs = NormalizeValues(enabled_codecs);
            //        public string bwLimit { get; set; }
            bwLimit = NormalizeValue(bwLimit);
            //        public string logging { get; set; }
            logging = NormalizeValue(logging);
            //        public string sip_mwi_uri { get; set; }
            sip_mwi_uri = NormalizeValue(sip_mwi_uri);
            //        public string sip_videomail_uri { get; set; }
            sip_videomail_uri = NormalizeValue(sip_videomail_uri);
            //        public string video_resolution_maximum { get; set; }
            video_resolution_maximum = NormalizeValue(video_resolution_maximum);
        }
        private List<string> NormalizeValues(List<string> values)
        {
            if (values == null)
            {
                values = new List<string>();
            }
            for (int i = 0; i < values.Count; i++)
            {
                values[i] = NormalizeValue(values[i]);
            }
            return values;
        }
        private string NormalizeValue(string value)
        {
            // these are spefcific cases to return ""
            if (string.IsNullOrEmpty(value) || value.Equals("\"") || value.Equals("\"\""))
            {
                return "";
            }
            else
            {
                // remove start and end quotes - but not all quotes in case there is data that allows quotes later.
                if (value.EndsWith("\""))
                {
                    value = value.Substring(0, value.Length - 1);
                }
                if (value.StartsWith("\""))
                {
                    if (value.Length == 1)
                        return "";
                    if (value.Length > 1)
                    {
                        return value.Substring(1, value.Length - 1);
                    }
                }
            }
            return value;
        }

        public void UpdateVATRPAccountFromACEConfig(VATRPAccount accountToUpdate)
        {
            // items not handled
            //        public int version { get; set; }
            //         public int expiration_time { get; set; }

            //       public string configuration_auth_password { get; set; }
            //       public int configuration_auth_expiration { get; set; }

            //       public int sip_registration_maximum_threshold { get; set; }
            //       public List<string> sip_register_usernames { get; set; }

            //       public List<string> enabled_codecs { get; set; }
            //       public string bwLimit { get; set; }
            //       public int upload_bandwidth { get; set; }
            //       public int download_bandwidth { get; set; }
            //       public string logging { get; set; }
            //       public string sip_mwi_uri { get; set; }

            //       public string video_resolution_maximum { get; set; }

            //       public bool enable_rtt { get; set; }  --> set in configuration service

            //        public bool user_is_agent { get; set; }
            //var trimChars = new[] { '\"' };
            accountToUpdate.configuration = this;

            //       public string sip_auth_username { get; set; }
            string username = "";
            if (!string.IsNullOrEmpty(this.sip_auth_username))
            {
                username = this.sip_auth_username;
                if (!string.IsNullOrWhiteSpace(username))
                {
                    accountToUpdate.RegistrationUser = username;
                    accountToUpdate.Username = username;
                }
            }

            //       public string sip_auth_password { get; set; }
            string password = "";
            if (!string.IsNullOrEmpty(this.sip_auth_password))
            {
                password = this.sip_auth_password;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    accountToUpdate.RegistrationPassword = password;
                    accountToUpdate.Password = password;
                }
            }
            //       public string sip_register_domain { get; set; }
            string domain = "";
            if (!string.IsNullOrEmpty(this.sip_register_domain))
            {
                domain = this.sip_register_domain;
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    accountToUpdate.ProxyHostname = domain;
                }
            }
            //       public int sip_register_port { get; set; }
            var port = this.sip_register_port;
            if (port > 0)
            {
                accountToUpdate.ProxyPort = (UInt16)port;
            }
            //       public string sip_register_transport { get; set; }
            string transport = "";
            if (!string.IsNullOrEmpty(transport))
            {
                transport = this.sip_register_transport;
                if (!string.IsNullOrWhiteSpace(transport))
                {
                    accountToUpdate.Transport = transport;
                }
            }

            //       public bool enable_echo_cancellation { get; set; }
            accountToUpdate.EchoCancel = this.enable_echo_cancellation;
            //       public bool enable_video { get; set; }
            accountToUpdate.EnableVideo = this.enable_video;
            accountToUpdate.VideoAutomaticallyStart = this.enable_video;

            //       public bool enable_adaptive_rate { get; set; }
            // TEMP - 3512, ignore accountToUpdate.EnableAdaptiveRate = this.enable_adaptive_rate;
            accountToUpdate.DownloadBandwidth = this.download_bandwidth;
            accountToUpdate.UploadBandwidth = this.upload_bandwidth;

            //       public bool enable_stun { get; set; }
            accountToUpdate.EnableSTUN = this.enable_stun;
            //       public string stun_server { get; set; }
            accountToUpdate.STUNAddress = this.stun_server ?? string.Empty;
            var stunServer = accountToUpdate.STUNAddress.Split(':');
            if (stunServer.Length > 1)
            {
                accountToUpdate.STUNAddress = stunServer[0];
                accountToUpdate.STUNPort = Convert.ToUInt16(stunServer[1]);
            }
            //       public bool enable_ice { get; set; }
            accountToUpdate.EnableICE = this.enable_ice;

            //       public string sip_videomail_uri { get; set; }
            accountToUpdate.VideoMailUri = (sip_videomail_uri ?? string.Empty);
            
            // on successful login, we need to update the following in config: (list in progress)
            // this.enable_rtt;

            accountToUpdate.UserNeedsAgentView = user_is_agent;
            // not working, commenting out to get the other items in
            /*
            // update available codecs
            accountToUpdate.AvailableAudioCodecsList.Clear();
            accountToUpdate.AvailableVideoCodecsList.Clear();
            bool pcmuAvailable = false;
            bool pcmaAvailable = false;
            foreach (VATRPCodec codec in accountToUpdate.AudioCodecsList)
            {
                if (codec.CodecName.ToLower().Equals("pcmu"))
                {
                    pcmuAvailable = true;
                }
                if (codec.CodecName.ToLower().Equals("pcma"))
                {
                    pcmaAvailable = true;
                }

                foreach (string enabled_codec in enabled_codecs)
                {
                    string codecName = enabled_codec.Replace(".", "");
                    if (!string.IsNullOrEmpty(codecName))
                    {
                        if (codecName.ToLower().Equals(codec.CodecName.ToLower()))
                        {
                            accountToUpdate.AvailableAudioCodecsList.Add(codec);
                        }
                    }
                }
            }
            // handle special cases
            if (pcmuAvailable && pcmaAvailable)
            {
                // add the g711 codec for display
                VATRPCodec newCodec = new VATRPCodec();
                newCodec.CodecName = "G711";
                newCodec.Description = "";
                newCodec.Channels = 0;
                newCodec.IPBitRate = 0;
                newCodec.IsUsable = false;
                newCodec.Priority = -1;
                newCodec.Rate = 0;
                if (accountToUpdate.AvailableAudioCodecsList.Count > 0)
                {
                    int index = 0;
                    foreach (VATRPCodec codec in accountToUpdate.AvailableAudioCodecsList)
                    {
                        index++;
                        if (codec.CodecName.ToLower().Equals("g722"))
                        {
                            accountToUpdate.AvailableAudioCodecsList.Insert(index, newCodec);
                        }
                    }
                }
                else
                {
                    accountToUpdate.AvailableAudioCodecsList.Add(newCodec);
                }
            }

            foreach (VATRPCodec codec in accountToUpdate.VideoCodecsList)
            {
                foreach (string enabled_codec in enabled_codecs)
                {
                    string codecName = enabled_codec.Replace(".", "");
                    if (codecName.ToLower().Equals(codec.CodecName.ToLower()))
                    {
                        accountToUpdate.AvailableVideoCodecsList.Add(codec);
                    }
                }
            }

            */
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
