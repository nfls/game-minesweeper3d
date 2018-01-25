using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour {
	GameController game;
	Text text1;
	Text text2;
	Text text3;
	Text text4;
	Text text5;

	float timePast;

	void Start() {
		game = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		text1 = transform.Find("Text 1").GetComponent<Text>();
		text2 = transform.Find("Text 2").GetComponent<Text>();
		text3 = transform.Find("Text 3").GetComponent<Text>();
		text4 = transform.Find("Text 4").GetComponent<Text>();
		text5 = transform.Find("Text 5").GetComponent<Text>();
	}

	void Update() {
		if (!game.IsGameOver() && !game.IsPaused()) {
			timePast += Time.deltaTime;
			string text = text5.text;
			text = text.Substring(0, text.IndexOf("=") + 1);
			text += " " + (int)timePast + "s";
			text5.text = text;
		}
	}

	public void Init() {
		timePast = 0;
		string text = text1.text;
		text = text.Substring(0, text.IndexOf("=") + 1);
		text += " " + "[" + game.maxX + "x" + game.maxY + "x" + game.maxZ + "]";
		text1.text = text;

		text = text2.text;
		text = text.Substring(0, text.IndexOf("=") + 1);
		text += " " + game.maxX * game.maxY * game.maxZ;
		text2.text = text;

		text = text3.text;
		text = text.Substring(0, text.IndexOf("=") + 1);
		text += " " + game.minesNum;
		text3.text = text;

		text = text4.text;
		text = text.Substring(0, text.IndexOf("=") + 1);
		text += " " + game.minesNum;
		text4.text = text;
	}

	public void OnMinesLeftChanged(int minesLeft) {
		string text = text4.text;
		text = text.Substring(0, text.IndexOf("=") + 1);
		text += " " + minesLeft;
		text4.text = text;
	}
}
