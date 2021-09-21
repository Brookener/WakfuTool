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
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string anmFolder = @"C:\Users\lclavel\Documents\LuaManager\scripts\anm";

        public MainWindow()
        {
            Database.Setup();
            InitializeComponent();
            Setup();

            try { Editor.LoadMonster(Database.datas.monsters[Database.parameters.lastMonsterOpened]); }
            catch { Editor.LoadMonster(null); }
        }
        private void Setup()
        {
            MonsterPanel.type = Monster.Type.npcs;
            PlayerPanel.type = Monster.Type.players;
            PetPanel.type = Monster.Type.pets;
            MonsterPanel.LoadList();
            PlayerPanel.LoadList();
            PetPanel.LoadList();

            MonsterPanel.MonsterSelection += new RoutedEventHandler(MonsterItemSelected);
            PlayerPanel.MonsterSelection += new RoutedEventHandler(MonsterItemSelected);
            PetPanel.MonsterSelection += new RoutedEventHandler(MonsterItemSelected);
            Editor.MonsterLoaded += new RoutedEventHandler(MonsterLoaded);
            Editor.MonsterRenamed += new RoutedEventHandler(MonsterRenamed);
            Database.CheckIfMonstersAreUpToDate();
        }

        
        #region Window Events

        private void OnClosed(object sender, EventArgs e)
        {
            Database.SaveDatas();
            Application.Current.Shutdown();
        }
        private void MonsterItemSelected(object monster, RoutedEventArgs e)
        {
            Editor.LoadMonster(monster as Monster);
        }
        private void MonsterLoaded(object sender, RoutedEventArgs e)
        {
            //MonsterPanel.SelectedItem((sender as MonsterEdition).monster.Id);
        }
        private void MonsterRenamed(object sender, RoutedEventArgs e)
        {
            //MonsterPanel.SetMonsterItemHeader(sender as Monster);
        }

        #endregion

        public static void SetInfos(string infos)
        {
            try
            {
                if (Application.Current.MainWindow as MainWindow != null)
                    (Application.Current.MainWindow as MainWindow).Infos.Text = infos;
            }
            catch { }
            
        }
    }
}
