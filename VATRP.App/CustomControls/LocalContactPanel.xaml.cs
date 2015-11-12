using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for LocalContactPanel.xaml
    /// </summary>
    public partial class LocalContactPanel : UserControl
    {
        #region Members

        private LocalContactViewModel _viewModel;
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
    }
}
