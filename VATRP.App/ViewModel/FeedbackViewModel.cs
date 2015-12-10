using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using HockeyApp;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class FeedbackViewModel : ViewModelBase
    {
        private string _contactName;
        private string _subject;
        private string _contactEmailAddress;
        private string _feedbackMessage;
        private string _attachmentFile;
        private string _feedbackResult;

        public FeedbackViewModel()
        {
        }

        #region Properties


        public string ContactName
        {
            get { return _contactName; }
            set
            {
                _contactName = value;
                OnPropertyChanged("ContactName");
            }
        }

        public string ContactEmailAddress
        {
            get { return _contactEmailAddress; }
            set
            {
                _contactEmailAddress = value;
                OnPropertyChanged("ContactEmailAddress");
            }
        }

        public string Subject
        {
            get { return _subject; }
            set
            {
                _subject = value;
                OnPropertyChanged("Subject");
            }
        }

        public string FeedbackMessage
        {
            get { return _feedbackMessage; }
            set
            {
                _feedbackMessage = value;
                OnPropertyChanged("FeedbackMessage");
                OnPropertyChanged("AllowSendFeedback");
            }
        }

        public string AttachmentFile
        {
            get { return _attachmentFile; }
            set
            {
                _attachmentFile = value;
                OnPropertyChanged("AttachmentFile");
            }
        }

        public bool AllowSendFeedback
        {
            get { return FeedbackMessage.NotBlank(); }
        }

        public string FeedbackResult
        {
            get { return _feedbackResult; }
            set
            {
                _feedbackResult = value;
                OnPropertyChanged("FeedbackResult");
            }
        }

        #endregion

        #region Methods

        internal async void SendFeedback(FeedbackViewModel viewModel)
        {
            IFeedbackThread feedbackThread = HockeyClient.Current.CreateFeedbackThread();
            if (feedbackThread != null)
            {
                viewModel.FeedbackResult = "Sending feedback ...";
                await feedbackThread.PostFeedbackMessageAsync(FeedbackMessage, ContactEmailAddress, Subject, ContactName);
                viewModel.FeedbackResult = "Feedback sent";
            }
            else
            {
                viewModel.FeedbackResult = "Feedback send failed";
            }
        }

        #endregion
    }
}