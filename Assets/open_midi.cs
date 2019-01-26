using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GracesGames.SimpleFileBrowser.Scripts;

public class open_midi : MonoBehaviour {

	public void ButtonClick()
    {
        OpenFileBrowser(FileBrowserMode.Load);
    }

    private void OpenFileBrowser(FileBrowserMode fileBrowserMode)
    {
        FileBrowser fileBrowserScript = gameObject.GetComponent<FileBrowser>();
        fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);
        if (fileBrowserMode == FileBrowserMode.Save)
        {
            fileBrowserScript.SaveFilePanel("DemoText", new string[] { "mid" });
            // Subscribe to OnFileSelect event (call SaveFileUsingPath using path) 
            // fileBrowserScript.OnFileSelect += SaveFileUsingPath;
        }
        else
        {
            fileBrowserScript.OpenFilePanel(new string[] { "mid" });
            // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
            fileBrowserScript.OnFileSelect += GameObject.Find("notes").GetComponent<Sheets>().SetInput;
        }
    }
}
