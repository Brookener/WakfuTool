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
using WakfuAudio.Controls;
using System.Diagnostics;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour AnimItem.xaml
    /// </summary>
    public partial class AnimItem : UserControl
    {
        public Animation animation;
        public RoutedEventHandler ScriptSelected;
        public RoutedEventHandler ScriptUnSelected;
        public RoutedEventHandler ScriptRemoved;
        public RoutedEventHandler Selected;
        public RoutedEventHandler UnSelected;
        public MonsterEdition editor;

        public AnimItem(Animation anim, MonsterEdition newEditor)
        {
            editor = newEditor;
            animation = anim;
            InitializeComponent();
            UpdateInfos();
            LoadScripts();
        }
        public void UpdateInfos()
        {
            NameBlock.Content = new TextBlock() { Text = animation.splited ? animation.SplitedName() : animation.name };
            NameBlock.ToolTip = animation.name + "\nClick to show animation.Shift + click to play sound of animation";
            SplitButton.Visibility = animation.splited ? Visibility.Collapsed : Visibility.Visible;
            MergeButton.Visibility = animation.splited ? Visibility.Visible : Visibility.Collapsed;
            MainGrid.Background = new SolidColorBrush(Animation.MediaColorFromType(animation.type, 80));
        }

        public void LoadScripts()
        {
            LoadScriptList(SoundBin, animation.sounds, ScriptType.mobAnim);
            LoadScriptList(BarkBin, animation.barks, ScriptType.mobBark);
            LoadScriptList(ApsBin, animation.aps, ScriptType.aps);
        }
        public void LoadScriptList(StackPanel bin, SortedDictionary<string, List<int>> scripts, ScriptType type)
        {
            bin.Children.Clear();
            foreach(KeyValuePair<string, List<int>> script in scripts)
            {
                var item = new ScriptItem();
                item.Load(script.Key, type, this, script.Value);
                item.Margin = new Thickness(2);
                item.GotFocus += new RoutedEventHandler(ScriptItemFocused);
                item.LostFocus += new RoutedEventHandler(ScriptItemUnFocused);
                item.Removed += new RoutedEventHandler(ScriptItemRemoved);
                bin.Children.Add(item);
            }
        }
        public void UnSelect()
        {
            SelectionBorder.Background = null;

        }

        #region Control Events

        private void ScriptItemFocused(object sender, RoutedEventArgs e)
        {
            ScriptSelected(sender, e);
            Selected(this, e);
            SelectionBorder.Background = new SolidColorBrush(Color.FromArgb(180,255,255,255));
        }
        private void ScriptItemUnFocused(object sender, RoutedEventArgs e)
        {
            ScriptUnSelected(sender, e);
            UnSelected(this, e);

        }
        private void ScriptItemRemoved(object sender, RoutedEventArgs e)
        {
            var item = (sender as ScriptItem);
            animation.Remove(item.script, item.type);
            LoadScripts();
            ScriptRemoved(this, e);
            Database.SaveDatas();
        }
        private void ControlMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            Selected(this, e);
        }
        private void AnimClick(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                animation.PlayAudio();
            else
                MonsterEdition.PlaySwf(SuperAnimExporter.ExportPath() + '\\' + animation.monster.Id + '\\' + animation.SplitedName() + ".swf");
        }
        private void AddScriptClick(object sender, RoutedEventArgs e)
        {
            animation.AddScript(false);
            LoadScripts();
            Database.SaveDatas();
        }
        private void AddBarkClick(object sender, RoutedEventArgs e)
        {
            animation.AddScript(true);
            LoadScripts();
            Database.SaveDatas();
        }
        private void AddApsClick(object sender, RoutedEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftShift))
                animation.IncrementApsScript();
            else
            {
                var ask = new AskForValue(AskForValue.ValueType.Long);
                ask.ShowDialog();
                if (ask.ok)
                    animation.AddApsScript(ask.value);
            }

            LoadScripts();
            Database.SaveDatas();
        }
        private void SplitClick(object sender, RoutedEventArgs e)
        {
            animation.monster.SplitAnimation(animation.name);
            editor.LoadMonster();
            Database.SaveDatas();
        }
        private void MergeClick(object sender, RoutedEventArgs e)
        {
            animation.monster.JoinAnimations(animation.SplitedName(), true);
            editor.LoadMonster();
            Database.SaveDatas();
        }


        #endregion


    }

}
