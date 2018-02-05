using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitSceneController : MonoBehaviour {

	NotificationManager notificationManager;

	void Start() {
		Camera.main.backgroundColor = PreferencesManager.GetPreferredBackgroundColor();
		notificationManager = GameObject.Find("Notification Manager").GetComponent<NotificationManager>();
		InGameData.notificationManager = notificationManager;
		notificationManager.Init();
		InGameData.VersionData.Init();
		DontDestroyOnLoad(notificationManager);
		ResourcesManager.Init();
		SceneManager.LoadScene("MenuScene");
	}

	void Update() {

	}
}
