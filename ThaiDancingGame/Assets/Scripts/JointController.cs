using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointController : MonoBehaviour {

    private Animator CurrentAnimator;
    [HideInInspector]
    public HumanBodyBonesTrackData CurrentBoneData;
    private Animator AnimatorToCompareTo;
    private BodyController CurrentBodyController;

    private JointInformation JI;
    private bool Tracking = false;
    private List<float> PercentageTrackingData;
    Vector3 JointInfoStartingPos;

    public void SetTrackingData (Animator _Animator, HumanBodyBonesTrackData _BoneData, Animator _Compare, BodyController _Controller) {
        CurrentAnimator = _Animator;
        CurrentBoneData = _BoneData;
        AnimatorToCompareTo = _Compare;
        CurrentBodyController = _Controller;

        //J.transform.SetParent(transform);
        if (CurrentBoneData.ShowJointInfo) {
            GameObject J = Instantiate (CurrentBodyController.JointInfoPrefab, transform.position, Quaternion.identity);
            JI = J.GetComponent<JointInformation> ();
            JI.UpdateInformation (CurrentBoneData.NameOverWrite != "" ? CurrentBoneData.NameOverWrite : CurrentBoneData.Joint.ToString ());
            transform.position = CurrentAnimator.GetBoneTransform (CurrentBoneData.Joint).position;
            JI.transform.position = transform.position;
            JointInfoStartingPos = JI.transform.position;
            JI.transform.position = JointInfoStartingPos + CurrentBoneData.JointInfoOffsetPos;
        }

    }

    void FixInfoPos () {
        JI.transform.position = JointInfoStartingPos + CurrentBoneData.JointInfoOffsetPos;
    }

    private void Update () {
        transform.position = CurrentAnimator.GetBoneTransform (CurrentBoneData.Joint).position;
        if (CurrentBoneData.ShowJointInfo) {
            FixInfoPos ();
        }
        //UpdateJointInfoPos();
        if (Tracking) PercentageTrackingData.Add (CalculateJointFeedBack ());
        else CalculateJointFeedBack ();
    }

    public void StartTracking () {
        Tracking = true;
        PercentageTrackingData = new List<float> ();
    }

    public void StopTracking () {
        Tracking = true;
    }

    public List<float> getTrackingData () {
        return PercentageTrackingData;
    }

    Vector3 predictedPos = Vector3.zero;

    float CalculateJointFeedBack () {
        //percentage cal
        float distance = CurrentBodyController.transform.position.x - AnimatorToCompareTo.transform.position.x;
        Vector3 cJointPos = transform.position;
        cJointPos.x -= distance;
        predictedPos = cJointPos;
        float distanceBetweenUserandData = Vector3.Distance (cJointPos, AnimatorToCompareTo.GetBoneTransform (CurrentBoneData.Joint).position);
        float PercentagePos = 100f - ((distanceBetweenUserandData / CurrentBoneData.MaxDistanceAway) * 100f);
        if (PercentagePos < 0) PercentagePos = 0;

        float angle = Quaternion.Angle (transform.localRotation, AnimatorToCompareTo.GetBoneTransform (CurrentBoneData.Joint).localRotation);
        float PercentageRot = 100f - ((angle / 360) * 100f);

        float totalPercentage = (PercentagePos + PercentageRot) / 2;
        totalPercentage = Mathf.RoundToInt (totalPercentage);

        //direction cal
        Vector3 TranslatedPos = transform.position - (transform.position.normalized * distance);
        Vector3 heading = AnimatorToCompareTo.GetBoneTransform (CurrentBoneData.Joint).position - TranslatedPos;
        float tempDistance = heading.magnitude;
        Vector3 direction = heading / tempDistance;
        direction = direction.normalized;
        direction = new Vector3 (Mathf.Round (direction.x), Mathf.Round (direction.y), Mathf.Round (direction.z));
        if (CurrentBoneData.ShowJointInfo) {
            JI.UpdateInformation (CurrentBoneData.NameOverWrite != "" ? CurrentBoneData.NameOverWrite : CurrentBoneData.Joint.ToString (), totalPercentage, GetDirectionName (direction));
        }
        return totalPercentage;
    }

    void UpdateJointInfoPos () {
        Vector3 JointInfoPos = transform.position;
        Vector3 CurrentBonePos = CurrentAnimator.GetBoneTransform (CurrentBoneData.Joint).position;
        if (!(CurrentBonePos.y >= CurrentAnimator.GetBoneTransform (HumanBodyBones.Head).position.y)) {
            if (CurrentBonePos.x > CurrentAnimator.GetBoneTransform (HumanBodyBones.Hips).position.x) {
                JointInfoPos.x += 1;
            } else {
                JointInfoPos.x -= 1;
            }
        }

        if (CurrentBonePos.y > CurrentAnimator.GetBoneTransform (HumanBodyBones.Chest).position.y && Mathf.Abs (CurrentBonePos.x - CurrentAnimator.GetBoneTransform (HumanBodyBones.Hips).position.x) < 0.5f) {
            JointInfoPos.y += 0.5f;
        } else if (CurrentBonePos.y < CurrentAnimator.GetBoneTransform (HumanBodyBones.Chest).position.y) {
            JointInfoPos.y -= 0.5f;
        }
        if (CurrentBoneData.ShowJointInfo) {
            JI.transform.position = Vector3.Lerp (JI.transform.position, JointInfoPos, Time.deltaTime * 4);
        }
    }

    string GetDirectionName (Vector3 Dir) {
        string Name = "nothing";
        if (Dir == Vector3.up) Name = "Up";
        else if (Dir == Vector3.down) Name = "Down";
        else if (Dir == Vector3.left) Name = "Left";
        else if (Dir == Vector3.right) Name = "Right";
        else if (Dir == Vector3.forward) Name = "Front";
        else if (Dir == Vector3.back) Name = "Back";
        else if (Dir == Vector3.zero) Name = "Perfect";
        return Name;
    }

    private void OnDrawGizmos () {
        if (BodyManager.instance.ShowBones) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (transform.position, 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere (predictedPos, 0.1f);

    }
}