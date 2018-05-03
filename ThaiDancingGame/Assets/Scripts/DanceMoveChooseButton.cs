using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DanceMoveChooseButton : MonoBehaviour {

    public Text DanceNameText;

    private string DanceName;
    private string[] DanceFileNames;

    public void SetDataForButton(string DanceName, string[] DanceFileNames)
    {
        this.DanceName = DanceName;
        DanceNameText.text = DanceName;
        this.DanceFileNames = DanceFileNames;
    }

    public void Choose()
    {
        GameManager.instance.ChooseDance(DanceName, DanceFileNames);
    }
}
