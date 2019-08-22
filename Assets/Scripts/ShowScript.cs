using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UI.Tables;

public class ShowScript : MonoBehaviour {
    [Header("人選択プルダウン")]
    public Dropdown HumanSelect;
    public List<string> HumanSelectOptions = new List<string> { };

    [Header("ダイアログ")]
    public Text DialogObject_Text;
    public GameObject DialogObject;
    public SimpleHealthBar DialogObject_Time;

    [Header("画像＆テキストダイアログ")]
    public Text OtherText;
    public RawImage ImageDialog;
    public GameObject ImageAndTextDialog;
    public List<int> SeletedNum;
    public RawImage MarkImage;
    public Texture CheckMark;
    public Texture XMark;

    [Header("症状表")]
    public TableLayout SickTable;
    public GameObject background;
    public GameObject text;
    public GameObject button;

    [Header("ツールチップ")]
    public RectTransform tooltip;
    public Text tooltipText;
    private int overRow = -1;
    private int overColumn = -1;

    SaveData inStorageData;

    public Button ScreenShotButton;

    public GameObject SickTableObject;

    TableRow row;
    TableCell cell;

    public void Start()
    {
        {
          print(!PlayerPrefs.HasKey("Famik"));
        if (!PlayerPrefs.HasKey("Famik") || (JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "{\"Humans\":[]}"))).Humans.Length == 0) {
            StartCoroutine(PleaseRegisterData(false));
        } else if (JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).FDV < FamikDatas.FamikDataVersion) {
            StartCoroutine(OldDataVersion());
        } else {
            if (SickTable != null) Destroy(SickTable.gameObject);
            GetComponent<GraphScript>().Start();

            HumanSelect.ClearOptions();

            SickTable = Instantiate(SickTableObject, GameObject.Find("Graphs").transform).GetComponent<TableLayout>();
            int g = 0;
            int o = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik")).Humans.Length;
            for (int i = 0; i < o; i++)
            {
                if (JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).Humans[i].OneSicks.Length == 0) g++;
            }
            if (o == g) //全部なかったら(データない人が全員の数と一致したら)
            {
                StartCoroutine(PleaseRegisterData(true));
            } else {
                // ストレージ読み込み
                inStorageData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik"));

                // プルダウンメニュー追加
                HumanSelectOptions = new List<string> { };
                for (int i = 0; i < inStorageData.Humans.Length; i++) HumanSelectOptions.Add(inStorageData.Humans[i].Name);
                HumanSelect.AddOptions(HumanSelectOptions);


                TableShow();
            }
        }
        }
    }
    public void TableShow()
    {
        // カラム追加
                for (int i = 0; i < SickTable.Rows.Count; i++)
                {
                    Destroy(SickTable.Rows[i].gameObject);

                }

                row = SickTable.AddRow();
                for (int i = 0; i < inStorageData.Humans[HumanSelect.value].OneSicks.Length; i++) {
                    cell = row.AddCell();
                    Instantiate(text, cell.gameObject.transform);
                    string minutstemp;
                    if (inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Minute < 10) {
                        minutstemp = "0" + inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Minute.ToString();

                    }
                    else
                    {
                        minutstemp = inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Minute.ToString();

                    }
                    cell.GetComponentInChildren<Text>().text = inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Month + "/" + inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Day + " " + inStorageData.Humans[HumanSelect.value].OneSicks[i].Time.Hour + ":" + minutstemp;
                }
                /*
                 SickTable サイズ自動調整プログラム

                 float y = SickTable.GetComponent<RectTransform>().sizeDelta.y;
                 SickTable.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * SickTable.Rows[0].Cells.Count, y);
                */

                for (int i = 0; i < 8; i++) {
                    row = SickTable.AddRow();
                    for (int s = 0; s < inStorageData.Humans[HumanSelect.value].OneSicks.Length; s++) {
                        cell = row.AddCell();

                        UnityEngine.Object a = Instantiate(background, cell.gameObject.transform);
                        int r = 0;
                        switch (i)
                        {
                            case 0:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.Sneeze) {
                                    cell.GetComponentInChildren<RawImage>().color = new Color(0.9f, 0.5f, 0.3f);
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "咳";
                                } else {
                                    r++;
                                }
                                break;
                            case 1:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.Dripping) {
                                    cell.GetComponentInChildren<RawImage>().color = new Color(0.5f, 0.5f, 0.9f);
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "鼻水";
                                } else {
                                    r++;
                                }
                                break;
                            case 2:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.Headache) {
                                    cell.GetComponentInChildren<RawImage>().color = Color.green;
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "頭痛";
                                } else {
                                    r++;
                                }
                                break;
                            case 3:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.SoreThroat) {
                                    cell.GetComponentInChildren<RawImage>().color = Color.yellow;
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "喉痛";
                                } else {
                                    r++;
                                }
                                break;
                            case 4:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.StomachAche) {
                                    cell.GetComponentInChildren<RawImage>().color = Color.magenta;
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "腹痛";
                                } else {
                                    r++;
                                }
                                break;
                            case 5:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.Dizzy) {
                                    cell.GetComponentInChildren<RawImage>().color = Color.gray;
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "めまい";
                                } else {
                                    r++;
                                }
                                break;
                            case 6:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.NoAppetite) {
                                    cell.GetComponentInChildren<RawImage>().color = new Color(1f, 0.9f, 0.1f);
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "食欲なし";
                                } else {
                                    r++;
                                }
                                break;
                            case 7:
                                if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Symptoms.Rash) {
                                    cell.GetComponentInChildren<RawImage>().color = new Color(0.6f, 0.5f, 0.3f);
                                    cell.GetComponentInChildren<RawImage>().GetComponentInChildren<Text>().text = "発疹";
                                } else {
                                    r++;
                                }
                                break;
                        }
                        if (cell.GetComponentInChildren<RawImage>().color == Color.white) Destroy(a);
                    }
                }

                GameObject temp;

                row = SickTable.AddRow();
                for (int s = 0; s < inStorageData.Humans[HumanSelect.value].OneSicks.Length; s++)
                {
                    cell = row.AddCell();
                    temp = Instantiate(button, cell.gameObject.transform);
                    if (inStorageData.Humans[HumanSelect.value].OneSicks[s].Image == "NOTHING") {
                        Destroy(temp);
                    } else {
                        int ss = s + 0;
                        temp.GetComponentInChildren<Text>().text = "画像";
                        temp.GetComponent<Button>().onClick.AddListener(() => {
                            OpenDialog(true, ss);
                        });
                    }
                }

                row = SickTable.AddRow();
                for (int r = 0; r < inStorageData.Humans[HumanSelect.value].OneSicks.Length; r++)
                {
                    cell = row.AddCell();
                    temp = Instantiate(button, cell.gameObject.transform);
                    if (inStorageData.Humans[HumanSelect.value].OneSicks[r].Other == string.Empty) {
                        Destroy(temp);
                    } else {
                        int rr = r + 0;
                        temp.GetComponentInChildren<Text>().text = "その他";
                        temp.GetComponent<Button>().onClick.AddListener(() => {
                            OpenDialog(false, rr);
                        });
                    }

                }

    }

    IEnumerator PleaseRegisterData(bool isRegister)
    {
        if (isRegister) {
            MarkImage.texture = XMark;
            DialogObject_Text.text = "データを登録してください。\n\nタイトル画面に戻ります。";
        } else {
            MarkImage.texture = XMark;
            DialogObject_Text.text = "ユーザー登録画面でユーザーを追加してください。\n\nタイトル画面に戻ります。";
        }
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
    }
    public void OnOverDelegate(int row, int column)
    {
        overRow = row;
        overColumn = column;
    }
    public void Share()
    {
        StartCoroutine(ShareButton());
    }
    public IEnumerator ShareButton()
    {
        ScreenCapture.CaptureScreenshot("image.png");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1);
        SocialConnector.SocialConnector.Share ("Famik " + inStorageData.Humans[HumanSelect.value].Name + "のデータ", "", Application.persistentDataPath + "/image.png");
        print("aaaa");
    }
    IEnumerator OldDataVersion()
    {
        MarkImage.texture = XMark;
        DialogObject_Text.text = "Famikデータ形式が古いため、\n読み込めませんでした。\nアプリバージョン: " + FamikDatas.FamikDataVersion + "\nデータバージョン: " + JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).FDV;
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

    }

    public void OpenDialog(bool PictureDialog, int i)
    {
        ImageAndTextDialog.SetActive(true);
        if (PictureDialog)
        {
            OtherText.gameObject.SetActive(false);
            /*
            var texture = new Texture2D(1, 1);
            byte[] bytes = Convert.FromBase64String(inStorageData.Humans[HumanSelect.value].OneSicks[i].Image);
            texture.LoadImage(bytes);
            */
            string FileName = inStorageData.Humans[HumanSelect.value].OneSicks[i].Image + ".famikimage";
            byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/" + FileName);


            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            ImageDialog.texture = texture;
            #if UNITY_ANDROID
            ImageDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            #elif UNITY_IOS
            ImageDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            #endif
        }
        else
        {
            OtherText.gameObject.SetActive(true);
            OtherText.text = inStorageData.Humans[HumanSelect.value].OneSicks[i].Other;
            ImageDialog.texture = Texture2D.whiteTexture;
        }

    }
    public void CloseDialog()
    {
        ImageAndTextDialog.SetActive(false);

    }
}
