using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitSceneController : MonoBehaviour {

	public float loadingIconRotateSpeed = 15f;
	public float loadingTextFlashSpeed = 1f;

	bool isLoading;

	bool IsLoading {
		set {
			isLoading = value;
			if (value) {
				loadingDialog.SetActive(true);
				StartCoroutine(ExeLoadingIconRotateTask());
				StartCoroutine(ExeLoadingTextFlashTask());
			} else {
				loadingDialog.SetActive(false);
				StopCoroutine(ExeLoadingIconRotateTask());
				StopCoroutine(ExeLoadingTextFlashTask());
			}
		}
		get {
			return isLoading;
		}
	}

	bool isDownloading;
	bool IsDownloading {
		set {
			isLoading = value;
			if (value) {
				downloadingDialog.SetActive(true);
				StartCoroutine(ExeDownloadingProgressTask());
			} else {
				downloadingDialog.SetActive(false);
				StopCoroutine(ExeDownloadingProgressTask());
			}
		}
		get {
			return isDownloading;
		}
	}

	GameObject currentMenu;
	GameObject mainMenu;
	GameObject loginMenu;
	GameObject loadingDialog;
	GameObject downloadingDialog;

	InputField usernameField;
	InputField passwordField;

	NotificationManager notificationManager;

	void Start() {
		mainMenu = GameObject.Find("Main");
		loginMenu = GameObject.Find("Login");
		loadingDialog = GameObject.Find("Loading");
		downloadingDialog = GameObject.Find("Downloading");

		usernameField = GameObject.Find("Username Field").GetComponent<InputField>();
		passwordField = GameObject.Find("Password Field").GetComponent<InputField>();

		currentMenu = mainMenu;
		loginMenu.SetActive(false);
		loadingDialog.SetActive(false);
		downloadingDialog.SetActive(false);

		PreferencesManager.Init();

		Camera.main.backgroundColor = PreferencesManager.GetPreferredBackgroundColor();
		notificationManager = GameObject.Find("Notification Manager").GetComponent<NotificationManager>();
		DataManager.Init();
		InGameData.notificationManager = notificationManager;
		notificationManager.Init();
		DontDestroyOnLoad(notificationManager);
		InGameData.Init();
		NetUtils.Init();
		if (DataManager.hasNoLocalHotAssets) {
			if (NetUtils.IsOffline()) {
				notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Please connect to the internet to initiate the game for the first time!");
			}
		}

		if (!NetUtils.REFRESH_TOKEN.Equals("Null")) {
			IsLoading = true;
			StartCoroutine(NetUtils.GetAccessToken(HandleGetAccessTokenSucceeds, delegate (string error, bool losesConnection) {
				if (!losesConnection) {
					notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Refresh Token is out of date! Please login again!");
				}
				IsLoading = false;
				StartLoginTask();
			}));
		} else {
			IsLoading = false;
			StartLoginTask();
		}
	}

	void Update() {
		if (Input.GetKey(KeyCode.LeftControl)) {
			if (Input.GetKey(KeyCode.Q)) {
				IsDownloading = false;
				OnQuitButtonClicked();
			}
		}
		if (loginMenu.activeSelf) {
			if (!IsLoading) {
				if (Input.GetKey(KeyCode.Return)) {
					OnLoginButtonClicked();
				}
			}
		}
	}

	public void ChangeToMenu(GameObject newMenu) {
		if (currentMenu) {
			currentMenu.SetActive(false);
		}
		newMenu.SetActive(true);
	}

	public void StartLoginTask() {
		ChangeToMenu(loginMenu);
		if (!UserManager.name.Equals("Guest")) {
			usernameField.text = UserManager.name;
		}
	}

	public void StartUpdateTask() {
		if (InGameData.VersionData.needsReinstall) {
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Brand New Version Released !", NotificationManager.DURATION_DEFAULT);
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Older Version May Crush !", NotificationManager.DURATION_DEFAULT);
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Reinstall the new version !", NotificationManager.DURATION_LONG);
			StartCoroutine(ExeOpenUrlTask("https://nfls.io/#/media/game", NotificationManager.DURATION_DEFAULT * 2));
		}
		if (InGameData.VersionData.needsUpdate) {
			IsDownloading = true;
			StartCoroutine(NetUtils.GetHotAssets(delegate {
				IsDownloading = false;
				IsLoading = true;
			}, delegate {
				IsLoading = false;
				notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Writing Finished !", NotificationManager.DURATION_SHORT);
				ResourcesManager.Init();
				InGameData.casHourManager.StartPeriodRewardCheckTask();
				SceneManager.LoadScene("MenuScene");
			}, delegate (string error, bool losesConnection) {
				IsDownloading = false;
				if (!losesConnection) {
					notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Server Error，Please Report Error Code [" + NetUtils.GetResponseCode(error) + "] To Development Team!", NotificationManager.DURATION_LONG);
				}
			}));
		} else {
			ResourcesManager.Init();
			InGameData.casHourManager.StartPeriodRewardCheckTask();
			SceneManager.LoadScene("MenuScene");
		}
	}

	public void OnLoginButtonClicked() {
		if (IsLoading) {
			return;
		}
		IsLoading = true;
		string username = usernameField.text;
		string password = passwordField.text;
		if (username == "" || password == "") {
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Input cannot be NULL!", NotificationManager.DURATION_SHORT);
			IsLoading = false;
			return;
		}
		StartCoroutine(NetUtils.GetRefreshToken(username, password, HandleGetAccessTokenSucceeds, delegate (string error, bool losesConnection) {
			IsLoading = false;
			if (!losesConnection) {
				int responseCode = NetUtils.GetResponseCode(error);
				if (responseCode == 401) {
					notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Invalid Username or Password!");
				}
			}
		}));
	}

	public void OnOfflineButtonClicked() {
		if (DataManager.hasNoLocalHotAssets) {
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Please connect to the internet to initiate the game for the first time!");
		} else {
			if (IsDownloading) {
				StopCoroutine(NetUtils.GetHotAssets(NetUtils.NullMethod, NetUtils.NullMethod, NetUtils.NullMethod));
			}
			NetUtils.offlineMode = true;
			ResourcesManager.Init();
			IsLoading = true;
			SceneManager.LoadScene("MenuScene");
		}
	}

	public void OnQuitButtonClicked() {
		if (IsDownloading) {
			notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Downloading is in progress, please wait or press CTRL + Q to QUIT immediately.");
			return;
		}

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void HandleGetAccessTokenSucceeds() {
		notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Login Succeeds!");
		StartCoroutine(NetUtils.GetVersionInfo(delegate {
			//InGameData.VersionData.PrintData();
			StartCoroutine(NetUtils.GetUserInfo(delegate {
				//UserManager.PrintData();
				IsLoading = false;
				StartUpdateTask();
			}, NetUtils.NullMethod));
		}, NetUtils.NullMethod));
	}

	IEnumerator ExeLoadingIconRotateTask() {
		Transform icon = loadingDialog.transform.Find("Icon");
		while (true) {
			icon.Rotate(0, 0, -loadingIconRotateSpeed * Time.deltaTime);
			yield return 0;
		}
	}

	IEnumerator ExeLoadingTextFlashTask() {
		Text text = loadingDialog.transform.Find("Panel/Text").GetComponent<Text>();
		while (true) {
			string suffix = text.text.Substring(text.text.IndexOf(".") + 1);
			if (suffix.Equals("/")) {
				text.text = "Loading .\\";
			} else {
				text.text = "Loading ./";
			}
			yield return new WaitForSeconds(loadingTextFlashSpeed);
		}
	}

	IEnumerator ExeDownloadingProgressTask() {
		Slider progressBar = downloadingDialog.transform.Find("Progress Bar").GetComponent<Slider>();
		Text text = downloadingDialog.transform.Find("Panel/Text").GetComponent<Text>();
		int lastProgress = 0;
		while (true) {
			if (NetUtils.hotassetsWWW != null) {
				float progress = NetUtils.hotassetsWWW.progress;
				progressBar.value = progress;
				if ((int)progress * 100 != lastProgress) {
					lastProgress = (int)progress * 100;
					text.text = "Downloading... " + lastProgress + "%";
				}
			}
			yield return 0;
		}
	}

	public static IEnumerator ExeOpenUrlTask(string url, float delay) {
		yield return new WaitForSeconds(delay);
		Application.OpenURL(url);
	}
}