using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;
using System.Linq;

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

	public static List<GameObject> prefabs;
	public static List<TextAsset> documents;
	public static List<AudioClip> audios;
	public static List<Texture> textures;
	public static List<Sprite> sprites;
	public static List<Material> materials;

	public static void Init() {
		LoadUpAssets();
		SetAppearance();
	}

	public static void LoadUpAssets() {
		if (prefabs != null) {
			foreach (GameObject go in prefabs) {
				Destroy(go);
			}
		}
		if (documents != null) {
			foreach (TextAsset ta in documents) {
				Destroy(ta);
			}
		}
		if (audios != null) {
			foreach (AudioClip ac in audios) {
				Destroy(ac);
			}
		}
		if (textures != null) {
			foreach (Texture tt in textures) {
				Destroy(tt);
			}
		}
		if (sprites != null) {
			foreach (Sprite sp in sprites) {
				Destroy(sp);
			}
		}
		if (materials != null) {
			foreach (Material mt in materials) {
				Destroy(mt);
			}
		}

		prefabs = new List<GameObject>();
		documents = new List<TextAsset>();
		audios = new List<AudioClip>();
		textures = new List<Texture>();
		sprites = new List<Sprite>();
		materials = new List<Material>();

		LoadUpHotAssets();
	}

	public static void LoadUpHotAssets() {
		string dir = Application.dataPath + "/AssetBundles/hotassets";
		AssetBundle hotassets = AssetBundle.LoadFromFile(dir, 0);

		string[] names = hotassets.GetAllAssetNames();

		foreach (string name in names) {
			string prefix = name.Substring(name.LastIndexOf("/") + 1);
			prefix = prefix.Substring(0, prefix.IndexOf("@"));
			if (String.Equals(prefix, "Prefab", StringComparison.CurrentCultureIgnoreCase)) {
				prefabs.Add(hotassets.LoadAsset<GameObject>(name));
			} else if (String.Equals(prefix, "Document", StringComparison.CurrentCultureIgnoreCase)) {
				documents.Add(hotassets.LoadAsset<TextAsset>(name));
			} else if (String.Equals(prefix, "Audio", StringComparison.CurrentCultureIgnoreCase)) {
				audios.Add(hotassets.LoadAsset<AudioClip>(name));
			} else if (String.Equals(prefix, "Texture", StringComparison.CurrentCultureIgnoreCase)) {
				textures.Add(hotassets.LoadAsset<Texture>(name));
			} else if (String.Equals(prefix, "Sprite", StringComparison.CurrentCultureIgnoreCase)) {
				sprites.Add(hotassets.LoadAsset<Sprite>(name));
			} else if (String.Equals(prefix, "Material", StringComparison.CurrentCultureIgnoreCase)) {
				materials.Add(hotassets.LoadAsset<Material>(name));
			}
		}

		hotassets.Unload(false);
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
		JsonData styleArray = JsonMapper.ToObject(GetDocumentByName("ColorStyles").text);
		JsonData defaultColorStyle = styleArray[0];
		JsonData colorArray = defaultColorStyle["colors"];

		for (int i = 0; i < textColors.Length; i++) {
			int r = (int)colorArray[i]["r"];
			int g = (int)colorArray[i]["g"];
			int b = (int)colorArray[i]["b"];
			int a = (int)colorArray[i]["a"];
			textColors[i] = new Color(r, g, b, a);
		}
	}

	public static Material GetMaterialByName(string name) {
		foreach (Material material in materials) {
			if (name.Equals(material.name.Substring(material.name.IndexOf("@") + 1))) {
				return material;
			}
		}
		return null;
	}

	public static GameObject GetPrefabByName(string name) {
		foreach (GameObject prefab in prefabs) {
			if (name.Equals(prefab.name.Substring(prefab.name.IndexOf("@") + 1))) {
				return prefab;
			}
		}
		return null;
	}

	public static TextAsset GetDocumentByName(string name) {
		foreach (TextAsset document in documents) {
			if (name.Equals(document.name.Substring(document.name.IndexOf("@") + 1))) {
				return document;
			}
		}
		return null;
	}

	public static Material[] GetMaterialsByType(string type) {
		List<Material> mMaterials = new List<Material>();
		foreach (Material material in materials) {
			string mType = material.name;
			mType = mType.Substring(mType.IndexOf("@") + 1);
			mType = mType.Substring(0, mType.IndexOf("@"));
			if (type.Equals(mType)) {
				mMaterials.Add(material);
			}
		}
		return mMaterials.ToArray();
	}

	public static Texture GetTextureByName(string name) {
		foreach (Texture texture in textures) {
			if (name.Equals(texture.name.Substring(texture.name.IndexOf("@") + 1))) {
				return texture;
			}
		}
		return null;
	}

	public static Texture[] GetTexturesByType(string type) {
		List<Texture> mTextures = new List<Texture>();
		foreach (Texture texture in textures) {
			string mType = texture.name;
			mType = mType.Substring(mType.IndexOf("@") + 1);
			mType = mType.Substring(0, mType.IndexOf("@"));
			if (type.Equals(mType)) {
				mTextures.Add(texture);
			}
		}
		return mTextures.ToArray();
	}

	public static Sprite GetSpriteByName(string name) {
		foreach (Sprite sprite in sprites) {
			if (name.Equals(sprite.name.Substring(sprite.name.IndexOf("@") + 1))) {
				return sprite;
			}
		}
		return null;
	}

	public static Sprite[] GetSpritesByType(string type) {
		List<Sprite> mSprites = new List<Sprite>();
		foreach (Sprite sprite in sprites) {
			string mType = sprite.name;
			mType = mType.Substring(mType.IndexOf("@") + 1);
			mType = mType.Substring(0, mType.IndexOf("@"));
			if (type.Equals(mType)) {
				mSprites.Add(sprite);
			}
		}
		return mSprites.ToArray();
	}

	public static AudioClip GetAduioByName(string name) {
		foreach (AudioClip audio in audios) {
			if (name.Equals(audio.name.Substring(audio.name.IndexOf("@") + 1))) {
				return audio;
			}
		}
		return null;
	}

	public static AudioClip[] GetAudiosByType(string type) {
		List<AudioClip> mAudios = new List<AudioClip>();
		foreach (AudioClip audio in audios) {
			string mType = audio.name;
			mType = mType.Substring(mType.IndexOf("@") + 1);
			mType = mType.Substring(0, mType.IndexOf("@"));
			if (type.Equals(mType)) {
				mAudios.Add(audio);
			}
		}
		return mAudios.ToArray();
	}

	public static int GetAudioTypeCount() {
		List<string> types = new List<string>();
		foreach (AudioClip audio in audios) {
			string mType = audio.name;
			mType = mType.Substring(mType.IndexOf("@") + 1);
			mType = mType.Substring(0, mType.IndexOf("@"));
			if (!types.Contains(mType)) {
				types.Add(mType);
			}
		}
		return types.Count;
	}

	public static UnityEngine.Object GetAssetByName(string name) {
		UnityEngine.Object[] assets;
		string type = name.Substring(0, name.IndexOf("@"));
		switch (name) {
			case "Prefab": assets = prefabs.ToArray(); break;
			case "Document": assets = documents.ToArray(); break;
			case "Audio": assets = audios.ToArray(); break;
			case "Texture": assets = textures.ToArray(); break;
			case "Sprite": assets = sprites.ToArray(); break;
			case "Material": assets = materials.ToArray(); break;
			default: return null;
		}
		foreach (UnityEngine.Object asset in assets) {
			if (name.Equals(asset.name.Substring(asset.name.IndexOf("@") + 1))) {
				return asset;
			}
		}
		return null;
	}
}