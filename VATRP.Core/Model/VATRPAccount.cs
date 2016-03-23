using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Security;
using System.Security.Cryptography;
using VATRP.Core.Utilities;

namespace VATRP.Core.Model
{
    [Table(Name = "ACCOUNTS")]
    public class VATRPAccount
    {
        #region Members

        private string _videoPreset;
        private string _videoMailUri;
        private string _mwiUri;
        private string _preferredVideoId;
        private string _transport;
        private string _mediaEncryption;

        // I do not think that we want to write this out locally, we will be looking it up.
        //   Stashing it here for now so that we can reference it for the technical support sheet.
        //   Also, I think that we may wind up referencing this instead of copying the information over as we proceed.
        public ACEConfig configuration;
        #endregion

        #region Properties

        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(50) NOT NULL ", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public string AccountID { get; set; }

        [Column]
        public string AuthID { get; set; }

        [Column]
        public string Username { get; set; }

        // [ Column ]
        public string Password { get; set; }

        [Column]
        public string Provider { get; set; }
        
        [Column]
        public VATRPAccountType AccountType { get; set; }

//        [Column]
//        public bool AutoLogin { get; set; }

        [Column]
        public bool RememberPassword { get; set; }

        [Column]
        public string RegistrationUser { get; set; }

        [Column]
        public string RegistrationPassword { get; set; }

        [Column]
        public string ProxyHostname { get; set; }

        [Column]
        public bool UseOutboundProxy { get; set; }

        [Column]
        public string DisplayName { get; set; }

        [Column]
        public ushort ProxyPort { get; set; }

        [Column]
        public string Transport
        {
            get { return _transport; }
            set
            {
                _transport = !string.IsNullOrWhiteSpace(value) ? value : "TCP"; 
            }
        }

        [Column]
        public string MediaEncryption
        {
            get { return _mediaEncryption; }
            set
            {
                _mediaEncryption = !string.IsNullOrWhiteSpace(value) ? value : "Unencrypted";
            }
        }

        [Column]
        public bool EnableSTUN { get; set; }

        [Column]
        public string STUNAddress { get; set; }

        [Column]
        public ushort STUNPort { get; set; }

        [Column]
        public bool EnableICE { get; set; }

        [Column]
        public string ICEAddress { get; set; }

        [Column]
        public ushort ICEPort { get; set; }

        [Column] 
        public bool EnableAVPF { get; set; }

        [Column]
        public bool MuteMicrophone { get; set; }

        [Column]
        public bool MuteSpeaker { get; set; }

        [Column]
        public string SelectedCameraId { get; set; }

        [Column]
        public string SelectedSpeakerId { get; set; }

        [Column]
        public string SelectedMicrophoneId { get; set; }
        
        [Column]
        public bool EchoCancel { get; set; }

        [Column]
        public bool VideoAutomaticallyStart { get; set; }

        [Column]
        public bool VideoAutomaticallyAccept { get; set; }

        private bool enableVideo;
        [Column]
        public bool EnableVideo 
        {
            get
            {
                return true;
            }
            set
            {
                enableVideo = value;
            }
        }

        [Column]
        public bool ShowSelfView { get; set; }

        [Column]
        public string PreferredVideoId
        {
            get { return _preferredVideoId; }
            set
            {
                _preferredVideoId = !string.IsNullOrWhiteSpace(value) ? value : "cif";
            }
        }

        [Column]
        public string VideoPreset
        {
            get { return _videoPreset; }
            set
            {
                // Liz E. - linphone uses null to get the default preset. Allow null here.
                _videoPreset = value;
            }
        }

        [Column]
        public string VideoMailUri
        {
            get { return _videoMailUri; }
            set
            {
                // Liz E. - linphone uses null to get the default preset. Allow null here.
                _videoMailUri = value;
            }
        }

        [Column]
        public string MWIUri
        {
            get { return _mwiUri; }
            set
            {
                _mwiUri = value;
            }
        }

        //public List<VATRPCodec> AvailableAudioCodecsList = new List<VATRPCodec>();
        //public List<VATRPCodec> AvailableVideoCodecsList = new List<VATRPCodec>();
        public List<VATRPCodec> AudioCodecsList = new List<VATRPCodec>();
        public List<VATRPCodec> VideoCodecsList = new List<VATRPCodec>();

        [Column]
        public bool UserNeedsAgentView { get; set; }

        [Column]
        public int VideoMailCount { get; set; }

        [Column]
        public float PreferredFPS { get; set; }

        [Column]
        public bool EnableAdaptiveRate { get; set; }
        [Column]
		
        public string AdaptiveRateAlgorithm { get; set; }
        [Column]
        public int UploadBandwidth { get; set; }

        [Column]
        public int DownloadBandwidth { get; set; }

        [Column]
        public bool EnableQualityOfService { get; set; }

        [Column]
        public int SipDscpValue { get; set; }

        [Column]
        public int AudioDscpValue { get; set; }

        [Column]
        public int VideoDscpValue { get; set; }

        [Column]
        public bool EnableIPv6 { get; set; }

        #endregion

        #region Methods

        public VATRPAccount()
        {
            AccountID = Guid.NewGuid().ToString();
            ProxyPort = Configuration.LINPHONE_SIP_PORT;
            ProxyHostname = Configuration.LINPHONE_SIP_SERVER;
            Transport = "TCP";
            MediaEncryption = "Unencrypted";
            EnableAVPF = false;
            PreferredVideoId = "cif";
            STUNAddress = "stl.vatrp.net";
            STUNPort = 3478;
            AuthID = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Provider = string.Empty;
            RegistrationUser = string.Empty;
            RegistrationPassword = string.Empty;
            DisplayName = string.Empty;
            VideoAutomaticallyStart = true;
            VideoAutomaticallyAccept = true;
            EnableVideo = true;
            ShowSelfView = true;
            PreferredVideoId = string.Empty;
            VideoPreset = null;
            MuteMicrophone = false;
            MuteSpeaker = false;
            EchoCancel = true;
            UseOutboundProxy = false;
            VideoPreset = "high-fps";
            SelectedCameraId = string.Empty;
            SelectedMicrophoneId = string.Empty;
            SelectedSpeakerId = string.Empty;
            UserNeedsAgentView = false;
            VideoMailCount = 0;
            PreferredFPS = 30;
            EnableAdaptiveRate = true;
            UploadBandwidth = 1500;
            DownloadBandwidth = 1500;
            EnableQualityOfService = true;
            AdaptiveRateAlgorithm = "Simple";
            SipDscpValue = 24;
            AudioDscpValue = 46;
            VideoDscpValue = 46;
            EnableIPv6 = false;
        }

        public void StorePassword(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(Password))
            {
                DataProtectionHelper.WriteProtectedBytesToFile(filePath, Password);
                Password = "";
            }
        }

        public void ReadPassword(string filePath)
        {
            // if autologin is set, load the password for this user
            if (!string.IsNullOrEmpty(filePath)) 
            {
                // method below handles checking for file existance
                Password = DataProtectionHelper.ReadUnprotectedBytesFromProtectedFile(filePath);
            }
        }

        #endregion
    }
}
