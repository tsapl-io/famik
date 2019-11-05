using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using System.Globalization;

[Serializable]
public class HumanData {
    public string Name;
    public Sick[] OneSicks;
}
// \n
[Serializable]
public class Sick {
    public Times Time;
    public Symptoms Symptoms;
    public string Other;
    public string Fever;
    public string Image;
}
[Serializable]
public class Times {
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;
}
[Serializable]
public class Symptoms {
    public bool Sneeze;
    public bool Dripping;
    public bool Headache;
    public bool SoreThroat;
    public bool StomachAche;
    public bool Dizzy;
    public bool NoAppetite;
    public bool Rash;
}


[Serializable]
public class SaveData {
    public HumanData[] Humans;
    public int FDV; // Famik Data Version
}

public static class FamikDatas
{
    public static int FamikDataVersion { get { return 5; } }
    public static void VersionConvert_to_5 (int FromVersion) {
        if (FromVersion == 4) {
            var dirs = Directory.GetFiles(Application.persistentDataPath);
            foreach (var d in dirs) {
              Debug.Log(d);
                if (d.IndexOf("Famik_Image_") > -1) {
                    File.Move(d, Application.persistentDataPath + "/ImageDatas/" + System.IO.Path.GetFileName(d));
                }
            }
            var sv = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA"));
            sv.FDV = 5;
            PlayerPrefs.SetString("Famik", JsonUtility.ToJson(sv));
            PlayerPrefs.Save();
        } else {
            Debug.LogError("コンバートエラー：対応していないデータバージョン");
        }
    }
    public static string[] VoiceList = {"データ登録しました。", "マスクの着用をおすすめします。", "咳が続くようならば病院の受診をおすすめします。", "手洗いうがいをこまめにすることをおすすめします。", "鼻をかんだら、水分の補給をお忘れなく。", "頭が痛いのは辛いですね。早めに休んでくださいね。", "のどを乾燥させないように気をつけてくださいね。", "のどの乾燥を防ぐのに、マスクやのど飴、うがいは有効ですよ！", "お腹が痛くなった状況や経過を、そのた欄に記録しておくことをおすすめします。", "ふらふらしたら早めに休んでくださいね。", "めまいが続いたら病院の受診をおすすめします。", "食欲がなくても水分の補給はお忘れなく！", "湿疹を写真に撮ってお医者さんに見せると、説明が楽ですよ！", "湿疹が広がっているようならば、病院の受診をおすすめします。", "水分を十分にとって、早めにお休みください。", "熱が続くようなら、病院の受診をおすすめします。"};
}

/*

SaveData                     [セーブデータをまとめるクラス]
    Humans                   [一人の人間記録]
        Name                 [人の名前]
        OneSicks             [一つの症状記録]
            Symptoms         [症状を入れる配列]
                Sneeze       [咳]
                Dripping     [鼻水]
                Headache     [頭痛]
                SoreThroat   [喉が痛い]
                StomachAche  [腹痛]
                Dizzy        [めまい]
                NoAppetite   [食欲なし]
                Rash         [発疹]
            Time             [時間を入れるところ]
                Month        [月]
                Day          [日]
                TimeZone     [時間帯]
            Fever            [熱]
            Other            [その他記入欄]
            Image            [画像Base64]

SaveData.Humans.OneSicks.Time.TimeZoneの数値について
    0: 夜中 (0:00~6:00)
    1: 朝   (6:01~12:00)
    2: 昼   (12:01~)
    3: 夜

*/


public class RegisterScript : MonoBehaviour {

  public GameObject DebugDialog;

    [Header("人選択プルダウン")]
    public Dropdown HumanSelect;
    public List<string> HumanSelectOptions = new List<string> {};

    [Header("外部取得")]
    public GameObject GoogleVoiceSpeech;
    public GameObject PictureScript;

    [Header("症状選択チェックボックス")]
    public Toggle Sneeze;
    public Toggle Dripping;
    public Toggle Headache;
    public Toggle SoreThroat;
    public Toggle StomachAche;
    public Toggle Dizzy;
    public Toggle NoAppetite;
    public Toggle Rash;

    [Header("その他入力欄")]
    public InputField Other;

    [Header("ダイアログ")]
    public GameObject DialogObject;
    public Text DialogObject_Text;
    public SimpleHealthBar DialogObject_Time;
    public RawImage MarkImage;
    public Texture CheckMark;
    public Texture XMark;

    [Header("よつばちゃんダイアログ")]
    public GameObject YotsubaDialog;
    public Text YotsubaDialog_Text;
    public AudioSource audioSource;

    //List<bool> symptoms = {Sneeze.isOn, Dripping.isOn, Headache.isOn, SoreThroat.isOn, StomachAche.isOn, Dizzy.isOn, NoAppetite.isOn, Rash.isOn};
    //List<int> already_number = new List<int>();

    IEnumerator Start () {
        if (PlayerPrefs.GetInt("FamikSetting_RegisterDebugDialog", 0) == 1) DebugDialog.SetActive(true); else DebugDialog.SetActive(false);
            print("マイクへのアクセス権限: " + Application.HasUserAuthorization(UserAuthorization.Microphone));
        if (PlayerPrefs.GetString("Famik", "NO DATA") == "NO DATA" || JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).Humans.Length == 0) {
            MarkImage.texture = XMark;
            DialogObject_Text.text = "ユーザー登録画面でユーザーを追加してください。\n\nタイトル画面に戻ります。";
            if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
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
            SceneManager.LoadScene("Main");
        } else {
            SaveData LoadedData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA"));
            for (int i = 0; i < LoadedData.Humans.Length; i++) {
                HumanSelectOptions.Add(LoadedData.Humans[i].Name);
            }
            HumanSelect.AddOptions(HumanSelectOptions);
            /*
            CultureInfo myCI = new CultureInfo("ja-JP");
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            DateTime LastDay = new System.DateTime(DateTime.Now.Year, 12, 31);
            "https://www.niid.go.jp/niid/images/idwr/sokuho/idwr-" + DateTime.Now.Year + "/" + DateTime.Now.Year + myCal.GetWeekOfYear(LastDay, myCWR, myFirstDOW) + "/" + DateTime.Now.Year + "-" + myCal.GetWeekOfYear(LastDay, myCWR, myFirstDOW) + "zensu.csv"
            */
        }
    }

	// Update is called once per frame
	void Update () {

	}

    public void Save()
    {
        try {
            DialogObject_Text.text = "保存中です。\n\n一切操作をしないでください。";
            DialogObject.SetActive(true);
            DialogObject_Time.UpdateBar(0, 5);



            if (Encoding.GetEncoding("UTF-8").GetByteCount(PlayerPrefs.GetString("Famik", " ")) > 10485760) {
                MarkImage.texture = XMark;
                if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
                StartCoroutine(CompleteBack("Famikのセーブデータサイズが大きいため、保存ができません。"));
            }

            if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult == 0 && !Sneeze.isOn && !Dripping.isOn && !Headache.isOn && !SoreThroat.isOn && !StomachAche.isOn && !Dizzy.isOn && !NoAppetite.isOn && !Rash.isOn && (Other.text == null || Other.text.Trim() == "") && !GetComponent<PictureScript>().isPictureSaved) {
                StartCoroutine(CompleteBack(true));
            } else {
                if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult < 35.0f && GetComponent<GoogleVoiceSpeech>().FeverSpeechResult != 0) {
                    MarkImage.texture = XMark;
                    if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
                    StartCoroutine(CompleteBack("異常に体温が低すぎます。\nもう一度やり直してください。"));
                } else if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult > 42.0f) {
                    MarkImage.texture = XMark;
                    if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
                    StartCoroutine(CompleteBack("異常に体温が高すぎます。\n例:「39.0」\nもう一度やり直してください。"));
                } else {
                    SaveData inStorageData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA"));
                    if (inStorageData.Humans[HumanSelect.value].OneSicks.Length == 0) {
                        Array.Resize(ref inStorageData.Humans[HumanSelect.value].OneSicks, 1);
                    } else {
                        Array.Resize(ref inStorageData.Humans[HumanSelect.value].OneSicks, inStorageData.Humans[HumanSelect.value].OneSicks.Length + 1);
                    }
                    int SequenceNumberToBeAddedThisTime = inStorageData.Humans[HumanSelect.value].OneSicks.Length - 1;
                    DialogObject_Time.UpdateBar(1, 5);
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime] = new Sick();
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Time = new Times();
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Time.Month = DateTime.Now.Month;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Time.Day = DateTime.Now.Day;
                    DialogObject_Time.UpdateBar(2, 5);
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Time.Hour = DateTime.Now.Hour;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Time.Minute = DateTime.Now.Minute;
                    DialogObject_Time.UpdateBar(3, 5);
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Fever = GetComponent<GoogleVoiceSpeech>().FeverSpeechResult.ToString();
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms = new Symptoms();
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.Sneeze = Sneeze.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.Dripping = Dripping.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.Headache = Headache.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.SoreThroat = SoreThroat.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.StomachAche = StomachAche.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.Dizzy = Dizzy.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.NoAppetite = NoAppetite.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Symptoms.Rash = Rash.isOn;
                    inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Other = Other.text;
                    if (GetComponent<PictureScript>().isPictureSaved) {
                        Guid guid = Guid.NewGuid();
                        string uuid = guid.ToString();
                        inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Image = "Famik_Image_" + uuid;
                        string FileName = "Famik_Image_" + uuid + ".famikimage";
                        byte[] Data = GetComponent<PictureScript>().ImageBytes;
                        if (!Directory.Exists(Application.persistentDataPath + "/ImageDatas")) Directory.CreateDirectory(Application.persistentDataPath + "/ImageDatas");
                        File.WriteAllBytes(Application.persistentDataPath + "/ImageDatas/" + FileName, Data);
                    } else {
                        inStorageData.Humans[HumanSelect.value].OneSicks[SequenceNumberToBeAddedThisTime].Image = "NOTHING";
                    }
                    inStorageData.FDV = FamikDatas.FamikDataVersion;
                    DialogObject_Time.UpdateBar(4, 5);
                    PlayerPrefs.SetString("Famik", JsonUtility.ToJson(inStorageData));
                    MarkImage.texture = CheckMark;
                    StartCoroutine(CompleteBack(false));
                }
            }
        } catch (PlayerPrefsException) {
            MarkImage.texture = XMark;
            if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
            StartCoroutine(CompleteBack("端末の容量が少ないため、保存ができませんでした。"));
        }
    }
    public IEnumerator CompleteBack(bool emptyError)
    {
        int temp_yotsuba = PlayerPrefs.GetInt("YotsubaChan", 1);
        if (!emptyError) {
            MarkImage.texture = CheckMark;
            if (temp_yotsuba == 0) {
                DialogObject_Text.text = "データ登録が完了しました。\n\nタイトル画面に戻ります。";
            } else {
                DialogObject.SetActive(false);
                YotsubaDialog.SetActive(true);
                YotsubaDialog_Text.text = "";

                int check_num = 0;
                if (Sneeze.isOn) check_num++;
                if (Dripping.isOn) check_num++;
                if (Headache.isOn) check_num++;
                if (SoreThroat.isOn) check_num++;
                if (StomachAche.isOn) check_num++;
                if (Dizzy.isOn) check_num++;
                if (NoAppetite.isOn) check_num++;
                if (Rash.isOn) check_num++;


                YotsubaDialog_Text.text = "データ登録しました。";
                if (check_num >= 1) {

                    List<int> order = new List<int>();
                    if (Sneeze.isOn) {
                        order.Add(0);
                    }
                    if (Dripping.isOn) {
                        order.Add(1);
                    }
                    if (Headache.isOn) {
                        order.Add(2);
                    }
                    if (SoreThroat.isOn) {
                        order.Add(3);
                    }
                    if (StomachAche.isOn) {
                        order.Add(4);
                    }
                    if (Dizzy.isOn) {
                        order.Add(5);
                    }
                    if (NoAppetite.isOn) {
                        order.Add(6);
                    }
                    if (Rash.isOn) {
                        order.Add(7);
                    }

                    order = order.OrderBy(a => Guid.NewGuid()).ToList();
                    for (int i = 0; i < order.Count; i++) {
                        if (order[i] == 0) {
                            string[] tmp = {"マスクの着用をおすすめします。", "咳が続くようならば病院の受診をおすすめします。"};
                            List<string> sneeze_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + sneeze_sentence[UnityEngine.Random.Range(0, sneeze_sentence.Count)];
                        } else if (order[i] == 1) {
                            string[] tmp = {"手洗いうがいをこまめにすることをおすすめします。", "鼻をかんだら、水分の補給をお忘れなく。"};
                            List<string> dripping_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + dripping_sentence[UnityEngine.Random.Range(0, dripping_sentence.Count)];
                        } else if (order[i] == 2) {
                            string[] tmp = {"頭が痛いのは辛いですね。早めに休んでくださいね。"};
                            List<string> headache_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + headache_sentence[UnityEngine.Random.Range(0, headache_sentence.Count)];
                        } else if (order[i] == 3) {
                            string[] tmp = {"のどを乾燥させないように気をつけてくださいね。", "のどの乾燥を防ぐのに、マスクやのど飴、うがいは有効ですよ！"};
                            List<string> sorethroat_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + sorethroat_sentence[UnityEngine.Random.Range(0, sorethroat_sentence.Count)];
                        } else if (order[i] == 4) {
                            string[] tmp = {"お腹が痛くなった状況や経過を、その他欄に記録しておくことをおすすめします。"};
                            List<string> stomachache_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + stomachache_sentence[UnityEngine.Random.Range(0, stomachache_sentence.Count)];
                        } else if (order[i] == 5) {
                            string[] tmp = {"ふらふらしたら早めに休んでくださいね。", "めまいが続いたら病院の受診をおすすめします。"};
                            List<string> dizzy_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + dizzy_sentence[UnityEngine.Random.Range(0, dizzy_sentence.Count)];
                        } else if (order[i] == 6) {
                            string[] tmp = {"食欲がなくても水分の補給はお忘れなく！"};
                            List<string> noappetite_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + noappetite_sentence[UnityEngine.Random.Range(0, noappetite_sentence.Count)];
                        } else if (order[i] == 7) {
                            string[] tmp = {"湿疹を写真に撮ってお医者さんに見せると、説明が楽ですよ！", "湿疹が広がっているようならば、病院の受診をおすすめします。"};
                            List<string> rash_sentence = new List<string>(tmp);
                            YotsubaDialog_Text.text += "\n\n" + rash_sentence[UnityEngine.Random.Range(0, rash_sentence.Count)];
                        }
                        if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult >= 38.0f && i == 0) break;
                        if (i == 1) break;
                    }
                }
                if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult >= 38.0f) {
                    string[] tmp = {"水分を十分にとって、早めにお休みください。", "熱が続くようなら、病院の受診をおすすめします。"};
                    List<string> fever_sentence = new List<string>(tmp);
                    YotsubaDialog_Text.text += "\n\n" + fever_sentence[UnityEngine.Random.Range(0, fever_sentence.Count)];
                }


                yield return new WaitForSeconds(0.2f);

                for (int i = 0; i < FamikDatas.VoiceList.Length; i++) {
                    if (YotsubaDialog_Text.text.IndexOf(FamikDatas.VoiceList[i]) > -1) {
                        //print("file://" + Application.persistentDataPath + "/YotsubaChanTalkAudio/" + i + ".wav");
                        using (WWW www = new WWW("file://" + Application.persistentDataPath + "/YotsubaChanTalkAudio/" + i + ".wav")) {
                            yield return www;
                            audioSource.clip = www.GetAudioClip(true, true);
                            audioSource.Play();
                            yield return new WaitForSeconds(audioSource.clip.length);
                        }
                    }
                }

                /*
                int limit;
                limit = 0;
                if (Sneeze.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "咳が出るようですね。\nマスクの着用をおすすめします。";
                    limit++;
                } if (Dripping.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "手洗いうがいを心がけてください。";
                    limit++;
                } if (Headache.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "なるべく横になっていた方が良いですね。";
                    limit++;
                } if (SoreThroat.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "あまり大声を出さないでくださいね。";
                    limit++;
                } if (StomachAche.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "消化の良いものを食べてくださいね。";
                    limit++;
                } if (Dizzy.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "なるべく動かないでくださいね。";
                    limit++;
                } if (NoAppetite.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "無理に食べないようにしてくださいね。";
                    limit++;
                } if (Rash.isOn && limit != 2) {
                    YotsubaDialog_Text.text += "発疹が広がるようならば病院に行くことをおすすめします。";
                    limit++;
                } if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult > 38.0f && limit != 2) {
                    YotsubaDialog_Text.text += "水分を十分にとって、早めにお休みください。";
                    limit++;
                }

                if (check_num > 1) {

                    YotsubaDialog_Text.text += "\n" + random_select_text();

                } else if (check_num == 1) {

                }

                if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult > 38.0f) {
                    YotsubaDialog_Text.text = "データ登録しました。\n熱が高いようですね。";
                }
                if (Dizzy.isOn) {
                    YotsubaDialog_Text.text = "データ登録しました。\nめまいがするようですね。";
                }
                if (GetComponent<GoogleVoiceSpeech>().FeverSpeechResult > 38.0f && Dizzy.isOn) {
                    YotsubaDialog_Text.text = "データ登録しました。\nどうぞお大事にしてください。";
                }
                if (Sneeze.isOn) {
                    YotsubaDialog_Text.text = "データ登録しました。\n咳が出るようですね。\nマスクの着用をおすすめします。";
                }
                if (Dripping.isOn) {
                    YotsubaDialog_Text.text = "データ登録しました。\n手洗いうがいを心がけましょう。";
                }
                if (Sneeze.isOn && Dripping.isOn) {
                    YotsubaDialog_Text.text = "データ登録しました。\n手洗いうがいを心がけましょう。\nマスクの着用をおすすめします。";
                }*/
            }
        } else {
            temp_yotsuba = 0;
            MarkImage.texture = XMark;
            DialogObject_Text.text = "データが空です。\n\nデータを入力してください。";
            DialogObject.SetActive(true);
            if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
        }
        if (temp_yotsuba == 0) {
            DialogObject_Time.UpdateBar(3, 3);
            yield return new WaitForSeconds(1);
            DialogObject_Time.UpdateBar(2, 3);
            yield return new WaitForSeconds(1);
            DialogObject_Time.UpdateBar(1, 3);
            yield return new WaitForSeconds(1);
            DialogObject_Time.UpdateBar(0, 3);
            DialogObject.SetActive(false);
            DialogObject_Time.UpdateBar(3, 3);
        } else {
            YotsubaDialog.SetActive(false);
        }
        if (!emptyError) {
            SceneManager.LoadScene("Main");
        } else {
            SceneManager.LoadScene("Register");
        }

    }
    public IEnumerator CompleteBack(string message)
    {
        DialogObject_Text.text = message;
        DialogObject.SetActive(true);
        if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) Handheld.Vibrate();
        DialogObject_Time.UpdateBar(3, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(2, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(1, 3);
        yield return new WaitForSeconds(1);
        DialogObject_Time.UpdateBar(0, 3);
        DialogObject.SetActive(false);
        DialogObject_Time.UpdateBar(0, 3);
        SceneManager.LoadScene("Register");

    }
    /*
    public string random_select_text() {
        int now_number = Math.Floor(UnityEngine.Random.Range(0, 7));

        bool flag = false;
        for (int i = 0; i < already_number.Count; i++) {
            if (now_number == already_number[i]) {
                flag = true;
            }
        }
        if (flag) {
            return random_select_text();
        } else {
            already_number.Add(now_number);
            if (now_number == 0) {
                return "マスクの着用をおすすめします。";
            } else if (now_number == 1) {
                return "手洗いうがいを心がけてください。";
            } else if (now_number == 2) {
                return "なるべく横になっていた方が良いですね。";
            } else if (now_number == 3) {
                return "あまり大声を出さないでくださいね。";
            } else if (now_number == 4) {
                return "消化の良いものを食べてくださいね。";
            } else if (now_number == 5) {
                return "なるべく動かないでくださいね。";
            } else if (now_number == 6) {
                return "無理に食べないようにしてくださいね。";
            } else if (now_number == 7) {
                return "発疹が広がるようならば病院に行くことをおすすめします。";
            }
        }
    }*/

}
