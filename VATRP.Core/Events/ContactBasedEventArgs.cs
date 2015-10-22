using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ContactBasedEventArgs : EventArgs
    {
        private ContactID _contact;
        public ContactBasedEventArgs(ContactID id)
        {
            _contact = id;
        }

        public ContactID contactId
        {
            get
            {
                return _contact;
            }
        }
    }
}

