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

        public abstract IContactsService ContactService
        {
            get;
        }

        public abstract IChatService ChatService
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

        public abstract IAccountService AccountService
        {
            get;
        }
        
        public abstract ILinphoneService LinphoneService
        {
            get;
        }
        public abstract IProviderService ProviderService
        {
            get;
        }

        public abstract System.Windows.Threading.Dispatcher Dispatcher
        {
            get;
        }
    }
}
