using System;
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
    public partial class IntegrationItem : UserControl
    {
        public Integration inte;
        public RoutedEventHandler DeleteAsset;
        public RoutedEventHandler CopyAssetName;

        private SolidColorBrush volumeBaseColor;
        private SolidColorBrush assetBaseColor;

        public IntegrationItem(Integration newInte)
        {
            inte = newInte;
            InitializeComponent();
            assetBaseColor = (SolidColorBrush)AssetBox.Background;
            volumeBaseColor = (SolidColorBrush)VolumeBox.Background;
            UpdateAsset();
        }
        public void UpdateAsset()
        {
            AssetBox.Text = inte.asset;
            VolumeBox.Text = inte.volume.ToString();
            UpdateAssetColor();
        }
        public void UpdateAssetColor()
        {
            AssetBox.Foreground = inte.AssetExists() ? Constantes.White : Constantes.RedClear;
        }

        private void RemoveAssetClick(object sender, RoutedEventArgs e)
        {
            inte.script.RemoveAsset(inte.asset);
            DeleteAsset(this, e);
        }

        #region Control Events

        private void AssetBoxKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                if (inte.SetAsset(AssetBox.Text))
                {
                    AssetBox.Background = assetBaseColor;
                    inte.script.SaveScript();
                    UpdateAsset();
                }
                else
                    AssetBox.Background = Constantes.Red;
            }
            else
            {
                if(e.Key == Key.Escape)
                {
                    AssetBox.Background = assetBaseColor;
                }
                else
                {
                    AssetBox.Background = Constantes.Orange;
                }
            }
        }
        private void VolumeBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (inte.SetVolume(VolumeBox.Text))
                {
                    VolumeBox.Background = volumeBaseColor;
                    inte.script.SaveScript();
                    UpdateAssetColor();
                }
                else
                    VolumeBox.Background = Constantes.Red;
            }
            else
            {
                if (e.Key == Key.Escape)
                {
                    VolumeBox.Background = volumeBaseColor;
                    UpdateAsset();
                }
                else
                {
                    VolumeBox.Background = Constantes.Orange;
                }
            }
        }
        private void PlayAssetClick(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                MainWindow.ShowFileInExplorer(inte.AssetFile());
            else
                inte.PlayAssetSource(1);
        }
        private void CopyAssetClick(object sender, RoutedEventArgs e)
        {
            CopyAssetName(inte, e);
        }

        #endregion


    }
}
