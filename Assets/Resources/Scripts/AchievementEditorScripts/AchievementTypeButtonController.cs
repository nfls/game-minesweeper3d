using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementTypeButtonController : MonoBehaviour
{
	public AchievementEditorController achievementEditorController;
	public AchievementManager.AchievementType achievementType;

	void Start()
	{
		transform.GetComponent<Button>().onClick.AddListener(OnClicked);
	}

	void Update()
	{

	}

	public void OnClicked()
	{
		AchievementManager.AchievementInfo achievementInfo = new AchievementManager.AchievementInfo();
		achievementInfo.achievementType = achievementType;
		achievementInfo.title = "New Achievement Info";
		achievementEditorController.AddAchievementInfoButton(achievementInfo);
	}
}
