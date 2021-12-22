using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour AudioPlayerControl.xaml
    /// </summary>
    public partial class AudioPlayerControl : UserControl
    {
        public string file;
        public MediaPlayer currentAudio;
        private bool loop = true;
        private bool barDrag = false;
        private bool volumeDrag = false;

        public AudioPlayerControl(string newFile)
        {
            file = newFile;
            InitializeComponent();
            Load();
            UpdateLoopButton();
            Update();
        }

        private async void Update()
        {
            while (true)
            {
                if (!barDrag)
                    UpdateBarValue();
                UpdateTime();
                if (volumeDrag)
                    UpdateVolume();
                await Task.Delay(5);
            }
        }
        private void UpdateBarValue()
        {
            if (currentAudio != null && currentAudio.NaturalDuration.HasTimeSpan)
                Bar.Value = (double)currentAudio.Position.Ticks / currentAudio.NaturalDuration.TimeSpan.Ticks * 10;
            else
                Bar.Value = 0;
        }
        private void UpdateTime()
        {
            if (currentAudio != null && currentAudio.NaturalDuration.HasTimeSpan)
                Time.Text = new DateTime(currentAudio.Position.Ticks).ToString("mm:ss") + "|" + new DateTime(currentAudio.NaturalDuration.TimeSpan.Ticks).ToString("mm:ss");
            else
                Time.Text = "";
        }
        private void UpdateVolume()
        {
            currentAudio.Volume = VolumeSlider.Value * 0.1;
            VolumeText.Text = ((int)(currentAudio.Volume * 100)).ToString();
        }

        public void Load()
        {
            FileNameBox.Text = new FileInfo(file).Name;
            currentAudio = new MediaPlayer();
            currentAudio.Volume = 0;
            currentAudio.MediaEnded += new EventHandler(OnMediaEnded);
            currentAudio.Open(new Uri(file));
            currentAudio.Volume = 1;
        }

        private void PlayClick(object sender, RoutedEventArgs e)
        {
            currentAudio?.Play();
            PlayButton.Visibility = Visibility.Collapsed;
            PauseButton.Visibility = Visibility.Visible;
        }
        private void PauseClick(object sender, RoutedEventArgs e)
        {
            currentAudio?.Pause();
            PauseButton.Visibility = Visibility.Collapsed;
            PlayButton.Visibility = Visibility.Visible;

        }
        private void OnMediaEnded(object sendr, EventArgs e)
        {
            currentAudio.Stop();
            currentAudio.Position = new TimeSpan(0);
            if (loop)
                currentAudio.Play();
        }
        private void LoopClick(object sender, RoutedEventArgs e)
        {
            loop = !loop;
            UpdateLoopButton();
        }
        private void UpdateLoopButton()
        {
            LoopBorder.Visibility = loop ? Visibility.Visible : Visibility.Collapsed;
        }
        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            barDrag = true;
        }
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            currentAudio.Position = new TimeSpan((long)((double)currentAudio.NaturalDuration.TimeSpan.Ticks * (Bar.Value * 0.1)));
            barDrag = false;
        }
        private void OnVolumeChanged(object sender, TextChangedEventArgs e)
        {
            if (currentAudio == null)
                return;
            if (Int32.TryParse(VolumeText.Text, out int vol) && vol <= 100 && vol >= 0)
            {
                currentAudio.Volume = vol * 0.01;
                VolumeSlider.Value = vol * 0.1; 
            }
            else
                VolumeText.Text = (currentAudio.Volume * 100).ToString();
        }
        private void Volume_DragStarted(object sender, DragStartedEventArgs e)
        {
            volumeDrag = true;
        }
        private void Volume_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            volumeDrag = false;
        }
    }
}
