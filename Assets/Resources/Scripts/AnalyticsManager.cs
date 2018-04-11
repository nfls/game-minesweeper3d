using System;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class AnalyticsManager {

	public static bool GameLaunchedEvent() {
		return CustomEvent("gameLaunched");
	}

	public static bool PlayerLongPlayEvent() {
		return CustomEvent("playerLongPlay", "period", CasHourManager.periodTime + "min");
	}

	public static bool StandardModePlayedEvent() {
		return CustomEvent("standardModePlayed", new Dictionary<string, object>() { { "x", InGameData.sMaxZ }, { "y", InGameData.sMaxY }, { "z", InGameData.sMaxZ } });
	}

	public static bool CustomModePlayedEvent() {
		return CustomEvent("customdModePlayed", new Dictionary<string, object>() { { "x", InGameData.maxX }, { "y", InGameData.maxY }, { "z", InGameData.maxZ } });
	}

	public static bool PlayerWinEvent() {
		return CustomEvent("playerWin");
	}

	public static bool PlayerRewardedEvent(double reward) {
		return CustomEvent("playerRewarded", "reward", Math.Round(reward, 2) + "Cas Hrs");
	}

	public static bool CustomEvent(string eventName) {
		return CustomEvent(eventName, new Dictionary<string, object>());
	}

	public static bool CustomEvent(string eventName, string paramName, object paramValue) {
		return CustomEvent(eventName, new Dictionary<string, object>() { { paramName, paramValue } });
	}

	public static bool CustomEvent(string eventName, Dictionary<string, object> paramList) {
		paramList["userId"] = UserManager.id;
		paramList["userName"] = UserManager.name;
		paramList["userLevel"] = UserManager.level;
		paramList["isAdmin"] = UserManager.isAdmin;
		paramList["userPhone"] = UserManager.phone;
		paramList["userEmail"] = UserManager.email;
		paramList["eventDate"] = DateTime.Now.ToString();
		string message = string.Empty;
		foreach (var param in paramList) {
			message += param.Key + " = " + param.Value + "\n";
		}
		message = message.Substring(0, message.Length - 1);
		NetUtils.PostLog(message);
		return Analytics.CustomEvent(eventName, paramList) == AnalyticsResult.Ok;
	}
}
