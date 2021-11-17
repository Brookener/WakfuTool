using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WakfuAudio.Scripts.Classes
{
    [Serializable]
    public class Animation
    {
        public string name;
        public Monster monster;
        public List<char> angles = new List<char>();
        public SortedDictionary<string, List<int>> sounds = new SortedDictionary<string, List<int>>();
        public SortedDictionary<string, List<int>> barks = new SortedDictionary<string, List<int>>();
        public SortedDictionary<string, List<int>> aps = new SortedDictionary<string, List<int>>();
        public bool splited = true;
        public AnimType type;

        public Animation(string newName)
        {
            name = newName;
            type = TypeFromAnimName(name);
        }
        public Animation(Animation anim)
        {
            name = anim.name;
            monster = anim.monster;
            angles = anim.angles.ToList();
            sounds = new SortedDictionary<string, List<int>>(anim.sounds);
            barks = new SortedDictionary<string, List<int>>(anim.barks);
            aps = new SortedDictionary<string, List<int>>(anim.aps);
            splited = anim.splited;
            type = anim.type;
        }

        public LuaScript AddScript(bool bark)
        {
            var script = monster.FirstScriptId(bark);
            (bark ? barks : sounds).Add(script, new List<int>() { 1 });
            if(!Database.GetOrCreate(bark ? ScriptType.mobBark : ScriptType.mobAnim, script, out LuaScript lua))
            {
                var inte = new Integration(lua, bark ? monster.FirstBarkAsset() : monster.FirstSoundAsset(type));
                lua.integrations.Add(inte);
                lua.SaveScript();
            }

            return lua;
        }
        public LuaScript AddApsScript(string apsId)
        {
            if(!Database.GetOrCreate(ScriptType.aps, apsId, out LuaScript lua))
            {
                var inte = new Integration(lua, lua.FirstApsAsset("410"));
                lua.integrations.Add(inte);
                lua.rolloff = 5;
                lua.SaveScript();
            }
            
            aps.Add(apsId, new List<int>() { 1 });
            return lua;
        }
        public LuaScript IncrementApsScript()
        {
            if (aps.Count == 0)
                return null;

            long value = Int64.Parse(aps.Last().Key);
            string model = value.ToString();
            while(aps.ContainsKey(value.ToString()))
                value += value >= 0 ? 1 : -1;
            var lua = AddApsScript(value.ToString());
            lua.integrations = Database.GetOrExtract(model).CopyIntegrations(lua);
            lua.SaveScript();
            return lua;
        }
        public void Remove(string script, ScriptType sType)
        {
            switch (sType)
            {
                case ScriptType.aps:
                    aps.Remove(script);
                    break;
                case ScriptType.mobAnim:
                    sounds.Remove(script);
                    break;
                case ScriptType.mobBark:
                    barks.Remove(script);
                    break;
            }
            Database.AskScriptDeletion(script);
        }
        public void Replace(string oldScript, string newScript, ScriptType sType)
        {
            var dic = DictionnaryFromType(sType);
            dic.Add(newScript, dic[oldScript]);
            dic.Remove(oldScript);
            Database.GetOrExtract(newScript).SaveScript();
        }

        #region Get Scripts

        public Dictionary<LuaScript, List<int>> GetScripts(bool bark)
        {
            var list = new Dictionary<LuaScript, List<int>>();
            foreach (KeyValuePair<string, List<int>> sound in bark ? barks : sounds)
                list.Add(Database.GetOrExtract(sound.Key), sound.Value);
            return list;
        }
        public Dictionary<LuaScript, List<int>> GetScriptsFromFiles(bool bark)
        {
            var list = new Dictionary<LuaScript, List<int>>();
            foreach (KeyValuePair<string, List<int>> sound in bark ? barks : sounds)
                list.Add(new LuaScript(Database.FullPathOfAnmScript(sound.Key)), sound.Value);
            return list;
        }
        public Dictionary<LuaScript, List<int>> GetApsScripts()
        {
            var dic = new Dictionary<LuaScript, List<int>>();
            aps.Keys.ToList().ForEach(x => dic.Add(Database.GetOrExtract(x), null));
            return dic;
        }
        public SortedDictionary<int, List<string>> GetSoundsAndBarksScriptByFrame()
        {
            return GetScriptByFrame(new SortedDictionary<string, List<int>>[] { sounds, barks });
        }
        public SortedDictionary<int, List<string>> GetScriptByFrame(SortedDictionary<string, List<int>>[] dics)
        {
            var list = new SortedDictionary<int, List<string>>();
            foreach(SortedDictionary<string, List<int>> dic in dics)
            foreach(KeyValuePair<string, List<int>> sound in dic)
            {
                foreach(int frame in sound.Value)
                {
                    if (!list.ContainsKey(frame))
                        list.Add(frame, new List<string>());
                    list[frame].Add(sound.Key);
                }
            }

            return list;
        }

        #endregion

        #region Utils
        
        public async Task PlayAudio()
        {
            var list = new Dictionary<int, List<LuaScript>>();

            foreach(KeyValuePair<int, List<string>> frame in GetScriptByFrame(new SortedDictionary<string, List<int>>[] {sounds, barks, aps }))
            {
                if (!list.ContainsKey(frame.Key))
                    list.Add(frame.Key, new List<LuaScript>());
                list[frame.Key].AddRange(frame.Value.Select(x => Database.GetOrExtract(x)));
            }

            foreach(KeyValuePair<int, List<LuaScript>> frame in list)
            {
                frame.Value.ForEach(x => x.PlayAudio());
                await Task.Delay(40);
            }

        }
        public string SplitedName()
        {
            return angles[0] + "_" + name;
        }
        
        public List<string> AllScriptIds()
        {
            var list = sounds.Keys.ToList();
            list.AddRange(barks.Keys);
            list.AddRange(aps.Keys);
            return list;
        }
        public List<string> AllAssets(bool bark)
        {
            var list = new List<string>();
            var scripts = GetScripts(bark).Keys.ToList();
            scripts.ForEach(x => list.AddRange(x.integrations.Select(i => i.asset)));
            return list;
        }
        public List<string> AllAssets()
        {
            var list = new List<string>();
            var scripts = GetScripts(false).Keys.ToList();
            scripts.ForEach(x => list.AddRange(x.integrations.Select(i => i.asset)));
            scripts = GetScripts(true).Keys.ToList();
            scripts.ForEach(x => list.AddRange(x.integrations.Select(i => i.asset)));
            return list;
        }
        public SortedDictionary<string, List<int>> DictionnaryFromType(ScriptType sType)
        {
            switch (sType)
            {
                default:
                    return sounds;
                case ScriptType.aps:
                    return aps;
                case ScriptType.mobAnim:
                    return sounds;
                case ScriptType.mobBark:
                    return barks;
            }
        }
        public bool UsesLuaScript(string script)
        {
            return AllScriptIds().Contains(script);
        }

        public static AnimType TypeFromAnimName(string name)
        {
            if (name.Length < 7)
                return AnimType.other;

            switch (name.Substring(4, 3))
            {
                default: return AnimType.other;
                case "Sor" : return AnimType.attack;
                case "Mor" : return AnimType.death;
                case "Hit" : return AnimType.hit;
                case "Cou" : return AnimType.move;
                case "Mar" : return AnimType.move;
                case "Com" : return AnimType.comp;
                case "Sau" : return AnimType.other;
                case "Sta" : return AnimType.fun;
            }
        }
        public static AnimType DetectAnimTypeFromAsset(string asset)
        {
            if (asset == null || asset.Length < 4)
                return AnimType.other;
            switch (asset.Substring(0, 3))
            {
                case "300": return AnimType.attack;
                case "310":
                    switch (asset[12])
                    {
                        case '0': return AnimType.move;
                        case '1': return AnimType.hit;
                        case '2': return AnimType.death;
                        case '3': return AnimType.other;
                    }
                    break;
                case "320": return AnimType.comp;
                case "330": return AnimType.fun;
            }
            return AnimType.other;
        }
        public static System.Windows.Media.Color MediaColorFromType(AnimType type, byte alpha)
        {
            var color = ColorFromType(type);
            return System.Windows.Media.Color.FromArgb(alpha, color.R, color.G, color.B);
        }
        public static Color ColorFromType(AnimType type)
        {
            switch (type)
            {
                default: return Color.White;
                case AnimType.attack: return Color.Red;
                case AnimType.comp: return Color.MistyRose;
                case AnimType.death: return Color.Orange;
                case AnimType.dialog: return Color.Beige;
                case AnimType.fun: return Color.Violet;
                case AnimType.hit: return Color.Yellow;
                case AnimType.move: return Color.Green;
            }
        }
        public static bool AreSame(Animation a, Animation b)
        {
            if (a.monster != b.monster) return false; 
            if (a.sounds.Count != b.sounds.Count) return false;
            if (a.barks.Count != b.barks.Count) return false;
            if (a.aps.Count != b.aps.Count) return false;
            if (!a.sounds.Keys.All(b.sounds.Keys.Contains)) return false;
            if (!a.barks.Keys.All(b.barks.Keys.Contains)) return false;
            if (!a.aps.Keys.All(b.aps.Keys.Contains)) return false;
            foreach(KeyValuePair<string, List<int>> sound in a.sounds)
                if (!b.sounds[sound.Key].All(sound.Value.Contains) || b.sounds[sound.Key].Count != sound.Value.Count) return false;
            foreach (KeyValuePair<string, List<int>> bark in a.barks)
                if (!b.barks[bark.Key].All(bark.Value.Contains) || b.barks[bark.Key].Count != bark.Value.Count) return false;
            foreach (KeyValuePair<string, List<int>> ap in a.aps)
                if (!b.aps[ap.Key].All(ap.Value.Contains) || b.aps[ap.Key].Count != ap.Value.Count) return false;
            return true;
        }

        #endregion
    }

    public enum AnimType { move, attack, hit, death, comp, dialog, fun, other, none }

}
