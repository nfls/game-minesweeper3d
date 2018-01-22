using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
	private List<AchievementInfo> achievements;
	private TextAsset achievementsInfo;

	public void Init()
	{

	}

	public void Notify(AchievementType achievementType)
	{

	}

	public enum AchievementType
	{
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

	public class AchievementInfo
	{
		public AchievementType achievementType;
		public string title;
		public string description;
		public int successNum;
		public int expReward;
	}
}