using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace SelectableRecorder
{
    abstract class DeviceObject
    {
        public MainWindow windows = null;
        public FileConverter fileConverter = null;

        protected System.Timers.Timer aTimer = null;
        protected Stopwatch stopWatch = null;
        protected int sampleNumber_Mic = 0;

        protected bool startListeningAfterStop = true;

        public abstract void startRecord();
        public abstract void stopRecord();
        public abstract void stopRecordWithStartListening();
        public abstract void startListening();
        public abstract void stopListening();

        public abstract void selectDevice_Mic(MMDevice device);
        public abstract void selectDevice_Speaker(MMDevice device);

        public abstract void windowsClosed();

        public void SetTimer()
        {
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Hours, ts.Minutes, ts.Seconds);

            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                windows.textBlock_timer.Text = elapsedTime;
            }));
        }

        public string getCurrentTimeFormat()
        {
            DateTime now = DateTime.Now;

            string ret = String.Format("{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
               now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            return ret;
        }

        public string getFileName(string extension)
        {
            return getCurrentTimeFormat() + extension;
        }
        public string getFileName()
        {
            return getCurrentTimeFormat();
        }

        public void resetVolumeBar()
        {
            windows.Dispatcher.BeginInvoke((Action)(() =>
            {
                windows.bar_volume.Value = 0;
                windows.bar_volume_Speaker.Value = 0;
            }));
        }

        public string getCurrentStopWatchElapsedTime()
        {
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Hours, ts.Minutes, ts.Seconds);

            return elapsedTime;
        }

    }
}
