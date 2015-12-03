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
using System.Threading;

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
            ServiceManager.Instance.ChatService.ConversationUpdated += ChatManagerOnConversationUpdated;
        }

        public void SetViewModel(MessagingViewModel viewModel)
        {
            if (_viewModel != null && _viewModel != viewModel)
            {
                _viewModel.ConversationStarted -= OnConversationStarted;
            }
            DataContext = viewModel;
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.ConversationStarted += OnConversationStarted;
                ScrollToEnd();
            }
        }

        private void OnConversationStarted(object sender, EventArgs eventArgs)
        {
            ScrollToEnd();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

       private void ChatManagerOnConversationUpdated(object sender, VATRP.Core.Events.ConversationUpdatedEventArgs e)
       {
           ScrollToEnd();
       }

       private void ScrollToEnd()
       {
           if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
           {
               ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(ScrollToEnd));
               return;
           }

           MessageListView.SelectedIndex = MessageListView.Items.Count - 1;
           MessageListView.ScrollIntoView(MessageListView.SelectedItem);
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
            if (!ServiceManager.Instance.IsRttAvailable)
            {
                _viewModel.SendMessage(_viewModel.MessageText);
            }
            else
            {
                _viewModel.SendMessage('\r', false);
            }
        }

        private void OnTextInpput(object sender, TextCompositionEventArgs e)
        {
            _viewModel.LastInput = e.Text;
        }
    }
        
   
}
