using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace VATRP.Core.Model
{
    [Table(Name = "ACCOUNTS")]
    public class VATRPAccount
    {
        #region Members

        private string _videoPreset;
        private string _preferredVideoId;
        private string _transport;
        private string _mediaEncryption;

		
        #endregion

        #region Properties

        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(50) NOT NULL ", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public string AccountID { get; set; }

        [Column]
        public string AuthID { get; set; }

        [Column]
        public string Username { get; set; }

        [Column]
        public string Password { get; set; }

        [Column]
        public string Provider { get; set; }
        
        [Column]
        public VATRPAccountType AccountType { get; set; }

        [Column]
        public bool AutoLogin { get; set; }

        [Column]
        public bool RememberPassword { get; set; }

        [Column]
        public string RegistrationUser { get; set; }

        [Column]
        public string RegistrationPassword { get; set; }

        [Column]
        public string ProxyHostname { get; set; }

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
        public bool EnubleSTUN { get; set; }

        [Column]
        public string STUNAddress { get; set; }

        [Column]
        public ushort STUNPort { get; set; }

        [Column] 
        public bool EnableAVPF { get; set; }

        [Column]
        public bool MuteMicrophone { get; set; }

        [Column]
        public bool MuteSpeaker { get; set; }

        [Column]
        public bool EchoCancel { get; set; }

        [Column]
        public bool VideoAutomaticallyStart { get; set; }

        [Column]
        public bool VideoAutomaticallyAccept { get; set; }

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

        public List<VATRPCodec> AudioCodecsList = new List<VATRPCodec>();
        public List<VATRPCodec> VideoCodecsList = new List<VATRPCodec>();

        #endregion

        #region Methods

        public VATRPAccount()
        {
            AccountID = Guid.NewGuid().ToString();
            ProxyPort = Configuration.LINPHONE_SIP_PORT;
            ProxyHostname = Configuration.LINPHONE_SIP_SERVER;
            Transport = "TCP";
            MediaEncryption = "Unencrypted";
            EnableAVPF = true;
            PreferredVideoId = "cif";
            STUNAddress = string.Empty;
            AuthID = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Provider = string.Empty;
            RegistrationUser = string.Empty;
            RegistrationPassword = string.Empty;
            DisplayName = string.Empty;
            VideoAutomaticallyStart = true;
            VideoAutomaticallyAccept = true;
            ShowSelfView = true;
            PreferredVideoId = string.Empty;
            VideoPreset = null;
            MuteMicrophone = false;
            MuteSpeaker = false;
            EchoCancel = false;
        }

        #endregion
    }
}
