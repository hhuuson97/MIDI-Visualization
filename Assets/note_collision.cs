using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public abstract class Sound
{
    protected Func<int, int, double, double, double>[] modulate = { Modulation0, Modulation1, Modulation2, Modulation3, Modulation4, Modulation5, Modulation6, Modulation7, Modulation8, Modulation9, Modulation10 };

    protected String name;

    private static double Modulation0(int i, int s, double f, double x)
    {
        return Math.Sin((2 * Math.PI) * ((double) i / s) * f + x);
    }

    private static double Modulation1(int i, int sampleRate, double frequency, double x)
    {
        return 1 * Math.Sin(2 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation2(int i, int sampleRate, double frequency, double x)
    {
        return 1 * Math.Sin(4 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation3(int i, int sampleRate, double frequency, double x)
    {
        return 1 * Math.Sin(8 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation4(int i, int sampleRate, double frequency, double x)
    {
        return 1 * Math.Sin(0.5 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation5(int i, int sampleRate, double frequency, double x)
    {
        return 1 * Math.Sin(0.25 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation6(int i, int sampleRate, double frequency, double x)
    {
        return 0.5 * Math.Sin(2 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation7(int i, int sampleRate, double frequency, double x)
    {
        return 0.5 * Math.Sin(4 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation8(int i, int sampleRate, double frequency, double x)
    {
        return 0.5 * Math.Sin(8 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation9(int i, int sampleRate, double frequency, double x)
    {
        return 0.5 * Math.Sin(0.5 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    private static double Modulation10(int i, int sampleRate, double frequency, double x)
    {
        return 0.5 * Math.Sin(0.25 * Math.PI * (((double) i / sampleRate) * frequency) + x);
    }

    public abstract double getAttack();
    public abstract double getDampen(int sampleRate, double frequency, int volume);
    public abstract double getWave(int i, int sampleRate, double frequency, int volume);
}

public class PianoSound : Sound
{

    public PianoSound()
    {
        name = "piano";
    }

    public override double getAttack()
    {
        return 0.02;
    }

    public override double getDampen(int sampleRate, double frequency, int volume)
    {
        return Math.Pow(0.5 * Math.Log((frequency * volume) / sampleRate), 2);
    }

    public override double getWave(int i, int sampleRate, double frequency, int volume)
    {
        Func<int, int, double, double, double> bas = modulate[0];
        return modulate[1](
            i,
            sampleRate,
            frequency,
            Math.Pow(bas(i, sampleRate, frequency, 0), 2) +
                (0.75 * bas(i, sampleRate, frequency, 0.25)) +
                (0.1 * bas(i, sampleRate, frequency, 0.5))
        );
    }
}

public class OrganSound : Sound
{
    public OrganSound()
    {
        name = "organ";
    }

    public override double getAttack()
    {
        return 0.3;
    }

    public override double getDampen(int sampleRate, double frequency, int volume)
    {
        return 1 + (frequency * 0.01);
    }

    public override double getWave(int i, int sampleRate, double frequency, int volume)
    {
        Func<int, int, double, double, double> bas = modulate[0];
        return this.modulate[1](
            i,
            sampleRate,
            frequency,
            bas(i, sampleRate, frequency, 0) +
                0.5 * bas(i, sampleRate, frequency, 0.25) +
                0.25 * bas(i, sampleRate, frequency, 0.5)
        );
    }
}

public class AudioSynth
{
    private static AudioSynth instance;
    private int sampleRate = 44100;
    private int volume = 4381;
    private int bitsPerSample = 16;
    private int channels = 1;
    private Dictionary<String, double> notes = new Dictionary<String, double>();
    private Dictionary<String, double> durations = new Dictionary<string, double>();
    private Sound _sound;
    private Dictionary<int, Dictionary<String, Dictionary<double, AudioClip>>> _fileCache;

    private AudioSynth()
    {
        notes["C"] = 261.63; durations["C"] = 3.822;
        notes["C#"] = 277.18; durations["C#"] = 3.608;
        notes["D"] = 293.66; durations["D"] = 3.405;
        notes["D#"] = 311.13; durations["D#"] = 3.214;
        notes["E"] = 329.63; durations["E"] = 3.034;
        notes["F"] = 346.23; durations["F"] = 2.863;
        notes["F#"] = 369.99; durations["F#"] = 2.703;
        notes["G"] = 392.00; durations["G"] = 2.273;
        notes["G#"] = 415.30; durations["G#"] = 2.408;
        notes["A"] = 440.00; durations["A"] = 2.273;
        notes["A#"] = 466.16; durations["A#"] = 2.145;
        notes["B"] = 493.88; durations["B"] = 2.025;
    }

    public static AudioSynth getInstance()
    {
        if (instance == null)
            instance = new AudioSynth();
        return instance;
    }

    public int setSampleRate(int v)
    {
        sampleRate = Math.Max(Math.Min(v, 44100), 4000);
        clearCache();
        return sampleRate;
    }

    public int getSampleRate()
    {
        return sampleRate;
    }

    public int setVolume(double v)
    {
        if (double.IsNaN(v)) { v = 0; }
        v = Math.Round(v * 32768);
        volume = Math.Max(Math.Min((int) v, 32768), 0);
        clearCache();
        return volume;
    }

    public double getVolume()
    {
        return Math.Round((double) volume / 32768 * 10000) / 10000;
    }

    public void clearCache()
    {
        _fileCache.Clear();
        for (int i = 0; i <= 8; i++)
        {
            _fileCache[i] = new Dictionary<string, Dictionary<double, AudioClip>>();
            foreach (var key in notes.Keys)
                _fileCache[i][key] = new Dictionary<double, AudioClip>();
        }
    }

    public AudioSynth createInstrument(String sound)
    {
        if (sound == "piano")
        {
            this._sound = new PianoSound();
        }
        if (sound == "organ")
        {
            this._sound = new OrganSound();
        }
        _fileCache = new Dictionary<int, Dictionary<string, Dictionary<double, AudioClip>>>();
        clearCache();
        return this;
    }

    private byte[] pack(int c, int arg)
    {
        if (c == 0)
            return new byte[] { (byte) (arg & 0xFF), (byte)((arg >> 8) & 0xFF) };
        return new byte[] { (byte)(arg & 0xFF), (byte)((arg >> 8) & 0xFF), (byte)((arg >> 16) & 0xFF), (byte)((arg >> 24) & 0xFF) };
    }

    public bool play(AudioSource audioSource, String note, int octave)
    {
        AudioClip audioClip = this.generate(note, octave);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
            return true;
        }
        return false;
    }

    private void writeBytes(MemoryStream ms, byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
            ms.WriteByte(data[i]);
    }

    public AudioClip generate(string note, int octave)
    {
        octave |= 0;
        octave = Math.Min(8, Math.Max(1, octave));
        double time = durations[note] * Math.Pow(2, 4 - octave);
        if (!notes.ContainsKey(note)) return null;
        if (_fileCache.ContainsKey(octave) && _fileCache[octave].ContainsKey(note) && _fileCache[octave][note].ContainsKey(time)) {
            return _fileCache[octave][note][time];
        } else
        {
            var frequency = notes[note] * Math.Pow(2, octave - 4);
            var sampleRate = this.sampleRate;
            var volume = this.volume;
            var channels = this.channels;
            var bitsPerSample = this.bitsPerSample;
            var attack = _sound.getAttack();
            var dampen = _sound.getDampen(sampleRate, frequency, volume);
            int val = 0;

            byte[] data = new byte[(int)(sampleRate * time * 2)];
            Array.Clear(data, 0, (int)(sampleRate * time * 2));

            int attackLen = (int) (sampleRate * attack);
            int decayLen = (int) (sampleRate * time);

            int i;

            for (i = 0; i != attackLen; i++)
            {
                val = (int) (volume * (i / (sampleRate * attack)) * _sound.getWave(i, sampleRate, frequency, volume));

                data[i << 1] = (byte) (val & 0xFF);
                data[(i << 1) + 1] = (byte) ((val >> 8) & 0xFF);
            }

            for (; i != decayLen; i++)
            {
                val = (int) (volume * Math.Pow((1 - ((i - (sampleRate * attack)) / (sampleRate * (time - attack)))), dampen) * _sound.getWave(i, sampleRate, frequency, volume));

                data[i << 1] = (byte) (val & 0xFF);
                data[(i << 1) + 1] = (byte) ((val >> 8) & 0xFF);
            }

            MemoryStream ms = new MemoryStream();

            writeBytes(ms, Encoding.ASCII.GetBytes("RIFF"));
            writeBytes(ms, pack(1, 4 + (8 + 24/* chunk 1 length */) + (8 + 8/* chunk 2 length */))); // Length
            writeBytes(ms, Encoding.ASCII.GetBytes("WAVE"));
            // chunk 1
            writeBytes(ms, Encoding.ASCII.GetBytes("fmt ")); // Sub-chunk identifier
            writeBytes(ms, pack(1, 16)); // Chunk length
            writeBytes(ms, pack(0, 1)); // Audio format (1 is linear quantization)
            writeBytes(ms, pack(0, channels));
            writeBytes(ms, pack(1, sampleRate));
            writeBytes(ms, pack(1, sampleRate * channels * bitsPerSample / 8)); // Byte rate
            writeBytes(ms, pack(0, channels * bitsPerSample / 8)); // Block Align
            writeBytes(ms, pack(0, bitsPerSample)); // 16
            // chunk 2
            writeBytes(ms, Encoding.ASCII.GetBytes("data")); // Sub-chunk identifier
            writeBytes(ms, pack(1, data.Length)); // Chunk length
            writeBytes(ms, data);
            ms.Flush();

            AudioClip audioClip = WavUtility.ToAudioClip(ms.ToArray());

            _fileCache[octave][note][time] = audioClip;
			return audioClip;
        }
    }
}

public class note_collision : MonoBehaviour {

    bool is_play;
    double y_instance;

    AudioSource audioSource;
    SpringJoint springJoint;
    AudioClip audioClip;
    
    String[][] keyOctaves = new String[][] {
        //new String[] {"Alpha1", "Alpha2", "Alpha3", "Alpha4", "Alpha5", "Alpha6", "Alpha7"},
        //new String[] {"Q", "W", "E", "R", "T", "Y", "U"},
        //new String[] {"A", "S", "D", "F", "G", "H", "J"},
        //new String[] {KeyCode.Z.ToString(), KeyCode.X.ToString(), KeyCode.C.ToString(), KeyCode.V.ToString(), KeyCode.B.ToString(), KeyCode.N.ToString(), KeyCode.M.ToString()},
        //new String[] {KeyCode.Comma.ToString(), KeyCode.Period.ToString(), KeyCode.Slash.ToString(), KeyCode.RightShift.ToString(), KeyCode.Keypad1.ToString(), KeyCode.Keypad2.ToString(), KeyCode.Keypad3.ToString()},
        //new String[] {KeyCode.L.ToString(), KeyCode.Semicolon.ToString(), KeyCode.Quote.ToString(), KeyCode.Return.ToString(), KeyCode.Keypad4.ToString(), KeyCode.Keypad5.ToString(), KeyCode.Keypad6.ToString()},
        //new String[] {KeyCode.P.ToString(), KeyCode.LeftBracket.ToString(), KeyCode.RightBracket.ToString(), KeyCode.Backslash.ToString(), KeyCode.Keypad7.ToString(), KeyCode.Keypad8.ToString(), KeyCode.Keypad9.ToString()},
        //new String[] {KeyCode.Minus.ToString(), KeyCode.Equals.ToString(), KeyCode.Backspace.ToString()}
    };

    // Use this for initialization
    void Start ()
    {
        is_play = false;
        y_instance = transform.position.y;

        audioSource = gameObject.AddComponent<AudioSource>();

        GameObject note = GameObject.Find("notes");
        Transform white_notes, black_notes;
        for (int i = 0; i < 7; i++)
        {
            white_notes = note.transform.GetChild(i).GetChild(0);
            for (int j = 0; j < white_notes.childCount; j++)
                if (white_notes.GetChild(j).gameObject != gameObject)
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), white_notes.GetChild(j).gameObject.GetComponent<Collider>());
            black_notes = note.transform.GetChild(i).GetChild(1);
            for (int j = 0; j < black_notes.childCount; j++)
                if (black_notes.GetChild(j).gameObject != gameObject)
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), black_notes.GetChild(j).gameObject.GetComponent<Collider>());
        }

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        springJoint = gameObject.AddComponent<SpringJoint>();
        springJoint.spring = 1000;
        springJoint.damper = 0.02f;
        springJoint.tolerance = 0;

        int vir_octave = int.Parse(transform.parent.parent.name.Split('_')[1]);
        int octave = vir_octave;
        var _note = transform.name.Split('_')[2];
        if (_note[0] > 'B') octave++;
        AudioSynth.getInstance().createInstrument("piano");
        audioClip = AudioSynth.getInstance().generate(_note, octave);
    }

    public void Play()
    {
        audioSource.PlayOneShot(audioClip);
    }

    // Update is called once per frame
    void Update()
    {
        int vir_octave = int.Parse(transform.parent.parent.name.Split('_')[1]);
        int octave = vir_octave;
        var note = transform.name.Split('_')[2];
        if (note[0] > 'B') octave++;
        // transform.position.Set(transform.position.x, Math.Max(joint.anchor.y - 0.05f, transform.position.y), transform.position.z);
        if (transform.position.y < y_instance - 0.025f)
        {
            if (!is_play)
            {
                Play();
            }
            is_play = true;
        }
        if (transform.position.y > y_instance - 0.01f)
            is_play = false;
        
        //if (vir_octave < 3 && Input.GetKey(keyOctaves[vir_octave][note[0] - 'A']) && (!Input.GetKey("KeypadPeriod") || note.Length == 2))
        //{
        //    rigidbody.AddForce(0, 5, 0);
        //}
    }
}
