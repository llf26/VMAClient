using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIPlayer
{
    public class MIDINote
    {
        public int noteNumber;
        public int velocity;
        public int type;
        public long deltaTime;

        public MIDINote(int noteNumber, int velocity, int type, long deltaTime)
        {
            this.noteNumber = noteNumber;
            this.velocity = velocity;
            this.type = type;
            this.deltaTime = deltaTime;
            
        }
    }
}
