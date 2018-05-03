using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

public class Test : MonoBehaviour
{
    Animator currentAnim;


    void Start()
    {
        
        RuntimeAnimatorController myController = currentAnim.runtimeAnimatorController;
        AnimatorOverrideController myOverrideController = new AnimatorOverrideController();
        myOverrideController.runtimeAnimatorController = myController;

        TextAsset dataAsJson = (TextAsset)Resources.Load("DanceClips/DanceClipData", typeof(TextAsset));
        var data = JSON.Parse(dataAsJson.text);
        var FullDanceArray = data["FullDance"].AsArray;

        string filename = "";

        for (int i = 0; i < FullDanceArray.Count; i++)
        {
            filename = FullDanceArray[i]["filename"].Value;
        }

        AnimationClip DanceClip = (AnimationClip)Resources.Load("DanceClips/" + filename, typeof(AnimationClip));
        myOverrideController["Clip1"] = DanceClip;
        myOverrideController["Clip3"] = DanceClip;
        currentAnim.runtimeAnimatorController = myOverrideController;
    }

}
