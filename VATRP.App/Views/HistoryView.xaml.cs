using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using log4net;
using VATRP.App.CustomControls;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView
    {
        #region Members

        private static readonly ILog LOG = LogManager.GetLogger(typeof (HistoryView));
        private ICollectionView callsView;

        private bool _populatingCalls = false;
        private ObservableCollection<RecentsCallItem> _calls;
        private IHistoryService _historyService;

        #endregion

        #region Properties

        public bool IsPopulating
        {
            get { return _populatingCalls; }
        }

        private ObservableCollection<RecentsCallItem> CallsList
        {
            get { return _calls ?? (_calls = new ObservableCollection<RecentsCallItem>()); }
        }

        #endregion

        public HistoryView()
            : base(VATRPWindowType.RECENTS_VIEW)
        {
            InitializeComponent();
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_historyService != null)
                return;
            _historyService = ServiceManager.Instance.HistoryService;
            _historyService.OnCallHistoryEvent += OnHistoryCallEvent;
            lstCallsBox.Visibility = Visibility.Collapsed;
            new System.Threading.Thread((System.Threading.ThreadStart)_historyService.LoadCallEvents).Start();
        }

        protected void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _historyService.OnCallHistoryEvent -= OnHistoryCallEvent;
            base.Window_Unloaded(sender, e);
        }

        private void ShowAllCalls()
        {
            LoadAllCalls();
            PopulateCalls(true);
        }

        private void ShowMissedCalls()
        {
            CallsList.Clear();

            var callsItemDB = from VATRPCallEvent call in _historyService.AllCallsEvents
                where call.Status == VATRPHistoryEvent.StatusType.Missed
                orderby call.StartTime descending
                select call;

            foreach (var avCall in callsItemDB)
            {
                try
                {
                    var callItem = new RecentsCallItem()
                    {
                        CallerName =
                            ServiceManager.Instance.ContactService.GetContactDisplayName(avCall.Contact,
                                avCall.RemoteParty),
                        CallTime = avCall.StartTime,
                        Duration = 0,
                        TargetNumber = avCall.RemoteParty,
                        CallStatus = avCall.Status
                    };
                    CallsList.Add(callItem);
                }
                catch (Exception ex)
                {
                    LOG.Error("Exception on ShowMissedCalls: " + ex.Message);
                }
            }

            PopulateCalls(false);
        }

        private void OnCallItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var callItem = lstCallsBox.SelectedItem as RecentsCallItem;

            if (callItem != null)
            {
                VATRPContact contact = ServiceManager.Instance.ContactService.FindContactId(callItem.ContactId);

                MediaActionHandler.MakeAudioCall(callItem.TargetNumber);
            }
        }

        private void OnHistoryCallEvent(object sender, EventArgs e)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnHistoryCallEvent(sender, e)));
                return;
            }

            _populatingCalls = true;

            LoadAllCalls();
            PopulateCalls(CallsTab.SelectedIndex == 0);
            _populatingCalls = false;
            lstCallsBox.Visibility = Visibility.Visible;
        }

        private void LoadAllCalls()
        {
            CallsList.Clear();

            var callsItemDB = from VATRPCallEvent call in _historyService.AllCallsEvents
                orderby call.StartTime descending
                select call;

            foreach (var avCall in callsItemDB)
            {
                try
                {
                    var dn = ServiceManager.Instance.ContactService.GetContactDisplayName(avCall.Contact,
                        avCall.RemoteParty);
                    var callItem = new RecentsCallItem()
                    {
                        CallerName = dn,
                        CallTime = avCall.StartTime,
                        Duration = avCall.Status == VATRPHistoryEvent.StatusType.Missed ? -1 : avCall.Duration,
                        TargetNumber = avCall.RemoteParty,
                        CallStatus = avCall.Status,
                        ContactId = avCall.Contact != null ? avCall.Contact.DisplayName : string.Empty
                    };
                    CallsList.Add(callItem);
                }
                catch (Exception ex)
                {
                    LOG.Error("Exception on LoadAllCalls: " + ex.Message);
                }
            }

        }

        private void PopulateCalls(bool allCalls)
        {
            DataContext = CallsList;
            ListBox lstBox = allCalls ? lstCallsBox : lstMissedCallsBox;
            lstBox.ItemsSource = CallsList;
            this.callsView = CollectionViewSource.GetDefaultView(lstBox.ItemsSource);
        }

        private void CallsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_historyService == null)
                return;

            if (CallsTab.SelectedIndex == 0)
                ShowAllCalls();
            else
                ShowMissedCalls();
        }
    }
}
