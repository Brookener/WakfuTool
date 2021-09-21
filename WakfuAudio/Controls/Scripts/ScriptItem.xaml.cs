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
    public partial class ScriptItem : UserControl
    {
        public string script;
        public ScriptType type;
        public List<int> startFrames = new List<int>();
        private Brush initialFrameBackground;
        private Brush initialIdBackground;
        private AnimItem animItem;

        public RoutedEventHandler Removed;

        public ScriptItem(string newScript, ScriptType newType, AnimItem newAnim, List<int> newFrames)
        {
            type = newType;
            animItem = newAnim;
            startFrames = newFrames;
            script = newScript;
            InitializeComponent();
            Setup();
        }
        private async void Setup()
        {
            Selector.Visibility = Visibility.Hidden;
            initialFrameBackground = FrameBox.Background;
            UpdateInfos();
        }
        public void UpdateInfos()
        {
            IdBox.Text = script;
            FrameBox.Text = FrameChain();
            FrameBox.Background = initialFrameBackground;
            IdBox.Foreground = Database.ScriptExists(script) ? Constantes.White : Constantes.RedClear;
            WarningIcon.Visibility = Database.GetOrCreate(script).HasMissingAsset() ? Visibility.Visible : Visibility.Collapsed;
        }
        public string FrameChain()
        {
            if (startFrames == null)
                return "";
            else
                return String.Join(",", startFrames);
        }
        private bool SetFrames(string chain)
        {
            var list = new List<int>();
            var split = chain.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string s in split)
                if(Int32.TryParse(s, out int result))
                    list.Add(result);
                else
                    return false;

            list.Sort();
            startFrames = list;
            Database.SaveDatas();
            return true;
        }

        #region Control Events

        
        private void ControlMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }
        private void ControlFocused(object sender, RoutedEventArgs e)
        {
            Selector.Visibility = Visibility.Visible;
        }
        private void ControlUnfocused(object sender, RoutedEventArgs e)
        {
            Selector.Visibility = Visibility.Hidden;

        }
        private void FrameBoxLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateInfos();
        }
        private void RemoveScriptClick(object sender, RoutedEventArgs e)
        {
            Removed(this, e);
        }
        private void FrameBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (SetFrames(FrameBox.Text))
                {
                    FrameBox.Background = initialFrameBackground;
                    UpdateInfos();

                }
                else
                    FrameBox.Background = Constantes.Red;
            }
            else
            {
                FrameBox.Background = Constantes.Orange;
            }
        }
        private void IdBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (IdBox.Text == script) return;

            if (e.Key == Key.Return)
            {
                if (Int64.TryParse(IdBox.Text, out long result))
                {
                    var newScript = result.ToString();
                    animItem.animation.Replace(script, newScript, type);
                    animItem.editor.ScriptEdition.Update(Database.GetOrCreate(script));
                    IdBox.Background = initialIdBackground;
                }
                else
                    IdBox.Background = Constantes.Red;
            }
            else
            {
                if (e.Key == Key.Escape)
                {
                    IdBox.Text = script;
                    IdBox.Background = initialIdBackground;
                }
                else
                    IdBox.Background = Constantes.Orange;
            }
        }




        #endregion


    }
}
