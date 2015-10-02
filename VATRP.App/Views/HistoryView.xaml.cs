using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using log4net;
using VATRP.App.CustomControls;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Events;
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
        private ObservableCollection<RecentsCallItem> _missedCalls;
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

        private ObservableCollection<RecentsCallItem> MissedCallsList
        {
            get { return _missedCalls ?? (_missedCalls = new ObservableCollection<RecentsCallItem>()); }
        }
        #endregion

        #region Events
        public delegate void MakeCallRequestedDelegate(string called_address);
        public event MakeCallRequestedDelegate MakeCallRequested;
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
            new Thread((ThreadStart)_historyService.LoadCallEvents).Start();
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

        
        private void OnCallItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var callItem = lstCallsBox.SelectedItem as RecentsCallItem;

            if (callItem != null)
            {
                if (MakeCallRequested != null)
                    MakeCallRequested(callItem.TargetNumber);
            }
        }

        private void OnHistoryCallEvent(object o, VATRPCallEventArgs e)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnHistoryCallEvent(o, e)));
                return;
            }

            var callEvent = o as VATRPCallEvent;
            _populatingCalls = true;

            switch (e.historyEventType)
            {
                case HistoryEventTypes.Add:
                    if (callEvent != null)
                        AddNewCallEvent(callEvent);
                    break;
                case HistoryEventTypes.Load:
                    LoadAllCalls();
                    PopulateCalls(CallsTab.SelectedIndex == 0);
                    _populatingCalls = false;
                    lstCallsBox.Visibility = Visibility.Visible;
                    break;
                case HistoryEventTypes.Reset:
                    lstCallsBox.ItemsSource = null;
                    lstMissedCallsBox.ItemsSource = null;
                    break;
                case HistoryEventTypes.Delete:
                    if (callEvent != null)
                        DeleteCallEvent(callEvent);
                    break;
            }
        }

        private void DeleteCallEvent(VATRPCallEvent callEvent)
        {
            
        }

        private void AddNewCallEvent(VATRPCallEvent callEvent)
        {
            try
            {
                var dn = ServiceManager.Instance.ContactService.GetContactDisplayName(callEvent.Contact,
                    callEvent.RemoteParty);
                var callItem = new RecentsCallItem()
                {
                    CallerName = dn,
                    CallTime = callEvent.StartTime,
                    Duration = callEvent.Status == VATRPHistoryEvent.StatusType.Missed ? -1 : callEvent.Duration,
                    TargetNumber = callEvent.RemoteParty,
                    CallStatus = callEvent.Status,
                    ContactId = callEvent.Contact != null ? callEvent.Contact.DisplayName : String.Empty
                };
                if ( callEvent.Status == VATRPHistoryEvent.StatusType.Missed)
                    MissedCallsList.Add(callItem);
                CallsList.Add(callItem);
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("AddNewCallEvent", ex);
            }
        }

        private void LoadAllCalls()
        {
            CallsList.Clear();

            var callsItemDB = from VATRPCallEvent call in _historyService.AllCallsEvents
                orderby call.StartTime descending
                select call;

            foreach (var avCall in callsItemDB.Take(30))
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
            ListBox lstBox = allCalls ? lstCallsBox : lstMissedCallsBox;
            ObservableCollection<RecentsCallItem> callsSource = allCalls ? CallsList : MissedCallsList;
            lstBox.ItemsSource = callsSource;
        }

        private void CallsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_historyService == null)
                return;

            PopulateCalls(CallsTab.SelectedIndex == 0);
        }
    }
}
