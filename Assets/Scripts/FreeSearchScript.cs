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
    public fY_Detail Detail;
}
[Serializable]
public class fY_Detail
{
    public string YUrl;
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
    public int Total;
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

    public Texture unStar;
    public Texture Star;
    public Texture[] StarAnimation;
    public List<Button> StarButtons;
    public List<Button> HPButtons;
    public List<Button> HPButtons2;
    public Dropdown CategoryDropdown;

    public InputField SearchBox;
    public Toggle Condition1;
    public Button SearchButton;

    public Text NameAndTel;

    public GameObject Dialog;
    public Text DialogText;

    public Text NameAndTelStar;

    public Text MoreThan7Text;

    bool isRunning;
    float lat;
    float lon;

    fY_YDF ApiResponse3;
    fY2_YDF ApiResponse2;
    fY_YDF ApiResponse;
    StarJson starJson;

	// Use this for initialization
	void Start () {
        CategoryDropdown.ClearOptions();
        CategoryDropdown.AddOptions(new List<string>{"指定なし", "小児科", "内科", "耳鼻咽喉科", "皮膚科", "歯科", "眼科", "外科", "整形外科", "胃腸科", "呼吸器科",});

	      for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].gameObject.SetActive(false); }
        //for (int i = 0; i < HPButtons.Count; i++) { HPButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < HPButtons2.Count; i++) { HPButtons2[i].gameObject.SetActive(false); }
        NameAndTel.text = "";
        StartCoroutine(Gps_Auto());
        SearchButton.onClick.AddListener(() => {
            StartCoroutine(Search_Auto());
        });
        StartCoroutine(Search_Stars());
        MoreThan7Text.color = Color.black;
        MoreThan7Text.text = "かかりつけ病院登録は15件までです";
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

    public void Search_Button() {
      StartCoroutine(Search_Auto());
    }

    IEnumerator Search_Auto()
    {
        if( isRunning ) { yield break; }
        isRunning = true;

        if (string.IsNullOrEmpty(SearchBox.text)) { isRunning = false; yield break; }

        MoreThan7Text.color = Color.black;
        MoreThan7Text.text = "かかりつけ病院登録は15件までです";

        NameAndTel.text = "";
        for (int i = 0; i < HPButtons2.Count; i++) { HPButtons2[i].gameObject.SetActive(false); }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].gameObject.SetActive(false); }
        for (int i = 0; i < StarButtons.Count; i++) { StarButtons[i].GetComponent<RawImage>().texture = unStar; }

        if (Input.location.status == LocationServiceStatus.Failed) {
            DialogManage("ただいま位置情報を取得できません。");
            isRunning = false;
            yield break;
        } else {

            List<string> SearchCategory = new List<string>{"0401", "0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017",};
            //float[] distances = {11f, 5.3f, 2.6f, 1.6f, 0.9f, 0.4f, 0.2f};

            string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_SearchYOLP + "&gc=" + SearchCategory[CategoryDropdown.value] + "&output=json&results=15&detail=full&query=" + SearchBox.text;

            if (Condition1.isOn) {
                yolp_url += "&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=20&sort=dist";
            } else {
                yolp_url += "&sort=kana";
            }

            using (WWW www = new WWW(yolp_url))
            {
                yield return www;
                ApiResponse2 = JsonUtility.FromJson<fY2_YDF>(www.text);

                print("API リターン\n" + www.text);
                yield return new WaitForEndOfFrame();



                if (ApiResponse2.ResultInfo.Count != 0)
                {

                    for (int tmp = 0; tmp < ApiResponse2.ResultInfo.Count; tmp++) {
                        if (tmp < ApiResponse2.ResultInfo.Count) StarButtons[tmp].gameObject.SetActive(true);
                    }
                    for (int tmp = 0; tmp < ApiResponse2.ResultInfo.Count; tmp++) {
                        if (tmp < ApiResponse2.ResultInfo.Count) HPButtons2[tmp].gameObject.SetActive(true);
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

                    if (ApiResponse2.ResultInfo.Total > 15) {
                        MoreThan7Text.color = Color.red;
                        MoreThan7Text.text = "検索結果が15件以上でした\n条件を絞ってください";
                    }
                }
                else
                {
                    NameAndTel.text = "\n\n     検索結果は0件でした";

                }

                isRunning = false;
            }
        }

    }

    IEnumerator Search_Stars()
    {
        #region FreeSearchには2019/07/30からかかりつけ医を表示しなくなりました
        yield break;
        #endregion

        if ( isRunning ) { yield break; }
        isRunning = true;

        for (int i = 0; i < HPButtons.Count; i++) { HPButtons[i].gameObject.SetActive(false); }

        NameAndTelStar.text = "";
        StarJson starJson2 = JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}"));

        if (starJson2.Stars.Length == 0)
        {
            NameAndTelStar.text = "登録しているかかりつけ医院なし";
            isRunning = false;
            yield break;
        }

        string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + AppId.SearchFree_StarsYOLP + "&gc=0401&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=20&output=json&results=15&sort=dist&detail=full";
        yolp_url += "&uid=";
        for (int i = 0; i < starJson2.Stars.Length; i++) {
            yolp_url += starJson2.Stars[i];
            if (i != starJson2.Stars.Length - 1) yolp_url += ",";
        }
        using (WWW www = new WWW(yolp_url))
        {
            yield return www;
            print("API リターン\n" + www.text);
            ApiResponse3 = JsonUtility.FromJson<fY_YDF>(www.text);
            for (int i = 0; i < ApiResponse3.Feature.Length; i++) {
                int temp = i + 1;
                string temp2 = ApiResponse3.Feature[i].Name;
                if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                NameAndTelStar.text += temp + ": " + temp2 + "\n";
            }
            for (int tmp = 0; tmp < ApiResponse3.Feature.Length; tmp++) {
                if (tmp < ApiResponse3.Feature.Length) HPButtons[tmp].gameObject.SetActive(true);
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
        StartCoroutine(Search_Stars());
    }
    public void OpenURL_Search(int number)
    {
        if (ApiResponse.Feature.Length > number) {
            Application.OpenURL(ApiResponse.Feature[number].Property.Detail.YUrl);
        }
    }
    public void OpenURL_Stars(int number)
    {
        if (ApiResponse3.Feature.Length > number) {
            Application.OpenURL(ApiResponse3.Feature[number].Property.Detail.YUrl);
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
        List<string> starList = new List<string>(JsonUtility.FromJson<StarJson>(PlayerPrefs.GetString("Stars", "{\"Stars\":[]}")).Stars);
        if (StarButtons[number].GetComponent<RawImage>().texture == unStar)
        {
            if (starList.Count <= 14) {
                starList.Add(ApiResponse.Feature[number].Property.Uid);
                StartCoroutine(StarAnimate(true, number));
            } else {
                Handheld.Vibrate();
            }
        }
        else
        {
            starList.Remove(ApiResponse.Feature[number].Property.Uid);
            StartCoroutine(StarAnimate(false, number));
        }
        StarJson temporaryStarJson = new StarJson();//以下二行だけ
        temporaryStarJson.Stars = starList.ToArray();
        PlayerPrefs.SetString("Stars", JsonUtility.ToJson(temporaryStarJson));
    }
}
