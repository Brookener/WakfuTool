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
    /// <summary>
    /// Logique d'interaction pour ScriptItem.xaml
    /// </summary>
    public partial class ScriptEdition : UserControl
    {
        public LuaScript script;
        public RoutedEventHandler AddAsset;
        public RoutedEventHandler ScriptCreated;
        public RoutedEventHandler CopyAssetName;
        public ScriptEdition()
        {
            InitializeComponent();
            TitlePanel.Visibility = Visibility.Visible;
        }

        public void Update()
        {
            Update(script);
        }
        public void Update(LuaScript newScript)
        {
            script = newScript;

            if (script == null)
            {
                TitlePanel.Visibility = Visibility.Visible;
                TitleLabel.Content = "Script Editor";
                return;
            }
            TypeBox.Text = script.type.ToString();
            switch(script.type)
            {
                default:
                    if (LuaScript.Editable(script.type))
                    {
                        TitlePanel.Visibility = Visibility.Collapsed;
                        Parameters.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TitlePanel.Visibility = Visibility.Visible;
                        TitleLabel.Content = "Special Script";
                        return;
                    }
                    break;
                case ScriptType.dialog:
                    Parameters.Visibility = Visibility.Collapsed;
                    TitlePanel.Visibility = Visibility.Collapsed;
                    break;
            }


            IdBox.Text = script.id;
            StopBox.IsChecked = script.stop;
            RolloffBox.Text = script.rolloff.ToString();
            UpdateAssetList();

            if (script.ScriptFileExists())
            {
                CreateScriptButton.Visibility = Visibility.Collapsed;
                Parameters.Visibility = Visibility.Visible;
                OpenScriptButton.Visibility = Visibility.Visible;
            }
            else
            {
                CreateScriptButton.Visibility = Visibility.Visible;
                OpenScriptButton.Visibility = Visibility.Collapsed;
                Parameters.Visibility = Visibility.Collapsed;
            }
        }
        public void UpdateAssetList()
        {
            AssetList.Children.Clear();
            foreach(Integration inte in script.SortedIntegrations())
            {
                NewAssetitem(inte);
            }
        }
        private IntegrationItem NewAssetitem(Integration inte)
        {
            var item = new IntegrationItem(inte);
            item.DeleteAsset += new RoutedEventHandler(DeleteAsset);
            item.CopyAssetName += new RoutedEventHandler(CopyAssetNameClick);
            AssetList.Children.Add(item);

            return item;
        }
        
        public void SetType(ScriptType newType)
        {
            script.type = newType;
        }

        #region Control Events

        private void DeleteAsset(object sender, RoutedEventArgs e)
        {
            script.SaveScript();
            UpdateAssetList();
            Database.SaveDatas();
        }
        private void TypeBoxClosed(object sender, EventArgs e)
        {
            SetType((ScriptType)(sender as ComboBox).SelectedIndex);
            Database.SaveDatas();
        }
        private void AddAssetClick(object sender, RoutedEventArgs e)
        {
            AddAsset(script, e);
            script.SaveScript();
            UpdateAssetList();
            Database.SaveDatas();
        }
        public void OpenScriptFile(object sender, RoutedEventArgs e)
        {
            script.OpenFile();
        }
        private void StopClicked(object sender, RoutedEventArgs e)
        {
            script.stop = (bool)StopBox.IsChecked;
            script.SaveScript();
        }
        private void RolloffBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                if (script.SetRolloff((sender as TextBox).Text))
                {
                    RolloffBox.Background = Constantes.White;
                    script.SaveScript();
                    Database.SaveDatas();
                }
                else
                    RolloffBox.Background = Constantes.Red;
            else
                RolloffBox.Background = Constantes.Orange;
        }
        private void OpenScriptClick(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                script.ShowFileInExplorer();
            else
                script.OpenFile();
        }
        private void CreateScriptClick(object sender, RoutedEventArgs e)
        {
            if (script != null)
            {
                script.SaveScript();
                ScriptCreated(script, e);
                Update();
            }
        }
        private void CopyAssetNameClick(object sender, RoutedEventArgs e)
        {
            CopyAssetName(sender, e);
        }

        #endregion


    }
}
