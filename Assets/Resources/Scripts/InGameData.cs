using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class InGameData {
	public static int maxX;
	public static int maxY;
	public static int maxZ;
	public static int minesNum;
	public static int sMaxX;
	public static int sMaxY;
	public static int sMaxZ;
	public static int sMinesNum;
	public static bool isStandardGame {
		get {
			if (maxX == sMaxX && maxY == sMaxY && maxZ == sMaxZ && minesNum == sMinesNum) {
				return true;
			}
			return false;
		}
	}
	public static bool audioEnabled = true;
	public static NotificationManager notificationManager;
	public static CasHourManager casHourManager;
	public static MenuSceneIntent menuSceneIntent = MenuSceneIntent.Main;
	public static List<PlayerRankInfo> rankInfos;

	public static void Init() {
		VersionData.Init();
	}

	public static void SetStandardModeData() {
		maxX = sMaxX;
		maxY = sMaxY;
		maxZ = sMaxZ;
		minesNum = sMinesNum;
		/*
		maxX = 3;
		maxY = 3;
		maxZ = 3;
		minesNum = 1;
		*/
	}

	public static class VersionData {

		public static string[] splitSigns = { "." };

		public static string versionNum = "Null";
		public static string versionName = "Null";
		public static string versionInfo = "Null";

		public static string latestVersionNum = "Null";
		public static string latestVersionName = "Null";
		public static string latestVersionInfo = "Null";

		public static string reinstallVersionNum = "Null";

		public static string hotassetsUrl = "Null";

		public static bool needsUpdate {
			get {
				if (versionNum.Equals("Null")) {
					return true;
				}
				if (latestVersionNum.Equals("Null")) {
					return false;
				}
				string[] numStrings = versionNum.Split(splitSigns, System.StringSplitOptions.None);
				int[] nums = new int[3];
				for (int i = 0; i < nums.Length; i++) {
					nums[i] = int.Parse(numStrings[i]);
				}
				numStrings = latestVersionNum.Split(splitSigns, System.StringSplitOptions.None);
				int[] latestNums = new int[3];
				for (int i = 0; i < latestNums.Length; i++) {
					latestNums[i] = int.Parse(numStrings[i]);
				}
				if (nums[0] < latestNums[0]) {
					return true;
				}
				if (nums[1] < latestNums[1]) {
					return true;
				}
				if (nums[2] < latestNums[2]) {
					return true;
				}
				if (versionInfo.Equals("Alpha")) {
					if (latestVersionInfo.Equals("Beta")) {
						return true;
					}
				}
				return false;
			}
		}

		public static bool needsReinstall {
			get {
				if (versionNum.Equals("Null")) {
					return true;
				}
				if (reinstallVersionNum.Equals("Null")) {
					return false;
				}
				string[] numStrings = versionNum.Split(splitSigns, System.StringSplitOptions.None);
				int[] nums = new int[3];
				for (int i = 0; i < nums.Length; i++) {
					nums[i] = int.Parse(numStrings[i]);
				}
				numStrings = versionNum.Split(splitSigns, System.StringSplitOptions.None);
				int[] reinstallNums = new int[3];
				for (int i = 0; i < reinstallNums.Length; i++) {
					reinstallNums[i] = int.Parse(numStrings[i]);
				}
				if (nums[0] < reinstallNums[0]) {
					return true;
				}
				if (nums[1] < reinstallNums[1]) {
					return true;
				}
				if (nums[2] < reinstallNums[2]) {
					return true;
				}
				return false;
			}
		}

		public static void Init() {
			if (!File.Exists(DataManager.VERSION_DATA_PATH)) {
				versionNum = "Null";
				versionName = "Null";
				versionInfo = "Null";
			}
			/*
			JsonData jsonData = JsonMapper.ToObject(Resources.Load<TextAsset>("Documents/VersionData").text);
			jsonData.SetJsonType(JsonType.Object);
			versionNum = (string)jsonData["versionNum"];
			versionName = (string)jsonData["versionName"];
			versionInfo = (string)jsonData["versionInfo"];
			*/
		}

		public static void PrintData() {
			Debug.Log("Version Num = " + versionNum);
			Debug.Log("Version Name = " + versionName);
			Debug.Log("Version Info = " + versionInfo);
			Debug.Log("Latest Version Num = " + latestVersionNum);
			Debug.Log("Latest Version Name = " + latestVersionName);
			Debug.Log("Latest Version Info = " + latestVersionInfo);
			Debug.Log("Needs Update = " + needsUpdate);
			Debug.Log("Needs Reinstall = " + needsReinstall);
			Debug.Log("Hot Assets Url = " + hotassetsUrl);
		}
	}

	public class PlayerRankInfo {
		public static int playerRank;

		public string name;
		public int id;
		public int time;
		public int rank;
		public bool isAdmin;
		public bool isPlayer;
	}

	public enum MenuSceneIntent {
		Main, Play, About, Custom, Option
	}
}