using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for ContactEditView.xaml
    /// </summary>
    public partial class ContactEditView : Window
    {
        private ContactEditViewModel _viewModel;
        public ContactEditView()
        {
            InitializeComponent();
        }

        public ContactEditView(ContactEditViewModel viewModel):this()
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.ValidateName())
            {
                MessageBox.Show("Please enter contact name", "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                NameBox.Focus();
                return;
            }

            _viewModel.UpdateContactAddress();
            if (!_viewModel.ValidateUsername(_viewModel.ContactSipUsername))
            {
                bool errorOccurred = true;
                var errorString = string.Empty;
                switch (_viewModel.ValidateAddress(_viewModel.ContactSipUsername))
                {
                    case 1:
                        errorString = "Empty username is not allowed";
                        break;
                    case 2:
                        errorString = "Calling address format is incorrect";
                        break;
                    case 3:
                        errorString = "Username format is incorrect";
                        break;
                    case 4:
                        errorString = "Registration host format is incorrect";
                        break;
                    case 5:
                        errorString = "Port is out of range";
                        break;
                    default:
                        errorOccurred = false;
                        break;
                }

                if (errorOccurred)
                {
                    MessageBox.Show(errorString, "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    AddressBox.Focus();
                    return;
                }
            }
            this.DialogResult = true;
            Close();
        }
    }
}
