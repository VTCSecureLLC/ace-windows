using VATRP.Core.Interfaces;

namespace VATRP.Core.Services
{
    public partial class SoundService : ISoundService
    {
        private readonly ServiceManagerBase manager;

        private bool _isStarting;
        private bool _isStarted;
        private bool _isStopping;
        private bool _isStopped;

        public SoundService(ServiceManagerBase manager)
        {
            this.manager = manager;
            _isStarting = false;
            _isStarted = false;
            _isStopped = true;
            _isStopping = false;
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

            return true;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping )
                return false;
            if (IsStopped)
                return true;

            return true;
        }
    }
}
