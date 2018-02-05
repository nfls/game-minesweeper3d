using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSceneController : MonoBehaviour {
	public GameObject background;
	public GameObject mainMenu;

	Stack<GameObject> previousMenus;

	Toggle audioToggle;

	void Start() {
		previousMenus = new Stack<GameObject>();
		previousMenus.Push(mainMenu);

		if (InGameData.menuSceneIntent != InGameData.MenuSceneIntent.Main) {
			mainMenu.SetActive(false);
			GameObject subMenu = mainMenu.transform.parent.Find(InGameData.menuSceneIntent.ToString()).gameObject;
			previousMenus.Push(subMenu);
			subMenu.SetActive(true);
			InGameData.menuSceneIntent = InGameData.MenuSceneIntent.Main;
		}

		Text versionText = GameObject.Find("Version Text").GetComponent<Text>();
		versionText.text = "@Version " + InGameData.VersionData.versionNum + " ";
		versionText.text += "<b><i>" + InGameData.VersionData.versionName + "</i></b> ";
		if (string.Equals(InGameData.VersionData.versionInfo, "Alpha", System.StringComparison.CurrentCultureIgnoreCase)) {
			versionText.text += " <color=red>Alpha</color>";
		} else if (string.Equals(InGameData.VersionData.versionInfo, "Beta", System.StringComparison.CurrentCultureIgnoreCase)) {
			versionText.text += " <color=cyan>Beta</color>";
		}

		audioToggle = GameObject.Find("Audio Toggle").GetComponent<Toggle>();
		audioToggle.isOn = InGameData.audioEnabled;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}
	}

	void Update() {

	}

	public void OnBackgroundToggleChanged(GameObject toggle) {
		if (toggle == null) {
			return;
		}
		Toggle toggleController = toggle.GetComponent<Toggle>();
		if (toggleController.isOn) {
			toggle.transform.Find("Label").GetComponent<Text>().text = "Dynamic\nBackground\nEnabled";
			background.GetComponent<BackgroundController>().OnEnabled();
		} else {
			toggle.transform.Find("Label").GetComponent<Text>().text = "Dynamic\nBackground\nDisabled";
			background.GetComponent<BackgroundController>().OnDisabled();
		}
	}

	public void OnChangeMenuButtonClicked(GameObject subCanvas) {
		previousMenus.Peek().SetActive(false);
		previousMenus.Push(subCanvas);
		subCanvas.SetActive(true);
		if (subCanvas.name.Equals("Options")) {
			subCanvas.transform.Find("X Inverse Toggle").GetComponent<Toggle>().isOn = PreferencesManager.xAxisInverse;
			subCanvas.transform.Find("Y Inverse Toggle").GetComponent<Toggle>().isOn = PreferencesManager.yAxisInverse;
			subCanvas.transform.Find("Special Effect Toggle").GetComponent<Toggle>().isOn = PreferencesManager.specialEffect;
			subCanvas.transform.Find("Rotate Speed Slider").GetComponent<Slider>().value = PreferencesManager.fieldRotateSpeed;
			subCanvas.transform.Find("Move Speed Slider").GetComponent<Slider>().value = PreferencesManager.cameraMoveSpeed;
		}
	}

	public void OnQuitButtonClicked() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
	}

	public void OnCustomSliderChanged(GameObject slider) {
		int v = (int)slider.GetComponent<Slider>().value;
		if (slider.name.Contains("Mines")) {
			InGameData.minesNum = v;
		} else {
			if (slider.name.Contains("MaxX")) {

				InGameData.maxX = v;
			} else if (slider.name.Contains("MaxY")) {

				InGameData.maxY = v;
			} else if (slider.name.Contains("MaxZ")) {

				InGameData.maxZ = v;
			}
			Slider mineSlider = slider.transform.parent.Find("Mines Num Slider").GetComponent<Slider>();
			int minesNum = InGameData.maxX * InGameData.maxY * InGameData.maxZ - 1;
			if (minesNum < InGameData.minesNum) {
				mineSlider.value = minesNum;
			}
			mineSlider.maxValue = minesNum;
		}
		slider.transform.Find("Num Text").GetComponent<Text>().text = v + "";
	}

	public void OnOptionsSliderChanged(GameObject slider) {
		int v = (int)slider.GetComponent<Slider>().value;
		slider.transform.Find("Num Text").GetComponent<Text>().text = v + "";
	}

	public void OnOptionsSaveButtonClicked(GameObject subCanvas) {
		PreferencesManager.xAxisInverse = subCanvas.transform.Find("X Inverse Toggle").GetComponent<Toggle>().isOn;
		PreferencesManager.yAxisInverse = subCanvas.transform.Find("Y Inverse Toggle").GetComponent<Toggle>().isOn;
		PreferencesManager.specialEffect = subCanvas.transform.Find("Special Effect Toggle").GetComponent<Toggle>().isOn;
		PreferencesManager.fieldRotateSpeed = (int)subCanvas.transform.Find("Rotate Speed Slider").GetComponent<Slider>().value;
		PreferencesManager.cameraMoveSpeed = (int)subCanvas.transform.Find("Move Speed Slider").GetComponent<Slider>().value;
		PreferencesManager.Save();
		InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Options Saved !", NotificationManager.Duration_Short);
	}

	public void PlayStandardMode() {
		InGameData.SetStandardModeData();
		Play();
	}

	public void Play() {
		SceneManager.LoadScene("GameScene");
	}

	public void OnBackButtonClicked() {
		previousMenus.Pop().SetActive(false);
		previousMenus.Peek().SetActive(true);
	}

	public void OnAudioToggleChanged() {
		if (audioToggle == null) {
			return;
		}
		InGameData.audioEnabled = !audioToggle.isOn;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}
	}
}