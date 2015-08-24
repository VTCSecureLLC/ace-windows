using VATRP.Core.Interfaces;

namespace VATRP.Core.Services
{
    public abstract class ServiceManagerBase
    {
        public abstract string BuildStoragePath(string folder);

        public abstract IConfigurationService ConfigurationService
        {
            get;
        }

        public abstract IContactService ContactService
        {
            get;
        }

        public abstract IHistoryService HistoryService
        {
            get;
        }

        public abstract ISoundService SoundService
        {
            get;
        }

    }
}
