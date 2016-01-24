using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using log4net;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Model.Commands;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Services
{
    public partial class LinphoneService : ILinphoneService
    {
		#region Members
		private static readonly ILog LOG = LogManager.GetLogger(typeof(LinphoneService));
		private readonly Preferences preferences;
		private readonly ServiceManagerBase manager;
		private IntPtr linphoneCore;
		private IntPtr proxy_cfg;
		private IntPtr auth_info;
		private IntPtr t_configPtr;
		private IntPtr vtablePtr;
		private string identity;
		Thread coreLoop;
		private string server_addr;
		private bool _isStarting;
		private bool _isStarted;
		private bool _isStopping;
		private bool _isStopped;
        private List<VATRPCodec> _audioCodecs = new List<VATRPCodec>();
        private List<VATRPCodec> _videoCodecs = new List<VATRPCodec>(); 
		private List<VATRPCall> callsList = new List<VATRPCall>();
        private Object callLock = new Object();
        private Object messagingLock = new Object();
		LinphoneCoreVTable vtable;
		LCSipTransports t_config;
        private static ManualResetEvent regulator = new ManualResetEvent(false);
        private static Queue<LinphoneCommand> commandQueue;

		private LinphoneCoreRegistrationStateChangedCb registration_state_changed;
		private LinphoneCoreCallStateChangedCb call_state_changed;
		private LinphoneCoreGlobalStateChangedCb global_state_changed;
		private LinphoneCoreNotifyReceivedCb notify_received;
	    private LinphoneCoreCallStatsUpdatedCb call_stats_updated;
        private LinphoneCoreIsComposingReceivedCb is_composing_received;
        private LinphoneCoreMessageReceivedCb message_received;
        private LinphoneChatMessageCbsMsgStateChangedCb message_status_changed;
        private LinphoneCoreCallLogUpdatedCb call_log_updated;
        private readonly string _chatLogPath;
        private readonly string _callLogPath;

        private LinphoneRegistrationState currentRegistrationState;
        private IntPtr _linphoneAudioCodecsList = IntPtr.Zero;
        private IntPtr _linphoneVideoCodecsList = IntPtr.Zero;
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

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreNotifyReceivedCb(IntPtr lc, IntPtr lev, string notified_event, IntPtr body);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreLogFuncCb(int level, string format, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCallStatsUpdatedCb(IntPtr lc, IntPtr call, IntPtr stats);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreIsComposingReceivedCb(IntPtr lc, IntPtr room);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreMessageReceivedCb(IntPtr lc, IntPtr room, IntPtr message);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneChatMessageCbsMsgStateChangedCb(IntPtr msg, LinphoneChatMessageState state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCallLogUpdatedCb(IntPtr lc, IntPtr newcl);
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

		public delegate void NotifyReceivedDelegate(string notify_event);
		public event NotifyReceivedDelegate NotifyReceivedEvent;

        public delegate void CallStatisticsChangedDelegate(VATRPCall call, LinphoneCallStats stats);
        public event CallStatisticsChangedDelegate CallStatisticsChangedEvent;

        public delegate void IsComposingReceivedDelegate(string remoteUser, IntPtr chatPtr, uint rttCode);
        public event IsComposingReceivedDelegate IsComposingReceivedEvent;

        public delegate void OnMessageReceivedDelegate(IntPtr chatPtr, IntPtr callChatPtr, string remote_party, VATRPChatMessage chatMessage);
        public event OnMessageReceivedDelegate OnChatMessageReceivedEvent;

        public delegate void OnMessageStatusChangedDelegate(IntPtr chatMsgPtr, LinphoneChatMessageState state);
        public event OnMessageStatusChangedDelegate OnChatMessageStatusChangedEvent;

        public delegate void OnCallLogUpdatedDelegate(IntPtr lc, IntPtr callPtr);
        public event OnCallLogUpdatedDelegate OnLinphoneCallLogUpdatedEvent;

        public delegate void MWIReceivedDelegate(MWIEventArgs args);
        public event MWIReceivedDelegate OnMWIReceivedEvent;

		#endregion

		#region Properties
		
		public Preferences LinphoneConfig
		{
			get { return preferences; }
		}

		public bool IsStarting
		{
			get
			{
				return _isStarting;
			}
		}

		public bool IsStarted
		{
			get
			{
				return _isStarted;
			}
		}

		public bool IsStopping
		{
			get
			{
				return _isStopping;
			}
		}

		public bool IsStopped
		{
			get
			{
				return _isStopped;
			}
		}

        public string ChatLogPath
        {
            get { return _chatLogPath; }
        }

        public string CallLogPath
        {
            get { return _callLogPath; }
        }

        public IntPtr LinphoneCore
        {
            get { return linphoneCore; }
        }

        public int GetActiveCallsCount
        {
            get
            {
                lock (callLock)
                {
                    return callsList.Count;
                }
            }
        }
        #endregion

		#region Methods
		public LinphoneService(ServiceManagerBase manager)
		{
            this.currentRegistrationState = LinphoneRegistrationState.LinphoneRegistrationNone;
			this.manager = manager;
            commandQueue = new Queue<LinphoneCommand>();
			preferences = new Preferences();
			_isStarting = false;
			_isStarted = false;
		    _chatLogPath = manager.BuildStoragePath("chathistory.db");
		    _callLogPath = manager.BuildStoragePath("callhistory.db");
		}

        public bool Start(bool enableLogs)
		{
			if (IsStarting)
				return false;

			if (IsStarted)
				return true;
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

			registration_state_changed = new LinphoneCoreRegistrationStateChangedCb(OnRegistrationChanged);
			call_state_changed = new LinphoneCoreCallStateChangedCb(OnCallStateChanged);
			global_state_changed = new LinphoneCoreGlobalStateChangedCb(OnGlobalStateChanged);
			notify_received = new LinphoneCoreNotifyReceivedCb(OnNotifyEventReceived);
            call_stats_updated = new LinphoneCoreCallStatsUpdatedCb(OnCallStatsUpdated);
		    is_composing_received = new LinphoneCoreIsComposingReceivedCb(OnIsComposingReceived);
            message_received = new LinphoneCoreMessageReceivedCb(OnMessageReceived);
            message_status_changed = new LinphoneChatMessageCbsMsgStateChangedCb(OnMessageStatusChanged);
            call_log_updated = new LinphoneCoreCallLogUpdatedCb(OnCallLogUpdated);
			vtable = new LinphoneCoreVTable()
			{
				global_state_changed = Marshal.GetFunctionPointerForDelegate(global_state_changed),
				registration_state_changed = Marshal.GetFunctionPointerForDelegate(registration_state_changed),
				call_state_changed = Marshal.GetFunctionPointerForDelegate(call_state_changed),
				notify_presence_received = IntPtr.Zero,
				new_subscription_requested = IntPtr.Zero,
				auth_info_requested = IntPtr.Zero,
                call_log_updated = Marshal.GetFunctionPointerForDelegate(call_log_updated),
                message_received = Marshal.GetFunctionPointerForDelegate(message_received),
				is_composing_received = Marshal.GetFunctionPointerForDelegate(is_composing_received),
				dtmf_received = IntPtr.Zero,
				refer_received = IntPtr.Zero,
				call_encryption_changed = IntPtr.Zero,
				transfer_state_changed = IntPtr.Zero,
				buddy_info_updated = IntPtr.Zero,
                call_stats_updated = Marshal.GetFunctionPointerForDelegate(call_stats_updated),
				info_received = IntPtr.Zero,
				subscription_state_changed = IntPtr.Zero,
				notify_received = Marshal.GetFunctionPointerForDelegate(notify_received),
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

            string configPath = manager.BuildStoragePath("linphonerc.cfg");

            linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, configPath, null, IntPtr.Zero);
			if (linphoneCore != IntPtr.Zero)
			{
                LinphoneAPI.linphone_core_set_log_level_mask(OrtpLogLevel.ORTP_TRACE);
                LinphoneAPI.libmsopenh264_init();
                // Liz E. - this is set in the account settings now
                //LinphoneAPI.linphone_core_set_video_preset(linphoneCore, "high-fps");
				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, false);
				LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);

			    LinphoneAPI.linphone_core_set_upload_bandwidth(linphoneCore, 660);
                LinphoneAPI.linphone_core_set_download_bandwidth(linphoneCore, 660);

                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
			    if (!string.IsNullOrEmpty(path))
			    {
			        var rootCAPath = Path.Combine(Path.GetDirectoryName(path), "rootca.pem");
			        LinphoneAPI.linphone_core_set_root_ca(linphoneCore, rootCAPath);
			    }
			    LinphoneAPI.linphone_core_verify_server_cn(linphoneCore, true);
                LinphoneAPI.linphone_core_verify_server_certificates(linphoneCore, true);

			    // load installed codecs
			    LoadAudioCodecs();
                LoadVideoCodecs();

                LinphoneAPI.linphone_core_set_chat_database_path(linphoneCore, _chatLogPath);
                LinphoneAPI.linphone_core_set_call_logs_database_path(linphoneCore, _callLogPath);

			    IntPtr defProxyCfg = LinphoneAPI.linphone_core_get_default_proxy_config(linphoneCore);
			    if (defProxyCfg != IntPtr.Zero)
			    {
			        proxy_cfg = defProxyCfg;
                    LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
                    LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
                    LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
			    }
                
                IntPtr coreConfig = LinphoneAPI.linphone_core_get_config(linphoneCore);
                if (coreConfig != IntPtr.Zero)
                {
                    LinphoneAPI.lp_config_set_int(coreConfig, "sip", "tcp_tls_keepalive", 1);
                    LinphoneAPI.lp_config_set_int(coreConfig, "sip", "keepalive_period", 90000);
                }

			    LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, false);

				coreLoop = new Thread(LinphoneMainLoop) {IsBackground = true};
				coreLoop.Start();

				_isStarted = true;
			}
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
			return _isStarted;
		}

        void LinphoneMainLoop()
        {
            LOG.Debug("Main loop started");
            bool isRunning = true;
            while (isRunning)
            {
                regulator.WaitOne(30); // fire each 30 msec
                try
                {
                    if (commandQueue.Count > 0)
                    {
                        LinphoneCommand command;
                        lock(commandQueue)
                        {
                            command = commandQueue.Dequeue();
                        }

                        if (command != null)
                        {
                            switch (command.Command)
                            {
                                case LinphoneCommandType.TerminateCall:
                                    var terminateCmd = command as TerminateCallCommand;

                                    if (terminateCmd != null)
                                        LinphoneAPI.linphone_core_terminate_call(LinphoneCore, terminateCmd.CallPtr);
                                    break;
                                case LinphoneCommandType.TerminateAllCalls:
                                    LinphoneAPI.linphone_core_terminate_all_calls(linphoneCore);
                                    break;
                                case LinphoneCommandType.AcceptCall:
                                    var acceptCmd = command as AcceptCallCommand;
                                    if (acceptCmd != null)
                                        LinphoneAPI.linphone_core_accept_call_with_params(linphoneCore,
                                            acceptCmd.CallPtr, acceptCmd.CallParamsPtr);
                                    break;
                                case LinphoneCommandType.DeclineCall:
                                    var declineCmd = command as DeclineCallCommand;
                                    if (declineCmd != null)
                                        LinphoneAPI.linphone_core_decline_call(linphoneCore, declineCmd.CallPtr,
                                            declineCmd.Reason);
                                    break;
                                case LinphoneCommandType.CreateCall:
                                {
                                    var createCmd = command as CreateCallCommand;
                                    if (createCmd != null)
                                    {
                                        // enable rtt
                                        LinphoneAPI.linphone_call_params_enable_realtime_text(createCmd.CallParamsPtr, createCmd.EnableRtt);
                                        MuteCall(createCmd.MuteMicrophone);
                                        MuteSpeaker(createCmd.MuteSpeaker);

                                        IntPtr callPtr = LinphoneAPI.linphone_core_invite_with_params(linphoneCore,
                                            createCmd.Callee, createCmd.CallParamsPtr);

                                        if (callPtr == IntPtr.Zero)
                                        {
                                            if (ErrorEvent != null)
                                                ErrorEvent(null, "Cannot create call to " + createCmd.Callee);
                                        }

                                        if (createCmd.CallParamsPtr != IntPtr.Zero)
                                            LinphoneAPI.linphone_call_params_destroy(createCmd.CallParamsPtr);
                                    }
                                }
                                    break;
                                case LinphoneCommandType.StopLinphone:
                                    isRunning = false;
                                    break;
                                case LinphoneCommandType.PauseCall:
                                    var pauseCmd = command as PauseCallCommand;
                                    if (pauseCmd != null)
                                    {
                                        LinphoneAPI.linphone_core_pause_call(linphoneCore, pauseCmd.CallPtr);
                                    }
                                    break;
                                case LinphoneCommandType.ResumeCall:
                                    var resumeCmd = command as ResumeCallCommand;
                                    if (resumeCmd != null)
                                    {
                                        LinphoneAPI.linphone_core_resume_call(linphoneCore, resumeCmd.CallPtr);
                                    }
                                    break;

                            }
                        }
                    }
                    LinphoneAPI.linphone_core_iterate(linphoneCore); // roll
                }
                catch (Exception ex)
                {
                    LOG.Debug("************ Linphone Main loop exception: " + ex.Message);
                }
            }

            LinphoneAPI.linphone_core_iterate(linphoneCore); // roll
            
            if (vtablePtr != IntPtr.Zero)
                Marshal.FreeHGlobal(vtablePtr);
            if (t_configPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(t_configPtr);

            if (RegistrationStateChangedEvent != null)
                RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared);

            
            //Marshal.FreeHGlobal(linphoneCore);
            LinphoneAPI.linphone_core_destroy(linphoneCore);
            registration_state_changed = null;
            call_state_changed = null;
            notify_received = null;
            message_received = null;
            message_status_changed = null;
            call_log_updated = null;
            linphoneCore = proxy_cfg = auth_info = t_configPtr = IntPtr.Zero;
            call_stats_updated = null;
            coreLoop = null;
            identity = null;
            server_addr = null;

            LOG.Debug("Main loop exited");
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
         
        }

        void SetTimeout(int miliseconds)
        {
            var timeout = new System.Timers.Timer {Interval = miliseconds, AutoReset = false};
            timeout.Elapsed += (sender, e) => DoUnregister();
            timeout.Start();
        }

        void SetTimeout(Action callback, int miliseconds)
        {
            var timeout = new System.Timers.Timer { Interval = miliseconds, AutoReset = false };
            timeout.Elapsed += (sender, e) => callback();
            timeout.Start();
        }

	    public void LockCalls()
	    {
            Monitor.Enter(callLock);  
	    }
        public void UnlockCalls()
        {
            Monitor.Exit(callLock);
        }

        public VATRPCall FindCall(IntPtr callPtr)
        {
            foreach (var call in callsList)
            {
                if (call.NativeCallPtr == callPtr)
                    return call;
            }
            return null;
        }

        public bool CanMakeVideoCall()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return false;
            }

            return true;
        }

        public void SendDtmfAsSipInfo(bool use_info)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return;
            }

            if (LinphoneAPI.linphone_core_get_use_info_for_dtmf(linphoneCore) != use_info)
            {
                LinphoneAPI.linphone_core_set_use_info_for_dtmf(linphoneCore, use_info);
                LOG.Debug(string.Format("{0} send dtmf as SIP info", use_info ? "Enable" : "Disable"));
            }
        }

        public void PlayDtmf(char dtmf, int duration)
        {
            if (linphoneCore == IntPtr.Zero)
                return;
            LinphoneAPI.linphone_core_play_dtmf(linphoneCore, dtmf, duration);
        }

        public void EnableAdaptiveRateControl(bool bEnable)
        {
            if (linphoneCore == IntPtr.Zero)
                return;

            var isCtrlEnabled = LinphoneAPI.linphone_core_adaptive_rate_control_enabled(linphoneCore);
            if (isCtrlEnabled != bEnable)
            {
                LinphoneAPI.linphone_core_enable_adaptive_rate_control(linphoneCore, bEnable);
                LOG.Debug(string.Format("{0} adaptive rate control", bEnable ? "Enable" : "Disable"));
            }
        }

		#endregion

		#region Registration
		public bool Register()
		{
            // make sure that we are not already registering
            //if ((currentRegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk) ||
            if (   (currentRegistrationState == LinphoneRegistrationState.LinphoneRegistrationProgress))
            {
                return false;
            }

			t_config = new LCSipTransports()
			{
				udp_port = preferences.Transport == "UDP" ? LinphoneAPI.LC_SIP_TRANSPORT_RANDOM : LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
                tcp_port = preferences.Transport == "TCP" ? LinphoneAPI.LC_SIP_TRANSPORT_RANDOM : LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
				dtls_port = preferences.Transport == "DTLS" ? LinphoneAPI.LC_SIP_TRANSPORT_RANDOM : LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
				tls_port = preferences.Transport == "TLS" ? LinphoneAPI.LC_SIP_TRANSPORT_RANDOM : LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
			};

			t_configPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_config));
			Marshal.StructureToPtr(t_config, t_configPtr, false);
			LinphoneAPI.linphone_core_set_sip_transports(linphoneCore, t_configPtr);
			LinphoneAPI.linphone_core_set_user_agent(linphoneCore, preferences.UserAgent, preferences.Version);

			if (string.IsNullOrEmpty(preferences.DisplayName))
			{
				identity = string.Format( "sip:{0}@{1}", preferences.Username, preferences.ProxyHost);
			}
			else
			{
				identity = string.Format("\"{0}\" <sip:{1}@{2}>", preferences.DisplayName, preferences.Username,
					preferences.ProxyHost);
			}

            int port = preferences.ProxyPort;
            if (preferences.Transport.ToLower().Equals("tls"))
            {
                port = 25061;
            }
			server_addr = string.Format("sip:{0}:{1};transport={2}", preferences.ProxyHost,
                port, preferences.Transport.ToLower());

            Debug.WriteLine(string.Format( "Register SIP account: {0} Server: {1} Password: {2}", identity, server_addr, preferences.Password));

            if (auth_info != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_remove_auth_info(linphoneCore, auth_info);
                auth_info = IntPtr.Zero;
            }

			auth_info = LinphoneAPI.linphone_auth_info_new(preferences.Username, string.IsNullOrEmpty(preferences.AuthID) ? null : preferences.AuthID, preferences.Password, null, null, null);
			if (auth_info == IntPtr.Zero)
				LOG.Debug("failed to get auth info");
			LinphoneAPI.linphone_core_add_auth_info(linphoneCore, auth_info);

		    LinphoneAPI.linphone_core_set_primary_contact(linphoneCore, identity);
            // remove all proxy entries from linphone configuration file

            if (proxy_cfg == IntPtr.Zero)
                proxy_cfg = LinphoneAPI.linphone_core_create_proxy_config(linphoneCore);
			/*set localParty with user name and domain*/
			LinphoneAPI.linphone_proxy_config_set_identity(proxy_cfg, identity);

		    LinphoneAPI.linphone_proxy_config_set_server_addr(proxy_cfg, server_addr);

		    LinphoneAPI.linphone_proxy_config_set_avpf_mode(proxy_cfg, (LinphoneAVPFMode)LinphoneAPI.linphone_core_get_avpf_mode(LinphoneCore));
            LinphoneAPI.linphone_proxy_config_set_avpf_rr_interval(proxy_cfg, 3);

		    string route = preferences.IsOutboundProxyOn ? server_addr : string.Empty;
            // use proxy as route if outbound_proxy is enabled
		    LinphoneAPI.linphone_proxy_config_set_route(proxy_cfg, route);
            LinphoneAPI.linphone_proxy_config_set_expires(proxy_cfg, preferences.Expires);
            LinphoneAPI.linphone_core_set_default_proxy_config(linphoneCore, proxy_cfg);
			LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, true);
			LinphoneAPI.linphone_core_add_proxy_config(linphoneCore, proxy_cfg);

            UpdateMediaEncryption();
			return true;

		}

		public bool Unregister(bool deferred)
		{
			if (RegistrationStateChangedEvent != null)
				RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationProgress); // disconnecting

            if (deferred)
			{
                SetTimeout(3000);
            }
            else
            {
                DoUnregister();
            }

			return true;
		}

	    private void DoUnregister()
	    {
            if (linphoneCore == IntPtr.Zero)
                return;

            IntPtr proxyCfg = IntPtr.Zero;
            LinphoneAPI.linphone_core_get_default_proxy(LinphoneCore, ref proxyCfg);
            if (proxyCfg != IntPtr.Zero && LinphoneAPI.linphone_proxy_config_is_registered(proxyCfg))
            {
                try
                {
                    LinphoneAPI.linphone_proxy_config_edit(proxyCfg);
                    LinphoneAPI.linphone_proxy_config_enable_register(proxyCfg, false);
                    LinphoneAPI.linphone_proxy_config_done(proxyCfg);
                    if (RegistrationStateChangedEvent != null)
                        RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared);
                }
                catch (Exception ex)
                {
                    LOG.Error("DoUnregister: " + ex.Message);
                }
                if (t_configPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(t_configPtr);
                    t_configPtr = IntPtr.Zero;
                }
            }
			ClearProxyInformation();
	    }

        public void ClearProxyInformation()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            // remove all proxy entries from linphone configuration file
            LinphoneAPI.linphone_core_clear_proxy_config(linphoneCore);
            // remove all authorization information
            LinphoneAPI.linphone_core_clear_all_auth_info(linphoneCore);
        }
        public void ClearAccountInformation()
        {
            ClearProxyInformation();
            // clear pushnotification preference
            // clear ice_preference
            // clear stun_preference

            LinphoneAPI.linphone_core_set_stun_server(linphoneCore, null);
            LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyNoFirewall);

            if (RegistrationStateChangedEvent != null)
                RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared);

        }
		#endregion

		#region Call
		public void MakeCall(string destination, bool videoOn, bool rttEnabled, bool muteMicrophone, bool muteSpeaker, bool enableVideo, string geolocation)
		{
		    if (callsList.Count > 0)
		    {
                LOG.Warn("Cannot make call. Cause - There is active call");
		        return;
		    }

		    if (string.IsNullOrEmpty(destination))
		    {
                LOG.Warn("Cannot make call. Cause - Destination is empty");
                return;
		    }

			if (linphoneCore == IntPtr.Zero) {
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot make when Linphone Core is not working.");
				return;
			}
            
            IntPtr callParams = LinphoneAPI.linphone_core_create_call_params(linphoneCore, IntPtr.Zero);
            LinphoneAPI.linphone_call_params_set_video_direction(callParams, LinphoneMediaDirection.LinphoneMediaDirectionSendRecv);
            LinphoneAPI.linphone_call_params_enable_video(callParams, enableVideo);
		    LinphoneAPI.linphone_call_params_set_audio_bandwidth_limit(callParams, 0);
            LinphoneAPI.linphone_call_params_enable_early_media_sending(callParams, true);

            if (geolocation.NotBlank())
		    {
                string un, host;
                int port;
		        VATRPCall.ParseSipAddress(destination, out un, out host, out port);

                if (un == "911")
                    LinphoneAPI.linphone_call_params_add_custom_header(callParams, "userLocation", geolocation);
		    }

            var cmd = new CreateCallCommand(callParams, destination, rttEnabled, muteMicrophone, muteSpeaker);

		    lock (commandQueue)
		    {
		        commandQueue.Enqueue(cmd);
		    }
		}

		public void AcceptCall(IntPtr callPtr, bool rttEnabled, bool muteMicrophone, bool muteSpeaker, bool enableVideo)
		{
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

		    lock (callLock)
		    {
		        VATRPCall call = FindCall(callPtr);

		        if (call == null)
		        {
		            LOG.Warn("Cannot accept call. Cause - Null call");
		            return;
		        }

                IntPtr callParamsPtr = LinphoneAPI.linphone_core_create_call_params(linphoneCore, callPtr);

		        IntPtr callerParams = LinphoneAPI.linphone_call_get_remote_params(call.NativeCallPtr);

		        if (callerParams != IntPtr.Zero)
		        {
		            bool remoteRttEnabled = LinphoneAPI.linphone_call_params_realtime_text_enabled(callerParams)!=0  &&
		                                    rttEnabled;

		            LinphoneAPI.linphone_call_params_enable_realtime_text(callParamsPtr, remoteRttEnabled);
                    LinphoneAPI.linphone_call_params_enable_video(callParamsPtr, enableVideo);
		        }
                MuteCall(muteMicrophone);
                MuteSpeaker(muteSpeaker);

		        var cmd = new AcceptCallCommand(call.NativeCallPtr, callParamsPtr);
		        //	LinphoneAPI.linphone_call_params_set_record_file(callsDefaultParams, null);
		        lock (commandQueue)
		        {
		            commandQueue.Enqueue(cmd);
		        }
		    }
		}

        public void DeclineCall(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot terminate calls when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("Cannot decline call. Cause - Null call");
                    return;
                }
                call.CallState = VATRPCallState.Closed;

                LOG.Info("Decline Call: " + callPtr);
                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
                    callsList.Count));
            }
            var cmd = new DeclineCallCommand(callPtr, LinphoneReason.LinphoneReasonDeclined);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }

        }

        public bool TerminateCall(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot terminate calls when Linphone Core is not working.");
                return false;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("TerminateCall No such call. " + callPtr);
                    return false;
                }

                // notify call state end
                //if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
                //    LinphoneAPI.linphone_call_stop_recording(call.NativeCallPtr);

                LOG.Info("Terminate Call " + callPtr);

                call.CallState = VATRPCallState.Closed;
                if (CallStateChangedEvent != null)
                    CallStateChangedEvent(call);
                callsList.Remove(call);
                LOG.Info(string.Format("Terminate Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
    callsList.Count));

            }

            var cmd = new TerminateCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
            return true;
        }

        public void ResumeCall(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot resume calls when Linphone Core is not working.");
                return;
            }

            var cmd = new ResumeCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }

        public void PauseCall(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot pause calls when Linphone Core is not working.");
                return;
            }

            var cmd = new PauseCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }
        public bool IsCallMuted()
		{
			if (linphoneCore == IntPtr.Zero)
				return false;
			return !LinphoneAPI.linphone_core_mic_enabled(linphoneCore);
		}
        public void MuteCall(bool muteCall)
        {
            LinphoneAPI.linphone_core_enable_mic(linphoneCore, !muteCall);
        }
		public void ToggleMute()
		{
			if (linphoneCore == IntPtr.Zero)
				return;

			LinphoneAPI.linphone_core_enable_mic(linphoneCore, !LinphoneAPI.linphone_core_mic_enabled(linphoneCore));
		}
        public bool IsSpeakerMuted()
        {
            if (linphoneCore == IntPtr.Zero)
                return false;
            float gainLevel = LinphoneAPI.linphone_core_get_playback_gain_db(linphoneCore);
            if (gainLevel < 0)
            {
                return true;
            }
            return false;
        }
        public void MuteSpeaker(bool muteSpeaker)
        {
            float gainLevel = 0f;
            if (muteSpeaker)
            {
                gainLevel = -1000.0f;
            }
            LinphoneAPI.linphone_core_set_playback_gain_db(linphoneCore, gainLevel);
        }

        public void ToggleVideo(bool enableVideo, IntPtr callPtr)
        {
			if (linphoneCore == IntPtr.Zero)
				return;

            if (callPtr == IntPtr.Zero)
            {
                LOG.Error("LinphoneService.ToggleVideo: Attempting to pause video but the call pointer is null. Returing without modifying call.");
                return;
            }

            // ToDo VATRP-842: Set static image instead of using default
            // LinphoneAPI.linphone_core_set_static_picture(linphoneCore, "Resources\\contacts.png");
            LinphoneAPI.linphone_call_enable_camera(callPtr, enableVideo);
        }

        public void SendDtmf(VATRPCall call, char dtmf)
        {
            if (call == null)
            {
                LOG.Warn("Cannot terminate call. Cause - Null call");
                return;
            }

            if ( LinphoneAPI.linphone_call_send_dtmf(call.NativeCallPtr, dtmf) != 0 )
                LOG.Error(string.Format( "Can't send dtmf {0}. Call {1}", dtmf, call.NativeCallPtr));
        }

		#endregion

        #region Messaging

        public bool IsRttEnabled(IntPtr callPtr)
        {
            if (callPtr == IntPtr.Zero)
                return false;

            IntPtr callerParams = LinphoneAPI.linphone_call_get_remote_params(callPtr);

            return callerParams != IntPtr.Zero && LinphoneAPI.linphone_call_params_realtime_text_enabled(callerParams) != 0;
        }

        public void AcceptRTTProposition(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("Cannot accept call. Cause - Null call");
                    return;
                }

                IntPtr paramsCopy =
                    LinphoneAPI.linphone_call_params_copy(
                        LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr));
                LinphoneAPI.linphone_call_params_enable_realtime_text(paramsCopy, true);
                LinphoneAPI.linphone_core_accept_call_update(linphoneCore, call.NativeCallPtr, paramsCopy);
            }
        }

        public void SendRTTProposition(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("Cannot accept call. Cause - Null call");
                    return;
                }

                IntPtr paramsCopy =
                    LinphoneAPI.linphone_call_params_copy(
                        LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr));
                LinphoneAPI.linphone_call_params_enable_realtime_text(paramsCopy, true);
                LinphoneAPI.linphone_core_update_call(linphoneCore, call.NativeCallPtr, paramsCopy);
            }
        }

        public bool SendChar(uint charCode, IntPtr callPtr, ref IntPtr chatRoomPtr, ref IntPtr chatMsgPtr)
        {
            bool retVal = false;

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);
                if (call == null)
                    return false;
                chatRoomPtr = LinphoneAPI.linphone_call_get_chat_room(callPtr);

                /*create a chat room associated to this call*/
                if (chatRoomPtr != IntPtr.Zero)
                {
                    if (chatMsgPtr == IntPtr.Zero)
                    {
                        chatMsgPtr = LinphoneAPI.linphone_chat_room_create_message(chatRoomPtr, "");
                    }
                }

                if (chatMsgPtr != IntPtr.Zero)
                {
                    int retCode = 1;
                    if (charCode == '\r' || charCode == '\n')
                    {
                        OnMessageStatusChanged(chatMsgPtr,
                                LinphoneChatMessageState.LinphoneChatMessageStateDelivered);
                        if (chatMsgPtr != IntPtr.Zero)
                        {
                            LinphoneAPI.linphone_chat_room_send_chat_message(chatRoomPtr, chatMsgPtr); /*sending message*/
                        }
                        chatMsgPtr = IntPtr.Zero;
                    }
                    else
                    {
                        retCode = LinphoneAPI.linphone_chat_message_put_char(chatMsgPtr, charCode);
                    }
                    retVal = (retCode == 0);
                }
            }
            return retVal;
        }

        public bool SendChatMessage(VATRPChat chat, string message, ref IntPtr msgPtr)
        {
            if (chat == null)
                return false;

            lock (messagingLock)
            {
                IntPtr chatPtr = LinphoneAPI.linphone_core_get_chat_room_from_uri(linphoneCore, chat.Contact.ID);
                chat.NativePtr = chatPtr;
                
                msgPtr = LinphoneAPI.linphone_chat_room_create_message(chat.NativePtr, message);
                if (msgPtr != IntPtr.Zero)
                {
                    IntPtr callbacks = LinphoneAPI.linphone_chat_message_get_callbacks(msgPtr);

                    LinphoneAPI.linphone_chat_message_cbs_set_msg_state_changed(callbacks, Marshal.GetFunctionPointerForDelegate(message_status_changed));
                    LinphoneAPI.linphone_chat_room_send_chat_message(chat.NativePtr, msgPtr); /*sending message*/
                }
            }
            return true;
        }

        public LinphoneChatMessageState GetMessageStatus(IntPtr messagePtr)
        {
            return LinphoneAPI.linphone_chat_message_get_state(messagePtr);
        }

        public void MarkChatAsRead(IntPtr cr)
        {
            lock (messagingLock)
            {
                if (cr != IntPtr.Zero)
                    LinphoneAPI.linphone_chat_room_mark_as_read(cr);
            }
        }

        #endregion

        #region Video

        public bool IsCameraEnabled(IntPtr callPtr)
        {
            if (callPtr == IntPtr.Zero)
                return false;

            return LinphoneAPI.linphone_call_camera_enabled(callPtr);
        }

        public void EnableVideo(bool enable, bool automaticallyInitiate, bool automaticallyAccept)
		{
            if ((linphoneCore == null) || (linphoneCore == IntPtr.Zero))
            {
                return;
            }
            // if current account exists and we are enabling video, intialize initiate and accept vars to account. Otherwise go with previous
            //   implementation - based on enable.
            bool autoInitiate = enable;
            bool autoAccept = enable;
            if (enable) // if we are not enabling video, do not allow autoInitiate and auto Accept to be true.
            {
                autoInitiate = automaticallyInitiate;
                autoAccept = automaticallyAccept;
            }

			var t_videoPolicy = new LinphoneVideoPolicy()
			{
				automatically_initiate = autoInitiate,
				automatically_accept = autoAccept
			};

			var t_videoPolicyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoPolicy));
            Marshal.StructureToPtr(t_videoPolicy, t_videoPolicyPtr, false);

            LinphoneVideoPolicy new_videoPolicy = (LinphoneVideoPolicy)Marshal.PtrToStructure(t_videoPolicyPtr, typeof(LinphoneVideoPolicy));
            Console.WriteLine("before calling core: new_videoPolicy values: new_videoPolicy.automatically_accept=" + new_videoPolicy.automatically_accept.ToString() + " new_videoPolicy.automatically_initiate=" + new_videoPolicy.automatically_initiate);

            if (t_videoPolicyPtr != IntPtr.Zero)
			{

				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, enable);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, enable);
				LinphoneAPI.linphone_core_set_video_policy(linphoneCore, t_videoPolicyPtr);
                Console.WriteLine("linphone_core_set_video_policy sent: t_videoPolicy values: t_videoPolicy.automatically_accept=" + t_videoPolicy.automatically_accept.ToString() + " t_videoPolicy.automatically_initiate=" + t_videoPolicy.automatically_initiate);
                Marshal.FreeHGlobal(t_videoPolicyPtr);
			}

            // here - if I call linphone_core_get_video_policy it returns a policy with automatically_accept = false, automatically_initiate = true
            IntPtr testPolicyPtr = LinphoneAPI.linphone_core_get_video_policy(linphoneCore);
            t_videoPolicy = (LinphoneVideoPolicy)Marshal.PtrToStructure(testPolicyPtr, typeof(LinphoneVideoPolicy));

            Console.WriteLine("linphone_core_get_video_policy returns: t_videoPolicy values: t_videoPolicy.automatically_accept=" + t_videoPolicy.automatically_accept.ToString() + " t_videoPolicy.automatically_initiate=" + t_videoPolicy.automatically_initiate);
        }

        // Liz E: needed for unified settings
        public bool IsEchoCancellationEnabled()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return false;
            }
            
            bool isEchoCancellationEnabled = LinphoneAPI.linphone_core_echo_cancellation_enabled(linphoneCore);

            return isEchoCancellationEnabled;
        }

        // Liz E: needed for unified settings
        public void EnableEchoCancellation(bool enable)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            LinphoneAPI.linphone_core_enable_echo_cancellation(linphoneCore, enable);
        }

        // Liz E: needed for unified settings
        public bool IsSelfViewEnabled()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return false;
            }

            bool isSelfViewEnabled = LinphoneAPI.linphone_core_self_view_enabled(linphoneCore);

            return isSelfViewEnabled;
        }

        // Liz E: needed for unified settings
        public void EnableSelfView(bool enable)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            LinphoneAPI.linphone_core_enable_self_view(linphoneCore, enable);
        }

		public void SwitchSelfVideo()
		{
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

			bool isSelfViewEnabled = LinphoneAPI.linphone_core_self_view_enabled(linphoneCore);
			LinphoneAPI.linphone_core_enable_self_view(linphoneCore, !isSelfViewEnabled);
		}

		public void SetVideoPreviewWindowHandle(IntPtr hWnd, bool reset = false)
		{
		    if (linphoneCore == IntPtr.Zero)
		        return;
			LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, !reset);
		    if (reset)
		    {
		        LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);
		    }
		    else
		    {
		        LinphoneAPI.linphone_core_use_preview_window(linphoneCore, true); // use preview in separated window
		        LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, hWnd.ToInt64());
		    }
		}
		
		public void SetPreviewVideoSize(MSVideoSize w, MSVideoSize h)
		{
			var t_videoSize = new MSVideoSizeDef()
			{
				height = Convert.ToInt32(h),
				width = Convert.ToInt32(w)
			};

			var t_videoSizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoSize));
			if (t_videoSizePtr != IntPtr.Zero)
			{
				LinphoneAPI.linphone_core_set_preview_video_size(linphoneCore, t_videoSizePtr);
				Marshal.FreeHGlobal(t_videoSizePtr);
			}
		}

		public void SetVideoCallWindowHandle(IntPtr hWnd, bool reset = false)
		{
			if (reset)
			{
				LinphoneAPI.linphone_core_set_native_video_window_id(linphoneCore, -1);
			}
			else
			{
				LinphoneAPI.linphone_core_set_native_video_window_id(linphoneCore, hWnd.ToInt64());
			}
		}

		public bool IsVideoEnabled(VATRPCall call)
		{
			var linphoneCallParams = LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr);
            var videoCodecName = string.Empty;
            if (linphoneCallParams != IntPtr.Zero)
                videoCodecName = GetUsedVideoCodec(linphoneCallParams);
			return !string.IsNullOrEmpty(videoCodecName);
		}

        public void UpdateMediaSettings(VATRPAccount account)
	    {
            if (linphoneCore == IntPtr.Zero) return;

	        if (account == null)
	        {
                LOG.Error("Account is null");
	            return;
	        }

            MuteCall(account.MuteMicrophone);
            MuteSpeaker(account.MuteSpeaker);

            EnableEchoCancellation(account.EchoCancel);

            EnableSelfView(account.ShowSelfView);
            // Liz E. - note: get_video_preset is not available in liphoneAPI. Null is an accepted value 
            //    for Linphone API as default.
            LOG.Info("Set preferred video size by name: " + account.VideoPreset);
            LinphoneAPI.linphone_core_set_video_preset(linphoneCore, account.VideoPreset);

            IntPtr namePtr = LinphoneAPI.linphone_core_get_preferred_video_size_name(linphoneCore);
            if (namePtr != IntPtr.Zero)
            {
                string name = Marshal.PtrToStringAnsi(namePtr);
                if (!string.IsNullOrWhiteSpace(account.PreferredVideoId) && account.PreferredVideoId != name)
                {
                    LOG.Info("Set preferred video size by name: " + account.PreferredVideoId);
                    LinphoneAPI.linphone_core_set_preferred_video_size_by_name(linphoneCore, account.PreferredVideoId);

                    int bandwidth = 512;
                    switch (account.PreferredVideoId)
                    {
                        case "720p":
                            bandwidth = 1024 + 128;
                            break;
                        case "svga":
                            bandwidth = 860;
                            break;
                        case "vga":
                            bandwidth = 660;
                            break;
                        case "cif":
                            bandwidth = 460;
                            break;
                        case "qvga":
                            bandwidth = 410;
                            break;
                        case "qcif":
                            bandwidth = 256;
                            break;
                    }
                    LinphoneAPI.linphone_core_set_upload_bandwidth(linphoneCore, bandwidth);
                    LinphoneAPI.linphone_core_set_download_bandwidth(linphoneCore, bandwidth);
                }
            }

            UpdateMediaEncryption();
	    }

        private void UpdateMediaEncryption()
        {
            LinphoneMediaEncryption lme = LinphoneAPI.linphone_core_get_media_encryption(linphoneCore);

            if (lme == LinphoneConfig.MediaEncryption) 
                return;

            int retVal = LinphoneAPI.linphone_core_set_media_encryption(linphoneCore, LinphoneConfig.MediaEncryption);
            if (retVal == 0)
            {
                LOG.Info("Media encryption set to " + LinphoneConfig.MediaEncryption.ToString());
            }
            else
            {
                LOG.Error("Failed to update Linphone media encryption");
            }
        }

        #endregion

		#region Codecs

        public bool UpdateNativeCodecs(VATRPAccount account, CodecType codecType)
	    {
            var retValue = true;

            if (linphoneCore == IntPtr.Zero)
				throw new Exception("Linphone not initialized");

	        if (account == null)
	            throw new ArgumentNullException("Account is not defined");
            
            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;
            var tmpCodecs = new List<VATRPCodec>();
            foreach (var cfgCodec in cfgCodecs)
            {
                // find cfgCodec in linphone codec list
                var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, cfgCodec.CodecName, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE,
                    cfgCodec.Channels);
                if (pt == IntPtr.Zero)
                {
                    LOG.Warn(string.Format("Codec not found: {0} , Channels: {1} ", cfgCodec.CodecName,
                        cfgCodec.Channels));
                    tmpCodecs.Add(cfgCodec);
                }
            }

            foreach (var codec in linphoneCodecs)
            {
                if (!cfgCodecs.Contains(codec))
                {
                    LOG.Info(string.Format("Adding codec into configuration: {0} , Channels: {1} ", codec.CodecName, codec.Channels));
                    cfgCodecs.Add(codec);
                }
            }

            foreach (var codec in tmpCodecs)
            {
                LOG.Info(string.Format("Removing Codec from configuration: {0} , Channels: {1} ", codec.CodecName, codec.Channels));
                cfgCodecs.Remove(codec);
            }

            foreach (var codec in linphoneCodecs)
            {
                for (int i = 0; i < cfgCodecs.Count; i++)
                {
                    if (cfgCodecs[i].CodecName == codec.CodecName && cfgCodecs[i].Rate == codec.Rate && cfgCodecs[i].Channels == codec.Channels)
                    {
                        cfgCodecs[i].Priority = codec.Priority;
                        cfgCodecs[i].Status = codec.Status;
                    }
                }
            }

            return retValue;
	    }

        public bool UpdateCodecsAccessibility(VATRPAccount account, CodecType codecType)
        {
            var retValue = true;

            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");

            if (account == null)
                throw new ArgumentNullException("Account is not defined");

            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;

            foreach (var cfgCodec in cfgCodecs)
            {
                var payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, cfgCodec.CodecName, cfgCodec.IPBitRate,
                    cfgCodec.Channels);
                if (payloadPtr == IntPtr.Zero)
                    continue;
                if (cfgCodec.Status == LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, payloadPtr))
                    continue;
                LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, cfgCodec.Status);
            }

            return retValue;
        }

	    public void FillCodecsList(VATRPAccount account, CodecType codecType)
	    {
            if (account == null)
                throw new ArgumentNullException("Account is not defined");
            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;
            cfgCodecs.Clear();
            cfgCodecs.AddRange(linphoneCodecs);
	    }

        public void configureFmtpCodec()
        {
            var h263PtPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, "H263", 90000, -1);
            setFmtpSetting(h263PtPtr, "CIF=1;QCIF=1", "CIF=1;QCIF=1");
            var h264PtPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, "H264", 90000, -1);
            setFmtpSetting(h264PtPtr, null, "packetization-mode=1;profile-level-id=42801F");
        }
        private void setFmtpSetting(IntPtr ptPtr, string sendFmtp, string recvFmtp)
        {
            if (ptPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(ptPtr, typeof(PayloadType));
                if (recvFmtp != null)
                     payload.recv_fmtp = recvFmtp;
                if (sendFmtp != null)
                    payload.send_fmtp = sendFmtp;
                Marshal.StructureToPtr(payload, ptPtr, false);
                LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, ptPtr);
            }

        }
		private void LoadAudioCodecs()
		{
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");
            _audioCodecs.Clear();
	
            // remove 
		    string[] primaryCodecs = {"G722", "PCMU", "PCMA", "speex", "speex"};
            int[] rate = { LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, 28000, 8000 };

            _linphoneAudioCodecsList = IntPtr.Zero;
		    int i;
		    for (i = 0; i < primaryCodecs.Length; i++)
		    {
		        var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, primaryCodecs[i],
                    rate[i],
		            LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS);

		        if (pt != IntPtr.Zero)
		        {
		            _linphoneAudioCodecsList = LinphoneAPI.ms_list_append(_linphoneAudioCodecsList, pt);
		        }
		    }

            IntPtr audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool found = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            if (rate[i] == LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE || payload.clock_rate == rate[i])
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        _linphoneAudioCodecsList = LinphoneAPI.ms_list_append(_linphoneAudioCodecsList, curStruct.data);
                    }
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);

            LinphoneAPI.linphone_core_set_audio_codecs(linphoneCore, _linphoneAudioCodecsList);

            //  enable codecs
            audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool enable = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            if (rate[i] == LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE || payload.clock_rate == rate[i])
                            {
                                enable = true;
                                break;
                            }
                        }
                    }
                    IntPtr payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, payload.mime_type, payload.clock_rate, payload.channels);
                    LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, enable);
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
            

            audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);
            int index = 1;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));
                    var codec = new VATRPCodec
                    {
                        Priority = index++,
                        Purpose = CodecType.Audio,
                        CodecName = payload.mime_type,
                        Rate = payload.normal_bitrate,
                        IPBitRate = payload.clock_rate,
                        Channels = payload.channels,
                        ReceivingFormat = payload.recv_fmtp,
                        SendingFormat = payload.send_fmtp,
                        Status = LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, curStruct.data),
                        IsUsable = LinphoneAPI.linphone_core_check_payload_type_usability(linphoneCore, curStruct.data)
                    };
                    _audioCodecs.Add(codec);
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
		}

        private void LoadVideoCodecs()
        {
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");
            _videoCodecs.Clear();

            // remove 
            string[] primaryCodecs = { "H264", "H263", "VP8" };

            _linphoneVideoCodecsList = IntPtr.Zero;
            int i;
            for (i = 0; i < primaryCodecs.Length; i++)
            {
                var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, primaryCodecs[i],
                    LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE,
                    LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS);

                if (pt != IntPtr.Zero)
                {
                    _linphoneVideoCodecsList = LinphoneAPI.ms_list_append(_linphoneVideoCodecsList, pt);
                }
            }

            IntPtr videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool found = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        _linphoneVideoCodecsList = LinphoneAPI.ms_list_append(_linphoneVideoCodecsList, curStruct.data);
                    }
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);

            LinphoneAPI.linphone_core_set_video_codecs(linphoneCore, _linphoneVideoCodecsList);

            //  enable codecs
            videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool enable = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            enable = true;
                            break;
                        }
                    }
                    IntPtr payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, payload.mime_type, payload.clock_rate, payload.channels);
                    LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, enable);
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);


            videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);
            int index = 1;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));
                    var codec = new VATRPCodec
                    {
                        Priority = index++,
                        Purpose = CodecType.Video,
                        CodecName = payload.mime_type,
                        Rate = payload.normal_bitrate,
                        IPBitRate = payload.clock_rate,
                        Channels = payload.channels,
                        ReceivingFormat = payload.recv_fmtp,
                        SendingFormat = payload.send_fmtp,
                        Status = LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, curStruct.data),
                        IsUsable = LinphoneAPI.linphone_core_check_payload_type_usability(linphoneCore, curStruct.data)
                    };
                    _videoCodecs.Add(codec);
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
        }

		#endregion

        #region Networking
        public bool UpdateNetworkingParameters(VATRPAccount account)
        {
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");

            if (account == null)
            {
                LOG.Error("UpdateNetworkingParameters: Account is NULL");
                return false;
            }
            if (account.EnableSTUN)
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyUseStun);
                var address = string.Format("{0}:{1}", account.STUNAddress, account.STUNPort);
                LinphoneAPI.linphone_core_set_stun_server(linphoneCore, address);
            }
            else if (account.EnableICE)
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyUseIce);
                var address = string.Format("{0}:{1}", account.ICEAddress, account.ICEPort);
                LinphoneAPI.linphone_core_set_stun_server(linphoneCore, address);
            }
            else
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyNoFirewall);
            }
            int firewallPolicy = LinphoneAPI.linphone_core_get_firewall_policy(linphoneCore);

            return false;
        }

	    public void SetAVPFMode(LinphoneAVPFMode mode, LinphoneRTCPMode rtcpMode)
	    {
	        if (linphoneCore == IntPtr.Zero)
	            return;
            
            LOG.Info("AVPF mode changed to " + mode);
            LinphoneAPI.linphone_core_set_avpf_mode(linphoneCore, mode);
            
	        if (proxy_cfg != IntPtr.Zero)
	        {
                LinphoneAPI.linphone_proxy_config_set_avpf_mode(proxy_cfg, mode);
	        }

	        IntPtr coreConfig = LinphoneAPI.linphone_core_get_config(linphoneCore);
            if (coreConfig != IntPtr.Zero)
            {
                LOG.Info("RTCP mode changing to " + rtcpMode);
                LinphoneAPI.lp_config_set_int(coreConfig, "rtp", "rtcp_fb_implicit_rtcp_fb", (int)rtcpMode);
            }
        }

        public int GetAVPFMode()
        {
            if (linphoneCore == IntPtr.Zero)
                return (int) LinphoneAVPFMode.LinphoneAVPFDefault;

            return LinphoneAPI.linphone_core_get_avpf_mode(linphoneCore);
        }

        #endregion

		#region Events
		void OnRegistrationChanged (IntPtr lc, IntPtr cfg, LinphoneRegistrationState cstate, string message) 
		{
			if (linphoneCore == IntPtr.Zero) return;
		    if (cfg == proxy_cfg)
		    {
                currentRegistrationState = cstate;
		        if (RegistrationStateChangedEvent != null)
		            RegistrationStateChangedEvent(cstate);
		        switch (cstate)
		        {
		            case LinphoneRegistrationState.LinphoneRegistrationOk:
		                LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, true);
		                break;
		            case LinphoneRegistrationState.LinphoneRegistrationFailed:
		            case LinphoneRegistrationState.LinphoneRegistrationCleared:
		                LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, false);
		                break;
		        }
		    }
        }

		void OnGlobalStateChanged(IntPtr lc, LinphoneGlobalState gstate, string message)
		{
			if (linphoneCore == IntPtr.Zero) return;

			if (GlobalStateChangedEvent != null)
				GlobalStateChangedEvent(gstate);
		}
		private void OnCallStateChanged(IntPtr lc, IntPtr callPtr, LinphoneCallState cstate, string message)
		{
			if (linphoneCore == IntPtr.Zero) return;

			LOG.Info(string.Format( "OnCallStateChanged: State - {0}, CallPtr - {1}, Message: {2}", cstate, callPtr, message));

			var newstate = VATRPCallState.None;
			var direction = LinphoneCallDir.LinphoneCallIncoming;
			string remoteParty = "";
			IntPtr addressStringPtr;
		    bool removeCall = false;
			// detecting direction, state and source-destination data by state
			switch (cstate)
			{
				case LinphoneCallState.LinphoneCallIncomingReceived:
				case LinphoneCallState.LinphoneCallIncomingEarlyMedia:
					newstate = cstate == LinphoneCallState.LinphoneCallIncomingReceived
						? VATRPCallState.InProgress
						: VATRPCallState.EarlyMedia;
					addressStringPtr = LinphoneAPI.linphone_call_get_remote_address_as_string(callPtr);
			        if (addressStringPtr != IntPtr.Zero)
			        {
			            identity = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
			        }
					remoteParty = identity;
					break;

				case LinphoneCallState.LinphoneCallConnected:
					newstate = VATRPCallState.Connected;
					break;
				case LinphoneCallState.LinphoneCallStreamsRunning:
                    newstate = VATRPCallState.StreamsRunning;
					break;
				case LinphoneCallState.LinphoneCallPausedByRemote:
                    newstate = VATRPCallState.RemotePaused;
			        break;
				case LinphoneCallState.LinphoneCallPausing:
                    newstate = VATRPCallState.LocalPausing;
					break;
                case LinphoneCallState.LinphoneCallPaused:
                    newstate = VATRPCallState.LocalPaused;
                    break;
                case LinphoneCallState.LinphoneCallResuming:
                    newstate = VATRPCallState.LocalResuming;
			        break;
				case LinphoneCallState.LinphoneCallOutgoingInit:
				case LinphoneCallState.LinphoneCallOutgoingProgress:
				case LinphoneCallState.LinphoneCallOutgoingRinging:
				case LinphoneCallState.LinphoneCallOutgoingEarlyMedia:
					newstate = cstate == LinphoneCallState.LinphoneCallOutgoingInit
						? VATRPCallState.Trying
						: VATRPCallState.Ringing;
					direction = LinphoneCallDir.LinphoneCallOutgoing;
					addressStringPtr = LinphoneAPI.linphone_call_get_remote_address_as_string(callPtr);
			        if (addressStringPtr != IntPtr.Zero)
			        {
			            remoteParty = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
			        }
					break;

				case LinphoneCallState.LinphoneCallError:
                    string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
                    LOG.Info("OnCallStateChanged: CallState=LinphoneCallError .LinphoneLib Version: " + linphoneLibraryVersion);
					newstate = VATRPCallState.Error;
			        removeCall = true;
					break;

				case LinphoneCallState.LinphoneCallEnd:
					newstate = VATRPCallState.Closed;
                    removeCall = true;
					break;
				case LinphoneCallState.LinphoneCallReleased:
			        return;
			}

		    lock (callLock)
		    {
		        VATRPCall call = FindCall(callPtr);

		        if (call == null && !removeCall)
		        {
		            LOG.Info("Call not found. Adding new call into list. ID - " + callPtr + " Calls count: " + callsList.Count);
		            call = new VATRPCall(callPtr) {CallState = newstate, CallDirection = direction};
		            CallParams from = direction == LinphoneCallDir.LinphoneCallIncoming ? call.From : call.To;
		            CallParams to = direction == LinphoneCallDir.LinphoneCallIncoming ? call.To : call.From;

		            if (
		                !VATRPCall.ParseSipAddressEx(remoteParty, out from.DisplayName, out from.Username,
		                    out from.HostAddress,
		                    out from.HostPort))
		                from.Username = "Unknown user";

		            if (
		                !VATRPCall.ParseSipAddressEx(remoteParty, out to.DisplayName, out to.Username, out to.HostAddress,
		                    out to.HostPort))
		                to.Username = "Unknown user";
                    
                    IntPtr chatPtr = LinphoneAPI.linphone_call_get_chat_room(callPtr);

		            if (chatPtr != IntPtr.Zero)
		            {
		                VATRPContact contact;
		                var contactAddress = string.Empty;
		                if (direction == LinphoneCallDir.LinphoneCallIncoming)
		                {
		                    contactAddress = string.Format("{0}@{1}", from.Username, from.HostAddress);
		                    contact = new VATRPContact(new ContactID(contactAddress, chatPtr))
		                    {
		                        DisplayName = from.DisplayName,
		                        Fullname = from.Username,
		                        SipUsername = from.Username
		                    };
		                }
		                else
		                {
		                    contactAddress = string.Format("{0}@{1}", to.Username, to.HostAddress);
		                    contact = new VATRPContact(new ContactID(contactAddress, chatPtr))
		                    {
		                        DisplayName = to.DisplayName,
		                        Fullname = to.Username,
		                        SipUsername = to.Username
		                    };
		                }
		                contact.RegistrationName = contactAddress;
		                call.ChatRoom = new VATRPChat(contact, "rtt");
		                var loggedContact = manager.ContactService.FindLoggedInContact();
		                if (loggedContact != null)
		                    call.ChatRoom.AddContact(loggedContact);
		            }

		            callsList.Add(call);
		        }

		        if (call != null)
		        {
		            call.LinphoneMessage = message;
		            call.CallState = newstate;

                    if (CallStateChangedEvent != null)
                        CallStateChangedEvent(call);
		            if (removeCall)
		            {
		                callsList.Remove(call);
		                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
		                    callsList.Count));
		            }
		        }
		    }
		}

		private void OnNotifyEventReceived(IntPtr lc, IntPtr eventPtr, string notified_event, IntPtr bodyPtr)
		{
			if (linphoneCore == IntPtr.Zero) return;

		    if (bodyPtr != IntPtr.Zero)
		    {
		        IntPtr subTypePtr = LinphoneAPI.linphone_content_get_subtype(bodyPtr);
		        if (subTypePtr != IntPtr.Zero)
		        {
		            if (Marshal.PtrToStringAnsi(subTypePtr) == "simple-message-summary")
		            {
		                if (OnMWIReceivedEvent != null)
                            OnMWIReceivedEvent(new MWIEventArgs(1));
		                return;
		            }
		        }
		    }

		    if (NotifyReceivedEvent != null)
				NotifyReceivedEvent(notified_event);
		}

        private void OnCallStatsUpdated(IntPtr lc, IntPtr callPtr, IntPtr statsPtr)
        {
            if (linphoneCore == IntPtr.Zero) return;

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);
                if (call != null)
                {
                    var callStats =
                        (LinphoneCallStats) Marshal.PtrToStructure(statsPtr, typeof (LinphoneCallStats));
                    if (CallStatisticsChangedEvent != null)
                        CallStatisticsChangedEvent(call, callStats);
                }
            }
        }
        private void OnIsComposingReceived(IntPtr lc, IntPtr chatPtr)
        {
            if (linphoneCore == IntPtr.Zero) return;

            var remoteUser = string.Empty;
            IntPtr remoteAddress = LinphoneAPI.linphone_chat_room_get_peer_address(chatPtr);
            if (remoteAddress != IntPtr.Zero)
            {
                IntPtr addressPtr = LinphoneAPI.linphone_address_as_string(remoteAddress);
                if (addressPtr != IntPtr.Zero)
                {
                    remoteUser = Marshal.PtrToStringAnsi(addressPtr);
                    LinphoneAPI.ortp_free(addressPtr);
                }
            }

            lock (messagingLock)
            {
                uint rttCode = LinphoneAPI.linphone_chat_room_get_char(chatPtr);
                if (rttCode == 0)
                    return;

                if (IsComposingReceivedEvent != null)
                    IsComposingReceivedEvent(remoteUser, chatPtr, rttCode);
            }
        }

        private void OnMessageReceived(IntPtr lc, IntPtr roomPtr, IntPtr message)
        {
            if (linphoneCore == IntPtr.Zero) return;

            IntPtr callChatRoomPtr = IntPtr.Zero;

            if (LinphoneAPI.linphone_core_in_call(linphoneCore))
            {
                IntPtr activeCallPtr = LinphoneAPI.linphone_core_get_current_call(linphoneCore);
                if (activeCallPtr != IntPtr.Zero)
                    callChatRoomPtr = LinphoneAPI.linphone_call_get_chat_room(activeCallPtr);
            }

            lock (messagingLock)
            {
                var from = string.Empty;
                var to = string.Empty;
                IntPtr addressPtr = LinphoneAPI.linphone_chat_message_get_from_address(message);
                if (addressPtr != IntPtr.Zero)
                {
                    IntPtr addressStringPtr = LinphoneAPI.linphone_address_as_string(addressPtr);
                    if (addressStringPtr != IntPtr.Zero)
                    {
                        from = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
                    }
                }

                addressPtr = LinphoneAPI.linphone_chat_message_get_to_address(message);
                if (addressPtr != IntPtr.Zero)
                {
                    IntPtr addressStringPtr = LinphoneAPI.linphone_address_as_string(addressPtr);
                    if (addressStringPtr != IntPtr.Zero)
                    {
                        to = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
                    }
                }

                IntPtr msgPtr = LinphoneAPI.linphone_chat_message_get_text(message);
                var messageString = string.Empty;
                if (msgPtr != IntPtr.Zero)
                    messageString = Marshal.PtrToStringAnsi(msgPtr);

               var localTime = Time.ConvertUtcTimeToLocalTime(LinphoneAPI.linphone_chat_message_get_time(message));
                var chatMessage = new VATRPChatMessage(MessageContentType.Text)
                {
                    Direction = LinphoneAPI.linphone_chat_message_is_outgoing(message) ? MessageDirection.Outgoing : MessageDirection.Incoming,
                    IsIncompleteMessage = false,
                    MessageTime = localTime,
                    Content = messageString,
                    IsRTTMessage = false,
                    IsRead = LinphoneAPI.linphone_chat_message_is_read(message)
                };

                if (OnChatMessageReceivedEvent != null)
                    OnChatMessageReceivedEvent(roomPtr, callChatRoomPtr, from, chatMessage);
            }
        }

        private void OnMessageStatusChanged(IntPtr msgPtr, LinphoneChatMessageState state)
        {
            if (linphoneCore == IntPtr.Zero) return;

            lock (messagingLock)
            {
                if (OnChatMessageStatusChangedEvent != null)
                    OnChatMessageStatusChangedEvent(msgPtr, state);
            }
        }

        private void OnCallLogUpdated(IntPtr lc, IntPtr newcl)
        {
            if (OnLinphoneCallLogUpdatedEvent != null)
                OnLinphoneCallLogUpdatedEvent(lc, newcl);
        }

	    #endregion

        #region Info
        public IntPtr GetCallParams(IntPtr callPtr)
        {
            lock (callLock)
            {
                var call = FindCall(callPtr);
                if (call == null)
                    return IntPtr.Zero;
                return LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr);
            }
        }
        public string GetUsedAudioCodec(IntPtr callParams)
        {
            if (linphoneCore == IntPtr.Zero)
                return string.Empty;

            IntPtr payloadPtr = LinphoneAPI.linphone_call_params_get_used_audio_codec(callParams);
            if (payloadPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(payloadPtr, typeof(PayloadType));
                return payload.mime_type;
            }
            return string.Empty;
        }

        public string GetUsedVideoCodec(IntPtr callParams)
        {
            if (linphoneCore == IntPtr.Zero)
                return string.Empty;

            IntPtr payloadPtr = LinphoneAPI.linphone_call_params_get_used_video_codec(callParams);
            if (payloadPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(payloadPtr, typeof(PayloadType));
                return payload.mime_type;
            }
            return string.Empty;
        }

	    public MSVideoSizeDef GetVideoSize(IntPtr curparams, bool sending)
	    {
	        MSVideoSizeDef msVideoSize = sending
	            ? LinphoneAPI.linphone_call_params_get_sent_video_size(curparams)
	            : LinphoneAPI.linphone_call_params_get_received_video_size(curparams);

	        return msVideoSize;
	    }

	    public float GetFrameRate(IntPtr curparams, bool sending)
        {
            if (linphoneCore == IntPtr.Zero)
                return 0;

            return sending
                ? LinphoneAPI.linphone_call_params_get_sent_framerate(curparams)
                : LinphoneAPI.linphone_call_params_get_received_framerate(curparams);
        }

        public LinphoneMediaEncryption GetMediaEncryption(IntPtr curparams)
        {
            if (linphoneCore == IntPtr.Zero)
                return LinphoneMediaEncryption.LinphoneMediaEncryptionNone;

            return (LinphoneMediaEncryption)LinphoneAPI.linphone_call_params_get_media_encryption(curparams);
        }

	    public LinphoneCallStats GetCallAudioStats(IntPtr callPtr)
	    {
	        lock (callLock)
	        {
	            var call = FindCall(callPtr);

	            if (call != null)
	            {
	                IntPtr statsPtr = LinphoneAPI.linphone_call_get_audio_stats(call.NativeCallPtr);

	                if (statsPtr != IntPtr.Zero)
	                {
	                    return (LinphoneCallStats) Marshal.PtrToStructure(statsPtr, typeof (LinphoneCallStats));
	                }
	            }
	        }
	        return new LinphoneCallStats(); 
        }

        public LinphoneCallStats GetCallVideoStats(IntPtr callPtr)
        {
            lock (callLock)
            {
                var call = FindCall(callPtr);

                if (call != null)
                {
                    IntPtr statsPtr = LinphoneAPI.linphone_call_get_video_stats(call.NativeCallPtr);

                    if (statsPtr != IntPtr.Zero)
                    {
                        return (LinphoneCallStats) Marshal.PtrToStructure(statsPtr, typeof (LinphoneCallStats));
                    }
                }
            }
            return new LinphoneCallStats();
        }

        public void GetUsedPorts(out int sipPort, out int rtpPort)
        {
            sipPort = LinphoneAPI.linphone_core_get_sip_port(linphoneCore);
            rtpPort = LinphoneAPI.linphone_core_get_audio_port(linphoneCore);
        }

	    #endregion

        #region Chat History

        public int GetHistorySize(string username)
        {
            var address = string.Format("sip:{1}@{2}", username, preferences.ProxyHost);
            IntPtr friendAddressPtr = LinphoneAPI.linphone_core_create_address(linphoneCore, address);
            if (friendAddressPtr == IntPtr.Zero)
                return 0;

            IntPtr chatRoomPtr = LinphoneAPI.linphone_core_get_chat_room(linphoneCore, friendAddressPtr);
            if (chatRoomPtr == IntPtr.Zero)
                return 0 ;

            return LinphoneAPI.linphone_chat_room_get_history_size(chatRoomPtr);
        }

        public void LoadChatRoom(VATRPChat chat)
        {
            var address = string.Format("sip:{1}@{2}",  chat.Contact.ID, preferences.ProxyHost);
            IntPtr friendAddressPtr = LinphoneAPI.linphone_core_create_address(linphoneCore, address);
            if (friendAddressPtr == IntPtr.Zero)
                return;

            IntPtr chatRoomPtr = LinphoneAPI.linphone_core_get_chat_room(linphoneCore, friendAddressPtr);
            if (chatRoomPtr == IntPtr.Zero)
                return;
            IntPtr historyListPtr = LinphoneAPI.linphone_chat_room_get_history(chatRoomPtr, 0); // load all messages

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(historyListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    IntPtr msgPtr = LinphoneAPI.linphone_chat_message_get_text(curStruct.data);
                    var messageString = string.Empty;
                    if (msgPtr != IntPtr.Zero)
                        messageString = Marshal.PtrToStringAnsi(msgPtr);

                    if (!string.IsNullOrEmpty(messageString) && messageString.Length > 1)
                    {
                        var localTime =
                            Time.ConvertUtcTimeToLocalTime(LinphoneAPI.linphone_chat_message_get_time(curStruct.data));

                        var chatMessage = new VATRPChatMessage(MessageContentType.Text)
                        {
                            Direction =
                                LinphoneAPI.linphone_chat_message_is_outgoing(curStruct.data)
                                    ? MessageDirection.Outgoing
                                    : MessageDirection.Incoming,
                            IsIncompleteMessage = false,
                            MessageTime = localTime,
                            Content = messageString,
                            IsRead = LinphoneAPI.linphone_chat_message_is_read(curStruct.data),
                            IsRTTMessage = false,
                            IsRTTStartMarker = false,
                            IsRTTEndMarker = false,
                            Chat = chat
                        };
                        chat.Messages.Add(chatMessage);
                    }
                }
                historyListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
        }

        #endregion


        #region IVATRPInterface

        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;
        public bool Start()
        {
            return Start(true);
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;

            if (IsStopped)
                return true;
            _isStopping = true;

            var cmd = new LinphoneCommand(LinphoneCommandType.TerminateAllCalls);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }

            DoUnregister();

            cmd = new LinphoneCommand(LinphoneCommandType.StopLinphone);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
            return true;
        }
        #endregion

        #region Devices
        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableCameras()
        {
            //linphone_core_get_video_devices
            List<VATRPDevice> cameraList = new List<VATRPDevice>();

            IntPtr videoDevicesPtr = LinphoneAPI.linphone_core_get_video_devices(linphoneCore);
            if (videoDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(videoDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.CAMERA);
                    cameraList.Add(newDevice);
                    offset += IntPtr.Size;
                }
            }
            return cameraList;
        }

        public void SetCamera(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_video_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedCamera()
        {
            IntPtr deviceIdPtr = LinphoneAPI.linphone_core_get_video_device(linphoneCore);
            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.CAMERA);
                return device;
            }
            return null;
        }


        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableMicrophones()
        {
            //linphone_core_get_sound_devices
            // filter with linphone_core_sound_device_can_capture
            List<VATRPDevice> microphoneList = new List<VATRPDevice>();
            IntPtr soundDevicesPtr = LinphoneAPI.linphone_core_get_sound_devices(linphoneCore);
            if (soundDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(soundDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    if (LinphoneAPI.linphone_core_sound_device_can_capture(linphoneCore, device))
                    {
                        VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.MICROPHONE);
                        microphoneList.Add(newDevice);
                    }
                    offset += IntPtr.Size;
                }
            }
            return microphoneList;
        }

        public void SetCaptureDevice(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_capture_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedMicrophone()
        {
            IntPtr deviceIdPtr = LinphoneAPI.linphone_core_get_capture_device(linphoneCore);
            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.MICROPHONE);
                return device;
            }
            return null;
        }


        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableSpeakers()
        {
            //linphone_core_get_sound_devices
            // filter with linphone_core_sound_device_can_playback
            List<VATRPDevice> speakerList = new List<VATRPDevice>();
            IntPtr soundDevicesPtr = LinphoneAPI.linphone_core_get_sound_devices(linphoneCore);
            if (soundDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(soundDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    if (LinphoneAPI.linphone_core_sound_device_can_playback(linphoneCore, device))
                    {
                        VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.SPEAKER);
                        speakerList.Add(newDevice);
                    }
                    offset += IntPtr.Size;
                }
            }
            return speakerList;
        }

        public void SetSpeakers(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_playback_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedSpeakers()
        {
            IntPtr deviceIdPtr = LinphoneAPI.linphone_core_get_playback_device(linphoneCore);
            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.SPEAKER);
                return device;
            }
            return null;
        }

        #endregion
    }
}
