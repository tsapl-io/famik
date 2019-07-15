using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMover : MonoBehaviour {
    public bool DebugMode = false;
    public void RegistData()
    {
        SceneManager.LoadScene("Register");
    }
    public void ShowData()
    {
        SceneManager.LoadScene("Show");
    }
    public void BackButton()
    {
        SceneManager.LoadScene("Main");
    }
    public void HumanManageButton()
    {
        SceneManager.LoadScene("HumanManage");
    }
    public void SearchButton()
    {
        SceneManager.LoadScene("Search");
    }
    public void DebugButton()
    {
        Debug.Log("デバッグモードが開始されました。");
        SceneManager.LoadScene("Debug");
    }
}
