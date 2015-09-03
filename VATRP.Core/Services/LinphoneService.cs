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
		Thread coreLoop;
		private bool isRunning = true;
		private string identity;
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

			_isStarted = true;
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

			identity = "sip:" + preferences.Username + "@" + preferences.ProxyHost;
			server_addr = "sip:" + preferences.ProxyHost + ":" + preferences.ProxyPort.ToString();

			auth_info = LinphoneAPI.linphone_auth_info_new(preferences.Username, null, preferences.Password, null, null, null);
			if (auth_info == IntPtr.Zero)
				LOG.Debug("failed to get auth info");
			LinphoneAPI.linphone_core_add_auth_info(linphoneCore, auth_info);

			proxy_cfg = LinphoneAPI.linphone_core_create_proxy_config(linphoneCore);
			/*set identity with user name and domain*/
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

		public void ReceiveCall(VATRPCall call)
		{
			if (call == null)
				throw new ArgumentNullException("call");

			if (linphoneCore == IntPtr.Zero || !isRunning)
			{
				if (ErrorEvent != null)
					ErrorEvent(call, "Cannot receive call when Linphone Core is not working.");
				return;
			}

			LinphoneAPI.linphone_call_params_set_record_file(callsDefaultParams, null);
			LinphoneAPI.linphone_core_accept_call_with_params(linphoneCore, call.NativeCallPtr, callsDefaultParams);
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
		private void OnCallStateChanged(IntPtr lc, IntPtr callPtr, LinphoneCallState cstate, string message)
		{
			if (linphoneCore == IntPtr.Zero || !isRunning) return;
			Console.WriteLine("OnCallStateChanged: {0}", cstate);

			var newstate = VATRPCallState.None;
			var direction = LinphoneCallDir.LinphoneCallIncoming;
			string caller = "";
			string callee = "";
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
						caller = Marshal.PtrToStringAnsi(addressStringPtr);
					callee = identity;

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
						callee = Marshal.PtrToStringAnsi(addressStringPtr);
					caller = this.identity;
					break;

				case LinphoneCallState.LinphoneCallError:
					newstate = VATRPCallState.Error;
					break;

				case LinphoneCallState.LinphoneCallReleased:
				case LinphoneCallState.LinphoneCallEnd:
					newstate = VATRPCallState.Closed;
					if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
						LinphoneAPI.linphone_call_stop_recording(callPtr);
					break;

				default:
					break;
			}

			VATRPCall existCall = FindCall(callPtr);

			if (existCall == null)
			{
				existCall = new VATRPCall(callPtr) {CallState = newstate, CallDirection = direction, From = caller, To = callee};
				callsList.Add(existCall);

				if ((CallStateChangedEvent != null))
					CallStateChangedEvent(existCall);
			}
			else
			{
				if (existCall.CallState != newstate)
				{
					existCall.CallState = newstate;
					CallStateChangedEvent(existCall);
				}
			}

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

		public VATRPCall FindCall(IntPtr callPtr)
		{
			foreach (var call in callsList)
			{
				if (call.NativeCallPtr == callPtr)
					return call;
			}
			return null;
		}

		public void ToggleMute(VATRPCall _currentCall)
		{
			if (_currentCall == null)
				throw new ArgumentNullException();

			if (linphoneCore == IntPtr.Zero || !isRunning) 
				return;

			LinphoneAPI.linphone_core_enable_mic(linphoneCore, !LinphoneAPI.linphone_core_mic_enabled(linphoneCore));
		}

		public bool CanMakeVideoCall()
		{
			if (linphoneCore == IntPtr.Zero || !isRunning)
				throw new Exception("Linphone not initialized");
			
			return true;
		}

        public void SwitchSelfVideo()
        {
            if (linphoneCore == IntPtr.Zero || !isRunning)
                throw new Exception("Linphone not initialized");

            bool isSelfViewEnabled = LinphoneAPI.linphone_core_self_view_enabled(linphoneCore);

            Debug.WriteLine("SelfView is enabled: " + isSelfViewEnabled );
            LinphoneAPI.linphone_core_enable_self_view(linphoneCore, !isSelfViewEnabled);
        }
    }
}
