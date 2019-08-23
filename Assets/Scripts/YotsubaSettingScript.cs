using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YotsubaSettingScript : MonoBehaviour {
	public UnityEngine.UI.Toggle YotsubaChan_Toggle;

	public void Start() {
		if (PlayerPrefs.GetInt("YotsubaChan", 1) == 1) {
				YotsubaChan_Toggle.isOn = true;
		} else {
				YotsubaChan_Toggle.isOn = false;
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
}
