using System;
using System.Media;
using System.Timers;
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

        private SoundPlayer ringTonePlayer;
        private SoundPlayer ringBackTonePlayer;
        private SoundPlayer eventPlayer;
        private SoundPlayer connPlayer;

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
            try
            {
                this.ringTonePlayer = new SoundPlayer(Properties.Resources.ringtone);
                this.ringBackTonePlayer = new SoundPlayer(Properties.Resources.ringbacktone);
                this.eventPlayer = new SoundPlayer(Properties.Resources.newmsg);
                this.connPlayer = new SoundPlayer(Properties.Resources.connevent);               
            }
            catch (Exception e)
            {
                _isStarting = false;
                _isStarted = false;
                return false;
            }

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

        public void PlayRingTone()
        {
            this.ringTonePlayer.PlayLooping();
        }

        public void StopRingTone()
        {
            this.ringTonePlayer.Stop();
        }

        public void PlayRingBackTone()
        {
            this.ringBackTonePlayer.PlayLooping();
        }

        public void StopRingBackTone()
        {
            this.ringBackTonePlayer.Stop();
        }

        public void PlayNewEvent()
        {
            this.eventPlayer.Play();
        }

        public void StopNewEvent()
        {
            this.eventPlayer.Stop();
        }

        public void PlayConnectionChanged(bool connected)
        {
            this.connPlayer.Play();
        }

        public void StopConnectionChanged(bool connected)
        {
            this.connPlayer.Stop();
        }

    }
}
