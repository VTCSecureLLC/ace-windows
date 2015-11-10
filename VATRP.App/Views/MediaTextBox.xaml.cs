using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for MediaTextWindow.xaml
    /// </summary>
    public partial class MediaTextWindow
    {
        private MessagingViewModel _model;
        public MediaTextWindow(MessagingViewModel vm) : base(VATRPWindowType.MESSAGE_VIEW)
        {
            _model = vm;
            DataContext = vm;
            InitializeComponent();
            ServiceManager.Instance.ChatService.ConversationUpdated += ChatManagerOnConversationUpdated;
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

        private void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsList.SelectedItem != null)
            {
                var contactModel = ContactsList.SelectedItem as ContactViewModel;

                if (contactModel != null)
                {
                    _model.SetActiveChatContact(contactModel.Contact);
                    ScrollToEnd();
                }
            }
        }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ServiceManager.Instance.IsRttAvailable)
            {
                _model.SendMessage(_model.MessageText);
            }
            else
            {
                _model.SendMessage('\r', false);
            }
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
                        _model.SendMessage(_model.MessageText);
                        return;
                    }
                    break;
                case Key.Space:
                    _model.LastInput = " ";
                    break;
                case Key.Back:
                    if (!_model.LastInput.NotBlank())
                        _model.LastInput += '\b';
                    break;
                default:
                    break;
            }

            if (_model.LastInput.NotBlank())
            {
                for(int i=0; i<_model.LastInput.Length; i++)
                    _model.SendMessage(_model.LastInput[i], isIncomplete);
            }
            _model.LastInput = string.Empty;
        }

        private void OnTextInpput(object sender, TextCompositionEventArgs e)
        {
            _model.LastInput = e.Text;
        }
       
    }
}
