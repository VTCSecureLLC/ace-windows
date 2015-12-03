using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Data;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallHistoryViewModel : ViewModelBase
    {
        private ICollectionView _callsListView;
        private IHistoryService _historyService;
        private ObservableCollection<HistoryCallEventViewModel> _callHistoryList;
        private string _eventSearchCriteria = string.Empty;
        private DialpadViewModel _dialpadViewModel;
        private double _historyPaneHeight;
        private HistoryCallEventViewModel _selectedCallEvent;
        private int _activeTab;

        public CallHistoryViewModel()
        {
            _activeTab = 0; // All tab is active by default
            _callsListView = CollectionViewSource.GetDefaultView(this.Calls);
            _callsListView.Filter = new Predicate<object>(this.FilterEventsList);
            _historyPaneHeight = 150;
        }
        public CallHistoryViewModel(IHistoryService historyService, DialpadViewModel dialpadViewModel):
            this()
        {
            _historyService = historyService;
            _historyService.OnCallHistoryEvent += CallHistoryEventChanged;
            _dialpadViewModel = dialpadViewModel;
            _dialpadViewModel.PropertyChanged += OnDialpadPropertyChanged;
        }

        private void OnDialpadPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RemotePartyNumber")
            {
                EventSearchCriteria = _dialpadViewModel.RemotePartyNumber;
            }
        }

        private void CallHistoryEventChanged(object sender, VATRP.Core.Events.VATRPCallEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.CallHistoryEventChanged(sender, e)));
                return;
            }

            var callEvent = sender as VATRPCallEvent;

            switch (e.historyEventType)
            {
                case HistoryEventTypes.Add:
                    AddNewCallEvent(callEvent, true);
                    break;
                case HistoryEventTypes.Load:
                    LoadCallEvents();
                    break;
                case HistoryEventTypes.Reset:
                    Calls.Clear();
                    CallsListView.Refresh();
                    break;
                case HistoryEventTypes.Delete:
                    RemoveCallEvent(callEvent);
                    break;
            }

        }
        public void LoadCallEvents()
        {
            if (_historyService.AllCallsEvents == null)
                return;

            lock (this.Calls)
            {
                var callsItemDB = from VATRPCallEvent call in _historyService.AllCallsEvents
                    orderby call.StartTime descending
                    select call;

                foreach (var avCall in callsItemDB)
                {
                    try
                    {
                        AddNewCallEvent(avCall);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception on LoadAllCalls: " + ex.Message);
                    }
                }
            }

        }

        private void AddNewCallEvent(VATRPCallEvent callEvent, bool refreshNow = false)
        {
            if (FindCallEvent(callEvent) != null)
                return;

            var contact = ServiceManager.Instance.ContactService.FindContactByPhone(callEvent.RemoteParty);
            lock (this.Calls)
            {
                Calls.Add(new HistoryCallEventViewModel(callEvent, contact));
            }

            if (refreshNow)
                CallsListView.Refresh();
        }

        private object FindCallEvent(VATRPCallEvent callEvent)
        {
            lock (this.Calls)
            {
                foreach (var call in Calls)
                {
                    if (call.CallEvent == callEvent)
                    {
                        return callEvent;
                    }
                }
            }
            return null;
        }

        private void RemoveCallEvent(VATRPCallEvent callEvent)
        {
            lock (this.Calls)
            {
                foreach (var call in Calls)
                {
                    if (call.CallEvent == callEvent)
                    {
                        Calls.Remove(call);
                        CallsListView.Refresh();
                        break;
                    }
                }
            }
        }

        public bool FilterEventsList(object item)
        {
            var callModel = item as HistoryCallEventViewModel;
            if (callModel != null)
            {

                if (callModel.CallEvent != null && ActiveTab == 1 && callModel.CallEvent.Status != VATRPHistoryEvent.StatusType.Missed)
                    return false;
                if (callModel.Contact != null)
                {
                    if (callModel.Contact.Fullname.ToLower().Contains(EventSearchCriteria.ToLower()))
                        return true;
                }
                return callModel.PhoneNumber.ToLower().Contains(EventSearchCriteria.ToLower());
            }
            return true;
        }

        public ICollectionView CallsListView
        {
            get { return this._callsListView; }
            private set
            {
                if (value == this._callsListView)
                {
                    return;
                }

                this._callsListView = value;
                OnPropertyChanged("CallsListView");
            }
        }

        public ObservableCollection<HistoryCallEventViewModel> Calls
        {
            get { return _callHistoryList ?? (_callHistoryList = new ObservableCollection<HistoryCallEventViewModel>()); }
            set { _callHistoryList = value; }
        }

        public string EventSearchCriteria
        {
            get { return _eventSearchCriteria; }
            set
            {
                _eventSearchCriteria = value;
                CallsListView.Refresh();
            }
        }

        public double HistoryPaneHeight
        {
            get { return _historyPaneHeight; }
            set
            {
                _historyPaneHeight = value;
                OnPropertyChanged("HistoryPaneHeight");
            }
        }

        public HistoryCallEventViewModel SelectedCallEvent
        {
            get { return _selectedCallEvent; }
            set
            {
                _selectedCallEvent = value; 
                OnPropertyChanged("SelectedCallEvent");
            }
        }

        public int ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                CallsListView.Refresh();
                OnPropertyChanged("ActiveTab");
            }
        }
    }
}