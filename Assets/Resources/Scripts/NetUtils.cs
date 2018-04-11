using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using LitJson;

public class NetUtils {
	public static readonly string OFFLINE_SIGN = "Cannot resolve destination host";
	public static readonly string CLIENT_ID = "BX9oQ30TKiXYYnbU0ISYsQ==";
	public static readonly string CLIENT_SECRET = "vCKja9UBcnk63lElGgBi2i7pjoqgsJtWndV7SiFrcVQ=";
	public static string REFRESH_TOKEN = "Null";
	public static string ACCESS_TOKEN = "Null";

	public static bool offlineMode;

	public static WWW hotassetsWWW;

	public static void Init() {

	}

	public static void LogOut() {
		REFRESH_TOKEN = "Null";
		ACCESS_TOKEN = "Null";
		DataManager.SaveNetData();
	}

	public static IEnumerator GetRefreshToken(string username, string password, Action successAction, Action<string, bool> errorAction) {
		string content = "grant_type=password&username=" + username + "&password=" + password + "&scope=user.all&client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET;
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Content-Type"] = "application/x-www-form-urlencoded";
		byte[] rawData = Encoding.UTF8.GetBytes(content);
		WWW www = new WWW("https://nfls.io/oauth/accessToken", rawData, headers);
		yield return www;
		if (www.error == null) {
			JsonData json = JsonMapper.ToObject(www.text);
			REFRESH_TOKEN = (string)json["refresh_token"];
			ACCESS_TOKEN = (string)json["access_token"];
			DataManager.SaveNetData();
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static IEnumerator GetAccessToken(Action successAction, Action<string, bool> errorAction) {
		string content = "grant_type=refresh_token&scope=user.all&client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET + "&refresh_token=" + REFRESH_TOKEN;
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Content-Type"] = "application/x-www-form-urlencoded";
		byte[] rawData = Encoding.UTF8.GetBytes(content);
		WWW www = new WWW("https://nfls.io/oauth/accessToken", rawData, headers);
		yield return www;
		if (www.error == null) {
			JsonData json = JsonMapper.ToObject(www.text);
			REFRESH_TOKEN = (string)json["refresh_token"];
			ACCESS_TOKEN = (string)json["access_token"];
			DataManager.SaveNetData();
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static IEnumerator GetVersionInfo(Action successAction, Action<string, bool> errorAction) {
		string url = "https://nfls.io/device/version?client_id=" + CLIENT_ID;
		WWW www = new WWW(url);
		yield return www;
		if (www.error == null) {
			string[] signs = { " " };
			string[] versionData = ((string)JsonMapper.ToObject(www.text)["data"]).Split(signs, StringSplitOptions.None);
			InGameData.VersionData.latestVersionNum = versionData[0];
			InGameData.VersionData.latestVersionName = versionData[1];
			InGameData.VersionData.latestVersionInfo = versionData[2];
			InGameData.VersionData.hotassetsUrl = versionData[3];
			InGameData.VersionData.reinstallVersionNum = versionData[4];
			DataManager.SaveTemperVersionData();
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static IEnumerator GetUserInfo(Action successAction, Action<string, bool> errorAction) {
		string url = "https://nfls.io/user/current";
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Authorization"] = "Bearer " + ACCESS_TOKEN;
		WWW www = new WWW(url, null, headers);
		yield return www;
		if (www.error == null) {
			JsonData json = JsonMapper.ToObject(www.text)["data"];
			Debug.Log(json.ToJson());
			UserManager.name = (string)json["username"];
			UserManager.email = (string)json["email"];
			UserManager.phone = (string)json["phone"];
			UserManager.id = (int)json["id"];
			try {
				UserManager.casHours = (double)json["point"];
			} catch {
				UserManager.casHours = (int)json["point"];
			}
			UserManager.isAdmin = (bool)json["admin"];
			DataManager.SaveUserData();
			if (!(bool)json["verified"]) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "You need to finish Real Name AUTH. to store your game rank!");
				Application.OpenURL("https://nfls.io/#/alumni/auth");
			}
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static IEnumerator GetHotAssets(Action downloadSuccessAction, Action saveSuccessAction, Action<string, bool> errorAction) {
		if (hotassetsWWW != null) {
			hotassetsWWW.Dispose();
		}
		hotassetsWWW = new WWW(InGameData.VersionData.hotassetsUrl);
		yield return hotassetsWWW;
		if (hotassetsWWW.error == null) {
			downloadSuccessAction.Invoke();
			InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Tip, "Writing Resources Into Disk, DO NOT quit the Game!", NotificationManager.DURATION_SHORT);
			DataManager.SaveHotAssets();
			DataManager.NewVersionDataFile();
			saveSuccessAction.Invoke();
		} else {
			bool losesConnection = false;
			if (hotassetsWWW.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(hotassetsWWW.error, losesConnection);
		}
	}

	public static IEnumerator GetRank(Action successAction, Action<string, bool> errorAction) {
		string url = "https://nfls.io/game/rank?game=1";
		WWW www = new WWW(url);
		yield return www;
		if (www.error == null) {
			Debug.Log(www.text);
			JsonData json = JsonMapper.ToObject(www.text)["data"];
			AnalyzeRankInfos(json);
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static IEnumerator PostScore(int time, Action successAction, Action<string, bool> errorAction) {
		string url = "https://nfls.io/game/rank?game=1";
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Authorization"] = "Bearer " + ACCESS_TOKEN;
		JsonData json = new JsonData();
		json["score"] = time;
		WWW www = new WWW(url, Encoding.UTF8.GetBytes(json.ToJson()), headers);
		yield return www;
		if (www.error == null) {
			json = JsonMapper.ToObject(www.text)["data"];
			AnalyzeRankInfos(json, true);
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	static void AnalyzeRankInfos(JsonData json) {
		AnalyzeRankInfos(json, false);
	}

	static void AnalyzeRankInfos(JsonData json, bool sorts) {
		InGameData.rankInfos = new List<InGameData.PlayerRankInfo>();
		for (int i = 0; i < json.Count; i++) {
			JsonData data = json[i];
			InGameData.PlayerRankInfo playerRankInfo = new InGameData.PlayerRankInfo();
			playerRankInfo.name = (string)data["user"]["username"];
			playerRankInfo.id = (int)data["user"]["id"];
			playerRankInfo.time = (int)data["score"];
			playerRankInfo.rank = (int)data["rank"];
			playerRankInfo.isAdmin = (bool)data["user"]["admin"];
			if (playerRankInfo.name.Equals(UserManager.name)) {
				InGameData.PlayerRankInfo.playerRank = playerRankInfo.rank;
				playerRankInfo.isPlayer = true;
			}
			InGameData.rankInfos.Add(playerRankInfo);
		}
		if (sorts) {
			InGameData.rankInfos.Sort(delegate (InGameData.PlayerRankInfo p1, InGameData.PlayerRankInfo p2) {
				return p1.time.CompareTo(p2.time);
			});
			int lastTime = -1;
			for (int i = 0; i < InGameData.rankInfos.Count; i++) {
				InGameData.PlayerRankInfo rankInfo = InGameData.rankInfos[i];
				if (rankInfo.time != lastTime) {
					rankInfo.rank = i + 1;
				} else {
					rankInfo.rank = InGameData.rankInfos[i - 1].rank;
				}
				if (rankInfo.name.Equals(UserManager.name)) {
					InGameData.PlayerRankInfo.playerRank = rankInfo.rank;
					rankInfo.isPlayer = true;
				}
			}
		}
		/*
		for (int i = 0; i < InGameData.rankInfos.Count; i++) {
			Debug.Log(InGameData.rankInfos[i].name);
		}
		*/
	}

	public static IEnumerator PostCasHours(double casHours, Action successAction, Action<string, bool> errorAction) {
		string url = "https://nfls.io/game/update";
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Authorization"] = "Bearer " + ACCESS_TOKEN;
		headers["Content-Type"] = "application/json";
		JsonData json = new JsonData();
		json["value"] = DataManager.Encrypt(Math.Round(casHours, 2).ToString());
		WWW www = new WWW(url, Encoding.UTF8.GetBytes(json.ToJson()), headers);
		yield return www;
		if (www.error == null) {
			InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.GainCasHours, "Reward " + Math.Round(casHours, 2) + " CAS Hours!", NotificationManager.DURATION_LONG);
			successAction.Invoke();
		} else {
			bool losesConnection = false;
			if (www.error.Equals(OFFLINE_SIGN)) {
				InGameData.notificationManager.NewNotification(NotificationManager.NotificationType.Warning, "Internet request failed due to offline.");
				losesConnection = true;
			}
			errorAction.Invoke(www.error, losesConnection);
		}
	}

	public static void PostLog(string message) {
		string url = "https://nfls.io/game/log";
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Authorization"] = "Bearer " + ACCESS_TOKEN;
		headers["Content-Type"] = "application/json";
		JsonData json = new JsonData();
		json["message"] = message;
		WWW www = new WWW(url, Encoding.UTF8.GetBytes(json.ToJson()), headers);
	}

	public static bool IsOffline() {
		if (offlineMode) {
			return true;
		}
		return Application.internetReachability == NetworkReachability.NotReachable;
	}

	public static bool ReachesInternet() {
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	public static bool UsesWifi() {
		return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
	}

	public static bool UsesData() {
		return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
	}

	public static int GetResponseCode(string error) {
		try {
			return int.Parse(error.Substring(0, error.IndexOf(" ")));
		} catch {
			return -1;
		}
	}

	public static void NullMethod() {

	}

	public static void NullMethod(string error, bool losesConnection) {

	}
}