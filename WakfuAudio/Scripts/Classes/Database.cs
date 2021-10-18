using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.IO.Compression;


namespace WakfuAudio.Scripts.Classes
{
    public class Database
    {
        public static Parameters parameters;
        public static DatabaseSave datas;

        public static List<string> allApsIds;

        public static void Setup()
        {
            LoadParameters();
            LoadDatas();
            LoadNames();
            //allApsIds = AllApsScriptId();
        }

        private const string dataSaveFileName = "Datas.dat";
        public static string DataFilePath()
        {
            return parameters.svnFolder + @"\trunk\audio\" + dataSaveFileName;
        }
        public static void LoadDatas()
        {

            if (File.Exists(DataFilePath()))
            {
                Stream file = File.Open(DataFilePath(), FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();

                try
                {
                    datas = (DatabaseSave)bf.Deserialize(file);
                }
                catch (Exception e)
                {
                    datas = new DatabaseSave();
                    MessageBox.Show(e.Message);

                }
                file.Close();
            }
            else
                datas = new DatabaseSave();
        }
        public static bool saving = false;
        public static async void SaveDatas()
        {
            if (!File.Exists(DataFilePath()))
                return;

            Stream file = null;
            MainWindow.SetInfos("Saving Datas");
            file = File.Open(DataFilePath(), FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, datas);
            file.Close();
            MainWindow.SetInfos("");
        }
        public static void SetAllLuaScriptsFromFiles()
        {
            datas.scripts.Clear();
            foreach(string file in AllScriptFiles())
            {
                var lua = new LuaScript(file);
                datas.scripts.Add(lua.id, lua);
            }
        }

        #region Parameters

        private const string parametersFileName = "Parameters.dat";
        public static void SaveParameters()
        {
            Stream file = File.Open(parametersFileName, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, parameters);
            file.Close();
        }
        public static void LoadParameters()
        {
            if (File.Exists(parametersFileName))
            {
                Stream file = File.Open(parametersFileName, FileMode.Open);

                BinaryFormatter bf = new BinaryFormatter();

                try
                {
                    parameters = (Parameters)bf.Deserialize(file);
                }
                catch (Exception)
                {
                    parameters = new Parameters();
                }
                file.Close();
            }
            else
            {
                parameters = new Parameters();
            }
        }

        #endregion

        #region Global

        public static FileInfo[] GetFilesAtPath(string path, string searchPattern, SearchOption option)
        {
            return Directory.GetFiles(path, searchPattern, option).Select(x => new FileInfo(x)).ToArray();
        }
        public static DirectoryInfo[] GetFoldersAtPath(string path)
        {
            return Directory.GetDirectories(path).Select(x => new DirectoryInfo(x)).ToArray();
        }

        #endregion


        #region Audio Folders

        public static string MainAudioFolder()
        {
            return parameters.svnFolder + @"\trunk\audio";
        }
        public static string AudioFolder(bool export)
        {
            return export ? ExportsFolder() : SourcesFolder();
        }
        public static string AudioFolder(ScriptType script, AnimType anim, bool export)
        {
            switch (script)
            {
                case ScriptType.aps:
                    return ApsExportsFolder(export);
                case ScriptType.mobAnim:
                    goto Anim;
                case ScriptType.mobBark:
                    return VoiceExportsFolder(export);
                case ScriptType.playerAnim:
                    goto Anim;
                case ScriptType.playerBark:
                    return VoiceExportsFolder(export);
                case ScriptType.sfx:
                    return SfxExportsFolder(export);
            }

            Anim:
            switch (anim)
            {
                default:
                    return AudioFolder(export);
                case AnimType.attack:
                    return FightExportsFolder(export);
                case AnimType.comp:
                    return FoleysExportsFolder(export);
                case AnimType.death:
                    return FoleysExportsFolder(export);
                case AnimType.dialog:
                    return FoleysExportsFolder(export);
                case AnimType.fun:
                    return FoleysExportsFolder(export);
                case AnimType.hit:
                    return FoleysExportsFolder(export);
                case AnimType.move:
                    return FoleysExportsFolder(export);
            }
        }
        public static string SourcesFolder()
        {
            return MainAudioFolder() + @"\sources";
        }
        public static string ExportsFolder()
        {
            return MainAudioFolder() + @"\export";
        }

        public static string AmbExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\Amb2D";
        }
        public static string ApsExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\APS";
        }
        public static string FightExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\fights";
        }
        public static string FoleysExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\foleys";
        }
        public static string GuiExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\gui";
        }
        public static string MusicExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\music";
        }
        public static string SfxExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\SFX";
        }
        public static string VoiceExportsFolder(bool export)
        {
            return AudioFolder(export) + @"\voices";
        }

        

        #endregion

        #region Script Folders

        public static string ScriptFolder()
        {
            return parameters.svnFolder + @"\Scripts";
        }
        public static string AnmFolder()
        {
            return ScriptFolder() + @"\anm";
        }
        public static string ApsFolder()
        {
            return ScriptFolder() + @"\particles";
        }
        public static string FullPathOfAnmScript(string script)
        {
            return AnmFolder() + @"\" + script + ".lua";
        }
        public static string FullPathOfApScript(string script)
        {
            return ApsFolder() + @"\" + script + ".lua";
        } 
        public static bool AnmScriptExists(string script)
        {
            return File.Exists(FullPathOfAnmScript(script));
        }
        public static bool ApsScriptExists(string script)
        {
            return File.Exists(FullPathOfApScript(script));
        }
        public static bool ScriptExists(string script)
        {
            return AnmScriptExists(script) || ApsScriptExists(script);
        }

        #endregion

        #region Other Folders

        public static string ToolsFolder()
        {
            return parameters.svnFolder + @"\Tools";
        }
        public static string SwfToAnmExporter()
        {
            return ToolsFolder() + @"\ConvertSwfToAnm.bat";
        }
        public static ZipArchive AnimNpcArchive()
        {
            return ZipFile.Open(parameters.trunkFolder + @"\contents\animations\npcs\npcs.jar", ZipArchiveMode.Update);
        }
        public static ZipArchive AnimPlayerArchive()
        {
            return ZipFile.Open(parameters.trunkFolder + @"\contents\animations\players\players.jar", ZipArchiveMode.Update);
        }
        public static ZipArchive AnimArchiveFromType(Monster.Type type)
        {
            switch (type)
            {
                default: return AnimNpcArchive();
                case Monster.Type.players: return AnimPlayerArchive();
            }
        }

        #endregion

        #region Visu Aps Folders

        public static string ApsArchive()
        {
            return parameters.trunkFolder + @"\contents\particles\particles.jar";
        }
        public static List<string> AllApsAnimIds()
        {
            return ZipFile.OpenRead(ApsArchive()).Entries.Select(e => e.Name).Where(x => x.Substring(x.Length - 4) == ".xps").ToList();
        }

        #endregion

        #region Animations Paths

        public static string AnimSourcesFolder()
        {
            return parameters.svnFolder + @"\trunk\animations\sources";
        }
        public static string AnimExportFolder()
        {
            return parameters.svnFolder + @"\trunk\animations\export";
        }
        public static List<string> AllMonstersAnimFile()
        {
            return Directory.GetFiles(AnimExportFolder(), "*.swf", SearchOption.AllDirectories).ToList();
        }
        public static string[] AllFamiliesPath()
        {
            if (Directory.Exists(AnimSourcesFolder()))
            {
                var list = Directory.GetDirectories(AnimSourcesFolder(),"*", SearchOption.AllDirectories);
                return list.Where(x => Directory.GetFiles(x).Length > 0).ToArray();
            }
            else
                return new string[] { };
        }
        public static Dictionary<string, string> AllFamilies()
        {
            var dic = new Dictionary<string, string>();
            string family;
            foreach (string path in AllFamiliesPath())
            {
                family = FolderNameFromPath(path);
                if (Directory.GetParent(path).FullName != AnimSourcesFolder())
                {
                    family = Directory.GetParent(path).Name + '\\' + family;
                }
                dic.Add(family, path);
            }
            return dic;
        }
        public static string[] GetMonstersFileOfFamily(string family)
        {
            var path = AnimSourcesFolder() + @"\" + family;
            if (Directory.Exists(path))
                return Directory.GetFiles(path, "*.fla");
            else
                return new string[] {};
        }
        public static Dictionary<string, string> GetMonstersOfFamily(string family)
        {
            var dic = new Dictionary<string, string>();
            foreach (string monster in GetMonstersFileOfFamily(family))
                dic.Add(FileNameFromPath(monster), monster);
            return dic;
        }

        public static string PlayerAnimSourcesFolder()
        {
            return parameters.svnFolder + @"\trunk\animations\sources\players";
        }
        public static string PlayerAnimExportFolder()
        {
            return parameters.svnFolder + @"\trunk\animations\export\players";
        }
        public static FileInfo[] AllPlayerAnimFile()
        {
            return Directory.GetFiles(PlayerAnimExportFolder(), "*.swf", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToArray();
        }
        public static DirectoryInfo[] AllPlayerGroupAnimFolders()
        {
            return GetFoldersAtPath(PlayerAnimSourcesFolder());
        }

        #endregion

        #region Monster Names

        public static Dictionary<int, string> monsterNames = new Dictionary<int, string>();
        public static void LoadNames()
        {
            monsterNames = Psql.GetMonsterNames();
        }
        public static string NameOf(string id)
        {
            if (id.Length > 4 && Int32.TryParse(id.Substring(id.Length - 4), out int monsterId) && monsterNames.ContainsKey(monsterId))
                return monsterNames[monsterId];
            else
                return "";

        }


        #endregion

        #region Values 

        public static string[] AllAudioExportFiles()
        {
            return Directory.GetFiles(ExportsFolder(), "*.ogg", SearchOption.AllDirectories);
        }
        public static string[] AllAudioSourcesFiles()
        {
            return Directory.GetFiles(SourcesFolder(), "*.wav", SearchOption.AllDirectories);
        }
        public static List<string> AllAssetFiles(ScriptType script, AnimType anim)
        {
            return Directory.GetFiles(AudioFolder(script, anim, true), ".ogg").ToList();
        }
        public static List<string> AllAssetId(ScriptType script, AnimType anim)
        {
            return AllAssetFiles(script, anim).Select(x => FileNameFromPath(x)).ToList();
        }
        public static List<string> AllAssetsReference()
        {
            var list = new List<string>();
            datas.scripts.Values.ToList().ForEach(x => list.AddRange(x.AllAssets()));
            return list;
        }
        public static bool DatasContainsAsset(string asset)
        {
            return AllAssetsReference().Contains(asset);
        }
        public static string[] AllAnimScriptFiles()
        {
            return Directory.GetFiles(AnmFolder());
        }
        public static string[] AllApsScriptFiles()
        {
            return Directory.GetFiles(ApsFolder());
        }
        public static List<string> AllScriptFiles()
        {
            var list = AllAnimScriptFiles().ToList();
            list.AddRange(AllApsScriptFiles());
            return list;
        }
        public static List<string> AllAnimScriptId()
        {
            return AllAnimScriptFiles().Select(x => FileNameFromPath(x)).ToList();
        }
        public static List<string> AllApsScriptId()
        {
            return AllApsScriptFiles().Select(x => FileNameFromPath(x)).ToList();
        }
        public static List<string> AllScriptId()
        {
            return AllScriptFiles().Select(x => FileNameFromPath(x)).ToList();
        }
        public static List<string> AllBarksScriptsId()
        {
            return AllAnimScriptId().Where(x => x.Substring(0, 3) == "499").ToList();
        }
        public static long FirstBarkScriptIdAvailable()
        {
            long id = 499000001;
            var list = AllBarksScriptsId();

            while (list.Contains(id.ToString()))
                id++;
            return id;
        }
        public static long FirstBarkAssetAvailable()
        {
            long id = 90000001;

            var refs = AllAssetsReference();
            var assets = AllAssetId(ScriptType.mobBark, AnimType.none);

            while (refs.Contains(id.ToString()) || assets.Contains(id.ToString()))
                id++;

            return id;
        }
        public static List<Monster> NotUpToDateMonsters()
        {
            if (datas == null || datas.monsters.Count == 0) return new List<Monster>();
            return datas.monsters.Values.Where(x => !x.IsUpToDate()).ToList();
        }
        public static void CheckIfMonstersAreUpToDate()
        {
            var list = NotUpToDateMonsters();
            if (list.Count == 0)
                return;

            var panel = new Controls.MonsterUpdatePanel(list);
            panel.Show();
        }

        #endregion

        #region Script Management

        public static bool GetOrCreate(string id, out LuaScript script)
        {
            if (datas.scripts.ContainsKey(id))
            {
                script = datas.scripts[id];
                return true;
            }
            else
            {
                script = new LuaScript(FullPathOfAnmScript(id));
                return false;
            }
        }
        public static bool GetOrCreate(ScriptType type, string id, out LuaScript script)
        {
            if (datas.scripts.ContainsKey(id))
            {
                script = datas.scripts[id];
                return true;
            }
            else
            {
                script = new LuaScript(type, id);
                return false;
            }
        }
        public static LuaScript GetOrCreate(string id)
        {
            GetOrCreate(id, out LuaScript script);
            return script;
        }
        public static LuaScript GetOrCreate(ScriptType type, string id)
        {
            GetOrCreate(type, id, out LuaScript script);
            return script;
        }
        public static LuaScript CreateScript(ScriptType type, string id)
        {
            return new LuaScript(type, id);
        }
        public static void LoadAllScriptsFromFolder()
        {
            foreach(string script in AllScriptFiles())
            {
                GetOrCreate(script);
            }
        }
        public static bool IsScriptUsed(LuaScript script)
        {
            return IsScriptUsed(script.id, out script);
        }
        public static bool IsScriptUsed(string id, out LuaScript script)
        {
            script = GetOrCreate(id);
            foreach(Monster m in datas.monsters.Values)
                if (m.UsesLuaScript(id))
                    return true;
            return false;
        }
        public static bool IsScriptUsed(string id)
        {
            return IsScriptUsed(id, out LuaScript script);
        }
        public static void AskScriptDeletion(string scriptId)
        {
            if (!IsScriptUsed(scriptId, out LuaScript script))
                if (MessageBox.Show("Removed Script is not used anywhere.\nDelete script file ?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    script.DeleteScript();
        }
        #endregion

        #region utils

        public static string FileNameFromPath(string path)
        {
            var split = path.Split('\\');
            var id = split[split.Length - 1];
            int index = id.IndexOf(".");
            if (index != -1)
                id = id.Substring(0, index);
            return id;
        }
        public static string FolderNameFromPath(string path)
        {
            var split = path.Split('\\');
            return split[split.Length - 1];
        }
        public static bool IsAps(string id)
        {
            return allApsIds.Contains(id);
        }
        public static bool IsScriptBark(string script)
        {
            return script.Substring(0, 3) == "499";
        }

        #endregion

    }

    [Serializable]
    public struct Parameters
    {
        public string svnFolder;
        public string trunkFolder;
        public int volume;
        public bool playerOn;
        public string lastMonsterOpened;
    }
    [Serializable]
    public class DatabaseSave
    {
        public SortedDictionary<string, Monster> monsters = new SortedDictionary<string, Monster>();
        public SortedDictionary<string, LuaScript> scripts = new SortedDictionary<string, LuaScript>();


    }
}
