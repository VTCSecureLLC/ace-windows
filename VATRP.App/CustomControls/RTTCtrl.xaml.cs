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
using VATRP.Core.Model;

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
                _viewModel.ConversationUpdated -= OnConversationUpdated;
            }
            DataContext = viewModel;
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.ConversationUpdated += OnConversationUpdated;
            }
        }

        private void OnConversationUpdated(object sender, EventArgs eventArgs)
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
           var item = MessageListView.SelectedItem as VATRPChatMessage;
           if (item != null)
           {
               MessageListView.ScrollIntoView(item);
           }
       }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                if (!ServiceManager.Instance.IsRttAvailable)
                {
                    _viewModel.SendMessage(_viewModel.MessageText);
                }
                else
                {
                    _viewModel.EnqueueInput("\r");
                }
            }
        }

        private void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_viewModel != null) 
                _viewModel.EnqueueInput(e.Text);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            Char inputKey = Char.MinValue;
            switch (e.Key)
            {
                case Key.None:
                    break;
                case Key.Back:
                    inputKey = '\b';
                    break;
                case Key.Tab:
                    inputKey = '\t';
                    break;
                case Key.LineFeed:
                    inputKey = '\n';
                    break;
                case Key.Clear:
                    break;
                case Key.Return:
                    inputKey = '\r';
                    break;
                case Key.Space:
                    inputKey = ' ';
                    break;
                default:
                    break;
            }
            if (inputKey != Char.MinValue)
            {
                if (_viewModel != null) 
                    _viewModel.EnqueueInput(inputKey.ToString());
            }
        }
    }
        
   
}
