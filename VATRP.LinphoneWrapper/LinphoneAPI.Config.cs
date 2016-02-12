using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        /**void linphone_proxy_config_set_dial_prefix (LinphoneProxyConfig* cfg, const char* prefix)
         * Sets a dialing prefix to be automatically prepended when inviting a number with linphone_core_invite(); This dialing prefix shall usually be the country code of the country where the user is living.
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_prefix(IntPtr cfg, string route);

        /** void linphone_proxy_config_set_dial_escape_plus	(LinphoneProxyConfig * 	cfg, bool_t val)
         * Sets whether liblinphone should replace "+" by international calling prefix in dialed numbers (passed to linphone_core_invite ).
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_escape_plus(IntPtr cfg, bool enable);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_config(IntPtr linphoneCore);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lp_config_set_int(IntPtr lpconfig, string section, string key, int value);
    }
}
