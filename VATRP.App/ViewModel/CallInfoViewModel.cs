using System;
using System.Windows;

namespace VATRP.App.ViewModel
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
        #endregion

    }
}