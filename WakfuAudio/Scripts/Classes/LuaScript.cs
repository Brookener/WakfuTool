using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Drawing;

namespace WakfuAudio.Scripts.Classes
{
    [Serializable]
    public class LuaScript
    {
        public string id;
        public ScriptType type;
        public int rolloff;
        public bool stop;
        public List<Integration> integrations = new List<Integration>();
        public string description = "";

        public LuaScript(string scriptFile)
        {
            id = Database.FileNameFromPath(scriptFile);
            Database.datas.scripts.Remove(id);
            Database.datas.scripts.Add(id, this);

            if (Utils.StringContains(scriptFile, "particles"))
                type = ScriptType.aps;
            else
                type = DetectScriptTypeFromId(id);


            if (!ScriptFileExists())
                return;

            var fileContent = File.ReadAllText(FilePath());
            string[] intes;
            if (SpecialCase(id))
            {
                type = ScriptType.special;
            }
            else
            {
                switch (type)
                {
                    default:
                        rolloff = ExtractAnimRollOff(fileContent);
                        intes = ExtractAnimIntegrations(fileContent);
                        break;
                    case ScriptType.aps:
                        rolloff = ExtractApsRollOff(fileContent);
                        intes = ExtractApsIntegrations(fileContent);
                        break;
                }

                if (intes.Length % 2 == 0)
                    for (int i = 0; i < intes.Length; i += 2)
                    {
                        var inte = new Integration(this, intes[i]);
                        inte.SetVolume(intes[i + 1]);
                        integrations.Add(inte);
                    }
            }
                
        }
        public LuaScript(ScriptType newType, string newId)
        {
            id = newId;
            type = newType;
            Database.datas.scripts.Remove(id);
            Database.datas.scripts.Add(id, this);
        }
        public void RemoveAsset(string asset)
        {
            foreach(Integration inte in integrations.ToList())
                if(inte.asset == asset)
                {
                    integrations.Remove(inte);
                    break;
                }
        }
        public bool SetRolloff(int newRolloff)
        {
            rolloff = newRolloff;
            return true;
        }
        public bool SetRolloff(string newRolloff)
        {
            if (Int32.TryParse(newRolloff, out int result))
            {
                return SetRolloff(result);
            }
            else
                return false;
        }

        
        #region Script File

        public void ShowFileInExplorer()
        {
            if (ScriptFileExists())
            {
                var info = new ProcessStartInfo("explorer.exe", "/select,\"" + FilePath() + "\"");
                Process.Start(info);
            }
        }
        public void OpenFile()
        {
            if (ScriptFileExists())
                Process.Start(FilePath());
            else
                MessageBox.Show(id + " script does not exist");
        }
        public void SaveScript()
        {
            File.WriteAllText(FilePath(), GetAnimScript());
        }
        public void DeleteScript()
        {
            File.Delete(FilePath());
            Database.datas.scripts.Remove(id);
        }
        public bool SetId(string newid)
        {
            if(Int64.TryParse(newid, out long result))
            {
                id = newid;
                return true;
            }
            else
                return false; 
        }

        public string FilePath()
        {
            if (type == ScriptType.aps)
                return Database.FullPathOfApScript(id);
            else
                return Database.FullPathOfAnmScript(id);
        }
        public string AssetVolumeChain()
        {
            var values = new List<string>();
            foreach(Integration inte in integrations)
            {
                values.Add(inte.asset);
                values.Add(inte.volume.ToString());
            }
            return String.Join(",", values);
        }
        public string AssetChain()
        {
            return String.Join(",", integrations.Select(x => x.asset));
        }
        public string VolumeChain()
        {
            return String.Join(",", integrations.Select(x => x.volume.ToString()));
        }
        public string GetAnimScript()
        {
            if (type == ScriptType.aps)
                return apsScript.Replace("ASSETS", AssetChain()).Replace("GAIN", VolumeChain()).Replace("ROLLOFF", rolloff.ToString()).Replace("STOP", stop.ToString());
            else
                return animScript.Replace("ASSETS", AssetVolumeChain()).Replace("ROLLOFF", rolloff.ToString()).Replace("STOP", stop.ToString());
        }



        #endregion

        #region Constantes

        public const string animScript = "rollOffPreset=ROLLOFF\nstopOnAnimationChange=STOP\n\nSound.playLocalRandomSound(rollOffPreset, stopOnAnimationChange,ASSETS)";
        public const string apsScript =
            "-- Script de son d'APS"
          + "\nsoundId={ASSETS}"
          + "\nrollOffId=ROLLOFF"
          + "\nfadeOutTime=0"
          + "\ngain={GAIN}"
          + "\nstopOnRemoveAps=STOP"
          + "\nloop=false"
          + "\ndelay=0"
          + "\n"
          + "\nfunction playAps(apsId, fightId, duration)"
          + "\n    local i = table.getn(soundId)"
          + "\n	if (i > 1) then"
          + "\n        local sounds = {}"
          + "\n		for j = 1,i do"
          + "\n			sounds[2 * j - 1] = soundId[j]"
          + "\n                sounds[2 * j] = gain[j]"
          + "\n"
          + "\n        end"
          + "\n		if (delay == 0) then"
          + "\n            Sound.playRandomApsSound(fightId, apsId, duration, fadeOutTime, rollOffId, loop, unpack(sounds))"
          + "\n		else"
          + "\n			invoke(delay, 1, \"Sound.playRandomApsSound\", fightId, apsId, duration, fadeOutTime, rollOffId, loop, unpack(sounds))"
          + "\n"
          + "\n        end"
          + "\n	else"
          + "\n		if (delay == 0) then"
          + "\n            Sound.playApsSound(soundId[1], fightId, gain[1], apsId, duration, fadeOutTime, rollOffId, loop)"
          + "\n		else"
          + "\n			invoke(delay, 1, \"Sound.playApsSound\", soundId[1], fightId, gain[1], apsId, duration, fadeOutTime, rollOffId, loop)"
          + "\n"
          + "\n        end"
          + "\n    end"
          + "\n	return loop or stopOnRemoveAps"
          + "\nend";

        #endregion

        #region Utils

        public bool IsUsed()
        {
            foreach (Monster m in Database.datas.monsters.Values)
                foreach (Animation anim in m.animations.Values)
                    if (anim.AllScriptIds().Contains(id))
                        return true;
            return false;
        }
        public List<Animation> AnimsUsing()
        {
            var list = new List<Animation>();
            foreach (Monster m in Database.datas.monsters.Values)
                foreach (Animation anim in m.animations.Values)
                    if (anim.AllScriptIds().Contains(id))
                        list.Add(anim);
            return list;
        }
        public List<Monster> MonstersUsing()
        {
            return AnimsUsing().Select(x => x.monster).ToList();
        }
        public bool SearchFilter(string patern)
        {
            return Utils.StringContains(id, patern) || Utils.StringContains(description, patern);
        }
        public bool ContainsAsset(string asset)
        {
            return AllAssets().Contains(asset);
        }
        public List<string> AllAssets()
        {
            return integrations.Select(x => x.asset).ToList();
        }
        public bool ScriptFileExists()
        {
            return File.Exists(FilePath());
        }
        public string FirstApsAsset(string prefix)
        {
            var first = Int64.Parse(prefix + id.Replace("-", "") + "001");
            while (ContainsAsset(first.ToString()))
                first++;
            return first.ToString();
        }
        public List<Integration> SortedIntegrations()
        {
            return integrations.OrderBy(x => x.asset).ToList();
        }
        public List<Integration> CopyIntegrations(LuaScript newScript)
        {
            return integrations.Select(x => new Integration(newScript, x)).ToList();
        }
        public List<Integration> MissingAssets()
        {
            return integrations.Where(x => !x.AssetExists()).ToList();
        }
        public bool HasMissingAsset()
        {
            return MissingAssets().Count > 0;
        }

        
        public static ScriptType DetectScriptTypeFromId(string id)
        {
            switch (id.Substring(0, 3))
            {
                default: return ScriptType.none;
                case "399" : return ScriptType.mobAnim;
                case "199": return ScriptType.playerAnim;
                case "499": return ScriptType.mobBark;
                case "299": return ScriptType.playerBark;
                case "599": return ScriptType.sfx;

            }
        }

        

        #endregion

        #region Decompile

        public static string[] ExtractAnimIntegrations(string script)
        {
            var chain = Utils.GetStringFromPaterns(script, "soundFileId=", "\n");
            if(Int64.TryParse(chain, out long asset))
            {
                var gain = Utils.GetStringFromPaterns(script, "gain=", "\n");
                if(Int32.TryParse(gain, out int volume))
                {
                    return new string[2] {chain, gain};
                }
            }
            chain = Utils.GetStringFromPaterns(script, "Change,", ")");
            return chain.Split(',');
        }
        public static string[] ExtractApsIntegrations(string script)
        {
            var assets = Utils.GetStringFromPaterns(script, "soundId={", "}").Split(',');
            var volumes = Utils.GetStringFromPaterns(script, "gain={", "}").Split(',');
            var values = new string[assets.Length + volumes.Length];
            int index = 0;
            for(int i = 0; i < assets.Length; i+=2)
            {
                values[i] = assets[index];
                values[i + 1] = volumes[index];
                index++;
            }
            return values;
        }
        public static int ExtractAnimRollOff(string script)
        {
            var r = Utils.GetStringFromPaterns(script, "rollOffPreset=", "\n");
            if (Int32.TryParse(r, out int result))
                return result;
            else
                return -1;
        }
        public static int ExtractApsRollOff(string script)
        {
            var r = Utils.GetStringFromPaterns(script, "rollOfId=", "\n");
            if (Int32.TryParse(r, out int result))
                return result;
            else
                return -1;
        }
        public static bool SpecialCase(string script)
        {
            switch (script)
            {
                default:
                    return false;
                case "1990000001":
                    return true;
                case "1990000002":
                    return true;
            }
        }

        #endregion

    }

    public enum ScriptType { mobAnim, playerAnim, mobBark, playerBark, sfx, aps, special, none}
}
