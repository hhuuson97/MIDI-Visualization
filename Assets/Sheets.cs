using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using System.IO;
using System.Linq;

public class FloatingNote
{
    GameObject gameObject;

    public static float showLength = 5;

    public static Color[] channelColors = new Color[] {
        new Color(0, 0, 1, 0.1f),
        new Color(0, 1, 0, 0.1f),
        new Color(1, 0, 0, 0.1f),
        new Color(0, 0.25f, 0.75f, 0.1f),
        new Color(0.25f, 0, 0.75f, 0.1f),
        new Color(0.25f, 0.75f, 0, 0.1f),
        new Color(0.75f, 0, 0.25f, 0.1f),
        new Color(0, 0.75f, 0.25f, 0.1f),
        new Color(0.75f, 0.25f, 0, 0.1f),
        new Color(0, 0.5f, 0.5f, 0.1f),
        new Color(0.5f, 0, 0.5f, 0.1f),
        new Color(0, 0.5f, 0.5f, 0.1f),
        new Color(0, 0, 0, 0.1f),
        new Color(1, 1, 0, 0.1f),
        new Color(1, 0, 1, 0.1f),
        new Color(0, 1, 1, 0.1f)
    };

    public FloatingNote(int octave, string name, float lifetime, int channel)
    {
        int vir_octave;
        if (name[0] > 'B')
            vir_octave = octave - 1;
        else
            vir_octave = octave;
        GameObject mObject = GameObject.Find("octave_" + vir_octave);
        Transform mTransform;
        if (name.Length == 1) // White note
        {
            mTransform = mObject.transform.GetChild(0).GetChild(name[0] - 'A');
        }
        else
        {
            switch (name[0])
            {
                case 'A':
                    mTransform = mObject.transform.GetChild(1).GetChild(0);
                    break;
                case 'C':
                    mTransform = mObject.transform.GetChild(1).GetChild(1);
                    break;
                case 'D':
                    mTransform = mObject.transform.GetChild(1).GetChild(2);
                    break;
                case 'F':
                    mTransform = mObject.transform.GetChild(1).GetChild(3);
                    break;
                default:
                    mTransform = mObject.transform.GetChild(1).GetChild(4);
                    break;
            }
        }
        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = "note_" + vir_octave + "_" + name;
        gameObject.AddComponent<note_sheet>();
        gameObject.transform.position = new Vector3(
            mTransform.position.x, 
            mTransform.position.y + 0.25f + channel * 0.25f, 
            mTransform.position.z + mTransform.localScale.z / 2 + showLength + lifetime / 1000f / 2);
        gameObject.transform.localScale = new Vector3(
            mTransform.localScale.x,
            mTransform.localScale.y,
            lifetime / 1000f
        );
        Renderer rend = gameObject.GetComponent<Renderer>();
        rend.material.color = channelColors[channel];
    }

}

public class NoteInfo
{
    public float time; // time note begin play
    public int number; // 0 - 127 ~ C-1 - G9 ~ C4 = 60
    public int length; // ms

    public NoteInfo(float time, int number, int length)
    {
        this.time = time;
        this.number = number;
        this.length = length;
    }
}

public class Sheets : MonoBehaviour
{

    string input;

    public MidiFile midi;

    public float ticks;
    public float offset;
    public float bpm;

    public bool isStop;

    public NoteInfo[][] notes;

    public string[] midiToNotes = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public void SetInput(string path)
    {
        input = path;
        //Loading midi file "mainSong.bytes" from resources folder
        //Its a midi file, extension has been changed to .bytes manually
        //TextAsset asset = Resources.Load(input) as TextAsset;
        //Stream s = new MemoryStream(asset.bytes);
        //Read the file
        midi = new MidiFile(input, true);
        //Ticks needed for timing calculations
        ticks = midi.DeltaTicksPerQuarterNote;
        StartPlayback();
    }

    public void Play()
    {
        isStop = false;
        offset = 0; // time of the sheets
    }

    // Use this for initialization
    void Start()
    {
        isStop = true;
        offset = 0; // time of the sheets
        bpm = 100; // speed
        input = Application.dataPath + "/Cannon_in_D.mid";
        SetInput(input);
    }

    public void StartPlayback()
    {
        //9 is the number of the track we are reading notes from
        //you'll have to experiment with that, i cant remember why i chose 9 here
        notes = new NoteInfo[midi.Tracks][];

        for (var n = 0; n < midi.Tracks; n++)
        {
            var count = 0;
            foreach (MidiEvent note in midi.Events[n])
                if (MidiEvent.IsNoteOn(note))
                    count++;
            notes[n] = new NoteInfo[count];
        }

        for (var n = 0; n < midi.Tracks; n++)
        {
            var count = 0;
            foreach (MidiEvent note in midi.Events[n])
            {
                //If its the start of the note event
                if (MidiEvent.IsNoteOn(note))
                {
                    //Cast to note event and process it
                    NoteOnEvent noe = (NoteOnEvent)note;
                    notes[n][count] = NoteEvent(noe);
                    count++;
                }
            }
        }
    }

    public NoteInfo NoteEvent(NoteOnEvent noe)
    {
        //Time until the start of the note in seconds
        float time = (60 * noe.AbsoluteTime) / (bpm * ticks);

        //The number (key) of the note. Heres a useful chart of number-to-note translation:
        //http://www.electronics.dit.ie/staff/tscarff/Music_technology/midi/midi_note_numbers_for_octaves.htm
        int noteNumber = noe.NoteNumber;

        //Start coroutine for each note at the start of the playback
        //Really awful way to do stuff, but its simple
        //StartCoroutine(CreateAction(time, noteNumber, noe.NoteLength));
        //Debug.Log(time + " " + noteNumber + " " + noe.NoteLength);
        return new NoteInfo(time, noteNumber, noe.NoteLength);
    }

    void Update()
    {
        if (!isStop)
        {
            bool isEnd = true;
            var currentOffset = offset + Time.deltaTime;
            for (var i = 0; i < notes.Length; i++)
                for (var j = 0; j < notes[i].Length; j++)
                {
                    if ((offset <= notes[i][j].time) && (notes[i][j].time < currentOffset) &&
                        (21 <= notes[i][j].number) && (notes[i][j].number <= 108))
                    {
                        new FloatingNote(
                            (notes[i][j].number + 3) / 12 - 1,
                            midiToNotes[notes[i][j].number % 12],
                            notes[i][j].length,
                            i
                            );
                    }

                    if (offset - FloatingNote.showLength < notes[i][j].time)
                        isEnd = false;
                }
            offset = currentOffset;

            if (isEnd)
            {
                play_midi.ShowMenu(true);
                isStop = true;
            }
        }
    }

}
