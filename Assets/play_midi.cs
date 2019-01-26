using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class play_midi : MonoBehaviour {

    static GameObject[] menu;

    public void PlayMidi()
    {
        GameObject.Find("notes").GetComponent<Sheets>().Play();
        ShowMenu(false);
    }

    public static void ShowMenu(bool isShow)
    {
        if (!isShow)
            menu = GameObject.FindGameObjectsWithTag("Menu");
        foreach (GameObject gameObject in menu)
            gameObject.SetActive(isShow);
    }
}
