using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public static class InGameData {
	public static int maxX = 10;
	public static int maxY = 10;
	public static int maxZ = 10;
	public static int minesNum = 100;
	public static bool isStandardGame = false;
	public static bool audioEnabled = true;
	public static NotificationManager notificationManager;
	public static MenuSceneIntent menuSceneIntent = MenuSceneIntent.Main;

	public static void SetStandardModeData() {
		maxX = 10;
		maxY = 10;
		maxZ = 10;
		minesNum = 100;
		isStandardGame = true;
	}

	public static class VersionData {
		public static string versionNum;
		public static string versionName;
		public static string versionInfo;

		public static void Init() {
			string dir = Application.dataPath + "/AssetBundles/hotassets";
			AssetBundle hotassets = AssetBundle.LoadFromFile(dir, 0);
			string[] names = hotassets.GetAllAssetNames();
			string name = "";
			foreach (string str in names) {
				if (str.Contains("version")) {
					name = str;
					break;
				}
			}
			TextAsset doc = hotassets.LoadAsset<TextAsset>(name);
			JsonData jsonData = JsonMapper.ToObject(doc.text);
			jsonData.SetJsonType(JsonType.Object);
			hotassets.Unload(true);
			versionNum = (string)jsonData["versionNum"];
			versionName = (string)jsonData["versionName"];
			versionInfo = (string)jsonData["versionInfo"];
		}
	}

	public enum MenuSceneIntent {
		Main, Play, About, Custom, Option
	}
}
