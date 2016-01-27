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

        public static ACEConfig LookupConfig(string address, string userName, string password)
        {
            string srvLookupUrl = "_rueconfig._tls.";
            srvLookupUrl += address; // concat with selected domain

            string[] srvRecords = SRVLookup.GetSRVRecords(srvLookupUrl);
            
            if (srvRecords.Length > 0)
            {
                string record = srvRecords[0];
                if (!record.Equals(SRVLookup.NETWORK_SRV_ERROR_CONFIG_SERVICE))
                {
                    string requestUrl = "https://" + srvRecords[0] + "/config/v1/config.json";
                    ACEConfig config = JsonFactoryConfig.createConfigFromURL(requestUrl, userName, password);
                    return config;
                }
            }
            return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.SRV_RECORD_NOT_FOUND);
        }

    }
}
