using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public class HistoryService : IHistoryService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (HistoryService));

        private readonly ServiceManagerBase manager;
        private readonly string connectionString;
        private readonly string dbFilePath;
        private bool isLoadingCalls;

        public event EventHandler<VATRPCallEventArgs> OnCallHistoryEvent;

        public HistoryService(ServiceManagerBase manager)
        {
            this.manager = manager;
            dbFilePath = manager.BuildStoragePath("history.db");
            connectionString = string.Format("Data Source={0};Version=3;UseUTF16Encoding=True;", dbFilePath);
            CreateHistoryTables();
        }


        #region IVATRPService
        public bool Start()
        {
            CreateHistoryTables();

            return false;
        }

        public bool Stop()
        {
            return false;
        }
        #endregion

        private void CreateHistoryTables()
        {
            if (!File.Exists(dbFilePath))
                SQLiteConnection.CreateFile(dbFilePath);

            var connection = new SQLiteConnection(connectionString);
            var cmd = new SQLiteCommand
            {
                Connection = connection
            };

            var sqlString = @"CREATE TABLE IF NOT EXISTS log_calls (
'log_id'  integer PRIMARY KEY AUTOINCREMENT NOT NULL, 'call_guid' string, 'local' string, 
 'remote' string, 'log_uts' integer, 'duration' integer default 0, 'call_state' string, 'contact' string, 
 'codec' string)";
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                cmd.CommandText = sqlString;
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("SQLite exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }

        #region IHistoryService
        public List<VATRPCallEvent> LoadCallEvents(int limit)
        {
            isLoadingCalls = true;
            var calls = new List<VATRPCallEvent>();
            var connection = new SQLiteConnection(connectionString);
            var cmd = new SQLiteCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM log_calls"
            };

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var local = reader["local"].ToString();
                        var remote = reader["remote"].ToString();
                        var callevent = new VATRPCallEvent(local, remote);

                        callevent.CallGuid = reader["call_guid"].ToString();
                        callevent.StartTime = new DateTime(Convert.ToInt64(reader["log_uts"]));

                        callevent.EndTime = callevent.StartTime.AddSeconds(Convert.ToInt32(reader["duration"]));
                        try
                        {
                            callevent.Status =
                                (VATRPHistoryEvent.StatusType)
                                    Enum.Parse(typeof (VATRPHistoryEvent.StatusType), reader["call_state"].ToString());
                        }
                        catch 
                        {
                            continue;
                        }
                        if (!reader.IsDBNull(7))
                            callevent.Contact = manager.ContactService.FindContactId(reader["contact"].ToString());
                        if (!reader.IsDBNull(8))
                            callevent.Codec = reader["codec"].ToString();
                        calls.Add(callevent);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("SQLite exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            isLoadingCalls = false;
            return calls;
        }

        public bool IsLoadingCalls
        {
            get { return isLoadingCalls; }
        }

        public int AddCallEvent(VATRPCallEvent callEvent)
        {
            var retVal = 0;
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
                    insertSQL.Parameters.AddWithValue("@contact", callEvent.Contact != null ? callEvent.Contact.ContactID : null);
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
                    OnCallHistoryEvent(callEvent, new VATRPCallEventArgs());
                }
            }
            return retVal;
        }

        public int DeleteCallEvent(string call_guid)
        {
            var retVal = 0;
            using (var sql_con = new SQLiteConnection(connectionString))
            {
                var deleteSQL = new SQLiteCommand("DELETE FROM log_calls WHERE call_guid = ?",
                    sql_con);
                deleteSQL.Parameters.Add(call_guid);
                try
                {
                    sql_con.Open();
                    retVal = deleteSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteCallEvent: " + ex.ToString());
                }
            }
            return retVal;
        }

        public void ClearCallsItems()
        {
            using (var sql_con = new SQLiteConnection(connectionString))
            {
                var deleteSQL = new SQLiteCommand("DELETE FROM log_calls",
                    sql_con);
                try
                {
                    sql_con.Open();
                    deleteSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteCallEvent: " + ex.ToString());
                }
            }
        }

        #endregion
    }
}
