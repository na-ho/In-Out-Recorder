using System;
using System.Collections.Generic;
using System.Text;

namespace SelectableRecorder
{
    public class AppInfo
    {
        public int deviceIndex = 0;
        public string strDeviceName = null;
        public string strOutputPath = null;

        public int deviceTwoSourceIndexMic = 0;
        public int deviceTwoSourceIndexSpeaker = 0;

        public string strTwoSourceMicName = null;
        public string strTwoSourceSpeakerDeviceName = null;

        public int compression_level = 5;
    }
}
