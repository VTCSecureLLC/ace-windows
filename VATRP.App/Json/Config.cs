using System.Collections.Generic;
namespace ConsoleApplication1
{
    class Config
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
        public string enable_echo_cancellation { get; set; }
        public string enable_video { get; set; }
        public string enable_rtt { get; set; }
        public string enable_adaptive_rate { get; set; }
        public List<string> enabled_codecs { get; set; }
        public string bwLimit { get; set; }
        public int upload_bandwidth { get; set; }
        public int download_bandwidth { get; set; }
        public string enable_stun { get; set; }
        public string stun_server { get; set; }
        public string enable_ice { get; set; }
        public string logging { get; set; }
        public string sip_mwi_uri { get; set; }
        public string sip_videomail_uri { get; set; }
        public string video_resolution_maximum { get; set; }

    }
}

