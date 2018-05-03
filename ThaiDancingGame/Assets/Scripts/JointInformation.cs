using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointInformation : MonoBehaviour
{
    public TextMesh JointText;
    public TextMesh PercentageText;
    public TextMesh WhatToDoText;
    public Color[] ColorOfTexts = new Color[3];

    public void UpdateInformation(string Joint, float Percentage=0, string Info="Perfect")
    {
        JointText.text = ""+Joint;
        JointText.color = ColorOfTexts[0];
        PercentageText.text = Percentage+"%";
        PercentageText.color = ColorOfTexts[1];
        WhatToDoText.text = Info == "Perfect" ? "You are Perfect!" : "Move " + Info; 
        WhatToDoText.color = ColorOfTexts[2];
    }
}
