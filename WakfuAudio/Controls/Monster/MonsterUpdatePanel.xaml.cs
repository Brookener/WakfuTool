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

namespace WakfuAudio.Controls
{
    /// <summary>
    /// Logique d'interaction pour MonsterUpdatePanel.xaml
    /// </summary>
    public partial class MonsterUpdatePanel : Window
    {
        private const int lineHeight = 25;
        public List<Monster> monsters;
        public MonsterUpdatePanel(List<Monster> newList)
        {
            InitializeComponent();
            monsters = newList;
            UpdateList();
        }
        public async Task UpdateList()
        {
            Info.Text = "Loading Monsters";
            InfoPanel.Visibility = Visibility.Visible;

            Ids.Children.Clear();
            Names.Children.Clear();
            Families.Children.Clear();
            Updates.Children.Clear();
            Selection.Items.Clear();

            int count = 1;
            foreach (Monster m in monsters)
            {
                Ids.Children.Add(new TextBlock()
                {
                    Text = m.Id,
                    Tag = m,
                    Foreground = Constantes.White,
                    Height = lineHeight,

                });
                Names.Children.Add(new TextBlock()
                {
                    Text = m.Name,
                    Tag = m,
                    Foreground = Constantes.White,
                    Height = lineHeight,

                });
                Families.Children.Add(new TextBlock
                {
                    Text = m.Family,
                    Tag = m,
                    Foreground = Constantes.White,
                    Height = lineHeight,

                });
                var item = new ComboBox()
                {
                    ItemsSource = Enum.GetValues(typeof(MonsterUpdateAction)),
                    Tag = m,
                    SelectedIndex = 0,
                    Height = lineHeight,
                };
                item.DropDownClosed += new EventHandler(ActionChange);
                Updates.Children.Add(item);
                Selection.Items.Add(new ListBoxItem()
                {
                    Tag = item,
                    Height = lineHeight,
                    IsSelected = true,
                    BorderThickness = new Thickness(2),
                    BorderBrush = Constantes.White,
                });

                Info.Text = "Loading Monsters : " + count + "/" + monsters.Count;
                await Task.Delay(1);
                count++;
            }
            InfoPanel.Visibility = Visibility.Collapsed;
        }
        private void UpdateSelectionAction(MonsterUpdateAction action)
        {
            foreach(ListBoxItem i in Selection.Items)
                if (i.IsSelected)
                    (i.Tag as ComboBox).SelectedValue = action;

        }
        public async Task StartUpdate()
        {
            Info.Text = "Updating Monsters";
            InfoPanel.Visibility = Visibility.Visible;
            Monster monster;
            ComboBox box;
            int count = 1;
            foreach (ListBoxItem i in Selection.Items)
            {
                box = i.Tag as ComboBox;
                monster = box.Tag as Monster;
                switch((MonsterUpdateAction)box.SelectedValue)
                {
                    case MonsterUpdateAction.IgnoreAlways:
                        monster.MakeUpToDate();
                        break;
                    case MonsterUpdateAction.IgnoreOnce:

                        break;
                    case MonsterUpdateAction.Update:
                        await monster.SaveSwf();
                        break;
                }

                Info.Text = "Updating Monsters : " + count + "/" + monsters.Count;
                count++;
                await Task.Delay(1);
            }
            InfoPanel.Visibility = Visibility.Collapsed;
            Database.SaveDatas();
            Close();
        }

        public enum MonsterUpdateAction { Update, IgnoreAlways, IgnoreOnce }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }
        private void StartClick(object sender, RoutedEventArgs e)
        {
            StartUpdate();
        }private void ActionChange(object sender, EventArgs e)
        {
            UpdateSelectionAction((MonsterUpdateAction)(sender as ComboBox).SelectedValue);
        }
    }
}
