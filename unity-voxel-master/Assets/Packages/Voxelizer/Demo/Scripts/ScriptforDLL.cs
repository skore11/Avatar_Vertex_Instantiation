using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptforDLL : MonoBehaviour {

    public UnityTrialDLL myClassObject;
    public string txt;

	// Use this for initialization
	void Start () {
      //  myClassObject = new UnityTrialDLL();
       myClassObject = gameObject.AddComponent<UnityTrialDLL>();

        txt = myClassObject.GetVersion();
      Debug.Log("this the txt from the DLL: " + txt);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
