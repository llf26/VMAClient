using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MIDIPlayer
{
    class MIDITrack
    {
        const int SEQUENCE_NUMBER = 0x00;
        const int TEXT_EVENT = 0x01;
        const int COPYRIGHT_NOTICE = 0x02;
        const int SEQTRACK_NAME = 0x03;
        const int INSTRUMENT_NAME = 0x04;
        const int LYRIC = 0x05;
        const int MARKER = 0x06;
        const int CUE_POINT = 0x07;
        const int MIDI_CHANNEL_PREFIX = 0x20;
        const int END_OF_TRACK = 0x2F;
        const int SET_TEMPO = 0x51;
        const int SMTPE_OFFSET = 0x54;
        const int TIME_SIGNATURE = 0x58;
        const int KEY_SIGNATURE = 0x59;

        public int length { get; }
        public string name;
        private bool reachedEnd = false;
        private bool lastEventWasNote = false;
        public List<MIDINote> notes = new List<MIDINote>();

        public MIDITrack(BinaryReader reader, int format, int trackNumber)
        {
            name = "Track " + trackNumber.ToString();

            byte[] trackChunkBytes = reader.ReadBytes(4);
            string trackChunk = Encoding.ASCII.GetString(trackChunkBytes);

            byte[] trackLengthBytes = reader.ReadBytes(4);


            int trackLength = Utils.BytesToInt(trackLengthBytes);
            Debug.Log($"(Track {trackNumber}) Length: {trackLength}");
            length = trackLength;

            if (format == 1 && trackNumber == 1)
            {
                
                //  1. Time signature
                //  2. Sequence/Track Name
                //  3. Sequence Number
                //  4. Marker
                //  5. SMTPE Offset

            }
            //for(int i = 0; i < 11; i++)
            while(!reachedEnd && reader.BaseStream.Position != reader.BaseStream.Length)
            {
                ReadEvent(reader);
            }
        }

        public void ReadEvent(BinaryReader reader)
        {
            long deltaTime = Utils.ReadVariableLength(reader);
            Debug.Log($"Ticks: {deltaTime} ");
            byte identifier = reader.ReadByte();

            switch(identifier)
            {
                case 0xF0:
                    Debug.Log("Sysex Event");
                    lastEventWasNote = false;
                    break;
                case 0xF7:
                    Debug.Log("Sysex Escape");
                    lastEventWasNote = false;
                    break;
                case 0xFF:
                    ReadMetaEvent(reader);
                    lastEventWasNote = false;
                    break;
                case 0xB0:
                    //  Of the form 0xB0NN, where NN is the channel #
                    //  Followed by 0ccccccc 0vvvvvvv where c's are the controller #
                    //  and v's are the controller value
                    Debug.Log("Control Change");
                    reader.ReadBytes(2);
                    lastEventWasNote = false;
                    break;
                case 0xC0:
                    //  Of the form 0xC0NN, where NN is the channel #
                    //  Followed by 0ppppppp where p's are the new program number.
                    Debug.Log("Program Change");
                    reader.ReadBytes(1);
                    lastEventWasNote = false;
                    break;
                case 0x0A:
                    //  Can't find much about pan values; they seem
                    //  to be one byte long.
                    Debug.Log("Pan Value");
                    lastEventWasNote = false;
                    reader.ReadByte();
                    break;
                case byte n when (n >= 0x5A && n <= 0x5F):
                    Debug.Log("Effects Depth");
                    lastEventWasNote = false;
                    reader.ReadByte();
                    break;
                default:
                    ReadMIDIEvent(identifier, reader, deltaTime);
                    break;
            }
        }

        public void ReadMIDIEvent(byte identifier, BinaryReader reader, long deltaTime)
        {
            switch (identifier)
            {
                case byte n when (n >= 0x90 && n <= 0x9F):
                    int notePlayed = Utils.BytesToInt(reader.ReadBytes(1));
                    int velocity = Utils.BytesToInt(reader.ReadBytes(1));
                    Debug.Log($"(Note On) Note:{notePlayed}, Vel: {velocity}");
                    notes.Add(new MIDINote(Clamp(notePlayed,0,127), Clamp(velocity, 0, 127), 1, deltaTime));
                    lastEventWasNote = true;
                    break;
                case byte n when (n >= 0x80 && n <= 0x8F):
                    int noteOff = Utils.BytesToInt(reader.ReadBytes(1));
                    int offVelocity = Utils.BytesToInt(reader.ReadBytes(1));
                    Debug.Log($"(Note Off) Note:{noteOff}, Vel: {offVelocity}");
                    notes.Add(new MIDINote(Clamp(noteOff,0,127), Clamp(offVelocity, 0, 127), 2, deltaTime));
                    lastEventWasNote = true;
                    break;
                default:
                    //Debug.Log("MIDI Event");
                    //reader.ReadBytes(2);
                    if (lastEventWasNote)
                    {
                        int tmpNoteOn = (int)identifier;
                        int tmpoffVelocity = Utils.BytesToInt(reader.ReadBytes(1));
                        Debug.Log($"(Note Off) Note:{tmpNoteOn}, Vel: {tmpoffVelocity}");
                        notes.Add(new MIDINote(Clamp(tmpNoteOn, 0, 127), Clamp(tmpoffVelocity, 0, 127), 1, deltaTime));
                        lastEventWasNote = true;
                        break;
                    }
                    else
                    {
                        Utils.ReadVariableLength(reader);
                        break;
                    }
            }
        }

        public void ReadMetaEvent(BinaryReader reader)
        {
            int metaIdentifier = reader.ReadByte();

            switch (metaIdentifier)
            {
                case SEQUENCE_NUMBER:
                    Debug.Log("Sequence Number");
                    byte[] seqNumberBytes = reader.ReadBytes(3);
                    break;
                case TEXT_EVENT:
                    Debug.Log("Text Event: ");
                    string text = GetTextData(reader);
                    Debug.Log(text);
                    break;
                case COPYRIGHT_NOTICE:
                    Debug.Log("Copyright Notice: ");
                    string cprtText = GetTextData(reader);
                    Debug.Log(cprtText);
                    break;
                case SEQTRACK_NAME:
                    Debug.Log("Sequence/Track Name: ");
                    name = GetTextData(reader);
                    Debug.Log(name);
                    break;
                case INSTRUMENT_NAME:
                    Debug.Log("Instrument Name: ");
                    string instrName = GetTextData(reader);
                    Debug.Log(instrName);
                    break;
                case LYRIC:
                    Debug.Log("Lyric(s): ");
                    string lyrics = GetTextData(reader);
                    Debug.Log(lyrics);
                    break;
                case MARKER:
                    Debug.Log("Marker: ");
                    string marker = GetTextData(reader);
                    Debug.Log(marker);
                    break;
                case CUE_POINT:
                    Debug.Log("Event Cue: ");
                    string cue = GetTextData(reader);
                    Debug.Log(cue);
                    break;
                case MIDI_CHANNEL_PREFIX:
                    Debug.Log("MIDI Channel Prefix");
                    byte[] channelPrefixBytes = reader.ReadBytes(2);
                    break;
                case END_OF_TRACK:
                    reader.ReadByte();
                    Debug.Log("End of Track");
                    reachedEnd = true;
                    break;
                case SET_TEMPO:
                    byte[] setTempoBytes = reader.ReadBytes(4);
                    Debug.Log("Set Tempo");
                    break;
                case SMTPE_OFFSET:
                    byte[] SMTPEBytes = reader.ReadBytes(6);
                    Debug.Log("SMTPE Time");
                    break;
                case TIME_SIGNATURE:
                    Debug.Log("Time Signature");
                    byte[] timeSignatureBytes = reader.ReadBytes(5);
                    break;
                case KEY_SIGNATURE:
                    byte[] keySignatureBytes = reader.ReadBytes(3);
                    Debug.Log("Key Signature");
                    break;
                default:
                    Debug.Log("Unknown Meta Event");
                    int length = (int)Utils.ReadVariableLength(reader);
                    reader.ReadBytes(length);
                    break;
            }
        }

        public string GetTextData(BinaryReader reader)
        {
            int textLength = (int)Utils.ReadVariableLength(reader);
            byte[] textBytes = reader.ReadBytes(textLength);
            return Encoding.ASCII.GetString(textBytes);
        }

        public int Clamp(int n, int min, int max)
        {
            if(n < min)
            {
                return min;
            }

            if(n > max)
            {
                return max;
            }

            return n;
        }
    }
}
