using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CasHourManager : MonoBehaviour {

	public static int periodTime = 15;
	public static double periodReward = 0.25f;
	public static double gameWinReward = 1;
	public static double dailyRewardLimit = 3;
	public static double singleRewardLimit = 10;

	bool isRewarding;
	Queue<double> rewards = new Queue<double>();
	IEnumerator postCasHoursTask;

	void Start() {
		DontDestroyOnLoad(gameObject);
		InGameData.casHourManager = this;
	}

	public void StartPeriodRewardCheckTask() {
		Debug.Log("Start Period Reward Check Task !");
		AnalyticsManager.GameLaunchedEvent();
		StartCoroutine(ExePeriodRewardCheckTask());
	}

	public bool canReward(double hours) {
		if (hours > singleRewardLimit) {
			return false;
		}
		if (!IsSameDate(DateTime.Now, UserManager.rewardDate)) {
			UserManager.hourRewarded = 0d;
			UserManager.rewardDate = DateTime.Now;
		}
		if (hours + UserManager.hourRewarded > dailyRewardLimit) {
			return false;
		}
		return true;
	}

	public static bool IsSameDate(DateTime date1, DateTime date2) {
		return CompareDate(date1, date2) == 0;
	}

	public static int CompareDate(DateTime date1, DateTime date2) {
		if (date1.Year < date2.Year) {
			return -1;
		}
		if (date1.Year > date2.Year) {
			return 1;
		}
		if (date1.Month < date2.Month) {
			return -1;
		}
		if (date1.Month > date2.Month) {
			return 1;
		}
		if (date1.Day < date2.Day) {
			return -1;
		}
		if (date1.Day > date2.Day) {
			return 1;
		}
		return 0;
	}

	void AddReward(double hours) {
		Debug.Log("Add Reward !");
		rewards.Enqueue(hours);
		if (rewards.Count == 1) {
			Reward(hours);
		}
	}

	void Reward(double hours) {
		if (!isRewarding) {
			if (canReward(hours)) {
				Debug.Log("Can Reward !");
				isRewarding = true;
				postCasHoursTask = NetUtils.PostCasHours(hours, delegate {
					UserManager.hourRewarded += hours;
					UserManager.rewardDate = DateTime.Now;
					DataManager.SaveUserData();
					rewards.Dequeue();
					isRewarding = false;
					AnalyticsManager.PlayerRewardedEvent(hours);
					Debug.Log("Reward Request Finished !");
					if (rewards.Count > 0) {
						Debug.Log("Next Reward !");
						Reward(rewards.Peek());
					}
				}, delegate (string error, bool losesConnection) {
					if (!losesConnection) {
						int responseCode = NetUtils.GetResponseCode(error);
					}
					rewards.Dequeue();
					isRewarding = false;
					Debug.Log("Reward Request Failed !");
					if (rewards.Count > 0) {
						Debug.Log("Next Reward !");
						Reward(rewards.Peek());
					}
				});
				StartCoroutine(postCasHoursTask);
			}
		}
	}

	public void RewardGameWin() {
		Debug.Log("Reward Game Win !");
		AnalyticsManager.PlayerWinEvent();
		AddReward(gameWinReward);
	}

	public void LogOut() {
		isRewarding = false;
		postCasHoursTask = null;
		StopAllCoroutines();
	}

	IEnumerator ExePeriodRewardCheckTask() {
		while (true) {
			yield return new WaitForSeconds(periodTime * 60);
			Debug.Log("Reward Period !");
			AnalyticsManager.PlayerLongPlayEvent();
			AddReward(periodReward);
		}
	}
}
