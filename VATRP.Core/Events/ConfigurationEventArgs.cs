using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ConfigurationEventArgs : VATRPEventArgs
    {
        private readonly Configuration.ConfSection section;
        private readonly Configuration.ConfEntry entry;
        private readonly object value;

        public ConfigurationEventArgs(Configuration.ConfSection section, Configuration.ConfEntry entry, object value)
        {
            this.section = section;
            this.entry = entry;
            this.value = value;
        }

        public Configuration.ConfSection Folder
        {
            get { return this.section; }
        }

        public Configuration.ConfEntry Entry
        {
            get { return this.entry; }
        }

        public object Value
        {
            get { return this.value; }
        }
    }
}
