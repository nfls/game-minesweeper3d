using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class NetUtils {
	public static bool offlineMode;
	static string CLIENT_ID;
	static string CLIENT_SECRET;
	public static string REFRESH_TOKEN;
	static string ACCESS_TOKEN;

	public static void Init() {
		offlineMode = false;
		CLIENT_ID = "Jg3NeDU6FAGXdaMXPh9KSEKUwd8laT8y3sDvE5crYIo=";
		CLIENT_SECRET = "LpL09CBIbK7vrdi9kSUjDemSaqXn17wqkuxURahqOwNJx61XXXIr5iYWzJnY2bjlcuZ6SjuAtJSRYge7HkUQMA==";
	}

	public static IEnumerator GetRefreshToken(string username, string password) {
		string content = "grant_type=password&username=" + username + "&password=" + password + "&scope=&client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET;
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Content-Type"] = "application/x-www-form-urlencoded";
		form.AddField("grant_type", "password");
		form.AddField("username", username);
		form.AddField("password", password);
		form.AddField("scope", "");
		form.AddField("client_id", CLIENT_ID);
		form.AddField("client_secret", CLIENT_SECRET);
		byte[] rawData = Encoding.UTF8.GetBytes(content);
		WWW www = new WWW("https://api-v3.nfls.io/oauth/accessToken", rawData, headers);
		yield return www;
		if (www.error == null) {
			REFRESH_TOKEN = www.text;
			Debug.Log(REFRESH_TOKEN);
		} else {
			Debug.Log(www.error + " " + content);
		}
	}

	public static IEnumerator GetAccessToken() {
		string content = "grant_type=refresh_token&scope=&client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET;
		WWWForm form = new WWWForm();
		Dictionary<string, string> headers = form.headers;
		headers["Content-Type"] = "application/x-www-form-urlencoded";
		byte[] rawData = Encoding.UTF8.GetBytes(content);
		WWW www = new WWW("https://api-v3.nfls.io/oauth/accessToken", rawData, headers);
		yield return www;
		if (www.error == null) {
			ACCESS_TOKEN = www.text;
			Debug.Log(ACCESS_TOKEN);
		} else {
			Debug.Log(www.error + " " + content);
		}
	}
}
