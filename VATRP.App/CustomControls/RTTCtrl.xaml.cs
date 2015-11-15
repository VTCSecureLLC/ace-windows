using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for RTTCtrl.xaml
    /// </summary>
    public partial class RTTCtrl : UserControl
    {
        #region Members
        private MessagingViewModel _viewModel;
        #endregion

        public RTTCtrl()
        {
            InitializeComponent();
        }

        public void SetViewModel(MessagingViewModel viewModel)
        {
            DataContext = viewModel;
            _viewModel = viewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            bool isIncomplete = true;
            switch (e.Key)
            {
                case Key.Enter:
                    isIncomplete = false;
                    if (!ServiceManager.Instance.IsRttAvailable)
                    {
                        _viewModel.SendMessage(_viewModel.MessageText);
                        return;
                    }
                    break;
                case Key.Space:
                    _viewModel.LastInput = " ";
                    break;
                case Key.Back:
                    if (!_viewModel.LastInput.NotBlank())
                        _viewModel.LastInput += '\b';
                    break;
                default:
                    break;
            }

            if (_viewModel.LastInput.NotBlank())
            {
                for (int i = 0; i < _viewModel.LastInput.Length; i++)
                    _viewModel.SendMessage(_viewModel.LastInput[i], isIncomplete);
            }
            _viewModel.LastInput = string.Empty;
        }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnTextInpput(object sender, TextCompositionEventArgs e)
        {
            _viewModel.LastInput = e.Text;
        }
    }
        
   
}
