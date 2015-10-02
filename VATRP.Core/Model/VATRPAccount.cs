using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace VATRP.Core.Model
{
    [Table(Name = "ACCOUNTS")]
    public class VATRPAccount
    {
        #region Properties

        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(50) NOT NULL ", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public string AccountID { get; set; }

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
        public string Transport { get; set; }

        [Column]
        public bool EnubleSTUN { get; set; }

        [Column]
        public string STUNAddress { get; set; }

        [Column]
        public ushort STUNPort { get; set; }

        [Column] 
        public bool EnableAVPF { get; set; }
        [Column]
        public string PreferredVideoId { get; set; }

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
            EnableAVPF = true;
            PreferredVideoId = "vga";
            STUNAddress = string.Empty;
        }

        #endregion
    }
}
