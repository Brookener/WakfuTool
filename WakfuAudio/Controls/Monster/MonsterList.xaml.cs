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
using WakfuAudio.Controls;
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour MonsterList.xaml
    /// </summary>
    public partial class MonsterList : UserControl
    {
        public Monster currentMonster;
        public RoutedEventHandler MonsterSelection;
        public Monster.Type type;

        public MonsterList()
        {
            InitializeComponent();
            Setup();
        }
        private void Setup()
        {
            ContextMenu = new ContextMenu();
            var reload = new MenuItem() { Header = "Reload list", ToolTip = "Refresh the monster list" };
            reload.Click += new RoutedEventHandler(ReloadClick);
            ContextMenu.Items.Add(reload);

            var update = new MenuItem() { Header = "Check for monsters to Update", ToolTip = "Check for monsters .swf files that have an more recent modification date than the last time it was exported from this tool." };
            update.Click += new RoutedEventHandler(CheckForMonstersToUpdate);
            ContextMenu.Items.Add(update);

            var global = new MenuItem() { Header = "Load all from Swf files" };
            global.Click += new RoutedEventHandler(StartGlobalLoading);
            ContextMenu.Items.Add(global);
        }
        public void LoadList()
        {
            ListTree.Items.Clear();

            string groupPath = Database.AnimSourcesFolder() + "\\" + type;
            var directory = new DirectoryInfo(groupPath);
            var exports = new Dictionary<string, FileInfo>();
            if (!Directory.Exists(Database.AnimExportFolder()))
            {
                return;
            }
            new DirectoryInfo(Database.AnimExportFolder() + "\\" + type).GetFiles("*.swf").ToList().ForEach(x => exports.Add(x.Name.Substring(0, x.Name.Length - 4), x));

            var directories = directory.GetDirectories("*", SearchOption.AllDirectories).ToList();
            directories.Add(directory);

            foreach (DirectoryInfo group in directories)
            {
                string prefix = "";
                if (group.Parent.Name != directory.Name)
                    prefix = group.Parent.Name + "_";

                string groupName = prefix + group.Name;

                var groupItem = new TreeViewItem()
                {
                    Header = groupName,
                    Foreground = Constantes.White, 
                };
                ListTree.Items.Add(groupItem);

                foreach (FileInfo swf in group.GetFiles("*.fla"))
                {
                    var id = swf.Name.Substring(0, swf.Name.Length - 4);
                    if (!exports.ContainsKey(id)) continue;
                    var monster = Database.datas.monsters.ContainsKey(id) ? Database.datas.monsters[id] : new Monster(id, groupName, type);
                    if(monster.Name == "")
                        monster.Name = Database.NameOf(id);
                    monster.LoadInteractiveDialog();
                    var item = new TreeViewItem()
                    {
                        Header = monster.FullName(),
                        Tag = monster,
                        Foreground = Constantes.White,

                    };
                    item.Selected += new RoutedEventHandler(ItemSelected);
                    item.MouseDoubleClick += new MouseButtonEventHandler(ItemDoubleClick);
                    item.KeyDown += new KeyEventHandler(MonsterKeyDown);
                    groupItem.Items.Add(item);
                }

                groupItem.Visibility = (groupItem.Items.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public void UpdateList(string filter)
        {
            foreach(TreeViewItem group in ListTree.Items)
            {
                group.Visibility = Visibility.Collapsed;
                if ((group.Header as string).IndexOf(filter) > -1)
                    group.Visibility = Visibility.Visible;

                foreach (TreeViewItem item in group.Items)
                    if (Utils.StringContains(item.Header as string, filter))
                        item.Visibility = group.Visibility = Visibility.Visible;
                    else
                        item.Visibility = Visibility.Collapsed;
            }
            
        }
        private void UpdateInfos()
        {
            MonsterView.Width = ActualWidth;
            MonsterView.Source = currentMonster.GetImage();
            IdBox.Text = currentMonster.Id;
        }
        public void SelectedItem(TreeViewItem selected)
        {
            foreach (TreeViewItem item in ListTree.Items)
            {
                item.IsExpanded = item == selected;
            }
            selected.IsSelected = true;
        }


        #region Control Events

        private void ReloadClick(object sender, RoutedEventArgs e)
        {
            LoadList();
        }
        private void FilterKeyUp(object sender, KeyEventArgs e)
        {
            UpdateList(FilterBox.Text);
        }
        private void MonsterKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                MonsterSelection((sender as TreeViewItem).Tag, e);
        }
        private void ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MonsterSelection((sender as TreeViewItem).Tag, e);
        }
        public void ItemSelected(object sender, RoutedEventArgs e)
        {
            currentMonster = (sender as TreeViewItem).Tag as Monster;
            UpdateInfos();
        }
        private void LoadMonsterNamesClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            if(dialog.FileName.Substring(dialog.FileName.Length - 4) != ".csv")
            {
                MessageBox.Show("File must be a .csv");
                return;
            }
            var values = File.ReadAllText(dialog.FileName).Split('\n');
            var dic = new Dictionary<string, List<string>>();
            foreach(string s in values)
            {
                var split = s.Split(',');
                if (split.Length < 2) continue;
                if (!dic.ContainsKey(split[0]))
                    dic.Add(split[0], new List<string>());
                dic[split[0]].Add(split[1]);
            }
            foreach(Monster m in Database.datas.monsters.Values)
            {
                if (dic.ContainsKey(m.Id))
                    m.Name = dic[m.Id][0];
            }
            LoadList();
            Database.SaveDatas();
        }
        private void StartGlobalLoading(object sender, RoutedEventArgs e)
        {
            new MonsterLoader().Show();
        }
        private void CheckForMonstersToUpdate(object sender, RoutedEventArgs e)
        {
            Database.CheckIfMonstersAreUpToDate();
        }

        #endregion

        static List<string[]> ReadCsv(string file, char separator, bool removeEmpty)
        {
            var table = new List<string[]>();
            var reader = new StreamReader(file);
            while (!reader.EndOfStream)
                table.Add(reader.ReadLine().Split(new char[] { separator }, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None));
            return table;
        }
    }
}
