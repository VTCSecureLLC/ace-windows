using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;
using log4net;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public class XmlConfigurationService : IConfigurationService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(XmlConfigurationService));

        private const string file_name = "vatrp_config.xml";
        private String fileFullPath;
        private readonly ServiceManagerBase manager;

        private MyObservableCollection<XmlSection> sections;
        private readonly Timer deferredSaveTimer;
        private readonly XmlSerializer xmlSerializer;
        private bool _isStarting;
        private bool _isStarted;
        private bool _isStopping;
        private bool _isStopped;
        private readonly bool _saveAllowed = true;
        
        #endregion        public string PlainPassword { get; set; }
        public XmlConfigurationService(ServiceManagerBase manager, bool allowSave)
        {
            this.manager = manager;
            _isStarting = false;
            _isStarted = false;
            _saveAllowed = allowSave;

            this.xmlSerializer = new XmlSerializer(typeof(MyObservableCollection<XmlSection>));

            this.deferredSaveTimer = new Timer(1000) {AutoReset = false};
        }
        
        #region Properties
        public bool IsStarting { get { return _isStarting; } }
        public bool IsStarted { get { return _isStarted; } }

        public bool IsStopping
        {
            get { return _isStopping; }
        }

        public bool IsStopped
        {
            get { return _isStopped; }
        }
        
        #endregion

        #region Methods
        public bool Start()
        {
            bool ret = true;

            if (!_saveAllowed)
            {
                this.sections = new MyObservableCollection<XmlSection>();
                return true;
            }

            if (IsStarting || IsStopping)
                return false;

            this.fileFullPath = this.manager.BuildStoragePath(XmlConfigurationService.file_name);
            LOG.Debug(String.Format("Loading XML configuration from {0}", this.fileFullPath));

            try
            {
                if (!File.Exists(this.fileFullPath))
                {
                    LOG.Debug(String.Format("{0} doesn't exist, trying to create new one", this.fileFullPath));
                    File.Create(this.fileFullPath).Close();
                    // create xml declaration
                    this.sections = new MyObservableCollection<XmlSection>();
                    this.ImmediateSave();
                }

                using (StreamReader reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.sections = this.xmlSerializer.Deserialize(reader) as MyObservableCollection<XmlSection>;
                    }
                    catch (InvalidOperationException ie)
                    {
                        LOG.Error("Failed to load configuration", ie);

                        reader.Close();
                        File.Delete(this.fileFullPath);
                    }
                }

                this.deferredSaveTimer.Elapsed += delegate
                {
                    this.ImmediateSave();
                };
            }
            catch (Exception e)
            {
                LOG.Error("Failed to load configuration", e);
                _isStarting = false;
                _isStarted = false;
                ret = false;
            }
            _isStarting = false;
            _isStarted = true;
            _isStopped = false;
            return ret;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;

            if (IsStopped)
                return true;

            _isStopping = true;
            if (this.deferredSaveTimer.Enabled)
            {
                try
                {
                    this.deferredSaveTimer.Stop();
                    this.ImmediateSave();
                }
                catch (System.UnauthorizedAccessException e)
                {
                    LOG.Error(e);
                }
            }
            _isStopping = false;
            _isStopped = true;
            return true;
        }


        private void DeferredSave()
        {
            if (!_saveAllowed)
                return;
            this.deferredSaveTimer.Stop();
            this.deferredSaveTimer.Start();
        }
        private bool ImmediateSave()
        {
            if (!_saveAllowed)
                return true;

            lock (this.sections)
            {
                LOG.Debug("Saving configuration...");
                try
                {
                    using (StreamWriter writer = new StreamWriter(this.fileFullPath))
                    {
                        this.xmlSerializer.Serialize(writer, this.sections);
                        writer.Flush();
                        writer.Close();
                    }
                    return true;
                }
                catch (IOException ioe)
                {
                    LOG.Error("Failed to save configuration", ioe);
                }
            }
            return false;
        }

        #endregion        

        #region IConfigurationService

        public String Get(Configuration.ConfSection folder, Configuration.ConfEntry entry, String defaultValue)
        {
            String result = defaultValue;
            try
            {
                lock (this.sections)
                {
                    XmlSection section = this.sections.FirstOrDefault((x) => String.Equals(x.Name, folder.ToString()));
                    if (section != null)
                    {
                        XmlSectionEntry sectionEntry = section.Entries.FirstOrDefault((x) => String.Equals(x.Key, entry.ToString()));
                        if (sectionEntry != null)
                        {
                            result = sectionEntry.Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error(e);
            }

            return result;
        }

        public void SaveConfig()
        {
            if (this.deferredSaveTimer.Enabled)
            {
                try
                {
                    this.deferredSaveTimer.Stop();
                }
                catch (System.UnauthorizedAccessException e)
                {
                    LOG.Error(e);
                }
            }
            try
            {
                this.ImmediateSave();
            }
            catch (System.UnauthorizedAccessException e)
            {
                LOG.Error(e);
            }
            
        }

        public void Reset()
        {
            if (!_saveAllowed)
            {
                this.sections.Clear();
                return;
            }

            if (this.deferredSaveTimer.Enabled)
            {
                try
                {
                    this.deferredSaveTimer.Stop();
                }
                catch (System.UnauthorizedAccessException e)
                {
                    LOG.Error(e);
                }
            }

            try
            {
                this.sections.Clear();
                this.ImmediateSave();
            }
            catch (System.UnauthorizedAccessException e)
            {
                LOG.Error(e);
            }
        }

        public bool Set(Configuration.ConfSection folder, Configuration.ConfEntry entry, String value)
        {
            if (value == null)
            {
                return false;
            }

            try
            {
                lock (this.sections)
                {
                    XmlSection section = this.sections.FirstOrDefault((x) => String.Equals(x.Name, folder.ToString()));
                    if (section == null)
                    {
                        section = new XmlSection(folder.ToString());
                        this.sections.Add(section);
                    }

                    XmlSectionEntry sectionEntry = section.Entries.FirstOrDefault((x) => String.Equals(x.Key, entry.ToString()));
                    if (sectionEntry == null)
                    {
                        sectionEntry = new XmlSectionEntry(entry.ToString(), value);
                        section.Entries.Add(sectionEntry);
                    }
                    else
                    {
                        sectionEntry.Value = value;
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error("Failed to set value into registry", e);
                return false;
            }

            // Trigger
            EventHandlerTrigger.TriggerEvent(this.onConfigurationEvent, this, new ConfigurationEventArgs(folder, entry, value));

            this.DeferredSave();

            return true;
        }

        public int Get(Configuration.ConfSection folder, Configuration.ConfEntry entry, int defaultValue)
        {
            int result = defaultValue;
            String value = this.Get(folder, entry, defaultValue.ToString());
            if (Int32.TryParse(value, out result))
            {
                return result;
            }
            return defaultValue;
        }

        public bool Set(Configuration.ConfSection folder, Configuration.ConfEntry entry, int value)
        {
            return this.Set(folder, entry, value.ToString());
        }

        public bool Get(Configuration.ConfSection folder, Configuration.ConfEntry entry, bool defaultValue)
        {
            return Convert.ToBoolean(this.Get(folder, entry, defaultValue ? Boolean.TrueString : Boolean.FalseString));
        }

        public bool Set(Configuration.ConfSection folder, Configuration.ConfEntry entry, bool value)
        {
            return this.Set(folder, entry, value ? Boolean.TrueString : Boolean.FalseString);
        }

        public event EventHandler<ConfigurationEventArgs> onConfigurationEvent;

        #endregion

        [Serializable]
        [XmlRoot("section", ElementName="section")] 
        public class XmlSection : IEquatable<XmlSection>, IComparable<XmlSection>, INotifyPropertyChanged
        {
            private String name;
            private MyObservableCollection<XmlSectionEntry> entries;

            public XmlSection()
                :this(null)
            {
            }

            public XmlSection(String name)
            {
                this.name = name;
                this.entries = new MyObservableCollection<XmlSectionEntry>();
            }

            [XmlElement("entry")]
            public MyObservableCollection<XmlSectionEntry> Entries
            {
                get { return this.entries; }
                set
                {
                    this.entries = value;
                }
            }

            [XmlAttribute("name")]
            public String Name
            {
                get { return this.name; }
                set
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }

            #region IEquatable

            public bool Equals(XmlSection other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }
                return this.Name.Equals(other.Name);
            }

            #endregion

            #region IComparable

            public int CompareTo(XmlSection other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }
                return this.Name.CompareTo(other.Name);
            }

            #endregion

            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(String propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }

        [Serializable]
        [XmlRoot("entry", ElementName="entry")]
        public class XmlSectionEntry : IEquatable<XmlSectionEntry>, IComparable<XmlSectionEntry>, INotifyPropertyChanged
        {
            private String key;
            private String value;

            public XmlSectionEntry()
                :this(null, null)
            {
            }

            public XmlSectionEntry(String key, String value)
            {
                this.key = key;
                this.value = value;
            }

            [XmlAttribute("key")]
            public String Key
            {
                get { return this.key; }
                set
                {
                    this.key = value;
                    this.OnPropertyChanged("Key");
                }
            }

            [XmlAttribute("value")]
            public String Value
            {
                get { return this.value; }
                set
                {
                    this.value = value;
                    this.OnPropertyChanged("Value");
                }
            }

            #region IEquatable

            public bool Equals(XmlSectionEntry other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }
                return this.Key.Equals(other.Key);
            }

            #endregion

            #region IComparable

            public int CompareTo(XmlSectionEntry other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }
                return this.Key.CompareTo(other.Key);
            }

            #endregion

            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(String propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }
    }
}
