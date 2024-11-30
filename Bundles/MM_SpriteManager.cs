using CielaSpike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class MM_SpriteManager : MonoBehaviour {

	public bool IsInitialised { get; private set; }

	private enum SpriteAtlasType {
		TIPS_ICONS,
		HUD_HEARTS,
		HUD_ABILITY,
		HUD_POWERUP_RING,
		GAME_MENU_ITEMS,
		GAME_MENU_ABILITY,
		PORTRAIT,
		PORTRAIT_WEAPONS,
		PORTRAIT_ARMOR,
		PORTRAIT_SHIELDS,
		PORTRAIT_BOOTS,
		AREA_LABELS,
		ENVIRONMENT
	}

	private string _language = "en";
	private const string _GETSPRITES_CLONE_SUFFIX = "(Clone)";

	// Tips icons bundle
	private AssetBundle _tipsBundle;
	private const string _TIPS_BUNDLE = "tips_icons";
	private const string _TIPS_SPRITEATLAS = "IconsSpriteAtlas";
	private bool _loadingScreenTipsIconsInitialised = false;
	private Dictionary<string, Sprite> _tipsIcons = new Dictionary<string, Sprite>();

	// HUD bundle
	private AssetBundle _hudBundle;
	private const string _HUD_BUNDLE = "hud";
	private const int _HEART_TOTAL_FRAMES = 51;
	private const string _HUD_HEART_SPRITEATLAS = "HeartSpriteAtlas";
	private const string _HEART_SPRITESHEET = "HeartSpritesheet_";
	private const string _HUD_ABILITY_SPRITEATLAS = "AbilitySpriteAtlas";
	private const string _HUD_POWERUP_RING_SPRITEATLAS = "PowerupRingSpriteAtlas";
	private const string _POWERUP_RING_SPRITESHEET = "HUDPowerupRingSpritesheet_";
	private Dictionary<string, Sprite> _hudHearts = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _hudAbilities = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _hudPowerupRing = new Dictionary<string, Sprite>();

	// Inventory bundle
	private AssetBundle _gameMenuBundle;
	private const string _INVENTORY_BUNDLE = "inventory";
	private const string _GAME_MENU_ITEM_SPRITEATLAS = "ItemSpriteAtlas";
	private const string _GAME_MENU_ABILITY_SPRITEATLAS = "AbilitySpriteAtlas";
	private Dictionary<string, Sprite> _gameMenuItems = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _gameMenuAbilities = new Dictionary<string, Sprite>();
	private const string _GAME_MENU_ABILITY_PREFIX = "Ability_";
	private const string _GAME_MENU_ABILITY_XL_PREFIX = "AbilityXL_";

	// NPC portraits
	private const string _PORTRAIT_SPRITEATLAS = "PortraitsSpriteAtlas";
	private Dictionary<string, Sprite> _portraits = new Dictionary<string, Sprite>();

	// Player portrait bundle
	private AssetBundle _portraitBundle;
	private const string _PORTRAIT_BUNDLE = "player_bock";
	private const string _PORTRAIT_WEAPON_SPRITEATLAS = "PortraitBockWeaponSpriteAtlas";
	private const string _PORTRAIT_ARMOR_SPRITEATLAS = "PortraitBockArmorSpriteAtlas";
	private const string _PORTRAIT_SHIELD_SPRITEATLAS = "PortraitBockShieldSpriteAtlas";
	private const string _PORTRAIT_BOOTS_SPRITEATLAS = "PortraitBockBootsSpriteAtlas";
	private Dictionary<string, Sprite> _playerPortraitWeapons = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _playerPortraitArmor = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _playerPortraitShields = new Dictionary<string, Sprite>();
	private Dictionary<string, Sprite> _playerPortraitBoots = new Dictionary<string, Sprite>();

	// Area labels bundle
	private AssetBundle _areaLabelsBundle;
	private const string _AREA_LABELS_BUNDLE = "area_labels_";
	private const string _AREA_LABELS_SPRITEATLAS = "AreaLabelsSpriteAtlas_";
	private Dictionary<string, Sprite> _areaLabels = new Dictionary<string, Sprite>();

	// Environment bundle
	private AssetBundle _environmentBundle;
	private const string _ENVIRONMENT_BUNDLE = "environment";
	private const string _ENVIRONMENT_SPRITEATLAS = "Environment";
	private Dictionary<string, Sprite> _environment = new Dictionary<string, Sprite>();

	void OnEnable() {
		SpriteAtlasManager.atlasRequested += RequestAtlas;
	}

	void OnDisable() {
		SpriteAtlasManager.atlasRequested -= RequestAtlas;
	}

	void RequestAtlas(string tag, System.Action<SpriteAtlas> callback) {
		//Debug.Log("MM_SpriteManager:RequestAtlas : atlas tag [" + tag + "] requested before assetbundle loaded");
	}

	void OnDestroy() {
		if (_tipsBundle != null) {
			_tipsBundle.Unload(true);
		}
		if (_hudBundle != null) {
			_hudBundle.Unload(true);
		}
		if (_gameMenuBundle != null) {
			_gameMenuBundle.Unload(true);
		}
		if (_portraitBundle != null) {
			_portraitBundle.Unload(true);
		}
		if (_areaLabelsBundle != null) {
			_areaLabelsBundle.Unload(true);
		}
		if (_environmentBundle != null) {
			_environmentBundle.Unload(true);
		}
	}

	public void Initialise() {
		if (!IsInitialised) {
			IsInitialised = true;
			this.StartCoroutineAsync(_loadAssetBundles());
		}
	}

	public bool IsLoadingScreenTipsIconsInitialised() {
		return _loadingScreenTipsIconsInitialised;
	}

	public Sprite GetTipIconSprite(string spriteName) {
		return _tipsIcons.ContainsKey(spriteName) ? _tipsIcons[spriteName] : null;
	}

	public Sprite GetHUDHeartSprite(int index) {
		return _hudHearts[_HEART_SPRITESHEET + index.ToString()];
	}

	public Sprite GetHUDItemSprite(string spriteName) {
		// Currently the HUD item sprites are the same as those that appear in the game menu
		return _gameMenuItems.ContainsKey(spriteName) ? _gameMenuItems[spriteName] : null;
	}

	public Sprite GetHUDAbilitySprite(string spriteName) {
		return _hudAbilities.ContainsKey(spriteName) ? _hudAbilities[spriteName] : null;
	}

	public Sprite GetHUDPowerupRingSprite(int index) {
		return _hudPowerupRing[_POWERUP_RING_SPRITESHEET + index.ToString()];
	}

	public Sprite GetGameMenuItemSprite(string spriteName) {
		return _gameMenuItems.ContainsKey(spriteName) ? _gameMenuItems[spriteName] : null;
	}

	public Sprite GetGameMenuAbilitySprite(string spriteName, bool getXL = false) {
		if (getXL) {
			spriteName = spriteName.Replace(_GAME_MENU_ABILITY_PREFIX, _GAME_MENU_ABILITY_XL_PREFIX);
		}
		return (_gameMenuAbilities.ContainsKey(spriteName) ? _gameMenuAbilities[spriteName] : null);
	}

	public Sprite GetPortraitSprite(string spriteName) {
		return _portraits.ContainsKey(spriteName) ? _portraits[spriteName] : null;
	}

	public Sprite GetPlayerPortraitWeaponSprite(string spriteName) {
		return (_playerPortraitWeapons.ContainsKey(spriteName) ? _playerPortraitWeapons[spriteName] : null);
	}

	public Sprite GetPlayerPortraitArmorSprite(string spriteName) {
		return (_playerPortraitArmor.ContainsKey(spriteName) ? _playerPortraitArmor[spriteName] : null);
	}

	public Sprite GetPlayerPortraitShieldSprite(string spriteName) {
		return (_playerPortraitShields.ContainsKey(spriteName) ? _playerPortraitShields[spriteName] : null);
	}

	public Sprite GetPlayerPortraitBootsSprite(string spriteName) {
		return (_playerPortraitBoots.ContainsKey(spriteName) ? _playerPortraitBoots[spriteName] : null);
	}

	public Sprite GetAreaLabelSprite(string spriteName) {
		return (_areaLabels.ContainsKey(spriteName) ? _areaLabels[spriteName] : null);
	}

	public Sprite GetEnvironmentSprite(string spriteName) {
		return (_environment.ContainsKey(spriteName) ? _environment[spriteName] : null);
	}

	private IEnumerator _loadAssetBundles() {
		AssetBundleCreateRequest bundleRequest;
		string filepath;

		// Load the tips icons bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _TIPS_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_tipsBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_tipsBundle != null) {
				yield return _initialiseTipsIconsSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load tips icons asset bundle");
		}

		// Load the HUD bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _HUD_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_hudBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_hudBundle != null) {
				yield return _initialiseHUDSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load HUD asset bundle");
		}

		// Load the inventory bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _INVENTORY_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_gameMenuBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_gameMenuBundle != null) {
				yield return _initialiseGameMenuSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load inventory asset bundle");
		}

		// Load the player portrait bundle
		// TODO: add a method to load the appropriate player bundle and unload any other still in use
		// This method needs to be called when loading a game after playing with another character
		filepath = Path.Combine(MM.StreamingAssetsPath, _PORTRAIT_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_portraitBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_portraitBundle != null) {
				yield return _initialisePlayerPortraitSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load player portrait asset bundle");
		}

		// Load the area labels bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _AREA_LABELS_BUNDLE + _language);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_areaLabelsBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_areaLabelsBundle != null) {
				yield return _initialiseAreaLabelsSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load area labels asset bundle");
		}

		// Load the environment bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _ENVIRONMENT_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_environmentBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_environmentBundle != null) {
				yield return _initialiseEnvironmentSprites();
			}
		} else {
			Debug.LogError("MM_SpriteManager:_loadAssetBundles : failed to load area labels asset bundle");
		}

		yield return Ninja.JumpToUnity;
		MM.events.Trigger(MM_Event.SPRITES_LOADED);
		yield return Ninja.JumpBack;
	}

	private IEnumerator _initialiseSprites(SpriteAtlasType atlasType) {
		AssetBundleRequest request = new AssetBundleRequest();
		// Determine the bundle to use and SpriteAtlas to load
		switch (atlasType) {
			case SpriteAtlasType.TIPS_ICONS:
				request = _tipsBundle.LoadAssetAsync<SpriteAtlas>(_TIPS_SPRITEATLAS);
				break;
			case SpriteAtlasType.HUD_HEARTS:
				request = _hudBundle.LoadAssetAsync<SpriteAtlas>(_HUD_HEART_SPRITEATLAS);
				break;
			case SpriteAtlasType.HUD_ABILITY:
				request = _hudBundle.LoadAssetAsync<SpriteAtlas>(_HUD_ABILITY_SPRITEATLAS);
				break;
			case SpriteAtlasType.HUD_POWERUP_RING:
				request = _hudBundle.LoadAssetAsync<SpriteAtlas>(_HUD_POWERUP_RING_SPRITEATLAS);
				break;
			case SpriteAtlasType.GAME_MENU_ITEMS:
				request = _gameMenuBundle.LoadAssetAsync<SpriteAtlas>(_GAME_MENU_ITEM_SPRITEATLAS);
				break;
			case SpriteAtlasType.GAME_MENU_ABILITY:
				request = _gameMenuBundle.LoadAssetAsync<SpriteAtlas>(_GAME_MENU_ABILITY_SPRITEATLAS);
				break;
			case SpriteAtlasType.PORTRAIT:
				request = _hudBundle.LoadAssetAsync<SpriteAtlas>(_PORTRAIT_SPRITEATLAS);
				break;
			case SpriteAtlasType.PORTRAIT_WEAPONS:
				request = _portraitBundle.LoadAssetAsync<SpriteAtlas>(_PORTRAIT_WEAPON_SPRITEATLAS);
				break;
			case SpriteAtlasType.PORTRAIT_ARMOR:
				request = _portraitBundle.LoadAssetAsync<SpriteAtlas>(_PORTRAIT_ARMOR_SPRITEATLAS);
				break;
			case SpriteAtlasType.PORTRAIT_SHIELDS:
				request = _portraitBundle.LoadAssetAsync<SpriteAtlas>(_PORTRAIT_SHIELD_SPRITEATLAS);
				break;
			case SpriteAtlasType.PORTRAIT_BOOTS:
				request = _portraitBundle.LoadAssetAsync<SpriteAtlas>(_PORTRAIT_BOOTS_SPRITEATLAS);
				break;
			case SpriteAtlasType.AREA_LABELS:
				request = _areaLabelsBundle.LoadAssetAsync<SpriteAtlas>(_AREA_LABELS_SPRITEATLAS + _language);
				break;
			case SpriteAtlasType.ENVIRONMENT:
				request = _environmentBundle.LoadAssetAsync<SpriteAtlas>(_ENVIRONMENT_SPRITEATLAS);
				break;
		}
		yield return request;
		if (request != null) {
			yield return new WaitWhile(() => request.isDone == false);
			SpriteAtlas atlas = (SpriteAtlas)request.asset;
			Sprite[] sprites = new Sprite[atlas.spriteCount];
			atlas.GetSprites(sprites);
			// Store each sprite in the appropriate dictionary
			// The GetSprites method adds a suffix to each sprite name that needs to be removed
			string spriteName;
			for (int i = 0; i < atlas.spriteCount; i++) {
				spriteName = sprites[i].name.Replace(_GETSPRITES_CLONE_SUFFIX, "");
				switch (atlasType) {
					case SpriteAtlasType.TIPS_ICONS:
						_tipsIcons[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.HUD_HEARTS:
						_hudHearts[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.HUD_ABILITY:
						_hudAbilities[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.HUD_POWERUP_RING:
						_hudPowerupRing[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.GAME_MENU_ITEMS:
						_gameMenuItems[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.GAME_MENU_ABILITY:
						_gameMenuAbilities[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.PORTRAIT:
						_portraits[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.PORTRAIT_WEAPONS:
						_playerPortraitWeapons[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.PORTRAIT_ARMOR:
						_playerPortraitArmor[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.PORTRAIT_SHIELDS:
						_playerPortraitShields[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.PORTRAIT_BOOTS:
						_playerPortraitBoots[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.AREA_LABELS:
						_areaLabels[spriteName] = sprites[i];
						break;
					case SpriteAtlasType.ENVIRONMENT:
						_environment[spriteName] = sprites[i];
						break;
				}
			}
			Debug.Log("MM_SpriteManager:_initialiseSprites : initialisation of " + atlas.spriteCount.ToString() + " " + atlasType.ToString() + " sprites complete");
		}
	}

	private IEnumerator _initialiseTipsIconsSprites() {
		yield return _initialiseSprites(SpriteAtlasType.TIPS_ICONS);
		_loadingScreenTipsIconsInitialised = true;
		yield return Ninja.JumpToUnity;
		MM.events.Trigger(MM_Event.TIPS_ICONS_LOADED);
		yield return Ninja.JumpBack;
	}

	private IEnumerator _initialiseHUDSprites() {
		yield return _initialiseSprites(SpriteAtlasType.HUD_HEARTS);
		yield return _initialiseSprites(SpriteAtlasType.HUD_ABILITY);
		yield return _initialiseSprites(SpriteAtlasType.HUD_POWERUP_RING);
		yield return _initialiseSprites(SpriteAtlasType.PORTRAIT);
	}

	private IEnumerator _initialiseGameMenuSprites() {
		yield return _initialiseSprites(SpriteAtlasType.GAME_MENU_ITEMS);
		yield return _initialiseSprites(SpriteAtlasType.GAME_MENU_ABILITY);
	}

	private IEnumerator _initialisePlayerPortraitSprites() {
		yield return _initialiseSprites(SpriteAtlasType.PORTRAIT_WEAPONS);
		yield return _initialiseSprites(SpriteAtlasType.PORTRAIT_ARMOR);
		yield return _initialiseSprites(SpriteAtlasType.PORTRAIT_SHIELDS);
		yield return _initialiseSprites(SpriteAtlasType.PORTRAIT_BOOTS);
	}

	private IEnumerator _initialiseAreaLabelsSprites() {
		yield return _initialiseSprites(SpriteAtlasType.AREA_LABELS);
	}

	private IEnumerator _initialiseEnvironmentSprites() {
		yield return _initialiseSprites(SpriteAtlasType.ENVIRONMENT);
	}
}