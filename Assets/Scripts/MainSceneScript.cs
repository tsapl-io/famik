using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Reqlient;

[Serializable]
public class GCP_Return {
    public string audioContent;
}

public class MainSceneScript : MonoBehaviour {
    public Button SearchButton;
    public GameObject ReloadPanel;

    public GameObject VersionDialog;
    public Text VersionDialogText;

    public AudioSource audioSource;

    [SerializeField]
    int DebugClickCount;

    [SerializeField]
    int DebugClickCount_Title;
    [SerializeField]
    int DebugClickCount_Clover;

    Coroutine DebugResetCoroutine;

  	// Use this for initialization
  	IEnumerator Start() {
  		  if (Application.internetReachability == NetworkReachability.NotReachable) {
            SearchButton.interactable = false;
            SearchButton.GetComponentInChildren<Text>().text = "インターネット接続なし";
        }
        if (!Directory.Exists(Application.persistentDataPath + "/YotsubaChanTalkAudio")) {
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
    public void DebugClick_Title() {
        DebugClickCount_Title++;
        DebugClickCheck();
    }
    public void DebugClick_Clover() {
        DebugClickCount_Clover++;
        DebugClickCheck();
    }
    public void DebugClickCheck() {
        if (DebugClickCount_Title > 10 && DebugClickCount_Clover > 10) {
            PlayerPrefs.SetInt("DebugMode_isActive", 1);
            VersionDialog.SetActive(true);
            VersionDialogText.text = "デバッグモードがオンになりました";
            DebugClickCount_Title = -10000;
            DebugClickCount_Clover = -10000;
            StartCoroutine(VersionDialogHide());
        }
    }
    public void Click_Credit() {
        VersionDialog.SetActive(true);
        VersionDialogText.text = "Famik " + Application.version;
        StartCoroutine(VersionDialogHide());
    }
    IEnumerator VersionDialogHide() {
        yield return new WaitForSeconds(2f);
        VersionDialog.SetActive(false);
    }
    public void DebugClick() {
        if (PlayerPrefs.GetInt("DebugMode_isActive", 0) != 0) {
            DebugClickCount++;
            DebugResetCoroutine = StartCoroutine(WaitForResetCount());
            if (DebugClickCount == 5)
            {
                StopCoroutine(DebugResetCoroutine);
                SceneManager.LoadScene("Debug");
                DebugClickCount = 0;
            }
        }
    }
    IEnumerator WaitForResetCount() {
        yield return new WaitForSeconds(3);
        DebugClickCount = 0;
    }
    public void OpenLink(string url) {
        #if UNITY_EDITOR
          Application.OpenURL(url);
        #elif UNITY_WEBGL
          Debug.LogError("WebGLには対応していません。");
        #else
          Application.OpenURL(url);
        #endif
    }
}
