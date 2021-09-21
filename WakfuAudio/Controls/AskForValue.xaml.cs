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
using System.Windows.Shapes;
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio.Controls
{
    /// <summary>
    /// Logique d'interaction pour AskForValue.xaml
    /// </summary>
    public partial class AskForValue : Window
    {
        public bool ok = false;
        public string value;
        private ValueType type;
        public AskForValue(ValueType newType)
        {
            type = newType;
            Left = System.Windows.Forms.Cursor.Position.X;
            Top = System.Windows.Forms.Cursor.Position.Y;
            InitializeComponent();
            ValueBox.Focus();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            switch (type)
            {
                case ValueType.Long:
                    if(!Int64.TryParse(ValueBox.Text, out long result))
                    {
                        Error();
                        return;
                    }
                    break;
                case ValueType.Int:
                    if (!Int32.TryParse(ValueBox.Text, out int res))
                    {
                        Error();
                        return;
                    }
                    break;
                case ValueType.String:
                    break;

            }
            value = ValueBox.Text;
            ok = true;
            Close();
        }
        private void Error()
        {
            ValueBox.Background = Constantes.Red;
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void BoxKeyUp(object sender, KeyEventArgs e)
        {
            ValueBox.Background = Constantes.White;
            if(e.Key == Key.Return)
            {
                OkClick(sender, e);
            }
        }
        public enum ValueType { Int, String, Long}

        
    }


}
