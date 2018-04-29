using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ButtonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void onClick() {
        Debug.Log("Button Clicked");
    }

}
