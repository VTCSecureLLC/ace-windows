using log4net;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public partial class ContactService : IContactService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (ContactService));

        private readonly ServiceManagerBase manager;

        public ContactService(ServiceManagerBase manager)
        {
            this.manager = manager;
        }

        #region IVATRPService

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        #endregion

        #region IContactService

        public string GetContactDisplayName(VATRPContact contact, string username)
        {
            if (contact != null)
                return contact.DisplayName;

            string dn = username;

            var c = FindContactByUsername(username);
            if (c != null)
            {
                dn = c.DisplayName ?? c.FullName;

            }
            return dn;
        }
        
        public VATRPContact FindContactByUsername(string useraname)
        {
            return null;
        }

        public VATRPContact FindContactId(string id)
        {
            return null;
        }

        #endregion
    }
}
