using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System;
using LitJson;

public class DataManager {

	public static string DATA_STORAGE_ROOT = Application.persistentDataPath + "/Data_Storage";
	public static string PURCHASEMENT_DATA_PATH = DATA_STORAGE_ROOT + "/pData";
	public static string USER_DATA_PATH = DATA_STORAGE_ROOT + "/uData";
	public static string ACHIEVEMENT_DATA_PATH = DATA_STORAGE_ROOT + "/aData";

	static byte[] KEY_ARRAY = UTF8Encoding.UTF8.GetBytes("fucknflsfucknflsfucknflsfucknfls");
	static RijndaelManaged rij;
	static ICryptoTransform encryptor;
	static ICryptoTransform decryptor;

	public static void Init() {
		rij = new RijndaelManaged();
		rij.Key = KEY_ARRAY;
		rij.Mode = CipherMode.ECB;
		rij.Padding = PaddingMode.PKCS7;
		encryptor = rij.CreateEncryptor();
		decryptor = rij.CreateDecryptor();



		if (Directory.Exists(DATA_STORAGE_ROOT)) {
			Directory.CreateDirectory(DATA_STORAGE_ROOT);
		} else {
			if (File.Exists(PURCHASEMENT_DATA_PATH)) {

			} else {
				GeneratePurchasementDataFile();
			}
			if (File.Exists(USER_DATA_PATH)) {

			} else {
				GenerateUserDataFile();
			}
			if (File.Exists(ACHIEVEMENT_DATA_PATH)) {

			} else {
				GenerateAchievementDataFile();
			}
		}
	}

	static void GeneratePurchasementDataFile() {
		PurchasementManager.InitData();
		SavePurchasementData();
	}

	static void GenerateUserDataFile() {
		UserManager.name = "Guest";
		UserManager.level = 0;
		UserManager.exp = 0;
		SaveUserData();
	}

	static void GenerateAchievementDataFile() {
		AchievementManager.InitData();
		SaveAchievementData();
	}

	static void SavePurchasementData() {
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

	static void SaveUserData() {
		JsonData json = new JsonData();
		json["name"] = UserManager.name;
		json["level"] = UserManager.level;
		json["exp"] = UserManager.exp;
		SaveDataToFile(USER_DATA_PATH, json);
	}

	static void SaveAchievementData() {
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

	static void SaveDataToFile(string path, JsonData json) {
		File.WriteAllText(path, Encrypt(json.ToJson()));
	}

	static string Encrypt(string toEncrypt) {
		byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
		byte[] encryptedArray = encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
		return Convert.ToBase64String(encryptedArray, 0, encryptedArray.Length);
	}

	static string Decrypt(string toDecrypt) {
		byte[] toDecryptArray = Convert.FromBase64String(toDecrypt);
		byte[] decryptedArray = decryptor.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
		return UTF8Encoding.UTF8.GetString(decryptedArray);
	}
}