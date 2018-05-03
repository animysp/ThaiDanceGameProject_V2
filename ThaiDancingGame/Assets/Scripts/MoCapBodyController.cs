using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoCapBodyController : MonoBehaviour
{
    Animator currentAnim;
    AnimatorOverrideController myOverrideController;

    private void Start()
    {
        currentAnim = GetComponent<Animator>();
        RuntimeAnimatorController myController = currentAnim.runtimeAnimatorController;
        myOverrideController = new AnimatorOverrideController();
        myOverrideController.runtimeAnimatorController = myController;
    }

    public float RunAnimation(AnimationClip[] Clips)
    {
        float TotalTimingToEnd = 0;
        for (int i = 0; i < Clips.Length; i++)
        {
            myOverrideController["Clip"+(i+1)] = Clips[i];
            TotalTimingToEnd += Clips[i].length;
        }
        currentAnim.runtimeAnimatorController = myOverrideController;
        currentAnim.Rebind();
        currentAnim.SetTrigger("StartTrack");
        StartCoroutine(RunAfterEnd(TotalTimingToEnd));
        return TotalTimingToEnd;
    }

    IEnumerator RunAfterEnd(float timetowait)
    {
        yield return new WaitForSeconds(timetowait-1);
        currentAnim.SetTrigger("StopTrack");
    }
}
