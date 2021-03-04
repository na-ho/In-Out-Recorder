using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SelectableRecorder
{
    class FileConverter
    {
        public MainWindow windows = null;
        public void startConvert(string outputFolder, string currentRecordFileWithoutExt, string currentRecordPathFile, int levelCompression)
        {
            string outputFilename = currentRecordFileWithoutExt + ".flac";
            if (windows != null)
            {
                windows.Dispatcher.BeginInvoke((Action)(() =>
                {
                    windows.textBlock_fileName.Text = outputFilename;
                }));
            }

            string outputFullPath = System.IO.Path.Combine(outputFolder, outputFilename);

            string command = " -i \"";
            command += currentRecordPathFile;
            command += "\"";

            if (levelCompression > 0)
            {
                command += " -compression_level " + levelCompression;
            }

            command += " \"";
            command += outputFullPath;
            command += "\"";

            startConvertProcess(command);

        }

        public void startConvertProcess(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = command;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }
    }
}
