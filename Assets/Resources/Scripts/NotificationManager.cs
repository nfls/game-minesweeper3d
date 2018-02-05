using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour {
	public static float Duration_Default = 3f;
	public static float Duration_Short = 1f;
	public static float Duration_Long = 5f;

	public float shadeInDuration = 1f;

	public static string notificationIconRootDirectory = "Textures/";

	Queue<Notification> notifications;
	Sprite[] icons;

	GameObject notificationCanvas;
	GameObject notificationBarA;
	GameObject notificationBarB;

	void Start() {

	}

	void Update() {

	}

	public void Init() {
		StopAllCoroutines();
		notifications = new Queue<Notification>();
		if (notificationBarA) {
			Destroy(notificationBarA);
			notificationBarA = null;
		}
		if (notificationBarB) {
			Destroy(notificationBarB);
			notificationBarB = null;
		}
		notificationCanvas = GameObject.Find("Notification Manager");
		notificationBarA = Resources.Load<GameObject>("Prefabs/NotificationBar");
		notificationBarA = Instantiate(notificationBarA, notificationCanvas.transform);
		notificationBarB = Instantiate(notificationBarA, notificationCanvas.transform);
		notificationBarA.SetActive(false);
		notificationBarB.SetActive(false);
		string[] names = Enum.GetNames(typeof(NotificationType));
		icons = new Sprite[names.Length];
		for (int i = 0; i < names.Length; i++) {
			icons[i] = Resources.Load<Sprite>(notificationIconRootDirectory + names[i] + "Icon");
		}
	}

	void AddNotification(Notification notification) {
		notifications.Enqueue(notification);
		if (notifications.Count == 1) {
			StartCoroutine(DisplayNotification(notification));
		}
	}

	IEnumerator DisplayNotification(Notification notification) {
		GameObject notificationBar;
		notificationBar = notificationBarA;
		LoadUpNotificationBar(notificationBar, notification);

		CanvasGroup canvasGroup = notificationCanvas.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0;

		float shadeInDuration = this.shadeInDuration;

		while (canvasGroup.alpha < 1) {
			float increase = Time.deltaTime * 1 / this.shadeInDuration;
			if (canvasGroup.alpha + increase > 1) {
				canvasGroup.alpha = 1;
			} else {
				canvasGroup.alpha += increase;
			}
			yield return 0;
		}

		yield return new WaitForSeconds(notification.duration);
		notificationBar.SetActive(false);
		notifications.Dequeue();
		if (notifications.Count > 0) {
			StartCoroutine(DisplayNotification(notifications.Peek()));
		}
	}

	void LoadUpNotificationBar(GameObject notificationBar, Notification notification) {
		notificationBar.SetActive(true);
		notificationBar.transform.Find("Icon Panel/Icon").GetComponent<Image>().sprite = icons[(int)notification.type];
		notificationBar.transform.Find("Content Panel/Title Text").GetComponent<Text>().text = notification.title;
		notificationBar.transform.Find("Content Panel/Content Text").GetComponent<Text>().text = notification.content;
	}

	public void NewNotification(NotificationType type, string content) {
		NewNotification(type, content, Duration_Default);
	}

	public void NewNotification(NotificationType type, string content, float duration) {
		Notification notification = new Notification();
		notification.type = type;
		switch (type) {
			case NotificationType.Achievement: {
					notification.title = "Achievenment Unlocked !";
					break;
				}
			case NotificationType.GainCasHours: {
					notification.title = "Gain Cas Hours !";
					break;
				}
			case NotificationType.GainExp: {
					notification.title = "Gain Exp !";
					break;
				}
			case NotificationType.Warning: {
					notification.title = "Warning !";
					break;
				}
			case NotificationType.Tip: {
					notification.title = "Tip !";
					break;
				}
		}
		notification.content = content;
		notification.duration = duration;
		AddNotification(notification);
	}



	class Notification {
		public NotificationType type;
		public string title;
		public string content;
		public float duration;
	}

	public enum NotificationType {
		Achievement = 0,
		LevelUp = 1,
		GainCasHours = 2,
		GainExp = 3,
		Warning = 4,
		Tip = 5
	}
}
