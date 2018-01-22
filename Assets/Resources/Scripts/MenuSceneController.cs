using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuSceneController : MonoBehaviour
{
	public GameObject background;
	public GameObject mainMenu;

	private NotificationManager notificationManager;

	private Stack<GameObject> previousMenus;

	private Toggle audioToggle;

	void Start()
	{
		//background = GameObject.Find("Background Controller").gameObject;
		//mainMenuCanvas = GameObject.Find("Main Menu").gameObject;
		notificationManager = GetComponent<NotificationManager>();
		notificationManager.Init();
		InGameData.notificationManager = notificationManager;
		//InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Achievement, "Coward For the 1st time !", NotificationManager.Duration_Default);
		//InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Achievement, "Winner For the 1st time !", NotificationManager.Duration_Default);
		previousMenus = new Stack<GameObject>();
		previousMenus.Push(mainMenu);

		if (InGameData.menuSceneIntent != InGameData.MenuSceneIntent.Main) {
			mainMenu.SetActive(false);
			GameObject subMenu = mainMenu.transform.parent.Find(InGameData.menuSceneIntent.ToString()).gameObject;
			previousMenus.Push(subMenu);
			subMenu.SetActive(true);
			InGameData.menuSceneIntent = InGameData.MenuSceneIntent.Main;
		}

		audioToggle = GameObject.Find("Audio Toggle").GetComponent<Toggle>();
		audioToggle.isOn = InGameData.audioEnabled;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}

		NetUtils.Init();
		StartCoroutine(NetUtils.GetRefreshToken("test", "fucknfls"));
	}

	void Update()
	{

	}

	public void OnBackgroundToggleChanged(GameObject toggle)
	{
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

	public void OnChangeMenuButtonClicked(GameObject subCanvas)
	{
		previousMenus.Peek().SetActive(false);
		previousMenus.Push(subCanvas);
		subCanvas.SetActive(true);
	}

	public void OnQuitButtonClicked()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
	}

	public void OnSliderChanged(GameObject slider)
	{
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

	public void PlayStandardMode()
	{
		InGameData.SetStandardModeData();
		Play();
	}

	public void Play()
	{
		SceneManager.LoadScene("GameScene");
	}

	public void OnBackButtonClicked()
	{
		previousMenus.Pop().SetActive(false);
		previousMenus.Peek().SetActive(true);
	}

	public void OnAudioToggleChanged()
	{
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