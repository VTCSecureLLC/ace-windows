using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ContactEventArgs : EventArgs
    {
        public ContactEventArgs(ContactID id)
        {
            Contact = id;
        }

        public ContactID Contact { get; private set; }
    }
}

