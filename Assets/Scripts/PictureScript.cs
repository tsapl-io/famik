using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureScript : MonoBehaviour {

    [Header("外部参照用変数")]
    public bool isPictureSaved;
    public byte[] ImageBytes;

    [Header("スクリプト内部変数")]
    WebCamTexture webCamTexture;
    public RawImage TemporaryPicture;
    public RawImage SimplePreviewPicture;

    [Header("画像ダイアログ")]
    public GameObject PictureDialog;
    public GameObject CompleteShot;

    Texture2D texture2d;
    Texture TempTexture;

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

    public void Start () {
        #if UNITY_ANDROID
        TemporaryPicture.GetComponent<RectTransform>().localScale = new Vector3(-1, -1, 1);
        SimplePreviewPicture.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        #endif
        int DeviceNumber = 0;
        for (int i = 0; i < WebCamTexture.devices.Length; i++) {
            if (!WebCamTexture.devices[i].isFrontFacing)
            {
                DeviceNumber = i;

            }
        }

        webCamTexture = new WebCamTexture(WebCamTexture.devices[DeviceNumber].name, 640, 480);
        TemporaryPicture.texture = webCamTexture;
        webCamTexture.Play();
    }

    /*
    public void PictureShotButton () {
        isPictureSaved = true;
        // AudioServicesPlaySystemSound(1108);

        Texture TempTexture = webCamTexture;
        TemporaryPicture.texture = TempTexture;
        SimplePreviewPicture.texture = TempTexture;

        ImageBase64 = ImageBase64Get(TempTexture);

        StartCoroutine(HidePictureDialogWith3Seconds());
    }
    public string ImageBase64Get(Texture texture){
        Texture TempTexture = webCamTexture;

        texture2d = ToTexture2D(TempTexture);
        StartCoroutine(Wait());

        string returnTmp = System.Convert.ToBase64String(texture2d.EncodeToJPG());
        webCamTexture.Stop();
        return returnTmp;
    }
    */
    public void ShowPictureDialog()
    {
        CompleteShot.SetActive(false);
        PictureDialog.SetActive(true);
        Start();
    }






    public void PictureShotButton () {
        isPictureSaved = true;

    		ImageBytes = SaveToPNGFile(webCamTexture.GetPixels());

        TempTexture = webCamTexture;

        #if UNITY_IOS
        TemporaryPicture.GetComponent<RectTransform>().localScale = new Vector3(-1, -1, 1);
        SimplePreviewPicture.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        #endif

    		webCamTexture.Stop();

        StartCoroutine(HidePictureDialogWith3Seconds());
    }

  	byte[] SaveToPNGFile( Color[] texData ) {
    		Texture2D takenPhoto = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.ARGB32, false);

    		takenPhoto.SetPixels(texData);
    		takenPhoto.Apply();

    		byte[] png = takenPhoto.EncodeToPNG();
    		Destroy(takenPhoto);
    		return png;
  	}








    IEnumerator HidePictureDialogWith3Seconds ()
    {
        yield return new WaitForEndOfFrame();
        var texture = new Texture2D(1, 1);
        texture.LoadImage(ImageBytes);

        TemporaryPicture.texture = texture;
        SimplePreviewPicture.texture = texture;

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
