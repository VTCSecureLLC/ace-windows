using System;
using System.Media;
using System.Timers;
using VATRP.Core.Interfaces;

namespace VATRP.Core.Services
{
    public partial class SoundService : ISoundService
    {
        private readonly ServiceManagerBase manager;

        private SoundPlayer ringTonePlayer;
        private SoundPlayer ringBackTonePlayer;
        private SoundPlayer eventPlayer;
        private SoundPlayer connPlayer;
        private bool isRingTonePlaying;
        private bool isRingbackPlaying;

        public SoundService(ServiceManagerBase manager)
        {
            this.manager = manager;
        }

        public bool IsStarted { get; set; }

        public bool IsStopped { get; set; }

        public bool Start()
        {
            if (IsStarted)
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
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            if (IsStopped)
                return true;

            return true;
        }

        public void PlayRingTone()
        {
            if (isRingTonePlaying)
                return;
            this.ringTonePlayer.PlayLooping();
        }

        public void StopRingTone()
        {
            if (!isRingTonePlaying)
                return;
            isRingTonePlaying = false;
            this.ringTonePlayer.Stop();
        }

        public void PlayRingBackTone()
        {
            if (isRingbackPlaying)
                return;
            isRingbackPlaying = true;
            this.ringBackTonePlayer.PlayLooping();
        }

        public void StopRingBackTone()
        {
            if (!isRingbackPlaying)
                return;
            isRingbackPlaying = false;
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
