﻿using System;
using System.Collections.ObjectModel;
using VATRP.Core.Events;
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IContactsService : IVATRPservice
    {
        void AddContact(VATRPContact contact, string group);
        void RemoveContact(string id, bool isUserAction);
        void RemoveContacts();
        VATRPContact FindContact(ContactID contactID);
        VATRPContact FindContactByPhone(string phoneNumber);
        VATRPContact FindLoggedInContact();
        int ImportVCards(string vcardPath);
        void ExportVCards(string vcardPath);

        void AddLinphoneContact(string name, string username, string sipAddress, IntPtr vcard);
        void EditLinphoneContact(string oldname, string oldsipAddress, string newname, string newsipassdress);
        void DeleteLinphoneContact(string name, string sipAddress);

        event EventHandler<ContactEventArgs> ContactAdded;
        event EventHandler<EventArgs> ContactsChanged;
        event EventHandler<ContactRemovedEventArgs> ContactRemoved;
        event EventHandler<ContactStatusChangedEventArgs> ContactStatusChanged;
        event EventHandler<EventArgs> GroupsChanged;
        event EventHandler<ContactEventArgs> LoggedInContactUpdated;
        event EventHandler<EventArgs> ContactsLoadCompleted;
		
        ObservableCollection<VATRPContact> Contacts { get; }

    }
}
