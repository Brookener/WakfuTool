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
using WakfuAudio.Controls;
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour PlayerList.xaml
    /// </summary>
    public partial class PlayerList : UserControl
    {
        private Dictionary<string, TreeViewItem> playerItems = new Dictionary<string, TreeViewItem>();
        private Dictionary<string, TreeViewItem> groupItems = new Dictionary<string, TreeViewItem>();


        public PlayerList()
        {
            InitializeComponent();
            LoadList();
        }

        public void LoadList()
        {
            ListTree.Items.Clear();
            playerItems.Clear();
            groupItems.Clear();

            var exports = new Dictionary<string, FileInfo>();
            Database.AllPlayerAnimFile().ToList().ForEach(x => exports.Add(x.Name.Substring(0, x.Name.Length - 4), x));

            TreeViewItem item;
            foreach (DirectoryInfo group in Database.AllPlayerGroupAnimFolders())
            {
                item = new TreeViewItem()
                {
                    Header = group.Name,
                };
                ListTree.Items.Add(item);
                groupItems.Add(group.Name, item);

                string name;

                foreach(FileInfo file in group.GetFiles())
                {
                    name = file.Name.Substring(0, file.Name.Length - 4);
                    if (!exports.ContainsKey(name)) continue;

                    item = new TreeViewItem()
                    {
                        Header = name,
                    };
                    groupItems[group.Name].Items.Add(item);
                    playerItems.Add(file.Name, item);
                }
            }

           

            return;

            foreach(FileInfo file in Database.AllPlayerAnimFile())
            {
                item = new TreeViewItem()
                {
                    Header = file.Name.Substring(0, file.Name.Length - 4),
                };
                ListTree.Items.Add(item);
            }
        }

        private void FilterKeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
