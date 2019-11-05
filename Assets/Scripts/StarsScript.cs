using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarsScript : MonoBehaviour {
    public GameObject SearchResultPanel;

    public Texture unStar;
    public Texture Star;
    public Texture[] StarAnimation;
    public List<Button> StarButtons;
    public List<Button> HPButtons;
    public List<Button> HPButtons2;

    public InputField SearchBox;
    public Button SearchButton;

    public Text NameAndTel;
    public GameObject NoStarsText;

    public GameObject Dialog;
    public Text DialogText;

    public Text NameAndTelStar;
    public GameObject YahooAPIImage;

    bool isRunning;

    fY_YDF ApiResponse3;
    fY2_YDF ApiResponse2;
    fY_YDF ApiResponse;
    StarJson starJson2;
    List<string> starList;
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
        starList = new List<string>(starJson2.Stars);

        print(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

        print(starJson2.Stars.Length);
        if (starJson2.Stars.Length == 0)
        {
            NoStarsText.SetActive(true);
            NameAndTelStar.text = "";
            isRunning = false;
            yield break;
        }

        string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_StarsYOLP + "&output=json&results=20&detail=full";
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


                for (int i = 0; i < starJson2.Stars.Length; i++) {
                    for (int s = 0; s < ApiResponse3.Feature.Length; s++) {
                        if (starJson2.Stars[i] == ApiResponse3.Feature[s].Property.Uid) {
                            int temp = i + 1;
                            string temp2 = ApiResponse3.Feature[s].Name;
                            if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                            NameAndTelStar.text += temp + ": " + temp2 + "\n";
                            StarButtons[i].GetComponent<RawImage>().texture = Star;
                            HPButtons[i].gameObject.SetActive(true);
                            StarButtons[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        //YahooAPIImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(YahooAPIImage.GetComponent<RectTransform>().anchoredPosition.x, 0 - (100 + (55 * ApiResponse3.Feature.Length)) + 30);
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
          for (int i = 0; i < ApiResponse3.Feature.Length; i++) {
              if (starJson2.Stars[number] == ApiResponse3.Feature[i].Property.Uid) {
                  Application.OpenURL(ApiResponse3.Feature[i].Property.Detail.YUrl);
              }
          }
    }

    public IEnumerator StarAnimate(bool isStar, int number) {
        if (isStar) {
            // スターをつけるアニメーション
            StarButtons[number].GetComponent<RawImage>().texture = unStar;
            yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            for (int i = 0; i < StarAnimation.Length; i++) {
                StarButtons[number].GetComponent<RawImage>().texture = StarAnimation[i];
                yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            }
            StarButtons[number].GetComponent<RawImage>().texture = Star;
        } else {
            // スターを消すアニメーション
            StarButtons[number].GetComponent<RawImage>().texture = Star;
            yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            StarButtons[number].GetComponent<RawImage>().texture = StarAnimation[1];
            yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            StarButtons[number].GetComponent<RawImage>().texture = StarAnimation[0];
            yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            StarButtons[number].GetComponent<RawImage>().texture = unStar;
        }
    }

    public void PushStarButton(int number)
    {
        if (StarButtons[number].GetComponent<RawImage>().texture == unStar) {
            starList.Add(starJson2.Stars[number]);
            StartCoroutine(StarAnimate(true, number));
        } else {
            starList.Remove(starJson2.Stars[number]);
            StartCoroutine(StarAnimate(false, number));
        }
        StarJson temporaryStarJson = new StarJson();//以下二行だけ
        temporaryStarJson.Stars = starList.ToArray();
        PlayerPrefs.SetString("Stars", JsonUtility.ToJson(temporaryStarJson));
    }
}
