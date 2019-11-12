using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIPlayer
{
    class MIDIFile
    {
        public int format { get; }
        public int trackCount { get; }
        public int unitsPerNote { get; }
        public List<MIDITrack> tracks = new List<MIDITrack>();
        
        public MIDIFile(Stream filePathIn)
        {
                using (var reader = new BinaryReader(filePathIn))
                {
                    //  First four bytes of file are ASCII string representing chunk type
                    byte[] chunkTypeBytes = reader.ReadBytes(4);

                    string chunkType = Encoding.ASCII.GetString(chunkTypeBytes);
                    Debug.Log($"(Header) Chunk Type: {chunkType}");

                    //  Next four bytes are 32-bit integer representing length of header (always 6 for MIDI 1.0)
                    byte[] headerDataLengthBytes = reader.ReadBytes(4);
                    int headerDataLength = Utils.BytesToInt(headerDataLengthBytes);
                    Debug.Log($"(Header) Data Length: {headerDataLength}");

                    //  For MIDI 1.0, next two bytes are the MIDI file format.
                    //  Info on formats here: csie.ntu.edu.tw/~r92092/ref/midi/#mff0
                    byte[] formatBytes = reader.ReadBytes(2);

                    format = Utils.BytesToInt(formatBytes);
                    Debug.Log($"(Header) Format: {format}");

                    byte[] numTracksBytes = reader.ReadBytes(2);
                    trackCount = Utils.BytesToInt(numTracksBytes);
                    Debug.Log($"(Header) Number of tracks: {trackCount}");

                    byte[] divisionBytes = reader.ReadBytes(2);

                    char[] divisionBinary = (Utils.ToBinaryString(divisionBytes[0], 8) +
                                                Utils.ToBinaryString(divisionBytes[1], 8)
                                              ).ToCharArray();
                    char divMSB = divisionBinary[0];

                    if (divMSB == '0')
                    {
                        //  Bits 0-14 represent the number of delta-time units in each a quarter-note.
                        char[] unitsPerNoteBinary = divisionBinary.Skip(1).Take(15).ToArray();
                        string accumulator = "";
                        for (int i = 0; i < unitsPerNoteBinary.Length; i++)
                        {
                            accumulator += unitsPerNoteBinary[i];
                        }

                        unitsPerNote = Convert.ToInt32(accumulator, 2);
                        Debug.Log($"(Header) Delta-time units (ticks) per quarter-note: {unitsPerNote}");
                    }

                    else if (divMSB == '1')
                    {
                        Debug.Log("SMTPE Frames");
                    }

                    else
                    {
                        Debug.Log("Bits: " + string.Concat(divisionBinary));
                        Debug.Log("MSB: " + divMSB);
                    }

                    //var track = new MIDITrack(reader, format, 1);

                    for(int i = 0; i < trackCount; i++)
                    {
                        if (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            var track = new MIDITrack(reader, format, i + 1);
                            tracks.Add(track);
                        }
                    }

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        byte test = reader.ReadByte();
                        Debug.Log(test.ToString("X2") + " ");
                    }
                    
                    //while (true)
                    //{

                  //  }
                }            
        }
    }
}
