using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WakfuAudio.Scripts.Classes
{
    [Serializable]
    public class Integration
    {
        public LuaScript script;
        public string asset;
        public bool bark;
        public int volume = 50;

        public Integration() { asset = "3000000"; }
        public Integration(LuaScript newScript, string newAsset)
        {
            script = newScript;
            asset = newAsset;
        }
        public Integration(LuaScript newScript, Integration i)
        {
            script = newScript;
            asset = i.asset;
            bark = i.bark;
            volume = i.volume;
        }

        public bool SetAsset(string a)
        {
            if(Int64.TryParse(a, out long result))
            {
                SetAsset(result);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetAsset(long a)
        {
            asset = a.ToString();
        }
        public bool SetVolume(string vol)
        {
            if(Int32.TryParse(vol, out int result))
            {
                SetVolume(result);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetVolume(int newVolume)
        {
            volume = newVolume;
        }

        public long Asset()
        {
            if (Int64.TryParse(asset, out long result))
                return result;
            else
                return - 1;
        }


        #region Utils

        public string AssetFile()
        {
            return Database.AudioFolder(script.type, Animation.DetectAnimTypeFromAsset(asset), true) + @"\" + asset + ".ogg";
        }
        public bool AssetExists()
        {
            return File.Exists(AssetFile());
        }
        public void PlayAsset()
        {
            if (AssetExists())
                AudioPlayer.PlayAudio(AssetFile(), 1);
        }

        #endregion

    }
}
