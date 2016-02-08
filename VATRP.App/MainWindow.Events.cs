﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Events;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows
{
	public partial class MainWindow
	{
		private bool registerRequested = false;
		private bool signOutRequest = false;
		private bool defaultConfigRequest;
        
	    private void DeferedHideOnError(object sender, EventArgs e)
	    {
	        deferredHideTimer.Stop();

	        if (_mainViewModel != null)
	        {
                _mainViewModel.IsCallPanelDocked = false;
	        }
	    }
        private void DeferredShowPreview(object sender, EventArgs e)
        {
            deferredShowPreviewTimer.Stop();

            if (_selfView != null)
            {
                _selfView.Show();
                _selfView.Activate();
            }
        }
		
	    private void OnCallStateChanged(VATRPCall call)
		{
			if (this.Dispatcher.Thread != Thread.CurrentThread)
			{
				this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
				return;
			}

		    if (call == null)
		        return;

            if (deferredHideTimer != null && deferredHideTimer.IsEnabled)
                deferredHideTimer.Stop();

			CallViewModel callViewModel = _mainViewModel.FindCallViewModel(call);

			if (callViewModel == null)
			{
				callViewModel = new CallViewModel(_linphoneService, call)
				{
					CallInfoCtrl = _callInfoView
				};

                callViewModel.VideoWidth = (int)CombinedUICallViewSize.Width;
			    callViewModel.VideoHeight = (int)CombinedUICallViewSize.Height;
				_mainViewModel.AddCalViewModel(callViewModel);
			}

		    if ((call.CallState == VATRPCallState.InProgress) ||
                (call.CallState == VATRPCallState.StreamsRunning))
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

            LOG.Info(string.Format("CallStateChanged: State - {0}. Call: {1}", call.CallState, call.NativeCallPtr));
		    ctrlCall.SetCallViewModel(_mainViewModel.ActiveCallModel);
			
			var stopPlayback = false;
		    var destroycall = false;
			switch (call.CallState)
			{
				case VATRPCallState.Trying:
					// call started, 
					call.RemoteParty = call.To;
					callViewModel.OnTrying();
			        _mainViewModel.IsCallPanelDocked = true;
					break;
				case VATRPCallState.InProgress:
			        this.ShowSelfPreviewItem.IsEnabled = false;
					call.RemoteParty = call.From;
					ServiceManager.Instance.SoundService.PlayRingTone();
                    callViewModel.OnIncomingCall();

			        if (_linphoneService.GetActiveCallsCount == 2)
			        {
			            ShowOverlayNewCallWindow(true);
                        ctrlCall.ctrlOverlay.SetNewCallerInfo(callViewModel.CallerInfo);
			        }
			        else
			        {
			            callViewModel.ShowIncomingCallPanel = true;
			        }

                    if (WindowState != WindowState.Minimized)
                        _mainViewModel.IsCallPanelDocked = true;

					_flashWindowHelper.FlashWindow(this);
			        if (WindowState == WindowState.Minimized)
                        this.WindowState = WindowState.Normal;
                    
                    Topmost = true;
                    Activate();
                    Topmost = false;
			        break;
				case VATRPCallState.Ringing:
                    this.ShowSelfPreviewItem.IsEnabled = false;
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
						_mainViewModel.MessagingModel.CreateRttConversation(call.RemoteParty.Username, call.NativeCallPtr);
					}
					
					callViewModel.OnConnected();
					_flashWindowHelper.StopFlashing();
					stopPlayback = true;
					callViewModel.ShowOutgoingEndCall = false;
			        callViewModel.IsRTTEnabled =
			            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
			                Configuration.ConfEntry.USE_RTT, true) && callViewModel.ActiveCall != null &&
			            _linphoneService.IsRttEnabled(callViewModel.ActiveCall.NativeCallPtr);

					ShowCallOverlayWindow(true);
                    ShowOverlayNewCallWindow(false);
					ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
					ctrlCall.ctrlOverlay.SetCallState("Connected");
                    ctrlCall.ctrlOverlay.ForegroundCallDuration = callViewModel.CallDuration;

			        if (_linphoneService.GetActiveCallsCount == 2)
			        {
			            CallViewModel nextVM = _mainViewModel.GetNextViewModel(callViewModel);
			            if (nextVM != null)
			            {
			                ShowOverlaySwitchCallWindow(true);
			                ctrlCall.ctrlOverlay.SetPausedCallerInfo(nextVM.CallerInfo);
                            ctrlCall.ctrlOverlay.BackgroundCallDuration = nextVM.CallDuration;
                            ctrlCall.ctrlOverlay.StartPausedCallTimer(ctrlCall.ctrlOverlay.BackgroundCallDuration);
                            ctrlCall.BackgroundCallViewModel = nextVM;
			            }
			        }
			        else
			        {
                        ShowOverlaySwitchCallWindow(false);
			        }

                    ctrlCall.ctrlOverlay.StartCallTimer(ctrlCall.ctrlOverlay.ForegroundCallDuration);
					_callOverlayView.EndCallRequested = false;

                    if (_selfView.IsVisible)
                    {
                        _selfView.ResetNativePreviewHandle = false;
                        _selfView.Hide();
                    }

                    ctrlCall.ctrlVideo.DrawCameraImage = false;
					ctrlCall.AddVideoControl();
                    ctrlCall.RestartInactivityDetectionTimer();
			        ctrlCall.UpdateVideoSettingsIfOpen();

//                    MuteCall(createCmd.MuteMicrophone);
//                    MuteSpeaker(createCmd.MuteSpeaker);

					break;
				case VATRPCallState.StreamsRunning:
					callViewModel.OnStreamRunning();
                    ShowCallOverlayWindow(true);
                    // VATRP-1623: we are setting mute microphone true prior to initiating a call, but the call is always started
                    //   with the mic enabled. attempting to mute right after call is connected here to side step this issue - 
                    //   it appears to be an initialization issue in linphone
                    if (_linphoneService.GetActiveCallsCount == 1)
                    {
                        ServiceManager.Instance.ApplyMediaSettingsChanges();
                    }
                    ctrlCall.ctrlOverlay.SetCallState("Connected");
			        ctrlCall.UpdateControls();
                    ctrlCall.ctrlOverlay.ForegroundCallDuration = _mainViewModel.ActiveCallModel.CallDuration;
                    ctrlCall.RestartInactivityDetectionTimer();
			        ctrlCall.UpdateVideoSettingsIfOpen();
                    // VATRP-1899: This is a quick and dirty solution for POC. It will be funational, but not the end implementation we will want.
                    if ((App.CurrentAccount != null) && App.CurrentAccount.UserNeedsAgentView)
                    {
                        _mainViewModel.IsMessagingDocked = true;
                    }
					break;
				case VATRPCallState.RemotePaused:
			        callViewModel.OnRemotePaused();
                    callViewModel.CallState = VATRPCallState.RemotePaused;
                    ShowCallOverlayWindow(true);
                    ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
                    ctrlCall.ctrlOverlay.SetCallState("On Hold");
                    ctrlCall.UpdateControls();
					break;
                case VATRPCallState.LocalPausing:
                    callViewModel.CallState = VATRPCallState.LocalPausing;
                    break;
                case VATRPCallState.LocalPaused:
                    callViewModel.OnLocalPaused();
                    callViewModel.CallState = VATRPCallState.LocalPaused;
			        callViewModel.IsCallOnHold = true;
			        bool updateInfoView = callViewModel.PauseRequest;
			        if (_linphoneService.GetActiveCallsCount == 2)
			        {
			            if (!callViewModel.PauseRequest)
			            {
			                CallViewModel nextVM = _mainViewModel.GetNextViewModel(callViewModel);

			                if (nextVM != null)
			                {
			                    ShowOverlaySwitchCallWindow(true);
			                    ctrlCall.ctrlOverlay.SetPausedCallerInfo(callViewModel.CallerInfo);
			                    ctrlCall.ctrlOverlay.BackgroundCallDuration = callViewModel.CallDuration;
			                    ctrlCall.ctrlOverlay.StartPausedCallTimer(ctrlCall.ctrlOverlay.BackgroundCallDuration);
			                    ctrlCall.BackgroundCallViewModel = callViewModel;
			                    ctrlCall.ctrlOverlay.ForegroundCallDuration = nextVM.CallDuration;
			                    ctrlCall.SetCallViewModel(nextVM);
			                    if (!nextVM.PauseRequest)
			                        _mainViewModel.ResumeCall(nextVM);
			                    else
			                        updateInfoView = true;
			                }
			            }
			        }

                    if (updateInfoView)
			        {
                        ShowCallOverlayWindow(true);
                        ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
                        ctrlCall.ctrlOverlay.SetCallState("On Hold");
                        ctrlCall.UpdateControls();
			        }
			        break;
                case VATRPCallState.LocalResuming:
                    callViewModel.OnResumed();
                    callViewModel.IsCallOnHold = false;
                    ShowCallOverlayWindow(true);
					ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
					ctrlCall.ctrlOverlay.SetCallState("Connected");
			        ctrlCall.UpdateControls();

			        if (_linphoneService.GetActiveCallsCount == 2)
			        {
			            CallViewModel nextVM = _mainViewModel.GetNextViewModel(callViewModel);
			            if (nextVM != null)
			            {
			                ShowOverlaySwitchCallWindow(true);
			                ctrlCall.ctrlOverlay.SetPausedCallerInfo(nextVM.CallerInfo);
                            ctrlCall.ctrlOverlay.BackgroundCallDuration = nextVM.CallDuration ;
                            ctrlCall.ctrlOverlay.StartPausedCallTimer(ctrlCall.ctrlOverlay.BackgroundCallDuration);
                            ctrlCall.BackgroundCallViewModel = nextVM;
                            ctrlCall.SetCallViewModel(callViewModel);
                            ctrlCall.ctrlOverlay.ForegroundCallDuration = callViewModel.CallDuration;
                            if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                        Configuration.ConfEntry.USE_RTT, true))
                            {
                                _mainViewModel.MessagingModel.CreateRttConversation(call.RemoteParty.Username, call.NativeCallPtr);
                            }
			            }
			            else
			            {
			                ShowOverlaySwitchCallWindow(false);
			            }
			        }
			        else
			        {
			            ShowOverlaySwitchCallWindow(false);
			        }
                    ctrlCall.ctrlVideo.DrawCameraImage = false;
					ctrlCall.AddVideoControl();
                    break;
                case VATRPCallState.Closed:
					_flashWindowHelper.StopFlashing();
					callViewModel.OnClosed(false, string.Empty);
					stopPlayback = true;
			        destroycall = true;
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                       Configuration.ConfEntry.USE_RTT, true))
                    {
                        _mainViewModel.MessagingModel.ClearRTTConversation(call.NativeCallPtr);
                        ctrlRTT.SetViewModel(null);
                    }
                    ShowOverlayNewCallWindow(false);
                    ShowOverlaySwitchCallWindow(false);
                    ctrlCall.BackgroundCallViewModel = null;

					int callsCount = _mainViewModel.RemoveCalViewModel(callViewModel);
					if (callsCount == 0)
					{
                        this.ShowSelfPreviewItem.IsEnabled = true;
						_callInfoView.Hide();
						ctrlCall.ctrlOverlay.StopCallTimer();
						ShowCallOverlayWindow(false);
						_mainViewModel.IsMessagingDocked = false;
						_mainViewModel.IsCallPanelDocked = false;
                        this.SizeToContent = SizeToContent.WidthAndHeight;
						_mainViewModel.ActiveCallModel = null;
					    OnFullScreenToggled(false); // restore main window to dashboard

                        if (this.ShowSelfPreviewItem.IsChecked && !_selfView.ResetNativePreviewHandle)
                        {
                            _selfView.ResetNativePreviewHandle = true;
                            deferredShowPreviewTimer.Start();
					    }
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
                                _mainViewModel.MessagingModel.CreateRttConversation(
                                    nextVM.ActiveCall.RemoteParty.Username, nextVM.ActiveCall.NativeCallPtr);
                            }
                            ShowCallOverlayWindow(true);
                            ctrlCall.ctrlOverlay.SetCallerInfo(nextVM.CallerInfo);
                            ctrlCall.ctrlOverlay.ForegroundCallDuration = _mainViewModel.ActiveCallModel.CallDuration;
                            ctrlCall.SetCallViewModel(_mainViewModel.ActiveCallModel);
                            ctrlCall.UpdateControls();
					        if (nextVM.ActiveCall.CallState == VATRPCallState.LocalPaused )
					        {
					            if (!nextVM.PauseRequest)
					                _mainViewModel.ResumeCall(nextVM);
					            else
					            {
                                    ctrlCall.ctrlOverlay.SetCallState("On Hold");
					            }
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
                    ctrlCall.BackgroundCallViewModel = null;
					callViewModel.OnClosed(true, call.LinphoneMessage);
					stopPlayback = true;
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                       Configuration.ConfEntry.USE_RTT, true))
                    {
                        _mainViewModel.MessagingModel.ClearRTTConversation(call.NativeCallPtr);
                        ctrlRTT.SetViewModel(null);
                    }

					if (_linphoneService.GetActiveCallsCount == 0)
					{
                        this.ShowSelfPreviewItem.IsEnabled = true;
                        if (this.ShowSelfPreviewItem.IsChecked && !_selfView.ResetNativePreviewHandle)
                        {
                            _selfView.ResetNativePreviewHandle = true;
                            deferredShowPreviewTimer.Start();
                        }
						_mainViewModel.RemoveCalViewModel(callViewModel);
						_callInfoView.Hide();
						ctrlCall.ctrlOverlay.StopCallTimer();
                        ShowCallOverlayWindow(false);
                        _mainViewModel.IsMessagingDocked = false;

                        if (deferredHideTimer != null)
                            deferredHideTimer.Start();
                        _mainViewModel.ActiveCallModel = null;
                        OnFullScreenToggled(false); // restore main window to dashboard
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
                        }
                        if (deferredHideTimer != null && !deferredHideTimer.IsEnabled)
                        {
                            _mainViewModel.IsMessagingDocked = false;
                            _mainViewModel.IsCallPanelDocked = false;
                        }
                    }

                    _mainViewModel.ActiveCallModel = null;
                }
		    }
		}

	    private void OnSwitchHoldCallsRequested(object sender, EventArgs eventArgs)
	    {
	        if (_linphoneService.GetActiveCallsCount != 2)
	            return;

	        CallViewModel callViewModel = ctrlCall.BackgroundCallViewModel;
            CallViewModel nextVM = _mainViewModel.GetNextViewModel(callViewModel);
	        if (nextVM != null)
	        {
	            ShowOverlaySwitchCallWindow(true);
	            ctrlCall.ctrlOverlay.SetPausedCallerInfo(nextVM.CallerInfo);
	            ctrlCall.ctrlOverlay.BackgroundCallDuration = nextVM.CallDuration;
	            ctrlCall.ctrlOverlay.StartPausedCallTimer(ctrlCall.ctrlOverlay.BackgroundCallDuration);
	            ctrlCall.BackgroundCallViewModel = nextVM;
	            ctrlCall.SetCallViewModel(callViewModel);
                ctrlCall.ctrlOverlay.SetCallerInfo(callViewModel.CallerInfo);
	            ctrlCall.ctrlOverlay.ForegroundCallDuration = callViewModel.CallDuration;
	            if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
	                Configuration.ConfEntry.USE_RTT, true))
	            {
	                _mainViewModel.MessagingModel.CreateRttConversation(callViewModel.ActiveCall.RemoteParty.Username,
	                    callViewModel.ActiveCall.NativeCallPtr);
	            }
	        }
	        else
	        {
	            ShowOverlaySwitchCallWindow(false);
	        }
	    }

	    private void ShowCallOverlayWindow(bool bShow)
		{
            if (bShow)
                RearrangeUICallView(GetCallViewSize());

			ctrlCall.ctrlOverlay.ShowCommandBar(bShow);
			ctrlCall.ctrlOverlay.ShowNumpadWindow(false);
			ctrlCall.ctrlOverlay.ShowCallInfoWindow(bShow);

		    if (!bShow)
		    {
		        ctrlCall.ctrlVideo.Visibility = Visibility.Hidden;
                ctrlCall.ctrlOverlay.ShowNewCallAcceptWindow(false);
                ctrlCall.ctrlOverlay.ShowCallsSwitchWindow(false);
                ctrlCall.ctrlOverlay.ShowOnHoldWindow(false);
            }
		}

        private void ShowOverlayNewCallWindow(bool bShow)
        {
            ctrlCall.ctrlOverlay.ShowNewCallAcceptWindow(bShow);
        }
        private void ShowOverlaySwitchCallWindow(bool bShow)
        {
            ctrlCall.ctrlOverlay.ShowCallsSwitchWindow(bShow);
            if (!bShow)
                ctrlCall.ctrlOverlay.StopPausedCallTimer();
        }

		private void OnRegistrationChanged(LinphoneRegistrationState state)
		{
			if (this.Dispatcher.Thread != Thread.CurrentThread)
			{
				this.Dispatcher.BeginInvoke((Action)(() => this.OnRegistrationChanged(state)));
				return;
			}
		    var processSignOut = false;
			RegistrationState = state;

			this.BtnSettings.IsEnabled = true;
			LOG.Info(String.Format("Registration state changed. Current - {0}", state));
			_mainViewModel.ContactModel.RegistrationState = state;
			switch (state)
			{
				case LinphoneRegistrationState.LinphoneRegistrationProgress:
					this.BtnSettings.IsEnabled = false;
			        return;
				case LinphoneRegistrationState.LinphoneRegistrationOk:
			        if (_playRegisterNotify)
			        {
			            _playRegisterNotify = false;
			            ServiceManager.Instance.SoundService.PlayConnectionChanged(true);
			        }
					break;
				case LinphoneRegistrationState.LinphoneRegistrationFailed:
					ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
			        _playRegisterNotify = true;
                    if (signOutRequest || defaultConfigRequest)
                    {
                        processSignOut = true;
                    }
					break;
				case LinphoneRegistrationState.LinphoneRegistrationCleared:
					
					ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
			        _playRegisterNotify = true;
					if (registerRequested)
					{
						registerRequested = false;
						_linphoneService.Register();
					}
					else if (signOutRequest || defaultConfigRequest)
					{
					    processSignOut = true;
                    }
			        
					break;
				default:
					break;
			}

		    if (processSignOut)
		    {
                // Liz E. note: we want to perfomr different actions for logout and default configuration request.
                // If we are just logging out, then we need to not clear the account, just the password, 
                // and jump to the second page of the wizard.
                WizardPagepanel.Children.Clear();
                // VATRP - 1325, Go directly to VRS page
                _mainViewModel.OfferServiceSelection = false;
                _mainViewModel.ActivateWizardPage = true;

                signOutRequest = false;
                _mainViewModel.IsAccountLogged = false;
                CloseAnimated();
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsMessagingDocked = false;
                
                if (defaultConfigRequest)
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
        Configuration.ConfEntry.ACCOUNT_IN_USE, string.Empty);

                    defaultConfigRequest = false;
                    ServiceManager.Instance.AccountService.DeleteAccount(App.CurrentAccount);
                    ResetConfiguration();
                    App.CurrentAccount = null;
                    // VATRP - 1325, Go directly to VRS page
                    OnVideoRelaySelect(this, null);
                }
                else
                {
                    this.Wizard_HandleLogout();
                }
                signOutRequest = false;
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

        // VATRP-1899: This is a quick and dirty solution for POC. It will be funational, but not the end implementation we will want.
        private void SetToUserAgentView(bool isUserAgent)
        {
            System.Windows.Visibility visibility = System.Windows.Visibility.Visible;

            if (isUserAgent)
            {
                visibility = System.Windows.Visibility.Collapsed; 
            }
            // for the agent view, we want to hide all navigation buttons except the settings button.
            // we want to ensure that auto answer is set to true.
            this.BtnContacts.Visibility = visibility;
            this.BtnDialpad.Visibility = visibility;
            this.BtnRecents.Visibility = visibility;
            this.BtnResourcesView.Visibility = visibility;
            this.BtnSettings.Visibility = System.Windows.Visibility.Visible;
            // configure the other settings that we need for user agent:
        }

		private void OnNewAccountRegistered(string accountId)
		{
		    _mainViewModel.OfferServiceSelection = false;
		    _mainViewModel.ActivateWizardPage = false;
			LOG.Info(string.Format( "New account registered. Username -{0}. Host - {1} Port - {2}",
				App.CurrentAccount.RegistrationUser,
				App.CurrentAccount.ProxyHostname,
				App.CurrentAccount.ProxyPort));

            _mainViewModel.IsAccountLogged = true;
            // VATRP-1899: This is a quick and dirty solution for POC. It will be funational, but not the end implementation we will want.
            if ((App.CurrentAccount != null) && (!App.CurrentAccount.UserNeedsAgentView))
            {
                OpenAnimated();
                _mainViewModel.IsCallHistoryDocked = true;
                _mainViewModel.DialpadModel.UpdateProvider();
                SetToUserAgentView(false);
            }
            else
            {
                SetToUserAgentView(true);
            }
            this.SelfViewItem.IsChecked = App.CurrentAccount.ShowSelfView;
            ServiceManager.Instance.UpdateLoggedinContact();
		    ServiceManager.Instance.StartupLinphoneCore();
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
                case VATRPWindowType.SELF_VIEW:
                    if (this.ShowSelfPreviewItem.IsEnabled)
                        this.ShowSelfPreviewItem.IsChecked = bShow;
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

	    private void OnStateChanged(object sender, EventArgs e)
	    {
	        Window_StateChanged(sender, e);

	        if (_mainViewModel.ActiveCallModel != null)
	        {
	            switch (WindowState)
	            {
	                case WindowState.Normal:
                        this.SizeToContent = SizeToContent.WidthAndHeight;
                        UpdateLayout();
                        _mainViewModel.IsCallPanelDocked = true;
	                    break;
	                case WindowState.Minimized:
                        this.SizeToContent = SizeToContent.Manual;
                        if (_mainViewModel.ActiveCallModel.ActiveCall.CallState == VATRPCallState.InProgress)
                            _flashWindowHelper.FlashWindow(this);
	                    break;
	                case WindowState.Maximized:
                        
	                    break;
	            }
	        }
	        else
	        {
	            if (WindowState == WindowState.Normal)
	            {
	                _mainViewModel.IsCallPanelDocked = true;
	                this.SizeToContent = SizeToContent.WidthAndHeight;
                    UpdateLayout();
                    _mainViewModel.IsCallPanelDocked = false;
	            }
	        }
	    }

        private void OnLinphoneCoreStarted(object sender, EventArgs e)
        {
            ServiceManager.Instance.LinphoneService.OnCameraMuteEvent += OnCameraMuted;
        }

        private void OnCameraMuted(InfoEventBaseArgs args)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCameraMuted(args)));
                return;
            }

            var cameraMuteArgs = args as CameraMuteEventArgs;
            if (cameraMuteArgs != null)
            {
                if (_mainViewModel.ActiveCallModel.ActiveCall != null && 
                    (_mainViewModel.ActiveCallModel != null && 
                    _mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr == cameraMuteArgs.ActiveCall.NativeCallPtr))
                {
                    LOG.Info(string.Format("Remote side {0} camera", cameraMuteArgs.IsMuted ? "muted" : "unmuted"));
                    if (cameraMuteArgs.IsMuted)
                    {
                        ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(IntPtr.Zero, true);
                        ctrlCall.ctrlVideo.DrawCameraImage = true;
                    }
                    else
                    {
                        ctrlCall.ctrlVideo.DrawCameraImage = false;
                        ctrlCall.AddVideoControl();
                    }
                }
            }
        }
	}
}
