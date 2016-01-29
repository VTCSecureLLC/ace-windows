using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using HockeyApp;
using VATRP.Core.Extensions;
using VATRP.Core.Model;
using System.Windows.Documents;
using System.Collections.Generic;
using System.IO;

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

        // Liz E. - this should really be a list to model what we are allowed to send.
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
                // see note below regarding attachments. Once we have these answer and can use the data.... uncomment below.
//                List<IFeedbackAttachment> attachmentList = GetFileAttachmentList();
//                if (attachmentList.Count == 0)
//                {
                string attachmentText = "";
                if (!string.IsNullOrEmpty(AttachmentFile) && AttachmentFile.EndsWith("txt"))
                {
                    if (File.Exists(AttachmentFile))
                    attachmentText = File.ReadAllText(AttachmentFile);
                    attachmentText = "\r\n\r\n" + attachmentText;
                }
                await feedbackThread.PostFeedbackMessageAsync(FeedbackMessage + attachmentText, ContactEmailAddress, Subject, ContactName);
//                }
//                else
//                {
//                    await feedbackThread.PostFeedbackMessageAsync(FeedbackMessage, ContactEmailAddress, Subject, ContactName, attachmentList);
//                }
                viewModel.FeedbackResult = "Feedback sent";
            }
            else
            {
                viewModel.FeedbackResult = "Feedback send failed";
            }
        }

        // Liz E - the attachment does not seem to be working properly. a link is provided int eh feedback for the attachment but the data is not there.
        //   I am seeking assistance throught he HockeyApp discussions. In the meantime, disabling this and attaching the info as part of the message if it is a text file.
        private List<IFeedbackAttachment> GetFileAttachmentList()
        {
            List<IFeedbackAttachment> attachmentList = new List<IFeedbackAttachment>();
            try
            {
                // HockeyApp allows multiple attachments. If we change Attachment file to allow a list, then this will need to be modified.
                if (!string.IsNullOrEmpty(AttachmentFile) && File.Exists(AttachmentFile))
                {
                    byte[] dataBytes = File.ReadAllBytes(AttachmentFile);
                    string contentType = "";
                    if (AttachmentFile.EndsWith("txt"))
                    {
                        contentType = "text/plain; charset=utf-8";
                    }
                    // if we decide to allow images for screen shot, for example
//                    else if (IsImage(AttachmentFile))
//                    {
//                        contentType = <whatever we need for image attachment>;
//                    }
                    // create IFeedbackAttachment in a list
                    IFeedbackAttachment feedbackAttachment = new HockeyApp.Model.FeedbackAttachment(AttachmentFile, dataBytes, contentType);
                    attachmentList.Add(feedbackAttachment);
                }

                return attachmentList;
            } 
            catch (Exception ex)
            {
                // we were note able to access the file data, return empty attachment list
                return attachmentList;
            }
        }

        #endregion
    }
}