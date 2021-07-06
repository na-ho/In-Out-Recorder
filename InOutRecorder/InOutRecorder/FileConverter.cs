using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace SelectableRecorder
{
    class FileConverter
    {
        public MainWindow windows = null;
        public void startConvert(string outputFolder, string currentRecordFileWithoutExt, string currentRecordPathFile, int levelCompression, List<string> files_to_encode)
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

            ///
            if (files_to_encode != null && files_to_encode.Count != 0)
            {
                foreach (var file_to_encode in files_to_encode)
                {
                    string command_file = " -i \"";
                    command_file += file_to_encode;
                    command_file += "\"";
                    //  string output_file = file_to_encode.Remove(file_to_encode.LastIndexOf("."), file_to_encode.Length);
                    string output_file = Path.GetDirectoryName(file_to_encode) + "\\" + Path.GetFileNameWithoutExtension(file_to_encode) + ".flac";
                    if (levelCompression > 0)
                    {
                        command_file += " -compression_level " + levelCompression;
                    }

                    command_file += " \"";
                    command_file += output_file;
                    command_file += "\"";

                    startConvertProcess(command_file);
                }
            }

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
