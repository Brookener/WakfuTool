using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakfuAudio.Scripts.Classes
{
    [Serializable]
    public class Note
    {
        public string name = "New Note";
        public string content = "";

        public override string ToString()
        {
            return name;
        }
    }
}
