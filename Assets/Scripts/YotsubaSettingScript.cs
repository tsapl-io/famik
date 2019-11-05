using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YotsubaSettingScript : MonoBehaviour {
	public UnityEngine.UI.Toggle YotsubaChan_Toggle;
	public UnityEngine.UI.Toggle YotsubaChanTalk_Toggle;
	public UnityEngine.UI.Toggle VibrateCheck_Toggle;

	public void Start() {
		if (PlayerPrefs.GetInt("YotsubaChan", 1) == 1) {
				YotsubaChan_Toggle.isOn = true;
		} else {
				YotsubaChan_Toggle.isOn = false;
		}
		if (PlayerPrefs.GetInt("YotsubaChanTalk", 0) == 0) {
				YotsubaChanTalk_Toggle.isOn = false;
		} else {
				YotsubaChanTalk_Toggle.isOn = true;
		}
		if (PlayerPrefs.GetInt("VibrateCheck", 1) == 1) {
				VibrateCheck_Toggle.isOn = true;
		} else {
				VibrateCheck_Toggle.isOn = false;
		}
	}

	public void YotsubaChan_Toggle_Event() {
			if (YotsubaChan_Toggle.isOn) {
					PlayerPrefs.SetInt("YotsubaChan", 1);
					YotsubaChan_Toggle.isOn = true;
			} else {
					PlayerPrefs.SetInt("YotsubaChan", 0);
					YotsubaChan_Toggle.isOn = false;
			}
	}
	public void YotsubaChanTalk_Toggle_Event() {
			if (YotsubaChanTalk_Toggle.isOn) {
					PlayerPrefs.SetInt("YotsubaChanTalk", 1);
					YotsubaChanTalk_Toggle.isOn = true;
			} else {
					PlayerPrefs.SetInt("YotsubaChanTalk", 0);
					YotsubaChanTalk_Toggle.isOn = false;
			}
	}
	public void VibrateCheck_Toggle_Event() {
			if (VibrateCheck_Toggle.isOn) {
					PlayerPrefs.SetInt("VibrateCheck", 1);
					VibrateCheck_Toggle.isOn = true;
			} else {
					PlayerPrefs.SetInt("VibrateCheck", 0);
					VibrateCheck_Toggle.isOn = false;
			}
	}
}
