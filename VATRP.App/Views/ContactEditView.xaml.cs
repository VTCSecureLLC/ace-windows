using System.IO;
using System.Windows;
using System.Windows.Input;
using com.vtcsecure.ace.windows.ViewModel;
using System;
using System.Diagnostics;
using com.vtcsecure.ace.windows.Services;
using log4net;
using VATRP.Core.Services;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for ContactEditView.xaml
    /// </summary>
    public partial class ContactEditView : Window
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ContactEditView));
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

            _viewModel.UpdateContactAddress(false);
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

            if (!string.IsNullOrEmpty(_viewModel.NewAvatarPath))
            {
                if (!string.IsNullOrEmpty(_viewModel.OriginAvatarPath))
                {
                    if (File.Exists(_viewModel.OriginAvatarPath))
                    {
                        try
                        {
                            File.Delete(_viewModel.OriginAvatarPath);
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn("Failed to remove file: " + _viewModel.OriginAvatarPath + " Cause: " + ex.Message);
                        }
                    }
                }

                var avatarPath =
                    ServiceManager.Instance.BuildDataPath(string.Format("{0}{1}",
                        _viewModel.ContactSipUsername, Path.GetExtension(_viewModel.NewAvatarPath)));
                try
                {
                    // just for confidence

                    if (File.Exists(avatarPath))
                        File.Delete(avatarPath);

                    File.Copy(_viewModel.NewAvatarPath, avatarPath);
                    _viewModel.AvatarChanged = true;
                }
                catch (Exception ex)
                {
                    LOG.Warn(string.Format("Failed to copy file: {0} -> {1}. Cause: {2}", _viewModel.NewAvatarPath,
                        avatarPath, ex.Message));
                }
            }
            else 
            {
                if (!string.IsNullOrEmpty(_viewModel.OriginAvatarPath))
                {
                    var fileExt = Path.GetExtension(_viewModel.OriginAvatarPath);

                    var newAvatarPath =
                    ServiceManager.Instance.BuildDataPath(string.Format("{0}{1}",
                        _viewModel.ContactSipUsername, fileExt));
                    if (newAvatarPath != _viewModel.OriginAvatarPath)
                    {
                        // rename old file
                        try
                        {
                            File.Move(_viewModel.OriginAvatarPath, newAvatarPath);
                            _viewModel.AvatarChanged = true;
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn(string.Format("Failed to move file: {0} -> {1}. Cause: {2}", _viewModel.OriginAvatarPath,
                                newAvatarPath, ex.Message));
                        }
                    }
                }

            }
            this.DialogResult = true;
            Close();
        }

        private void PictureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._viewModel != null)
                _viewModel.SelectAvatar();
        }
    }
}
