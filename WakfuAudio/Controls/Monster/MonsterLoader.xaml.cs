using System;
using System.Runtime;
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
using System.Windows.Shapes;
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio.Controls
{
    /// <summary>
    /// Logique d'interaction pour MonsterLoader.xaml
    /// </summary>
    public partial class MonsterLoader : Window
    {
        bool stop = false;
        public MonsterLoader()
        {
            InitializeComponent();
        }

        private async Task StartLoading()
        {
            stop = false;
            StartTimer();
            SwfDecompiler decompiler;
            int monsterCount = 1;

            foreach(Monster monster in Database.datas.monsters.Values)
            {
                FamilyInfo.Text = "Family : " + monster.Family;
                if (CantWrite(monster.Id))
                {
                    total--;
                    continue;
                }
                MonsterInfo.Text = "Monster : " + monster.Id + " " + monster.Name;
                decompiler = new SwfDecompiler(monster.SwfPath());
                decompiler.showErrors = (bool)ShowErrorBox.IsChecked;
                await Task.Run(() => monster.LoadFromSwf(decompiler));
                monsterCount++;
                UpdateRemainig();

                count++;
                if (stop)
                    return;
            }
                    
            stop = true;
            Close();
        }
        private bool CantWrite(string id)
        {
            return !(bool)OverwritteBox.IsChecked && Database.datas.monsters.ContainsKey(id) && Database.datas.monsters[id].animations.Count > 0;
        }

        TimeSpan current;
        double total;
        double count;
        double timePerMonster;
        TimeSpan remaining;
        private async Task StartTimer()
        {
            total = Database.AllMonstersAnimFile().Count;
            count = 1;
            var startTime = DateTime.Now;

            while (!stop)
            {
                current = DateTime.Now.Subtract(startTime);
                TimeBox.Text = TimeSpanChain(current) + " Remaining : " + TimeSpanChain(remaining);

                await Task.Delay(100);
            }
        }
        private void UpdateRemainig()
        {
            timePerMonster = current.TotalSeconds / count;
            var seconds = (timePerMonster * total) - current.TotalSeconds;
            remaining = TimeSpan.FromSeconds(seconds);
        }
        private string TimeSpanChain(TimeSpan t)
        {
            return (t.Minutes.ToString().Length == 1 ? "0" : "") + t.Minutes + ":" + (t.Seconds.ToString().Length == 1 ? "0" : "") + t.Seconds;
        }

        private void StopLoadingClick(object sender, RoutedEventArgs e)
        {
            stop = true;
            Close();
        }

        private void OnCLose(object sender, EventArgs e)
        {
            stop = true;
        }
        private void StartLoadingClick(object sender, RoutedEventArgs e)
        {
            StartLoading();
            StartButton.Visibility = Visibility.Collapsed;
            OverwritteBox.IsEnabled = false;
        }
    }
}
