using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ContactListCtrl.xaml
    /// </summary>
    public partial class ContactListCtrl : UserControl
    {
        #region Members
        private ContactsViewModel _contactsViewModel;

        #endregion

        #region Events
        public delegate void MakeCallRequestedDelegate(string called_address);
        public event MakeCallRequestedDelegate MakeCallRequested;
        private bool bEditRequest;
        private bool bDeleteRequest;
        private bool bFavoriteRequest;
        #endregion

        public ContactListCtrl()
        {
            InitializeComponent();            
        }

        public ContactListCtrl(ContactsViewModel viewModel) :
            this()
        {
            SetDataContext(viewModel);
        }

        public void SetDataContext(ContactsViewModel viewModel)
        {
            _contactsViewModel = viewModel;
            DataContext = viewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void OnContactSelected(object sender, SelectionChangedEventArgs e)
        {
            //var dn = string.Empty;
            //var un = string.Empty;
            if (_contactsViewModel.SelectedContact != null)
            {
                //if (!bEditRequest && !bDeleteRequest && !bFavoriteRequest)
                //{
                    if (MakeCallRequested != null && _contactsViewModel.SelectedContact.Contact != null)
                        MakeCallRequested(_contactsViewModel.SelectedContact.Contact.SipUsername);
                //}
                //else if (bDeleteRequest)
                //{
                //    bDeleteRequest = false;
                //    dn = _contactsViewModel.SelectedContact.Contact.Fullname;
                //    un = _contactsViewModel.SelectedContact.Contact.SipUsername;
                //    _contactsViewModel.SelectedContact = null;
                //    if (MessageBox.Show("Do you want to remove the selected contact?", "ACE", MessageBoxButton.YesNo,
                //        MessageBoxImage.Question) == MessageBoxResult.Yes)

                //        ServiceManager.Instance.ContactService.DeleteLinphoneContact(dn,un);
                //}
                //else if (bEditRequest)
                //{
                //    bEditRequest = false;
                    
                //}
                //else if (bFavoriteRequest)
                //{
                //    _contactsViewModel.SelectedContact.Contact.IsFavorite =
                //        !_contactsViewModel.SelectedContact.Contact.IsFavorite;
                //}
                _contactsViewModel.SelectedContact = null;
            }
        }

        private void OnAddContact(object sender, RoutedEventArgs e)
        {
            ContactEditViewModel model = new ContactEditViewModel(true);
            var contactEditView = new ContactEditView(model);
            Nullable<bool> dialogResult = contactEditView.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                ServiceManager.Instance.ContactService.AddLinphoneContact(model.ContactName, model.ContactSipAddress);
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            var contact = ((ToggleButton)sender).Tag as VATRPContact;
            if (contact != null)
            {
                ContactEditViewModel model = new ContactEditViewModel(false);
                model.ContactName = contact.Fullname;
                model.ContactSipAddress = contact.SipUsername;
                var contactEditView = new ContactEditView(model);
                Nullable<bool> dialogResult = contactEditView.ShowDialog();
                if (dialogResult != null && dialogResult.Value)
                {
                    ServiceManager.Instance.ContactService.EditLinphoneContact(
                        contact.Fullname,
                        contact.SipUsername, model.ContactName,
                        model.ContactSipAddress);
                }
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {

            var contact = ((ToggleButton)sender).Tag as VATRPContact;
            if (contact != null)
            {
                if (MessageBox.Show("Do you want to remove the selected contact?", "ACE", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)

                    ServiceManager.Instance.ContactService.DeleteLinphoneContact(
                        contact.Fullname,
                        contact.SipUsername);
            }
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            var contact = ((ToggleButton)sender).Tag as VATRPContact;
            if (contact != null)
                contact.IsFavorite =
                        !contact.IsFavorite;
        }
    }
        
   
}
