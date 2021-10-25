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

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour ScriptList.xaml
    /// </summary>
    public partial class ScriptList : UserControl
    {
        public EventHandler SelectionChanged;

        private List<LuaDataGridItem> items = new List<LuaDataGridItem>();


        public ScriptList()
        {
            InitializeComponent();
            SetupCartegpryFilter();
        }
        private void SetupCartegpryFilter()
        {
            var list = Enum.GetNames(typeof(ScriptType)).ToList();
            list.Insert(0, "All");
            CategoryFilter.ItemsSource = list;
            CategoryFilter.SelectedIndex = 0;
        }

        public void LoadList()
        {
            LoadList(SearchPatern.Text);
        }
        public async Task LoadList(string filter)
        {
            ResultBox.Text = "Loading...";
            await Task.Delay(10);
            items.Clear();
            foreach (KeyValuePair<string, LuaScript> file in Database.datas.scripts)
            {
                if (file.Value.SearchFilter(filter) && (CategoryFilter.SelectedIndex == 0 || CategoryFilter.SelectedValue.ToString() == file.Value.type.ToString()))
                {
                    var item = new LuaDataGridItem(file.Value);
                    items.Add(item);
                }
            }

            ScriptPanel.ItemsSource = null;
            ScriptPanel.ItemsSource = items;
            ResultBox.Text = items.Count == 0 ? "" : items.Count + " Results";
        }

        #region Control Events 

        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                LoadList(SearchPatern.Text);
        }
        private void ListKeyUp(object sender, KeyEventArgs e)
        {
            
        }
        private void LoadScriptFromFolder(object sender, RoutedEventArgs e)
        {
            Database.LoadAllScriptsFromFolder();
        }
        private void PanelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = new List<LuaScript>();
            foreach(var o in ScriptPanel.SelectedItems)
                list.Add((o as LuaDataGridItem).script);
            SelectionChanged(list, e);
        }
        private void CategoryFilterDropdownClosed(object sender, EventArgs e)
        {
            LoadList();
        }

        #endregion

        public class LuaDataGridItem
        {
            public string Id { get; set; }
            public ScriptType Type { get; set; }
            public string Usage { get; set; }
            public string Modification
            {
                get => new FileInfo(script.FilePath()).LastWriteTime.ToString();
            }
            public string MissingAsset
            {
                get => script.MissingAssets().Count.ToString();
            }

            public LuaScript script;

            public LuaDataGridItem(LuaScript newScript)
            {
                script = newScript;
                UpdateValues();
            }

            public void UpdateValues()
            {
                Id = script.id;
                Type = script.type;
                Usage = String.Join("\n", script.MonstersUsing().Select(x => x.Id));
            }
        }
    }

    
}
