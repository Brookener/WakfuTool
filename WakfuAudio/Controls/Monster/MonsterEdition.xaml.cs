using System;
using System.Collections.Generic;
using System.IO;
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
    /// Logique d'interaction pour MonsterEdition.xaml
    /// </summary>
    public partial class MonsterEdition : UserControl
    {
        public Monster monster;
        public Animation animSelected;
        public Dictionary<Animation, AnimItem> items = new Dictionary<Animation, AnimItem>();
        private SwfDecompiler decompiler;

        public RoutedEventHandler MonsterLoaded;
        public RoutedEventHandler MonsterRenamed;

        public static Process playedSwf;

        public MonsterEdition()
        {
            InitializeComponent();
            Setup();
        }
        private void Setup()
        {
            SetContextMenu();
            ScriptEdition.AddAsset += new RoutedEventHandler(AddAssetClick);
            ScriptEdition.ScriptCreated += new RoutedEventHandler(CreateMissingScript);
            ScriptEdition.CopyAssetName += new RoutedEventHandler(CopyAssetNameClick);
        }
        private void SetContextMenu()
        {
            AnimGrid.ContextMenu = new ContextMenu();
            var load = new MenuItem() { Header = "Load from Swf file" };
            load.Click += new RoutedEventHandler(LoadFromSwfClick);
            AnimGrid.ContextMenu.Items.Add(load);

        }

        public void LoadMonster()
        {
            LoadMonster(monster);
        }
        public async void LoadMonster(Monster m)
        {
            monster = m;
            UpdateMonsterInfos();
            UpdateBackground();
            UpdateAnimList();
            ScriptEdition.Update(null);
            animSelected = null;
            if(monster != null)
            {
                decompiler = new SwfDecompiler(monster.SwfPath());
                SaveButton.Visibility = Visibility.Visible;
                RefreshButton.Visibility = Visibility.Visible;
                CopyAnimsButton.Visibility = Visibility.Visible;
                Database.parameters.lastMonsterOpened = monster.Id;
                Database.SaveParameters();
                MonsterLoaded(this, null);
                await SuperAnimExporter.Load(monster.SwfPath());
                SuperAnimExporter.StartExport();
            }
            else
            {
                SaveButton.Visibility = Visibility.Hidden;
                RefreshButton.Visibility = Visibility.Hidden;
                CopyAnimsButton.Visibility = Visibility.Hidden;
            }
        }
        public void UpdateMonsterInfos()
        {
            FlaButton.Visibility = monster == null ? Visibility.Hidden : Visibility.Visible;
            FamilyLabel.Text = monster == null ? "" : monster.Family;
            NameLabel.Text = monster == null ? "" : monster.Name;
            IdLabel.Text = monster == null ? "" : monster.Id;
        }
        public void UpdateBackground()
        {
            if (monster == null) return;
            AnimGrid.Background = new ImageBrush(monster.GetImage()) { Stretch = Stretch.UniformToFill};
        }
        public void UpdateAnimList()
        {
            items.Clear();
            AnimBin.Children.Clear();
            if (monster == null) return;

            foreach(Animation anim in monster.SortedAnimations())
            {
                var item = new AnimItem(anim, this);
                item.Tag = anim;
                item.Selected += new RoutedEventHandler(AnimItemSelected);
                item.ScriptSelected += new RoutedEventHandler(ScriptItemSelected);
                item.ScriptRemoved += new RoutedEventHandler(ScriptRemoved);
                AnimBin.Children.Add(item);
                items.Add(anim, item);
            }
        }
        public void AddAssetToScript(LuaScript lua)
        {
            Integration inte;
            switch (lua.type)
            {
                case ScriptType.aps:
                    inte = new Integration(lua, lua.FirstApsAsset("410"));
                    break;
                default:
                    bool bark = lua.type == ScriptType.mobBark;
                    inte = new Integration(lua, bark ? monster.FirstBarkAsset() : monster.FirstSoundAsset(animSelected.type));
                    break;
            }
            if (lua.integrations.Count > 0)
                inte.volume = lua.integrations[lua.integrations.Count - 1].volume;

            lua.integrations.Add(inte);
        }
        public void CopyAnimsToClipboard()
        {
            
            
        }

        #region Control Events

        private void LoadFromSwfClick(object sender, RoutedEventArgs e)
        {
            if (monster != null)
            {
                monster.LoadFromSwf(decompiler);
                UpdateMonsterInfos();
                UpdateAnimList();
            }
        }
        private void ScriptItemSelected(object sender, RoutedEventArgs e)
        {
            var item = sender as ScriptItem;
            Database.GetOrCreate(item.script, out LuaScript script);
            ScriptEdition.Update(script);
        }
        private void ScriptRemoved(object sender, RoutedEventArgs e)
        {
            ScriptEdition.Update(null);
        }
        private void AddAssetClick(object sender, RoutedEventArgs e)
        {
            AddAssetToScript(sender as LuaScript);
        }
        private void AnimItemSelected(object sender, RoutedEventArgs e)
        {
            var anim = (sender as AnimItem).Tag as Animation;
            animSelected = anim;
        }
        private void OpenFlaClick(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                monster.ShowAnimateFileInExplorer();
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    monster.ShowSwfFileInExplorer();
                else
                    monster.OpenAnimateFile();
            }
        }
        private void NameBoxKeyUp(object sender, KeyEventArgs e)
        {
            monster.Name = (sender as TextBox).Text;
            Database.SaveDatas();
            MonsterRenamed(monster, e);
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            monster.SaveSwf(decompiler);
        }
        private void CreateMissingScript(object sender, RoutedEventArgs e)
        {
            AddAssetToScript(sender as LuaScript);
            UpdateAnimList();
        }
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            LoadMonster(monster);
        }
        private void CopyAnimsClick(object sender, RoutedEventArgs e)
        {
            var path = SuperAnimExporter.ExportPath() + @"\" + monster.Id;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Can't find anims folder\n" + path);
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                var files = Directory.GetFiles(path);
                var coll = new System.Collections.Specialized.StringCollection();

                foreach (string file in files)
                {
                    coll.Add(file);
                }
                Clipboard.SetFileDropList(coll);
            }

        }
        private void CopyAssetNameClick(object sender, RoutedEventArgs e)
        {
            var inte = sender as Integration;
            var name = new string[]
            {
                inte.asset,
                monster.Name.Replace(" ",""),
                animSelected.name.Substring(animSelected.splited ? 2 : 0),
        
            };
            Clipboard.SetText(String.Join("_", name.Where(x => x != "")));
        }


        #endregion

        #region Utils

        public static void PlaySwf(string file)
        {
            if(playedSwf != null && !playedSwf.HasExited) playedSwf.Kill();
            if (File.Exists(file))
                playedSwf = Process.Start(file);
            else
                MessageBox.Show(file);
        }

        #endregion

    }
}
