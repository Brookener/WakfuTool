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
using mshtml;
using WakfuAudio.Scripts.Classes;
using System.Diagnostics;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour SwfPlayer.xaml
    /// </summary>
    public partial class SwfPlayer : UserControl
    {
        public Monster monster;
        private string currentAnim;
        private Dictionary<string, string> animations = new Dictionary<string, string>();
        private Dictionary<string, List<int>> animAngles = new Dictionary<string, List<int>>();
        private bool play = true;

        public SwfPlayer()
        {
            InitializeComponent();
            UpdateButtons();
            ButtonsPanel.Visibility = Visibility.Hidden;
        }

        public async void LoadMonster(Monster newMonster)
        {
            StopPlayer();
            monster = newMonster;
            await SuperAnimExporter.Load(monster.SwfPath());
            LoadingLabel.Visibility = Visibility.Visible;
            await SuperAnimExporter.StartExport();
            LoadingLabel.Visibility = Visibility.Collapsed;
            LoadAnimations();
            currentAnim = "";
            play = Database.parameters.playerOn;
            ButtonsPanel.Visibility = Visibility.Hidden;

        }
        private void LoadAnimations()
        {
            animations.Clear();
            animAngles.Clear();
            var path = SuperAnimExporter.ExportPath() + @"\" + monster.Id;

            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path))
            {
                var split = file.Split('\\');
                var name = split[split.Length - 1];
                name = name.Substring(0, name.Length - 4);
                animations.Add(name, file);

                if(Int32.TryParse(name[0].ToString(), out int angle) && name[1] == '_')
                {
                    name = name.Substring(2);
                    if (!animAngles.ContainsKey(name))
                        animAngles.Add(name, new List<int>());
                    animAngles[name].Add(angle);
                }
            }
            animAngles.Values.ToList().ForEach(x => x.Sort());
        }
        private void StopPlayer()
        {
            Player.Visibility = Visibility.Hidden;
        }
        public void ShowAnim(string anim)
        {
            ButtonsPanel.Visibility = Visibility.Visible;
            if (animAngles.ContainsKey(anim) && anim != currentAnim)
            {
                currentAnim = anim;
                AngleBox.ItemsSource = animAngles[anim];
                AngleBox.SelectedIndex = 0;
                Play();
            }
        }
        private void Play()
        {
            if (play)
                try
                {
                    var text = defaultHtml.Replace("PATH", animations[AngleBox.SelectedValue + "_" + currentAnim]);
                    Player.Navigate(text);
                    Player.Visibility = Visibility.Visible;
                }
                catch { }
        }
        private void UpdateButtons()
        {
            PlayButton.Visibility = play ? Visibility.Collapsed : Visibility.Visible;
            StopButton.Visibility = play ? Visibility.Visible : Visibility.Collapsed;
        }

        private const string defaultHtml = "<html><center><body scroll=no style=\"padding:0px;margin:0px;\" style=\"border:0px;\"> <embed width=100% height=100% fullscreen=yes bgcolor=\"#415F68\" align=\"middle\" src=\"file:///PATH\"></body></center></html>";

        private void PlayClick(object sender, RoutedEventArgs e)
        {
            play = true;
            Play();
            UpdateButtons();
            Database.parameters.playerOn = true;
            Database.SaveParameters();
        }
        private void StopClick(object sender, RoutedEventArgs e)
        {
            play = false;
            StopPlayer();
            UpdateButtons();
            Database.parameters.playerOn = true;
            Database.SaveParameters();
        }
        private void AngleBoxClosed(object sender, EventArgs e)
        {
            Play();
        }
    }
}
