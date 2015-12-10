using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Structs;

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
        public event EventHandler<EventArgs> ContactsLoadCompleted;
        public event EventHandler<ContactEventArgs> LoggedInContactUpdated;
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

        private void LoadLinphoneContacts()
        {
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr contactsPtr = LinphoneAPI.linphone_core_get_friend_list(manager.LinphoneService.LinphoneCore);
            if (contactsPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(contactsPtr, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(curStruct.data);
                        if (addressPtr == IntPtr.Zero)
                            continue;

                        string dn = "";
                        IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            dn = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string un = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            un = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string host = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            host = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        if (!string.IsNullOrWhiteSpace(un))
                        {

                            var cfgSipaddress = string.Format("{0}@{1}", un, host);
                            VATRPContact contact = new VATRPContact(new ContactID(cfgSipaddress, IntPtr.Zero))
                            {
                                DisplayName = dn,
                                Fullname = dn.NotBlank() ? dn : un,
                                Gender = "male",
                                SipUsername = un,
                                RegistrationName = cfgSipaddress,
                                IsLinphoneContact = true
                            };

                            Contacts.Add(contact);
                        }
                    }
                    contactsPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

            }
           
            if ( ContactsLoadCompleted != null)
                ContactsLoadCompleted(this, EventArgs.Empty);
        }
        
        public void AddLinphoneContact(string name, string username,  string sipAddress)
        {
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            sipAddress.TrimSipPrefix();

            var fqdn = string.Format("{0} <sip:{1}>", name, sipAddress);
            IntPtr friendPtr = LinphoneAPI.linphone_friend_new_with_address(fqdn);
            if (friendPtr != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_add_friend(manager.LinphoneService.LinphoneCore, friendPtr);
                LinphoneAPI.linphone_friend_enable_subscribes(friendPtr, false);
                VATRPContact contact = new VATRPContact(new ContactID(sipAddress, IntPtr.Zero))
                {
                    DisplayName = name,
                    Fullname = name,
                    Gender = "male",
                    SipUsername = username,
                    RegistrationName = sipAddress,
                    IsLinphoneContact = true
                };
                Contacts.Add(contact);
                if (ContactAdded != null)
                    ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
            }
        }

        public void EditLinphoneContact(string oldname, string oldsipAddress, string newname, string newsipassdress)
        {
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr contactsPtr = LinphoneAPI.linphone_core_get_friend_list(manager.LinphoneService.LinphoneCore);
            if (contactsPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(contactsPtr, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(curStruct.data);
                        if (addressPtr != IntPtr.Zero)
                        {

                            string dn = "";
                            IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
                            if (tmpPtr != IntPtr.Zero)
                            {
                                dn = Marshal.PtrToStringAnsi(tmpPtr);
                            }

                            if (dn == oldname)
                            {
                                string un = "";
                                tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                                if (tmpPtr != IntPtr.Zero)
                                {
                                    un = Marshal.PtrToStringAnsi(tmpPtr);
                                }

                                string host = "";
                                tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
                                if (tmpPtr != IntPtr.Zero)
                                {
                                    host = Marshal.PtrToStringAnsi(tmpPtr);
                                }

                                var fqdn = string.Format("sip:{0}@{1}", un, host);
                                var cfgSipAddress = string.Format("{0}@{1}", un, host);


                                newsipassdress.TrimSipPrefix();

                                var newfqdn = string.Format("sip:{0}", newsipassdress);

                                int port;
                                VATRPCall.ParseSipAddress(newsipassdress, out un, out host, out port);

                                if (string.Compare(oldsipAddress, cfgSipAddress, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    VATRPContact contact = FindContact(new ContactID(cfgSipAddress, IntPtr.Zero));
                                    if (contact != null)
                                    {
                                        
                                        tmpPtr =
                                            LinphoneAPI.linphone_core_create_address(
                                                manager.LinphoneService.LinphoneCore, string.Format("{0} <{1}>", newname, newfqdn));
                                        if (tmpPtr != IntPtr.Zero)
                                        {
                                            LinphoneAPI.linphone_friend_edit(curStruct.data);
                                            LinphoneAPI.linphone_friend_set_name(curStruct.data, newname);

                                            LinphoneAPI.linphone_friend_set_address(curStruct.data, tmpPtr);
                                            LinphoneAPI.linphone_friend_done(curStruct.data);
                                            contact.DisplayName = newname;
                                            contact.Fullname = newname;
                                            contact.SipUsername = un;
                                            contact.RegistrationName = newsipassdress;
                                            contact.ID = newsipassdress;
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    contactsPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);
            }
        }

        public void DeleteLinphoneContact(string name, string sipAddress)
        {
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr contactsPtr = LinphoneAPI.linphone_core_get_friend_list(manager.LinphoneService.LinphoneCore);
            if (contactsPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(contactsPtr, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(curStruct.data);
                        if (addressPtr != IntPtr.Zero)
                        {
                            string dn = "";
                            IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
                            if (tmpPtr != IntPtr.Zero)
                            {
                                dn = Marshal.PtrToStringAnsi(tmpPtr);
                            }

                            if (dn == name)
                            {
                                string un = "";
                                tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                                if (tmpPtr != IntPtr.Zero)
                                {
                                    un = Marshal.PtrToStringAnsi(tmpPtr);
                                }

                                string host = "";
                                tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
                                if (tmpPtr != IntPtr.Zero)
                                {
                                    host = Marshal.PtrToStringAnsi(tmpPtr);
                                }

                                var fqdn = string.Format("sip:{0}@{1}", un, host);
                                var cfgSipAddress = string.Format("{0}@{1}", un, host);

                                sipAddress.TrimSipPrefix();

                                if (string.Compare(sipAddress, cfgSipAddress, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    VATRPContact contact = FindContact(new ContactID(cfgSipAddress, IntPtr.Zero));
                                    if (contact != null)
                                    {
                                        LinphoneAPI.linphone_core_remove_friend(manager.LinphoneService.LinphoneCore,
                                            curStruct.data);
                                        RemoveContact(cfgSipAddress, true);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    contactsPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

            }
        }

        public void AddContact(VATRPContact contact, string group)
        {
            Contacts.Add(contact);
            if (ContactAdded != null)
                ContactAdded(this, new ContactEventArgs(new ContactID(contact)));

            if (contact.IsLoggedIn && LoggedInContactUpdated != null)
                LoggedInContactUpdated(this, new ContactEventArgs(new ContactID(contact)));

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
        public VATRPContact FindContactByPhone(string phoneNumber)
        {
            if (!phoneNumber.NotBlank())
                return null;
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && (contact.HomePhone == phoneNumber ||
                        contact.MobilePhone == phoneNumber))
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
                        if (ContactRemoved != null)
                            ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), isUserAction));
                        this.Contacts.Remove(contact);
                        break;
                    }
                }
            }
        }

        public void RemoveContacts()
        {
            lock (this.Contacts)
            {
                while (this.Contacts.Count > 0)
                {
                    foreach (VATRPContact contact in this.Contacts)
                    {
                        if (contact != null)
                        {
                            if (ContactRemoved != null)
                                ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), true));
                            this.Contacts.Remove(contact);
                            break;
                        }
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
            IsLoaded = false;
            LoadLinphoneContacts();
            IsLoaded = true;
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;
    }
}

