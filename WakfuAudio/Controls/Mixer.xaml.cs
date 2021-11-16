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
    /// Logique d'interaction pour Mixer.xaml
    /// </summary>
    public partial class Mixer : UserControl
    {
        public Mixer()
        {
            InitializeComponent();
        }

        public void Add(string file)
        {
            var player = new AudioPlayerControl(file);
            player.Height = 59;
            PlayerBin.Children.Add(player);
            var del = new Button()
            {
                Tag = player,
                Background = Model.Background,
            };
            del.Click += new RoutedEventHandler(DeleteClick);
            del.Height = 59;
            DeleteBin.Children.Add(del);
        }
        public void New(string file)
        {
            Add(file);
            if (Database.parameters.audioPlayers == null)
                Database.parameters.audioPlayers = new List<string>();
            Database.parameters.audioPlayers.Add(file);
            Database.SaveParameters();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var player = (sender as Button).Tag as AudioPlayerControl;
            Database.parameters.audioPlayers?.Remove(player.file);
            PlayerBin.Children.Remove(player);
            DeleteBin.Children.Remove(sender as Button);
            Database.SaveParameters();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            files.Where(x => x.Substring(x.Length - 4) == ".wav").ToList().ForEach(x => New(x));
        }
    }
}
