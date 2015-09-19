using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VATRP.Core.Services
{
    public partial class LinphoneService
    {
        public class Preferences
        {
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
        }

    }
}
