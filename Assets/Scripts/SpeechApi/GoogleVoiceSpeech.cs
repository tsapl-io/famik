﻿//	Copyright (c) 2016 steele of lowkeysoft.com
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


	struct ClipData
	{
			public int samples;
	}
	const int HEADER_SIZE = 44;
	private int minFreq;
	private int maxFreq;
	private AudioSource goAudioSource;
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

    public void RecordButtonPush()
    {
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
            yield return new WaitForSeconds(1);

            float filenameRand = UnityEngine.Random.Range (0.0f, 10.0f);
            string filename = "testing" + filenameRand;
            Microphone.End(null);
            if (!filename.ToLower().EndsWith(".wav")) filename += ".wav";
            var filePath = Path.Combine("speechtemp/", filename);
            filePath = Path.Combine(Application.persistentDataPath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            SavWav.Save (filePath, goAudioSource.clip);
            string Response;
            Response = HttpUploadFile ("https://speech.googleapis.com/v1/speech:recognize?&key=XXXXXXXXXXXXXXXXXXXX", filePath, "file", "audio/wav; rate=44100");
            var jsonresponse = SimpleJSON.JSON.Parse(Response);
            print(jsonresponse);
            File.Delete(filePath);
            RecordButton_Text.text = "録音する";

            bool ErrorFlag = false;

            try {
                FeverSpeechResult = float.Parse(jsonresponse["results"][0]["alternatives"][0]["transcript"].ToString().Trim(new char[]{'"'}));
            } catch (FormatException) {
                DialogObject_Text.text = "体温ではない文字が認識されました。\n\nもう一度やり直してください。";
                ErrorFlag = true;
            } catch (Exception) {
                DialogObject_Text.text = "大変申し訳ございません。\n\nもう一度やり直してください。";
                ErrorFlag = true;
            }
            if (!ErrorFlag) {
                if (FeverSpeechResult <= 35) {
                    DialogObject_Text.text = "異常に体温が低すぎます。\n\nもう一度やり直してください。";
                    ErrorFlag = true;
                } else if (FeverSpeechResult >= 41) {
                    DialogObject_Text.text = "異常に体温が高すぎます。\n例:「39.0」\nもう一度やり直してください。";
                    ErrorFlag = true;
                }
            }

            if (ErrorFlag) {
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
            }

            RecordButton.interactable = true;
            TimeLimit.UpdateBar(5, 5);

            if (FeverSpeechResult != 0) ResultText.text = FeverSpeechResult.ToString();
        }
    }

    public string HttpUploadFile(string url, string file, string paramName, string contentType) {

        ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        Byte[] bytes = File.ReadAllBytes(file);
        String file64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        try {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{ \"config\": {\"encoding\":\"LINEAR16\", \"languageCode\": \"ja-JP\", \"sampleRateHertz\": 44100, \"enableWordTimeOffsets\": false }, \"audio\" : { \"content\" : \"" + file64 + "\"}}";
                Debug.Log(file64);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        
        } catch (WebException ex) {
            var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            Debug.Log(resp);
        }
        return "empty";
	}

    public void InputFieldTextAttach()
    {
        if (!string.IsNullOrEmpty(ResultText.text)) {
            FeverSpeechResult = float.Parse(ResultText.text);
        } else {
            FeverSpeechResult = 0f;
        }
    }
}
		
