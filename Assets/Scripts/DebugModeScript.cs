using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DebugModeScript : MonoBehaviour {
    public Dropdown RequestTarget;

    public InputField WriteJSON;
    public GameObject WriteJSONDialog;

    public Toggle DebugPanelShow_01;

    public Text DebugLog;
    int LogNumLines;

    public void Start() {
        if (PlayerPrefs.GetInt("FamikSetting_RegisterDebugDialog", 0) == 1) DebugPanelShow_01.isOn = true; else DebugPanelShow_01.isOn = false;
    }

    public void ChangeDebugPanelShow_01 () {
        if (DebugPanelShow_01.isOn) PlayerPrefs.SetInt("FamikSetting_RegisterDebugDialog", 1); else PlayerPrefs.SetInt("FamikSetting_RegisterDebugDialog", 0);
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
        AllDataRemove();
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\": \"未設定\", \"OneSicks\": []}], \"FDV\": " + FamikDatas.FamikDataVersion + "}");
        LogOutput("全データリセットを実行し、登録できる状態にしました。");
    }
    public void AllDataRemove()
    {
        PlayerPrefs.DeleteAll();
        LogOutput("全データを削除しました。");
    }
    public void InsertTestData()
    {
        PlayerPrefs.SetString("Famik", "{\"Humans\":[{\"Name\":\"A\",\"OneSicks\":[]},{\"Name\":\"B\",\"OneSicks\":[]},{\"Name\":\"C\",\"OneSicks\":[]}], \"FDV\": " + FamikDatas.FamikDataVersion + "}");
        LogOutput("テストデータを挿入しました。");
    }

    public void LogOutput(string Log)
    {/*
        LogNumLines++;
        if (LogNumLines > 10) {
            DebugLog.text = "";
            LogNumLines = 0;
            LogOutput("ログを自動クリアしました。");
        }*/
        Debug.Log(Log);
        DebugLog.text += Log + "\n";
    }

    public void LoadSaveJSON()
    {
        if (RequestTarget.value == 0) {
            LogOutput(ToReadable(PlayerPrefs.GetString("Famik", "{\"通知\":\"データがありません\"}")));
        } else if (RequestTarget.value == 1) {
            LogOutput(ToReadable(PlayerPrefs.GetString("Stars", "{\"通知\":\"データがありません\"}")));
        }
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
