using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
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
        private readonly ServiceManagerBase _manager;
        private ObservableCollection<VATRPContact> _contacts;
        private ObservableCollection<VATRPUserGroup> _groups;

        public event EventHandler<ContactEventArgs> ContactAdded;
        public event EventHandler<ContactRemovedEventArgs> ContactRemoved;
        public event EventHandler<ContactStatusChangedEventArgs> ContactStatusChanged;
        public event EventHandler<EventArgs> GroupsChanged;
        public event EventHandler<ContactEventArgs> ContactsChanged;
        public event EventHandler<EventArgs> ContactsLoadCompleted;
        public event EventHandler<ContactEventArgs> LoggedInContactUpdated;
        public bool IsLoaded { get; private set; }

        private bool editing { get; set; } // use as a lock
        public ContactService(ServiceManagerBase manager)
        {
            this._manager = manager;
            IsLoaded = false;
        }

        public bool IsEditing()
        {
            return editing;
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
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr contactsPtr =
                LinphoneAPI.linphone_core_get_friend_list(_manager.LinphoneService.LinphoneCore);
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

                        string refKey = "";
                        tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(curStruct.data);
                        if (tmpPtr != IntPtr.Zero)
                            refKey = Marshal.PtrToStringAnsi(tmpPtr);

                        if (string.IsNullOrEmpty(refKey))
                        {
                            // generate refkey
                            refKey = Guid.NewGuid().ToString();
                            LinphoneAPI.linphone_friend_set_ref_key(curStruct.data, refKey);
                        }

                        int port = LinphoneAPI.linphone_address_get_port(addressPtr);

                        if (!string.IsNullOrWhiteSpace(un))
                        {
                            var cfgSipaddress = port == 0 ? string.Format("{0}@{1}", un, host):
                                string.Format("{0}@{1}:{2}", un, host, port);
                            VATRPContact contact = new VATRPContact(new ContactID(cfgSipaddress, IntPtr.Zero))
                            {
                                DisplayName = dn,
                                Fullname = dn.NotBlank() ? dn : un,
                                Gender = "male",
                                SipUsername = un,
                                RegistrationName = cfgSipaddress,
                                IsLinphoneContact = true,
                                LinphoneRefKey = refKey
                            };
                            UpdateAvatar(contact);
                            Contacts.Add(contact);
                        }
                    }
                    contactsPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

            }

            LoadContactsOptions();

            if ( ContactsLoadCompleted != null)
                ContactsLoadCompleted(this, EventArgs.Empty);
        }

        private void LoadContactsOptions()
        {
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    lock (this.Contacts)
                    {
                        foreach (VATRPContact contact in this.Contacts)
                        {
                            if (contact.IsLoggedIn)
                                continue;
                            using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"SELECT id FROM friends WHERE sip_uri = ?" })
                            {
                                var sipUri = string.Format(@"""{0}"" <sip:{1}>", contact.DisplayName, contact.ID);
                                var uriParam = new SQLiteParameter(DbType.AnsiString) {Value = sipUri};
                                cmd.Parameters.Add(uriParam);
                                var dbReader = cmd.ExecuteReader();
                                if (dbReader.Read())
                                {
                                    contact.DbID = dbReader.GetInt32(0);
                                }
                            }

                            // update contact 

                            using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"SELECT is_favorite FROM friend_options WHERE id = ?" })
                            {
                                var idParam = new SQLiteParameter(DbType.Int32);
                                idParam.Value = contact.DbID;
                                cmd.Parameters.Add(idParam);
                                var dbReader = cmd.ExecuteReader();
                                if (dbReader.Read())
                                {
                                    contact.IsFavorite = dbReader.GetBoolean(0);
                                }
                            }
                        }
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        private void RemoveFavoriteOption(VATRPContact contact)
        {
            if (contact.DbID == 0)
                return;
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();

                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"DELETE FROM friend_options WHERE id = ?"
                        })
                    {
                        var idParam = new SQLiteParameter(DbType.Int32) {Value = contact.DbID};
                        cmd.Parameters.Add(idParam);
                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        public void UpdateFavoriteOption(VATRPContact contact)
        {
            if (contact.DbID == 0)
                return;
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();

                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"INSERT OR REPLACE INTO friend_options (id, is_favorite) VALUES ( ?, ?)"
                        })
                    {
                        var idParam = new SQLiteParameter(DbType.Int32) {Value = contact.DbID};
                        cmd.Parameters.Add(idParam);

                        var favParam = new SQLiteParameter(DbType.Int32) {Value = contact.IsFavorite ? 1 : 0};
                        cmd.Parameters.Add(favParam);

                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        #region VCARD

        public int ImportVCards(string vcardPath)
        {
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return 0;
            IntPtr vcardsList = LinphoneAPI.linphone_vcard_list_from_vcard4_file(vcardPath);

            if (vcardsList != IntPtr.Zero)
            {
                MSList curStruct;
                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(vcardsList, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        string fullname = string.Empty;
                        IntPtr tmpPtr = LinphoneAPI.linphone_vcard_get_full_name(curStruct.data);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            fullname = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        IntPtr addressListPtr = LinphoneAPI.linphone_vcard_get_sip_addresses(curStruct.data);

                        if (addressListPtr == IntPtr.Zero)
                            continue;
                        MSList addressdata;
                        string sipAddress = "";
                        do
                        {
                            addressdata.next = IntPtr.Zero;
                            addressdata.prev = IntPtr.Zero;
                            addressdata.data = IntPtr.Zero;

                            addressdata = (MSList)Marshal.PtrToStructure(addressListPtr, typeof(MSList));
                            if (addressdata.data != IntPtr.Zero)
                            {
                                sipAddress = Marshal.PtrToStringAnsi(addressdata.data);
                                break;
                            }
                            addressListPtr = addressdata.next;
                        } while (addressdata.data != IntPtr.Zero);


                        if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
                        {
                            string un, host;
                            int port;
                            if ( VATRPCall.ParseSipAddress(sipAddress, out un, out host, out port) )
                                AddLinphoneContact(fullname, un, sipAddress);
                        }
                    }
                    vcardsList = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);
            }
            return 0;
        }

        public void ExportVCards(string vcardPath)
        {
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;
            IntPtr defList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            if (defList != IntPtr.Zero)
                LinphoneAPI.linphone_friend_list_export_friends_as_vcard4_file(defList, vcardPath);
        }

        #endregion

        public void AddLinphoneContact(string name, string username, string address)
        {
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            var sipAddress = address.TrimSipPrefix();

            var fqdn = string.Format("{0} <sip:{1}>", name, sipAddress);
            IntPtr friendPtr = LinphoneAPI.linphone_friend_new_with_address(fqdn);
            if (friendPtr != IntPtr.Zero)
            {
                IntPtr friendList =
                    LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
                LinphoneAPI.linphone_friend_set_name(friendPtr, name);
                LinphoneAPI.linphone_friend_enable_subscribes(friendPtr, false);
                LinphoneAPI.linphone_friend_set_inc_subscribe_policy(friendPtr, 1);
                LinphoneAPI.linphone_friend_list_add_friend(friendList, friendPtr);

                var refKey = Guid.NewGuid().ToString();
                LinphoneAPI.linphone_friend_set_ref_key(friendPtr, refKey);

                var contactID = new ContactID(sipAddress, IntPtr.Zero);
                VATRPContact contact = FindContact(contactID);
                if (contact == null)
                {
                    contact = new VATRPContact(new ContactID(sipAddress, IntPtr.Zero))
                    {
                        DisplayName = name,
                        Fullname = name,
                        Gender = "male",
                        SipUsername = username,
                        RegistrationName = sipAddress,
                        IsLinphoneContact = true,
                        LinphoneRefKey = refKey
                    };
                    Contacts.Add(contact);
                }
                else
                {
                    contact.DisplayName = name;
                    contact.Fullname = name;
                    contact.IsLinphoneContact = true;
                    contact.SipUsername = username;
                    contact.LinphoneRefKey = refKey;
                }
                UpdateAvatar(contact);
                UpdateContactDbId(contact);

                if (ContactAdded != null)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
                    }));
                }
            }
        }

        private void UpdateContactDbId(VATRPContact contact)
        {
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"SELECT id FROM friends WHERE sip_uri = ?"
                        })
                    {
                        var sipUri = string.Format(@"""{0}"" <sip:{1}>", contact.DisplayName, contact.ID);
                        var uriParam = new SQLiteParameter(DbType.AnsiString) {Value = sipUri};
                        cmd.Parameters.Add(uriParam);
                        var dbReader = cmd.ExecuteReader();
                        if (dbReader.Read())
                        {
                            contact.DbID = dbReader.GetInt32(0);
                        }
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        public void EditLinphoneContact(VATRPContact contact, string newname, string newsipassdress)
        {
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr friendList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            if (friendList == IntPtr.Zero)
                return;
            IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_ref_key(friendList,
                contact.LinphoneRefKey);
            if (friendPtr == IntPtr.Zero)
                return;

            var newfqdn = string.Format("sip:{0}", newsipassdress);
            IntPtr addressPtr = LinphoneAPI.linphone_core_create_address(
                _manager.LinphoneService.LinphoneCore, string.Format("{0} <{1}>", newname, newfqdn));
            if (addressPtr != IntPtr.Zero)
            {
                LinphoneAPI.linphone_friend_edit(friendPtr);
                LinphoneAPI.linphone_friend_set_name(friendPtr, newname);
                LinphoneAPI.linphone_friend_set_address(friendPtr, addressPtr);
                LinphoneAPI.linphone_friend_enable_subscribes(friendPtr, false);
                LinphoneAPI.linphone_friend_set_inc_subscribe_policy(friendPtr, 1);
                LinphoneAPI.linphone_friend_done(friendPtr);

                string un = "";
                IntPtr unPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                if (unPtr != IntPtr.Zero)
                {
                    un = Marshal.PtrToStringAnsi(unPtr);
                }

                contact.ID = newsipassdress;
                contact.DisplayName = newname;
                contact.Fullname = newname;
                contact.SipUsername = un;
                contact.RegistrationName = newsipassdress;
            }
            UpdateAvatar(contact);
            UpdateContactDbId(contact);

            if (ContactsChanged != null)
                ContactsChanged(this, new ContactEventArgs(new ContactID(contact)));
        }

        public void DeleteLinphoneContact(VATRPContact contact)
        {
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr friendList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            if (friendList != IntPtr.Zero)
            {
                IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_ref_key(friendList,
                    contact.LinphoneRefKey);
                if (friendPtr == IntPtr.Zero)
                    return;

                if (friendList != IntPtr.Zero)
                    LinphoneAPI.linphone_friend_list_remove_friend(friendList, friendPtr);
                RemoveFavoriteOption(contact);
                try
                {
                    if (contact.Avatar.NotBlank() && File.Exists(contact.Avatar))
                    {
                        File.Delete(contact.Avatar);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to remove file: " + ex.Message);
                }
                RemoveContact(contact.ID, true);
            }
        }

        public void AddContact(VATRPContact contact, string group)
        {
            // update avatar
            UpdateAvatar(contact);
            Contacts.Add(contact);
            if (ContactAdded != null)
                ContactAdded(this, new ContactEventArgs(new ContactID(contact)));

            if (contact.IsLoggedIn && LoggedInContactUpdated != null)
            {
                LoggedInContactUpdated(this, new ContactEventArgs(new ContactID(contact)));
            }
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
                VATRPContact contact = null;
                foreach (VATRPContact c in this.Contacts)
                {
                    if ((c != null) && (c.ID == id))
                    {
                        contact = c;
                        break;
                    }
                }

                if (contact != null)
                {
                    if (ContactRemoved != null)
                        ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), isUserAction));

                    this.Contacts.Remove(contact);
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

        private void UpdateAvatar(VATRPContact contact)
        {
            var supportedFormats = new[] {"jpg", "jpeg", "png", "bmp"};
            for (var i = 0; i < supportedFormats.Length; i++)
            {
                var avatar = _manager.BuildDataPath(string.Format("{0}.{1}", contact.ID, supportedFormats[i]));
                if (File.Exists(avatar))
                {
                    contact.Avatar = avatar;
                    return;
                }
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

        private void InitializeContactOptions()
        {
            if (!File.Exists(_manager.LinphoneService.ContactsDbPath))
                return;
            string sqlString = @"CREATE TABLE IF NOT EXISTS friend_options (id INTEGER NOT NULL," +
                               " is_favorite INTEGER NOT NULL DEFAULT 0, PRIMARY KEY (id))";
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    using (var cmd = new SQLiteCommand(dbConnection) {CommandText = sqlString})
                    {
                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        
        public bool Start()
        {
            IsLoaded = false;
            InitializeContactOptions();
            _manager.LinphoneService.CardDAVContactCreated += LinphoneCardDavContactCreated;
            _manager.LinphoneService.CardDAVContactUpdated += LinphoneCardDavContactUpdated;
            _manager.LinphoneService.CardDAVContactDeleted += LinphoneCardDavContactDeleted;
            _manager.LinphoneService.CardDAVSyncEvent += LinphoneCardDavSynced;

            LoadLinphoneContacts();

            _manager.LinphoneService.CardDAVSync();

            IsLoaded = true;
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            _manager.LinphoneService.CardDAVContactCreated -= LinphoneCardDavContactCreated;
            _manager.LinphoneService.CardDAVContactUpdated -= LinphoneCardDavContactUpdated;
            _manager.LinphoneService.CardDAVContactDeleted -= LinphoneCardDavContactDeleted;
            _manager.LinphoneService.CardDAVSyncEvent -= LinphoneCardDavSynced;

            RemoveContacts();
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return true;
        }

        private void LinphoneCardDavContactCreated(CardDavContactEventArgs args)
        {
            if (args.NewContactPtr == IntPtr.Zero) return;
            IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(args.NewContactPtr);
            if (addressPtr == IntPtr.Zero)
                return;
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

            int port = LinphoneAPI.linphone_address_get_port(addressPtr);

            if (string.IsNullOrWhiteSpace(un)) return;
            var cfgSipaddress = port == 0 ? string.Format("{0}@{1}", un, host) :
                string.Format("{0}@{1}:{2}", un, host, port);

            var refKey = "";
            IntPtr refKeyPtr = LinphoneAPI.linphone_friend_get_ref_key(args.NewContactPtr);
            if (refKeyPtr != IntPtr.Zero)
                refKey = Marshal.PtrToStringAnsi(refKeyPtr);

            if (String.IsNullOrEmpty(refKey))
            {
                refKey = Guid.NewGuid().ToString();
                LinphoneAPI.linphone_friend_set_ref_key(args.NewContactPtr, refKey);
            }
            VATRPContact contact = new VATRPContact(new ContactID(cfgSipaddress, IntPtr.Zero))
            {
                DisplayName = dn,
                Fullname = dn.NotBlank() ? dn : un,
                Gender = "male",
                SipUsername = un,
                RegistrationName = cfgSipaddress,
                IsLinphoneContact = true,
                LinphoneRefKey = refKey
            };
            AddContact(contact, string.Empty);
        }

        private void LinphoneCardDavContactUpdated(CardDavContactEventArgs args)
        {

        }

        private void LinphoneCardDavContactDeleted(CardDavContactEventArgs args)
        {
            if (args.ChangedContactPtr == IntPtr.Zero) return;
            IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(args.ChangedContactPtr);
            if (addressPtr == IntPtr.Zero) return;
            string dn = string.Empty;
            IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                dn = Marshal.PtrToStringAnsi(tmpPtr);
            }

            if (string.IsNullOrEmpty(dn))
                return;

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

            var cfgSipAddress = string.Format("{0}@{1}", un, host);

            VATRPContact contact = FindContact(new ContactID(cfgSipAddress, IntPtr.Zero));
            if (contact != null)
            {
                RemoveFavoriteOption(contact);
                try
                {
                    if (contact.Avatar.NotBlank() && File.Exists(contact.Avatar))
                    {
                        File.Delete(contact.Avatar);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to remove file: " + ex.Message);
                }
                RemoveContact(cfgSipAddress, false);
            }
        }

        private void LinphoneCardDavSynced(CardDavSyncEventArgs args)
        {

        }

        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;
    }
}

