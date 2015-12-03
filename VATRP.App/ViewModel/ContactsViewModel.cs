using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Data;
using VATRP.Core.Extensions;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactsViewModel : ViewModelBase
    {
        private ICollectionView _contactsListView;
        private IContactsService _contactsService;
        private ObservableCollection<ContactViewModel> _contactsList;
        private string _eventSearchCriteria = string.Empty;
        private DialpadViewModel _dialpadViewModel;
        private double _contactPaneHeight;
        private ContactViewModel _selectedContact;
        private int _activeTab;
        public ContactsViewModel()
        {
            _activeTab = 0; // All tab is active by default
            _contactsListView = CollectionViewSource.GetDefaultView(this.Contacts);
            _contactsListView.Filter = new Predicate<object>(this.FilterContactsList);
            _contactPaneHeight = 150;
        }
        public ContactsViewModel(IContactsService contactService, DialpadViewModel dialpadViewModel) :
            this()
        {
            _contactsService = contactService;
            _contactsService.ContactAdded += ContactAdded;
            _contactsService.ContactRemoved += ContactRemoved;
            _contactsService.ContactsLoadCompleted += ContactsLoadCompleted;
            _dialpadViewModel = dialpadViewModel;
            _dialpadViewModel.PropertyChanged += OnDialpadPropertyChanged;
        }

        private void ContactsLoadCompleted(object sender, EventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactsLoadCompleted(sender, e)));
                return;
            }

            LoadContacts();
            OnPropertyChanged("Contacts");
        }

        private void OnDialpadPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RemotePartyNumber")
            {
                EventSearchCriteria = _dialpadViewModel.RemotePartyNumber;
            }
        }

        private void ContactAdded(object sender, ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactAdded(sender, e)));
                return;
            }

            var contact = _contactsService.FindContact(e.Contact);
            if (contact != null)
            {
                AddContact(contact, true);
            }
        }

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsFavorite" && ActiveTab == 1)
            {
                ContactsListView.Refresh();
            }
        }

        private void ContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactRemoved(sender, e)));
                return;
            }

            RemoveContactModel(e.contactId);
        }

        public void LoadContacts()
        {
            if (_contactsService.Contacts == null)
                return;

            lock (this.Contacts)
            {
                foreach (var c in _contactsService.Contacts)
                {
                    try
                    {
                        AddContact(c);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception on LoadContacts: " + ex.Message);
                    }
                }
            }

        }

        private void AddContact(VATRPContact contact, bool refreshNow = false)
        {
            if (!contact.SipUsername.NotBlank() || !contact.IsLinphoneContact)
                return;

            if (FindContact(contact) != null)
                return;

            lock (this.Contacts)
            {
                contact.PropertyChanged += OnContactPropertyChanged;
                Contacts.Add(new ContactViewModel(contact));
            }

            if (refreshNow)
                ContactsListView.Refresh();
        }

        private object FindContact(VATRPContact contact)
        {
            lock (this.Contacts)
            {
                foreach (var c in Contacts)
                {
                    if (c.Contact == contact)
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        private void RemoveContactModel(ContactID contactID)
        {
            lock (this.Contacts)
            {
                foreach (var contact in Contacts)
                {
                    if (contact.Contact == contactID)
                    {
                        contact.PropertyChanged -= OnContactPropertyChanged;
                        Contacts.Remove(contact);
                        ContactsListView.Refresh();
                        break;
                    }
                }
            }
        }

        public bool FilterContactsList(object item)
        {
            var contactModel = item as ContactViewModel;
            if (contactModel != null)
            {
                if (contactModel.Contact != null && ActiveTab == 1 && !contactModel.Contact.IsFavorite)
                    return false;

                if (contactModel.Contact != null)
                {
                    if (contactModel.Contact.Fullname.ToLower().Contains(EventSearchCriteria.ToLower()))
                        return true;
                    return contactModel.Contact.ContactAddress_ForUI.ToLower().Contains(EventSearchCriteria.ToLower());
                }
            }
            return true;
        }

        public ICollectionView ContactsListView
        {
            get { return this._contactsListView; }
            private set
            {
                if (value == this._contactsListView)
                {
                    return;
                }

                this._contactsListView = value;
                OnPropertyChanged("CallsListView");
            }
        }

        public ObservableCollection<ContactViewModel> Contacts
        {
            get { return _contactsList ?? (_contactsList = new ObservableCollection<ContactViewModel>()); }
            set { _contactsList = value; }
        }

        public string EventSearchCriteria
        {
            get { return _eventSearchCriteria; }
            set
            {
                _eventSearchCriteria = value;
                ContactsListView.Refresh();
            }
        }

        public double ContactPaneHeight
        {
            get { return _contactPaneHeight; }
            set
            {
                _contactPaneHeight = value;
                OnPropertyChanged("HistoryPaneHeight");
            }
        }

        public ContactViewModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
        }
        public int ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                ContactsListView.Refresh();
                OnPropertyChanged("ActiveTab");
            }
        }
    }
}