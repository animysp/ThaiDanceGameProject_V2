using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    public Animator AnimatorToCompareTracker;

    private Animator animatorComponent;
    private List<JointController> JointControllers;

    private GameObject JointControllerPrefab;
    [HideInInspector]
    public GameObject JointInfoPrefab;

    void Start()
    {
        animatorComponent = GetComponent<Animator>();
        JointControllerPrefab = (GameObject)Resources.Load("prefabs/JointController", typeof(GameObject));
        JointInfoPrefab = (GameObject)Resources.Load("prefabs/JointInfo", typeof(GameObject));
        CreateJoints();
    }

    void CreateJoints()
    {
        JointControllers = new List<JointController>();
        HumanBodyBonesTrackData[] BonesToTrack = BodyManager.instance.BonesToTrack;
        for (int i = 0; i < BonesToTrack.Length; i++)
        {
            if (animatorComponent.GetBoneTransform(BonesToTrack[i].Joint) != null)
            {
                GameObject JointObject = Instantiate(JointControllerPrefab, Vector3.zero, Quaternion.identity);
                JointController JC = JointObject.GetComponent<JointController>();
                JC.SetTrackingData(animatorComponent, BonesToTrack[i], AnimatorToCompareTracker, this);
                JointControllers.Add(JC);
            }
            else
            {
                Debug.Log(gameObject.name + " doesn't have " + BonesToTrack[i].ToString() + " Bone", gameObject);
            }
        }
    }

    public void StartTrackingOnAllJoints()
    {
        for (int i = 0; i < JointControllers.Count; i++)
        {
            JointControllers[i].StartTracking();
        }
    }

    public void StopTrackingOnAllJoints()
    {
        for (int i = 0; i < JointControllers.Count; i++)
        {
            JointControllers[i].StopTracking();
        }
    }

    public float GetAverageTrackingAccuracy()
    {
        float overallPercentage = 0;
        int NumberOfDataTrackerd = 0;
        for (int i = 0; i < JointControllers.Count; i++)
        {
            List<float> tempJointData = JointControllers[i].getTrackingData();
            NumberOfDataTrackerd += tempJointData.Count;
            for (int j = 0; j < tempJointData.Count; j++)
            {
                overallPercentage += tempJointData[j];
            }
        }

        return Mathf.Round((overallPercentage / NumberOfDataTrackerd));
    }


    public List<AverageJointPercentageStore> GetAverageTrackingAccuracyOfIndividualJoints()
    {
        List<AverageJointPercentageStore> AverageJointPercentageStoreList = new List<AverageJointPercentageStore>();
        for (int i = 0; i < JointControllers.Count; i++)
        {
            float overallPercentage = 0;
            List<float> tempJointData = JointControllers[i].getTrackingData();
            for (int j = 0; j < tempJointData.Count; j++)
            {
                overallPercentage += tempJointData[j];
            }
            AverageJointPercentageStore temp = new AverageJointPercentageStore(JointControllers[i].CurrentBoneData.Joint.ToString(),Mathf.Round((overallPercentage / tempJointData.Count)));
            AverageJointPercentageStoreList.Add(temp);
        }

        return AverageJointPercentageStoreList;
    }
}
