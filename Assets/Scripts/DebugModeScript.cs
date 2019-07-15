using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DebugModeScript : MonoBehaviour {
    public InputField WriteJSON;
    public GameObject WriteJSONDialog;

    public InputField socialtext;
    public InputField socialurl;
    public InputField socialimageurl;

    public Text DebugLog;
    int LogNumLines;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void pushsocial()
    {
        File.Delete("image.png");
        ScreenCapture.CaptureScreenshot("image.png");
        StartCoroutine(nume());
    }
    public IEnumerator nume()
    {
        yield return new WaitForEndOfFrame();
        SocialConnector.SocialConnector.Share(socialtext.text, socialurl.text, socialimageurl.text);
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
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\": \"未設定\", \"OneSicks\": []}]}");
        LogOutput("全データリセットを実行し、登録できる状態にしました。");
    }
    public void AllDataRemove()
    {
        PlayerPrefs.DeleteAll();
        LogOutput("全データを削除しました。");
    }
    public void InsertTestData()
    {
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\":\"A\",\"OneSicks\":[]},{\"Name\":\"B\",\"OneSicks\":[]},{\"Name\":\"C\",\"OneSicks\":[]}]}");
        LogOutput("テストデータを挿入しました。");
    }

    public void LogOutput(string Log)
    {
        LogNumLines++;
        if (LogNumLines > 10) {
            DebugLog.text = "";
            LogNumLines = 0;
            LogOutput("ログを自動クリアしました。");
        }
        Debug.Log(Log);
        DebugLog.text += Log + "\n";
    }

    public void LoadSaveJSON()
    {
        if (PlayerPrefs.GetString("Famik", "NO DATA") != "NO DATA") {
            SaveData inStorageData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "{}"));
            for (int i = 0; i < inStorageData.Humans.Length; i++)
            {
                for (int s = 0; s < inStorageData.Humans[i].OneSicks.Length; s++)
                {
                    inStorageData.Humans[i].OneSicks[s].Image = "...";
                }
            }
            LogOutput(ToReadable(JsonUtility.ToJson(inStorageData)));
        } else {
            LogOutput("データがありません");
        }

    }

    public void Write()
    {
        WriteJSONDialog.SetActive(false);
        PlayerPrefs.SetString("Famik", WriteJSON.text);
        LogOutput("書き込みました。");
    }
    public void WriteDialogShow()
    {
        WriteJSONDialog.SetActive(true);
        WriteJSON.text = PlayerPrefs.GetString("Famik");
    }
}
