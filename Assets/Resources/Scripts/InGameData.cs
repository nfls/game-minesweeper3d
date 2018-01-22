using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InGameData
{
	public static int maxX = 10;
	public static int maxY = 10;
	public static int maxZ = 10;
	public static int minesNum = 100;
	public static bool isStandardGame = false;
	public static bool audioEnabled = true;
	public static NotificationManager notificationManager;
	public static MenuSceneIntent menuSceneIntent = MenuSceneIntent.Main;

	public static void SetStandardModeData()
	{
		maxX = 10;
		maxY = 10;
		maxZ = 10;
		minesNum = 100;
		isStandardGame = true;
	}

	public enum MenuSceneIntent
	{
		Main, Play, About, Custom
	}
}
