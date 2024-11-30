using CielaSpike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MM_UILabel {
	MAP_SCREEN = 0,
	INVENTORY_SCREEN = 1,
	EQUIPMENT_SCREEN = 2,
	ABILITIES_SCREEN = 3,
	SYSTEM_SCREEN = 4,
	GAME_SAVED = 5,
	SAVING_GAME = 6,
	LOADING = 7,
	ITEM_DETAILS_DIALOG_MULTIPLIER = 8,
	DIALOG_POINTS_SUFFIX = 9,
	NEW_ITEM_LABEL = 10,
	BUY_LABEL = 11,
	EXIT_LABEL = 12,
	STORY_INTRO_1 = 101,
	STORY_INTRO_2 = 102,
	STORY_INTRO_3 = 103
}

[System.Serializable]
public class MM_Language_Item {
	public int id;
	public string name;
	public string description;
}

public class MM_LanguageManager : MonoBehaviour {

	public bool IsInitialised { get; private set; }

	private string _language = "en";
	private const string _TIPS_BUNDLE = "tips_";
	private const string _TEXT_BUNDLE = "text_";

	private const string _TIPS = "tips_";
	private string _loadingScreenTipsJson = "";
	private bool _loadingScreenTipsTextInitialised = false;

	private const string _UI = "ui_";
	private Dictionary<MM_UILabel, string> _uiLabels = new Dictionary<MM_UILabel, string>();

	private const string _ITEMS = "items_";
	private Dictionary<int, string> _itemNames = new Dictionary<int, string>();
	private Dictionary<int, string> _itemDescriptions = new Dictionary<int, string>();

	private const string _AREAS = "areas_";
	private Dictionary<int, string> _areaNames = new Dictionary<int, string>();

	private void Start() {
		this.StartCoroutineAsync(_loadCommonAssetBundle());
	}

	public void Initialise() {
		if (!IsInitialised) {
			IsInitialised = true;
			this.StartCoroutineAsync(_loadAssetBundles());
		}
	}

	public string GetCurrentLanguage() {
		return _language;
	}

	public bool IsLoadingScreenTipsTextInitialised() {
		return _loadingScreenTipsTextInitialised;
	}

	public string GetLoadingScreenTipsJson() {
		return _loadingScreenTipsJson;
	}

	public string GetUILabel(MM_UILabel label) {
		return (_uiLabels.ContainsKey(label) ? _uiLabels[label] : MM_Constants.TEXT_NOT_FOUND);
	}

	public string GetItemName(int item) {
		return (_itemNames.ContainsKey(item) ? _itemNames[item] : MM_Constants.TEXT_NOT_FOUND);
	}

	public string GetItemDescription(int item) {
		return (_itemDescriptions.ContainsKey(item) ? _itemDescriptions[item] : MM_Constants.TEXT_NOT_FOUND);
	}

	public string GetAreaName(int item) {
		return (_areaNames.ContainsKey(item) ? _areaNames[item] : MM_Constants.TEXT_NOT_FOUND);
	}

	public string GetStyledItemName(PlayerItem item, int count = 0) {
		string name = "";
		if (item != PlayerItem.EQUIPPED_NONE && MM_Items.ItemData.ContainsKey(item)) {
			switch (item) {
				case PlayerItem.ELIXIR:
					name = MM.lang.GetItemName((int)item).Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_TAG_INDEX), MM.player.Elixirs == 1 ? "" : MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_SUFFIX_INDEX));
					break;
				case PlayerItem.CHARMSTONE:
					name = MM.lang.GetItemName((int)item).Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_TAG_INDEX), MM.player.Charmstones == 1 ? "" : MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_SUFFIX_INDEX));
					break;
				default:
					name = MM.lang.GetItemName((int)item);
					break;
			}
			if (count > 1) {
				name = name + MM.lang.GetUILabel(MM_UILabel.ITEM_DETAILS_DIALOG_MULTIPLIER) + count.ToString();
			}
		}
		return name;
	}

	public string GetStyledItemDescription(PlayerItem item, DialogType? dialogType = null) {
		string description = "<line-height=100%>";
		string textSize = "40";
		string spriteSize = "64";
		string ability;
		int spriteIndex;

		if (dialogType == DialogType.PURCHASE) {
			textSize = "36";
		}

		if (item != PlayerItem.EQUIPPED_NONE && MM_Items.ItemData.ContainsKey(item)) {
			description += MM.lang.GetItemDescription((int)item).Replace("\n<size=" + textSize + "> ", "\n<space=2px><size=" + textSize + ">");
			switch (item) {
				case PlayerItem.ELIXIR:
					description = description.Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_AMOUNT_TAG_INDEX), MM.player.Elixirs.ToString());
					description = description.Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_TAG_INDEX), MM.player.Elixirs == 1 ? "" : MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_SUFFIX_INDEX));
					break;
				case PlayerItem.CHARMSTONE:
					description = description.Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_AMOUNT_TAG_INDEX), MM.player.Charmstones.ToString());
					description = description.Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_TAG_INDEX), MM.player.Charmstones == 1 ? "" : MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_SUFFIX_INDEX));
					break;
			}

			if (dialogType == DialogType.PURCHASE) {
				// Remove the 2nd line of the description for purchase dialogs
				description = description.Substring(0, description.IndexOf("\n"));
			}

			// If the item has additional abilities then add them to the description
			List<PlayerItem> abilities = MM_Items.ItemData[item].itemAbilities;
			if (abilities.Count > 0) {
				description += "\n";
				for (int i = 0; i < abilities.Count; i++) {
					ability = MM.player.ItemAbilities[abilities[i]] || !MM_Items.ItemAbilityData[abilities[i]].locked ? MM.lang.GetItemName((int)abilities[i]) : MM.lang.GetItemName(MM_Constants.LOCKED_ABILITY_TEXT_INDEX);
					spriteIndex = MM.player.ItemAbilities[abilities[i]] || !MM_Items.ItemAbilityData[abilities[i]].locked ? MM_Items.ItemAbilityData[abilities[i]].spriteIndex : (int)TextMeshProSprite.LOCKED;
					description += "<size=" + textSize + "><voffset=0.2em><size=" + spriteSize + "><sprite=" + spriteIndex + "></size><space=20px>" + ability + "</voffset></size>" + (i < abilities.Count - 1 ? "<space=60px>" : "");
				}
			}
		}
		return description;
	}

	private IEnumerator _loadCommonAssetBundle() {
		AssetBundleCreateRequest bundleRequest;
		AssetBundle bundle;
		AssetBundleRequest request;
		TextAsset asset;
		string filepath;
		string json;

		// Load the general text asset bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _TEXT_BUNDLE + _language);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			bundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (bundle != null) {
				// Retrieve the UI text
				yield return Ninja.JumpToUnity;
				request = bundle.LoadAssetAsync<TextAsset>(_UI + _language);
				yield return Ninja.JumpBack;
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					yield return Ninja.JumpToUnity;
					asset = (TextAsset)request.asset;
					json = asset.text;
					yield return Ninja.JumpBack;
					_parseUIText(json);
				}
				// Retrieve the items text
				yield return Ninja.JumpToUnity;
				request = bundle.LoadAssetAsync<TextAsset>(_ITEMS + _language);
				yield return Ninja.JumpBack;
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					yield return Ninja.JumpToUnity;
					asset = (TextAsset)request.asset;
					json = asset.text;
					yield return Ninja.JumpBack;
					_parseItemsText(json);
				}
				// Retrieve the area names text
				yield return Ninja.JumpToUnity;
				request = bundle.LoadAssetAsync<TextAsset>(_AREAS + _language);
				yield return Ninja.JumpBack;
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					yield return Ninja.JumpToUnity;
					asset = (TextAsset)request.asset;
					json = asset.text;
					yield return Ninja.JumpBack;
					_parseAreaNamesText(json);
				}
				yield return Ninja.JumpToUnity;
				bundle.Unload(true);
			}
		} else {
			Debug.LogError("MM_LanguageManager:Start : failed to load text asset bundle");
		}
		yield return Ninja.JumpBack;
	}

	private IEnumerator _loadAssetBundles() {
		AssetBundleCreateRequest bundleRequest;
		AssetBundle bundle;
		AssetBundleRequest request;
		TextAsset asset;
		string filepath;

		// Load the tips asset bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _TIPS_BUNDLE + _language);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			bundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (bundle != null) {
				yield return Ninja.JumpToUnity;
				request = bundle.LoadAssetAsync<TextAsset>(_TIPS + _language);
				yield return Ninja.JumpBack;
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					yield return Ninja.JumpToUnity;
					asset = (TextAsset)request.asset;
					_loadingScreenTipsJson = asset.text;
					bundle.Unload(true);
					_loadingScreenTipsTextInitialised = true;
					MM.events.Trigger(MM_Event.TIPS_TEXT_LOADED);
				}
			}
		} else {
			Debug.LogError("MM_LanguageManager:_loadAssetBundles : failed to load tips asset bundle");
		}

		MM.events.Trigger(MM_Event.LANGUAGE_LOADED);
		yield return Ninja.JumpBack;
	}

	private void _parseUIText(string assetText) {
		// Parse the TextAsset: assign each property from the UIm data into the appropriate dictionary
		try {
			MM_Language_Item[] items = JsonHelper.getJsonArray<MM_Language_Item>(assetText);

			foreach (MM_Language_Item item in items) {
				_uiLabels[(MM_UILabel)item.id] = item.name;
			}
		} catch (Exception e) {
			Debug.LogWarning("MM_LanguageManager:_parseUIText : cannot load file : " + e.Message);
		}
		Debug.Log("MM_LanguageManager:_parseUIText : initialisation of " + _uiLabels.Count.ToString() + " UI labels complete");
	}

	private void _parseItemsText(string assetText) {
		// Parse the TextAsset: assign each property from the item data into the appropriate dictionary
		try {
			MM_Language_Item[] items = JsonHelper.getJsonArray<MM_Language_Item>(assetText);

			foreach (MM_Language_Item item in items) {
				_itemNames[item.id] = item.name;
				_itemDescriptions[item.id] = item.description;
			}
		} catch (Exception e) {
			Debug.LogWarning("MM_LanguageManager:_parseItemsText : cannot load file : " + e.Message);
		}
		Debug.Log("MM_LanguageManager:_parseItemsText : initialisation of " + _itemNames.Count.ToString() + " items text complete");
	}

	private void _parseAreaNamesText(string assetText) {
		try {
			MM_Language_Item[] items = JsonHelper.getJsonArray<MM_Language_Item>(assetText);

			foreach (MM_Language_Item item in items) {
				_areaNames[item.id] = item.name;
			}
		} catch (Exception e) {
			Debug.LogWarning("MM_LanguageManager:_parseAreasText : cannot load file : " + e.Message);
		}
		Debug.Log("MM_LanguageManager:_parseAreasText : initialisation of " + _areaNames.Count.ToString() + " area names text complete");
	}
}