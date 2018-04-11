using UnityEngine;
using System;

public class UserManager {
	public static string name;
	public static string email;
	public static string phone;
	public static int id;
	public static int level;
	public static int exp;
	public static double casHours;
	public static double hourRewarded;
	public static DateTime rewardDate;
	public static bool isAdmin;

	public static void Init() {
		name = "Guest";
		email = "Null";
		phone = "Null";
		id = -1;
		level = 0;
		exp = 0;
		casHours = -1;
		hourRewarded = 0d;
		rewardDate = DateTime.Now;
		isAdmin = false;
	}

	public static void LogOut() {
		name = "Guest";
		email = "Null";
		phone = "Null";
		id = -1;
		level = 0;
		exp = 0;
		casHours = -1;
		isAdmin = false;
		DataManager.SaveUserData();
	}

	public static void PrintData() {
		Debug.Log("Name = " + name);
		Debug.Log("Phone = " + phone);
		Debug.Log("Email = " + email);
		Debug.Log("ID = " + id);
		Debug.Log("CAS Hours = " + casHours);
		Debug.Log("CAS Hours Rewarded = " + hourRewarded);
		Debug.Log("Is Admin = " + isAdmin);
	}
}
