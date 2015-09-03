using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using log4net;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace VATRP.Core.Services
{
    public partial class ContactService : IContactService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (ContactService));

        private bool _isStarting = false;
        private bool _isStarted = false;
        private bool _isStopping = false;
        private bool _isStopped = false;
        private readonly ServiceManagerBase manager;

        public ContactService(ServiceManagerBase manager)
        {
            this.manager = manager;
            _isStarting = false;
            _isStarted = false;
        }

        public bool IsStarting { get { return _isStarting; } }

        public bool IsStarted { get { return _isStarted; } }

        public bool IsStopping
         {
            get { return _isStopping; }
        }

        public bool IsStopped
        {
            get { return _isStopped; }
        }

        public bool Start()
        {
            if (IsStarting || IsStarted)
                return true;

            _isStarting = true;
            _isStarted = false;

            return true;
        }

        public bool Stop()
        {
            if (IsStopping || IsStarting)
                return false;

            if (IsStopped)
                return true;

            _isStarted = false;
            return true;
        }
    }
}
