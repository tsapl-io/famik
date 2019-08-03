using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarsScript : MonoBehaviour {
    public GameObject SearchResultPanel;

    public Texture unStar;
    public Texture Star;
    public List<Button> StarButtons;
    public List<Button> HPButtons;
    public List<Button> HPButtons2;

    public InputField SearchBox;
    public Button SearchButton;

    public Text NameAndTel;

    public GameObject Dialog;
    public Text DialogText;

    public Text NameAndTelStar;
    public GameObject YahooAPIImage;

    bool isRunning;

    fY_YDF ApiResponse3;
    fY2_YDF ApiResponse2;
    fY_YDF ApiResponse;
    StarJson starJson;
    StarJson starJson2;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }
        for (int i = 0; i < HPButtons.Count; i++) { HPButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < HPButtons2.Count; i++) { HPButtons2[i].gameObject.SetActive(false); }
        StartCoroutine(Search_Stars());
    }

    // Update is called once per frame
    void Update () {

    }

    IEnumerator Search_Stars()
    {
        if( isRunning ) { yield break; }
        isRunning = true;

        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }
        for (int i = 0; i < HPButtons.Count; i++) { HPButtons[i].gameObject.SetActive(false); }

        NameAndTelStar.text = "";
        starJson2 = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

        print(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

        if (starJson2.Stars.Length == 0)
        {
            NameAndTelStar.text = "登録しているかかりつけ医院なし";
            isRunning = false;
            yield break;
        }

        string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_StarsYOLP + "&output=json&results=10&detail=full";
        yolp_url += "&uid=";
        for (int i = 0; i < starJson2.Stars.Length; i++) {
            yolp_url += starJson2.Stars[i];
            if (i != starJson2.Stars.Length - 1) yolp_url += ",";
        }

        print(yolp_url);

        using (WWW www = new WWW(yolp_url))
        {
            yield return www;
            print("API リターン\n" + www.text);
            ApiResponse2 = JsonUtility.FromJson<fY2_YDF>(www.text);
            if (ApiResponse2.ResultInfo.Count != 0) {
                ApiResponse3 = JsonUtility.FromJson<fY_YDF>(www.text);
                for (int i = 0; i < ApiResponse3.Feature.Length; i++) {
                    int temp = i + 1;
                    string temp2 = ApiResponse3.Feature[i].Name;
                    if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                    NameAndTelStar.text += temp + ": " + temp2 + "\n";
                    int s = 0;
                    for (s = 0; s < starJson2.Stars.Length; s++) {
                        if (starJson2.Stars[s] == ApiResponse3.Feature[i].Property.Uid) {
                            StarButtons[i].GetComponent<RawImage>().texture = Star;
                        }
                    }
                }
                for (int tmp = 0; tmp < ApiResponse3.Feature.Length; tmp++) {
                    if (tmp < ApiResponse3.Feature.Length) {
                        HPButtons[tmp].gameObject.SetActive(true);
                        StarButtons[tmp].gameObject.SetActive(true);
                    }
                }
            }
        }
        print(ApiResponse3.Feature.Length);
        print(55 * ApiResponse3.Feature.Length);
        print(0 - 55 * ApiResponse3.Feature.Length);
        YahooAPIImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(YahooAPIImage.GetComponent<RectTransform>().anchoredPosition.x, 0 - (100 + (55 * ApiResponse3.Feature.Length)) + 30);
        isRunning = false;
    }

    public void DialogManage(string mes)
    {
        DialogText.text = mes;
        Dialog.SetActive(true);
    }
    public void DialogManage()
    {
        Dialog.SetActive(false);

    }
    public void OpenURL_Stars(int number)
    {
        if (ApiResponse3.Feature.Length > number) {
            Application.OpenURL(ApiResponse3.Feature[number].Property.Detail.YUrl);
        }
    }

    public void PushStarButton(int number)
    {
        starJson = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));
        if (StarButtons[number].GetComponent<RawImage>().texture == unStar)
        {
            //for (int i = 0; i < starJson.Stars.Length; i++) print(starJson.Stars[i]);
            if (starJson.Stars.Length <= 5) {
                print(starJson.Stars.Length);
                if (starJson.Stars.Length == 0) Array.Resize(ref starJson.Stars, 1); else Array.Resize(ref starJson.Stars, starJson.Stars.Length + 1);
                print(ApiResponse3.Feature.Length + "  " + number);
                print(string.Format("starJson.Stars({0})[{1}] = {2}", starJson.Stars.Length, starJson.Stars.Length - 1, ApiResponse3.Feature[number].Property.Uid));
                starJson.Stars[starJson.Stars.Length - 1] = ApiResponse3.Feature[number].Property.Uid;
                StarButtons[number].GetComponent<RawImage>().texture = Star;
                print(JsonUtility.ToJson(starJson));
                PlayerPrefs.SetString("Stars", JsonUtility.ToJson(starJson));
            }
        }
        else
        {
          /*
          消したいのは3版（面で4）
          [0][1][2][3][4][5][6]
          i = 3
          4かいforぶん
          8 != 6
          i + s ==      starJson.Stars.Length = 7;
          */
            for (int i = 0; i < starJson.Stars.Length; i++) {
                if (starJson.Stars[i] == ApiResponse3.Feature[number].Property.Uid) {
                    for (int s = 0; s < starJson.Stars.Length - i; s++) {
                      print(starJson.Stars.Length);
                        if (i + s != starJson.Stars.Length - 1) {
                            starJson.Stars[s + i] = starJson.Stars[s + i + 1];
                        }
                        Array.Resize(ref starJson.Stars, starJson.Stars.Length - 1);
                    }
                }
            }
            PlayerPrefs.SetString("Stars", JsonUtility.ToJson(starJson));
            print(JsonUtility.ToJson(starJson));
            StarButtons[number].GetComponent<RawImage>().texture = unStar;
        }

    }
}
