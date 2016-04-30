using System;
using System.Diagnostics;
using System.Linq;
using NAudio.Mixer;
using NAudio.Wave;

namespace com.vtcsecure.ace.windows.Model
{
    public class AudioRecorder
    {
        WaveIn waveIn;
        //readonly SampleAggregator sampleAggregator;
        UnsignedMixerControl volumeControl;
        double desiredVolume = 100;
        bool _isMonitoring;
        WaveFormat recordingFormat;
        readonly SampleAggregator sampleAggregator;
        public event EventHandler Stopped = delegate { };
        
        public AudioRecorder()
        {
            sampleAggregator = new SampleAggregator();
            recordingFormat = new WaveFormat(44100, 1);
            sampleAggregator.NotificationCount = recordingFormat.SampleRate / 10;
        }

        public void BeginMonitoring(int recordingDevice)
        {
            if (_isMonitoring)
                return;
            Debug.WriteLine("Dtart monitoring");
            waveIn = new WaveIn();
            waveIn.DeviceNumber = recordingDevice;
            waveIn.WaveFormat = recordingFormat;
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
            TryGetVolumeControl();
            _isMonitoring = true;
        }

        public void Stop()
        {
            if (_isMonitoring)
            {
                Debug.WriteLine("Stop monitoring");
                _isMonitoring = false;
                waveIn.StopRecording();
            }
        }

        private void TryGetVolumeControl()
        {
            int waveInDeviceNumber = waveIn.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = waveIn.GetMixerLine();
                //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
                foreach (var control in mixerLine.Controls)
                {
                    if (control.ControlType == MixerControlType.Volume)
                    {
                        this.volumeControl = control as UnsignedMixerControl;
                        MicrophoneLevel = desiredVolume;
                        break;
                    }
                }
            }
            else
            {
                var mixer = new Mixer(waveInDeviceNumber);
                foreach (var destination in mixer.Destinations
                    .Where(d => d.ComponentType == MixerLineComponentType.DestinationWaveIn))
                {
                    foreach (var source in destination.Sources
                        .Where(source => source.ComponentType == MixerLineComponentType.SourceMicrophone))
                    {
                        foreach (var control in source.Controls
                            .Where(control => control.ControlType == MixerControlType.Volume))
                        {
                            volumeControl = control as UnsignedMixerControl;
                            MicrophoneLevel = desiredVolume;
                            break;
                        }
                    }
                }
            }

        }

        public double MicrophoneLevel
        {
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                if (volumeControl != null)
                {
                    volumeControl.Percent = value;
                }
            }
        }

        public bool IsMonitoring
        {
            get
            {
                return _isMonitoring;
            }
        }

        public SampleAggregator SampleAggregator
        {
            get
            {
                return sampleAggregator;
            }
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            _isMonitoring = false;
            Stopped(this, EventArgs.Empty);
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }
    }
}
