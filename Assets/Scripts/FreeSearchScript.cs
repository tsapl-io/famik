using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class fY_YDF
{
    public fY_Feature[] Feature;
}
[Serializable]
public class fY_Feature
{
    public string Name;
    public fY_Propety Property;
}
[Serializable]
public class fY_Propety
{
    public string Uid;
}

[Serializable]
public class fY2_YDF
{
    public fY2_ResultInfo ResultInfo;
}
[Serializable]
public class fY2_ResultInfo
{
    public int Count;
}

[Serializable]
public class StarJson
{
    public string[] Stars;

}

// アプリケーションID一括管理クラス
public static class AppId {
    public const string SearchFree_StarsYOLP   = "XXXXXXXXXXXXXXXXXXXX";
    public const string SearchFree_SearchYOLP  = "XXXXXXXXXXXXXXXXXXXX";
    public const string SearchNoFree_YOLP      = "XXXXXXXXXXXXXXXXXXXX";
    public const string SearchNoFree_StaticMap = "XXXXXXXXXXXXXXXXXXXX";
}

public class FreeSearchScript : MonoBehaviour {
    public GameObject SearchResultPanel;

    public Texture unStar;
    public Texture Star;
    public List<Button> StarButtons;

    public InputField SearchBox;
    public Button SearchButton;

    public Text NameAndTel;

    public GameObject Dialog;
    public Text DialogText;

    public Text NameAndTelStar;

    bool isRunning;
    float lat;
    float lon;

    fY2_YDF ApiResponse2;
    fY_YDF ApiResponse;
    StarJson starJson;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }
        StartCoroutine(Gps_Auto());
        SearchButton.onClick.AddListener(() => {
            StartCoroutine(Search_Auto());
        });
        StartCoroutine(Search_Stars());
	}
	
	// Update is called once per frame
	void Update () {
		if (isRunning)
        {
            SearchBox.interactable = false;
            SearchButton.interactable = false;
        } else {
            SearchBox.interactable = true;
            SearchButton.interactable = true;
        }
    }

    IEnumerator Gps_Auto()
    {
        #if UNITY_EDITOR
            lat = 35.680914f;
            lon = 139.767735f;
            print("\n<color=#FF0000><size=15>エディターのため、擬似的な位置情報を入れました。</size></color>\n");
            yield return new WaitForEndOfFrame();
        #else
            if( isRunning ) { yield break; }
            isRunning = true;
            if (!Input.location.isEnabledByUser) {
                DialogManage("権限がありません。\nFamikへの位置情報アクセス許可をお願いします。");
                yield break;
            }
            Input.location.Start();
            int maxWait = 10;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait < 1) {
                DialogManage("タイムアウトしました。もう一度やり直してください。");
                isRunning = false;          
                yield break;
            }
            if (Input.location.status != LocationServiceStatus.Failed) {
                lat = Input.location.lastData.latitude;
                lon = Input.location.lastData.longitude;
                Input.location.Stop();
                isRunning = false;
            } else {
                isRunning = false;
            }
        #endif
    }

    IEnumerator Search_Auto()
    {
        if( isRunning ) { yield break; }
        isRunning = true;

        if (string.IsNullOrEmpty(SearchBox.text)) { isRunning = false; yield break; }

        SearchResultPanel.SetActive(true);
        NameAndTel.text = "";
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }

        if (Input.location.status == LocationServiceStatus.Failed) {
            DialogManage("ただいま位置情報を取得できません。");
            isRunning = false;          
            yield break;
        } else {

            //List<string> SearchCategory = new List<string>{"0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017",};
            //float[] distances = {11f, 5.3f, 2.6f, 1.6f, 0.9f, 0.4f, 0.2f};

            string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_SearchYOLP + "&gc=0401&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=20&output=json&results=7&detail=full&sort=dist&query=" + SearchBox.text;

            using (WWW www = new WWW(yolp_url))
            {
                yield return www;
                ApiResponse2 = JsonUtility.FromJson<fY2_YDF>(www.text);

                print("API リターン\n" + www.text);
                yield return new WaitForEndOfFrame();



                if (ApiResponse2.ResultInfo.Count != 0)
                {

                    for (int temp_starfalse = 0; temp_starfalse < ApiResponse2.ResultInfo.Count; temp_starfalse++) {
                        if (temp_starfalse < ApiResponse2.ResultInfo.Count) StarButtons[temp_starfalse].gameObject.SetActive(true);
                    }
                    ApiResponse = JsonUtility.FromJson<fY_YDF>(www.text);
                    yield return new WaitForEndOfFrame();

                    for (int i = 0; i < ApiResponse2.ResultInfo.Count; i++) {
                        starJson = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

                        int s = 0;
                        for (s = 0; s < starJson.Stars.Length; s++) {
                            if (starJson.Stars[s] == ApiResponse.Feature[i].Property.Uid) {
                                StarButtons[i].GetComponent<RawImage>().texture = Star;
                            }
                        }
                        int temp = i + 1;
                        string temp2 = ApiResponse.Feature[i].Name;
                        if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                        NameAndTel.text += temp + ": " + temp2 + "\n";
                    }


                }

                isRunning = false;
            }
        }

    }

    IEnumerator Search_Stars()
    {
        if( isRunning ) { yield break; }
        isRunning = true;


        NameAndTelStar.text = "";
        StarJson starJson2 = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

        if (starJson2.Stars.Length == 0)
        {
            NameAndTelStar.text = "登録しているかかりつけ医院がありません";
            isRunning = false;
            yield break;
        }

        string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_StarsYOLP + "&gc=0401&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=20&output=json&results=10&sort=dist";
        yolp_url += "&uid=";
        for (int i = 0; i < starJson2.Stars.Length; i++) {
            yolp_url += starJson2.Stars[i];
            if (i != starJson2.Stars.Length - 1) yolp_url += ",";
        }
        using (WWW www = new WWW(yolp_url))
        {
            yield return www;
            print("API リターン\n" + www.text);
            fY_YDF ApiResponse3 = JsonUtility.FromJson<fY_YDF>(www.text);
            for (int i = 0; i < ApiResponse3.Feature.Length; i++) {
                int temp = i + 1;
                string temp2 = ApiResponse3.Feature[i].Name;
                if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                NameAndTelStar.text += temp + ": " + temp2 + "\n";
            }

        }
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
    public void SearchResultHide()
    {
        SearchResultPanel.SetActive(false);
        StartCoroutine(Search_Stars());
    }
    public void PushStarButton(int number)
    {
        if (StarButtons[number].GetComponent<RawImage>().texture == unStar)
        {
            starJson = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));
            //for (int i = 0; i < starJson.Stars.Length; i++) print(starJson.Stars[i]);
            if (starJson.Stars.Length <= 5) {
                print(starJson.Stars.Length);
                if (starJson.Stars.Length == 0) Array.Resize(ref starJson.Stars, 1); else Array.Resize(ref starJson.Stars, starJson.Stars.Length + 1);
                print(ApiResponse.Feature.Length + "  " + number);
                print(string.Format("starJson.Stars({0})[{1}] = {2}", starJson.Stars.Length, starJson.Stars.Length - 1, ApiResponse.Feature[number].Property.Uid));
                starJson.Stars[starJson.Stars.Length - 1] = ApiResponse.Feature[number].Property.Uid;
                StarButtons[number].GetComponent<RawImage>().texture = Star;
                print(JsonUtility.ToJson(starJson));
                PlayerPrefs.SetString("Stars", JsonUtility.ToJson(starJson));
            }
        }
        else
        {
            /*
            List<string> list = starJson.Stars.ToList();
            
            list.Remove(ApiResponse.Feature[number].Property.Uid);
            
            starJson.Stars = list.ToArray();

            for (int i = 0; i < list.Count; i++) print(list[i]);          
            */

            /*
            for (int i = 0; i < starJson.Stars.Length; i++) {
                if (starJson.Stars[i] == ApiResponse.Feature[number].Property.Uid) Array.Clear(starJson.Stars, i, i+1);
            }
            */

            for (int i = 0; i < starJson.Stars.Length; i++) {
                if (starJson.Stars[i] == ApiResponse.Feature[number].Property.Uid) {
                    for (int s = 0; s < starJson.Stars.Length - i; s++) {
                        if (s + i + 1 != starJson.Stars.Length) {
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
