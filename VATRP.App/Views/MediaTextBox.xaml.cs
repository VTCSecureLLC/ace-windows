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

        private readonly MessagingViewModel model = new MessagingViewModel(ServiceManager.Instance.ChatService,
            ServiceManager.Instance.ContactService);
        public MediaTextWindow() : base(VATRPWindowType.MESSAGE_VIEW)
        {
            ServiceManager.Instance.ChatService.ConversationUpdated += ChatManagerOnConversationUpdated;
            InitializeComponent();
            DataContext = model;
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
                    model.SetActiveChatContact(contactModel.Contact);
                    ScrollToEnd();
                }
            }
        }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ServiceManager.Instance.IsRttAvailable)
            {
                model.SendMessage(model.MessageText);
            }
            else
            {
                model.SendMessage('\r', false);
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
                        model.SendMessage(model.MessageText);
                        return;
                    }
                    break;
                case Key.Space:
                    model.LastInput = " ";
                    break;
                case Key.Back:
                    if (!model.LastInput.NotBlank())
                        model.LastInput += '\b';
                    break;
                default:
                    break;
            }

            if (model.LastInput.NotBlank())
            {
                for(int i=0; i<model.LastInput.Length; i++)
                    model.SendMessage(model.LastInput[i], isIncomplete);
            }
            model.LastInput = string.Empty;
        }

        private void OnTextInpput(object sender, TextCompositionEventArgs e)
        {
            model.LastInput = e.Text;
        }

        internal void CreateConversation(string remoteUsername)
        {
            var contactID = new ContactID(remoteUsername, IntPtr.Zero);
            VATRPContact contact =
                ServiceManager.Instance.ContactService.FindContact(contactID);
            if (contact == null)
            {
                contact = new VATRPContact(contactID);
                contact.Fullname = remoteUsername;
                contact.DisplayName = remoteUsername;
                ServiceManager.Instance.ContactService.AddContact(contact, string.Empty);
            }
            model.SetActiveChatContact(contact);
        }
    }
}
