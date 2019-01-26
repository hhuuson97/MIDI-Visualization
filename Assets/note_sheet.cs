using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class note_sheet : MonoBehaviour {

    bool isPlay;
    GameObject note;

	// Use this for initialization
	void Start () {
        string[] info = name.Split('_');
        note = GameObject.Find("octave_" + info[1]);
        if (info[2].Length == 1) // White note
        {
            note = note.transform.GetChild(info[2].Length - 1).GetChild(info[2][0] - 'A').gameObject;
        }
        else
        {
            switch (info[2][0])
            {
                case 'A':
                    note = note.transform.GetChild(info[2].Length - 1).GetChild(0).gameObject;
                    break;
                case 'C':
                    note = note.transform.GetChild(info[2].Length - 1).GetChild(1).gameObject;
                    break;
                case 'D':
                    note = note.transform.GetChild(info[2].Length - 1).GetChild(2).gameObject;
                    break;
                case 'F':
                    note = note.transform.GetChild(info[2].Length - 1).GetChild(3).gameObject;
                    break;
                default:
                    note = note.transform.GetChild(info[2].Length - 1).GetChild(4).gameObject;
                    break;
            }
        }
        isPlay = false;
    }
	
	// Update is called once per frame
	void Update () {
        transform.Translate(0, 0, -Time.deltaTime);
        if (Math.Abs(note.transform.position.z - transform.position.z) < (transform.localScale.z + note.transform.localScale.z) / 2 && !isPlay)
        {
            // note.GetComponent<note_collision>().Play();
            note.GetComponent<Rigidbody>().AddForce(new Vector3(0, -100, 0));
            isPlay = true;

            Renderer rend = GetComponent<Renderer>();
            rend.material.color = Color.gray;
        }
        if (Math.Abs(note.transform.position.z + note.transform.localScale.z / 2 - transform.position.z - transform.localScale.z / 2) < Time.deltaTime)
            Destroy(gameObject, 0);
    }
}
