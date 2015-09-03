using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using log4net;
using VATRP.Core.Interfaces;

namespace VATRP.Core.Services
{
    public class HistoryService : IHistoryService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (HistoryService));

        private readonly ServiceManagerBase manager;

        private bool _isStarting;
        private bool _isStarted;
        private bool _isStopping;
        private bool _isStopped;
        private string connectionString = "history.db";
        
        public HistoryService(ServiceManagerBase manager)
        {
            this.manager = manager;
            _isStarting = false;
            _isStarted = false;

            CreateHistoryTables();
        }


        #region IService
        public bool IsStarting
        {
            get
            {
                return _isStarting;
            }
        }

        public bool IsStarted
        {
            get
            {
                return _isStarted;
            }
        }

        public bool IsStopping
        {
            get
            {
                return _isStopping;
            }
        }

        public bool IsStopped
        {
            get
            {
                _isStopped = true;
                return _isStopped;
            }
        }
        #endregion

        #region IVATRPService
        public bool Start()
        {
            if (IsStarting)
                return false;

            if (IsStarted)
                return true;

            LoadCalls();
            LoadMessages();
            return false;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;

            if (IsStopped)
                return true;
            _isStopping = true;
            _isStopped = true;
            return false;
        }
        #endregion

        private void CreateHistoryTables()
        {
            var connection = new SQLiteConnection(connectionString);
            var sqlString = string.Empty;
            var cmd = new SQLiteCommand
            {
                Connection = connection
            };

            sqlString = @"CREATE TABLE IF NOT EXISTS log_calls (
'log_id'  integer NOT NULL, 'caller' string, 'callee' string, 'start_uts' integer, 'call_state' string,
PRIMARY KEY ('log_id' ASC)";
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
            }

            sqlString = @"CREATE TABLE IF NOT EXISTS log_messages (
'log_id'  integer NOT NULL, 'sender' string, 'receiver' string, 'message_uts' integer, 'call_state' string, 'message' string,
PRIMARY KEY ('log_id' ASC)";
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
            }

            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }

        private void LoadMessages()
        {

        }

        private void LoadCalls()
        {
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

                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("SQLite exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
            }
        }
    }
}
