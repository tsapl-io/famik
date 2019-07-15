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

    public Dropdown CategoryDropdown;

    public RawImage Maps;
    public Text NameAndTel;
    //0401003


    Y_YDF ApiResponse;
    Y2_YDF ApiResponse2;

	// Use this for initialization
	void Start () {

        //SearchCategory.Add("0401003");
        /*
        string[] SearchCategory2 = new string[10] {"あ", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017"};

        string str = SearchCategory2[0];

        print(SearchCategory2);
        print(SearchCategory2.Length);

        print(str);
        */

        StartCoroutine(ApiGet());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PushSearchButton()
    {
        StartCoroutine(ApiGet());
    }

    IEnumerator ApiGet()
    {
        Maps.texture = new Texture2D(0, 0);

        //public string[] SearchCategory = new string[10] {"0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017"};
        List<string> SearchCategory = new List<string>{"0401003", "0401002", "0401009", "0401008", "0401001", "0401007", "0401006", "0401004", "0401005", "0401017",};
        List<string> SearchCategoryName = new List<string>{"小児科", "内科", "耳鼻咽喉科", "皮膚科", "歯科", "眼科", "外科", "整形外科", "胃腸科", "呼吸器科",};


        CategoryDropdown.ClearOptions();
        CategoryDropdown.AddOptions(SearchCategoryName);

        float lat = 35.65804109f;
        float lon = 139.69826201f;
        //float lat = 35.680914f;
        //float lon = 139.767735f;
        int distance = 5;
        //string open = "now";
        string code = SearchCategory[CategoryDropdown.value];
        int z = 16;

        print(SearchCategory[0]);

        print("CategoryDropdown.value: " + CategoryDropdown.value);
        print("SearchCategory[CategoryDropdown.value]: " + SearchCategory[CategoryDropdown.value]);
        print("SearchCategoryName[CategoryDropdown.value]:" + SearchCategoryName[CategoryDropdown.value]);

        string appid = "XXXXXXXXXXXXXXXXXXXX";
        string staticmap_url = "https://map.yahooapis.jp/map/V1/static?appid=" + appid + "&lat=" + lat + "&lon=" + lon + "&z=" + z + "&pointer=on&width=750&height=750";
        string yolp_url = "https://map.yahooapis.jp/search/local/V1/localSearch?appid=" + appid + "&gc=" + code + "&lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&dist=" + distance.ToString() + "&sort=hybrid&output=json&results=7";  

        using (WWW www = new WWW(yolp_url))
        {
            yield return www;
            ApiResponse2 = JsonUtility.FromJson<Y2_YDF>(www.text);

            yield return new WaitForEndOfFrame();

            print(www.text);

            if (ApiResponse2.ResultInfo.Count != 0)
            {
                ApiResponse = JsonUtility.FromJson<Y_YDF>(www.text);

                yield return new WaitForEndOfFrame();

                NameAndTel.text = "";

                print(ApiResponse2.ResultInfo.Count);
                for (int i = 0; i < ApiResponse2.ResultInfo.Count; i++) {
                    string[] arr = ApiResponse.Feature[i].Geometry.Coordinates.Split(',');
                    int temp = i + 1;
                    staticmap_url += "&pin" + temp + "=" + arr[1] + "," + arr[0] + "," + ApiResponse.Feature[i].Name;
                    NameAndTel.text += temp + ": " + ApiResponse.Feature[i].Name + " (" + ApiResponse.Feature[i].Property.Tel1 + ")\n";
                }
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
}
