using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public sealed class ContactService : IContactsService
    {
        private readonly ServiceManagerBase manager;
        private ObservableCollection<VATRPContact> _contacts;
        private ObservableCollection<VATRPUserGroup> _groups;


        public event EventHandler<ContactEventArgs> ContactAdded;
        public event EventHandler<ContactRemovedEventArgs> ContactRemoved;

        public event EventHandler<ContactStatusChangedEventArgs> ContactStatusChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs> GroupsChanged;
        public event EventHandler<EventArgs> ContactsChanged;

        public bool IsLoaded { get; private set; }
        public ContactService(ServiceManagerBase manager)
        {
            this.manager = manager;
            IsLoaded = false;
        }

        private void RemoveGroupFromContactList(string _groupName)
        {
            lock (Contacts)
            {
                foreach (var contact in Contacts)
                {
                    if (contact.IsGroupExistInGroupList(_groupName))
                        contact.RemoveGroupFromGroupList(_groupName);
                }
            }
        }

        private void RemoveGroup(int removed)
        {
            lock (this.Groups)
            {
                foreach (var gr in Groups)
                {
                    if (gr.ID == removed)
                    {
                        Groups.Remove(gr);
                        return;
                    }
                }
            }
        }

        private VATRPUserGroup FindGroups(int removed)
        {
            lock (this.Groups)
            {
                foreach (var gr in Groups)
                {
                    if (gr.ID == removed)
                    {
                        return gr;
                    }
                }
            }
            return null;
        }

        public void AddContact(VATRPContact contact, string group)
        {
            Contacts.Add(contact);
            if (ContactAdded != null)
                ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
        }

        public VATRPContact FindContact(ContactID contactID)
        {
            if (contactID == null)
            {
                return null;
            }
            return this.FindContact(contactID.ID);
        }

        public VATRPContact FindContact(int contactID)
        {
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && (contact.ID == contactID.ToString()))
                    {
                        return contact;
                    }
                }
            }
            return null;
        }
        
        public VATRPContact FindLoggedInContact()
        {
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && contact.IsLoggedIn)
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        private VATRPContact FindContact(string id)
        {
            lock (this.Contacts)
            {
                foreach (var contact in this.Contacts)
                {
                    if ((contact != null) && (contact.ID == id))
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        public void RemoveContact(string id, bool isUserAction)
        {
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && (contact.ID == id))
                    {
                        this.Contacts.Remove(contact);
                        if (ContactRemoved != null)
                            ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), isUserAction));
                        break;
                    }
                }
            }
        }

        public void RenameContact(ContactID contactID, string nick)
        {

        }

        private void SaveContactsAsync(ObservableCollection<VATRPContact> observableCollection)
        {
            if (this.IsLoaded)
            {
                
            }
        }

        internal bool SearchContactIfExistOrCreateNew(ContactID contactID, out VATRPContact contact)
        {
            bool flag = false;
            contact = this.FindContact(contactID);
            if (contact == null)
            {
                flag = true;
                contact = new VATRPContact(contactID);
            }
            return flag;
        }

        internal void SetMail_inViewList(VATRPContact contact, bool mailVisible)
        {
            if (contact != null)
            {
                VATRPContact contact2 = this.FindContact(contact);
                if (contact2 != null)
                {
                    if (mailVisible)
                    {
                        contact2.UnreadMsgCount++;
                    }
                    else
                    {
                        contact2.UnreadMsgCount = 0;
                    }
                }
            }
        }

        public ObservableCollection<VATRPContact> Contacts
        {
            get
            {
                if (this._contacts == null)
                {
                    this._contacts = new ObservableCollection<VATRPContact>();
                }
                return this._contacts;
            }
            private set { this._contacts = value; }
        }

        public ObservableCollection<VATRPUserGroup> Groups
        {
            get
            {
                if (this._groups == null)
                {
                    this._groups = new ObservableCollection<VATRPUserGroup>();
                }
                return this._groups;
            }
            private set { this._groups = value; }
        }

        private string GetGroupName(int groupID)
        {
            lock (Groups)
            {
                foreach (var group in Groups)
                {
                    if (group.ID == groupID)
                        return group.Name;
                }
            }

            return groupID == 0 ? "All" : string.Empty;
        }

        public bool Start()
        {
            IsLoaded = true;
            return true;
        }

        public bool Stop()
        {
            return true;
        }
    }
}

