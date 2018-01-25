using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResourcesManager : MonoBehaviour {
	static BlockSurfaceStyle blockSurfaceStyle;
	static NumFont textFont;
	static FontStyle fontStyle;
	static NumColorSet textColorSet;

	static Material blockSurfaceMaterial;
	static Material textMaterial;
	static Font font;
	static Color[] textColors;
	static Color backgroundColor;

	static TextAsset textColorSetsFile;

	public static void Init() {
		textColorSetsFile = (TextAsset)Resources.Load("Documents/TextColorSets");
		GameController.mainCamera.backgroundColor = PreferencesManager.GetPreferredBackgroundColor();
		SetAppearance();
		/*
		print(textColors.Length);
		for (int i = 0; i < textColors.Length; i++) {
			print(textColors[i]);
		}
		*/
	}

	public static void SetAppearance() {
		SetBlockSurfaceStyle(BlockSurfaceStyle.Default);
		SetTextFont(NumFont.Futura);
		SetFontStyle(FontStyle.Normal);
		SetTextColorSet(NumColorSet.Default);
	}

	public static void SetAppearance(BlockSurfaceStyle blockSurfaceStyle, NumFont textFont, FontStyle textStyle, NumColorSet textColorSet) {
		SetBlockSurfaceStyle(blockSurfaceStyle);
		SetTextFont(textFont);
		SetFontStyle(textStyle);
		SetTextColorSet(textColorSet);
	}

	public static void SetBlockSurfaceStyle(BlockSurfaceStyle style) {
		blockSurfaceStyle = style;
		LoadBlockSurfaceStyle();
	}

	public static void SetTextFont(NumFont font) {
		textFont = font;
		LoadTextFont();
	}

	public static void SetFontStyle(FontStyle style) {
		fontStyle = style;
	}

	public static void SetTextColorSet(NumColorSet colorSet) {
		textColorSet = colorSet;
		LoadTextColorSet();
	}

	public static Material GetBlockSurfaceMaterial() {
		return blockSurfaceMaterial;
	}

	public static Material GetTextMaterial() {
		return textMaterial;
	}

	public static Font GetFont() {
		return font;
	}

	public static FontStyle GetFontStyle() {
		return fontStyle;
	}

	public static Color GetTextColor(int num) {
		if (num >= 1 && num <= 26) {
			return textColors[num - 1];
		} else {
			return Color.black;
		}
	}

	public static Color GetTextColor(string symbol) {
		switch (symbol) {
			case "Flag":
				return textColors[26];
			case "Mark":
				return textColors[27];
			case "Mine":
				return textColors[28];
			default:
				return Color.black;
		}
	}

	public static void LoadBlockSurfaceStyle() {
		blockSurfaceMaterial = (Material)Resources.Load("Materials/" + blockSurfaceStyle.ToString() + "BlockSurfaceMaterial");
	}

	public static void LoadTextFont() {
		font = (Font)Resources.Load("Fonts/" + textFont.ToString());
		textMaterial = (Material)Resources.Load("Materials/" + textFont.ToString() + "TextMaterial");
	}

	public static void LoadTextColorSet() {

		textColors = new Color[29];

		string[] lines = textColorSetsFile.text.Split(new string[] { "\n" }, StringSplitOptions.None);
		int startLineNum = -1;
		int endLineNum = -1;

		for (int i = 0; i < lines.Length; i++) {
			if (lines[i].Equals("Style=" + textColorSet.ToString())) {
				startLineNum = i;
			} else if (startLineNum != -1) {
				if (lines[i].Equals("StyleEnd")) {
					endLineNum = i;
					break;
				}
			}
		}

		for (int i = 0; i < endLineNum - startLineNum - 1; i++) {
			string colorString = lines[startLineNum + i + 1];
			colorString = colorString.Substring(colorString.IndexOf("[") + 1, colorString.IndexOf("]") - colorString.IndexOf("[") - 1);
			string[] colorStrings = colorString.Split(new string[] { "," }, StringSplitOptions.None);
			float a = float.Parse(colorStrings[0].Substring(colorStrings[0].IndexOf("=") + 1));
			float r = float.Parse(colorStrings[1].Substring(colorStrings[1].IndexOf("=") + 1));
			float g = float.Parse(colorStrings[2].Substring(colorStrings[2].IndexOf("=") + 1));
			float b = float.Parse(colorStrings[3].Substring(colorStrings[3].IndexOf("=") + 1));

			textColors[i] = new Color(r / 255, g / 255, b / 255, a / 255);
		}
	}
}