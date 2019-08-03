using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneScript : MonoBehaviour {
    public UnityEngine.UI.Button SearchButton;

    [SerializeField]
    int DebugClickCount;
    Coroutine DebugResetCoroutine;

	// Use this for initialization
	void Start () {
		if (Application.internetReachability == NetworkReachability.NotReachable) {
            SearchButton.interactable = false;
            SearchButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "インターネット接続なし";
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void DebugClick()
    {
        DebugClickCount++;
        DebugResetCoroutine = StartCoroutine(WaitForResetCount());
        if (DebugClickCount == 5)
        {
            StopCoroutine(DebugResetCoroutine);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Debug");
            DebugClickCount = 0;
        }
    }
    IEnumerator WaitForResetCount()
    {
        yield return new WaitForSeconds(3);
        DebugClickCount = 0;

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
