using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class m_setButtonPosition : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        float w = this.GetComponent<RectTransform>().rect.width;
        float h = this.GetComponent<RectTransform>().rect.height;
        float x = this.transform.position.x;
        if (this.gameObject.name == "LeaderboardButton")
        {
            
            this.transform.position = new Vector3( x , Screen.height - (Screen.height/27) - (h/2), 0);
        }
        else if(this.gameObject.name == "BacktoMenu")
        {
            this.transform.position = new Vector3( x , Screen.height - (Screen.height / 27) - (h / 2), 0);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
