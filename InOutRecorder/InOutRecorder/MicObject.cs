using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SelectableRecorder
{
    class MicObject : DeviceObject
    {

        public MMDevice selectedDevice = null;
        public WaveFileWriter wavWriter = null;
        public IWaveIn newWaveIn = null;

        private string currentRecordPathFile = null;
        private string currentRecordFile = null;

        public override void windowsClosed()
        {
            if (newWaveIn != null)
            {
                newWaveIn.StopRecording();
                stopListening();
            }
        }


        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (wavWriter != null)
            {
                wavWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }

            if (sampleNumber_Mic > 3)
            {
                sampleNumber_Mic = 0;
                /*  double accSample = 0.0;
                  //float max = 0;
                  for (int index = 0; index < e.BytesRecorded; index += 2)
                  {
                      short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                      // to floating point
                      var sample32 = sample / 32768f;
                      // absolute value 
                      if (sample32 < 0) sample32 = -sample32;
                      // is this the max value?
                      //if (sample32 > max) max = sample32;
                      // max = sample32;
                      accSample += sample32;
                  }
                  accSample /= e.BytesRecorded;
                 */
                windows.Dispatcher.BeginInvoke((Action)(() =>
                {
                    var volume = selectedDevice.AudioMeterInformation.MasterPeakValue;
                    int toHundredScale = (int)(100 * volume);
                    windows.bar_volume.Value = toHundredScale;
                }));
            }
            sampleNumber_Mic++;


        }

        public override void startRecord()
        {
            string outputFilename = getFileName();
            currentRecordFile = outputFilename;
            outputFilename += ".wav";
            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                windows.textBlock_fileName.Text = outputFilename;
            }));
            currentRecordPathFile = System.IO.Path.Combine(windows.TextBox_path.Text, outputFilename);
            setCapture(selectedDevice);
            wavWriter = new WaveFileWriter(filename: currentRecordPathFile, newWaveIn.WaveFormat);

            stopWatch = new Stopwatch();
            stopWatch.Start();
            SetTimer();
        }

        public override void stopRecord()
        {
            if (newWaveIn != null)
            {
                newWaveIn.StopRecording();
                newWaveIn?.Dispose();
                newWaveIn = null;
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
            setCapture(selectedDevice);
        }

        public override void stopListening()
        {
            if (newWaveIn != null)
            {
                newWaveIn.DataAvailable -= OnDataAvailable;
                newWaveIn.RecordingStopped -= OnRecordingStopped;
                newWaveIn?.Dispose();
                newWaveIn = null;
            }
        }

        public override void selectDevice_Mic(MMDevice device)
        {
            selectedDevice = device;
        }

        public override void selectDevice_Speaker(MMDevice device)
        {
        }

        private void setCapture(MMDevice device)
        {
            stopListening();
            newWaveIn = new WasapiCapture(device);
            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            newWaveIn.StartRecording();
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (wavWriter != null)
            {
                wavWriter?.Dispose();
                wavWriter = null;
                fileConverter.startConvert(windows.TextBox_path.Text, currentRecordFile, currentRecordPathFile, 12);
            }
            setCapture(selectedDevice);
        }

    }
}
