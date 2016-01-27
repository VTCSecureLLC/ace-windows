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
    public class ProviderService : IProviderService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ProviderService));
        private readonly ServiceManagerBase manager;
        private List<VATRPServiceProvider> providersList;
        private readonly string fileFullPath;
        private bool loading;
        private bool _isStarted;

        private bool _isStopping;
        private bool _isStopped;

        private readonly Timer deferredSaveTimer;

        private readonly XmlSerializer xmlSerializer;

        #endregion

        #region Methods

        public ProviderService(ServiceManagerBase manager)
        {
            this.manager = manager;
            this.loading = false;
            _isStarted = false;
            this.fileFullPath = this.manager.BuildStoragePath("providers.xml");

            this.deferredSaveTimer = new Timer(500) { AutoReset = false };
            this.deferredSaveTimer.Elapsed += delegate
            {
                this.manager.Dispatcher.Invoke((System.Threading.ThreadStart)delegate
                {
                    this.ImmediateSave();
                }, null);
            };
            this.xmlSerializer = new XmlSerializer(typeof(List<VATRPServiceProvider>));
        }

        private void DeferredSave()
        {
            this.deferredSaveTimer.Stop();
            this.deferredSaveTimer.Start();
        }

        private bool ImmediateSave()
        {
            lock (this.providersList)
            {
                try
                {
                    using (var writer = new StreamWriter(this.fileFullPath))
                    {
                        this.xmlSerializer.Serialize(writer, this.providersList);
                        writer.Flush();
                        writer.Close();
                    }
                    return true;
                }
                catch (IOException ioe)
                {
                    LOG.Error("Failed to save providers", ioe);
                }
                
            }
            return false;
        }
#endregion

        #region IService

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
        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;
        public bool Start()
        {
            if (IsStarting || IsStopping)
                return false;

            this.LoadProviders();
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
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

        #region IProviderService
        private void LoadProviders()
        {
            this.loading = true;

            try
            {
                if (!File.Exists(this.fileFullPath))
                {
                    File.Create(this.fileFullPath).Close();
                    // create xml declaration
                    this.providersList = new List<VATRPServiceProvider>();
                    this.ImmediateSave();
                }
                using (var reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.providersList = this.xmlSerializer.Deserialize(reader) as List<VATRPServiceProvider>;
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
        }

        public bool AddProvider(VATRPServiceProvider provider)
        {
            var query = from record in this.providersList where 
                            record.Label == provider.Label
                        select record;

            if (query.Any())
            {
                return true;
            }

            this.providersList.Insert(0, provider);
            this.DeferredSave();
            return true;
        }

        public bool DeleteProvider(VATRPServiceProvider provider)
        {
            if (string.IsNullOrEmpty(provider.Label))
                return false;

            var query = from record in this.providersList
                        where
                            record.Label == provider.Label 
                        select record;

            if (!query.Any())
            {
                return false;
            }

            this.providersList.Remove(provider);
            this.DeferredSave();
            return true;
        }

        public bool ContainsProvider(VATRPServiceProvider provider)
        {
            if (this.providersList == null)
                return false;

            var query = from record in this.providersList
                        where
                            record.Label == provider.Label 
                        select record;

            if (query.Any())
            {
                return true;
            }


            IEnumerable<VATRPServiceProvider> allItems = (from c in this.providersList
                                                          where c.Label == provider.Label 
                                                  select c).ToList();

            foreach (var c in allItems)
            {
                providersList.Remove(c);
            }

            this.DeferredSave();

            return false;
        }

        public int GetProvidersCount()
        {
            var VATRPproviders = this.providersList;
            if (VATRPproviders != null)
            {
                IEnumerable<VATRPServiceProvider> providers = (from c in VATRPproviders
                    select c).ToList();
                return providers.Count();
            }
            return 0;
        }

        public void ClearProvidersList()
        {
            this.providersList.Clear();
            this.DeferredSave();
        }

        public VATRPServiceProvider FindProvider(string providerLabel)
        {
            IEnumerable<VATRPServiceProvider> allproviders = (from c in this.providersList
                                                              where c.Label == providerLabel
                                                  select c).ToList();
            return allproviders.FirstOrDefault();
        }

        public VATRPServiceProvider FindProviderLooseSearch(string providerLabel)
        {
            foreach (VATRPServiceProvider provider in providersList)
            {
                if (provider.Label.Contains(providerLabel))
                {
                    return provider;
                }
            }
            return null;
        }

        public void Save()
        {
            this.DeferredSave();
        }
        #endregion


        public string[] GetProviderList()
        {
            if (this.providersList == null)
                return null;

            var providers = this.providersList.Select(c => c.Label).ToList();

            return providers.ToArray();
        }
    }
}
