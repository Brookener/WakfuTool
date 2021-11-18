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
    /// Logique d'interaction pour AssetList.xaml
    /// </summary>
    public partial class AssetList : UserControl
    {
        private Dictionary<string, AssetListItem> items = new Dictionary<string, AssetListItem>();

        public AssetList()
        {
            InitializeComponent();
            SetupCartegpryFilter();
        }

        private void SetupCartegpryFilter()
        {
            var list = new DirectoryInfo(Database.AudioFolder(true)).GetDirectories().Select(x=> x.Name).ToList();
            list.Insert(0, "All");
            CategoryFilter.ItemsSource = list;
            CategoryFilter.SelectedIndex = 0;
        }

        public void LoadList()
        {
            LoadList(SearchPatern.Text);
        }
        public async void LoadList(string filter)
        {
            ResultBox.Text = "Loading...";
            await Task.Delay(10);

            items.Clear();
            var list = Database.AssetFilesByIds();

            var exports = Database.ExportFilesByAsset();
            var sourcesFiles = Database.SourcesByAsset();
            var dic = Database.AssetInScriptsDictionnary().ToDictionary(x => x.Key, x => String.Join("\n", x.Value));

            foreach (KeyValuePair<string,string> asset in 
                list.Where(x => 
                (CategoryFilter.SelectedIndex == 0 || new FileInfo(x.Value).Directory.Name == CategoryFilter.SelectedValue.ToString()) &&
                (filter == "" || Utils.StringContains(x.Key, filter))))
            {
                items.Add(asset.Key, new AssetListItem()
                {
                    Asset = asset.Key,
                    Usage = dic.ContainsKey(asset.Key) ? dic[asset.Key] : "",
                    AssetInGame = exports.ContainsKey(asset.Key),
                    sources = sourcesFiles.ContainsKey(asset.Key) ? sourcesFiles[asset.Key] : new List<string>(),
                    Modification = exports.ContainsKey(asset.Key) ? new FileInfo(exports[asset.Key]).LastWriteTime.ToString() : "",
                });
            }

            AssetPanel.ItemsSource = null;
            AssetPanel.ItemsSource = items.Values;
            ResultBox.Text = items.Count == 0 ? "" : items.Count + " Results";
        }

        #region Control Events 

        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                LoadList();
        }
        private void ListKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                (AssetPanel.SelectedItems[0] as AssetListItem).PlayAudio();
            }
        }
        private void LoadScriptFromFolder(object sender, RoutedEventArgs e)
        {
            Database.LoadAllScriptsFromFolder();
        }
        private void CategoryFilterDropdownClosed(object sender, EventArgs e)
        {
            LoadList();
        }

        #endregion

        public class AssetListItem
        {
            public string Asset { get; set; }
            public string Usage { get; set; }
            public bool AssetInGame { get; set; }
            public List<string> sources;
            public string SourcesCount
            {
                get => sources?.Count.ToString();
                set => SourcesCount = value;
            }
            public string Modification { get; set; }

            public void PlayAudio()
            {
                if (sources.Count > 0)
                    AudioPlayer.PlayAudio(sources[0], 1);
            }

        }
    }
}
