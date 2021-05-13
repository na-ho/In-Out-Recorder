using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.IO;
using NAudio.Wave.SampleProviders;

namespace SelectableRecorder
{
    /// <summary>
    /// Interaction logic for SubWindowTools.xaml
    /// </summary>
    public partial class SubWindowTools : Window
    {

        private string _loopBackPath = null;
        private string _micPath = null;
        private string _outputPath = null;
        private string _fileOutputWavFullExt = null;
        private MainWindow _mainWindow = null;

        public SubWindowTools(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void button_merge_Click(object sender, RoutedEventArgs e)
        {

            mixing();

            FileConverter fileConverter = new FileConverter();
            fileConverter.startConvert(_outputPath, Path.GetFileNameWithoutExtension(_fileOutputWavFullExt), _fileOutputWavFullExt, 12);
        }

        private void Button_output_folder_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", _outputPath);
        }

        private void TextBox_area_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        public void TextBox_area_Drop_LoopBack(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _loopBackPath = files[0];
                TextBox_area.Text = _loopBackPath;
            }
        }

        public void TextBox_area_Drop_Mic(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _micPath = files[0];
                TextBox_area1.Text = _micPath;
            }
        }

        public void TextBox_area_Drop_Output(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _outputPath = files[0];
                TextBox_area2.Text = _outputPath;
            }
        }

        public void mixing()
        {
            /*
            var mixer = new WaveMixerStream32 { AutoStop = true };
            var wav1 = new WaveFileReader(_micPath);
            var wav2 = new WaveFileReader(_loopBackPath);

            // float levelWav1_mic = 1.0f;
            // float levelWav2_loopBack = 1.0f;
            var waveChan1 = new WaveChannel32(wav1);
            // waveChan1.Volume = 2.0f;
            if (TextBox_LevelMic.Text.Length > 0)
            { 
                string strLevelMic = TextBox_LevelMic.Text;
                if(strLevelMic != "1.0")
                {
                    float fLevelMic = float.Parse(strLevelMic);
                    waveChan1.Volume = fLevelMic;
                }
               
            }
            mixer.AddInputStream(waveChan1);
            var waveChan2 = new WaveChannel32(wav2);
            // waveChan2.Volume = 0.5f;
            if (TextBox_levelLoopBack.Text.Length > 0)
            {
                string strlevelLoopBack = TextBox_levelLoopBack.Text;
                if (strlevelLoopBack != "1.0")
                {
                    float fLevelLoopBack = float.Parse(strlevelLoopBack);
                    waveChan2.Volume = fLevelLoopBack;
                }
            }
            mixer.AddInputStream(waveChan2);
            _outputPath = TextBox_area2.Text;

            _fileOutputWavFullExt = System.IO.Path.Combine(_outputPath, Path.GetFileName(_micPath).Replace("_mic", ""));
            WaveFileWriter.CreateWaveFile(_fileOutputWavFullExt, mixer);

            */
            ////////////////////////////////////////////////////////////////////////////////
            const int rate = 48000;
            //const int bits = 32;
            const int channels = 2;
            //WaveFormat wave_format = new WaveFormat(rate, bits, channels);
            WaveFormat wave_format = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var wav1 = new AudioFileReader(_micPath);
            var wav2 = new AudioFileReader(_loopBackPath);

            if (TextBox_LevelMic.Text.Length > 0)
            {
                string strLevelMic = TextBox_LevelMic.Text;
                if (strLevelMic != "1.0")
                {
                    float fLevelMic = float.Parse(strLevelMic);
                    wav1.Volume = fLevelMic;
                }

            }
            if (TextBox_levelLoopBack.Text.Length > 0)
            {
                string strlevelLoopBack = TextBox_levelLoopBack.Text;
                if (strlevelLoopBack != "1.0")
                {
                    float fLevelLoopBack = float.Parse(strlevelLoopBack);
                    wav2.Volume = fLevelLoopBack;
                }
            }
            var resampler2 = new MediaFoundationResampler(wav2, wave_format);
            var mixer = new MixingSampleProvider(wave_format);
            //      var mixer = new MixingSampleProvider(new[] { wav1, wav2 });
            mixer.AddMixerInput(resampler2);
            mixer.AddMixerInput(wav1.ToWaveProvider());

            _outputPath = TextBox_area2.Text;
            

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_outputPath.Length == 0)
                {
                    _outputPath = _mainWindow.TextBox_path.Text;
                    TextBox_area2.Text = _outputPath;

                }
                _fileOutputWavFullExt = System.IO.Path.Combine(_outputPath, Path.GetFileName(_micPath).Replace("_mic", ""));
                WaveFileWriter.CreateWaveFile(_fileOutputWavFullExt, mixer.ToWaveProvider());
            }));

            
        }

    }
}
