using com.vtcsecure.ace.windows.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class ConfigLookup
    {

        public static ACEConfig LookupConfig_works(string address)
        {
            // test
            string testDomain = "_rueconfig._tls.acetest-registrar.vatrp.net";
            string[] test = SRVLookup.GetSRVRecords(testDomain);
            bool flag = false;
            return null;
        }
        public static ACEConfig LookupConfig(string address, string userName, string password)
        {
            string srvLookupUrl = "_rueconfig._tls.";
            srvLookupUrl += address;

            string[] srvRecords = SRVLookup.GetSRVRecords(srvLookupUrl);
            
            if (srvRecords.Length > 0)
            {
                string record = srvRecords[0];
                if (!record.Contains("does not exist"))
                {
                    string requestUrl = "https://" + srvRecords[0] + "/config/v1/config.json";
                    ACEConfig config = JsonFactoryConfig.createConfigFromURL(requestUrl, userName, password);
                    return config;
                }
            }
            return JsonFactoryConfig.defaultConfig();
        }

    }
}
