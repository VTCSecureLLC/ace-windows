using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private string identity;
		Thread coreLoop;
		private bool isRunning = true;
		private string server_addr;
		private bool _isStarting;
		private bool _isStarted;
		private bool _isStopping;
		private bool _isStopped;
        private List<VATRPCodec> _audioCodecs = new List<VATRPCodec>();
        private List<VATRPCodec> _videoCodecs = new List<VATRPCodec>(); 
		private List<VATRPCall> callsList = new List<VATRPCall>();
        private Object callLock = new Object();
		LinphoneCoreVTable vtable;
		LCSipTransports t_config;

		private LinphoneCoreRegistrationStateChangedCb registration_state_changed;
		private LinphoneCoreCallStateChangedCb call_state_changed;
		private LinphoneCoreGlobalStateChangedCb global_state_changed;
		private LinphoneCoreNotifyReceivedCb notify_received;
	    private LinphoneCoreCallStatsUpdatedCb call_stats_updated;
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
 #endregion

		#region Methods
		public LinphoneService(ServiceManagerBase manager)
		{
			this.manager = manager;
			preferences = new Preferences();
			_isStarting = false;
			_isStarted = false;
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


		    isRunning = true;
			registration_state_changed = new LinphoneCoreRegistrationStateChangedCb(OnRegistrationChanged);
			call_state_changed = new LinphoneCoreCallStateChangedCb(OnCallStateChanged);
			global_state_changed = new LinphoneCoreGlobalStateChangedCb(OnGlobalStateChanged);
			notify_received = new LinphoneCoreNotifyReceivedCb(OnNotifyEventReceived);
            call_stats_updated = new LinphoneCoreCallStatsUpdatedCb(OnCallStatsUpdated);
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

			linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, null, null, IntPtr.Zero);
			if (linphoneCore != IntPtr.Zero)
			{
                LinphoneAPI.libmsopenh264_init();
			    LinphoneAPI.linphone_core_set_video_preset(linphoneCore, "30");
				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, false);
				LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);

                callsDefaultParams = LinphoneAPI.linphone_core_create_default_call_parameters(linphoneCore);
                LinphoneAPI.linphone_call_params_enable_video(callsDefaultParams, true);
                LinphoneAPI.linphone_call_params_enable_early_media_sending(callsDefaultParams, true);

                // load installed codecs
			    LoadAudioCodecs();
                LoadVideoCodecs();

				coreLoop = new Thread(LinphoneMainLoop) {IsBackground = false};
				coreLoop.Start();

				_isStarted = true;
			}
			return _isStarted;
		}

		public bool Stop()
		{
			if (IsStarting || IsStopping)
				return false;

			if (IsStopped)
				return true;
			_isStopping = true;
			if (linphoneCore != IntPtr.Zero)
				LinphoneAPI.linphone_core_terminate_all_calls(linphoneCore);

            try
            {
                if (callsDefaultParams != IntPtr.Zero)
                    LinphoneAPI.linphone_call_params_destroy(callsDefaultParams);
            }
            catch (Exception ex)
            {

            }

			SetTimeout(delegate
			{
				isRunning = false;
			}, 1000);

			_isStarted = false;
			_isStopped = true;
			return true;
		}

        void LinphoneMainLoop()
        {
            LOG.Debug("Main loop started");
            while (isRunning)
            {
                LinphoneAPI.linphone_core_iterate(linphoneCore); // roll

                Thread.Sleep(30);
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
            notify_received = null;
            linphoneCore = callsDefaultParams = proxy_cfg = auth_info = t_configPtr = IntPtr.Zero;
            call_stats_updated = null;
            coreLoop = null;
            identity = null;
            server_addr = null;

            LOG.Debug("Main loop exited");
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
            if (linphoneCore == IntPtr.Zero || !isRunning)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return false;
            }

            return true;
        }

        public void PlayDtmf(char dtmf, int duration)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
                return;
            LinphoneAPI.linphone_core_play_dtmf(linphoneCore, dtmf, duration);
		}
		
        public void SendDtmfAsSipInfo(bool use_info)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
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

		#endregion

		#region Registration
		public bool Register()
		{
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
            
			server_addr = string.Format("sip:{0}:{1};transport={2}", preferences.ProxyHost,
                preferences.ProxyPort, preferences.Transport.ToLower());

            LOG.Debug(string.Format( "Register SIP account: {0} Server: {1}", identity, server_addr));

			auth_info = LinphoneAPI.linphone_auth_info_new(preferences.Username, null, preferences.Password, null, null, null);
			if (auth_info == IntPtr.Zero)
				LOG.Debug("failed to get auth info");
			LinphoneAPI.linphone_core_add_auth_info(linphoneCore, auth_info);

			proxy_cfg = LinphoneAPI.linphone_core_create_proxy_config(linphoneCore);
			/*set localParty with user name and domain*/
			LinphoneAPI.linphone_proxy_config_set_identity(proxy_cfg, identity);

			LinphoneAPI.linphone_proxy_config_set_server_addr(proxy_cfg, server_addr);
			LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, true);
			LinphoneAPI.linphone_core_add_proxy_config(linphoneCore, proxy_cfg);
			LinphoneAPI.linphone_core_set_default_proxy_config(linphoneCore, proxy_cfg);
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
            if (proxy_cfg != IntPtr.Zero && LinphoneAPI.linphone_proxy_config_is_registered(proxy_cfg))
            {
                try
                {
                    if (proxy_cfg != IntPtr.Zero)
                        LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);

                    if (proxy_cfg != IntPtr.Zero)
                        LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
                    if (proxy_cfg != IntPtr.Zero)
                    LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
                    proxy_cfg = IntPtr.Zero;
                }
                catch (Exception ex)
                {
                    
                }
                if (t_configPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(t_configPtr);
                    t_configPtr = IntPtr.Zero;
                }
            }
	    }
		#endregion

		#region Call
		public void MakeCall(string destination, bool videoOn)
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

			if (linphoneCore == IntPtr.Zero || !isRunning) {
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot make when Linphone Core is not working.");
				return;
			}


			IntPtr callPtr = LinphoneAPI.linphone_core_invite_with_params (linphoneCore, destination, callsDefaultParams);

			if (callPtr == IntPtr.Zero)
			{
                LOG.Warn("Cannot make call. Cause - Failed to initialize call in linphone libray");
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot call.");
				return;
			}

			var call = new VATRPCall(callPtr);
            LOG.Warn("Make call. Add new call. Ptr - " + callPtr);
		}

		public void AcceptCall(IntPtr callPtr)
		{
            if (linphoneCore == IntPtr.Zero || !isRunning)
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
		        //	LinphoneAPI.linphone_call_params_set_record_file(callsDefaultParams, null);
		        LinphoneAPI.linphone_core_accept_call_with_params(linphoneCore, call.NativeCallPtr, callsDefaultParams);
		    }
		}

	    public void DeclineCall(IntPtr callPtr)
	    {
	        if (linphoneCore == IntPtr.Zero || !isRunning)
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

                LOG.Info("Decline Call: "+ callPtr);
                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
    callsList.Count));
                if (CallStateChangedEvent != null)
                    CallStateChangedEvent(call);
                callsList.Remove(call);
                LinphoneAPI.linphone_core_decline_call(linphoneCore, callPtr, LinphoneReason.LinphoneReasonDeclined);
	        }
	    }

	    public bool TerminateCall(IntPtr callPtr)
	    {
	        if (linphoneCore == IntPtr.Zero || !isRunning)
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

	            IntPtr hwndVideo = LinphoneAPI.linphone_core_get_native_video_window_id(linphoneCore);

	            if (Win32NativeAPI.IsWindow(hwndVideo))
	                Win32NativeAPI.DestroyWindow(hwndVideo);

	            // notify call state end
	            if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
	                LinphoneAPI.linphone_call_stop_recording(call.NativeCallPtr);

                LOG.Info("Terminate Call " + callPtr);

	            call.CallState = VATRPCallState.Closed;
	            if (CallStateChangedEvent != null)
	                CallStateChangedEvent(call);
                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
    callsList.Count));

	            callsList.Remove(call);

                /* terminate the call */
                try
                {
                    LinphoneAPI.linphone_core_terminate_call(linphoneCore, call.NativeCallPtr);
                }
                catch (Exception ex)
                {
                    LOG.Error("Exception on terminate calls. " + ex.Message);
                }

//                LinphoneAPI.linphone_call_unref(call.NativeCallPtr);
	        }
	        return true;
	    }

	    public bool IsCallMuted()
		{
			if (linphoneCore == IntPtr.Zero || !isRunning)
				return false;
			return !LinphoneAPI.linphone_core_mic_enabled(linphoneCore);
		}

		public void ToggleMute()
		{
			if (linphoneCore == IntPtr.Zero || !isRunning)
				return;

			LinphoneAPI.linphone_core_enable_mic(linphoneCore, !LinphoneAPI.linphone_core_mic_enabled(linphoneCore));
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

		#region Video

		public void EnableVideo(bool enable)
		{
			var t_videoPolicy = new LinphoneVideoPolicy()
			{
				automatically_initiate = enable,
				automatically_accept = enable
			};

			var t_videoPolicyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoPolicy));
			if (t_videoPolicyPtr != IntPtr.Zero)
			{

				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, true);
				LinphoneAPI.linphone_core_set_video_policy(linphoneCore, t_videoPolicyPtr);
				Marshal.FreeHGlobal(t_videoPolicyPtr);
			}
		}

		public void SwitchSelfVideo()
		{
			if (linphoneCore == IntPtr.Zero || !isRunning)
				throw new Exception("Linphone not initialized");

			bool isSelfViewEnabled = LinphoneAPI.linphone_core_self_view_enabled(linphoneCore);
			LinphoneAPI.linphone_core_enable_self_view(linphoneCore, !isSelfViewEnabled);
		}

		public void SetVideoPreviewWindowHandle(IntPtr hWnd, bool reset = false)
		{
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

	    public void UpdateVideoSize(VATRPAccount account)
	    {
	        if (account == null)
	        {
                LOG.Error("Account is null");
	            return;
	        }
	        MSVideoSize w , h;
	        switch (account.PreferredVideoId)
	        {
	            case "qcif":
	                w = MSVideoSize.MS_VIDEO_SIZE_QCIF_W;
	                h = MSVideoSize.MS_VIDEO_SIZE_QCIF_H;
	                break;
	            case "cif":
	                w = MSVideoSize.MS_VIDEO_SIZE_CIF_W;
	                h = MSVideoSize.MS_VIDEO_SIZE_CIF_H;
	                break;
	            case "4cif":
	                w = MSVideoSize.MS_VIDEO_SIZE_4CIF_W;
	                h = MSVideoSize.MS_VIDEO_SIZE_4CIF_H;
	                break;
	            case "vga":
	                w = MSVideoSize.MS_VIDEO_SIZE_VGA_W;
	                h = MSVideoSize.MS_VIDEO_SIZE_VGA_H;
	                break;
	            case "svga":
	                w = MSVideoSize.MS_VIDEO_SIZE_SVGA_W;
	                h = MSVideoSize.MS_VIDEO_SIZE_SVGA_H;
	                break;
	            default:
                    LOG.Warn("Not supported video size: " + account.PreferredVideoId);
	                return;
	        }
	        var t_videoSize = new MSVideoSizeDef()
	        {
	            height = Convert.ToInt32(h),
	            width = Convert.ToInt32(w)
	        };

            LOG.Info("Set preferred video size by name: " + account.PreferredVideoId);
#if true
            LinphoneAPI.linphone_core_set_preferred_video_size_by_name(linphoneCore, account.PreferredVideoId);
#else
            var t_videoSizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoSize));
            if (t_videoSizePtr != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_set_preferred_video_size(linphoneCore, t_videoSizePtr);
                Marshal.FreeHGlobal(t_videoSizePtr);
            }
#endif
        }

	    #endregion

		#region Codecs

        public bool UpdateNativeCodecs(VATRPAccount account, CodecType codecType)
	    {
            if (linphoneCore == IntPtr.Zero || !isRunning)
				throw new Exception("Linphone not initialized");

	        if (account == null)
	            throw new ArgumentNullException("Account is not defined");
            var retValue = true;
            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;
            var tmpCodecs = new List<VATRPCodec>();
            foreach (var cfgCodec in cfgCodecs)
            {
                // find cfgCodec in linphone codec list
                var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, cfgCodec.CodecName, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE,
                    cfgCodec.Channels);
                if (pt != IntPtr.Zero)
                {
                    var isEnabled = LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, pt);
                    LOG.Info("Codec: " + cfgCodec.CodecName + " Enabled: " + cfgCodec.Status + " In Linphone Status: " +
                             isEnabled);

                    if (LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, pt, cfgCodec.Status) != 0)
                    {
                        LOG.Warn("Failed to update codec: " + cfgCodec.CodecName + " Enabled: " + cfgCodec.Status +
                                 " Restoring status");
                        cfgCodec.Status = isEnabled;
                        retValue = false;
                    }
                    else
                    {
                        LOG.Info(string.Format("=== Updated Codec {0}, Channels {1} Status: {2}. ", cfgCodec.CodecName,
                            cfgCodec.Channels,
                            cfgCodec.Status ? "Enabled" : "Disabled"));
                    }
                }
                else
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

		private void LoadAudioCodecs()
		{
            if (linphoneCore == IntPtr.Zero || !isRunning)
                throw new Exception("Linphone not initialized");
            _audioCodecs.Clear();
			var audioCodecsList = new List<PayloadType>();
			IntPtr audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);

			MSList curStruct;
			do
			{
				curStruct.next = IntPtr.Zero;
				curStruct.prev = IntPtr.Zero;
				curStruct.data = IntPtr.Zero;
				curStruct = (MSList) Marshal.PtrToStructure(audioCodecListPtr, typeof (MSList));
				if (curStruct.data != IntPtr.Zero)
				{
					var payload = (PayloadType) Marshal.PtrToStructure(curStruct.data, typeof (PayloadType));

				    var codec = new VATRPCodec
				    {
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
            if (linphoneCore == IntPtr.Zero || !isRunning)
                throw new Exception("Linphone not initialized");

            _videoCodecs.Clear();
            var videoCodecsList = new List<PayloadType>();
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
                    var codec = new VATRPCodec
                    {
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
            if (linphoneCore == IntPtr.Zero || !isRunning)
                throw new Exception("Linphone not initialized");

            if (account == null)
            {
                LOG.Error("UpdateNetworkingParameters: Account is NULL");
                return false;
            }

            if (account.EnubleSTUN)
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyUseStun);
                var address = string.Format("{0}:{1}", account.STUNAddress, account.STUNPort);
                LinphoneAPI.linphone_core_set_stun_server(linphoneCore, address);
            }
            else
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyNoFirewall);
            }
            
            return false;
        }

	    public void SetAVPFMode(LinphoneAVPFMode mode)
	    {
	        if (linphoneCore == IntPtr.Zero || !isRunning)
                throw new Exception("Linphone not initialized");

	        int linphoneAvpfMode = LinphoneAPI.linphone_core_get_avpf_mode(linphoneCore);
	        if (linphoneAvpfMode != (int) mode)
	        {
                LOG.Info("AVPF mode changed to " + mode);
	            LinphoneAPI.linphone_core_set_avpf_mode(linphoneCore, mode);
	        }
	    }

        #endregion

		#region Events
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
		private void OnCallStateChanged(IntPtr lc, IntPtr callPtr, LinphoneCallState cstate, string message)
		{
			if (linphoneCore == IntPtr.Zero || !isRunning) return;

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
						identity = Marshal.PtrToStringAnsi(addressStringPtr);
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
				case LinphoneCallState.LinphoneCallUpdatedByRemote:
                    newstate = VATRPCallState.RemoteUpdated;
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
						remoteParty = Marshal.PtrToStringAnsi(addressStringPtr);
					break;

				case LinphoneCallState.LinphoneCallError:
					newstate = VATRPCallState.Error;
			        removeCall = true;
					break;

				case LinphoneCallState.LinphoneCallEnd:
					newstate = VATRPCallState.Closed;
                    removeCall = true;
					break;
				case LinphoneCallState.LinphoneCallReleased:
                    if (CallStateChangedEvent != null)
                        CallStateChangedEvent(null);
			        return;
			}

		    lock (callLock)
		    {
		        VATRPCall call = FindCall(callPtr);

		        if (call == null && !removeCall)
		        {
		            LOG.Info("Call not found. Adding new call into list. ID - " + callPtr);
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

		            callsList.Add(call);
		        }

		        if (call != null)
		        {
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

		private void OnNotifyEventReceived(IntPtr lc, IntPtr lev, string notified_event, IntPtr body)
		{
			if (linphoneCore == IntPtr.Zero || !isRunning) return;

			Debug.Print("linphoneService Notify:  " + notified_event);
			if (NotifyReceivedEvent != null)
				NotifyReceivedEvent(notified_event);
		}

        private void OnCallStatsUpdated(IntPtr lc, IntPtr callPtr, IntPtr statsPtr)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning) return;

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
            if (linphoneCore == IntPtr.Zero || !isRunning)
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
            if (linphoneCore == IntPtr.Zero || !isRunning)
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
            do
            {
                if (linphoneCore == IntPtr.Zero || !isRunning)
                    break;


                MSVideoSizeDef msVideoSize = sending
                    ? LinphoneAPI.linphone_call_params_get_sent_video_size(curparams)
                    : LinphoneAPI.linphone_call_params_get_received_video_size(curparams);

                return msVideoSize;

            } while (false);

            return new MSVideoSizeDef()
            {
                height = (int)MSVideoSize.MS_VIDEO_SIZE_UNKNOWN_H,
                width = (int)MSVideoSize.MS_VIDEO_SIZE_UNKNOWN_W,
            };
        }

        public float GetFrameRate(IntPtr curparams, bool sending)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
                return 0;

            return sending
                ? LinphoneAPI.linphone_call_params_get_sent_framerate(curparams)
                : LinphoneAPI.linphone_call_params_get_received_framerate(curparams);
        }

        public LinphoneMediaEncryption GetMediaEncryption(IntPtr curparams)
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
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
    }
}
