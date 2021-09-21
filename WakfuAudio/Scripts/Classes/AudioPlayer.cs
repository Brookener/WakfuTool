using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WakfuAudio.Scripts.Classes
{

    public class AudioPlayer
    {


        public static MediaPlayer PlayAudio(string file, double volume)
        {
            if (!File.Exists(file)) return null;
            var fxSound = new MediaPlayer();
            volume = volume > 1 ? 1 : volume;
            volume = volume < 0 ? 0 : volume;

            fxSound.Open(new Uri(file));
            fxSound.Volume = volume * (Database.parameters.volume * 0.01);
            fxSound.Play();

            return fxSound;
        }
    }
}
