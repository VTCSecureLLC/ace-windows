using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;
using com.vtcsecure.ace.windows.CustomControls.Resources;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for LocalContactPanel.xaml
    /// </summary>
    public partial class LocalContactPanel : UserControl
    {
        public Resources_CallResource CallResourceRequested;

        #region Members

        private LocalContactViewModel _viewModel;
        #endregion

        #region Events

        public event EventHandler VideomailCountReset;

        #endregion

        public LocalContactPanel()
        {
            InitializeComponent();
        }

        public void SetDataContext(LocalContactViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;

        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void ResetVideoMailCount(object sender, RoutedEventArgs e)
        {
            if (VideomailCountReset != null) 
                VideomailCountReset(this, EventArgs.Empty);
        }

        //OnCallVideoMail
        private void OnCallVideoMail(object sender, RoutedEventArgs e)
        {
            if (CallResourceRequested != null)
            {
                // sanity check here.
                if ((App.CurrentAccount != null) && !string.IsNullOrEmpty(App.CurrentAccount.VideoMailUri))
                {
                    ResourceInfo resourceInfo = new ResourceInfo();
                    resourceInfo.address = App.CurrentAccount.VideoMailUri;
                    resourceInfo.name = "Video Mail";
                    CallResourceRequested(resourceInfo);
                }
            }
        }
    }
}
