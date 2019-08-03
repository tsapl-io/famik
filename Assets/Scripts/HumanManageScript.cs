using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HumanManageScript : MonoBehaviour {
    public Dropdown HumanSelect;
    public List<string> HumanSelectOptions = new List<string> { };

    public InputField humanname;


    [Header("ダイアログ")]
    public GameObject DialogObject;
    public Text DialogObject_Text;

    [Header("ボタンとか")]
    public Button DeleteHumanButton;

    string TempRegisterName;

    // Use this for initialization
    void Start () {
        if (PlayerPrefs.GetString("Famik", "NO DATA") == "NO DATA" || JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).Humans.Length == 0)
        {
            DeleteHumanButton.interactable = false;
            HumanSelect.interactable = false;
            PlayerPrefs.SetString("Famik", "{\"Humans\":[], \"FDV\": " + FamikDatas.FamikDataVersion + "}");
        }
        SaveData LoadedData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA"));
        for (int i = 0; i < LoadedData.Humans.Length; i++)
        {
            HumanSelectOptions.Add(LoadedData.Humans[i].Name);
        }
        HumanSelect.AddOptions(HumanSelectOptions);
    }

	// Update is called once per frame
	void Update () {

	}

    public void CreateHuman()
    {
        if (humanname.text != "")
        {
            SaveData storage = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik"));

            for (int i = 0; i < storage.Humans.Length; i++) {
                if (storage.Humans[i].Name == humanname.text) TempRegisterName = humanname.text;
            }

            if (String.IsNullOrEmpty(TempRegisterName)) {
                Array.Resize(ref storage.Humans, storage.Humans.Length + 1);
                storage.Humans[storage.Humans.Length - 1] = new HumanData();
                storage.Humans[storage.Humans.Length - 1].Name = humanname.text;

                PlayerPrefs.SetString("Famik", JsonUtility.ToJson(storage));
                SceneManager.LoadScene("HumanManage");
            } else DeleteHumanDialogShow("この名前はすでに使われています。\n重複して登録しますか？");
        }
    }
    public void DeleteHumanDialogShow()
    {
        string inStorageData = PlayerPrefs.GetString("Famik", "NO DATA");
        if (inStorageData != "NO DATA")
        {
            DialogObject_Text.text = "本当に削除しますか？";
            DialogObject.SetActive(true);
        }
        else
        {
            // error
        }
    }
    public void DeleteHumanDialogShow(string mes)
    {
        string inStorageData = PlayerPrefs.GetString("Famik", "NO DATA");
        if (inStorageData != "NO DATA")
        {
            DialogObject_Text.text = mes;
            DialogObject.SetActive(true);
        }
        else
        {
            // error
        }
    }

    public void Dialog_OK()
    {
        if (DialogObject_Text.text == "本当に削除しますか？") {
            StartCoroutine(deleteUser());
        } else if (DialogObject_Text.text == "この名前はすでに使われています。\n重複して登録しますか？") {
          SaveData storage = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik"));

          Array.Resize(ref storage.Humans, storage.Humans.Length + 1);
          storage.Humans[storage.Humans.Length - 1] = new HumanData();
          storage.Humans[storage.Humans.Length - 1].Name = TempRegisterName;

          PlayerPrefs.SetString("Famik", JsonUtility.ToJson(storage));
          SceneManager.LoadScene("HumanManage");
        }
    }
    IEnumerator deleteUser() {
        SaveData storage = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik"));

        for (int i = 0; i < storage.Humans[HumanSelect.value].OneSicks.Length; i++) {
            if (storage.Humans[HumanSelect.value].OneSicks[i].Image != "NOTHING") {
              print(@Application.persistentDataPath + "/" + storage.Humans[HumanSelect.value].OneSicks[i].Image + ".famikimage");
              File.Delete(@Application.persistentDataPath + "/" + storage.Humans[HumanSelect.value].OneSicks[i].Image + ".famikimage");
              yield return new WaitForEndOfFrame();
            }
        }

        var list = new List<HumanData>();
        list.AddRange(storage.Humans);

        list.Remove(storage.Humans[HumanSelect.value]);

        storage.Humans = list.ToArray();

        PlayerPrefs.SetString("Famik", JsonUtility.ToJson(storage));
        SceneManager.LoadScene("HumanManage");
    }
    public void Dialog_Cancel()
    {
        DialogObject.SetActive(false);
    }
}
