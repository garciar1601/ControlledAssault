using UnityEngine;
using System.Collections;

public class QAManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Application.LoadLevel("Game");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Application.LoadLevel("Demo");
        }
	}
}
