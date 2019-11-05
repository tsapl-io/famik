using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DebugModeScript : MonoBehaviour {
    public Dropdown RequestTarget;

    public Text FolderPathText;

    public InputField WriteJSON;
    public GameObject WriteJSONDialog;

    public Toggle DebugPanelShow_01;
    public Toggle YotusubaDialogShow_Toggle;
    public Toggle ActiveDebugMode_Toggle;
    public Toggle ActiveErrorVibrate_Toggle;

    public GameObject DebugLogObject;
    public InputField DebugLog;
    int LogNumLines;

    public void Start() {
      if (PlayerPrefs.GetInt("FamikSetting_RegisterDebugDialog", 0) == 1) DebugPanelShow_01.isOn = true; else DebugPanelShow_01.isOn = false;
      if (PlayerPrefs.GetInt("YotsubaChan", 0) == 1) YotusubaDialogShow_Toggle.isOn = true; else YotusubaDialogShow_Toggle.isOn = false;
      if (PlayerPrefs.GetInt("DebugMode_isActive", 0) == 1) ActiveDebugMode_Toggle.isOn = true; else ActiveDebugMode_Toggle.isOn = false;
      FolderPathText.text = "DataPath: " + Application.persistentDataPath + "\nTempPath: " + Application.temporaryCachePath;
    }

    public void ChangeDebugPanelShow_01 () {
        if (DebugPanelShow_01.isOn) PlayerPrefs.SetInt("FamikSetting_RegisterDebugDialog", 1); else PlayerPrefs.SetInt("FamikSetting_RegisterDebugDialog", 0);
    }
    public void Change_YotusubaDialogShow_Toggle () {
        if (YotusubaDialogShow_Toggle.isOn) PlayerPrefs.SetInt("YotsubaChan", 1); else PlayerPrefs.SetInt("YotsubaChan", 0);
    }
    public void Change_ActiveDebugMode_Toggle () {
        if (ActiveDebugMode_Toggle.isOn) PlayerPrefs.SetInt("DebugMode_isActive", 1); else PlayerPrefs.SetInt("DebugMode_isActive", 0);
    }
    public void Change_ActiveErrorVibrate_Toggle () {
        if (ActiveErrorVibrate_Toggle.isOn) PlayerPrefs.SetInt("VibrateCheck", 1); else PlayerPrefs.SetInt("VibrateCheck", 0);
    }

    public static string ToReadable( string json )
    {
        if ( string.IsNullOrEmpty( json ) ) return json;

        int i           = 0;
        int indent      = 0;
        int quoteCount  = 0;
        int position    = -1;
        var sb          = new StringBuilder();
        int lastindex   = 0;

        while ( true )
        {
            if ( i > 0 && json[ i ] == '"' && json[ i - 1 ] != '\\' ) quoteCount++;

            if ( quoteCount % 2 == 0 )
            {
                if ( json[ i ] == '{' || json[ i ] == '[' )
                {
                    indent++;
                    position = 1;
                }
                else if ( json[ i ] == '}' || json[ i ] == ']' )
                {
                    indent--;
                    position = 0;
                }
                else if ( json.Length > i && json[ i ] == ',' && json[ i + 1 ] == '"' )
                {
                    position = 1;
                    sb.Append("\n");
                }
                if ( position >= 0 )
                {
                    sb.AppendLine( json.Substring( lastindex, i + position - lastindex ) );
                    sb.Append( new string( ' ', indent * 4 ) );
                    lastindex = i + position;
                    position = -1;
                }
            }

            i++;

            if ( json.Length <= i )
            {
                sb.Append( json.Substring( lastindex ) );
                break;
            }

        }
        return sb.ToString();
    }

    public void OneHumanAdd() {
        AllDataRemove();
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\": \"未設定\", \"OneSicks\": []}], \"FDV\": " + FamikDatas.FamikDataVersion + "}");
        LogOutput("全データリセットを実行し、登録できる状態にしました。");
    }
    public void AllDataRemove()
    {
      if (RequestTarget.value == 0) {
          PlayerPrefs.DeleteKey("Famik");
      } else if (RequestTarget.value == 1) {
          PlayerPrefs.DeleteKey("Stars");
      }
        LogOutput("指定されたデータを削除しました。");
    }
    public void InsertTestData()
    {
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\":\"A\",\"OneSicks\":[]},{\"Name\":\"B\",\"OneSicks\":[]},{\"Name\":\"C\",\"OneSicks\":[]}], \"FDV\": " + FamikDatas.FamikDataVersion + "}");
        LogOutput("テストデータを挿入しました。");
    }

    public void LogOutput(string Log){
        Debug.Log(Log);
        DebugLog.text = Log + "\n" + DebugLog.text;
    }

    public void LoadSaveJSON()
    {
        if (RequestTarget.value == 0) {
            LogOutput(PlayerPrefs.GetString("Famik", "データがありません"));
        } else if (RequestTarget.value == 1) {
            LogOutput(PlayerPrefs.GetString("Stars", "データがありません"));
        }
    }

    public void YotsubaChanTalkReget() {
      Directory.CreateDirectory(Application.persistentDataPath + "/YotsubaChanTalkAudio");

      string reqBody;
      string result;
      for (int i = 0; i < FamikDatas.VoiceList.Length; i++) {
          reqBody = "{'audioConfig': {'pitch': 0,'speakingRate': 1,'audioEncoding': 'LINEAR16'},'input': {'text': '" + FamikDatas.VoiceList[i] + "'},'voice': {'languageCode': 'ja-JP','name': 'ja-JP-Wavenet-B'}}";
          result = Reqlient.HttpRequest.Request("https://texttospeech.googleapis.com/v1/text:synthesize?key=XXXXXXXXXXXXXXXXXXXX", reqBody);
          byte[] sound = Convert.FromBase64String(JsonUtility.FromJson<GCP_Return>(result).audioContent);;

          File.WriteAllBytes(Application.persistentDataPath + "/YotsubaChanTalkAudio/" + i + ".wav", sound);
      }
      LogOutput("よつばちゃん音声データ再取得完了");
    }

    public void VersionConvert_4_to_5 () {
        FamikDatas.VersionConvert_to_5(4);
    }

    public void Write()
    {
        WriteJSONDialog.SetActive(false);
        if (RequestTarget.value == 0) {
            PlayerPrefs.SetString("Famik", WriteJSON.text);
        } else if (RequestTarget.value == 1) {
            PlayerPrefs.SetString("Stars", WriteJSON.text);
        }
        LogOutput("書き込みました。");
    }
    public void WriteDialogShow()
    {
        WriteJSONDialog.SetActive(true);
        if (RequestTarget.value == 0) {
            WriteJSON.text = ToReadable(PlayerPrefs.GetString("Famik"));
        } else if (RequestTarget.value == 1) {
            WriteJSON.text = ToReadable(PlayerPrefs.GetString("Stars"));
        }
    }
}
