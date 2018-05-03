using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RockVR.Video;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public enum GameStates { UserNameInput, CharacterDetectionChecking, Playing, Pause, DanceChoiceMenu, Gameover, Restart, Tutorial, Waiting };
 GameStates currentGameState;

 public GameObject MainMenuUI;
 public GameObject TutorialUI;
 public GameObject InGameUI;
 public GameObject GameOverUI;
 public GameObject UserNameInputUI;
 public GameObject LeaderboardUI;
 public Text LeaderboardDanceNameText;
 public Text TutorialDanceNameText;
 public RectTransform LeaderboardScrollContent;
 public GameObject LeaderboardScrollUIPrefab;
 public Text LeaderboardUICurrentPlayerName;
 public Text LeaderboardUICurrentPlayerScore;
 public Text GameOverAvgText;
 public RectTransform AvgJointScrollContent;
 public GameObject AvgJointScrollUIPrefab;
 public Text InGameTextFeedback;
 public GameObject SingleClipsScrollObject;
 public GameObject DanceClipsScrollObject;
 public RectTransform SingleClipsScrollContent;
 public RectTransform DanceClipsScrollContent;
 public GameObject GestureButton;
 public GameObject SingleButton;
 public Color SelectColor;
 public Color UnSelectedColor;
 public Sprite SelectImage;
 public Sprite UnSelectedImage;
 public GameObject GestureSelectIcon;
 public GameObject SingleSelectIcon;
 public GameObject DanceDetailsUIPrefab;
 public UnityEngine.Video.VideoPlayer CharacterVideoPlayer;
 public UnityEngine.Video.VideoPlayer MoCapVideoPlayer;
 public MoCapBodyController MCBC;
 public float TimeToWaitForGameToStart = 10;
     public float ReadySetGoTiming = 6;

 public UnityEngine.Video.VideoPlayer tutorialVDO;

 [HideInInspector]
 public List<int> VideoNumsUsed;

 private string CurrentDanceName = null;
 private AnimationClip[] DanceClipsChoosen;

 private bool Starting = false;
 private Coroutine StartTimerStore;
 private float TimeToWait = 10;
 private float Playtime = 0;

 private string UsernameValue;
 private string UserIDValue;
 private float Usercurrenthighscoreforgame = 0;
 private Coroutine CheckUserInputLoopStore;

 private Coroutine stopTutorial;

 private void Start () {
 instance = this;
 currentGameState = GameStates.UserNameInput;
    }

    private void OnApplicationQuit () {
        //VideoCaptureCtrl.instance.StopCapture ();
    }

    private void Init () {
        Playtime = 0;
        Starting = false;
        TimeToWait = TimeToWaitForGameToStart+ReadySetGoTiming;
        VideoNumsUsed = new List<int> ();
        CloseAllUI ();
        if (File.Exists (Application.dataPath + "/StreamingAssets/videoclip0.mp4")) File.Delete (Application.dataPath + "/StreamingAssets/videoclip0.mp4");
        if (File.Exists (Application.dataPath + "/StreamingAssets/videoclip1.mp4")) File.Delete (Application.dataPath + "/StreamingAssets/videoclip1.mp4");
        if (StartTimerStore != null) StopCoroutine (StartTimerStore);
    }

    void CloseAllUI () {
        MainMenuUI.SetActive (false);
        TutorialUI.SetActive (false);
        InGameUI.SetActive (false);
        GameOverUI.SetActive (false);
        UserNameInputUI.SetActive (false);
        LeaderboardUI.SetActive (false);
    }

    private void Update () {
        switch (currentGameState) {
            case GameStates.Playing:
                InGamePlaying ();
                break;
            case GameStates.CharacterDetectionChecking:
                InGameCheck ();
                break;
            case GameStates.Pause:
                break;
            case GameStates.DanceChoiceMenu:
                Init ();
                MainMenuUI.SetActive (true);
                SingleMovementAreaPress ();
                GenerateDanceList (SingleClipsScrollContent, "SingleMovement");
                GenerateDanceList (DanceClipsScrollContent, "FullDance");
                currentGameState = GameStates.Waiting;
                break;
            case GameStates.Tutorial:
                stopTutorial = StartCoroutine (WaitForTutorialToEnd ());
                currentGameState = GameStates.Waiting;
                break;
            case GameStates.Restart:
                break;
            case GameStates.Gameover:
                GameOverState ();
                currentGameState = GameStates.Waiting;
                break;
            case GameStates.UserNameInput:
                Init ();
                UserNameInputUI.SetActive (true);
                CheckUserInputLoopStore = StartCoroutine (CheckUserInputFields ());
                currentGameState = GameStates.Waiting;
                break;
            case GameStates.Waiting:
                break;
        }
    }

    #region UserDataInput
    public void UserNameUpdate (string name) {
        UsernameValue = name;
    }

    public void UserIDUpdate (string id) {
        UserIDValue = id;
    }

    public bool UserNameCheck () {
        if (UsernameValue == "" || UsernameValue == null) {
            return false;
        }

        return true;
    }

    public bool UserIDCheck () {
        if (UserIDValue == "" || UserIDValue == null) {
            return false;
        }

        return true;
    }

    IEnumerator CheckUserInputFields () {
        yield return StartCoroutine (WaitForKeyDown (KeyCode.Return));
        if (UserIDCheck () == true && UserNameCheck () == true) {
            EnterUser ();
        } else {
            StartCoroutine (CheckUserInputFields ());
        }
    }

    public void UserInputDoneButton () {
        if (UserIDCheck () && UserNameCheck ()) {
            if (CheckUserInputLoopStore != null) StopCoroutine (CheckUserInputLoopStore);
            EnterUser ();
        }
    }

    public void EnterUser () {
        Debug.Log ("Username: " + UsernameValue + "\nId: " + UserIDValue);
        CheckUserData ();
        currentGameState = GameStates.DanceChoiceMenu;
    }
    #endregion

    IEnumerator WaitForKeyDown (KeyCode keyCode) {
        do {
            yield return null;
        } while (!Input.GetKeyDown (keyCode));
    }

    void CheckUserData () {
        TextAsset dataAsJson = (TextAsset) Resources.Load ("Users", typeof (TextAsset));
        var data = JSON.Parse (dataAsJson.text);
        var FullDanceArray = data["Users"].AsArray;
        bool found = false;
        for (int i = 0; i < FullDanceArray.Count; i++) {
            if (FullDanceArray[i]["user_id"] == UserIDValue &&
                FullDanceArray[i]["username"] == UsernameValue) {
                found = true;
                break;
            }
        }

        if (found == false) {
            int c = FullDanceArray.Count;
            FullDanceArray[c]["username"] = UsernameValue;
            FullDanceArray[c]["user_id"] = UserIDValue;

            string path = "Assets/Resources/Users.json";

            string str = data.ToString ();
            using (FileStream fs = new FileStream (path, FileMode.Create)) {
                using (StreamWriter writer = new StreamWriter (fs)) {
                    writer.Write (str);
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh ();
#endif
        }
    }

    public void BackToMenuPress () {
        currentGameState = GameStates.DanceChoiceMenu;
    }

    public void SingleMovementAreaPress () {
        SingleClipsScrollObject.SetActive (true);
        DanceClipsScrollObject.SetActive (false);
        SingleButton.GetComponent<Image> ().sprite = SelectImage;
        GestureButton.GetComponent<Image> ().sprite = UnSelectedImage;
        SingleButton.GetComponentInChildren<Text> ().color = SelectColor;
        GestureButton.GetComponentInChildren<Text> ().color = UnSelectedColor;

        SingleSelectIcon.SetActive (true);
        GestureSelectIcon.SetActive (false);
    }

    public void DanceMovementAreaPress () {
        SingleClipsScrollObject.SetActive (false);
        DanceClipsScrollObject.SetActive (true);
        SingleButton.GetComponent<Image> ().sprite = UnSelectedImage;
        GestureButton.GetComponent<Image> ().sprite = SelectImage;
        SingleButton.GetComponentInChildren<Text> ().color = UnSelectedColor;
        GestureButton.GetComponentInChildren<Text> ().color = SelectColor;

        SingleSelectIcon.SetActive (false);
        GestureSelectIcon.SetActive (true);
    }

    void GameOverState () {
        if (VideoCaptureCtrl.instance.status != VideoCaptureCtrlBase.StatusType.FINISH) {
            VideoCaptureCtrl.instance.StopCapture ();
        }
        CloseAllUI ();
        BodyManager.instance.StopChecking ();
        GameOverUI.SetActive (true);
        float avgoverallScore = BodyManager.instance.BC.GetAverageTrackingAccuracy ();
        GameOverAvgText.text = avgoverallScore + "%";

        UpdateCurrentHighScore (avgoverallScore);

        List<AverageJointPercentageStore> tempStore = BodyManager.instance.BC.GetAverageTrackingAccuracyOfIndividualJoints ();

        Vector2 ScrollContentSize = AvgJointScrollContent.sizeDelta;
        RectTransform AvgDetailUirect = AvgJointScrollUIPrefab.GetComponent (typeof (RectTransform)) as RectTransform;

        ScrollContentSize.y = (AvgDetailUirect.sizeDelta.y + 10) * tempStore.Count;
        AvgJointScrollContent.sizeDelta = ScrollContentSize;

        float cHeight = -5;
        for (int i = 0; i < tempStore.Count; i++) {
            GameObject GDUI = Instantiate (AvgJointScrollUIPrefab);
            GDUI.transform.SetParent (AvgJointScrollContent.transform);
            RectTransform r = GDUI.GetComponent (typeof (RectTransform)) as RectTransform;
            r.localScale = AvgDetailUirect.localScale;
            Vector3 p = new Vector3 (0, cHeight, 0);
            r.anchoredPosition = p;
            cHeight -= (AvgDetailUirect.sizeDelta.y + 10);

            GDUI.GetComponent<AvgJointDetailContainer> ().SetDataForButton (tempStore[i].JointName, tempStore[i].AveragePercentage);
        }
        StartCoroutine (PlayVideo (CharacterVideoPlayer, Application.dataPath + "/StreamingAssets/videoclip0.mp4"));
        StartCoroutine (PlayVideo (MoCapVideoPlayer, Application.dataPath + "/StreamingAssets/videoclip1.mp4"));
    }

    void UpdateCurrentHighScore (float score) {
        Usercurrenthighscoreforgame = score;
        TextAsset dataAsJson = (TextAsset) Resources.Load ("Leaderboard", typeof (TextAsset));
        var data = JSON.Parse (dataAsJson.text);
        var FullDanceArray = data["Leaderboard"].AsArray;
        bool foundDanceName = false;
        int danceIndex = 0;
        for (int i = 0; i < FullDanceArray.Count; i++) {
            if (FullDanceArray[i]["dance_name"] == CurrentDanceName) {
                foundDanceName = true;
                danceIndex = i;
                break;
            }
        }

        if (foundDanceName == false) {
            int c = FullDanceArray.Count;
            danceIndex = c;
            FullDanceArray[c]["dance_name"] = CurrentDanceName;
        }

        var userinDanceArray = FullDanceArray[danceIndex]["users_highscore"].AsArray;

        if (userinDanceArray == null || userinDanceArray.Count == 0) {
            FullDanceArray[danceIndex]["users_highscore"][0]["username"] = UsernameValue;
            FullDanceArray[danceIndex]["users_highscore"][0]["user_id"] = UserIDValue;
            FullDanceArray[danceIndex]["users_highscore"][0]["highscore"].AsFloat = score;
        } else {
            bool foundUserInDanceMove = false;
            for (int i = 0; i < userinDanceArray.Count; i++) {
                if (userinDanceArray[i]["username"] == UsernameValue && userinDanceArray[i]["user_id"] == UserIDValue) {
                    if (score > userinDanceArray[i]["highscore"].AsFloat) {
                        foundUserInDanceMove = true;
                        userinDanceArray[i]["highscore"].AsFloat = score;
                    }
                    break;
                }
            }

            if (foundUserInDanceMove == false) {
                int c = userinDanceArray.Count;
                userinDanceArray[c]["username"] = UsernameValue;
                userinDanceArray[c]["user_id"] = UserIDValue;
                userinDanceArray[c]["highscore"].AsFloat = score;
            }
            FullDanceArray[danceIndex]["users_highscore"] = userinDanceArray.AsArray;
        }

        string path = "Assets/Resources/Leaderboard.json";

        string str = data.ToString ();
        using (FileStream fs = new FileStream (path, FileMode.Create)) {
            using (StreamWriter writer = new StreamWriter (fs)) {
                writer.Write (str);
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh ();
#endif
    }

    public void Leaderboard () {
        CloseAllUI ();
        LeaderboardUI.SetActive (true);

        LeaderboardDanceNameText.text = CurrentDanceName + " Leaderboard";

        TextAsset dataAsJson = (TextAsset) Resources.Load ("Leaderboard", typeof (TextAsset));
        var data = JSON.Parse (dataAsJson.text);
        var FullDanceArray = data["Leaderboard"].AsArray;

        for (int i = 0; i < FullDanceArray.Count; i++) {
            if (FullDanceArray[i]["dance_name"] == CurrentDanceName) {
                FullDanceArray = FullDanceArray[i]["users_highscore"].AsArray;
            }
        }

        List<TempUserDataStore> userDataForDance = new List<TempUserDataStore> ();;

        for (int i = 0; i < FullDanceArray.Count; i++) {
            userDataForDance.Add (new TempUserDataStore (FullDanceArray[i]["username"], FullDanceArray[i]["username"], FullDanceArray[i]["highscore"].AsFloat));
        }

        userDataForDance.Sort ((x, y) => y.score.CompareTo (x.score));

        int UsersToShow = userDataForDance.Count >= 5 ? 5 : userDataForDance.Count;

        Vector2 ScrollContentSize = LeaderboardScrollContent.sizeDelta;
        RectTransform LeaderboardDetailUirect = LeaderboardScrollUIPrefab.GetComponent (typeof (RectTransform)) as RectTransform;

        ScrollContentSize.y = (LeaderboardDetailUirect.sizeDelta.y + 10) * UsersToShow;
        LeaderboardScrollContent.sizeDelta = ScrollContentSize;

        float cHeight = -5;
        for (int i = 0; i < UsersToShow; i++) {
            GameObject GDUI = Instantiate (LeaderboardScrollUIPrefab);
            GDUI.transform.SetParent (LeaderboardScrollContent.transform);
            RectTransform r = GDUI.GetComponent (typeof (RectTransform)) as RectTransform;
            r.localScale = LeaderboardDetailUirect.localScale;
            Vector3 p = new Vector3 (0, cHeight, 0);
            r.anchoredPosition = p;
            cHeight -= (LeaderboardDetailUirect.sizeDelta.y + 10);

            GDUI.GetComponent<LeaderboardUserDetailsContainer> ().SetData (userDataForDance[i].username, userDataForDance[i].score, i + 1);
        }

        LeaderboardUICurrentPlayerScore.text = Usercurrenthighscoreforgame + "%";
        LeaderboardUICurrentPlayerName.text = UsernameValue;
    }

    public void BackFromLeaderboard () {
        CloseAllUI ();
        GameOverUI.SetActive (true);
    }

    IEnumerator PlayVideo (UnityEngine.Video.VideoPlayer VP, string ClipUrl) {
        yield return new WaitForSeconds (5);
        while (!File.Exists (ClipUrl)) {
            yield return new WaitForSeconds (5);
        }
        VP.source = VideoSource.Url;
        Debug.Log (ClipUrl);
        VP.url = ClipUrl;
        VP.Play ();
    }

    void InGamePlaying () {
        Playtime -= Time.deltaTime;
        if (Playtime <= 0) {
            currentGameState = GameStates.Gameover;
        }
    }


    void InGameCheck () {
        //change to false for build and true for testing
        //pluged into kinect: false else true
        if (KinectManager.Instance.IsUserDetected () == true) {
            InGameTextFeedback.text = "Please stand infront of the Kinect.";
            if (StartTimerStore != null) StopCoroutine (StartTimerStore);
            Starting = false;
            TimeToWait = TimeToWaitForGameToStart + ReadySetGoTiming;
            return;
        } else if (Starting == false) {
            InGameTextFeedback.text = "";
            StartTimerStore = StartCoroutine (WaitToStart ());
            Starting = true;
        }

        if (Starting == true) {
            TimeToWait -= Time.deltaTime;
            string TextToShow ="";
            if (TimeToWait > ReadySetGoTiming) {
                TextToShow = "Starting in " + Mathf.RoundToInt (TimeToWait - ReadySetGoTiming);
            }
            else if(TimeToWait < 6 && TimeToWait>4)
            {
                TextToShow ="Ready";
            }
            else if(TimeToWait < 4 && TimeToWait>2)
            {
                TextToShow ="Set";
            }
            else if(TimeToWait < 2 && TimeToWait>0)
            {
                TextToShow ="Go";
            }

            InGameTextFeedback.text = TextToShow;

        }
    }

    IEnumerator WaitToStart () {
        yield return new WaitForSeconds (TimeToWaitForGameToStart + ReadySetGoTiming);
        InGameTextFeedback.text = "";
        Playtime = MCBC.RunAnimation (DanceClipsChoosen);
        VideoCaptureCtrl.instance.captureTime = Playtime;
        VideoCaptureCtrl.instance.StartCapture ();
        BodyManager.instance.StartChecking ();
        currentGameState = GameStates.Playing;
    }

    IEnumerator WaitForTutorialToEnd () {
        CloseAllUI ();
        TutorialUI.SetActive (true);
        TutorialDanceNameText.text = CurrentDanceName;
        UnityEngine.Video.VideoPlayer videoPlayer = TutorialUI.GetComponentInChildren<UnityEngine.Video.VideoPlayer> ();
        videoPlayer.frame = 0;
        videoPlayer.Play ();
        
        yield return new WaitForSeconds ((float) videoPlayer.clip.length);
        CloseAllUI ();
        InGameUI.SetActive (true);
        currentGameState = GameStates.CharacterDetectionChecking;
    }

    
    public void SkipTutorialVDO()
    {
        StopCoroutine(stopTutorial);
        CloseAllUI();
        InGameUI.SetActive(true);
        currentGameState = GameStates.CharacterDetectionChecking;
    }

    public void GenerateDanceList (RectTransform ScrollContent, string DataTitle) {
        TextAsset dataAsJson = (TextAsset) Resources.Load ("DanceClips/DanceClipData", typeof (TextAsset));
        var data = JSON.Parse (dataAsJson.text);
        var FullDanceArray = data[DataTitle].AsArray;

        Vector2 ScrollContentSize = ScrollContent.sizeDelta;
        RectTransform DanceDetailsUIrect = DanceDetailsUIPrefab.GetComponent (typeof (RectTransform)) as RectTransform;

        ScrollContentSize.y = (DanceDetailsUIrect.sizeDelta.y + 10) * FullDanceArray.Count;
        ScrollContent.sizeDelta = ScrollContentSize;

        float cHeight = -5;
        for (int i = 0; i < FullDanceArray.Count; i++) {
            GameObject GDUI = Instantiate (DanceDetailsUIPrefab);
            GDUI.transform.SetParent (ScrollContent.transform);
            RectTransform r = GDUI.GetComponent (typeof (RectTransform)) as RectTransform;
            r.localScale = DanceDetailsUIrect.localScale;
            Vector3 p = new Vector3 (0, cHeight, 0);
            r.anchoredPosition = p;
            cHeight -= (DanceDetailsUIrect.sizeDelta.y + 10);

            var FileNamesArray = FullDanceArray[i]["filename"].AsArray;
            string[] FileNames = new string[FileNamesArray.Count];

            for (int y = 0; y < FileNamesArray.Count; y++) {
                FileNames[y] = FileNamesArray[y].Value;
            }

            GDUI.GetComponent<DanceMoveChooseButton> ().SetDataForButton (FullDanceArray[i]["name"].Value, FileNames);
        }
    }

    public void ChooseDance (string DanceName, string[] DanceFileNames) {
        CloseAllUI ();
        AnimationClip[] DanceClips = new AnimationClip[DanceFileNames.Length];
        VideoClip[] videoClips = new VideoClip[DanceFileNames.Length];
        for (int i = 0; i < DanceFileNames.Length; i++) {
            DanceClips[i] = (AnimationClip) Resources.Load ("DanceClips/" + DanceFileNames[i], typeof (AnimationClip));
            videoClips[i] = (VideoClip)Resources.Load("DemoVids/" + DanceFileNames[i], typeof(VideoClip));
        }
        tutorialVDO.clip = videoClips[0];
        DanceClipsChoosen = DanceClips;
        CurrentDanceName = DanceName;
        currentGameState = GameStates.Tutorial;
    }
}

public class TempUserDataStore {
    public string username;
    public string user_id;
    public float score;

    public TempUserDataStore (string username, string user_id, float score) {
        this.username = username;
        this.user_id = user_id;
        this.score = score;
    }
}