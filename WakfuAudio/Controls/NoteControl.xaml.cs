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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WakfuAudio.Scripts.Classes;

namespace WakfuAudio
{
    /// <summary>
    /// Logique d'interaction pour NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        //public List<Note> notes;
        //public Note note;

        public SortedDictionary<string, string> notes = new SortedDictionary<string, string>();
        public string noteName = "";

        public NoteControl()
        {
            notes = Database.notes.notesList;
            InitializeComponent();
            UpdateNoteList();
            Load(Database.notes.lastNote);
        }

        public void UpdateNoteList()
        {
            NoteList.ItemsSource = notes.Keys.ToList();
            NoteList.SelectedItem = noteName;

        }

        public void Load(string newNote)
        {
            if (!notes.ContainsKey(newNote))
                return;
            noteName = newNote;
            NameBox.Text = newNote;
            Content.Text = notes[newNote];

            NoteList.SelectedItem = noteName;
            //NameBox.IsEnabled = Content.IsEnabled = note != null;
        }

        private void NameKeyUp(object sender, KeyEventArgs e)
        {
            if (notes.ContainsKey(NameBox.Text))
                return;
            notes.Remove(noteName);
            noteName = NameBox.Text;
            notes.Add(noteName, Content.Text);
            Database.SaveNotes();
            UpdateNoteList();

        }

        private void ContentKeyUp(object sender, KeyEventArgs e)
        {
            if (notes[noteName] == Content.Text)
                return;
            notes[noteName] = Content.Text;
            Database.SaveNotes();
        }

        private void NewNoteClick(object sender, RoutedEventArgs e)
        {
            var name = "New Note " + notes.Keys.Where(x => x.Contains("New Note ")).Count().ToString();
            notes.Add(name, "");
            Load(name);
            Database.SaveNotes();
            UpdateNoteList();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            notes.Remove(noteName);
            Database.SaveNotes();
            Load(notes.Last().Key);
            UpdateNoteList();

        }

        private void NoteListClosed(object sender, EventArgs e)
        {
            //MessageBox.Show(NoteList.SelectedItem?.ToString());

            var name = NoteList.SelectedItem?.ToString();
            if (name == null)
                return;
            Load(name);
            Database.notes.lastNote = name;
            Database.SaveNotes();
        }
    }
}
