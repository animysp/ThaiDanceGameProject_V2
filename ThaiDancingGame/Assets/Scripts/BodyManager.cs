using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyManager : MonoBehaviour
{
    public static BodyManager instance;
    public HumanBodyBonesTrackData[] BonesToTrack;
    public bool ShowBones = true;
    public BodyController BC;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(!KinectManager.IsKinectInitialized()) return;
    }

    public void StartChecking()
    {
        BC.StartTrackingOnAllJoints();
    }

    public void StopChecking()
    {
        BC.StopTrackingOnAllJoints();
    }
}

[System.Serializable]
public class HumanBodyBonesTrackData
{
    public HumanBodyBones Joint;
    public string NameOverWrite;
    public float MaxDistanceAway = 1;
    public bool ShowJointInfo = true;
    public Vector3 JointInfoOffsetPos = Vector3.zero;
}

public class AverageJointPercentageStore
{
    public string JointName;
    public float AveragePercentage;
    public AverageJointPercentageStore(string JointName,float AveragePercentage)
    {
        this.JointName = JointName;
        this.AveragePercentage = AveragePercentage;
    }
}
