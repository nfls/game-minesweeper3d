using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class AchievementEditorController : MonoBehaviour
{
	GameObject leftSection;
	GameObject rightSection;
	GameObject bottomSection;
	GameObject scrollView;
	GameObject popupDialog;

	Text typeLabel;
	InputField titleInput;
	InputField descriptionInput;
	InputField successNumInput;
	InputField expRewardInput;

	public List<AchievementManager.AchievementType> achievementTypes;
	public List<AchievementManager.AchievementInfo> achievementInfos;
	public List<AchievementTypeButtonController> achievementTypeButtonControllers;
	public List<AchievementInfoButtonController> achievementInfoButtonControllers;

	AchievementManager.AchievementInfo currentAchievementInfo;

	string filePath;

	GameObject achievementTypeButtonPrototype;
	GameObject achievementInfoButtonPrototype;

	void Start()
	{

		achievementTypeButtonPrototype = Resources.Load<GameObject>("Prefabs/AchievementEditorPrefabs/Button AchievementType");
		achievementInfoButtonPrototype = Resources.Load<GameObject>("Prefabs/AchievementEditorPrefabs/Button AchievementInfo");

		leftSection = GameObject.Find("Left Section");
		rightSection = GameObject.Find("Right Section");
		bottomSection = GameObject.Find("Bottom Section");
		scrollView = leftSection.transform.Find("Achievement Scroll View").gameObject;
		popupDialog = GameObject.Find("Popup Dialog");
		popupDialog.SetActive(false);

		typeLabel = rightSection.transform.Find("Panel/Type Label/Text").GetComponent<Text>();
		titleInput = rightSection.transform.Find("Panel/Title Panel/InputField").GetComponent<InputField>();
		descriptionInput = rightSection.transform.Find("Panel/Description Panel/InputField").GetComponent<InputField>();
		successNumInput = rightSection.transform.Find("Panel/SuccessNum Panel/InputField").GetComponent<InputField>();
		expRewardInput = rightSection.transform.Find("Panel/ExpReward Panel/InputField").GetComponent<InputField>();

		ClearAndDisableInputSection();

		filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/Data_Storage.json";
		achievementTypes = new List<AchievementManager.AchievementType>();
		string[] names = Enum.GetNames(typeof(AchievementManager.AchievementType));
		for (int i = 0; i < names.Length; i++) {
			achievementTypes.Add((AchievementManager.AchievementType)Enum.Parse(typeof(AchievementManager.AchievementType), names[i]));
			AddAchievementTypeButton(achievementTypes.Last());
		}
		achievementInfos = new List<AchievementManager.AchievementInfo>();
		if (File.Exists(filePath)) {
			JsonData jsonData = new JsonData();
			jsonData.SetJsonType(JsonType.Array);
			jsonData = JsonMapper.ToObject(File.ReadAllText(filePath));
			for (int i = 0; i < jsonData.Count; i++) {
				AchievementManager.AchievementType type = (AchievementManager.AchievementType)Enum.Parse(typeof(AchievementManager.AchievementType), jsonData[i]["type"].ToString());
				if (jsonData[i]["achievements"].IsArray) {
					JsonData achievementsData = jsonData[i]["achievements"];
					for (int j = 0; j < achievementsData.Count; j++) {
						JsonData achievementData = achievementsData[j];
						AchievementManager.AchievementInfo achievementInfo = new AchievementManager.AchievementInfo();
						achievementInfo.achievementType = type;
						achievementInfo.title = (string)achievementData["title"];
						achievementInfo.description = (string)achievementData["description"];
						achievementInfo.successNum = (int)achievementData["successNum"];
						achievementInfo.expReward = (int)achievementData["expReward"];
						AddAchievementInfoButton(achievementInfo);
					}
				}
			}
		} else {
			JsonData jsonData = new JsonData();
			jsonData.SetJsonType(JsonType.Array);
			for (int i = 0; i < achievementTypes.Count; i++) {
				JsonData data = new JsonData();
				data["type"] = achievementTypes[i].ToString();
				jsonData.Add(data);
			}
			string jsonStr = JsonMapper.ToJson(jsonData);
			File.WriteAllText(filePath, jsonStr);
		}
	}

	void AddAchievementTypeButton(AchievementManager.AchievementType type)
	{
		GameObject content = scrollView.transform.Find("Viewport/Content").gameObject;
		GameObject button = Instantiate(achievementTypeButtonPrototype);
		AchievementTypeButtonController buttonController = button.GetComponent<AchievementTypeButtonController>();
		achievementTypeButtonControllers.Add(buttonController);
		buttonController.achievementType = type;
		buttonController.achievementEditorController = this;
		button.name = "Button " + type;
		Text text = button.transform.Find("Text").GetComponent<Text>();
		text.text = type.ToString();
		button.transform.SetParent(content.transform);
	}

	public void AddAchievementInfoButton(AchievementManager.AchievementInfo info)
	{
		achievementInfos.Add(info);
		GameObject content = scrollView.transform.Find("Viewport/Content").gameObject;
		GameObject button = Instantiate(achievementInfoButtonPrototype);
		AchievementInfoButtonController buttonController = button.GetComponent<AchievementInfoButtonController>();
		buttonController.achievementInfo = info;
		buttonController.achievementEditorController = this;
		achievementInfoButtonControllers.Add(buttonController);
		string typeStr = info.achievementType.ToString();
		Text text = button.transform.Find("Text").GetComponent<Text>();
		text.text = info.title;
		List<GameObject> buttons = new List<GameObject>();
		for (int i = 0; i < content.transform.childCount; i++) {
			buttons.Add(content.transform.GetChild(i).gameObject);
			if (buttons[i].name == "Button " + typeStr) {
				buttons.Add(button);
			}
		}
		RearrangeButtonsToList(buttons, content);
		LoadInputSection(info);
	}

	public void RemoveAchievementInfoButton(AchievementInfoButtonController buttonController)
	{
		achievementInfoButtonControllers.Remove(buttonController);
		achievementInfos.Remove(buttonController.achievementInfo);
		buttonController.gameObject.SetActive(false);
		Destroy(buttonController.gameObject);
		ClearAndDisableInputSection();
	}

	void RearrangeButtonsToList(List<GameObject> buttons, GameObject parent)
	{
		parent.transform.DetachChildren();
		for (int i = 0; i < buttons.Count; i++) {
			buttons[i].transform.SetParent(parent.transform);
		}
	}

	public void LoadInputSection(AchievementManager.AchievementInfo achievementInfo)
	{
		typeLabel.text = achievementInfo.achievementType.ToString();
		titleInput.text = achievementInfo.title;
		descriptionInput.text = achievementInfo.description;
		successNumInput.text = achievementInfo.successNum.ToString();
		expRewardInput.text = achievementInfo.expReward.ToString();
		rightSection.GetComponent<CanvasGroup>().interactable = true;
		currentAchievementInfo = achievementInfo;
	}

	void ClearAndDisableInputSection()
	{
		typeLabel.text = "Achievement Type";
		titleInput.text = "";
		descriptionInput.text = "";
		successNumInput.text = "";
		expRewardInput.text = "";
		rightSection.GetComponent<CanvasGroup>().interactable = false;
	}

	public void OnRevertButtonClicked()
	{
		LoadInputSection(currentAchievementInfo);
	}

	public void OnDeleteButtonClicked()
	{
		foreach (AchievementInfoButtonController buttonController in achievementInfoButtonControllers) {
			if (buttonController.achievementInfo == currentAchievementInfo) {
				currentAchievementInfo = null;
				RemoveAchievementInfoButton(buttonController);
				break;
			}
		}
	}

	public void OnConfirmButtonClicked()
	{
		currentAchievementInfo.title = titleInput.text;
		currentAchievementInfo.description = descriptionInput.text;
		int successNum = 0;
		if (successNumInput.text != "") {
			successNum = int.Parse(successNumInput.text);
			if (successNum < 0) {
				successNum = 0;
			}
		}
		currentAchievementInfo.successNum = successNum;
		int expReward = 0;
		if (expRewardInput.text != "") {
			expReward = int.Parse(expRewardInput.text);
			if (expReward < 0) {
				expReward = 0;
			}
		}
		currentAchievementInfo.expReward = expReward;
		foreach (AchievementInfoButtonController buttonController in achievementInfoButtonControllers) {
			if (buttonController.achievementInfo == currentAchievementInfo) {
				buttonController.transform.Find("Text").GetComponent<Text>().text = currentAchievementInfo.title;
				break;
			}
		}
		LoadInputSection(currentAchievementInfo);
	}

	public void OnExitButtonClicked()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void OnSaveButtonClicked()
	{
		leftSection.GetComponent<CanvasGroup>().interactable = false;
		rightSection.GetComponent<CanvasGroup>().interactable = false;
		bottomSection.GetComponent<CanvasGroup>().interactable = false;

		StartCoroutine(DoSavingAnimation());

		Save();

		StopAllCoroutines();
		popupDialog.SetActive(false);

		leftSection.GetComponent<CanvasGroup>().interactable = true;
		rightSection.GetComponent<CanvasGroup>().interactable = true;
		bottomSection.GetComponent<CanvasGroup>().interactable = true;
	}

	IEnumerator DoSavingAnimation()
	{
		popupDialog.SetActive(true);
		Text text = popupDialog.transform.Find("Text").GetComponent<Text>();
		text.text = "Saving ......\n\nPlease Wait.\\";
		bool left = false;
		while (true) {
			yield return new WaitForSeconds(0.3f);
			string oldValue;
			string newValue;
			if (left) {
				oldValue = "/";
				newValue = "\\";
			} else {
				oldValue = "\\";
				newValue = "/";
			}
			text.text = text.text.Replace(oldValue, newValue);
			left = !left;
		}
	}

	void Save()
	{
		JsonData jsonData = new JsonData();
		jsonData.SetJsonType(JsonType.Array);
		for (int i = 0; i < achievementTypes.Count; i++) {
			JsonData data = new JsonData();
			data["type"] = achievementTypes[i].ToString();
			JsonData achievementsData = new JsonData();
			achievementsData.SetJsonType(JsonType.Array);
			List<AchievementManager.AchievementInfo> infos = new List<AchievementManager.AchievementInfo>();
			foreach (AchievementManager.AchievementInfo info in achievementInfos) {
				if (info.achievementType == achievementTypes[i]) {
					infos.Add(info);
				}
			}
			infos.Sort(delegate (AchievementManager.AchievementInfo a, AchievementManager.AchievementInfo b) {
				return a.successNum.CompareTo(b.successNum);
			});
			for (int j = 0; j < infos.Count; j++) {
				JsonData achievementData = new JsonData();
				AchievementManager.AchievementInfo info = infos[j];
				if (info.title == null) {
					info.title = "null";
				}
				if (info.description == null) {
					info.description = "null";
				}
				achievementData["title"] = info.title;
				achievementData["description"] = info.description;
				achievementData["successNum"] = info.successNum;
				achievementData["expReward"] = info.expReward;
				achievementsData.Add(achievementData);
			}
			data["achievements"] = achievementsData;
			jsonData.Add(data);
		}
		string jsonStr = JsonMapper.ToJson(jsonData);
		File.WriteAllText(filePath, jsonStr);
	}
}