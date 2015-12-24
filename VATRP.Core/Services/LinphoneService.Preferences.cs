﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VATRP.Core.Services
{
    public partial class LinphoneService
    {
        public class Preferences
        {
            public Preferences()
            {
                IsOutboundProxyOn = true;
                Expires = 3600;
            }

            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public string Realm { get; set; }

            public string ProxyHost { get; set; }

            public int ProxyPort { get; set; }

            public string UserAgent { get; set; }
            public string Version { get; set; }

            public string Transport { get; set; }
			
            public string STUNAddress { get; set; }
			
            public int STUNPort { get; set; }
			
            public bool EnableSTUN { get; set; }
			
            public bool EnableAVPF { get; set; }
			
            public LinphoneWrapper.Enums.LinphoneMediaEncryption MediaEncryption { get; set; }
			
            public string AuthID { get; set; }

            public bool IsOutboundProxyOn { get; set; }

            public int Expires { get; set; }
        }
    }
}
