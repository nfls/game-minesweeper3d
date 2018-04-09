using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System;
using Object = UnityEngine.Object;

public class PiplineBuilder : Editor {

	[MenuItem("Tool/Build Asset Bundle")]
	static void BuildAllAssetBundles() {
		Debug.Log("Build Process Started !");
		string dir = Application.dataPath + "/AssetBundles";
		if (!Directory.Exists(dir)) {
			Directory.CreateDirectory(dir);
		}
		BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
		Debug.Log("Build Process Finished !");
	}

	[MenuItem("Tool/BuildAssetBundle")]
	static void BuildAssetBundle() {
		AssetBundleBuild[] builds = new AssetBundleBuild[1];
		Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		string[] asset = new string[selects.Length];
		for (int i = 0; i < selects.Length; i++) {
			asset[i] = AssetDatabase.GetAssetPath(selects[i]);
			Debug.Log(asset[i]);
		}
		builds[0].assetNames = asset;
		BuildPipeline.BuildAssetBundles(Application.dataPath + "/AssetBundles", builds, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
		AssetDatabase.Refresh();
	}

	[MenuItem("Tool/SetAssetBundleNameExtension")] // 将选定的资源进行统一设置AssetBundle名
	static void SetBundleNameExtension() {
		Object[] selects = Selection.objects;
		foreach (Object item in selects) {
			string path = AssetDatabase.GetAssetPath(item);
			AssetImporter asset = AssetImporter.GetAtPath(path);
			asset.assetBundleName = item.name; // 设置Bundle文件的名称
			asset.assetBundleVariant = "asset"; // 设置Bundle文件的扩展名
			asset.SaveAndReimport();
		}
		AssetDatabase.Refresh();
	}

	[MenuItem("Tool/Export Std. Game Config.")]
	static void ExportStdGameConfig() {
		DataManager.InitCrypto();
		JsonData json = new JsonData();
		json["sMaxX"] = 7;
		json["sMaxY"] = 7;
		json["sMaxZ"] = 7;
		json["sMinesNum"] = 52;
		json["cPeriodTime"] = 15;
		json["cPeriodReward"] = 0.25d;
		json["cGameWinReward"] = 1d;
		json["cDailyRewardLimit"] = 3d;
		json["cSingleRewardLimit"] = 10d;
		DataManager.SaveDataToFile(Application.dataPath + "/Bundled Resources/Document@StdGameConfig.txt", json);
	}

	[MenuItem("Tool/Generate Test Json")]
	static void GenerateTestJson() {
		DataManager.InitCrypto();
		JsonData json = new JsonData();
		json["value"] = DataManager.Encrypt(Math.Round(2f, 2).ToString());
		File.WriteAllText(Application.dataPath + "/Bundled Resources/Document@TestJson.txt", json.ToJson());
	}
}
