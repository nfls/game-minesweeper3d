using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager {

	public static string name;
	public static int level;
	public static int exp;
	public static int casHours;

	public static void InitData() {
		name = "Guest";
		level = 0;
		exp = 0;
	}
}
