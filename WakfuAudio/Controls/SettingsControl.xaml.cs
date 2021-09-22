using System;
using System.IO;
using System.Collections.Generic;
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
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            Setup();
        }
        private void Setup()
        {
            SvnFolderBox.Text = Database.parameters.svnFolder;
            TrunkFolderBox.Text = Database.parameters.trunkFolder;
            VolumeSlider.Value = Database.parameters.volume;
            VolumeBox.Text = Database.parameters.volume.ToString();
        }

        #region Control Events

        private void SvnFolderBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var path = (sender as TextBox).Text;

            if (Directory.Exists(path) && path != Database.parameters.svnFolder)
            {
                Database.parameters.svnFolder = path;
                Database.SaveParameters();
                Database.LoadDatas();
            }
        }
        private void SetSvnFolderClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Database.parameters.svnFolder;
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Database.parameters.svnFolder = dialog.SelectedPath;
                Database.SaveParameters();
                Database.LoadDatas();
                Setup();
            }
        }
        private void TrunkFolderBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var path = (sender as TextBox).Text;
            if (Directory.Exists(path) && path != Database.parameters.trunkFolder)
            {
                Database.parameters.trunkFolder = path;
                Database.SaveParameters();
            }
        }
        private void SetTrunkFolderClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Database.parameters.trunkFolder;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Database.parameters.trunkFolder = dialog.SelectedPath;
                Database.SaveParameters();
                Setup();
            }
        }
        private void VolumeValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Database.parameters.volume = (int)VolumeSlider.Value;
            Database.SaveParameters();
            Setup();
        }
        
        private void VolumeTextChanged(object sender, TextChangedEventArgs e)
        {
            if(Int32.TryParse(VolumeBox.Text, out int result))
            {
                Database.parameters.volume = result;
                Database.SaveParameters();
            }
            Setup();

        }

        #endregion


    }
}
