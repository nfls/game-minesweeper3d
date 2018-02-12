using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchasementManager {

	public static List<SkinPackInfo> skinPackInfos;
	public static List<AudioPackInfo> audioPackInfos;

	public static Dictionary<string, bool> skinPackAvailabilities;
	public static Dictionary<string, bool> audioPackAvailabilities;

	public static void Init() {
		skinPackInfos = new List<SkinPackInfo>();
		audioPackInfos = new List<AudioPackInfo>();
		skinPackAvailabilities = new Dictionary<string, bool>();
		audioPackAvailabilities = new Dictionary<string, bool>();
		foreach (var item in skinPackInfos) {
			skinPackAvailabilities[item.name] = false;
		}
		foreach (var item in audioPackInfos) {
			audioPackAvailabilities[item.name] = false;
		}
	}

	public class SkinPackInfo {
		public string name;
		public string description;
		public int levelRequired;
		public int price;
	}

	public class AudioPackInfo {
		public string name;
		public string description;
		public int levelRequired;
		public int price;
	}
}
