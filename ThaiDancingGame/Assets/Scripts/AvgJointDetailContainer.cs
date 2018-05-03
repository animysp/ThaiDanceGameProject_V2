using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvgJointDetailContainer : MonoBehaviour
{
    public Text JointNameText;
    public Text AvgJointPercText;


    public void SetDataForButton(string JointName,float AvgJointPerc)
    {
        JointNameText.text = JointName;
        AvgJointPercText.text = AvgJointPerc + "%";
    }
}
