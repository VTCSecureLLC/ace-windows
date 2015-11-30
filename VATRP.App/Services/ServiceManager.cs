using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using log4net;
using VATRP.Core.Extensions;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.Core.Interfaces;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Services
{
    internal class ServiceManager : ServiceManagerBase
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ServiceManager));
        private string _applicationDataPath;
        private static ServiceManager _singleton;
        private IConfigurationService _configurationService;
        private IContactsService _contactService;
        private IChatService _chatService;
        private IHistoryService _historyService;
        private ISoundService _soundService;
        private IAccountService _accountService;
        private ILinphoneService _linphoneService;
        private IProviderService _providerService;

        private WebClient _webClient;
        #endregion

        #region Event
        public delegate void NewAccountRegisteredDelegate(string accountId);
        public event NewAccountRegisteredDelegate NewAccountRegisteredEvent;
        #endregion

        public static ServiceManager Instance
        {
            get { return _singleton ?? (_singleton = new ServiceManager()); }
        }

        public string ApplicationDataPath
        {
            get
            {
                if (_applicationDataPath == null)
                {
                    String applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _applicationDataPath = Path.Combine(applicationData, "VATRP");
                    Directory.CreateDirectory(_applicationDataPath);
                }
                return _applicationDataPath;
            }
        }

        #region Overrides
        public override string BuildStoragePath(string folder)
        {
            return Path.Combine(ApplicationDataPath, folder);
        }

        public override IConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = new XmlConfigurationService(this, true)); }
        }

        public override IContactsService ContactService
        {
            get { return _contactService ?? (_contactService = new ContactService(this)); }
        }
        
        public override IChatService ChatService
        {
            get { return _chatService ?? (_chatService = new ChatsService(this)); }
        }

        public override IHistoryService HistoryService
        {
            get { return _historyService ?? (_historyService = new HistoryService(this)); }
        }

        public override ISoundService SoundService
        {
            get { return _soundService ?? (_soundService = new SoundService(this)); }
        }

        public override IAccountService AccountService
        {
            get { return _accountService ?? (_accountService = new AccountService(this)); }
        }

        public override ILinphoneService LinphoneService
        {
            get { return _linphoneService ?? (_linphoneService = new LinphoneService(this)); }
        }
        public override IProviderService ProviderService
        {
            get { return _providerService ?? (_providerService = new ProviderService(this)); }
        }

        public override System.Windows.Threading.Dispatcher Dispatcher
        {
            get
            {
                if (Application.Current != null) 
                    return Application.Current.Dispatcher;
                return null;
            }
        }
 #endregion

        #region Properties

        public IntPtr ActiveCallPtr { get; set; }

        internal bool ConfigurationStopped { get; set; }
        internal bool LinphoneCoreStopped { get; set; }
        internal bool HistoryServiceStopped { get; set; }

        internal bool AccountServiceStopped { get; set; }
        internal bool ProviderServiceStopped { get; set; }
        #endregion

        private ServiceManager()
        {
            ConfigurationStopped = true;
            LinphoneCoreStopped = true;
            HistoryServiceStopped = true;
            AccountServiceStopped = true;
            ProviderServiceStopped = true;
        }

        public bool Initialize()
        {
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += CredentialsReceived;

            this.ConfigurationService.ServiceStarted += OnConfigurationServiceStarted;
            this.AccountService.ServiceStarted += OnAccountsServiceStarted;
            this.ProviderService.ServiceStarted += OnProviderServiceStarted;
            this.HistoryService.ServiceStarted += OnHistoryServiceStarted;
            this.LinphoneService.ServiceStarted += OnLinphoneServiceStarted;

            this.ConfigurationService.ServiceStopped += OnConfigurationServiceStopped;
            this.AccountService.ServiceStarted += OnAccountsServiceStopped;
            this.ProviderService.ServiceStopped += OnProviderServiceStopped;
            this.HistoryService.ServiceStopped += OnHistoryServiceStopped;
            this.LinphoneService.ServiceStopped += OnLinphoneServiceStopped;
            return true;
        }

        private void OnConfigurationServiceStarted(object sender, EventArgs args)
        {
            App.CurrentAccount = LoadActiveAccount();
        }
        private void OnAccountsServiceStarted(object sender, EventArgs args)
        {
            App.CurrentAccount = LoadActiveAccount();
            AccountServiceStopped = false;
        }
        private void OnProviderServiceStarted(object sender, EventArgs args)
        {
            UpdateProvidersList();
            ProviderServiceStopped = false;
        }
        private void OnLinphoneServiceStarted(object sender, EventArgs args)
        {
            LinphoneCoreStopped = false;
            HistoryService.Start();
            ContactService.Start();
        }

        private void OnHistoryServiceStarted(object sender, EventArgs args)
        {
            HistoryServiceStopped = false;
        }

        private void OnConfigurationServiceStopped(object sender, EventArgs args)
        {
            ConfigurationStopped = true;
        }
        private void OnAccountsServiceStopped(object sender, EventArgs args)
        {
            AccountServiceStopped = true;
        }
        private void OnProviderServiceStopped(object sender, EventArgs args)
        {
            ProviderServiceStopped = true;
        }
        private void OnLinphoneServiceStopped(object sender, EventArgs args)
        {
            LinphoneCoreStopped = true;
        }

        private void OnHistoryServiceStopped(object sender, EventArgs args)
        {
            HistoryServiceStopped = true;
        }

        internal bool WaitForServiceCompletion(int secToWait)
        {
            int nWait = secToWait*10;
            while (nWait-- > 0)
            {
                if (LinphoneCoreStopped && HistoryServiceStopped && AccountServiceStopped && ProviderServiceStopped &&
                    ConfigurationStopped)
                    return true;
                System.Threading.Thread.Sleep(100);
            }
            return false;
        }
        internal bool Start()
        {
            LOG.Info("Starting services...");
            var retVal = true;
            retVal &= ConfigurationService.Start();
            retVal &= AccountService.Start();
            retVal &= SoundService.Start();
            retVal &= ProviderService.Start();
            return retVal;
        }

        public bool UpdateLinphoneConfig()
        {
            if (App.CurrentAccount == null)
            {
                LOG.Warn("Can't update linphone config. Account is no configured");
                return false;
            }

            this.LinphoneService.LinphoneConfig.ProxyHost = string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) ?
                Configuration.LINPHONE_SIP_SERVER : App.CurrentAccount.ProxyHostname;
            LinphoneService.LinphoneConfig.ProxyPort = App.CurrentAccount.ProxyPort;
            LinphoneService.LinphoneConfig.UserAgent = ConfigurationService.Get(Configuration.ConfSection.LINPHONE, Configuration.ConfEntry.LINPHONE_USERAGENT,
                    Configuration.LINPHONE_USERAGENT);
            LinphoneService.LinphoneConfig.AuthID = App.CurrentAccount.AuthID;
            LinphoneService.LinphoneConfig.Username = App.CurrentAccount.RegistrationUser;
            LinphoneService.LinphoneConfig.DisplayName = App.CurrentAccount.DisplayName;
            LinphoneService.LinphoneConfig.Password = App.CurrentAccount.RegistrationPassword;
            string[] transportList = {"UDP", "TCP", "DTLS", "TLS"};

            if (transportList.All(s => App.CurrentAccount.Transport != s))
            {
                App.CurrentAccount.Transport = "TCP";
                AccountService.Save();
            }

            LinphoneService.LinphoneConfig.Transport = App.CurrentAccount.Transport;
            LinphoneService.LinphoneConfig.EnableSTUN = App.CurrentAccount.EnubleSTUN;
            LinphoneService.LinphoneConfig.STUNAddress = App.CurrentAccount.STUNAddress;
            LinphoneService.LinphoneConfig.STUNPort = App.CurrentAccount.STUNPort;
            LinphoneService.LinphoneConfig.MediaEncryption = GetMediaEncryptionText(App.CurrentAccount.MediaEncryption);
#if !DEBUG
            LinphoneService.LinphoneConfig.EnableAVPF = true;
#else
            LinphoneService.LinphoneConfig.EnableAVPF = App.CurrentAccount.EnableAVPF;
#endif
            LOG.Info("Linphone service configured for account: " + App.CurrentAccount.RegistrationUser);
            return true;
        }

        internal void Stop()
        {
            LOG.Info("Stopping services...");
            HistoryService.Stop();
            ConfigurationService.Stop();
            LinphoneService.Unregister(true);
            LinphoneService.Stop();
            AccountService.Stop();
            ProviderService.Stop();
        }

        internal bool RequestLinphoneCredentials(string username, string passwd)
        {
            bool retValue = true;
            var requestLink = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.REQUEST_LINK, Configuration.DEFAULT_REQUEST);
            var request = (HttpWebRequest)WebRequest.Create(requestLink);

            var postData = string.Format("{{ \"user\" : {{ \"email\" : \"{0}\", \"password\" : \"{1}\" }} }}", username, passwd);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                var response = (HttpWebResponse) request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var responseString = new StreamReader(responseStream).ReadToEnd();
                    ParseHttpResponse(responseString);
                }
                else
                {
                    retValue = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return retValue;
        }

        private void ParseHttpResponse(string response)
        {
            // parse response stream, 
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, string>>(response);

            if (dict.ContainsKey("pbx_extension"))
            {
                App.CurrentAccount.RegistrationUser = dict["pbx_extension"];
            }
            if (dict.ContainsKey("auth_token"))
            {
                App.CurrentAccount.RegistrationPassword = dict["auth_token"];
            }

            if (UpdateLinphoneConfig())
            {
                if (LinphoneService.Start(true))
                    LinphoneService.Register();
            }
        }

        private void CredentialsReceived(object sender, DownloadStringCompletedEventArgs e)
        {
            
        }

        internal VATRPAccount LoadActiveAccount()
        {
            var accountUID = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, "");
            if (string.IsNullOrEmpty(accountUID))
                return null;
            var account = AccountService.FindAccount(accountUID);
            return account;
        }

        private void UpdateProvidersList()
        {
            // this list may be filled later

            string[] labels = { "Sorenson VRS", "Purple VRS", "ZVRS", "Convo Relay" };
            foreach (var label in labels)
            {
                if (ProviderService.FindProvider(label) == null)
                    ProviderService.AddProvider(new VATRPServiceProvider()
                    {
                        Label = label
                    });
            }
        }

        internal static void LogError(string message, Exception ex)
        {
            LOG.Error(string.Format( "Exception occurred in {0}: {1}", message, ex.Message));
        }

        internal void SaveAccountSettings()
        {
            if (App.CurrentAccount == null)
                return;

            AccountService.Save();
        }

        internal bool StartLinphoneService()
        {
            if (App.CurrentAccount == null)
                return false;
            if (!LinphoneService.Start(true))
                return false;
            
            if (App.CurrentAccount.AudioCodecsList.Count > 0)
                LinphoneService.UpdateNativeCodecs(App.CurrentAccount, CodecType.Audio);
            else
                LinphoneService.FillCodecsList(App.CurrentAccount, CodecType.Audio);

            if (App.CurrentAccount.VideoCodecsList.Count > 0)
                LinphoneService.UpdateNativeCodecs(App.CurrentAccount, CodecType.Video);
            else
                LinphoneService.FillCodecsList(App.CurrentAccount, CodecType.Video);

            LinphoneService.UpdateNetworkingParameters(App.CurrentAccount);
            ApplyAVPFChanges();
            ApplyDtmfOnSIPInfoChanges();
            ApplyMediaSettingsChanges();
            return true;
        }

        internal void Register()
        {
            LinphoneService.Register();
        }

        internal void RegisterNewAccount(string id)
        {
            if (NewAccountRegisteredEvent != null)
                NewAccountRegisteredEvent(id);
        }

        internal void ApplyCodecChanges()
        {
            var retValue = LinphoneService.UpdateNativeCodecs(App.CurrentAccount,
                CodecType.Audio);

            retValue &= LinphoneService.UpdateNativeCodecs(App.CurrentAccount, CodecType.Video);

            if (!retValue)
                SaveAccountSettings();
        }

        internal void ApplyNetworkingChanges()
        {
            LinphoneService.UpdateNetworkingParameters(App.CurrentAccount);
        }

        internal void ApplyAVPFChanges()
        {
            var mode = LinphoneAVPFMode.LinphoneAVPFEnabled;
#if DEBUG
            if (!this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true))
            {
                mode = LinphoneAVPFMode.LinphoneAVPFDisabled;
            }
#endif
            LinphoneService.SetAVPFMode(mode);
        }

        internal void ApplyDtmfOnSIPInfoChanges()
        {
            bool val = this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, false);
            LinphoneService.SendDtmfAsSipInfo(val);
        }

        private LinphoneMediaEncryption GetMediaEncryptionText(string s)
        {

            switch (s)
            {
                case "Encrypted (DTLS)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionDTLS;
                case "Encrypted (SRTP)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionSRTP;
                case "Encrypted (ZRTP)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionZRTP;
                default:
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionNone;
            }
        }

        internal void ApplyMediaSettingsChanges()
        {
            LinphoneService.LinphoneConfig.MediaEncryption = GetMediaEncryptionText(App.CurrentAccount.MediaEncryption);
            LinphoneService.UpdateMediaSettings(App.CurrentAccount);
            bool bEnable = this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ENABLE_ADAPTIVE_RATE_CTRL, true);
            LinphoneService.EnableAdaptiveRateControl(bEnable);
        }

        internal void UpdateLoggedinContact()
        {
            if (App.CurrentAccount == null || !App.CurrentAccount.Username.NotBlank())
                return;
            VATRPContact contact = this.ContactService.FindLoggedInContact();
            if (contact == null)
            {
                contact = new VATRPContact(new ContactID(App.CurrentAccount.Username, IntPtr.Zero));
                contact.IsLoggedIn = true;
                contact.Fullname = App.CurrentAccount.Username;
                contact.DisplayName = App.CurrentAccount.DisplayName;
                contact.Initials = contact.Fullname.Substring(0, 1).ToUpper();
                this.ContactService.AddContact(contact, string.Empty);
            }
            else
            {
                contact.IsLoggedIn = false;
            }
        }

        public bool IsRttAvailable
        {
            get
            {
                return (ConfigurationService != null &&
                        (ActiveCallPtr != IntPtr.Zero && ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                            Configuration.ConfEntry.USE_RTT, true)));
            } 
        }
    }
}