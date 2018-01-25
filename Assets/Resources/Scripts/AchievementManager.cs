using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager {

	public static List<AchievementType> achievementTypes;
	public static List<AchievementInfo> achievementInfos;

	public static Dictionary<string, int> achievementProgresses;
	public static Dictionary<string, bool> achievementFinishedInfos;

	public static void Init() {

	}

	public static void InitData() {
		foreach (var item in achievementTypes) {
			achievementProgresses[item.ToString()] = 0;
		}
		foreach (var item in achievementInfos) {
			achievementFinishedInfos[item.ToString()] = false;
		}
	}

	public enum AchievementType {
		NumOfMinedBlock,
		NumOfSafeBlock,
		NumOfUnsafeBlock,
		NumOfGame,
		NumOfStandardGame,
		NumOfCustomGame,
		NumOfWin,
		NumOfLose,
		NumOfStandardWin,
		NumOfStandardLose,
		NumOfGiveUp,
		NumOfFinish,
		NumOfStandardGiveUp,
		NumOfStandardFinish,
		NumOfSkinPack,
		NumOfAudioPack,
		PlayerLevel
	}

	public class AchievementInfo {
		public AchievementType achievementType;
		public string title;
		public string description;
		public int successNum;
		public int expReward;
	}
}