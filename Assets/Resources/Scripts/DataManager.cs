using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System;
using LitJson;

public class DataManager {
	public static readonly string DATA_STORAGE_ROOT = Application.persistentDataPath + "/Data_Storage";
	public static readonly string PURCHASEMENT_DATA_PATH = DATA_STORAGE_ROOT + "/pData";
	public static readonly string USER_DATA_PATH = DATA_STORAGE_ROOT + "/uData";
	public static readonly string ACHIEVEMENT_DATA_PATH = DATA_STORAGE_ROOT + "/aData";
	public static readonly string NET_DATA_PATH = DATA_STORAGE_ROOT + "/nData";
	public static readonly string VERSION_DATA_PATH = DATA_STORAGE_ROOT + "/vData";
	public static readonly string TEMPER_VERSION_DATA_PATH = DATA_STORAGE_ROOT + "/vData_temper";
	public static readonly string ASSETS_ROOT_PATH = Application.persistentDataPath + "/AssetBundles";
	public static readonly string HOT_ASSETS_PATH = ASSETS_ROOT_PATH + "/hotassets";
	public static readonly string TEMPER_HOT_ASSETS_PATH = ASSETS_ROOT_PATH + "/hotassets_temper";

	public static bool hasNoLocalHotAssets {
		get {
			return !File.Exists(HOT_ASSETS_PATH);
		}
	}

	static byte[] KEY_ARRAY = Encoding.UTF8.GetBytes("fucknflsfucknflsfucknflsfucknfls");
	static RijndaelManaged rij;
	static ICryptoTransform encryptor;
	static ICryptoTransform decryptor;

	public static void Init() {

		InitCrypto();

		if (!Directory.Exists(DATA_STORAGE_ROOT)) {
			Directory.CreateDirectory(DATA_STORAGE_ROOT);
		}

		if (!Directory.Exists(ASSETS_ROOT_PATH)) {
			Directory.CreateDirectory(ASSETS_ROOT_PATH);
		}

		if (File.Exists(PURCHASEMENT_DATA_PATH)) {
			LoadPurchasementData();
		} else {
			GeneratePurchasementDataFile();
		}
		if (File.Exists(USER_DATA_PATH)) {
			LoadUserData();
		} else {
			GenerateUserDataFile();
		}
		if (File.Exists(ACHIEVEMENT_DATA_PATH)) {
			LoadAchievementData();
		} else {
			GenerateAchievementDataFile();
		}
		if (File.Exists(NET_DATA_PATH)) {
			LoadNetData();
		} else {
			GenerateNetDataFile();
		}
		if (File.Exists(VERSION_DATA_PATH)) {
			LoadVersionData();
		}
	}

	public static void InitCrypto() {
		rij = new RijndaelManaged();
		rij.Key = KEY_ARRAY;
		rij.Mode = CipherMode.ECB;
		rij.Padding = PaddingMode.PKCS7;
		encryptor = rij.CreateEncryptor();
		decryptor = rij.CreateDecryptor();
	}

	static void GeneratePurchasementDataFile() {
		PurchasementManager.Init();
		SavePurchasementData();
	}

	static void GenerateUserDataFile() {
		UserManager.Init();
		SaveUserData();
	}

	static void GenerateAchievementDataFile() {
		AchievementManager.Init();
		SaveAchievementData();
	}

	static void GenerateNetDataFile() {
		SaveNetData();
	}

	public static void NewVersionDataFile() {
		if (File.Exists(VERSION_DATA_PATH)) {
			File.Delete(VERSION_DATA_PATH);
		}
		File.Move(TEMPER_VERSION_DATA_PATH, VERSION_DATA_PATH);
		LoadVersionData();
		ClearTemperData();
	}

	public static void SavePurchasementData() {
		JsonData json = new JsonData();
		JsonData skinJsonArray = new JsonData();
		skinJsonArray.SetJsonType(JsonType.Array);
		foreach (var item in PurchasementManager.skinPackAvailabilities) {
			JsonData itemJson = new JsonData();
			itemJson["name"] = item.Key;
			itemJson["available"] = item.Value;
			skinJsonArray.Add(itemJson);
		}
		JsonData audioJsonArray = new JsonData();
		audioJsonArray.SetJsonType(JsonType.Array);
		foreach (var item in PurchasementManager.audioPackAvailabilities) {
			JsonData itemJson = new JsonData();
			itemJson["name"] = item.Key;
			itemJson["available"] = item.Value;
			audioJsonArray.Add(itemJson);
		}
		json["skinPacks"] = skinJsonArray;
		json["audioPacks"] = audioJsonArray;
		SaveDataToFile(PURCHASEMENT_DATA_PATH, json);
	}

	public static void SaveUserData() {
		JsonData json = new JsonData();
		json["name"] = UserManager.name;
		json["level"] = UserManager.level;
		json["exp"] = UserManager.exp;
		SaveDataToFile(USER_DATA_PATH, json);
	}

	public static void SaveAchievementData() {
		JsonData json = new JsonData();
		JsonData progressJsonArray = new JsonData();
		progressJsonArray.SetJsonType(JsonType.Array);
		foreach (var item in AchievementManager.achievementProgresses) {
			JsonData itemJson = new JsonData();
			itemJson["type"] = item.Key;
			itemJson["progress"] = item.Value;
			progressJsonArray.Add(itemJson);
		}
		JsonData finishedInfoJsonArray = new JsonData();
		finishedInfoJsonArray.SetJsonType(JsonType.Array);
		foreach (var item in AchievementManager.achievementFinishedInfos) {
			JsonData itemJson = new JsonData();
			itemJson["type"] = item.Key;
			itemJson["finished"] = item.Value;
			finishedInfoJsonArray.Add(itemJson);
		}
		json["progresses"] = progressJsonArray;
		json["finishedInfos"] = finishedInfoJsonArray;
		SaveDataToFile(ACHIEVEMENT_DATA_PATH, json);
	}

	public static void SaveNetData() {
		JsonData json = new JsonData();
		json["refreshToken"] = NetUtils.REFRESH_TOKEN;
		json["accessToken"] = NetUtils.ACCESS_TOKEN;
		SaveDataToFile(NET_DATA_PATH, json);
	}

	public static void SaveTemperVersionData() {
		JsonData json = new JsonData();
		json["versionNum"] = InGameData.VersionData.latestVersionNum;
		json["versionName"] = InGameData.VersionData.latestVersionName;
		json["versionInfo"] = InGameData.VersionData.latestVersionInfo;
		SaveDataToFile(TEMPER_VERSION_DATA_PATH, json);
	}

	public static void SaveHotAssets() {
		SaveDataToFile(HOT_ASSETS_PATH, NetUtils.hotassetsWWW.bytes);
	}

	public static void SaveDataToFile(string path, JsonData json) {
		File.WriteAllText(path, Encrypt(json.ToJson()));
	}

	public static void SaveDataToFile(string path, byte[] bytes) {
		File.WriteAllBytes(path, bytes);
	}

	public static void ClearTemperData() {
		if (File.Exists(TEMPER_HOT_ASSETS_PATH)) {
			File.Delete(TEMPER_HOT_ASSETS_PATH);
		}
		if (File.Exists(TEMPER_VERSION_DATA_PATH)) {
			File.Delete(TEMPER_VERSION_DATA_PATH);
		}
	}

	public static void LoadPurchasementData() {
		JsonData json = ReadDataFromFile(PURCHASEMENT_DATA_PATH);
	}

	public static void LoadUserData() {
		JsonData json = ReadDataFromFile(USER_DATA_PATH);
		UserManager.name = (string)json["name"];
		UserManager.level = (int)json["level"];
		UserManager.exp = (int)json["exp"];
	}

	public static void LoadAchievementData() {
		JsonData json = ReadDataFromFile(ACHIEVEMENT_DATA_PATH);
	}

	public static void LoadNetData() {
		JsonData json = ReadDataFromFile(NET_DATA_PATH);
		NetUtils.REFRESH_TOKEN = (string)json["refreshToken"];
		NetUtils.ACCESS_TOKEN = (string)json["accessToken"];
	}

	public static void LoadVersionData() {
		JsonData json = ReadDataFromFile(VERSION_DATA_PATH);
		InGameData.VersionData.versionNum = (string)json["versionNum"];
		InGameData.VersionData.versionName = (string)json["versionName"];
		InGameData.VersionData.versionInfo = (string)json["versionInfo"];
	}

	public static JsonData ReadDataFromFile(string path) {
		return JsonMapper.ToObject(Decrypt(File.ReadAllText(path)));
	}

	public static string Encrypt(string toEncrypt) {
		byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
		byte[] encryptedArray = encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
		return Convert.ToBase64String(encryptedArray, 0, encryptedArray.Length);
	}

	public static string Decrypt(string toDecrypt) {
		byte[] toDecryptArray = Convert.FromBase64String(toDecrypt);
		byte[] decryptedArray = decryptor.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
		return Encoding.UTF8.GetString(decryptedArray);
	}
}