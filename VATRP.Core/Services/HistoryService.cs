using System;
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

        public HistoryService(ServiceManagerBase manager)
        {
            this.manager = manager;
            _isStarting = false;
            _isStarted = false;

            var historyDirPath = this.manager.BuildStoragePath("History");

            try
            {
                if (!Directory.Exists(historyDirPath))
                    Directory.CreateDirectory(historyDirPath);
            }
            catch (Exception ex)
            {
                LOG.Debug("Failed to create history path");
            }
        }


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

        public bool Start()
        {
            if (IsStarting)
                return false;

            if (IsStarted)
                return true;

            return false;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;

            if (IsStopped)
                return true;

            return false;
        }

    }
}
