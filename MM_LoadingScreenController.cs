using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using TMPro;
using System.IO;
using MoreMountains.Tools;
using MoreMountains.MMInterface;

[System.Serializable]
public class MM_LoadingScreen_Tip {
	public string id;
	public int level;
	public string title;
	public string icon;
	public string description;
}

public class MM_LoadingScreenController : MonoBehaviour {

	private const float _MINIMUM_FADE_IN_TIME = 1f;
	private const float _MINIMUM_DISPLAY_TIME = 0f;
	private const float _TIP_TIMEOUT = 5f;

	private string _sceneToLoad;
	private AreaAssetBundle _areaBundle;

	[Header("Circular Indicator")]
	[Tooltip("Circular loading delay.")]
	public GameObject circularIndicator;
	[Tooltip("Scene Load Delay.")]
	public float circularLoadDelay = 6f;
	[Tooltip("Circular Indicator rotation speed.")]
	public float circularIndicatorAnimSpeed = 1f;

	[Header("Tip objects")]
	public GameObject tipIcon;
	public GameObject tipTitle;
	public GameObject tipDescription;

	private AsyncOperation _scene;
	private bool _resourcesLoaded = false;
	private bool _newSceneLoading = false;
	private bool _newSceneReady = false;
	private bool _minimumTimeElapsed = false;
	private bool _inTransition = false;
	private DateTime _startTime;
	private bool _languageInitialised;
	private bool _spritesInitialised;
	private bool _sfxInitialised;
	private bool _areaBundleInitialised;
	private MM_LoadingScreen_Tip[] _tips;
	private List<MM_LoadingScreen_Tip> _tipCollection = new List<MM_LoadingScreen_Tip>();
	private List<int> _tipsShown = new List<int>();
	private const int _RANDOM_ATTEMPTS = 10;
	private int _currentTip = 0;
	private bool _cancelNextTip = false;

	private void OnEnable() {
		MM_Events.OnTipsTextLoaded += _checkTipsAreReady;
		MM_Events.OnTipsIconsLoaded += _checkTipsAreReady;
		MM_Events.OnResourcesLoaded += _resourcesAreReady;
		MM_Events.OnInputPressed += _inputReceived;
	}

	private void OnDisable() {
		MM_Events.OnTipsTextLoaded -= _checkTipsAreReady;
		MM_Events.OnTipsIconsLoaded -= _checkTipsAreReady;
		MM_Events.OnResourcesLoaded -= _resourcesAreReady;
		MM_Events.OnInputPressed -= _inputReceived;
	}

	public void Awake() {
		// Check for the global objects from the preload scene
		// and if not found then restart at the preload scene
		if (GameObject.Find("_app") == null) {
			UnityEngine.SceneManagement.SceneManager.LoadScene("_preload");
		}
	}

	void Start() {
		// Set the canvas to use the UI camera in the preload scene
		transform.GetComponent<Canvas>().worldCamera = MM.UICamera;

		// Setup references
		_sceneToLoad = MM.player.CurrentScene != "" ? MM.player.CurrentScene : MM_Constants.SCENE_DEFAULT;
		_areaBundle = MM_Locations.SceneAreaData[MM.player.CurrentSceneArea].bundle;
		
		// Setup an event group listener to set the _resourcesLoaded flag when all
		// the additional resources (apart from the loading scene) have been loaded
		List<MM_Event> setupList = new List<MM_Event> {};
		if (!MM.lang.IsInitialised) {
			_languageInitialised = false;
			setupList.Add(MM_Event.LANGUAGE_LOADED);
		} else {
			_languageInitialised = true;
		}
		if (!MM.spriteManager.IsInitialised) {
			_spritesInitialised = false;
			setupList.Add(MM_Event.SPRITES_LOADED);
		} else {
			_spritesInitialised = true;
		}
		if (!MM.soundEffects.IsFullSetInitialised) {
			_sfxInitialised = false;
			setupList.Add(MM_Event.SFX_FULL_PRELOAD_COMPLETE);
		} else {
			_sfxInitialised = true;
		}
		if (!MM.areaManager.IsAreaInitialised || MM.areaManager.CurrentAreaInitialised != _areaBundle) {
			_areaBundleInitialised = false;
			setupList.Add(MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE);
		} else {
			_areaBundleInitialised = true;
		}
		if (setupList.Count > 0) {
			_resourcesLoaded = false;
			MM.events.AddEventGroupListener(setupList, MM_Event.RESOURCES_LOADED);
		} else {
			_resourcesLoaded = true;
		}

		// Ensure the HUD is hidden
		MM.hud.Hide(HUDFadeType.IMMEDIATE);
		MM.hud.ResetState();

		// Start spinning the loading hourglass
		circularIndicator.SetActive(true);

		// Read the current time in order to keep the loading screen displayed for a minimum length
		_startTime = System.DateTime.Now;
		_newSceneLoading = false;
		_checkTipsAreReady();
	}

	void Update() {
		// Perform the tasks on the setup list
		if (!_languageInitialised && (System.DateTime.Now - _startTime).TotalSeconds >= _MINIMUM_FADE_IN_TIME) {
			_languageInitialised = true;
			MM.lang.Initialise();
		}
		if (!_sfxInitialised && (System.DateTime.Now - _startTime).TotalSeconds >= _MINIMUM_FADE_IN_TIME) {
			_sfxInitialised = true;
			MM.soundEffects.PreloadFullSet();
		}
		if (!_areaBundleInitialised && (System.DateTime.Now - _startTime).TotalSeconds >= _MINIMUM_FADE_IN_TIME) {
			_areaBundleInitialised = true;
			MM.areaManager.LoadAreaBundle(_areaBundle);
		}
		if (!_spritesInitialised && (System.DateTime.Now - _startTime).TotalSeconds >= _MINIMUM_FADE_IN_TIME) {
			_spritesInitialised = true;
			MM.spriteManager.Initialise();
		}
		// Load the new scene after all other resources have loaded
		// (there is a bug that halts other asynchronous loads if a scene is also being loaded at the same time)
		if (_resourcesLoaded && !_newSceneLoading) {
			_newSceneLoading = true;
			_scene = SceneManager.LoadSceneAsync(_sceneToLoad);
			_scene.allowSceneActivation = false;
		}
		// Check if the minimum display time required to show the loading screen has elapsed
		if (!_minimumTimeElapsed && (System.DateTime.Now - _startTime).TotalSeconds >= _MINIMUM_DISPLAY_TIME) {
			_minimumTimeElapsed = true;
		}
		// Switch to the loaded scene if it is ready and the minimum display time has elapsed
		if (_minimumTimeElapsed && _newSceneLoading && !_newSceneReady && _scene != null && _scene.progress >= 0.9f) {
			if (!_inTransition) {
				_inTransition = true;
				MM.SetGameInputLocked(true, InputLockType.LOADING_SCENE);
				MM.AddInputFocus(InputFocus.GAME);
				MM.SetMenuInputLocked(false);
				FadeIntoNewScene();
			}
			if (_scene.isDone) {
				_newSceneReady = true;
			}
		}
	}

	void FadeIntoNewScene() {
		// Fade in the black overlay
		_cancelNextTip = true;
		MM.hud.TriggerFadeIn(0f, MM_Constants.FADE_DURATION);
		Invoke("_triggerNewSceneReadyEvent", 1f);
	}

	void _triggerNewSceneReadyEvent() {
		// Switch to the referenced scene
		_scene.allowSceneActivation = true;
	}

	private void _checkTipsAreReady() {
		if (MM.lang.IsLoadingScreenTipsTextInitialised() && MM.spriteManager.IsLoadingScreenTipsIconsInitialised()) {
			// Fade out the black overlay and display the loading screen
			MM.hud.TriggerFadeOut(0f, MM_Constants.FADE_DURATION);
			MM.SetGameInputLocked(false, InputLockType.SCENEAREA_TRANSITION);
			// Display the first tip
			_tips = JsonHelper.getJsonArray<MM_LoadingScreen_Tip>(MM.lang.GetLoadingScreenTipsJson());
			_setTip();
		}
	}

	private void _setTip() {
		if (!_cancelNextTip) {
			if (_currentTip++ >= _tipCollection.Count) {
				_currentTip = 1;
			}
			MM_LoadingScreen_Tip tip = _getTip();
			if (tip.icon != "") {
				tipIcon.GetComponent<Image>().sprite = MM.spriteManager.GetTipIconSprite(tip.icon);
				tipIcon.gameObject.SetActive(true);
			} else {
				tipIcon.gameObject.SetActive(false);
			}
			tipTitle.GetComponent<TextMeshProUGUI>().text = (tip != null && tip.title != null) ? tip.title : "";
			tipDescription.GetComponent<TextMeshProUGUI>().text = (tip != null && tip.description != null) ? tip.description.Replace("\n\n", "<size=10>\n\n</size>") : "";
			Invoke("_setTip", _TIP_TIMEOUT);
		}
	}

	private void _resourcesAreReady() {
		_resourcesLoaded = true;
	}

	private void _inputReceived(MM_Input input) {
		// Allow the player to press jump to skip through the loading screen as fast as possible
		if (input == MM_Input.JUMP && !_minimumTimeElapsed) {
			_minimumTimeElapsed = true;
		}
	}

	private MM_LoadingScreen_Tip _getTip() {
		System.Random random = new System.Random();
		int requiredTipLevel = 10,
			selectedTip = -1,
			tipLevelAttempts = 0,
			tipShownAttempts = 0;

		// Pick out a random array element
		// Exclude the first tip (that tip contains instructions on how tip data is structured)
		if (_tipCollection.Count == 0) {
			if (!MM.gameManager.IsNewGame) {
				// TODO: Determine the tip level appropriate to the player's progress through the game
			}
			// Build the collection of possible tips to display
			foreach (MM_LoadingScreen_Tip tip in _tips) {
				if (tip.level >= 0 && tip.level <= requiredTipLevel) {
					_tipCollection.Add(tip);
				}
			}
		}

		// Select a tip that has not been seen before in this scene instance
		do {
			selectedTip = random.Next(0, _tipCollection.Count);
		} while ((_tipCollection[selectedTip].level < requiredTipLevel && tipLevelAttempts++ < _RANDOM_ATTEMPTS) || (_tipsShown.Contains(selectedTip) && tipShownAttempts++ < _RANDOM_ATTEMPTS));
		_tipsShown.Add(selectedTip);
		if (_tipsShown.Count > _RANDOM_ATTEMPTS) {
			_tipsShown.RemoveAt(0);
		}
		return _tipCollection[selectedTip];
	}
}
