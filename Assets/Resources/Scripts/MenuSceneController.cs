using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSceneController : MonoBehaviour {
	public GameObject background;
	public GameObject mainMenu;

	Camera mainCamera;

	Stack<GameObject> previousMenus;

	Toggle audioToggle;

	bool hasDisplayedShop;

	void Start() {
		mainCamera = Camera.main;
		mainCamera.backgroundColor = PreferencesManager.GetPreferredBackgroundColor();

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
			versionText.text += " <color=red>Alpha</color> ";
		} else if (string.Equals(InGameData.VersionData.versionInfo, "Beta", System.StringComparison.CurrentCultureIgnoreCase)) {
			versionText.text += " <color=cyan>Beta</color> ";
		}
		if (NetUtils.IsOffline()) {
			versionText.text += "Offline";
		} else {
			versionText.text += "<color=green>Online</color>";
		}

		audioToggle = GameObject.Find("Audio Toggle").GetComponent<Toggle>();
		audioToggle.isOn = InGameData.audioEnabled;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}

		InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Welcome, " + UserManager.name + "!");
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
		} else if (subCanvas.name.Equals("Shop")) {
			DisplayShop(subCanvas);
		}
	}

	public void OnRankButtonClicked(GameObject subCanvas) {
		if (NetUtils.IsOffline()) {
			InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Offline Mode! Cannot connect to server!", NotificationManager.DURATION_SHORT);
		} else {
			OnChangeMenuButtonClicked(subCanvas);
			StartCoroutine(NetUtils.GetRank(delegate {
				RefreshRankText(subCanvas.transform.Find("Scroll View/Viewport/Content/Text").GetComponent<Text>());
			}, NetUtils.NullMethod));
		}
	}

	void RefreshRankText(Text t) {
		t.text = "";
		for (int i = 0; i < InGameData.rankInfos.Count; i++) {
			InGameData.PlayerRankInfo rankInfo = InGameData.rankInfos[i];
			if (rankInfo.rank == 1) {
				t.text += "<b><color=yellow>[" + rankInfo.rank + "]  " + rankInfo.name + "  -  id:" + rankInfo.id + "  -  " + rankInfo.time + "s</color></b>";
			} else if (rankInfo.rank == 2) {
				t.text += "<b><color=silver>[" + rankInfo.rank + "]  " + rankInfo.name + "  -  id:" + rankInfo.id + "  -  " + rankInfo.time + "s</color></b>";
			} else if (rankInfo.rank == 3) {
				t.text += "<b><color=brown>[" + rankInfo.rank + "]  " + rankInfo.name + "  -  id:" + rankInfo.id + "  -  " + rankInfo.time + "s</color></b>";
			} else if (rankInfo.isPlayer) {
				t.text += "<color=cyan>[" + rankInfo.rank + "]  " + rankInfo.name + "  -  id:" + rankInfo.id + "  -  " + rankInfo.time + "s</color>";
			}

			if (rankInfo.isPlayer) {
				t.text += "<color=cyan>  [Player]</color>";
			}
			if (rankInfo.isAdmin) {
				t.text += "<color=green>  [Admin]</color>";
			}
			t.text += "\n";
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
		InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Options Saved !", NotificationManager.DURATION_SHORT);
	}

	public void OnLogOutButtonClicked() {
		if (NetUtils.ReachesInternet()) {
			LogOut();
			SceneManager.LoadScene("InitScene");
		} else {
			LogOut();
		}
	}

	void LogOut() {
		UserManager.LogOut();
		NetUtils.LogOut();
		InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Log Out Succeeds!");
	}

	public void PlayStandardMode() {
		InGameData.SetStandardModeData();
		Play();
	}

	public void Play() {
		SceneManager.LoadScene("GameScene");
	}

	public void DisplayShop(GameObject subCanvas) {
		if (hasDisplayedShop) {
			return;
		}

		hasDisplayedShop = true;
		string[] skinPackTypes = ResourcesManager.GetSkinPackTypes();
		string[] audioPackTypes = ResourcesManager.GetAudioPackTypes();

		GameObject packPanel = Resources.Load<GameObject>("Prefabs/PackPanel");
		GameObject packButton = Resources.Load<GameObject>("Prefabs/PackButton");
		Transform content = subCanvas.transform.Find("Scroll View/Viewport/Content");

		GameObject skinPackPanel = Instantiate(packPanel, content);
		skinPackPanel.name = "Skin Pack Panel";
		skinPackPanel.GetComponentInChildren<Text>().text = "Skin Packs";

		for (int i = 0; i < skinPackTypes.Length; i++) {
			GameObject skinPackButton = Instantiate(packButton, content);
			skinPackButton.name = skinPackTypes[i] + " Skin Button";
			skinPackButton.GetComponentInChildren<Text>().text = skinPackTypes[i] + " Skin Pack";
			skinPackButton.GetComponent<Button>().onClick.AddListener(delegate {
				ResourcesManager.SetCurrentSkinPack(skinPackButton.name.Substring(0, skinPackButton.name.IndexOf(" ")));
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Skin Pack Changed! Wait for the next turn to see the effect!", NotificationManager.DURATION_SHORT);
			});
		}

		GameObject audioPackPanel = Instantiate(packPanel, content);
		audioPackPanel.name = "Audio Pack Panel";
		audioPackPanel.GetComponentInChildren<Text>().text = "Audio Packs";

		for (int i = 0; i < audioPackTypes.Length; i++) {
			GameObject audioPackButton = Instantiate(packButton, content);
			audioPackButton.name = audioPackTypes[i] + " Audio Button";
			audioPackButton.GetComponentInChildren<Text>().text = audioPackTypes[i] + " Audio Pack";
			audioPackButton.GetComponent<Button>().onClick.AddListener(delegate {
				ResourcesManager.SetCurrentAudioPack(audioPackButton.name.Substring(0, audioPackButton.name.IndexOf(" ")));
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Audio Pack Changed! Wait for the next turn to see the effect!", NotificationManager.DURATION_SHORT);
			});
		}
	}

	public void DisplayRank() {

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