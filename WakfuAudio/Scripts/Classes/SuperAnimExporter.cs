using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace WakfuAudio.Scripts.Classes
{

    public class SuperAnimExporter
    {
        private bool stopExport = false;
        private static Process process;

        public static string FolderPath()
        {
            return Directory.GetCurrentDirectory() + @"\Resources\SuperAnimExporter";
        }
        public static string ExePath()
        {
            return FolderPath() + @"\SuperAnimExporter.bat";
        }
        public static string AnimWakfuPath()
        {
            return FolderPath() + @"\" + "anim_wakfu.xml";
        }
        public static string ExportPath()
        {
            Directory.CreateDirectory(FolderPath() + @"\export");
            return FolderPath() + @"\export";
        }

        public static void UpdateSkinPath()
        {
            string file = File.ReadAllText(AnimWakfuPath());
            string path = Utils.GetStringFromPaterns(file, "links path = \"", "\"");
            string newPath = FolderPath() + @"\" + "skin_wakfu/";
            file = Utils.ReplaceBetweenPaterns(file, newPath, "links path=\"", "\"");
            File.WriteAllText(AnimWakfuPath(), file);
        }
        public static async Task ClearAllExports()
        {
            while (process != null && !process.HasExited)
                await Task.Delay(10);
            foreach (string folder in Directory.GetDirectories(ExportPath()))
                Directory.Delete(folder, true);
        }
        public static void ClearAllSwf()
        {
            foreach (string file in Directory.GetFiles(FolderPath()))
                if (file.Substring(file.Length - 4) == ".swf")
                    File.Delete(file);
        }
        public static async Task Load(string swfFile)
        {
            if (!File.Exists(swfFile))
            {
                MessageBox.Show("Can't find swf file :\n" + swfFile);
                return;
            }
            ClearAllSwf();
            await ClearAllExports();
            var newFile = FolderPath() + @"\" + ExtractFileName(swfFile);
            File.Copy(swfFile, newFile);
        }
        public static async Task StartExport()
        {
            UpdateSkinPath();
            ProcessStartInfo info = new ProcessStartInfo(ExePath(), "4");
            info.WorkingDirectory = FolderPath();
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            process = Process.Start(info);
            while (!process.HasExited)
                await Task.Delay(100);
        }


        private static string ExtractFileName(string file)
        {
            var split = file.Split('\\');
            return split[split.Length - 1];
        }
    }
}
