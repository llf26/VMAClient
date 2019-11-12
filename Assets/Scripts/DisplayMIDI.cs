using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DisplayMIDI : MonoBehaviour
{
    public Text text;
    public GameObject prefabCube, parentGameobject;

    private MIDIPlayer.MIDIFile midiFile;

    private const float START_HEIGHT = 0;
    private const int Z_COORD = 0;
    private const float NOTE_WIDTH = 22f / 128f;

    private float curY = START_HEIGHT;

    private Dictionary<int, float> noteDict = new Dictionary<int, float>(); // Key: Note Number, Value: Deltatime

    // Start is called before the first frame update
    void Start()
    {
        using (var stream = new FileStream("C:/Users/lfrit/source/repos/VMAClient/Assets/StreamingAssets/coldplay-a_sky_full_of_stars.mid", FileMode.Open, FileAccess.Read))
        {
            midiFile = new MIDIPlayer.MIDIFile(stream);
        }

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //prefabCube.transform.position = new Vector3(0, START_HEIGHT, Z_COORD);
        //prefabCube.transform.localScale = new Vector3(NOTE_WIDTH, 1, 0.1f);
        //Instantiate(prefabCube);

        foreach (var note in midiFile.tracks[0].notes)
        {
            Debug.Log($"Delta Time: {note.deltaTime}, Note Type: {note.type} Note Number: {note.noteNumber}");
            curY += (note.deltaTime / 1000f);

            if(note.type == 1 && note.velocity > 0) //  If note on, store reference to current Y in dictionary
            {
                noteDict[note.noteNumber] = curY;
            }

            else if((note.type == 1 && note.velocity <= 0) || note.type == 2) // If note off, find difference in height and then inst. rectangle of that height
            {
                //notenumber is between 0 and 127, vison is between -11 and 11
                prefabCube.transform.position = new Vector3(((float)note.noteNumber - 64)*(11f/64f), noteDict[note.noteNumber] + ((curY - (noteDict[note.noteNumber]))/2f), Z_COORD);
                prefabCube.transform.localScale = new Vector3(NOTE_WIDTH, curY - (noteDict[note.noteNumber]), 0.1f);
                Instantiate(prefabCube, parentGameobject.transform);
            }

            
        }
    }
    // Update is called once per frame
    void Update()
    {

        
    }
}
