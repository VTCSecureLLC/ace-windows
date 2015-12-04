using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows
{
	public partial class MainWindow
	{
		private bool registerRequested = false;
		private bool isRegistering = true;
		private bool signOutRequest = false;
		private bool defaultConfigRequest;
		private string videoTitle = string.Empty;
		private void OnCallStateChanged(VATRPCall call)
		{
			if (this.Dispatcher.Thread != Thread.CurrentThread)
			{
				this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
				return;
			}

		    if (call == null)
		        return;

			CallViewModel callViewModel = _mainViewModel.FindCallViewModel(call);

			if (callViewModel == null)
			{
				callViewModel = new CallViewModel(_linphoneService, call)
				{
					CallInfoCtrl = _callInfoView
				};
				switch (App.CurrentAccount.PreferredVideoId.ToLower())
				{
					case "qcif":
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_QCIF_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_QCIF_H;
						break;
					case "cif":
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_CIF_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_CIF_H;
						break;
					case "4cif":
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_4CIF_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_4CIF_H;
						break;
					case "vga":
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_VGA_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_VGA_H;
						break;
					case "svga":
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_SVGA_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_SVGA_H;
						break;
					default:
						callViewModel.VideoWidth = (int)MSVideoSize.MS_VIDEO_SIZE_CIF_W;
						callViewModel.VideoHeight = (int)MSVideoSize.MS_VIDEO_SIZE_CIF_H;
						break;
				}
				_mainViewModel.AddCalViewModel(callViewModel);
			}

		    if (call.CallState == VATRPCallState.InProgress)
		    {
		        if (_linphoneService.GetActiveCallsCount == 2)
		        {
		            // check to ensure we have not ringing call
		            CallViewModel nextCall = _mainViewModel.GetNextViewModel(callViewModel);
		            if (nextCall != null && (nextCall.ActiveCall.CallState == VATRPCallState.InProgress ||
		                                     nextCall.ActiveCall.CallState == VATRPCallState.Ringing))
		            {
		                // decline call
                        callViewModel.Declined = true;
		                callViewModel.DeclineCall(true);
		                return;
		            }
		        }
		        _mainViewModel.ActiveCallModel = callViewModel;
		    }
		    else if (call.CallState == VATRPCallState.Connected)
		    {
		    }
		    else if (call.CallState == VATRPCallState.LocalResumed)
		    {
		        _mainViewModel.ActiveCallModel = callViewModel;
		    }

		    if (callViewModel.Declined)
		    {
                // do not process declined call
                _mainViewModel.RemoveCalViewModel(callViewModel);
		        return;
		    }

		    if (_mainViewModel.ActiveCallModel == null)
		        _mainViewModel.ActiveCallModel = callViewModel;

		    ctrlCall.SetCallViewModel(_mainViewModel.ActiveCallModel);
			
			LOG.Info(string.Format("CallStateChanged: State - {0}. Call: {1}", call.CallState, call.NativeCallPtr ));
			var stopPlayback = false;
		    var destroycall = false;
			switch (call.CallState)
			{
				case VATRPCallState.Trying:
					// call started, 
					videoTitle = call.To.DisplayName;
					videoTitle = !string.IsNullOrWhiteSpace(call.To.DisplayName)
						? string.Format("\"{0}\" {1}", call.To.DisplayName, call.To.Username)
						: call.To.Username;
					call.RemoteParty = call.To;
					callViewModel.OnTrying();

			        if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
			            Configuration.ConfEntry.USE_RTT, true))
			        {
                        ctrlRTT.SetViewModel(_mainViewModel.MessagingModel);
			            _mainViewModel.MessagingModel.CreateConversation(call.To.Username);
			        }
			        _mainViewModel.IsCallPanelDocked = true;
					break;
				case VATRPCallState.InProgress:
					videoTitle = !string.IsNullOrWhiteSpace(call.From.DisplayName)
						? string.Format("\"{0}\" {1}", call.From.DisplayName, call.From.Username)
						: call.From.Username;
					
					call.RemoteParty = call.From;
					ServiceManager.Instance.SoundService.PlayRingTone();
					_mainViewModel.IsCallPanelDocked = true;

					if (_linphoneService.GetActiveCallsCount == 2)
					{
						callViewModel.ShowSwitchCallPanel = true;
						ShowCallOverlayWindow(false);
					}
					else
						callViewModel.ShowIncomingCallPanel = true;
					
					callViewModel.OnIncomingCall();
					
					_flashWindowHelper.FlashWindow(_callView);
					Topmost = true;
					Activate();
					Topmost = false;
					break;
				case VATRPCallState.Ringing:
					callViewModel.OnRinging();
                    _mainViewModel.IsCallPanelDocked = true;
					call.RemoteParty = call.To;
					ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
					ctrlCall.ctrlOverlay.SetCallState("Ringing");
					ServiceManager.Instance.SoundService.PlayRingBackTone();
					break;
				case VATRPCallState.EarlyMedia:
					callViewModel.OnEarlyMedia();
					break;
				case VATRPCallState.Connected:
					if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
						Configuration.ConfEntry.USE_RTT, true))
					{
                        ctrlRTT.SetViewModel(_mainViewModel.MessagingModel);
						_mainViewModel.MessagingModel.CreateConversation(call.RemoteParty.Username);
					}
					
					callViewModel.OnConnected();
					_flashWindowHelper.StopFlashing();
					stopPlayback = true;
					callViewModel.ShowOutgoingEndCall = false;
					
					ShowCallOverlayWindow(true);
					ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
					ctrlCall.ctrlOverlay.SetCallState("Connected");
					ctrlCall.ctrlOverlay.StartCallTimer(callViewModel.Duration);

					//try
					//{
					//    if (_selfView.IsVisible)
					//    {
					//        Window window = Window.GetWindow(_selfView);
					//        if (window != null)
					//        {
					//            var wih = new WindowInteropHelper(window);
					//            IntPtr hWnd = wih.EnsureHandle();
					//            ServiceManager.Instance.LinphoneService.SetVideoPreviewWindowHandle(hWnd);
					//        }
					//    }
					//    else
					//    {
					//        _selfView.Show();
					//    }
					//}
					//catch (Exception ex)
					//{
					//    ServiceManager.LogError("Show Self preview", ex);
					//    RecreateSelfView();
					//}
					_callOverlayView.EndCallRequested = false;

					ctrlCall.AddVideoControl();
					break;
				case VATRPCallState.StreamsRunning:
					callViewModel.OnStreamRunning();
					break;
				case VATRPCallState.RemotePaused:
			        callViewModel.OnRemotePaused();
					break;
                case VATRPCallState.LocalPausing:
                    callViewModel.CallState = VATRPCallState.LocalPausing;
                    break;
                case VATRPCallState.LocalPaused:
                    callViewModel.OnLocalPaused();
                    callViewModel.CallState = VATRPCallState.LocalPaused;
                    break;
                case VATRPCallState.LocalResuming:
                    callViewModel.OnResumed();
                    ShowCallOverlayWindow(true);
					ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
					ctrlCall.ctrlOverlay.SetCallState("Connected");
					ctrlCall.ctrlOverlay.StartCallTimer(callViewModel.Duration);
			        ctrlCall.UpdateControls();
					ctrlCall.AddVideoControl();
                    break;
                case VATRPCallState.Closed:
					_flashWindowHelper.StopFlashing();
					callViewModel.OnClosed(false);
					stopPlayback = true;
			        destroycall = true;
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                       Configuration.ConfEntry.USE_RTT, true))
                    {
                        _mainViewModel.MessagingModel.ClearConversation();
                        ctrlRTT.SetViewModel(null);
                    }
					int callsCount = _mainViewModel.RemoveCalViewModel(callViewModel);
					if (callsCount == 0)
					{
						_callInfoView.Hide();
                       
						ctrlCall.ctrlOverlay.StopCallTimer();
						ShowCallOverlayWindow(false);
						_mainViewModel.IsMessagingDocked = false;
						_mainViewModel.IsCallPanelDocked = false;
						_mainViewModel.ActiveCallModel = null;

					}
					else
					{
						var nextVM = _mainViewModel.GetNextViewModel(null);
					    if (nextVM != null)
					    {
                            _mainViewModel.ActiveCallModel = nextVM;
                            if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                        Configuration.ConfEntry.USE_RTT, true))
                            {
                                ctrlRTT.SetViewModel(_mainViewModel.MessagingModel);
                                _mainViewModel.MessagingModel.CreateConversation(nextVM.ActiveCall.RemoteParty.Username);
                            }
                            ShowCallOverlayWindow(true);
                            ctrlCall.ctrlOverlay.SetCallerInfo(nextVM.CallerInfo);
                            ctrlCall.ctrlOverlay.StartCallTimer(nextVM.Duration);
                            ctrlCall.SetCallViewModel(_mainViewModel.ActiveCallModel);
                            ctrlCall.UpdateControls();
					        if (nextVM.ActiveCall.CallState == VATRPCallState.LocalPaused)
					        {
					            _mainViewModel.ResumeCall(nextVM);
					        }
					    }
					}
					
					if ((registerRequested || signOutRequest || defaultConfigRequest) && _linphoneService.GetActiveCallsCount == 0)
					{
						_linphoneService.Unregister(false);
					}
					break;
				case VATRPCallState.Error:
			        destroycall = true;
					_flashWindowHelper.StopFlashing();
					callViewModel.OnClosed(true);
					stopPlayback = true;
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                       Configuration.ConfEntry.USE_RTT, true))
                    {
                        _mainViewModel.MessagingModel.ClearConversation();
                        ctrlRTT.SetViewModel(null);
                    }

					if (_linphoneService.GetActiveCallsCount == 0)
					{
						_mainViewModel.RemoveCalViewModel(callViewModel);
						_callInfoView.Hide();
						ctrlCall.ctrlOverlay.StopCallTimer();
						ShowCallOverlayWindow(false);
						_mainViewModel.IsMessagingDocked = false;
						_mainViewModel.IsCallPanelDocked = false;
						_mainViewModel.ActiveCallModel = null;
					}
					
					break;
				default:
					break;
			}

			if (stopPlayback)
			{
				ServiceManager.Instance.SoundService.StopRingBackTone();
				ServiceManager.Instance.SoundService.StopRingTone();
			}

		    if (destroycall)
		    {
                if (_linphoneService.GetActiveCallsCount == 0)
                {
                    if (_mainViewModel.IsCallPanelDocked)
                    {
                        ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(IntPtr.Zero, true);
                        if (_remoteVideoView != null)
                        {
                            _remoteVideoView.DestroyOnClosing = true; // allow window to be closed
                            _remoteVideoView.Close();
                            _remoteVideoView = null;
                            _callOverlayView.Hide();
                        }
                        _mainViewModel.IsMessagingDocked = false;
                        _mainViewModel.IsCallPanelDocked = false;
                    }
                    _mainViewModel.ActiveCallModel = null;
                }
		    }
		}

		private void ShowCallOverlayWindow(bool bShow)
		{
			Rect r = LayoutInformation.GetLayoutSlot(ctrlCall);
			ctrlCall.ctrlOverlay.CommandWindowLeftMargin = ctrlDialpad.ActualWidth + (700 - 550) / 2;
			ctrlCall.ctrlOverlay.CommandWindowTopMargin = 500 - SystemParameters.CaptionHeight;

			ctrlCall.ctrlOverlay.NumpadWindowLeftMargin = ctrlDialpad.ActualWidth + (700 - 230) / 2;
			ctrlCall.ctrlOverlay.NumpadWindowTopMargin = 170 - SystemParameters.CaptionHeight;

			ctrlCall.ctrlOverlay.CallInfoWindowLeftMargin = ctrlDialpad.ActualWidth + (700 - 550) / 2;
			ctrlCall.ctrlOverlay.CallInfoWindowTopMargin = 40 - SystemParameters.CaptionHeight;

			ctrlCall.ctrlOverlay.ShowCommandBar(bShow);
			ctrlCall.ctrlOverlay.ShowNumpadWindow(false);
			ctrlCall.ctrlOverlay.ShowCallInfoWindow(bShow);
			if (!bShow)
				ctrlCall.ctrlVideo.Visibility = Visibility.Hidden;
		}

		private void RecreateSelfView()
		{
			LOG.Info("Recreate Self view");
			try
			{
				_selfView = new SelfView();
				_selfView.Show();
			}
			catch (Exception ex)
			{
				ServiceManager.LogError("RecreateSelfView", ex);
			}
		}

		private void OnRegistrationChanged(LinphoneRegistrationState state)
		{
			if (this.Dispatcher.Thread != Thread.CurrentThread)
			{
				this.Dispatcher.BeginInvoke((Action)(() => this.OnRegistrationChanged(state)));
				return;
			}

			RegistrationState = state;
			var statusString = "Unregistered";
			this.BtnSettings.IsEnabled = true;
			LOG.Info(String.Format("Registration state changed. Current - {0}", state));
			_mainViewModel.ContactModel.RegistrationState = state;
			switch (state)
			{
				case LinphoneRegistrationState.LinphoneRegistrationProgress:
					statusString = isRegistering ? "Registering..." : "Unregistering...";
					this.BtnSettings.IsEnabled = false;
					break;
				case LinphoneRegistrationState.LinphoneRegistrationOk:
					statusString = "Registered";
					ServiceManager.Instance.SoundService.PlayConnectionChanged(true);
					isRegistering = false;
					break;
				case LinphoneRegistrationState.LinphoneRegistrationFailed:
					ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
					break;
				case LinphoneRegistrationState.LinphoneRegistrationCleared:
					statusString = "Unregistered";
					isRegistering = true;
					ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
					if (registerRequested)
					{
						registerRequested = false;
						_linphoneService.Register();
					}
					else if (signOutRequest || defaultConfigRequest)
					{
						isRegistering = false;
						WizardPagepanel.Children.Clear();
						ServiceSelector.Visibility = Visibility.Visible;
						WizardPagepanel.Visibility = Visibility.Visible;
						NavPanel.Visibility = Visibility.Collapsed;
						signOutRequest = false;
						_mainViewModel.IsAccountLogged = false;
						_mainViewModel.IsDialpadDocked = false;
						_mainViewModel.IsCallHistoryDocked = false;
						_mainViewModel.IsContactDocked = false;
						_mainViewModel.IsMessagingDocked = false;
						ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
				Configuration.ConfEntry.ACCOUNT_IN_USE, string.Empty);
						ServiceManager.Instance.AccountService.DeleteAccount(App.CurrentAccount);
						if (defaultConfigRequest)
						{
							ResetConfiguration();
						}
						App.CurrentAccount = null;
					}
					break;
				default:
					break;
			}
		}

		private void ResetConfiguration()
		{
			ServiceManager.Instance.AccountService.ClearAccounts();
			ServiceManager.Instance.ConfigurationService.Reset();   
			ServiceManager.Instance.HistoryService.ClearCallsItems();
			ServiceManager.Instance.ContactService.RemoveContacts();
			defaultConfigRequest = false;
		}

		private void OnNewAccountRegistered(string accountId)
		{
			ServiceSelector.Visibility = Visibility.Collapsed;
			WizardPagepanel.Visibility = Visibility.Collapsed;
			LOG.Info(string.Format( "New account registered. Useaname -{0}. Host - {1} Port - {2}",
				App.CurrentAccount.RegistrationUser,
				App.CurrentAccount.ProxyHostname,
				App.CurrentAccount.ProxyPort));
			NavPanel.Visibility = Visibility.Visible;

			_mainViewModel.IsDialpadDocked = true;
			_mainViewModel.IsCallHistoryDocked = true;
			_mainViewModel.IsAccountLogged = true;
			ServiceManager.Instance.UpdateLoggedinContact();
			
			if (ServiceManager.Instance.UpdateLinphoneConfig())
			{
				if (ServiceManager.Instance.StartLinphoneService())
					ServiceManager.Instance.Register();
			}
		}

		private void OnChildVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (App.AllowDestroyWindows)
				return;
			var window = sender as VATRPWindow;
			if (window == null)
				return;
			var bShow = (bool)e.NewValue;
			switch (window.WindowType)
			{
				case VATRPWindowType.CONTACT_VIEW:
					BtnContacts.IsChecked = bShow;
					break;
				case VATRPWindowType.MESSAGE_VIEW:
					BtnResourcesView.IsChecked = bShow;
					break;
				case VATRPWindowType.RECENTS_VIEW:
					BtnRecents.IsChecked = bShow;
					_mainViewModel.IsCallHistoryDocked = !bShow;
					break;
				case VATRPWindowType.DIALPAD_VIEW:
					BtnDialpad.IsChecked = bShow;
					_mainViewModel.IsDialpadDocked = !bShow;
					break;
				case VATRPWindowType.SETTINGS_VIEW:
					break;
			}
		}

		private void OnCallInfoVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (App.AllowDestroyWindows)
				return;
			var window = sender as VATRPWindow;
			if (window == null)
				return;
			var bShow = (bool)e.NewValue;
			if (_mainViewModel.ActiveCallModel != null)
			{
				_mainViewModel.ActiveCallModel.IsCallInfoOn = bShow;
			}

			if (ctrlCall != null) 
				ctrlCall.BtnInfo.IsChecked = bShow;
		}

		private void OnMakeCallRequested(string called_address)
		{
		    _mainViewModel.DialpadModel.RemotePartyNumber = "";
			MediaActionHandler.MakeVideoCall(called_address);
		}

		private void OnKeypadClicked(object sender, KeyPadEventArgs e)
		{
			_linphoneService.PlayDtmf((char)e.Key, 250);
			if (_mainViewModel.ActiveCallModel != null)
				_linphoneService.SendDtmf(_mainViewModel.ActiveCallModel.ActiveCall, (char)e.Key);
		}

		private void OnDialpadClicked(object sender, KeyPadEventArgs e)
		{
			_linphoneService.PlayDtmf((char)e.Key, 250);
		}

		private void OnSettingsChangeRequired(Enums.VATRPSettings settingsType)
		{
			if (_settingsView.IsVisible)
				return;

			_mainViewModel.SettingsModel.SetActiveSettings(settingsType);
			_settingsView.Show();
			_settingsView.Activate();
		}

		private void OnResetToDefaultConfiguration()
		{
			if (ServiceManager.Instance.ActiveCallPtr != IntPtr.Zero)
			{
				MessageBox.Show("There is an active call. Please try later");
				return;
			}

			if (defaultConfigRequest)
				return;
			defaultConfigRequest = true;

			if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
			{
				_linphoneService.Unregister(false);
			}
			else
			{
				ResetConfiguration();
			}
		}

	}
}
