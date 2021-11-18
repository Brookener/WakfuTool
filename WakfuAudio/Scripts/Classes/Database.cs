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
        public static NoteSave notes;

        public static List<string> allApsIds;

        public static void Setup()
        {
            LoadParameters();
            LoadDatas();
            LoadMonsterDatas();
            LoadNotes();
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

        #region Notes

        private const string NoteFileName = "Notes.dat";
        public static void LoadNotes()
        {
            if (File.Exists(NoteFileName))
            {
                Stream file = File.Open(NoteFileName, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();

                try
                {
                    notes = (NoteSave)bf.Deserialize(file);
                }
                catch (Exception e)
                {
                    notes = new NoteSave();
                    MessageBox.Show(e.Message);

                }
                file.Close();
            }
            else
                notes = new NoteSave();
        }
        public static void SaveNotes()
        {
            Stream file = null;
            file = File.Open(NoteFileName, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, notes);
            file.Close();
        }

        #endregion

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
        public static string DialogFolder()
        {
            return ScriptFolder() + @"\interactiveDialogs";
        }
        public static string FullPathOfScriptOfType(string script)
        {
            var array = Directory.GetFiles(ScriptFolder(), script + "*", SearchOption.AllDirectories);

            return (array.Length > 0) ? array[0] : "";
        }
        public static string FullPathOfScriptOfType(string script, ScriptType type)
        {
            switch (type)
            {
                default:
                    return FullPathOfAnmScript(script);
                case ScriptType.aps:
                    return FullPathOfApsScript(script);
                case ScriptType.dialog:
                    return FullPathOfDialogScript(script);
            }
        }
        public static string FullPathOfAnmScript(string script)
        {
            return AnmFolder() + @"\" + script + ".lua";
        }
        public static string FullPathOfApsScript(string script)
        {
            return ApsFolder() + @"\" + script + ".lua";
        }
        public static string FullPathOfDialogScript(string script)
        {
            return DialogFolder() + @"\" + script + ".lua";
        }
        public static bool AnmScriptExists(string script)
        {
            return File.Exists(FullPathOfAnmScript(script));
        }
        public static bool ApsScriptExists(string script)
        {
            return File.Exists(FullPathOfApsScript(script));
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

        #region Monster Datas

        public static Dictionary<int, Monster.Datas> monsterDatas = new Dictionary<int, Monster.Datas>();
        public static void LoadMonsterDatas()
        {
            monsterDatas = Psql.GetMonsterDatas();
        }
        public static string NameOf(string id)
        {
            if (id.Length > 4 && Int32.TryParse(id.Substring(id.Length - 4), out int monsterId) && monsterDatas.ContainsKey(monsterId))
                return monsterDatas[monsterId].name;
            else
                return "";

        }
        public static string DialogOf(string id)
        {
            if (id.Length > 4 && Int32.TryParse(id.Substring(id.Length - 4), out int monsterId) && monsterDatas.ContainsKey(monsterId))
                return monsterDatas[monsterId].dialog;
            else
                return "";

        }


        #endregion

        #region Values 

        public static string[] AllAnimScriptFiles()
        {
            return Directory.GetFiles(AnmFolder());
        }
        public static string[] AllApsScriptFiles()
        {
            return Directory.GetFiles(ApsFolder());
        }
        public static string[] AllDialogScriptFiles()
        {
            return Directory.GetFiles(DialogFolder());
        }
        public static List<string> AllScriptFiles()
        {
            var list = AllAnimScriptFiles().ToList();
            list.AddRange(AllApsScriptFiles());
            list.AddRange(AllDialogScriptFiles());
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

            var refs = AllAssetsReferences();
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

        public static bool GetOrExtract(ScriptType type, string id, out LuaScript script)
        {
            if (datas.scripts.ContainsKey(id))
            {
                script = datas.scripts[id];
                return true;
            }
            else
            {
                script = new LuaScript(FullPathOfScriptOfType(id, type));
                return false;
            }
        }
        public static bool GetOrExtract(string id, out LuaScript script)
        {
            if (datas.scripts.ContainsKey(id))
            {
                script = datas.scripts[id];
                return true;
            }
            else
            {
                script = new LuaScript(FullPathOfScriptOfType(id));
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
        public static LuaScript GetOrExtract(ScriptType type, string id)
        {
            GetOrExtract(type, id, out LuaScript script);
            return script;
        }
        public static LuaScript GetOrExtract(string id)
        {
            return GetOrExtract(id, out LuaScript lua) ? lua : null;
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
            foreach(string script in AllScriptId())
            {
                GetOrExtract(script);
            }
        }
        public static bool IsScriptUsed(LuaScript script)
        {
            return IsScriptUsed(script.id, out script);
        }
        public static bool IsScriptUsed(string id, out LuaScript script)
        {
            script = GetOrExtract(id);
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
        public static IEnumerable<LuaScript> ScriptsUsingsAsset(string asset)
        {
            return datas.scripts.Values.Where(x => x.AllAssets().Contains(asset));
        }
        public static Dictionary<string, List<string>> AssetInScriptsDictionnary()
        {
            var dic = new Dictionary<string, List<string>>();
            foreach(LuaScript script in datas.scripts.Values)
                foreach(string asset in script.AllAssets())
                {
                    if (!dic.ContainsKey(asset))
                        dic.Add(asset, new List<string>());
                    dic[asset].Add(script.id);
                }

            return dic;
        }
        public static Dictionary<string, List<Monster>> MonstersUsedInScripts()
        {
            var dic = new Dictionary<string, List<Monster>>();
            foreach(Monster monster in datas.monsters.Values)
            {
                foreach(string script in monster.AllScriptIds())
                {
                    if (!dic.ContainsKey(script))
                        dic.Add(script, new List<Monster>());
                    dic[script].Add(monster);
                }
            }
            return dic;
        }
        

        #endregion

        #region Assets

        public static bool AssetFile(string asset, out string file)
        {
            file = "";
            if (asset == null)
                return false;
            var files = Directory.GetFiles(ExportsFolder(), asset.Replace("\r", "") + ".ogg", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                file = files[0];
                return true;
            }
            return false;
        }
        public static string[] AllAssetSources(string asset)
        {
            try
            {
                return Directory.GetFiles(Database.SourcesFolder(), asset + "*", SearchOption.AllDirectories);
            }
            catch
            {
                return new string[0];
            }
        }
        
        public static string FirstAssetSource(string asset)
        {
            var sources = AllAssetSources(asset);
            return sources.Length > 0 ? sources[0] : "";
        }
        public static void PlayAssetSource(string asset, double vol)
        {
            AudioPlayer.PlayAudio(FirstAssetSource(asset), vol);
        }
        public static string[] AllAssetFiles()
        {
            return Directory.GetFiles(ExportsFolder(), "*.ogg", SearchOption.AllDirectories);
        }
        public static string[] AllAudioSourcesFiles()
        {
            return Directory.GetFiles(SourcesFolder(), "*.wav", SearchOption.AllDirectories);
        }
        public static IEnumerable<string> AllAudioSourcesFilesNames()
        {
            return AllAudioSourcesFiles().Select(x => FileNameFromPath(x));
        }
        public static string[] AllAssetFiles(ScriptType script, AnimType anim)
        {
            return Directory.GetFiles(AudioFolder(script, anim, true), ".ogg");
        }
        public static IEnumerable<string> AllAssetId()
        {
            return AllAssetFiles().Select(x => FileNameFromPath(x)).Distinct();
        }
        public static IEnumerable<string> AllAssetId(ScriptType script, AnimType anim)
        {
            return AllAssetFiles(script, anim).Select(x => FileNameFromPath(x));
        }
        public static IEnumerable<string> AllAssetsReferences()
        {
            return datas.scripts.Values.SelectMany(x => x.AllAssets()).Distinct();
        }
        public static bool DatasContainsAsset(string asset)
        {
            return AllAssetsReferences().Contains(asset);
        }
        public static IEnumerable<string> AllUnreferencedAssets()
        {
            var assets = AllAssetsReferences().ToList();
            var list = AllAssetId().ToList();
            foreach(string id in list.ToList())
                if (assets.Contains(id))
                {
                    assets.Remove(id);
                    list.Remove(id);
                }
            return list;
        }
        public static Dictionary<string, List<string>> SourcesByAsset()
        {
            var dic = new Dictionary<string, List<string>>();
            foreach (string s in AllAudioSourcesFiles())
            {
                var id = FileNameFromPath(s).Split('_')[0];
                if (!dic.ContainsKey(id))
                    dic.Add(id, new List<string>());
                dic[id].Add(s);
            }
            return dic;
        }
        public static Dictionary<string, string> ExportFilesByAsset()
        {
            var dic = new Dictionary<string, string>();
            foreach (string s in AllAssetFiles())
            {
                var id = FileNameFromPath(s);
                if (!dic.ContainsKey(id))
                    dic.Add(id, s);
            }
            return dic;
        }
        public static List<Integration> AllIntegrations()
        {
            return datas.scripts.Values.SelectMany(x => x.integrations).ToList();
        }

        #endregion

        #region utils

        public static string FileNameFromPath(string path)
        {
            var split = path.Split('\\');
            var id = split[split.Length - 1];
            id = id.Substring(0, id.Length - 4);
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
        public List<string> audioPlayers;
    }
    [Serializable]
    public class DatabaseSave
    {
        public SortedDictionary<string, Monster> monsters = new SortedDictionary<string, Monster>();
        public SortedDictionary<string, LuaScript> scripts = new SortedDictionary<string, LuaScript>();
        
    }

    [Serializable]
    public class NoteSave
    {
        public SortedDictionary<string, string> notesList = new SortedDictionary<string, string>();
        public string lastNote = "";
        //public List<Note> notesList = new List<Note>();
        //public Note lastNote;
    }
}
