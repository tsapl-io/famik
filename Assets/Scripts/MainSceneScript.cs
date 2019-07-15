using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OpenLink(string url)
    {
        #if UNITY_EDITOR
          Application.OpenURL(url);
        #elif UNITY_WEBGL
          Debug.LogError("WebGLには対応していません。");
        #else
          Application.OpenURL(url);
        #endif
    }
}
