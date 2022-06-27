using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Timers;

namespace SelectableRecorder
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private int countUpdate = 0;

        private bool bRecording = false;

        private List<MMDevice> listDevices_Microphone;
        private List<MMDevice> listDevices_Speaker;

        private AppInfo appInfo = new AppInfo();

        private MicObject micObject = null;
        private MicSpeakerObject micSpeakerObject = null;

        private DeviceObject selectedMethod = null;

        private FileConverter fileConverter = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += new EventHandler(MainWindow_Closed);

            Startup();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            selectedMethod.stopRecord();
            selectedMethod.windowsClosed();
            // save json
            saveAppInfo();

            Application.Current.Shutdown();
        }

        private void loadAppInfo()
        {
            using (FileStream reader = new FileStream("app.json",
            FileMode.Open, FileAccess.Read))
            {
                appInfo = Utf8Json.JsonSerializer.Deserialize<AppInfo>(reader, Utf8Json.Resolvers.StandardResolver.Default);
                TextBox_path.Text = appInfo.strOutputPath;

                this.ComboBox_comp_level.SelectedIndex = appInfo.compression_level;
            }

        }

        private void saveAppInfo()
        {
            appInfo.deviceIndex = cmb_devices.SelectedIndex;
            appInfo.strOutputPath = TextBox_path.Text;
            appInfo.deviceTwoSourceIndexMic = cmb_deviceSelectTwoSource_1.SelectedIndex;
            appInfo.deviceTwoSourceIndexSpeaker = cmb_deviceSelectTwoSource_2.SelectedIndex;

            appInfo.strDeviceName = cmb_devices.Text;
            appInfo.strTwoSourceMicName = cmb_deviceSelectTwoSource_1.Text;
            appInfo.strTwoSourceSpeakerDeviceName = cmb_deviceSelectTwoSource_2.Text;
            appInfo.compression_level = ComboBox_comp_level.SelectedIndex;

            using (FileStream writer = new FileStream("app.json", FileMode.Create))
            {
                // Utf8Json.JsonSerializer.Serialize(writer, appInfo, Utf8Json.Resolvers.StandardResolver.Default);
                var result = Utf8Json.JsonSerializer.Serialize(appInfo);
                var pretty = Utf8Json.JsonSerializer.PrettyPrintByteArray(result);
                //Utf8Json.JsonSerializer.Serialize(writer, pretty);
                writer.Write(pretty, 0, pretty.Length);
            }
        }

        public void Startup()
        {
            loadAppInfo();
            InitItemsProperty();

            fileConverter = new FileConverter();
            fileConverter.windows = this;

            micObject = new MicObject();
            micSpeakerObject = new MicSpeakerObject();

            micObject.windows = this;
            micSpeakerObject.windows = this;

            micObject.fileConverter = fileConverter;
            micSpeakerObject.fileConverter = fileConverter;

            selectedMethod = micSpeakerObject;

            startWASAPI();
            setBaseInfo();


        }


        private void InitItemsProperty()
        {
            for(int index = 0; index <= 12; ++index)
            {
                if(index == 5)
                {
                    this.ComboBox_comp_level.Items.Add(index + " (default)");
                } else
                {
                    this.ComboBox_comp_level.Items.Add(index);
                }
                
            }
        }

        private void setBaseInfo()
        {
            textBlock_fileName.Text = selectedMethod.getFileName(".flac");
        }

        public void startWASAPI()
        {
            var deviceEnum = new MMDeviceEnumerator();
            listDevices_Microphone = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                cmb_devices.ItemsSource = listDevices_Microphone;
                if (cmb_devices.Items.Count > 0)
                {
                    if (appInfo.deviceIndex >= listDevices_Microphone.Count || appInfo.deviceIndex == -1)
                    {
                        appInfo.deviceIndex = 0;
                        cmb_devices.SelectedIndex = appInfo.deviceIndex;
                    }
                    else
                    {
                        cmb_devices.SelectedIndex = appInfo.deviceIndex;
                    }

                    micObject.selectDevice_Mic(listDevices_Microphone[appInfo.deviceIndex]);
                }
            }));

            listDevices_Speaker = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                cmb_deviceSelectTwoSource_1.ItemsSource = listDevices_Microphone;
                cmb_deviceSelectTwoSource_2.ItemsSource = listDevices_Speaker;

                //if (cmb_deviceSelectTwoSource_1.Items.Count > 0)
                //{
                //    if (appInfo.deviceTwoSourceIndexMic >= listDevices_Microphone.Count || appInfo.deviceTwoSourceIndexMic == -1)
                //    {
                //        appInfo.deviceTwoSourceIndexMic = 0;
                //        cmb_deviceSelectTwoSource_1.SelectedIndex = appInfo.deviceTwoSourceIndexMic;
                //    }
                //    else
                //    {
                //        cmb_deviceSelectTwoSource_1.SelectedIndex = appInfo.deviceTwoSourceIndexMic;
                //    }

                //    micSpeakerObject.selectDevice_Mic(listDevices_Microphone[appInfo.deviceTwoSourceIndexMic]);
                //}

                //if (cmb_deviceSelectTwoSource_2.Items.Count > 0)
                //{
                //    if (appInfo.deviceTwoSourceIndexSpeaker >= listDevices_Speaker.Count || appInfo.deviceTwoSourceIndexSpeaker == -1)
                //    {
                //        appInfo.deviceTwoSourceIndexSpeaker = 0;
                //        cmb_deviceSelectTwoSource_2.SelectedIndex = appInfo.deviceTwoSourceIndexSpeaker;
                //    }
                //    else
                //    {
                //        cmb_deviceSelectTwoSource_2.SelectedIndex = appInfo.deviceTwoSourceIndexSpeaker;
                //    }

                //    micSpeakerObject.selectDevice_Speaker(listDevices_Speaker[appInfo.deviceTwoSourceIndexSpeaker]);
                //}

                if (cmb_deviceSelectTwoSource_1.Items.Count > 0)
                {
                    //if (appInfo.deviceTwoSourceIndexMic >= listDevices_Microphone.Count || appInfo.deviceTwoSourceIndexMic == -1)
                    //{
                    //    appInfo.deviceTwoSourceIndexMic = 0;
                    //    cmb_deviceSelectTwoSource_1.SelectedIndex = appInfo.deviceTwoSourceIndexMic;
                    //}
                    //else
                    //{
                    //    cmb_deviceSelectTwoSource_1.SelectedIndex = appInfo.deviceTwoSourceIndexMic;
                    //}
                    (var device_microphone, int index_microphone) = searchListMMDevice(appInfo.strTwoSourceMicName, listDevices_Microphone);
                    appInfo.deviceTwoSourceIndexMic = index_microphone;
                    if (device_microphone != null)
                    {
                        micSpeakerObject.selectDevice_Mic(device_microphone);
                        cmb_deviceSelectTwoSource_1.SelectedIndex = index_microphone;
                    }
                    else
                    {
                        appInfo.strTwoSourceMicName = listDevices_Microphone[index_microphone].FriendlyName;
                        cmb_deviceSelectTwoSource_1.SelectedIndex = appInfo.deviceTwoSourceIndexMic;
                        micSpeakerObject.selectDevice_Mic(listDevices_Microphone[index_microphone]);
                    }

                }

                if (cmb_deviceSelectTwoSource_2.Items.Count > 0)
                {
                    //if (appInfo.deviceTwoSourceIndexSpeaker >= listDevices_Speaker.Count || appInfo.deviceTwoSourceIndexSpeaker == -1)
                    //{
                    //    appInfo.deviceTwoSourceIndexSpeaker = 0;
                    //    cmb_deviceSelectTwoSource_2.SelectedIndex = appInfo.deviceTwoSourceIndexSpeaker;
                    //}
                    //else
                    //{
                    //    cmb_deviceSelectTwoSource_2.SelectedIndex = appInfo.deviceTwoSourceIndexSpeaker;
                    //}

                    //micSpeakerObject.selectDevice_Speaker(listDevices_Speaker[appInfo.deviceTwoSourceIndexSpeaker]);

                    (var device_speaker, int index_speaker) = searchListMMDevice(appInfo.strTwoSourceSpeakerDeviceName, listDevices_Speaker);
                    appInfo.deviceTwoSourceIndexSpeaker = index_speaker;
                    if (device_speaker != null)
                    {
                        micSpeakerObject.selectDevice_Speaker(device_speaker);
                        cmb_deviceSelectTwoSource_2.SelectedIndex = index_speaker;
                    }
                    else
                    {

                        cmb_deviceSelectTwoSource_2.SelectedIndex = appInfo.deviceTwoSourceIndexSpeaker;
                        appInfo.strTwoSourceSpeakerDeviceName = listDevices_Speaker[index_speaker].FriendlyName;
                        micSpeakerObject.selectDevice_Speaker(listDevices_Speaker[index_speaker]);
                    }
                }

                micSpeakerObject.startListening();
            }));
        }

        //private (MMDevice, int) searchListMMDevice(string keyword, List<MMDevice> list)
        //{
        //    if (keyword == null) return (null,0);
        //    // foreach (var device in list)
        //    for (int idx_device = 0; idx_device < list.Count; idx_device++)
        //    {
        //        if (list[idx_device].FriendlyName == keyword)
        //        {
        //            return (list[idx_device], idx_device);
        //        }
        //    }
        //    return (null, 0);
        //}

        private (MMDevice, int) searchListMMDevice(string keyword, List<MMDevice> list)
        {
            if (keyword == null) return (null, 0);
            // foreach (var device in list)
            //for (int idx_device = 0; idx_device < list.Count; idx_device++)
            //{
            //    if (list[idx_device].FriendlyName == keyword)
            //    {
            //        return (list[idx_device], idx_device);
            //    }
            //}

            List<int> matchList = new List<int>();
            for (int idx_device_loop = 0; idx_device_loop < list.Count; idx_device_loop++)
            {
                matchList.Add(Utils.LevenshteinDistance(list[idx_device_loop].FriendlyName, keyword));
            }
            int idx_device = matchList.IndexOf(matchList.Min());
            //   string match = tracksList.ElementAt();
            return (list[idx_device], idx_device);
            //  return (null, 0);
        }

        private void button_record_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_path.Text != "")
            {
                if (cmb_devices.SelectedItem != null)
                {
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (bRecording == false)
                        {
                            button_record.Content = "Stop";
                            button_record.Background = Brushes.Red;
                            bRecording = true;
                            selectedMethod.startRecord();
                        }
                        else
                        {
                            button_record.Background = Brushes.LightGray;
                            button_record.Content = "Record";
                            bRecording = false;
                            selectedMethod.stopRecordWithStartListening();
                        }
                    }));
                }
            }
        }

        private void cmb_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            string yourValue = String.Empty;

            if (cbx.SelectedValue != null)
            {
                selectedMethod.selectDevice_Mic(listDevices_Microphone[cbx.SelectedIndex]);
            }

        }
        private void cmb_devices_SelectionChangedTwoSource_1(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;

            if (cbx.SelectedValue != null)
            {
                selectedMethod.selectDevice_Mic(listDevices_Microphone[cbx.SelectedIndex]);
            }
        }

        private void cmb_devices_SelectionChangedTwoSource_2(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;

            if (cbx.SelectedValue != null)
            {
                selectedMethod.selectDevice_Speaker(listDevices_Speaker[cbx.SelectedIndex]);
            }

        }

        private void radio_twoSource_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            if ((bool)radioButton.IsChecked)
            {
                if (radio_oneSource != null)
                {
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        radio_oneSource.IsChecked = false;
                        selectedMethod.stopRecord();
                        selectedMethod.stopListening();
                        selectedMethod.resetVolumeBar();
                        selectedMethod = micSpeakerObject;
                        selectedMethod.startListening();
                    }));
                }
            }
        }

        private void radio_oneSource_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            if ((bool)radioButton.IsChecked)
            {
                if (radio_twoSource != null)
                {
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        radio_twoSource.IsChecked = false;
                        selectedMethod.stopRecord();
                        selectedMethod.stopListening();
                        selectedMethod.resetVolumeBar();
                        selectedMethod = micObject;
                        selectedMethod.startListening();
                    }));
                }
            }
        }

        private void Button_saveFloder_Click(object sender, RoutedEventArgs e)
        {
            string strOutputDirectory = TextBox_path.Text;
            Process.Start("explorer.exe", @strOutputDirectory);
        }

        private void Button_tools_Click(object sender, RoutedEventArgs e)
        {
            SubWindowTools subWindow = new SubWindowTools(this);
            subWindow.Show();
        }

        public static implicit operator MainWindow(SubWindowTools v)
        {
            throw new NotImplementedException();
        }
    }
}
