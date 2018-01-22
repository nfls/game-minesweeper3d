using UnityEngine;
using System.Collections;
using System;

public class UserManager : MonoBehaviour
{
	void Start()
	{

	}

	void Update()
	{

	}

	public static string GetPlayerName()
	{
		return PlayerPrefs.GetString("playerName", "Unknown");
	}

	public static int GetPlayerLevel()
	{
		return PlayerPrefs.GetInt("playerLevel", 1);
	}

	public static int GetPlayerExp()
	{
		return PlayerPrefs.GetInt("playerExp", 0);
	}

	public static int GetPlayerExpFull()
	{
		return (int)(GetPlayerLevel() * 1.5 + 100);
	}

	public static void ClearAll()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	public static void SetPlayername(string playerName)
	{
		PlayerPrefs.SetString("playerName", playerName);
		PlayerPrefs.Save();
	}

	public static void OnPlayerGainExp(int exp)
	{
		int expRemain = exp + GetPlayerExp() - GetPlayerExpFull();
		if (expRemain < 0) {
			PlayerPrefs.SetInt("playerExp", exp + GetPlayerExp());
		} else {
			OnPlayerLevelUp();
			PlayerPrefs.SetInt("playerExp", expRemain);
		}
	}

	public static void OnPlayerLevelUp()
	{
		PlayerPrefs.SetInt("playerLevel", GetPlayerLevel() + 1);
		PlayerPrefs.SetInt("playerExp", 0);
	}

	public static void SetPreferredNumColorSet(NumColorSet numColorSet)
	{
		PlayerPrefs.SetString("numColorSet", numColorSet.ToString());
	}

	public static NumColorSet GetPreferredNumColorSet()
	{
		return (NumColorSet)Enum.Parse(typeof(NumColorSet), PlayerPrefs.GetString("numColorSet", NumColorSet.Default.ToString()));
	}

	public static void SetPreferredNumFont(NumFont numFont)
	{
		PlayerPrefs.SetString("numFont", numFont.ToString());
	}

	public static NumFont GetPreferredNumFont()
	{
		return (NumFont)Enum.Parse(typeof(NumFont), PlayerPrefs.GetString("numFont", NumFont.Futura.ToString()));
	}

	public static void SetPreferredBackgroundColor(Color color)
	{
		string colorString = color.a + "," + color.r + "," + color.g + "," + color.b;
		PlayerPrefs.SetString("backgroundColor", colorString);
	}

	public static Color GetPreferredBackgroundColor()
	{
		float a;
		float r;
		float g;
		float b;

		string colorString = PlayerPrefs.GetString("backgroundColor", "1,1,0.8,0.4");
		string[] components = colorString.Split(new string[] { "," }, System.StringSplitOptions.None);
		a = float.Parse(components[0]);
		r = float.Parse(components[1]);
		g = float.Parse(components[2]);
		b = float.Parse(components[3]);
		return new Color(r, g, b, a);
	}
}