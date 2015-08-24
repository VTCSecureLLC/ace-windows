using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper
{
    public static class LinphoneAPI
    {
        public const string DllName = "liblinphone-7.dll";
        #region Constants

        public const int LC_SIP_TRANSPORT_RANDOM = -1; // Randomly chose a sip port for this transport
        public const int LC_SIP_TRANSPORT_DISABLED = 0; // Disable a sip transport

        #endregion

        #region Methods

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_logs(IntPtr file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_logs();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_new(IntPtr vtable, string config_path, string factory_config,
            IntPtr userdata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_destroy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_auth_info_new(string username, string userid, string passwd,
            string ha1, string realm, string domain);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_auth_info(IntPtr lc, IntPtr info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_identity(IntPtr obj, string identity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_server_addr(IntPtr cfg, string server_addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_enable_register(IntPtr obj, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_address_destroy(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_add_proxy_config(IntPtr lc, IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_default_proxy_config(IntPtr lc, IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_iterate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_default_call_parameters(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_video(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_early_media_sending(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite_with_params(IntPtr lc, string url, IntPtr param);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_destroy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_all_calls(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_default_proxy(IntPtr lc, ref IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_proxy_config_is_registered(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_edit(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_done(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_address_as_string(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_with_params(IntPtr lc, IntPtr call, IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_start_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_stop_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_record_file(IntPtr callparams, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_record_file(IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_sip_transports(IntPtr lc, IntPtr tr_config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_user_agent(IntPtr lc, string ua_name, string version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_play_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_record_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_use_files(IntPtr lc, bool yesno);

        #endregion

    }
}
