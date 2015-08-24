using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using log4net;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Services
{
    public partial class LinphoneService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LinphoneService));
        private readonly Preferences preferences;
        private readonly ServiceManagerBase manager;
        private IntPtr linphoneCore;
        private IntPtr callsDefaultParams;
        private IntPtr proxy_cfg;
        private IntPtr auth_info;
        private IntPtr t_configPtr;
        private IntPtr vtablePtr;
        Thread coreLoop;
        private bool isRunning = true;
        private string identity;
        private string server_addr;

        LinphoneCoreVTable vtable;
        LCSipTransports t_config;

        LinphoneCoreRegistrationStateChangedCb registration_state_changed;
        LinphoneCoreCallStateChangedCb call_state_changed;
        private LinphoneCoreGlobalStateChangedCb global_state_changed;
#endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreGlobalStateChangedCb(IntPtr lc, LinphoneGlobalState gstate, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreRegistrationStateChangedCb(IntPtr lc, IntPtr cfg, LinphoneRegistrationState cstate, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCallStateChangedCb(IntPtr lc, IntPtr call, LinphoneCallState cstate, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreNotifyPresenceReceivedCb(IntPtr lc, IntPtr lf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreNewSubscriptionRequestedCb(IntPtr lc, IntPtr lf, string url);

        #endregion

        #region Events
        public delegate void GlobalStateChangedDelegate(LinphoneGlobalState state);
        public event GlobalStateChangedDelegate GlobalStateChangedEvent;

        public delegate void RegistrationStateChangedDelegate(LinphoneRegistrationState state);
        public event RegistrationStateChangedDelegate RegistrationStateChangedEvent;

        public delegate void CallStateChangedDelegate(VATRPCall call);
        public event CallStateChangedDelegate CallStateChangedEvent;

        public delegate void ErrorDelegate(VATRPCall call, string message);
        public event ErrorDelegate ErrorEvent;
        #endregion

        public Preferences LinphoneConfig
        {
            get { return preferences; }
        } 

        public LinphoneService(ServiceManagerBase manager)
        {
            this.manager = manager;
            preferences = new Preferences();
        }

        public void Start(bool enableLogs)
        {
            try
            {
                if (enableLogs)
                    LinphoneAPI.linphone_core_enable_logs(IntPtr.Zero);
                else
                    LinphoneAPI.linphone_core_disable_logs();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex.ToString());
            }
        

        isRunning = true;
            registration_state_changed = new LinphoneCoreRegistrationStateChangedCb(OnRegistrationChanged);
            call_state_changed = new LinphoneCoreCallStateChangedCb(OnCallStateChanged);
            global_state_changed = new LinphoneCoreGlobalStateChangedCb(OnGlobalStateChanged);
            vtable = new LinphoneCoreVTable()
            {
                global_state_changed = Marshal.GetFunctionPointerForDelegate(global_state_changed),
                registration_state_changed = Marshal.GetFunctionPointerForDelegate(registration_state_changed),
                call_state_changed = Marshal.GetFunctionPointerForDelegate(call_state_changed),
                notify_presence_received = IntPtr.Zero,
                new_subscription_requested = IntPtr.Zero,
                auth_info_requested = IntPtr.Zero,
                call_log_updated = IntPtr.Zero,
                message_received = IntPtr.Zero,
                is_composing_received = IntPtr.Zero,
                dtmf_received = IntPtr.Zero,
                refer_received = IntPtr.Zero,
                call_encryption_changed = IntPtr.Zero,
                transfer_state_changed = IntPtr.Zero,
                buddy_info_updated = IntPtr.Zero,
                call_stats_updated = IntPtr.Zero,
                info_received = IntPtr.Zero,
                subscription_state_changed = IntPtr.Zero,
                notify_received = IntPtr.Zero,
                publish_state_changed = IntPtr.Zero,
                configuring_status = IntPtr.Zero,
                display_status = IntPtr.Zero,
                display_message = IntPtr.Zero,
                display_warning = IntPtr.Zero,
                display_url = IntPtr.Zero,
                show = IntPtr.Zero,
                text_received = IntPtr.Zero,
            };
            vtablePtr = Marshal.AllocHGlobal(Marshal.SizeOf(vtable));
            Marshal.StructureToPtr(vtable, vtablePtr, false);

            linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, null, null, IntPtr.Zero);

            coreLoop = new Thread(LinphoneMainLoop) {IsBackground = false};
            coreLoop.Start();

            t_config = new LCSipTransports()
            {
                udp_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                tcp_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                dtls_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                tls_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM
            };
            t_configPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_config));
            Marshal.StructureToPtr(t_config, t_configPtr, false);
            LinphoneAPI.linphone_core_set_sip_transports(linphoneCore, t_configPtr);

            LinphoneAPI.linphone_core_set_user_agent(linphoneCore, preferences.UserAgent, preferences.Version);

            callsDefaultParams = LinphoneAPI.linphone_core_create_default_call_parameters(linphoneCore);
            LinphoneAPI.linphone_call_params_enable_video(callsDefaultParams, false);
            LinphoneAPI.linphone_call_params_enable_early_media_sending(callsDefaultParams, true);

            identity = "sip:" + preferences.Username + "@" + preferences.ProxyHost;
            server_addr = "sip:" + preferences.ProxyHost + ":" + preferences.ProxyPort.ToString();

            auth_info = LinphoneAPI.linphone_auth_info_new(preferences.Username, null, preferences.Password, null, null, null);
            if (auth_info == IntPtr.Zero)
                LOG.Debug("failed to get auth info");
            LinphoneAPI.linphone_core_add_auth_info(linphoneCore, auth_info);

            proxy_cfg = LinphoneAPI.linphone_core_create_proxy_config(linphoneCore);
            LinphoneAPI.linphone_proxy_config_set_identity(proxy_cfg, identity);
            LinphoneAPI.linphone_proxy_config_set_server_addr(proxy_cfg, server_addr);
            LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, true);
            LinphoneAPI.linphone_core_add_proxy_config(linphoneCore, proxy_cfg);
            LinphoneAPI.linphone_core_set_default_proxy_config(linphoneCore, proxy_cfg);
        }

        public void Stop()
        {
            if (RegistrationStateChangedEvent != null)
                RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationProgress); // disconnecting

            LinphoneAPI.linphone_core_terminate_all_calls(linphoneCore);

            SetTimeout(delegate
            {
                LinphoneAPI.linphone_call_params_destroy(callsDefaultParams);

                if (LinphoneAPI.linphone_proxy_config_is_registered(proxy_cfg))
                {
                    LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
                    LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
                    LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
                }

                SetTimeout(delegate
                {
                    isRunning = false;
                }, 10000);

            }, 5000);

        }

        void LinphoneMainLoop()
        {
            LOG.Debug("Main loop started");
            while (isRunning)
            {
                LinphoneAPI.linphone_core_iterate(linphoneCore); // roll
                Thread.Sleep(50);
            }

            LinphoneAPI.linphone_core_destroy(linphoneCore);

            if (vtablePtr != IntPtr.Zero)
                Marshal.FreeHGlobal(vtablePtr);
            if (t_configPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(t_configPtr);

            if (RegistrationStateChangedEvent != null)
                RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared);

            registration_state_changed = null;
            call_state_changed = null;
            linphoneCore = callsDefaultParams = proxy_cfg = auth_info = t_configPtr = IntPtr.Zero;
            coreLoop = null;
            identity = null;
            server_addr = null;

            LOG.Debug("Main loop exited");
        }

        void OnRegistrationChanged (IntPtr lc, IntPtr cfg, LinphoneRegistrationState cstate, string message) 
		{
			if (linphoneCore == IntPtr.Zero || !isRunning) return;
            
            if (RegistrationStateChangedEvent != null)
				RegistrationStateChangedEvent (cstate);
		}

        void OnGlobalStateChanged(IntPtr lc, LinphoneGlobalState gstate, string message)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning) return;

            if (GlobalStateChangedEvent != null)
                GlobalStateChangedEvent(gstate);
        }
        private void OnCallStateChanged(IntPtr lc, IntPtr call, LinphoneCallState cstate, string message)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
                return;
            LOG.Info(string.Format("OnCallStateChanged: {0}", cstate));

        }

        void SetTimeout(Action callback, int miliseconds)
        {
            var timeout = new System.Timers.Timer();
            timeout.Interval = miliseconds;
            timeout.AutoReset = false;
            timeout.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                callback();
            };
            timeout.Start();
        }
    }
}
