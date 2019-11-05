using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reqlient;

[Serializable]
public class GCP_Return {
    public string audioContent;
}

public class MainSceneScript : MonoBehaviour {
    public UnityEngine.UI.Button SearchButton;
    public GameObject ReloadPanel;

    public AudioSource audioSource;

    [SerializeField]
    int DebugClickCount;

    [SerializeField]
    int DebugClickCount_Title;
    [SerializeField]
    int DebugClickCount_Credit;

    Coroutine DebugResetCoroutine;

	// Use this for initialization
	IEnumerator Start () {
		  if (Application.internetReachability == NetworkReachability.NotReachable) {
          SearchButton.interactable = false;
          SearchButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "インターネット接続なし";
      }
      if (PlayerPrefs.GetString("Famik", "Not Found") == "Not Found" && !Directory.Exists(Application.persistentDataPath + "/YotsubaChanTalkAudio")) {
          // 初回起動処理
          ReloadPanel.SetActive(true);

          yield return new WaitForSeconds(1f);

          Directory.CreateDirectory(Application.persistentDataPath + "/YotsubaChanTalkAudio");

          string reqBody;
          string result;
          for (int i = 0; i < FamikDatas.VoiceList.Length; i++) {
              reqBody = "{'audioConfig': {'pitch': 0,'speakingRate': 1,'audioEncoding': 'LINEAR16'},'input': {'text': '" + FamikDatas.VoiceList[i] + "'},'voice': {'languageCode': 'ja-JP','name': 'ja-JP-Wavenet-B'}}";
              result = Reqlient.HttpRequest.Request("https://texttospeech.googleapis.com/v1/text:synthesize?key=XXXXXXXXXXXXXXXXXXXX", reqBody);
              byte[] sound = Convert.FromBase64String(JsonUtility.FromJson<GCP_Return>(result).audioContent);

              File.WriteAllBytes(Application.persistentDataPath + "/YotsubaChanTalkAudio/" + i + ".wav", sound);
          }

          yield return new WaitForEndOfFrame();

          ReloadPanel.SetActive(false);
      }
  }

	// Update is called once per frame
	void Update () {

	}

  public void DebugClick_Title() {
      DebugClickCount_Title++;
      if (DebugClickCount_Title > 10 && DebugClickCount_Credit > 10) {
          PlayerPrefs.SetInt("DebugMode_isActive", 1);
      }
  }
  public void DebugClick_Credit() {
      DebugClickCount_Credit++;
      if (DebugClickCount_Title > 10 && DebugClickCount_Credit > 10) {
          PlayerPrefs.SetInt("DebugMode_isActive", 1);
      }
  }

    public void DebugClick()
    {
        if (PlayerPrefs.GetInt("DebugMode_isActive", 0) != 0) {
            DebugClickCount++;
            DebugResetCoroutine = StartCoroutine(WaitForResetCount());
            if (DebugClickCount == 5)
            {
                StopCoroutine(DebugResetCoroutine);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Debug");
                DebugClickCount = 0;
            }
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
