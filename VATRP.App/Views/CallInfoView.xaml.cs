using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallInfoView.xaml
    /// </summary>
    public partial class CallInfoView
    {
        public CallInfoView()
            : base(VATRPWindowType.CALL_INFO_VIEW)
        {
            InitializeComponent();
            DataContext = new CallInfoViewModel();
        }

        internal void UpdateCallInfo(VATRPCall call)
        {
            if (call == null || call.CallState != VATRPCallState.StreamsRunning)
            {
                ResetCallInfoView();
                return;
            }

            ServiceManager.Instance.LinphoneService.LockCalls();
            LinphoneCallStats audioStats = ServiceManager.Instance.LinphoneService.GetCallAudioStats(call.NativeCallPtr);
            LinphoneCallStats videoStats = ServiceManager.Instance.LinphoneService.GetCallVideoStats(call.NativeCallPtr);
	
            IntPtr curparams = ServiceManager.Instance.LinphoneService.GetCallParams(call.NativeCallPtr);
            if (curparams != IntPtr.Zero)
            {

                var model = DataContext as CallInfoViewModel;
                if (model != null)
                {
                    int sipPort, rtpPort;
                    ServiceManager.Instance.LinphoneService.GetUsedPorts(out sipPort, out rtpPort);

                    model.SipPort = sipPort;
                    model.RtpPort = rtpPort;
                    bool has_video = LinphoneAPI.linphone_call_params_video_enabled(curparams);

                    MSVideoSizeDef size_received = LinphoneAPI.linphone_call_params_get_received_video_size(curparams);
                    MSVideoSizeDef size_sent = LinphoneAPI.linphone_call_params_get_sent_video_size(curparams);
                    IntPtr rtp_profile = LinphoneAPI.linphone_call_params_get_rtp_profile(curparams);

                    if (rtp_profile != IntPtr.Zero)
                    {
                        model.RtpProfile = Marshal.PtrToStringAnsi(rtp_profile);
                    }
                    model.AudioCodec = ServiceManager.Instance.LinphoneService.GetUsedAudioCodec(curparams);
                    var videoCodecName = ServiceManager.Instance.LinphoneService.GetUsedVideoCodec(curparams);

                    if (has_video && !string.IsNullOrWhiteSpace(videoCodecName))
                    {
                        model.VideoCodec = videoCodecName;
                        model.UploadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                            audioStats.upload_bandwidth + videoStats.upload_bandwidth, audioStats.upload_bandwidth,
                            videoStats.upload_bandwidth);

                        model.DownloadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                            audioStats.download_bandwidth + videoStats.download_bandwidth, audioStats.download_bandwidth,
                            videoStats.download_bandwidth);
                        model.ReceivingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, false);
                        model.SendingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, true);
                        var vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, false);
                        model.ReceivingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                        vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, true);
                        model.SendingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                    }
                    else
                    {
                        model.VideoCodec = "Not used";
                        model.ReceivingFPS = 0;
                        model.SendingFPS = 0;
                        model.UploadBandwidth = string.Format("a {0:0.##} kbit/s", audioStats.upload_bandwidth);
                        model.DownloadBandwidth = string.Format("a {0:0.##} kbit/s", audioStats.download_bandwidth);
                        model.ReceivingVideoResolution = "N/A";
                        model.SendingVideoResolution = "N/A";
                    }
                    switch ((LinphoneIceState) audioStats.ice_state)
                    {
                        case LinphoneIceState.LinphoneIceStateNotActivated:
                            model.IceSetup = "Not Activated";
                            break;
                        case LinphoneIceState.LinphoneIceStateFailed:
                            model.IceSetup = "Failed";
                            break;
                        case LinphoneIceState.LinphoneIceStateInProgress:
                            model.IceSetup = "In Progress";
                            break;
                        case LinphoneIceState.LinphoneIceStateHostConnection:
                            model.IceSetup = "Connected directly";
                            break;
                        case LinphoneIceState.LinphoneIceStateReflexiveConnection:
                            model.IceSetup = "Connected through NAT";
                            break;
                        case LinphoneIceState.LinphoneIceStateRelayConnection:
                            model.IceSetup = "Connected through a relay";
                            break;
                    }

                    switch (ServiceManager.Instance.LinphoneService.GetMediaEncryption(curparams))
                    {
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionNone:
                            model.MediaEncryption = "None";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionSRTP:
                            model.MediaEncryption = "SRTP";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionZRTP:
                            model.MediaEncryption = "ZRTP";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionDTLS:
                            model.MediaEncryption = "DTLS";
                            break;
                    }
                    model.CallQuality = LinphoneAPI.linphone_call_get_current_quality(call.NativeCallPtr);
                }
            }
            ServiceManager.Instance.LinphoneService.UnlockCalls();
        }

        private void ResetCallInfoView()
        {
            var model = DataContext as CallInfoViewModel;
            if (model != null)
            {
                model.AudioCodec = "Not used";
                model.VideoCodec = "Not used";
                model.ReceivingFPS = 0;
                model.SendingFPS = 0;
                model.DownloadBandwidth = "N/A";
                model.UploadBandwidth = "N/A";
                model.IceSetup = "N/A";
                model.MediaEncryption = "N/A";
                model.ReceivingVideoResolution = "N/A";
                model.SendingVideoResolution = "N/A";
                model.SipPort = 5060;
                model.RtpPort = 0;
                model.CallQuality = 0f;
            }
        }

    }
}
