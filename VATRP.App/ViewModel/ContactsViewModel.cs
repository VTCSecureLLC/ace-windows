using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Data;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Views;
using Microsoft.Win32;
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

        public ActionCommand TabPanelImportContactsCommand
        {
            get { return new ActionCommand(ExecuteImportCommand, CanExecuteImport); } 
            
        }

        public ActionCommand TabPanelExportContactsCommand
        {
            get { return new ActionCommand(ExecuteExportCommand, CanExecuteExport); }
        }

        public ActionCommand TabPanelAddContactCommand
        {
            get { return new ActionCommand(ExecuteAddCommand, CanExecuteAdd); }
        }

        public ContactsViewModel()
        {
            _activeTab = 0; // All tab is active by default
            _contactsListView = CollectionViewSource.GetDefaultView(this.Contacts);
            _contactsListView.Filter = new Predicate<object>(this.FilterContactsList);
            _contactsListView.SortDescriptions.Add(new SortDescription("ContactUI", ListSortDirection.Ascending));
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

        private void ExecuteImportCommand(object obj)
        {
            var openDlg = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "vCard Files (*.VCF, *.vcard)|*.VCF;*vcard",
                FilterIndex = 0,

                ShowReadOnly = false,
            };

            if (openDlg.ShowDialog() != true)
                return;

            if (ServiceManager.Instance.LinphoneService.VCardSupported)
            {
                var recordsImported = ServiceManager.Instance.ContactService.ImportVCards(openDlg.FileName);
            }
            else
            {
                var cardReader = new vCardReader(openDlg.FileName);

                string un, host;
                int port;

                foreach (var card in cardReader.vCards)
                {
                    var remoteParty = card.Title.TrimSipPrefix();
                    var contact =
                        ServiceManager.Instance.ContactService.FindContact(new ContactID(remoteParty, IntPtr.Zero));
                    if (contact != null && contact.Fullname == card.FormattedName)
                    {
                        continue;
                    }
                    VATRPCall.ParseSipAddress(remoteParty, out un, out host, out port);
                    if ((App.CurrentAccount != null && App.CurrentAccount.ProxyHostname != host) ||
                        App.CurrentAccount == null)
                    {
                        un = remoteParty;
                    }
                    ServiceManager.Instance.ContactService.AddLinphoneContact(card.FormattedName, un,
                        remoteParty);
                }
            }
        }

        private bool CanExecuteImport(object arg)
        {
            return true;
        }

        private void ExecuteExportCommand(object obj)
        {
            var saveDlg = new SaveFileDialog()
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = "ace_contacts",
                Filter = "VCF files (*.vcf) | *.vcf",
                FilterIndex = 0,
            };

            if (saveDlg.ShowDialog() != true)
                return;

            if (ServiceManager.Instance.LinphoneService.VCardSupported)
            {
               ServiceManager.Instance.ContactService.ExportVCards(saveDlg.FileName);
            }
            else
            {
                var cardWriter = new vCardWriter();
                var vCards = new List<vCard>();

                foreach (var contactVM in this.Contacts)
                {
                    var card = new vCard()
                    {
                        GivenName = contactVM.Contact.Fullname,
                        FormattedName = contactVM.Contact.Fullname,
                        Title = contactVM.Contact.RegistrationName
                    };
                    vCards.Add(card);
                }
                cardWriter.WriteCards(saveDlg.FileName, vCards);
            }
        }

        private bool CanExecuteExport(object arg)
        {
            return ServiceManager.Instance.ContactService.Contacts.Any(contact => contact.IsLinphoneContact);
        }

        private void ExecuteAddCommand(object obj)
        {
            ContactEditViewModel model = new ContactEditViewModel(true, "");
            var contactEditView = new ContactEditView(model);
            var dialogResult = contactEditView.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                var contact = ServiceManager.Instance.ContactService.FindContact(new ContactID(model.ContactSipAddress, IntPtr.Zero));
                if (contact != null && contact.Fullname == model.ContactName)
                    return;

                ServiceManager.Instance.ContactService.AddLinphoneContact(model.ContactName, model.ContactSipUsername,
                    model.ContactSipAddress);
            }
        }

        private bool CanExecuteAdd(object arg)
        {
            return true;
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