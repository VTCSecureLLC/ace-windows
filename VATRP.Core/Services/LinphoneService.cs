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
		private List<VATRPCall> callsList = new List<VATRPCall>();
		LinphoneCoreVTable vtable;
		LCSipTransports t_config;

		LinphoneCoreRegistrationStateChangedCb registration_state_changed;
		LinphoneCoreCallStateChangedCb call_state_changed;
		LinphoneCoreGlobalStateChangedCb global_state_changed;
        LinphoneCoreNotifyReceivedCb notify_received;

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
				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, false);
				LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);
			    
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

			SetTimeout(delegate
			{
				isRunning = false;
			}, 1000);

			_isStarted = false;
			_isStopped = true;
			return true;
		}
		#endregion

		#region Registration
		public bool Register()
		{
			t_config = new LCSipTransports()
			{
				udp_port = LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
				tcp_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
				dtls_port = LinphoneAPI.LC_SIP_TRANSPORT_DISABLED,
				tls_port = LinphoneAPI.LC_SIP_TRANSPORT_DISABLED
			};
			t_configPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_config));
			Marshal.StructureToPtr(t_config, t_configPtr, false);
			LinphoneAPI.linphone_core_set_sip_transports(linphoneCore, t_configPtr);

			LinphoneAPI.linphone_core_set_user_agent(linphoneCore, preferences.UserAgent, preferences.Version);

			callsDefaultParams = LinphoneAPI.linphone_core_create_default_call_parameters(linphoneCore);
			LinphoneAPI.linphone_call_params_enable_video(callsDefaultParams, true);
			LinphoneAPI.linphone_call_params_enable_early_media_sending(callsDefaultParams, true);

		    if (string.IsNullOrEmpty(preferences.DisplayName))
		    {
		        identity = string.Format( "sip:{0}@{1}", preferences.Username, preferences.ProxyHost);
		    }
            else
            {
                identity = string.Format("\"{0}\" <sip:{1}@{2}>", preferences.DisplayName, preferences.Username,
                    preferences.ProxyHost);
            }
		    server_addr = string.Format("sip:{0}:{1}", preferences.ProxyHost, preferences.ProxyPort);

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

		public bool Unregister()
		{
			if (RegistrationStateChangedEvent != null)
				RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationProgress); // disconnecting

			SetTimeout(delegate
			{
				LinphoneAPI.linphone_call_params_destroy(callsDefaultParams);

				if (LinphoneAPI.linphone_proxy_config_is_registered(proxy_cfg))
				{
					LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
					LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
					LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
				}
			}, 5000);

			return true;
		}
		#endregion

		#region Call
		public void MakeCall(string destination, bool videoOn)
		{
			if (string.IsNullOrEmpty(destination))
				throw new ArgumentNullException("sipUriOrPhone");

			if (linphoneCore == IntPtr.Zero || !isRunning) {
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot make or receive calls when Linphone Core is not working.");
				return;
			}


			IntPtr callPtr = LinphoneAPI.linphone_core_invite_with_params (linphoneCore, destination, callsDefaultParams);

			if (callPtr == IntPtr.Zero)
			{
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot call.");
				return;
			}

			VATRPCall call = new VATRPCall(callPtr);
		}

		public void AcceptCall(VATRPCall call)
		{
			if (call == null)
				throw new ArgumentNullException("call");

			if (linphoneCore == IntPtr.Zero || !isRunning)
			{
				if (ErrorEvent != null)
					ErrorEvent(call, "Cannot receive call when Linphone Core is not working.");
				return;
			}

		    int p1 = 0, p2 = 0;
		    LinphoneAPI.linphone_core_get_video_port_range(linphoneCore, ref p1, ref p2);
            Debug.WriteLine("Accept: P1 = " + p1 + " P2 = " + p2);
           // LinphoneAPI.linphone_call_enable_camera(call.NativeCallPtr, true); // accept video call

            LinphoneAPI.linphone_call_params_set_record_file(callsDefaultParams, null);

			LinphoneAPI.linphone_core_accept_call_with_params(linphoneCore, call.NativeCallPtr, callsDefaultParams);
		}
		public void DeclineCall(VATRPCall call)
		{
			if (call == null)
				throw new ArgumentNullException("call");

			if (linphoneCore == IntPtr.Zero || !isRunning)
			{
				if (ErrorEvent != null)
					ErrorEvent(call, "Cannot receive call when Linphone Core is not working.");
				return;
			}

			/* terminate the call */
			LinphoneAPI.linphone_core_terminate_call(linphoneCore, call.NativeCallPtr);
		}

		public void TerminateCall(VATRPCall call)
		{
			if (linphoneCore == IntPtr.Zero || !isRunning)
			{
				if (ErrorEvent != null)
					ErrorEvent(null, "Cannot make or receive calls when Linphone Core is not working.");
				return;
			}

			if (call != null )
			{
				int call_state = LinphoneAPI.linphone_call_get_state(call.NativeCallPtr);

				IntPtr hwndVideo = LinphoneAPI.linphone_core_get_native_video_window_id(linphoneCore);

				if (Win32NativeAPI.IsWindow(hwndVideo))
					Win32NativeAPI.DestroyWindow(hwndVideo);

				if (call_state != (int)LinphoneCallState.LinphoneCallEnd)
					Debug.WriteLine("Call State: " + call_state);
				/* terminate the call */
				LinphoneAPI.linphone_core_terminate_call(linphoneCore, call.NativeCallPtr);

				// notify call state end
				 if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
						LinphoneAPI.linphone_call_stop_recording(call.NativeCallPtr);

				callsList.Remove(call);

				if ((CallStateChangedEvent != null))
					CallStateChangedEvent(call);
				LinphoneAPI.linphone_call_unref(call.NativeCallPtr);
			}
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
			return true;
		}

		#endregion

        #region Codecs

        #endregion

        #region Methods
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
		    notify_received = null;
			linphoneCore = callsDefaultParams = proxy_cfg = auth_info = t_configPtr = IntPtr.Zero;
			coreLoop = null;
			identity = null;
			server_addr = null;

			LOG.Debug("Main loop exited");
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
			Console.WriteLine("Linphoneserviec OnCallStateChanged: {0}", cstate);

			var newstate = VATRPCallState.None;
			var direction = LinphoneCallDir.LinphoneCallIncoming;
			string remoteParty = "";
			IntPtr addressStringPtr;

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
				case LinphoneCallState.LinphoneCallStreamsRunning:
				case LinphoneCallState.LinphoneCallPausedByRemote:
				case LinphoneCallState.LinphoneCallUpdatedByRemote:
					newstate = VATRPCallState.Connected;
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
					break;

				case LinphoneCallState.LinphoneCallEnd:
					newstate = VATRPCallState.Closed;
					if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
						LinphoneAPI.linphone_call_stop_recording(callPtr);
					break;
                case LinphoneCallState.LinphoneCallReleased:
			        return;
				default:
					break;
			}

			VATRPCall call = FindCall(callPtr);

			if (call == null)
			{
				call = new VATRPCall(callPtr) {CallState = newstate, CallDirection = direction};
			    CallParams from = direction == LinphoneCallDir.LinphoneCallIncoming ? call.From : call.To;
                CallParams to = direction == LinphoneCallDir.LinphoneCallIncoming ? call.To : call.From;

			    if (
                    !VATRPCall.ParseSipAddressEx(remoteParty, out from.DisplayName, out from.Username, out from.HostAddress,
			            out from.HostPort))
			        from.Username = "Unknown user";

                Console.WriteLine("DN: " + from.DisplayName);
                Console.WriteLine("UN: " + from.Username);
			    if (
                    !VATRPCall.ParseSipAddressEx(remoteParty, out to.DisplayName, out to.Username, out to.HostAddress,
			            out to.HostPort))
			        to.Username = "Unknown user";


				callsList.Add(call);
				LinphoneAPI.linphone_call_ref(call.NativeCallPtr);

				if ((CallStateChangedEvent != null))
					CallStateChangedEvent(call);
			}
			else
			{
				if (call.CallState != newstate)
				{
					call.CallState = newstate;
					CallStateChangedEvent(call);
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
		#endregion

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
				throw new Exception("Linphone not initialized");
			
			return true;
		}




    }
}
