using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace WakfuAudio.Scripts.Classes
{
    [Serializable]
    public class Monster
    {
        public string Family { get; set;} = "";
        public string Id     { get; set;} = "";
        public string Name   { get; set;} = "";
        public DateTime lastSwfModification;
        public List<Datas> datas = new List<Datas>();
        public Type type = Type.npcs;

        public string interactiveDialog;
        public SortedDictionary<string, Animation> animations = new SortedDictionary<string, Animation>();

        public Monster(string newid, string newFamily, Type newType)
        {
            type = newType;
            Family = newFamily;
            Id = newid;
            Name = "";
            Database.datas.monsters.Remove(Id);
            Database.datas.monsters.Add(Id, this);
        }
        public void LoadFromSwf(SwfDecompiler decompiler)
        {
            SplitAllAnimations();
            decompiler.GetAnimations().Where(x => !animations.ContainsKey(x.Key)).ToList().ForEach(x => animations.Add(x.Key, x.Value));
            animations.Values.ToList().ForEach(x => x.monster = this);
            JoinAllAnimations();
            lastSwfModification = File.GetLastWriteTime(SwfPath());
            Database.SaveDatas();
        }
        public string FullName()
        {
            return Id + (Name == "" ? "" : ("_" + Name));
        }
        public async Task SaveSwf()
        {
            await SaveSwf(new SwfDecompiler(SwfPath()));
        }
        public async Task SaveSwf(SwfDecompiler decompiler)
        {
            MainWindow.SetInfos("Exporting Swf...");
            await Task.Delay(10);
            foreach (Animation anim in animations.Values)
                foreach (char angle in anim.angles)
                    decompiler.WriteAudioScriptsOfAnim(angle + "_" + anim.name, anim.GetSoundsAndBarksScriptByFrame());

            await Task.Run(() => decompiler.WriteSwf());
            MakeUpToDate();
            MainWindow.SetInfos("");
            await Task.Delay(10);
            await UpdateAnm();
            Database.SaveDatas();
        }
        public async Task UpdateAnm()
        {
            MainWindow.SetInfos("Exporting Anm...");
            string anmFile = Database.ToolsFolder() + @"\EXPORT\" + Id + ".anm";
            if (File.Exists(anmFile))
                File.Delete(anmFile);
            var bat = File.ReadAllLines(Database.SwfToAnmExporter());
            bat[5] = AnmZoomChain(type);
            bat[6] = AnmDepthChain(type);
            File.WriteAllLines(Database.SwfToAnmExporter(), bat);
            ProcessStartInfo info = new ProcessStartInfo(Database.SwfToAnmExporter(), SwfPath());
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            Process.Start(info);
            while (!File.Exists(anmFile))
                await Task.Delay(200);

            var archive = Database.AnimArchiveFromType(type);
            var entry = archive.CreateEntryFromFile(anmFile, Id + ".anm");
            archive.Dispose();
            Directory.Delete(Database.ToolsFolder() + @"\EXPORT", true);
            MainWindow.SetInfos("");
            await Task.Delay(10);
        }

        #region Edition

        public void SetId(string newId)
        {
            Database.datas.monsters.Remove(newId);
            Id = newId;
            Database.datas.monsters.Add(Id, this);
        }

        #endregion


        public void JoinAllAnimations()
        {
            foreach(Animation a in animations.Values.ToList())
                JoinAnimations(a.SplitedName(), false);
        }
        public void JoinAnimations(string splitedName, bool force)
        {
            if (!animations.ContainsKey(splitedName)) return;
            Animation a = new Animation(animations[splitedName]);
            var removed = new List<string>() { splitedName};

            foreach (Animation anim in animations.Values.ToList())
                if((anim.SplitedName() != a.SplitedName()) && (anim.name == a.name))
                {
                    if (!force && !Animation.AreSame(a, anim))
                        return;
                    if (!a.angles.Contains(anim.angles[0]))
                        a.angles.Add(anim.angles[0]);
                    removed.Add(anim.SplitedName());
                }
            //removed.ForEach(x => MessageBox.Show(animations.ContainsKey(x).ToString()));
            removed.ForEach(x => animations.Remove(x));
            animations.Add(a.name, a);
            a.splited = false;

        }
        public void SplitAllAnimations()
        {
            animations.Where(x => !x.Value.splited).ToList().ForEach(x => SplitAnimation(x.Key));
        }
        public void SplitAnimation(string anim)
        {
            if (!animations.ContainsKey(anim)) return;
            foreach(char angle in animations[anim].angles)
            {
                var a = new Animation(animations[anim]);
                a.angles = new List<char>() { angle };
                a.splited = true;
                animations.Add(a.SplitedName(), a);
            }
            animations.Remove(anim);
        }
        public List<Animation> SortedAnimations()
        {
            return animations.Values.OrderBy(x => (int)x.type).ToList();
        }
        public string FirstScriptId(bool bark)
        {
            if (bark)
                return Database.FirstBarkScriptIdAvailable().ToString();

            var value = "399" + Id + "001";

            var first = Int64.Parse(value);
            while (UsesLuaScript(first.ToString()))
                first++;
            return first.ToString();
        }
        public string DefaultSoundAsset(AnimType type)
        {
            switch (type)
            {
                case AnimType.attack: return "300" + Id + "001";
                case AnimType.move: return "310" + Id + "001";
                case AnimType.hit: return "310" + Id + "101";
                case AnimType.death: return "310" + Id + "201";
                case AnimType.other: return "310" + Id + "301";
                case AnimType.comp: return "320" + Id + "001";
                case AnimType.dialog: return "320" + Id + "001";
                case AnimType.fun: return "330" + Id + "001";
                default: return "300" + Id + "901";
            }
        }
        public string FirstSoundAsset(AnimType type)
        {
            var value = Int64.Parse(DefaultSoundAsset(type));
            var assets = AllAssets();
            while (assets.Contains(value.ToString()))
                value++;
            return value.ToString();
        }
        public string FirstBarkAsset()
        {
            return Database.FirstBarkAssetAvailable().ToString();
        }
        

        #region utils

        public List<string> AllScriptIds()
        {
            var list = new List<string>();
            animations.Values.ToList().ForEach(s => list.AddRange(s.AllScriptIds()));
            return list;
        }
        public List<string> AllAssets()
        {
            var list = new List<string>();
            AllScriptIds().Select(x => Database.GetOrExtract(x)).ToList().ForEach(x => list.AddRange(x.AllAssets()));
            return list;
        }
        public bool IsScriptFileForThisMonster(string script)
        {
            return IsScriptIdForThisMonster(Database.FileNameFromPath(script));
        }
        public bool IsScriptIdForThisMonster(string script)
        {
            if (script.Length != 15) return false;

            var monsterId = script.Substring(3, script.Length - 6);
            if(Int64.TryParse(monsterId, out long result))
            {
                if (result.ToString() == Id)
                    return true;
            }
            return false;
        }
        public bool UsesLuaScript(string scriptId)
        {
            foreach (Animation anim in animations.Values)
                if (anim.UsesLuaScript(scriptId))
                    return true;
            return false;
        }
        public string SwfPath()
        {
            return Database.AnimExportFolder() + "\\" + type + "\\" + Id + ".swf";
        }
        public string ImagePath()
        {
            return Database.AnimSourcesFolder() + "\\" + type + "\\" + Family + "\\" + Id + ".png";
        }
        public string AnimatePath()
        {
            return Database.AnimSourcesFolder() + "\\" + type + "\\" + Family.Replace("_","\\") + "\\" + Id + ".fla";
        }
        public void OpenAnimateFile()
        {
            if(File.Exists(AnimatePath()))
                Process.Start(AnimatePath());
            else
            {
                MessageBox.Show("Can't find .fla projet :\n" + AnimatePath());
            }
        }
        public void ShowAnimateFileInExplorer()
        {
            MainWindow.ShowFileInExplorer(AnimatePath());
        }
        public void ShowSwfFileInExplorer()
        {
            MainWindow.ShowFileInExplorer(SwfPath());
        }
        public System.Windows.Media.ImageSource GetImage()
        {
            if(File.Exists(ImagePath()))
                return Utils.GetSourceImage(ImagePath());
            return null;
        }
        public bool IsUpToDate()
        {
            return lastSwfModification != null && lastSwfModification.CompareTo(File.GetLastWriteTime(SwfPath())) <= 0;
        }
        public void MakeUpToDate()
        {
            lastSwfModification = File.GetLastWriteTime(SwfPath());
        }
        public bool Filter(string filter)
        {
            return Utils.StringContains(Name == null ? "" : Name, filter) || Utils.StringContains(Id, filter) || Utils.StringContains(Family, filter);
        }

        public static string AnmZoomChain(Type type)
        {
            switch (type)
            {
                default: return "set zoom=1.5";
                case Type.players: return "set zoom=0.3";
                case Type.pets:return "set zoom=1.5";
            }
        }
        public static string AnmDepthChain(Type type)
        {
            switch (type)
            {
                default: return "set depth=-1";
                case Type.players: return "set depth=3";
                case Type.pets: return "set depth=-1";

            }
        }

        #endregion

        public void LoadInteractiveDialog()
        {
            interactiveDialog = Database.DialogOf(Id);
        }

        [Serializable]
        public struct Datas
        {
            public int id;
            public string name;
            public string dialog;
        }

        public enum Type { npcs, players, pets}
    }
}
