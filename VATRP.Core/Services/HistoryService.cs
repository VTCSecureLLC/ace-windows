using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper;
using System.Runtime.InteropServices;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Services
{
    public class HistoryService : IHistoryService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (HistoryService));

        private readonly ServiceManagerBase manager;
        private readonly string connectionString;
        private readonly string dbFilePath;
        private bool isLoadingCalls;
        private  List<VATRPCallEvent> _allCallsEvents;

        public event EventHandler<VATRPCallEventArgs> OnCallHistoryEvent;

        public HistoryService(ServiceManagerBase manager)
        {
            this.manager = manager;
            dbFilePath = manager.BuildStoragePath("history.db");
            connectionString = string.Format("Data Source={0};Version=3;UseUTF16Encoding=True;", dbFilePath);
           // CreateHistoryTables();
        }


        #region IVATRPService

        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;

        public bool Start()
        {
            if (manager.LinphoneService != null)
                manager.LinphoneService.OnLinphoneCallLogUpdatedEvent += LinphoneCallEventAdded;

            new Thread((ThreadStart)LoadLinphoneCallEvents).Start();
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);

            return true;
        }

        public bool Stop()
        {
            if (manager.LinphoneService != null)
                manager.LinphoneService.OnLinphoneCallLogUpdatedEvent -= LinphoneCallEventAdded;
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return false;
        }
        #endregion

        #region IHistoryService

        public List<VATRPCallEvent> AllCallsEvents
        {
            get
            {
                return _allCallsEvents;
            }
        }

        public void LoadLinphoneCallEvents()
        {
            if (_allCallsEvents == null)
                _allCallsEvents = new List<VATRPCallEvent>();

            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            isLoadingCalls = true;
            IntPtr callsListPtr = LinphoneAPI.linphone_core_get_call_logs(manager.LinphoneService.LinphoneCore);
            if (callsListPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList) Marshal.PtrToStructure(callsListPtr, typeof (MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        var callevent = ParseLinphoneCallLog(curStruct.data);
                        _allCallsEvents.Add(callevent);
                    }
                    callsListPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

            }
            isLoadingCalls = false;
            if (OnCallHistoryEvent != null)
            {
                var eargs = new VATRPCallEventArgs(HistoryEventTypes.Load);
                OnCallHistoryEvent(null, eargs);
            }
        }

        private VATRPCallEvent ParseLinphoneCallLog(IntPtr callLogPtr)
        {
            LinphoneCallDir direction = LinphoneAPI.linphone_call_log_get_dir(callLogPtr);
            IntPtr tmpPtr = LinphoneAPI.linphone_call_log_get_remote_address(callLogPtr);
            if (tmpPtr == IntPtr.Zero)
                return null;

            tmpPtr = LinphoneAPI.linphone_address_as_string(tmpPtr);
            if (tmpPtr == IntPtr.Zero)
                return null;

            var remoteParty = Marshal.PtrToStringAnsi(tmpPtr);
            LinphoneAPI.ortp_free(tmpPtr);

            string dn = "", un = "", host = "";
            int port = 0;
            VATRPCall.ParseSipAddressEx(remoteParty, out dn, out un, out host,
                out port);
            if (string.IsNullOrEmpty(un))
                return null;

            var callevent = new VATRPCallEvent("", un);
            callevent.DisplayName = dn;

            tmpPtr = LinphoneAPI.linphone_call_log_get_call_id(callLogPtr);
            if (tmpPtr != IntPtr.Zero)
                callevent.CallGuid = Marshal.PtrToStringAnsi(tmpPtr);
            callevent.StartTime =
                new DateTime(1970, 1, 1).AddSeconds(LinphoneAPI.linphone_call_log_get_start_date(callLogPtr));
            callevent.EndTime =
                callevent.StartTime.AddSeconds(
                    Convert.ToInt32(LinphoneAPI.linphone_call_log_get_duration(callLogPtr)));
            switch (LinphoneAPI.linphone_call_log_get_status(callLogPtr))
            {
                case LinphoneCallStatus.LinphoneCallSuccess:
                {
                    callevent.Status = direction == LinphoneCallDir.LinphoneCallIncoming
                        ? VATRPHistoryEvent.StatusType.Incoming
                        : VATRPHistoryEvent.StatusType.Outgoing;
                }
                    break;
                case LinphoneCallStatus.LinphoneCallAborted:
                    callevent.Status = VATRPHistoryEvent.StatusType.Failed;
                    break;
                case LinphoneCallStatus.LinphoneCallDeclined:
                    callevent.Status = VATRPHistoryEvent.StatusType.Rejected;
                    break;
                case LinphoneCallStatus.LinphoneCallMissed:
                    callevent.Status = VATRPHistoryEvent.StatusType.Missed;
                    break;
            }
            return callevent;
        }

        private void LinphoneCallEventAdded(IntPtr lc,  IntPtr callPtr)
        {
            if (callPtr == IntPtr.Zero || lc == IntPtr.Zero)
                return;

            var callEvent = ParseLinphoneCallLog(callPtr);

            if (callEvent != null)
            {
                if (OnCallHistoryEvent != null)
                {
                    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Add);
                    OnCallHistoryEvent(callEvent, eargs);
                }
            }
        }

        public bool IsLoadingCalls
        {
            get { return isLoadingCalls; }
        }

        public int AddCallEvent(VATRPCallEvent callEvent)
        {
            var retVal = 0;
            /*
            using (var sql_con = new SQLiteConnection(connectionString))
            {
                try
                {
                    var insertSQL =
                        new SQLiteCommand("INSERT INTO log_calls (log_id, call_guid, local, remote, log_uts, " +
                                          "duration, call_state, contact, codec) VALUES (null, @call_guid, @local, @remote, @log_uts, " +
                                          "@duration, @call_state, @contact, @codec)",
                            sql_con);
                    insertSQL.Parameters.AddWithValue("@call_guid", callEvent.CallGuid);
                    insertSQL.Parameters.AddWithValue("@local", callEvent.LocalParty);
                    insertSQL.Parameters.AddWithValue("@remote", callEvent.RemoteParty);
                    insertSQL.Parameters.AddWithValue("@log_uts", callEvent.StartTime.Ticks);
                    insertSQL.Parameters.AddWithValue("@duration", callEvent.Duration);
                    insertSQL.Parameters.AddWithValue("@call_state", callEvent.Status.ToString());
                    insertSQL.Parameters.AddWithValue("@contact", callEvent.Contact != null ? callEvent.Contact.Fullname : null);
                    insertSQL.Parameters.AddWithValue("@codec", callEvent.Codec);


                    sql_con.Open();
                    retVal = insertSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            if (retVal > 0)
            {
                if (OnCallHistoryEvent != null)
                {
                    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Add);
                    OnCallHistoryEvent(callEvent, eargs);
                }
            }*/
            return retVal;
        }

        public int DeleteCallEvent(VATRPCallEvent callEvent)
        {
            var retVal = 0;
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return 0;
            //using (var sql_con = new SQLiteConnection(connectionString))
            //{
            //    var deleteSQL = new SQLiteCommand("DELETE FROM log_calls WHERE call_guid = ?",
            //        sql_con);
            //    deleteSQL.Parameters.Add(callEvent.CallGuid);
            //    try
            //    {
            //        sql_con.Open();
            //        retVal = deleteSQL.ExecuteNonQuery();
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine("DeleteCallEvent: " + ex.ToString());
            //    }
            //}

            //if (OnCallHistoryEvent != null)
            //{
            //    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Delete);
            //    OnCallHistoryEvent(callEvent, eargs);
            //}
            return retVal;
        }

        public void ClearCallsItems()
        {
            if (manager.LinphoneService.LinphoneCore != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_clear_call_logs(manager.LinphoneService.LinphoneCore);


                //using (var sql_con = new SQLiteConnection(connectionString))
                //{
                //    var deleteSQL = new SQLiteCommand("DELETE FROM log_calls",
                //        sql_con);
                //    try
                //    {
                //        sql_con.Open();
                //        deleteSQL.ExecuteNonQuery();
                //    }
                //    catch (Exception ex)
                //    {
                //        Debug.WriteLine("DeleteCallEvent: " + ex.ToString());
                //    }
                //    finally
                //    {
                //        if (sql_con.State == ConnectionState.Open)
                //            sql_con.Close();
                //    }

                //    deleteSQL = new SQLiteCommand("VACUUM", sql_con);
                //    try
                //    {
                //        sql_con.Open();
                //        deleteSQL.ExecuteNonQuery();
                //    }
                //    catch (Exception ex)
                //    {
                //        Debug.WriteLine("VAcuum: " + ex.ToString());
                //    }
                //    finally
                //    {
                //        if (sql_con.State == ConnectionState.Open)
                //            sql_con.Close();
                //    }
                //}


                if (OnCallHistoryEvent != null)
                {
                    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Reset);
                    OnCallHistoryEvent(null, eargs);
                }
            }
        }

        #endregion
    }
}
