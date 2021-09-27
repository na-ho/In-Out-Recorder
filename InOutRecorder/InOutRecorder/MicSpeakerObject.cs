using System;
using System.Collections.Generic;
using System.Text;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace SelectableRecorder
{
    class MicSpeakerObject : DeviceObject
    {
        [Flags]
        public enum statusFlag
        {
            None = 0,
            Mic = 1,
            Loopback = 2,

        }

        private MMDevice device_mic;
        private MMDevice device_loopback;
        WaveFileWriter writer_Mic;
        WaveFileWriter writer_Loopback;
        IWaveIn newWaveIn_Mic;
        IWaveIn newWaveIn_Loopback;
        WasapiOut wasapiOut_Loopback;

        public MMDevice Mic { get => device_mic; set => device_mic = value; }
        public MMDevice Loopback { get => device_loopback; set => device_loopback = value; }

        ////////////////////////////
        string currentRecordFile_mic;
        string currentRecordFile_loopback;

        string currentRecordPathFile_micExt;
        string currentRecordPathFile_loopbackExt;

        string currentRecordPathFile_mixed;
        string currentRecordPathFile_mixedExt;

        statusFlag statusRecording = statusFlag.None;

        bool recording = false;
        /// ////////////////////////////////////////////////////////////////////

        public override void windowsClosed()
        {
            if (newWaveIn_Mic != null)
            {
                newWaveIn_Mic.StopRecording();
            }

            if (newWaveIn_Loopback != null)
            {
                newWaveIn_Loopback.StopRecording();
            }

            stopListening();
        }

        public override void startRecord()
        {
            string rootFile = getFileName();
            currentRecordFile_mic = rootFile + "_mic";
            currentRecordFile_loopback = rootFile + "_loopback";
            currentRecordPathFile_mixed = rootFile;
            currentRecordPathFile_mixedExt = System.IO.Path.Combine(windows.TextBox_path.Text, currentRecordPathFile_mixed + ".wav");

            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                windows.textBlock_fileName.Text = currentRecordFile_mic;
            }));
            currentRecordPathFile_micExt = System.IO.Path.Combine(windows.TextBox_path.Text, currentRecordFile_mic + ".wav");
            currentRecordPathFile_loopbackExt = System.IO.Path.Combine(windows.TextBox_path.Text, currentRecordFile_loopback + ".wav");

            setCapture(device_mic, device_loopback);
            writer_Mic = new WaveFileWriter(filename: currentRecordPathFile_micExt, newWaveIn_Mic.WaveFormat);
            writer_Loopback = new WaveFileWriter(filename: currentRecordPathFile_loopbackExt, newWaveIn_Loopback.WaveFormat);


            /////////
            stopWatch = new Stopwatch();
            stopWatch.Start();
            SetTimer();

            recording = true;
        }

        public override void stopRecord()
        {
            if (newWaveIn_Mic != null)
            {
                newWaveIn_Mic.StopRecording();
            }

            if (newWaveIn_Loopback != null)
            {
                newWaveIn_Loopback.StopRecording();
            }


            if (stopWatch != null)
            {
                stopWatch.Stop();

            }
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Dispose();
            }

            startListeningAfterStop = false;
        }

        public override void stopRecordWithStartListening()
        {
            stopRecord();
            startListeningAfterStop = true;
        }

        public override void startListening()
        {
            setCapture(device_mic, device_loopback);
        }

        public void stopListening_Mic()
        {
            if (newWaveIn_Mic != null)
            {
                newWaveIn_Mic.DataAvailable -= OnDataAvailable_Mic;
                newWaveIn_Mic.RecordingStopped -= OnRecordingStopped_Mic;
                newWaveIn_Mic?.Dispose();
                newWaveIn_Mic = null;
            }
        }

        public void stopListening_Loopback()
        {
            if (newWaveIn_Loopback != null)
            {
                newWaveIn_Loopback.DataAvailable -= OnDataAvailable_LoopBack;
                newWaveIn_Loopback.RecordingStopped -= OnRecordingStopped_LoopBack;
                newWaveIn_Loopback?.Dispose();
                newWaveIn_Loopback = null;

                wasapiOut_Loopback.Stop();
                wasapiOut_Loopback = null;
            }
        }

        public override void stopListening()
        {
            stopListening_Mic();
            stopListening_Loopback();
        }

        private void setCapture(MMDevice deviceMic, MMDevice deviceSpeaker)
        {
            if (deviceMic != null)
            {
                stopListening_Mic();
                newWaveIn_Mic = new WasapiCapture(deviceMic);
                newWaveIn_Mic.DataAvailable += OnDataAvailable_Mic;
                newWaveIn_Mic.RecordingStopped += OnRecordingStopped_Mic;
                newWaveIn_Mic.StartRecording();
            }

            if (deviceSpeaker != null)
            {
                stopListening_Loopback();
                newWaveIn_Loopback = new WasapiLoopbackCapture(deviceSpeaker);

                var silenceProvider = new SilenceProvider(newWaveIn_Loopback.WaveFormat);
                wasapiOut_Loopback = new WasapiOut(deviceSpeaker, AudioClientShareMode.Shared, false, 0);
                wasapiOut_Loopback.Init(silenceProvider);
                

                newWaveIn_Loopback.DataAvailable += OnDataAvailable_LoopBack;
                newWaveIn_Loopback.RecordingStopped += OnRecordingStopped_LoopBack;

                wasapiOut_Loopback.Play();
                newWaveIn_Loopback.StartRecording();

                //using (wasapiOut_Loopback = new WasapiOut(deviceSpeaker, AudioClientShareMode.Shared, false, 0))
                //{
                //    wasapiOut_Loopback.Init(silenceProvider);
                //    wasapiOut_Loopback.Play();
                //}

            }

        }

        private void OnRecordingStopped_Mic(object sender, StoppedEventArgs e)
        {
            // throw new NotImplementedException();
            if (writer_Mic != null)
            {
                writer_Mic?.Dispose();
                writer_Mic = null;
            }
            if (startListeningAfterStop)
            {
                setCapture(device_mic, null);
            }

            statusRecording |= statusFlag.Mic;

            checkMixing();
        }

        private void OnRecordingStopped_LoopBack(object sender, StoppedEventArgs e)
        {
            // throw new NotImplementedException();
            if (writer_Loopback != null)
            {
                writer_Loopback?.Dispose();
                writer_Loopback = null;
            }
            if (startListeningAfterStop)
            {
                setCapture(null, device_loopback);
            }

            statusRecording |= statusFlag.Loopback;

            checkMixing();
        }


        private void OnDataAvailable_Mic(object sender, WaveInEventArgs e)
        {
            if (writer_Mic != null)
            {
                writer_Mic.Write(e.Buffer, 0, e.BytesRecorded);
                /*
                 BufferedWaveProvider buffered = new BufferedWaveProvider(newWaveIn_Mic.WaveFormat);
                 buffered.AddSamples(e.Buffer, 0, e.BytesRecorded);
                 VolumeSampleProvider  volume = (VolumeSampleProvider)buffered.ToSampleProvider();
                 volume.Volume = 0.5f;
                 buffered.Read(e.Buffer, 0, e.BytesRecorded);
                 writer_Mic.Write(e.Buffer, 0, e.BytesRecorded);*/

                //writer_Mic.Write(e.Buffer, 0, e.BytesRecorded);

            }

            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                var volume = device_mic.AudioMeterInformation.MasterPeakValue;
                int toHundredScale = (int)(100 * volume);
                windows.bar_volume.Value = toHundredScale;
            }));
        }


        private void OnDataAvailable_LoopBack(object sender, WaveInEventArgs e)
        {
            if (writer_Loopback != null)
            {
                writer_Loopback.Write(e.Buffer, 0, e.BytesRecorded);
            }

            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                var volume = device_loopback.AudioMeterInformation.MasterPeakValue;
                int toHundredScale = (int)(100 * volume);
                windows.bar_volume_Speaker.Value = toHundredScale;
            }));
        }


        public override void selectDevice_Mic(MMDevice device)
        {
            device_mic = device;

        }

        public override void selectDevice_Speaker(MMDevice device)
        {
            device_loopback = device;
        }

        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void checkMixing()
        {
            if (recording)
            {
                if (statusRecording.HasFlag(statusFlag.Mic | statusFlag.Loopback))
                {
                    recording = false;
                    statusRecording = statusFlag.None;
                    mixing();
                    List<string> files_list = null;
                    if (windows.checkBox_convertMicSpeaker.IsChecked.Value)
                    {
                        files_list = new List<string>
                            { currentRecordPathFile_micExt , currentRecordPathFile_loopbackExt };
                    }
                   
                    fileConverter.startConvert(windows.TextBox_path.Text, currentRecordPathFile_mixed, currentRecordPathFile_mixedExt, 12, files_list);
                }
            }
        }
        public void mixing()
        {
            Thread.Sleep(2000);

            //var mixer = new WaveMixerStream32 { AutoStop = true };
            //var wav1 = new WaveFileReader(currentRecordPathFile_micExt);
            //var wav2 = new WaveFileReader(currentRecordPathFile_loopbackExt);

            //var waveChan1 = new WaveChannel32(wav1);
            ////  waveChan1.Volume = 10.0f;
            //mixer.AddInputStream(waveChan1);
            //var waveChan2 = new WaveChannel32(wav2);
            //waveChan2.Volume = 0.5f;
            //mixer.AddInputStream(waveChan2);
            //WaveFileWriter.CreateWaveFile(currentRecordPathFile_mixedExt, mixer);

            const int rate = 48000;
            //const int bits = 32;
            const int channels = 2;
            //WaveFormat wave_format = new WaveFormat(rate, bits, channels);
            WaveFormat wave_format = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var wav1 = new AudioFileReader(currentRecordPathFile_micExt);
            var wav2 = new AudioFileReader(currentRecordPathFile_loopbackExt);

           // var mixer = new MixingSampleProvider(wave_format);
            var mixer = new MixingSampleProvider(new[] { wav1, wav2 });
            //mixer.AddMixerInput(wav1.ToWaveProvider());
            //   mixer.AddMixerInput(wav2.ToWaveProvider());
            WaveFileWriter.CreateWaveFile(currentRecordPathFile_mixedExt, mixer.ToWaveProvider());
        }
    }
}
