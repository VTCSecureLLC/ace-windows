using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;
using log4net;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public class AccountService : IAccountService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof (AccountService));
        private readonly ServiceManagerBase manager;
        private List<VATRPAccount> accountsList;
        private string fileFullPath;
        private bool loading;
        private bool _isStarted;

        private bool _isStopping;
        private bool _isStopped;

        private readonly Timer deferredSaveTimer;

        private readonly XmlSerializer xmlSerializer;

        #endregion

        #region Methods
        public List<VATRPAccount> CodecList
        {
            get { return accountsList; }
        }

        public AccountService(ServiceManagerBase manager)
        {
            this.manager = manager;
            this.loading = false;
            _isStarted = false;
            this.fileFullPath = this.manager.BuildStoragePath("accounts.xml");

            this.deferredSaveTimer = new Timer(1000) { AutoReset = false };
            this.deferredSaveTimer.Elapsed += delegate
            {
                this.manager.Dispatcher.Invoke((System.Threading.ThreadStart)delegate
                {
                    this.ImmediateSave();
                }, null);
            };
            this.xmlSerializer = new XmlSerializer(typeof(List<VATRPAccount>));
        }

        private void DeferredSave()
        {
                this.deferredSaveTimer.Stop();
            this.deferredSaveTimer.Start();
        }

        private bool ImmediateSave()
        {
            lock (this.accountsList)
            {
                LOG.Debug("Saving accounts....");
                try
                {
                    using (var writer = new StreamWriter(this.fileFullPath))
                    {
                        this.xmlSerializer.Serialize(writer, this.accountsList);
                        writer.Flush();
                        writer.Close();
                    }
                    return true;
                }
                catch (IOException ioe)
                {
                    LOG.Error("Failed to save accounts", ioe);
                }
                
            }
            return false;
        }
#endregion

        #region IService

        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;

        public bool IsStopping
        {
            get { return _isStopping; }
        }

        public bool IsStopped
        {
            get { return _isStopped; }
        }
        public bool IsStarting
        {
            get
            {
                return loading;
            }
        }

        public bool IsStarted { get { return _isStarted; } }
        public bool Start()
        {
            if (IsStarting || IsStopping)
                return false;

            this.LoadAccounts();
            return true;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;
            _isStopping = true;
            _isStopped = true;

            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return true;
        }
        #endregion

        #region IAccountService
        private void LoadAccounts()
        {
            this.loading = true;
            LOG.Debug(string.Format("Loading accounts from {0}", this.fileFullPath));

            try
            {
                if (!File.Exists(this.fileFullPath))
                {
                    LOG.Debug(String.Format("{0} doesn't exist, trying to create new one", this.fileFullPath));
                    File.Create(this.fileFullPath).Close();
                    // create xml declaration
                    this.accountsList = new List<VATRPAccount>();
                    this.ImmediateSave();
                }
                using (StreamReader reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.accountsList = this.xmlSerializer.Deserialize(reader) as List<VATRPAccount>;
                    }
                    catch (InvalidOperationException ie)
                    {
                        LOG.Error("Failed to load history", ie);

                        reader.Close();
                        File.Delete(this.fileFullPath);
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error("Failed to load history", e);
                _isStarted = false;
                loading = false;
            }

            this.loading = false;
            _isStarted = true;

            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        public bool AddAccount(VATRPAccount account)
        {
            if (account.AccountType == VATRPAccountType.Unknown)
                return false;

            var query = from record in this.accountsList where 
                            record.AccountID == account.AccountID &&
                            record.AccountType == account.AccountType
                        select record;

            if (query.Any())
            {
                return true;
            }

            this.accountsList.Insert(0, account);
            this.DeferredSave();
            return true;
        }

        public bool DeleteAccount(VATRPAccount account)
        {
            if (string.IsNullOrEmpty(account.AccountID))
                return false;

            var query = from record in this.accountsList
                        where
                            record.AccountID == account.AccountID 
                        select record;

            if (!query.Any())
            {
                return false;
            }

            this.accountsList.Remove(account);
            this.DeferredSave();
            return true;
        }

        public bool ContainsAccount(VATRPAccount account)
        {
            var query = from record in this.accountsList
                        where
                            record.Username == account.Username &&
                            record.AccountType == account.AccountType
                        select record;

            if (query.Any())
            {
                return true;
            }


            IEnumerable<VATRPAccount> allItems = (from c in this.accountsList
                                                  where c.Username == account.Username && 
                                                  c.AccountType == account.AccountType
                                                  select c).ToList();

            foreach (var c in allItems)
            {
                accountsList.Remove(c);
            }

            this.DeferredSave();

            return false;
        }

        public int GetAccountsCount()
        {
            var VATRPAccounts = this.accountsList;
            if (VATRPAccounts != null)
            {
                IEnumerable<VATRPAccount> accounts = (from c in VATRPAccounts
                    select c).ToList();
                return accounts.Count();
            }
            return 0;
        }

        public void ClearAccounts()
        {
            this.accountsList.Clear();
            this.DeferredSave();
        }

        public VATRPAccount FindAccount(string accountUID)
        {
            var vatrpAccounts = this.accountsList;
            if (vatrpAccounts == null)
                return null;
            IEnumerable<VATRPAccount> allAccounts = (from c in vatrpAccounts
                where c.AccountID == accountUID
                select c).ToList();
            return allAccounts.FirstOrDefault();
        }

        public VATRPAccount FindAccount(string username, string password, string hostname)
        {
            var vatrpAccounts = this.accountsList;
            if (vatrpAccounts == null)
                return null;
            IEnumerable<VATRPAccount> allAccounts = (from c in vatrpAccounts
                                                     where (c.Username == username) &&
                                                     (c.Password == password) &&
                                                     (c.ProxyHostname == hostname)
                                                     select c).ToList();
            return allAccounts.FirstOrDefault();
        }

        public void Save()
        {
            this.DeferredSave();
        }
        #endregion

    }
}
