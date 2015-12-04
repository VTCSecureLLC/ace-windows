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

            if (!_viewModel.ValidateUsername(_viewModel.ContactSipUsername))
            {
                if (!_viewModel.ValidateAddress(_viewModel.ContactSipUsername))
                {
                    MessageBox.Show("Please enter correct username", "ACE", MessageBoxButton.OK,
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
