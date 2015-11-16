using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallInfoViewModel : ViewModelBase
    {
        #region Members
        private string _audioCodec = "None";
        private string _videoCodec;
        private int _sipPort = 5060;
        private int _rtpPort = 7000;
        private string _sendingVideoResolution = string.Empty;
        private string _receivingVideoResolution = string.Empty;
        private float _sendingFps = 0.0f;
        private float _receivingFps = 0.0f;
        private string _uploadBandwidth = string.Empty;
        private string _downloadBandwidth = string.Empty;
        private string _iceSetup = string.Empty;
        private string _mediaEncryption = string.Empty;
        private string _rtpProfile;
        private float _quality;
        private bool _avpfenabled = false;
        #endregion

        public CallInfoViewModel()
        {

        }

        #region Properties

        public string AudioCodec
        {
            get { return _audioCodec; }
            set
            {
                _audioCodec = value;
                OnPropertyChanged("AudioCodec");
            }
        }

        public string VideoCodec
        {
            get { return _videoCodec; }
            set
            {
                _videoCodec = value;
                OnPropertyChanged("VideoCodec");
            }
        }

        public int SipPort
        {
            get { return _sipPort; }
            set
            {
                _sipPort = value;
                OnPropertyChanged("SipPort");
            }
        }

        public int RtpPort
        {
            get { return _rtpPort; }
            set
            {
                _rtpPort = value;
                OnPropertyChanged("RtpPort");
            }
        }

        public string SendingVideoResolution
        {
            get { return _sendingVideoResolution; }
            set
            {
                _sendingVideoResolution = value;
                OnPropertyChanged("SendingVideoResolution");
            }
        }

        public string ReceivingVideoResolution
        {
            get { return _receivingVideoResolution; }
            set
            {
                _receivingVideoResolution = value;
                OnPropertyChanged("ReceivingVideoResolution");
            }
        }

        public float SendingFPS
        {
            get { return _sendingFps; }
            set
            {
                _sendingFps = value;
                OnPropertyChanged("SendingFPS");
            }
        }

        public float ReceivingFPS
        {
            get { return _receivingFps; }
            set
            {
                _receivingFps = value;
                OnPropertyChanged("ReceivingFPS");
            }
        }

        public string UploadBandwidth
        {
            get { return _uploadBandwidth; }
            set
            {
                _uploadBandwidth = value;
                OnPropertyChanged("UploadBandwidth");
            }
        }

        public string DownloadBandwidth
        {
            get { return _downloadBandwidth; }
            set
            {
                _downloadBandwidth = value;
                OnPropertyChanged("DownloadBandwidth");
            }
        }

        public string IceSetup
        {
            get { return _iceSetup; }
            set
            {
                _iceSetup = value;
                OnPropertyChanged("IceSetup");
            }
        }

        public string MediaEncryption
        {
            get { return _mediaEncryption; }
            set
            {
                _mediaEncryption = value;
                OnPropertyChanged("MediaEncryption");
            }
        }

        public string RtpProfile
        {
            get { return _rtpProfile; }
            set
            {
                _rtpProfile = value;
                OnPropertyChanged("RtpProfile");
            }
        }
        public float CallQuality
        {
            get { return _quality; }
            set
            {
                _quality = value;
                OnPropertyChanged("CallQuality");
            }
        }
        public bool AVPFEnabled
        {
            get { return _avpfenabled; }
            set
            {
                _avpfenabled = value;
                OnPropertyChanged("AVPFEnabled");
            }
        }

        #endregion

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

                    int sipPort, rtpPort;
                    ServiceManager.Instance.LinphoneService.GetUsedPorts(out sipPort, out rtpPort);

                    SipPort = sipPort;
                    RtpPort = rtpPort;
                    bool has_video = LinphoneAPI.linphone_call_params_video_enabled(curparams);

                    MSVideoSizeDef size_received = LinphoneAPI.linphone_call_params_get_received_video_size(curparams);
                    MSVideoSizeDef size_sent = LinphoneAPI.linphone_call_params_get_sent_video_size(curparams);
                    IntPtr rtp_profile = LinphoneAPI.linphone_call_params_get_rtp_profile(curparams);

                    if (rtp_profile != IntPtr.Zero)
                    {
                        RtpProfile = Marshal.PtrToStringAnsi(rtp_profile);
                    }
                    AudioCodec = ServiceManager.Instance.LinphoneService.GetUsedAudioCodec(curparams);

                    int avpf_mode = ServiceManager.Instance.LinphoneService.GetAVPFMode();

                    if (avpf_mode == 0)
                    {
                        AVPFEnabled = false;
                    }
                    else if (avpf_mode == 1)
                    {
                        AVPFEnabled = true;
                    }


                    var videoCodecName = ServiceManager.Instance.LinphoneService.GetUsedVideoCodec(curparams);


                    if (has_video && !string.IsNullOrWhiteSpace(videoCodecName))
                    {
                        VideoCodec = videoCodecName;
                        UploadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                            audioStats.upload_bandwidth + videoStats.upload_bandwidth, audioStats.upload_bandwidth,
                            videoStats.upload_bandwidth);

                        DownloadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                            audioStats.download_bandwidth + videoStats.download_bandwidth, audioStats.download_bandwidth,
                            videoStats.download_bandwidth);
                        ReceivingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, false);
                        SendingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, true);
                        var vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, false);
                        ReceivingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                        vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, true);
                        SendingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                    }
                    else
                    {
                        VideoCodec = "Not used";
                        ReceivingFPS = 0;
                        SendingFPS = 0;
                        UploadBandwidth = string.Format("a {0:0.##} kbit/s", audioStats.upload_bandwidth);
                        DownloadBandwidth = string.Format("a {0:0.##} kbit/s", audioStats.download_bandwidth);
                        ReceivingVideoResolution = "N/A";
                        SendingVideoResolution = "N/A";
                    }
                    switch ((LinphoneIceState)audioStats.ice_state)
                    {
                        case LinphoneIceState.LinphoneIceStateNotActivated:
                            IceSetup = "Not Activated";
                            break;
                        case LinphoneIceState.LinphoneIceStateFailed:
                            IceSetup = "Failed";
                            break;
                        case LinphoneIceState.LinphoneIceStateInProgress:
                            IceSetup = "In Progress";
                            break;
                        case LinphoneIceState.LinphoneIceStateHostConnection:
                            IceSetup = "Connected directly";
                            break;
                        case LinphoneIceState.LinphoneIceStateReflexiveConnection:
                            IceSetup = "Connected through NAT";
                            break;
                        case LinphoneIceState.LinphoneIceStateRelayConnection:
                            IceSetup = "Connected through a relay";
                            break;
                    }

                    switch (ServiceManager.Instance.LinphoneService.GetMediaEncryption(curparams))
                    {
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionNone:
                            MediaEncryption = "None";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionSRTP:
                            MediaEncryption = "SRTP";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionZRTP:
                            MediaEncryption = "ZRTP";
                            break;
                        case LinphoneMediaEncryption.LinphoneMediaEncryptionDTLS:
                            MediaEncryption = "DTLS";
                            break;
                    }
                    CallQuality = LinphoneAPI.linphone_call_get_current_quality(call.NativeCallPtr);
                }
            ServiceManager.Instance.LinphoneService.UnlockCalls();
        }

        private void ResetCallInfoView()
        {
            AudioCodec = "Not used";
            VideoCodec = "Not used";
            ReceivingFPS = 0;
            SendingFPS = 0;
            DownloadBandwidth = "N/A";
            UploadBandwidth = "N/A";
            IceSetup = "N/A";
            MediaEncryption = "N/A";
            ReceivingVideoResolution = "N/A";
            SendingVideoResolution = "N/A";
            SipPort = 5060;
            RtpPort = 0;
            CallQuality = 0f;
            AVPFEnabled = false;
        }

        internal void OnCallStatisticsChanged(VATRPCall call, LinphoneCallStats stats)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnCallStatisticsChanged(call, stats)));
                return;
            }
            
            UpdateCallInfo(call);
        }
    }
}