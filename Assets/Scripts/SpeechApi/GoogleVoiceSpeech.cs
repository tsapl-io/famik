//	Copyright (c) 2016 steele of lowkeysoft.com
//        http://lowkeysoft.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
// Acquired from https://github.com/steelejay/LowkeySpeech
//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;


[RequireComponent (typeof (AudioSource))]
public class GoogleVoiceSpeech : MonoBehaviour {

    [Header("uGUI関係")]
    public Button RecordButton;
    public Text RecordButton_Text;
    public SimpleHealthBar TimeLimit;
    public InputField ResultText;

    [Header("ダイアログ")]
    public GameObject DialogObject;
    public Text DialogObject_Text;
    public SimpleHealthBar DialogObject_Time;

    [Header("状態を保存する変数")]
    public bool isMicrophoneFound;
    public float FeverSpeechResult;

    [Header("ダイアログ")]
    public RawImage MarkImage;
    public Texture CheckMark;
    public Texture XMark;

    public InputField Register_DebugText;

  	struct ClipData { public int samples; }
  	const int HEADER_SIZE = 44;
  	private int minFreq;
  	private int maxFreq;
  	private AudioSource goAudioSource;

    float SpeechResultConvert (string FromString) {

        // 基本変換 //
        if (FromString.EndsWith("摂氏温度")) FromString.Replace("摂氏温度", "");

        FromString = FromString.Replace("°c ", ".");
        FromString = FromString.Replace("°", ".");
        FromString = FromString.Replace(" D", "");
        FromString = FromString.Replace("V", "");
        FromString = FromString.Replace("部", "");
        FromString = FromString.Replace("と", ".");
        FromString = FromString.Replace("摂氏温度", ".");
        FromString = FromString.Replace("度", ".");
        FromString = FromString.Replace("分", "");
        FromString = FromString.Replace("c ", ".");
        FromString = FromString.Replace("点", ".");

        if (FromString.EndsWith(".")) {
            FromString += "|";
            FromString = FromString.Replace(".|", "");
        }

        // 「v」変換 //
        FromString = FromString.Replace("0v", "0");
        FromString = FromString.Replace("1v", "1");
        FromString = FromString.Replace("2v", "2");
        FromString = FromString.Replace("3v", "3");
        FromString = FromString.Replace("4v", "4");
        FromString = FromString.Replace("5v", "5");
        FromString = FromString.Replace("6v", "6");
        FromString = FromString.Replace("7v", "7");
        FromString = FromString.Replace("8v", "8");
        FromString = FromString.Replace("9v", "9");

        // 「ぜろぶ」変換 //
        FromString = FromString.Replace(" dlove", ".0");
        FromString = FromString.Replace("05", "0");
        FromString = FromString.Replace("全", "0");

        // 「れい」変換 //
        FromString = FromString.Replace("ドル", ".0");

        // 「れいぶ」変換 //
        FromString = FromString.Replace(" A", ".0");
        FromString = FromString.Replace("ドライブ", ".0");
        FromString = FromString.Replace("レイヴ", "0");

        // 「きゅうぶ」変換 //
        FromString = FromString.Replace("キューブ", "9");

        // 「くぶ」変換 //
        FromString = FromString.Replace("毒位", ".9");
        FromString = FromString.Replace(" X", ".9");
        FromString = FromString.Replace(" dcv", ".9");
        FromString = FromString.Replace("株", "9");

        // 漢数字変換 //
        FromString = FromString.Replace("三十", "3");
        FromString = FromString.Replace("四十", "4");
        FromString = FromString.Replace("零", "0");
        FromString = FromString.Replace("一", "1");
        FromString = FromString.Replace("二", "2");
        FromString = FromString.Replace("三", "3");
        FromString = FromString.Replace("四", "4");
        FromString = FromString.Replace("五", "5");
        FromString = FromString.Replace("六", "6");
        FromString = FromString.Replace("七", "7");
        FromString = FromString.Replace("八", "8");
        FromString = FromString.Replace("九", "9");
        FromString = FromString.Replace("１", "1");
        FromString = FromString.Replace("２", "2");
        FromString = FromString.Replace("３", "3");
        FromString = FromString.Replace("４", "4");
        FromString = FromString.Replace("５", "5");
        FromString = FromString.Replace("６", "6");
        FromString = FromString.Replace("７", "7");
        FromString = FromString.Replace("８", "8");
        FromString = FromString.Replace("９", "9");
        FromString = FromString.Replace("０", "0");

        // ひらがな変換 //
        FromString = FromString.Replace("さんじゅう", "3");
        FromString = FromString.Replace("よんじゅう", "4");
        FromString = FromString.Replace("れい", "0");
        FromString = FromString.Replace("いち", "1");
        FromString = FromString.Replace("に", "2");
        FromString = FromString.Replace("さん", "3");
        FromString = FromString.Replace("し", "4");
        FromString = FromString.Replace("ご", "5");
        FromString = FromString.Replace("ろく", "6");
        FromString = FromString.Replace("なな", "7");
        FromString = FromString.Replace("しち", "7");
        FromString = FromString.Replace("はち", "8");
        FromString = FromString.Replace("く", "9");
        FromString = FromString.Replace("きゅう", "9");
        FromString = FromString.Replace("ど", ".");
        FromString = FromString.Replace("ぶ", "");

        // デバッグ文 //
        Register_DebugText.text += "\n変換限度 : " + FromString;

        // 最終チェック (フェーズ1)・エラーリターン //
        if (!Regex.IsMatch(FromString, @"[3-4]\d\.\d$")) {
            return -0.4f;
        }

        // 最終チェック (フェーズ2)・エラーリターン //
        float returnValue;
        try {
            returnValue = float.Parse(FromString);
        } catch (FormatException) {
            return -0.1f;
        }

        if (returnValue < 35) return -0.2f;
        else if (returnValue > 42) return -0.3f;

        // リターン //
        return returnValue;
    }

    void Start () {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            RecordButton.interactable = false;
            RecordButton_Text.text = "インターネット接続なし";
        } else {
            if (Microphone.devices.Length <= 0) {
                Debug.LogError("Speech: マイクが見つかりません");
                isMicrophoneFound = false;
            } else {
                Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
                if(minFreq == 0 && maxFreq == 0) maxFreq = 44100;
                goAudioSource = this.GetComponent<AudioSource>();
                isMicrophoneFound = true;
            }
        }
    }

    public void RecordButtonPush() {
        StartCoroutine(Record());
    }

    public IEnumerator Record() {
        if (!Microphone.IsRecording(null)) {
            RecordButton.interactable = false;
            RecordButton_Text.text = "録音中";
            goAudioSource.clip = Microphone.Start( null, true, 7, 44100);
            TimeLimit.UpdateBar(5, 5);
            yield return new WaitForSeconds(1);
            TimeLimit.UpdateBar(4, 5);
            yield return new WaitForSeconds(1);
            TimeLimit.UpdateBar(3, 5);
            yield return new WaitForSeconds(1);
            TimeLimit.UpdateBar(2, 5);
            yield return new WaitForSeconds(1);
            TimeLimit.UpdateBar(1, 5);
            yield return new WaitForSeconds(1);
            TimeLimit.UpdateBar(0, 5);
            RecordButton_Text.text = "解析中";

            for (int i = 0; i < 5; i++) yield return new WaitForEndOfFrame();

            TimeLimit.UpdateBar(5, 5);
            float filenameRand = UnityEngine.Random.Range (0.0f, 10.0f);
            string filename = "speech" + filenameRand;
            Microphone.End(null);
            if (!filename.ToLower().EndsWith(".wav")) filename += ".wav";
            var filePath = Path.Combine("SpeechAudio/", filename);
            filePath = Path.Combine(Application.temporaryCachePath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            SavWav.Save (filePath, goAudioSource.clip);
            string Response = HttpUploadFile ("https://speech.googleapis.com/v1/speech:recognize?&key=XXXXXXXXXXXXXXXXXXXX", filePath, "file", "audio/wav; rate=44100");
            Register_DebugText.text = "認識結果 : " + SimpleJSON.JSON.Parse(Response)["results"][0]["alternatives"][0]["transcript"];
            Register_DebugText.text += "\n認識精度 : " + SimpleJSON.JSON.Parse(Response)["results"][0]["alternatives"][0]["confidence"].ToString().Trim(new char[]{'"'});
            var jsonresponse = SimpleJSON.JSON.Parse(Response);
            File.Delete(filePath);
            RecordButton_Text.text = "録音する";
            RecordButton.interactable = true;

            if (jsonresponse == "{}") {
                Register_DebugText.text += "\nAPIレスポンスが空です (Response: {})";
                MarkImage.texture = XMark;
                DialogObject_Text.text = "申し訳ございません。\n認識できませんでした。";
                DialogObject.SetActive(true);
                DialogObject_Time.UpdateBar(3, 3);
                yield return new WaitForSeconds(1);
                DialogObject_Time.UpdateBar(2, 3);
                yield return new WaitForSeconds(1);
                DialogObject_Time.UpdateBar(1, 3);
                yield return new WaitForSeconds(1);
                DialogObject_Time.UpdateBar(0, 3);
                DialogObject.SetActive(false);
                DialogObject_Time.UpdateBar(0, 3);
                RecordButton.interactable = true;
                if (FeverSpeechResult != 0) ResultText.text = FeverSpeechResult.ToString();
            } else {
                float ConvertResult = -0.5f;

                if (Regex.IsMatch(jsonresponse["results"][0]["alternatives"][0]["transcript"].ToString().Trim(new char[]{'"'}), @"[3-4]\d\.\d$")) {
                    Register_DebugText.text += "\n正規表現チェックを通過しました。";
                    try {
                        ConvertResult = float.Parse(jsonresponse["results"][0]["alternatives"][0]["transcript"].ToString().Trim(new char[]{'"'}));
                    } catch (FormatException) {
                        Register_DebugText.text += "\n!!!!! float.Parseに失敗しました !!!!!";
                        Register_DebugText.text += "\n!!!!! 正規表現チェックでは発見できなかったパターンがあります !!!!!";
                        Register_DebugText.text += "\n!!!!! パターン : [" + jsonresponse["results"][0]["alternatives"][0]["transcript"].ToString().Trim(new char[]{'"'}) + "] !!!!!";
                    }
                } else {
                    Register_DebugText.text += "\n正規表現チェックを通過しませんでした。変換を実行します。";
                    ConvertResult = SpeechResultConvert(jsonresponse["results"][0]["alternatives"][0]["transcript"].ToString().Trim(new char[]{'"'}));
                    if (ConvertResult < 0f) {
                        Register_DebugText.text += "\n変換エラー番号 : " + ConvertResult.ToString();
                    } else {
                        Register_DebugText.text += "\n変換結果 : " + ConvertResult.ToString();
                    }
                }

                if (ConvertResult >= 0f) {
                    FeverSpeechResult = ConvertResult;
                    ResultText.text = ConvertResult.ToString();
                } else if (ConvertResult == -0.1f) {
                    StartCoroutine(DialogObject_ShowAndHide("体温ではない文字が認識されました。\n\nもう一度やり直してください。"));
                } else if (ConvertResult == -0.2f) {
                    StartCoroutine(DialogObject_ShowAndHide("異常に体温が低すぎます。\n\nもう一度やり直してください。"));
                } else if (ConvertResult == -0.3f) {
                    StartCoroutine(DialogObject_ShowAndHide("異常に体温が高すぎます。\n例:「39.0」\nもう一度やり直してください。"));
                } else if (ConvertResult == -0.4f) {
                    StartCoroutine(DialogObject_ShowAndHide("変換できませんでした。\nもう一度やり直してください。"));
                } else if (ConvertResult == -0.5f) {
                    StartCoroutine(DialogObject_ShowAndHide("コードが実行されませんでした。\nもう一度やり直してください。"));
                } else {
                    StartCoroutine(DialogObject_ShowAndHide("原因不明のエラーが発生しました。\n(ハンドリングしていないエラー)"));
                }
                Register_DebugText.text += "\n全ての処理が完了しました";
            }

        }
    }

    public IEnumerator DialogObject_ShowAndHide(string Message) {
        FeverSpeechResult = 0f;
        MarkImage.texture = XMark;
        ResultText.text = "";
        DialogObject_Text.text = Message;
        DialogObject.SetActive(true);
        DialogObject_Time.UpdateBar(3, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(2, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(1, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(0, 3);
        DialogObject.SetActive(false);
        DialogObject_Time.UpdateBar(0, 3);
        RecordButton.interactable = true;
    }

    public string HttpUploadFile(string url, string file, string paramName, string contentType) {
        ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        Byte[] bytes = File.ReadAllBytes(file);
        String file64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        try {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                string json = "{ \"config\": {\"encoding\":\"LINEAR16\", \"languageCode\": \"ja-JP\", \"sampleRateHertz\": 44100, \"enableWordTimeOffsets\": false }, \"audio\" : { \"content\" : \"" + file64 + "\"}}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                var result = streamReader.ReadToEnd();
                return result;
            }
        } catch (WebException ex) {
            var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
        }
        return "empty";
    }

    public void InputFieldTextAttach() {
        if (!string.IsNullOrEmpty(ResultText.text)) {
            FeverSpeechResult = float.Parse(ResultText.text);
        } else {
            FeverSpeechResult = 0f;
        }
    }
}
