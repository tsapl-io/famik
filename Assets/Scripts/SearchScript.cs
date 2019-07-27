using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Y_YDF
{
    public Y_Feature[] Feature;
}
[Serializable]
public class Y_Feature
{
    public string Name;
    public Y_Geometry Geometry;
    public Y_Propety Property;
}
[Serializable]
public class Y_Geometry
{
    public string Coordinates;
}
[Serializable]
public class Y_Propety
{
    public string Tel1;
    public Y_Detail Detail;
}
[Serializable]
public class Y_Detail
{
    public string YUrl;
}

[Serializable]
public class Y2_YDF
{
    public Y2_ResultInfo ResultInfo;
}
[Serializable]
public class Y2_ResultInfo
{
    public int Count;
}

public class SearchScript : MonoBehaviour {
    int[] distances = {5000000, 5000000, 4000000, 2000000, 1000000, 500000, 250000, 100000, 50000, 25000, 10000, 5000, 2000, 1000, 500, 250, 100, 75, 50, 25, 10}; //直径を*10にしてyolpは検索、staticはそのままバーの数

    public List<Button> HPButtons = new List<Button> {};

    bool isRunning = false;
    float lat;
    float lon;

    public Dropdown CategoryDropdown;
    public Toggle OpenNow;
    public Slider z_MapSlider;
    public GameObject HPButtonsObject;

    public RawImage Maps;
    public Text NameAndTel;
    //0401003


    Y_YDF ApiResponse;
    Y2_YDF ApiResponse2;

    public GameObject Dialog;
    public Text DialogText;

    Coroutine ApiCoroutine;
	// Use this for initialization
	void Start () {
        CategoryDropdown.ClearOptions();
        CategoryDropdown.AddOptions(new List<string>{"小児科", "内科", "耳鼻咽喉科", "皮膚科", "歯科", "眼科", "外科", "整形外科", "胃腸科", "呼吸器科",});
        //SearchCategory.Add("0401003");
        /*
        string[] SearchCategory2 = new string[10] {"あ", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017"};

        string str = SearchCategory2[0];

        print(SearchCategory2);
        print(SearchCategory2.Length);

        print(str);
        */
        StartCoroutine(GpsGet());
	}
	
	// Update is called once per frame
	void Update () {
		if (isRunning)
        {
            CategoryDropdown.interactable = false;
            OpenNow.interactable = false;
            z_MapSlider.interactable = false;
            HPButtonsObject.SetActive(false);
        }
        else
        {
            CategoryDropdown.interactable = true;
            OpenNow.interactable = true;
            z_MapSlider.interactable = true;
            HPButtonsObject.SetActive(true);
        }
    }

    public void PushSearchButton()
    {
        for (int i = 0; i < HPButtons.Count; i++) { HPButtons[i].gameObject.SetActive(false); }
        StartCoroutine(ApiGet());
    }

    IEnumerator GpsGet()
    {
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
            StartCoroutine(ApiGet());
        }
        isRunning = false;
    }

    IEnumerator ApiGet()
    {
        if( isRunning ) { yield break; }
        isRunning = true;

        if (Input.location.status == LocationServiceStatus.Failed) {
            DialogManage("ただいま位置情報を取得できません。");
            isRunning = false;          
            yield break;
        } else {

            //float lat = 35.680914f;
            //float lon = 139.767735f;
            
            Maps.texture = new Texture2D(0, 0);

            NameAndTel.text = "";
            //public string[] SearchCategory = new string[10] {"0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017"};
            List<string> SearchCategory = new List<string>{"0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017",};
            //List<string> SearchCategoryName = 



            //float lat = 35.680914f;
            //float lon = 139.767735f;
            //int distance = 5;
            //string open = "now";
            string code = SearchCategory[CategoryDropdown.value];
            //int z = 16;

            string appid = "XXXXXXXXXXXXXXXXXXXX";
            string staticmap_url = "https://map.yahooapis.jp/map/V1/static?appid=" + appid + "&lat=" + lat + "&lon=" + lon + "&z=" + ((int)Math.Floor(z_MapSlider.value)).ToString() + "&pointer=on&width=750&height=750";
            string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + appid + "&gc=" + code + "&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=" + (distances[(int)Math.Floor(z_MapSlider.value)] / 100).ToString() + "&output=json&results=7&detail=full";  
            if (OpenNow.isOn) yolp_url += "&open=now";

            using (WWW www = new WWW(yolp_url))
            {
                yield return www;
                ApiResponse2 = JsonUtility.FromJson<Y2_YDF>(www.text);

                yield return new WaitForEndOfFrame();

                print(www.text);
                
                isRunning = false;
                
                if (ApiResponse2.ResultInfo.Count != 0)
                {

                    ApiResponse = JsonUtility.FromJson<Y_YDF>(www.text);

                    yield return new WaitForEndOfFrame();
                    
                    print(ApiResponse2.ResultInfo.Count);
                    for (int i = 0; i < ApiResponse2.ResultInfo.Count; i++) {
                        string[] arr = ApiResponse.Feature[i].Geometry.Coordinates.Split(',');
                        int temp = i + 1;
                        staticmap_url += "&pin" + temp + "=" + arr[1] + "," + arr[0] + "," + ApiResponse.Feature[i].Name;
                        string temp2 = ApiResponse.Feature[i].Name;
                        if (temp2.Length > 15) temp2 = temp2.Substring(0, 15) + "...";
                        NameAndTel.text += temp + ": " + temp2 + "\n";// + " (" + ApiResponse.Feature[i].Property.Tel1 + ")\n";
                    }

                    for (int s = 0; s < ApiResponse.Feature.Length; s++) { HPButtons[s].gameObject.SetActive(true); }

                }

                print(staticmap_url);
                print(yolp_url);

                using (WWW www2 = new WWW(staticmap_url))
                {
                    yield return www2;
                    Maps.texture = www2.texture;
                }
            }
        }
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
        SocialConnector.SocialConnector.Share ("現在地周辺の病院のデータ", "", Application.persistentDataPath + "/image.png");
        print("aaaa");
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
    public void OpenURL(int number)
    {
        if (ApiResponse.Feature.Length > number) {
            Application.OpenURL(ApiResponse.Feature[number].Property.Detail.YUrl);
        }
    }
}
