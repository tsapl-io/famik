using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureScript : MonoBehaviour {

    [Header("外部参照用変数")]
    public bool isPictureSaved;
    public string ImageBase64;

    [Header("スクリプト内部変数")]
    WebCamTexture webCamTexture;
    public RawImage TemporaryPicture;
    public RawImage SimplePreviewPicture;

    [Header("画像ダイアログ")]
    public GameObject PictureDialog;
    public GameObject CompleteShot;

    Texture2D texture2d;

    public static Texture2D ToTexture2D( Texture self )
    {
        var sw = self.width;
        var sh = self.height;
        var format = TextureFormat.RGBA32;
        var result = new Texture2D( sw, sh, format, false );
        var currentRT = RenderTexture.active;
        var rt = new RenderTexture( sw, sh, 32 );
        Graphics.Blit( self, rt );
        RenderTexture.active = rt;
        var source = new Rect( 0, 0, rt.width, rt.height );
        result.ReadPixels( source, 0, 0 );
        result.Apply();
        RenderTexture.active = currentRT;
        return result;
    }
    public IEnumerator Wait()
    {
        yield return new WaitForEndOfFrame();
        texture2d.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture2d.Apply();
    }
    public string ImageBase64Get(Texture texture){
        webCamTexture.Play();
        Texture TempTexture = webCamTexture;
        webCamTexture.Stop();

        texture2d = ToTexture2D(TempTexture);
        StartCoroutine(Wait());

        return System.Convert.ToBase64String(texture2d.EncodeToJPG());
    }


    public void Start () {
        int DeviceNumber = 0;
        for (int i = 0; i < WebCamTexture.devices.Length; i++) {
            if (!WebCamTexture.devices[i].isFrontFacing)
            {
                DeviceNumber = i;

            }
        }

        webCamTexture = new WebCamTexture(WebCamTexture.devices[DeviceNumber].name);
        TemporaryPicture.texture = webCamTexture;
        webCamTexture.Play();
    }

    public void PictureShotButton () {
        isPictureSaved = true;
        // AudioServicesPlaySystemSound(1108);

        Texture TempTexture = webCamTexture;
        webCamTexture.Stop();
        TemporaryPicture.texture = TempTexture;
        SimplePreviewPicture.texture = TempTexture;

        ImageBase64 = ImageBase64Get(TempTexture);

        StartCoroutine(HidePictureDialogWith3Seconds());
    }
    public void ShowPictureDialog()
    {
        CompleteShot.SetActive(false);
        PictureDialog.SetActive(true);
        Start();
    }
    IEnumerator HidePictureDialogWith3Seconds ()
    {
        CompleteShot.SetActive(true);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)){
            CompleteShot.GetComponentInChildren<Text>().text = "権限がありません";
            CompleteShot.GetComponentInChildren<Text>().color = Color.red;
        } else {
            CompleteShot.GetComponentInChildren<Text>().text = "撮影完了";
            CompleteShot.GetComponentInChildren<Text>().color = Color.green;
        }
        yield return new WaitForSeconds(3);
        HidePictureDialog();
    }
    public void HidePictureDialog()
    {
        CompleteShot.SetActive(false);
        PictureDialog.SetActive(false);

    }
}
