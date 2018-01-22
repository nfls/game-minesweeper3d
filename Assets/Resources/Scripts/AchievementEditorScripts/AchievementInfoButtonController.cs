using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementInfoButtonController : MonoBehaviour
{
	public AchievementEditorController achievementEditorController;
	public AchievementManager.AchievementInfo achievementInfo;

	void Start()
	{
		transform.GetComponent<Button>().onClick.AddListener(OnClicked);
	}

	void Update()
	{

	}

	public void OnClicked()
	{
		achievementEditorController.LoadInputSection(achievementInfo);
	}
}
