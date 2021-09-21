using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SwfDotNet.IO;
using SwfDotNet.IO.Tags;
using SwfDotNet.IO.ByteCode;
using SwfDotNet.IO.ByteCode.Actions;

namespace WakfuAudio.Scripts.Classes
{
    public class SwfDecompiler
    {
        public Swf swf;
        public Dictionary<string, int> animations = new Dictionary<string, int>();
        public Dictionary<int, DefineSpriteTag> animTags = new Dictionary<int, DefineSpriteTag>();
        public string file;
        private bool set = false;
        public bool showErrors = true;

        public SwfDecompiler(string newFile)
        {
            file = newFile;
        }
        public bool SetSwf()
        {
            if (set) return true;
            if (!File.Exists(file))
            {
                if(showErrors) Task.Run(() => MessageBox.Show("File does not exist :\n" + file));
                return false;
            }

            var reader = new SwfReader(file);
            try { swf = reader.ReadSwf(); }
            catch
            {
                if (showErrors) Task.Run(() => MessageBox.Show("Can't decompile SWF :\n" + file));
                return false;
            }
           
            reader.Close();
            SetAnimations();
            set = true;
            return true;
        }
        private void SetAnimations()
        {
            animations.Clear();
            animTags.Clear();
            foreach (BaseTag t in swf.Tags)
            {
                if (!(t is ExportAssetsTag)) continue;
                var assert = (t as ExportAssetsTag).ExportedCharacters[0];
                if(assert.Name.Length > 2 && Int32.TryParse(assert.Name[0].ToString(), out int result))
                    animations.Add(assert.Name, assert.TargetCharacterId);
            }

            foreach(BaseTag tag in swf.Tags)
            {
                if (tag is DefineSpriteTag)
                    if (animations.ContainsValue((tag as DefineSpriteTag).CharacterId))
                        animTags.Add((tag as DefineSpriteTag).CharacterId, tag as DefineSpriteTag);
            }
        }

        public SortedDictionary<string, Animation> GetAnimations()
        {
            if (SetSwf())
            {
                return GetAnimList();
            }
            else
                return new SortedDictionary<string, Animation>();
            
        }
        public async Task WriteSwf()
        {
            await Task.Delay(1);
            SwfWriter writer = new SwfWriter(file);
            writer.Write(swf);
            writer.Close();
        }

        #region Swf Edition

        public void WriteAudioScriptsOfAnim(string animName, SortedDictionary<int, List<string>> scripts)
        {
            if (!SetSwf())
                return;

            Decompiler dc = new Decompiler(swf.Version);
            Compiler comp = new Compiler();
            var anim = GetAnimation(animName);

            int frame = 1;
            int index = 0;
            foreach(BaseTag tag in anim.Tags.ToArray().ToList())
            {
                #region Supression des scripts audio présents

                if (tag is DoActionTag)
                {
                    var byteCodeEnu = tag.GetEnumerator();
                    while (byteCodeEnu.MoveNext())
                        foreach (BaseAction action in dc.Decompile((byte[])byteCodeEnu.Current).ToArray())
                            if (ActionIsAudioScript(action))
                            {
                                anim.Tags.Remove(tag);
                                index--;
                                goto Next;
                            }
                        Next:;
                }

                #endregion

                #region creation des nouveaux DoActionTag

                if (tag is ShowFrameTag)
                {
                    if (scripts.ContainsKey(frame))
                    {
                        foreach (string script in scripts[frame])
                        {
                            comp = new Compiler();
                            var actions = new ArrayList();
                            actions.Add(new ActionPush(0, "runScript(" + script + ")"));
                            actions.Add(new ActionTrace());
                            actions.Add(new ActionEnd());
                            var action = new DoActionTag(comp.Compile(actions));
                            anim.Tags.Insert(index, action);
                            index++;
                        }
                    }
                    frame++;
                }

                #endregion
                index++;
            }
        }

        #endregion

        #region utils

        private DefineSpriteTag GetAnimation(string name)
        {
            return GetAnimation(animations[name]);
        }
        private DefineSpriteTag GetAnimation(int id)
        {
            foreach(BaseTag tag in swf.Tags)
            {
                if (!(tag is DefineSpriteTag)) continue;
                var def = tag as DefineSpriteTag;
                if (def.CharacterId == id)
                    return def;
            }
            return null;
        }
        private SortedDictionary<int,ArrayList> GetActionsOfAnimation(DefineSpriteTag anim)
        {
            SortedDictionary<int, ArrayList> list = new SortedDictionary<int, ArrayList>();
            IEnumerator tagsEnu = anim.Tags.GetEnumerator();
            Decompiler dc = new Decompiler(swf.Version);

            int frame = 1;
            while (tagsEnu.MoveNext())
            {
                var tag = (BaseTag)tagsEnu.Current;
                if (tag is ShowFrameTag) frame++;
                if (tag.ActionRecCount != 0)
                {
                    IEnumerator byteCodeEnu = tag.GetEnumerator();
                    while (byteCodeEnu.MoveNext())
                    {
                        ArrayList actions = dc.Decompile((byte[])byteCodeEnu.Current);
                        if (!list.ContainsKey(frame))
                            list.Add(frame, new ArrayList());
                        list[frame].AddRange(actions);
                    }
                }
            }
            return list;
        }
        private SortedDictionary<int, List<string>> GetScriptsOfAnimation(DefineSpriteTag anim)
        {
            var list = new SortedDictionary<int, List<string>>();
            var actions = GetActionsOfAnimation(anim);

            foreach(KeyValuePair<int, ArrayList> action in actions)
            {
                foreach(BaseAction a in action.Value)
                {
                    if (!ActionIsAudioScript(a)) continue;
                    string id = Utils.GetStringFromPaterns(a.ToString(), "runScript(", ")");
                    if (!list.ContainsKey(action.Key))
                        list.Add(action.Key, new List<string>());
                    list[action.Key].Add(id);
                }
            }

            return list;
        }
        private SortedDictionary<string, Animation> GetAnimList()
        {
            var list = new SortedDictionary<string, Animation>();
            foreach(KeyValuePair<string, int> anim in animations)
            {
                char angle = anim.Key[0];
                string name = anim.Key.Substring(2);

                var a = new Animation(name);
                a.angles.Add(angle);

                var frames = GetScriptsOfAnimation(animTags[anim.Value]);
                foreach(KeyValuePair<int, List<string>> frame in frames)
                {
                    foreach(string script in frame.Value)
                    {
                        var dic = Database.IsScriptBark(script) ? a.barks : a.sounds;
                        try { dic.Add(script, new List<int>()); } catch { }
                        dic[script].Add(frame.Key);
                    }
                }

                list.Add(anim.Key, a);
            }

            return list;
        }
        private static bool ActionIsAudioScript(BaseAction action)
        {
            return action.ToString().IndexOf("runScript") != -1;
        }

        #endregion

    }
}
