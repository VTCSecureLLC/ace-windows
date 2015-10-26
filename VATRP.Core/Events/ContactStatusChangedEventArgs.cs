using System;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ContactStatusChangedEventArgs : EventArgs
    {
        internal ContactStatusChangedEventArgs(VATRPContact _contact, UserStatus oldStatus)
        {
            this.OldStatus = oldStatus;
            this.contact = _contact;
        }

        public VATRPContact contact { get; set; }
        public UserStatus OldStatus { get; private set; }
    }
}

