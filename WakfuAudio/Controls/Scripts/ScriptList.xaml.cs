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
    /// Logique d'interaction pour ScriptList.xaml
    /// </summary>
    public partial class ScriptList : UserControl
    {
        private List<LuaDataGridItem> items = new List<LuaDataGridItem>();


        public ScriptList()
        {
            InitializeComponent();
        }

        public async Task LoadList(string filter)
        {
            items.Clear();
            foreach (KeyValuePair<string, LuaScript> file in Database.datas.scripts)
            {
                if (file.Value.SearchFilter(filter))
                {
                    var item = new LuaDataGridItem(file.Value);
                    items.Add(item);
                }
            }

            ScriptPanel.ItemsSource = null;
            ScriptPanel.ItemsSource = items;
            ResultBox.Text = items.Count == 0 ? "" : items.Count + " Results";
        }
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


    }

    public class LuaDataGridItem
    {
        public string Id { get; set; }
        public ScriptType Type { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }

        public LuaScript script;

        public LuaDataGridItem(LuaScript newScript)
        {
            script = newScript;
            Description = script.description;
            UpdateValues();
        }

        public void UpdateValues()
        {
            Id = script.id;
            Type = script.type;
            Usage = String.Join(", ", script.MonstersUsing().Select(x => x.Id));
        }
    }
}
