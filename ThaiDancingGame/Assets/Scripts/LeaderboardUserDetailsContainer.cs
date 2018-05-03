using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUserDetailsContainer : MonoBehaviour {

    public Text UserNameText;
    public Text HighscorePercText;
    public Sprite GoldImage;

    public void SetData (string UserNameText, float HighscorePercText, int index = -1, bool gold = false) {
        if (index <= -1) {
            this.UserNameText.text = UserNameText;
        } else {
            this.UserNameText.text = index + ". " + UserNameText;
        }

        this.HighscorePercText.text = HighscorePercText + "%";
        if (gold) {
            Image i = GetComponent<Image> ();
            i.sprite = GoldImage;
            i.color = new Color (218 / 255, 165 / 255, 32 / 255);

        }
    }
}