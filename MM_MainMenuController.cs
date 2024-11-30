using MoreMountains.MMInterface;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MM_MainMenuController : MonoBehaviour {

	class MM_MainMenu_MenuItem {
		public string name;
		public Vector2 position;
		public int upIndex;
		public int downIndex;
		public int leftIndex;
		public int rightIndex;
		public MM_MainMenu_MenuItem(string name, Vector2 position, int upIndex, int downIndex, int leftIndex, int rightIndex) {
			this.name = name;
			this.position = position;
			this.upIndex = upIndex;
			this.downIndex = downIndex;
			this.leftIndex = leftIndex;
			this.rightIndex = rightIndex;
		}
	}

	const int _WIDTH_GOLD_TEXT = 18;
	const int _GOLD_SPACING_X = 25;
	const float _MAIN_MENU_NEW_YPOS = -200f;
	const float _MAIN_MENU_CONTINUE_YPOS = -300f;
	const int _MAIN_MENU_DEFAULT_SELECTION = 2;
	const int _NEW_GAME_DEFAULT_SELECTION = 1;
	const int _CONTINUE_DEFAULT_SELECTION = 0;
	const int _OPTIONS_DEFAULT_SELECTION = 1;
	const int _DEBUG_DEFAULT_SELECTION = 1;
	const float _MAIN_MENU_DISPLAY_DELAY = 0.1f;
	const float _CONTINUE_CONTENT_OFFSET = 80f;
	const float _CONTINUE_CONTENT_HEIGHT = 700f;
	const float _CONTINUE_SLOT_START_YPOS = 274f;
	const float _CONTINUE_SLOT_SPACING = 240f;
	const float _CONTINUE_SLOT_DIVIDER = -200f;
	const float _CONTINUE_SLOT_HEIGHT = 180f;
	const float _CONTINUE_SLOT_PADDING = 30f;
	const float _CONTINUE_SLOT_PLAYED_ICON_XPOS = -180f;
	const string _CONTINUE_SLOT_NAME = "MainMenuSaveSlot";
	const string _CONTINUE_DIVIDER_NAME = "MainMenuSaveSlotDivider";
	const int _CONTINUE_SLOT_MAX = 3;
	const int _MENU_INVALID = -1;
	const int _MENU_HERO = 1;
	const int _MENU_DIFFICULTY = 2;
	const int _MENU_MUSIC = 1;
	const int _MENU_SFX = 2;
	const int _MENU_LANGUAGE_ENGLISH = 3;
	const int _MENU_VOLUME_MUSIC_0 = 20;
	const int _MENU_VOLUME_SFX_0 = 25;
	static readonly float[] _mainMenuYPos = { -200f, -300f, -400f };
	static readonly Dictionary<int, MM_MainMenu_MenuItem> _newGameMenuItems = new Dictionary<int, MM_MainMenu_MenuItem> {
		{ _MENU_HERO, new MM_MainMenu_MenuItem("Hero", new Vector2(-700f, 160f), _MENU_INVALID, _MENU_DIFFICULTY, _MENU_HERO, _MENU_HERO) },
		{ _MENU_DIFFICULTY, new MM_MainMenu_MenuItem("Difficulty", new Vector2(-700f, -226f), _MENU_HERO, _MENU_INVALID, _MENU_DIFFICULTY, _MENU_DIFFICULTY) }
	};
	static readonly Dictionary<int, MM_MainMenu_MenuItem> _optionsMenuItems = new Dictionary<int, MM_MainMenu_MenuItem> {
		{ _MENU_MUSIC, new MM_MainMenu_MenuItem("Music", new Vector2(-690f, 264f), _MENU_INVALID, _MENU_LANGUAGE_ENGLISH, _MENU_INVALID, _MENU_SFX) },
		{ _MENU_SFX, new MM_MainMenu_MenuItem("SFX", new Vector2(-210f, 264f), _MENU_INVALID, _MENU_LANGUAGE_ENGLISH, _MENU_MUSIC, _MENU_INVALID) },
		{ _MENU_LANGUAGE_ENGLISH, new MM_MainMenu_MenuItem("English", new Vector2(-690f, 94f), _MENU_MUSIC, _MENU_INVALID, _MENU_INVALID, _MENU_INVALID) },
		{ _MENU_VOLUME_MUSIC_0, new MM_MainMenu_MenuItem("Music0", new Vector2(-483f, 310f), _MENU_INVALID, _MENU_INVALID, _MENU_INVALID, 21) },
		{ 21, new MM_MainMenu_MenuItem("Music25", new Vector2(-433f, 310f), _MENU_INVALID, _MENU_INVALID, 20, 22) },
		{ 22, new MM_MainMenu_MenuItem("Music50", new Vector2(-383f, 310f), _MENU_INVALID, _MENU_INVALID, 21, 23) },
		{ 23, new MM_MainMenu_MenuItem("Music75", new Vector2(-333f, 310f), _MENU_INVALID, _MENU_INVALID, 22, 24) },
		{ 24, new MM_MainMenu_MenuItem("Music100", new Vector2(-283f, 310f), _MENU_INVALID, _MENU_INVALID, 23, _MENU_INVALID) },
		{ _MENU_VOLUME_SFX_0, new MM_MainMenu_MenuItem("SFX0", new Vector2(-37f, 310f), _MENU_INVALID, _MENU_INVALID, _MENU_INVALID, 26) },
		{ 26, new MM_MainMenu_MenuItem("SFX25", new Vector2(13f, 310f), _MENU_INVALID, _MENU_INVALID, 25, 27) },
		{ 27, new MM_MainMenu_MenuItem("SFX50", new Vector2(63f, 310f), _MENU_INVALID, _MENU_INVALID, 26, 28) },
		{ 28, new MM_MainMenu_MenuItem("SFX75", new Vector2(113f, 310f), _MENU_INVALID, _MENU_INVALID, 27, 29) },
		{ 29, new MM_MainMenu_MenuItem("SFX100", new Vector2(163f, 310f), _MENU_INVALID, _MENU_INVALID, 28, _MENU_INVALID) }
	};
	static readonly Dictionary<int, MM_MainMenu_MenuItem> _debugMenuItems = new Dictionary<int, MM_MainMenu_MenuItem> {
		{ 1, new MM_MainMenu_MenuItem("Location1", new Vector2(-690f, 284f), _MENU_INVALID, 2, _MENU_INVALID, 8) },
		{ 2, new MM_MainMenu_MenuItem("Location2", new Vector2(-690f, 234f), 1, 3, _MENU_INVALID, 9) },
		{ 3, new MM_MainMenu_MenuItem("Location3", new Vector2(-690f, 184f), 2, 4, _MENU_INVALID, 10) },
		{ 4, new MM_MainMenu_MenuItem("Location4", new Vector2(-690f, 134f), 3, 5, _MENU_INVALID, 11) },
		{ 5, new MM_MainMenu_MenuItem("Location5", new Vector2(-690f, 84f), 4, 6, _MENU_INVALID, 12) },
		{ 6, new MM_MainMenu_MenuItem("Location6", new Vector2(-690f, 34f), 5, 7, _MENU_INVALID, 13) },
		{ 7, new MM_MainMenu_MenuItem("Location7", new Vector2(-690f, -16f), 6, 15, _MENU_INVALID, 14) },
		{ 8, new MM_MainMenu_MenuItem("Location8", new Vector2(20f, 284f), _MENU_INVALID, 9, 1, _MENU_INVALID) },
		{ 9, new MM_MainMenu_MenuItem("Location9", new Vector2(20f, 234f), 8, 10, 2, _MENU_INVALID) },
		{ 10, new MM_MainMenu_MenuItem("Location10", new Vector2(20f, 184f), 9, 11, 3, _MENU_INVALID) },
		{ 11, new MM_MainMenu_MenuItem("Location11", new Vector2(20f, 134f), 10, 12, 4, _MENU_INVALID) },
		{ 12, new MM_MainMenu_MenuItem("Location12", new Vector2(20f, 84f), 11, 13, 5, _MENU_INVALID) },
		{ 13, new MM_MainMenu_MenuItem("Location13", new Vector2(20f, 34f), 12, 14, 6, _MENU_INVALID) },
		{ 14, new MM_MainMenu_MenuItem("Location14", new Vector2(20f, -16f), 13, 21, 7, _MENU_INVALID) },
		{ 15, new MM_MainMenu_MenuItem("Location15", new Vector2(-690f, -119f), 7, 16, _MENU_INVALID, 21) },
		{ 16, new MM_MainMenu_MenuItem("Location16", new Vector2(-690f, -169f), 15, 17, _MENU_INVALID, 22) },
		{ 17, new MM_MainMenu_MenuItem("Location17", new Vector2(-690f, -219f), 16, 18, _MENU_INVALID, 23) },
		{ 18, new MM_MainMenu_MenuItem("Location18", new Vector2(-690f, -269f), 17, 19, _MENU_INVALID, 24) },
		{ 19, new MM_MainMenu_MenuItem("Location19", new Vector2(-690f, -319f), 18, 20, _MENU_INVALID, 25) },
		{ 20, new MM_MainMenu_MenuItem("Location20", new Vector2(-690f, -369f), 19, _MENU_INVALID, _MENU_INVALID, 26) },
		{ 21, new MM_MainMenu_MenuItem("Location21", new Vector2(20f, -119f), 14, 22, 15, _MENU_INVALID) },
		{ 22, new MM_MainMenu_MenuItem("Location22", new Vector2(20f, -169f), 21, 23, 16, _MENU_INVALID) },
		{ 23, new MM_MainMenu_MenuItem("Location23", new Vector2(20f, -219f), 22, 24, 17, _MENU_INVALID) },
		{ 24, new MM_MainMenu_MenuItem("Location24", new Vector2(20f, -269f), 23, 25, 18, _MENU_INVALID) },
		{ 25, new MM_MainMenu_MenuItem("Location25", new Vector2(20f, -319f), 24, 26, 19, _MENU_INVALID) },
		{ 26, new MM_MainMenu_MenuItem("Location26", new Vector2(20f, -369f), 25, _MENU_INVALID, 20, _MENU_INVALID) }
	};

	enum PanelType {
		NONE,
		MAIN,
		NEW_GAME,
		CONTINUE,
		OPTIONS,
		DEBUG
	}

	[Header("Menu Panels")]
	public GameObject MainPanel;
	public GameObject NewGamePanel;
	public GameObject ContinuePanel;
	public GameObject OptionsPanel;
	public GameObject DebugPanel;
	public GameObject ActionsList;

	Animator _anim;
	Animator _glow;
	Animator _blur;
	private bool _skipNextGlowAnimation;
	private PanelType _currentPanelType;
	private RectTransform _mainCursor;
	private RectTransform _newGameCursor;
	private RectTransform _continueCursor;
	private RectTransform _optionsCursor;
	private RectTransform _debugCursor;
	private RectTransform _playerSelectionLeft;
	private RectTransform _playerSelectionRight;
	private RectTransform _difficultySelectionLeft;
	private RectTransform _difficultySelectionRight;
	private RectTransform _continueSavedGamesContent;
	private int _selectedSavedGameSlot;
	private int _continueSavedGamesCount;
	private List<int> _continueSavedGamesKeys;
	private bool _isMainPanelContinueAvailable;
	private int _currentCursorPosition;
	private InputDirection _currentInputDirection;
	private IEnumerator _cursorMovementCoroutine;
	private bool _cursorMovementCoroutineRunning;
	private bool _ignoreLock;
	private int _currentHeroSelection = MM_Constants.PLAYER_BOCK;
	private int _currentGameDifficulty = MM_Constants.DIFFICULTY_NORMAL;
	private float _originalMusicVolume;
	private float _originalSFXVolume;

	private void OnEnable() {
		MM_Events.OnSceneReady += _sceneReady;
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnStoredGameLoaded += _loadExistingGame;
	}

	private void OnDisable() {
		MM_Events.OnSceneReady -= _sceneReady;
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnStoredGameLoaded -= _loadExistingGame;
	}

	public void Awake() {
		Cursor.visible = false;
		// Check for the global objects from the preload scene
		// and if not found then restart at the preload scene
		if (GameObject.Find("_app") == null) {
			UnityEngine.SceneManagement.SceneManager.LoadScene("_preload");
		}
	}

	void Start() {
		MM.MainCamera = Camera.main;
		_anim = GetComponent<Animator>();
		_blur = MM.MainCamera.GetComponent<Animator>();
		Time.timeScale = MM_Constants.TIMESCALE_NORMAL;
		MM.gameManager.SetVolumeModifier(1f);
		MM.Player = null;

		// Set the main menu canvas to use the UI camera in the preload scene
		transform.GetComponent<Canvas>().worldCamera = MM.UICamera;

		// Remove the "Continue" option if no previous saved games are available
		Transform newOptionTransform = MainPanel.transform.Find("New").transform;
		if (!MM.gameManager.IsSavedGamesAvailable) {
			MainPanel.transform.Find("Continue").gameObject.SetActive(false);
			newOptionTransform.transform.localPosition = new Vector3(newOptionTransform.transform.localPosition.x, _MAIN_MENU_CONTINUE_YPOS, newOptionTransform.transform.localPosition.z);
			_isMainPanelContinueAvailable = false;
		} else {
			newOptionTransform.transform.localPosition = new Vector3(newOptionTransform.transform.localPosition.x, _MAIN_MENU_NEW_YPOS, newOptionTransform.transform.localPosition.z);
			MainPanel.transform.Find("Continue").gameObject.SetActive(true);
			_isMainPanelContinueAvailable = true;
		}

		_glow = MainPanel.transform.Find("Glow").GetComponent<Animator>();
		_mainCursor = MainPanel.transform.Find("Cursor").GetComponent<RectTransform>();
		_newGameCursor = NewGamePanel.transform.Find("Cursor").GetComponent<RectTransform>();
		_continueCursor = ContinuePanel.transform.Find("Cursor").GetComponent<RectTransform>();
		_optionsCursor = OptionsPanel.transform.Find("Cursor").GetComponent<RectTransform>();
		_debugCursor = DebugPanel.transform.Find("Cursor").GetComponent<RectTransform>();
		_playerSelectionLeft = NewGamePanel.transform.Find("Container/PlayerSelectionLeft").GetComponent<RectTransform>();
		_playerSelectionRight = NewGamePanel.transform.Find("Container/PlayerSelectionRight").GetComponent<RectTransform>();
		_difficultySelectionLeft = NewGamePanel.transform.Find("Container/DifficultySelectionLeft").GetComponent<RectTransform>();
		_difficultySelectionRight = NewGamePanel.transform.Find("Container/DifficultySelectionRight").GetComponent<RectTransform>();
		_continueSavedGamesContent = ContinuePanel.transform.Find("Container/SavedGamesContainer/SavedGamesContent").GetComponent<RectTransform>();

		// Turn off gameobjects that are not required on start
		for (int i = 0; i < _CONTINUE_SLOT_MAX; i++) {
			_continueSavedGamesContent.Find(_CONTINUE_SLOT_NAME + i.ToString()).gameObject.SetActive(false);
			if (i < (_CONTINUE_SLOT_MAX - 1)) {
				_continueSavedGamesContent.Find(_CONTINUE_DIVIDER_NAME + i.ToString()).gameObject.SetActive(false);
			}
		}
		NewGamePanel.SetActive(false);
		OptionsPanel.SetActive(false);
		ContinuePanel.SetActive(false);
		DebugPanel.SetActive(false);
		ActionsList.SetActive(false);

		// Initialise the cursor/panel settings
		_currentPanelType = PanelType.MAIN;
		_setCursorPosition(_currentPanelType, _MAIN_MENU_DEFAULT_SELECTION, false);
		_cursorMovementCoroutineRunning = false;
		_skipNextGlowAnimation = false;
		_ignoreLock = false;

		// Setup an event group listener to trigger a SCENE_READY event when everything is ready
		List<MM_Event> setupList = new List<MM_Event> {
			MM_Event.DELAY_COMPLETE
		};
		if (!MM.areaManager.IsCommonAreaInitialised) {
			setupList.Add(MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE);
		}
		if (!MM.soundEffects.IsBasicSetInitialised) {
			setupList.Add(MM_Event.SFX_BASIC_PRELOAD_COMPLETE);
		}
		MM.events.AddEventGroupListener(setupList, MM_Event.SCENE_READY);

		// Perform all other tasks that need to be performed before fading the scene in
		// The _sceneReady function will be called when everything has been prepared
		if (!MM.areaManager.IsCommonAreaInitialised) {
			MM.areaManager.LoadCommonBundle();
		}
		if (!MM.soundEffects.IsBasicSetInitialised) {
			MM.soundEffects.PreloadBasicSet();
		}

		MM.events.StartDelay(_MAIN_MENU_DISPLAY_DELAY);
	}

	private void _fadeIntoLoadingScreen() {
		SceneManager.LoadSceneAsync(MM_Constants.SCENE_LOADING).allowSceneActivation = false;
		MM.hud.TriggerFadeIn(0f, MM_Constants.FADE_DURATION);
		Invoke("_activateLoadingScene", 1f);
	}

	private void _activateLoadingScene() {
		SceneManager.LoadScene(MM_Constants.SCENE_LOADING);
	}

	private void _sceneReady() {
		// Turn on weather sounds
		_playAmbientSFX(true);
		// Preload the music track
		MM.music.Preload(MusicalTrack.COMMON_MENU);
		// Ensure the HUD is hidden
		MM.hud.Hide(HUDFadeType.IMMEDIATE);
		MM.hud.ResetState();
		// Release input and show the scene
		MM.SetMenuInputLocked(false);
		MM.hud.TriggerFadeOut(0f, MM_Constants.FADE_DURATION);
	}

	private void _playAmbientSFX(bool play = true) {
		try {
			if (play) {
				EnviroSky.instance.TryPlayAmbientSFX();
			} else {
				EnviroSky.instance.StopAmbientSFX();
			}
		} catch (Exception e) {
			Debug.LogError("MM_MainMenuController:_playAmbientSFX(" + play.ToString() + ") : EnviroSky cannot play/stop ambient sfx : " + e.Message);
		}

	}

	private void _updateActionsList(int position = _MENU_INVALID) {
		if (_currentPanelType == PanelType.MAIN || _currentPanelType == PanelType.NONE) {
			ActionsList.SetActive(false);
		} else {
			foreach (Transform child in ActionsList.transform) {
				child.gameObject.SetActive(false);
			}
			switch (_currentPanelType) {
				case PanelType.NEW_GAME:
					ActionsList.transform.Find("MainMenuStartGame").gameObject.SetActive(true);
					ActionsList.transform.Find("MainMenuBack").gameObject.SetActive(true);
					break;
				case PanelType.CONTINUE:
					ActionsList.transform.Find("MainMenuStartGame").gameObject.SetActive(true);
					ActionsList.transform.Find("MainMenuBack").gameObject.SetActive(true);
					break;
				case PanelType.OPTIONS:
					if (position >= _MENU_MUSIC && position <= _MENU_SFX) {
						ActionsList.transform.Find("MainMenuSetVolume").gameObject.SetActive(true);
						ActionsList.transform.Find("MainMenuBack").gameObject.SetActive(true);
					} else if (position >= _MENU_LANGUAGE_ENGLISH && position <= _MENU_LANGUAGE_ENGLISH) {
						ActionsList.transform.Find("MainMenuSetLanguage").gameObject.SetActive(true);
						ActionsList.transform.Find("MainMenuBack").gameObject.SetActive(true);
					} else if (position >= _MENU_VOLUME_MUSIC_0 && position <= _MENU_VOLUME_SFX_0 + 4) {
						ActionsList.transform.Find("MainMenuSelect").gameObject.SetActive(true);
						ActionsList.transform.Find("MainMenuCancel").gameObject.SetActive(true);
					}
					break;
				case PanelType.DEBUG:
					ActionsList.transform.Find("MainMenuStartGame").gameObject.SetActive(true);
					ActionsList.transform.Find("MainMenuBack").gameObject.SetActive(true);
					break;
			}
			ActionsList.SetActive(true);
		}
	}

	private void _populateNewGamePanel_Hero() {
		_playerSelectionLeft.GetComponent<CanvasGroup>().alpha = 0f;
		_playerSelectionRight.GetComponent<CanvasGroup>().alpha = 0f;
		switch (_currentHeroSelection) {
			case MM_Constants.PLAYER_BOCK:
				NewGamePanel.transform.Find("Container/Bock").gameObject.SetActive(true);
				NewGamePanel.transform.Find("Container/Tanya").gameObject.SetActive(false);
				break;
			case MM_Constants.PLAYER_TANYA:
				NewGamePanel.transform.Find("Container/Bock").gameObject.SetActive(false);
				NewGamePanel.transform.Find("Container/Tanya").gameObject.SetActive(true);
				break;
		}
	}

	private void _populateNewGamePanel_Difficulty() {
		_difficultySelectionLeft.GetComponent<CanvasGroup>().alpha = 0f;
		_difficultySelectionRight.GetComponent<CanvasGroup>().alpha = 0f;
		switch (_currentGameDifficulty) {
			case MM_Constants.DIFFICULTY_NORMAL:
				NewGamePanel.transform.Find("Container/Normal").gameObject.SetActive(true);
				NewGamePanel.transform.Find("Container/Legendary").gameObject.SetActive(false);
				NewGamePanel.transform.Find("Container/Insane").gameObject.SetActive(false);
				break;
			case MM_Constants.DIFFICULTY_LEGENDARY:
				NewGamePanel.transform.Find("Container/Normal").gameObject.SetActive(false);
				NewGamePanel.transform.Find("Container/Legendary").gameObject.SetActive(true);
				NewGamePanel.transform.Find("Container/Insane").gameObject.SetActive(false);
				break;
			case MM_Constants.DIFFICULTY_INSANE:
				NewGamePanel.transform.Find("Container/Normal").gameObject.SetActive(false);
				NewGamePanel.transform.Find("Container/Legendary").gameObject.SetActive(false);
				NewGamePanel.transform.Find("Container/Insane").gameObject.SetActive(true);
				break;
		}
	}

	private void _populateContinuePanel() {
		_continueSavedGamesCount = Math.Min(MM.gameManager.SavedGamesCount, _CONTINUE_SLOT_MAX);
		_continueSavedGamesKeys = MM.gameManager.SavedGamesByDateDesc;
		Debug.Log("MM_MainMenuController:_populateContinuePanel : displaying saved game details for slots [" + String.Join(",", _continueSavedGamesKeys.ToArray().Select(i => i.ToString()).ToArray()) + "]");

		// Set the height of the saved games container
		_continueSavedGamesContent.sizeDelta = new Vector2(_continueSavedGamesContent.sizeDelta.x, _continueSavedGamesCount * (_CONTINUE_SLOT_HEIGHT + (_CONTINUE_SLOT_PADDING * 2)));

		// Enable the save slots in the container and set their content
		GameObject slotObject;
		for (int i = 0; i < _continueSavedGamesCount; i++) {
			slotObject = _continueSavedGamesContent.Find(_CONTINUE_SLOT_NAME + i.ToString()).gameObject;
			if (slotObject) {
				_populateContinueSaveSlot(slotObject, MM.gameManager.GetSavedGame(_continueSavedGamesKeys[i]));
				slotObject.SetActive(true);
			}
			if (i < _continueSavedGamesCount - 1) {
				_continueSavedGamesContent.Find(_CONTINUE_DIVIDER_NAME + i.ToString()).gameObject.SetActive(true);
			}
		}
	}

	private void _populateContinueSaveSlot(GameObject slotObject, MM_PlayerState savedPlayerState) {
		// Save game image
		Texture2D mainCameraTexture = new Texture2D(MM_Constants.SCREENSHOT_WIDTH, MM_Constants.SCREENSHOT_HEIGHT, TextureFormat.ARGB32, false);
		mainCameraTexture.LoadRawTextureData(savedPlayerState.SaveScreenshot);
		mainCameraTexture.Apply();
		slotObject.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(mainCameraTexture, new Rect(0, 0, MM_Constants.SCREENSHOT_WIDTH, MM_Constants.SCREENSHOT_HEIGHT), new Vector2(0, 1));

		// Player portrait image
		if (savedPlayerState.Hero == MM_Constants.PLAYER_BOCK) {
			slotObject.transform.Find("PlayerPortrait/Bock").gameObject.SetActive(true);
			slotObject.transform.Find("PlayerPortrait/Tanya").gameObject.SetActive(false);
		} else {
			slotObject.transform.Find("PlayerPortrait/Bock").gameObject.SetActive(false);
			slotObject.transform.Find("PlayerPortrait/Tanya").gameObject.SetActive(true);
		}

		// Area name
		slotObject.transform.Find("Area").GetComponent<TextMeshProUGUI>().text = MM.lang.GetAreaName((int)savedPlayerState.CurrentSceneArea);

		// Last saved time
		slotObject.transform.Find("SavedTime").GetComponent<TextMeshProUGUI>().text =
			((savedPlayerState.LastSaveTime.Date == DateTime.Today) ? MM_Constants.TEXT_TODAY : savedPlayerState.LastSaveTime.ToString("dddd"))
			+ ", " + savedPlayerState.LastSaveTime.ToString("h:mmtt").ToLower();

		// Played time
		string playedTime = "";
		TimeSpan t = TimeSpan.FromSeconds(savedPlayerState.PlayedTime);
		TextMeshProUGUI playedTimeText = slotObject.transform.Find("AdditionalData/PlayedTime").GetComponent<TextMeshProUGUI>();
		if (savedPlayerState.PlayedTime < MM_Constants.ONE_MINUTE_IN_SECONDS) {
			playedTime = string.Format("{0:D1}s", t.Seconds);
		} else if (savedPlayerState.PlayedTime < MM_Constants.ONE_HOUR_IN_SECONDS) {
			playedTime = string.Format("{0:D1}m:{1:D2}s", t.Minutes, t.Seconds);
		} else {
			playedTime = string.Format("{0:D1}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
		}
		playedTimeText.text = playedTime;

		// Difficulty
		Transform difficulty = slotObject.transform.Find("Difficulty");
		switch (savedPlayerState.Difficulty) {
			case MM_Constants.DIFFICULTY_NORMAL:
				difficulty.GetChild(0).gameObject.SetActive(true);
				difficulty.GetChild(1).gameObject.SetActive(false);
				difficulty.GetChild(2).gameObject.SetActive(false);
				break;
			case MM_Constants.DIFFICULTY_LEGENDARY:
				difficulty.GetChild(0).gameObject.SetActive(false);
				difficulty.GetChild(1).gameObject.SetActive(true);
				difficulty.GetChild(2).gameObject.SetActive(false);
				break;
			case MM_Constants.DIFFICULTY_INSANE:
				difficulty.GetChild(0).gameObject.SetActive(false);
				difficulty.GetChild(1).gameObject.SetActive(false);
				difficulty.GetChild(2).gameObject.SetActive(true);
				break;
		}

		slotObject.transform.Find("AdditionalData/ElixirAmount").GetComponent<TextMeshProUGUI>().text = savedPlayerState.Elixirs.ToString(MM_Constants.ELIXIR_AMOUNT_FORMAT);
		slotObject.transform.Find("AdditionalData/CharmstoneAmount").GetComponent<TextMeshProUGUI>().text = savedPlayerState.Charmstones.ToString(MM_Constants.CHARMSTONE_AMOUNT_FORMAT);
		slotObject.transform.Find("AdditionalData/HeartAmount").GetComponent<TextMeshProUGUI>().text = savedPlayerState.GetHeartContainersAmount(true).ToString(MM_Constants.HEART_AMOUNT_FORMAT);
		Transform gold = slotObject.transform.Find("GoldAmount");
		TextMeshProUGUI goldText = gold.GetComponent<TextMeshProUGUI>();
		Transform additionalData = slotObject.transform.Find("AdditionalData");
		goldText.text = savedPlayerState.Gold.ToString(MM_Constants.GOLD_AMOUNT_FORMAT);
		additionalData.localPosition = new Vector3(gold.localPosition.x + _GOLD_SPACING_X + (_WIDTH_GOLD_TEXT * goldText.text.Length), additionalData.localPosition.y, additionalData.localPosition.z);
	}

	private void _populateOptionsPanel() {
		try {
			// Volume
			OptionsPanel.transform.Find("Container/Music/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeMusic;
			OptionsPanel.transform.Find("Container/SFX/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeSFX;

			// Language
			TextMeshProUGUI englishText = OptionsPanel.transform.Find("Container/English").GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI japaneseText = OptionsPanel.transform.Find("Container/Japanese").GetComponent<TextMeshProUGUI>();
			if (MM.gameManager.Language == MM_Language.ENGLISH) {
				englishText.color = MM_Constants.MENU_HIGHLIGHT_COLOR;
				japaneseText.color = MM_Constants.MENU_INACTIVE_COLOR;
			} else if (MM.gameManager.Language == MM_Language.JAPANESE) {
				englishText.color = MM_Constants.MENU_INACTIVE_COLOR;
				japaneseText.color = MM_Constants.MENU_HIGHLIGHT_COLOR;
			}
		} catch (Exception e) {
			Debug.LogError("MM_MainMenuController:_populateOptionsPanel : cannot populate options : " + e.Message);
		}
	}

	private void _setCursorPosition(Func<int, int, int> action, PanelType panel, int position, bool doTween, float rotate = 0f, bool doPlaySFX = true) {
		if (action != null) {
			position = action(_currentCursorPosition, position);
		}
		if (position > _MENU_INVALID) {
			_setCursorPosition(panel, position, doTween, rotate, doPlaySFX);
		}
	}

	private void _setCursorPosition(Func<MM_Input, int, int, int> action, MM_Input input, PanelType panel, int position, bool doTween, float rotate = 0f, bool doPlaySFX = true) {
		if (action != null) {
			position = action(input, _currentCursorPosition, position);
		}
		if (position > _MENU_INVALID) {
			_setCursorPosition(panel, position, doTween, rotate, doPlaySFX);
		}
	}

	private void _setCursorPosition(PanelType panel, int position, bool doTween, float rotate = 0f, bool doPlaySFX = true) {
		_currentCursorPosition = position;

		// Move the cursor to the new position
		switch (panel) {
			case PanelType.MAIN:
				if (doTween) {
					iTween.MoveTo(_mainCursor.gameObject, iTween.Hash("y", _mainMenuYPos[_currentCursorPosition - 1], "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
				} else {
					_mainCursor.localPosition = new Vector3(_mainCursor.localPosition.x, _mainMenuYPos[_currentCursorPosition - 1], _mainCursor.localPosition.z);
				}
				break;
			case PanelType.NEW_GAME:
				if (doTween) {
					iTween.MoveTo(_newGameCursor.gameObject, iTween.Hash("x", _newGameMenuItems[_currentCursorPosition].position.x, "y", _newGameMenuItems[_currentCursorPosition].position.y, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
				} else {
					_newGameCursor.localPosition = new Vector3(_newGameMenuItems[_currentCursorPosition].position.x, _newGameMenuItems[_currentCursorPosition].position.y, _newGameCursor.localPosition.z);
				}
				break;
			case PanelType.CONTINUE:
				// Use calculated coordinates for the continue screen (which also needs to control the scrolling item window)
				bool moveCursor;
				float yMoveTo;
				float yContent = _continueSavedGamesContent.localPosition.y - _CONTINUE_CONTENT_OFFSET;
				float rowPosition = _currentCursorPosition * _CONTINUE_SLOT_SPACING;
				if (rowPosition >= yContent && rowPosition < yContent + _CONTINUE_CONTENT_HEIGHT) {
					moveCursor = true;
					yMoveTo = _CONTINUE_SLOT_START_YPOS - rowPosition + yContent;
				} else {
					moveCursor = false;
					yMoveTo = _CONTINUE_CONTENT_OFFSET + yContent + ((rowPosition < yContent) ? -_CONTINUE_SLOT_SPACING : _CONTINUE_SLOT_SPACING);
				}
				if (doTween) {
					if (moveCursor) {
						iTween.MoveTo(_continueCursor.gameObject, iTween.Hash("x", _continueCursor.anchoredPosition.x, "y", yMoveTo, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
					} else {
						iTween.MoveTo(_continueSavedGamesContent.gameObject, iTween.Hash("y", yMoveTo, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
					}
				} else {
					if (moveCursor) {
						_continueCursor.localPosition = new Vector3(_continueCursor.anchoredPosition.x, yMoveTo, _continueCursor.localPosition.z);
					} else {
						_continueSavedGamesContent.localPosition = new Vector3(_continueSavedGamesContent.localPosition.x, yMoveTo, _continueSavedGamesContent.localPosition.z);
					}
				}
				break;
			case PanelType.OPTIONS:
				if (doTween) {
					iTween.MoveTo(_optionsCursor.gameObject, iTween.Hash("x", _optionsMenuItems[_currentCursorPosition].position.x, "y", _optionsMenuItems[_currentCursorPosition].position.y, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
					if (rotate != 0f) {
						iTween.RotateAdd(_optionsCursor.gameObject, new Vector3(0f, 0f, rotate), MM_Constants.CURSOR_TWEEN_DURATION);
					}
				} else {
					_optionsCursor.localPosition = new Vector3(_optionsMenuItems[_currentCursorPosition].position.x, _optionsMenuItems[_currentCursorPosition].position.y, _optionsCursor.localPosition.z);
				}
				break;
			case PanelType.DEBUG:
				if (doTween) {
					iTween.MoveTo(_debugCursor.gameObject, iTween.Hash("x", _debugMenuItems[_currentCursorPosition].position.x, "y", _debugMenuItems[_currentCursorPosition].position.y, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "time", MM_Constants.CURSOR_TWEEN_DURATION));
				} else {
					_debugCursor.localPosition = new Vector3(_debugMenuItems[_currentCursorPosition].position.x, _debugMenuItems[_currentCursorPosition].position.y, _debugCursor.localPosition.z);
				}
				break;
		}
		// If animating then play the cursor movement sound effect
		if (doTween && doPlaySFX) {
			MM.soundEffects.Play(MM_SFX.MENU_MOVE);
		}
	}

	private bool _isInputLocked() {
		if (MM.IsMenuInputLocked) {
			return true;
		}

		// If the cursor is currently moving then input should be locked
		RectTransform cursor = null;
		switch (_currentPanelType) {
			case PanelType.MAIN:
				cursor = _mainCursor;
				break;
			case PanelType.NEW_GAME:
				cursor = _newGameCursor;
				break;
			case PanelType.CONTINUE:
				cursor = _continueCursor;
				break;
			case PanelType.OPTIONS:
				cursor = _optionsCursor;
				break;
			case PanelType.DEBUG:
				cursor = _debugCursor;
				break;
		}
		if (cursor != null && cursor.gameObject.GetComponent<iTween>() != null) {
			return true;
		}
		return false;
	}

	private void _actionCurrentSelection() {
		Button button = null;
		switch (_currentPanelType) {
			case PanelType.MAIN:
				MM.soundEffects.Play(MM_SFX.MENU_SELECT);
				switch (_currentCursorPosition) {
					case 1:
						button = MainPanel.transform.Find("New").GetComponent<Button>();
						break;
					case 2:
						if (_isMainPanelContinueAvailable) {
							button = MainPanel.transform.Find("Continue").GetComponent<Button>();
						} else {
							button = MainPanel.transform.Find("New").GetComponent<Button>();
						}
						break;
					case 3:
						button = MainPanel.transform.Find("Options").GetComponent<Button>();
						break;
					case 4:
						button = MainPanel.transform.Find("Exit").GetComponent<Button>();
						break;
				}
				break;
			case PanelType.NEW_GAME:
				NewGame();
				break;
			case PanelType.CONTINUE:
				LoadGame();
				break;
			case PanelType.OPTIONS:
				MM.soundEffects.Play(MM_SFX.MENU_SELECT);
				switch (_currentCursorPosition) {
					case _MENU_MUSIC:
						_originalMusicVolume = MM.gameManager.VolumeMusic;
						int musicIndex = (_MENU_VOLUME_MUSIC_0 + (int)(MM.gameManager.VolumeMusic * 4));
						_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, musicIndex, true, -90);
						break;
					case _MENU_VOLUME_MUSIC_0:
					case _MENU_VOLUME_MUSIC_0 + 1:
					case _MENU_VOLUME_MUSIC_0 + 2:
					case _MENU_VOLUME_MUSIC_0 + 3:
					case _MENU_VOLUME_MUSIC_0 + 4:
						// Save the current game settings into preferences so that the new volume setting is stored
						MM.gameManager.SaveGameSettings();
						// Update the slider
						OptionsPanel.transform.Find("Container/Music/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeMusic;
						// Move back to the parent
						_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, _MENU_MUSIC, true, 90);
						break;
					case _MENU_SFX:
						_originalSFXVolume = MM.gameManager.VolumeSFX;
						int sfxIndex = _MENU_VOLUME_SFX_0 + (int)(MM.gameManager.VolumeSFX * 4);
						_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, sfxIndex, true, -90);
						break;
					case _MENU_VOLUME_SFX_0:
					case _MENU_VOLUME_SFX_0 + 1:
					case _MENU_VOLUME_SFX_0 + 2:
					case _MENU_VOLUME_SFX_0 + 3:
					case _MENU_VOLUME_SFX_0 + 4:
						// Save the current game settings into preferences so that the new volume setting is stored
						MM.gameManager.SaveGameSettings();
						// Update the slider
						OptionsPanel.transform.Find("Container/SFX/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeSFX;
						// Move back to the parent
						_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, _MENU_SFX, true, 90);
						break;
				}
				break;
			case PanelType.DEBUG:
				button = DebugPanel.transform.Find("Container/" + _debugMenuItems[_currentCursorPosition - 1].name).GetComponent<Button>();
				break;
		}
		if (button != null) {
			button.onClick.Invoke();
		}
	}

	private void _cancelCurrentSelection() {
		switch (_currentPanelType) {
			case PanelType.NEW_GAME:
				MM.soundEffects.Play(MM_SFX.MENU_CLOSE);
				closeNewGamePanel();
				break;
			case PanelType.CONTINUE:
				MM.soundEffects.Play(MM_SFX.MENU_CLOSE);
				closeContinuePanel();
				break;
			case PanelType.OPTIONS:
				MM.soundEffects.Play(MM_SFX.MENU_CLOSE);
				if (_currentCursorPosition >= _MENU_VOLUME_MUSIC_0 && _currentCursorPosition <= _MENU_VOLUME_MUSIC_0 + 4) {
					// Revert the game volume
					MM.gameManager.VolumeMusic = _originalMusicVolume;
					OptionsPanel.transform.Find("Container/Music/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeMusic;
					_setCursorPosition(_currentPanelType, _MENU_MUSIC, true, 90f, false);
					_updateActionsList(_currentCursorPosition);
				} else if (_currentCursorPosition >= _MENU_VOLUME_SFX_0 && _currentCursorPosition <= _MENU_VOLUME_SFX_0 + 4) {
					// Revert the game volume
					MM.gameManager.VolumeSFX = _originalSFXVolume;
					OptionsPanel.transform.Find("Container/SFX/Slider").GetComponent<Slider>().value = MM.gameManager.VolumeSFX;
					_setCursorPosition(_currentPanelType, _MENU_SFX, true, 90f, false);
					_updateActionsList(_currentCursorPosition);
				} else {
					closeOptionsPanel();
				}
				break;
			case PanelType.DEBUG:
				MM.soundEffects.Play(MM_SFX.MENU_CLOSE);
				closeDebugPanel();
				break;
		}
	}

	private int _checkCursorPosition_NewGame(MM_Input input, int from, int to) {
		// This code really should be cleaned up and optimised
		// When moving onto the hero selection row enable the appropriate selection arrows
		if (to == _MENU_HERO && (from != _MENU_HERO || (from == _MENU_HERO && (input == MM_Input.LEFT || input == MM_Input.RIGHT)))) {
			if (input == MM_Input.LEFT) {
				if (_currentHeroSelection > MM_Constants.PLAYER_BOCK) {
					if (_currentHeroSelection == MM_Constants.PLAYER_TANYA) {
						_playerSelectionRight.GetComponent<Animator>().Play("FadeIn");
					}
					_currentHeroSelection--;
					if (_currentHeroSelection == MM_Constants.PLAYER_BOCK) {
						_playerSelectionLeft.GetComponent<Animator>().Play("FadeOut");
					}
					_populateNewGamePanel_Hero();
				} else {
					return _MENU_INVALID;
				}
			} else if (input == MM_Input.RIGHT) {
				if (_currentHeroSelection < MM_Constants.PLAYER_TANYA) {
					if (_currentHeroSelection == MM_Constants.PLAYER_BOCK) {
						_playerSelectionLeft.GetComponent<Animator>().Play("FadeIn");
					}
					_currentHeroSelection++;
					if (_currentHeroSelection == MM_Constants.PLAYER_TANYA) {
						_playerSelectionRight.GetComponent<Animator>().Play("FadeOut");
					}
					_populateNewGamePanel_Hero();
				} else {
					return _MENU_INVALID;
				}
			}
			if (to != from) {
				if (_currentHeroSelection == MM_Constants.PLAYER_BOCK) {
					_playerSelectionRight.GetComponent<Animator>().Play("FadeIn");
				} else if (_currentHeroSelection == MM_Constants.PLAYER_TANYA) {
					_playerSelectionLeft.GetComponent<Animator>().Play("FadeIn");
				} else {
					_playerSelectionLeft.GetComponent<Animator>().Play("FadeIn");
					_playerSelectionRight.GetComponent<Animator>().Play("FadeIn");
				}
			}
		}
		// Fade the player selection arrows if moving to another row
		else if (from == _MENU_HERO && to != from) {
			if (_currentHeroSelection == MM_Constants.PLAYER_BOCK) {
				_playerSelectionRight.GetComponent<Animator>().Play("FadeOut");
			} else if (_currentHeroSelection == MM_Constants.PLAYER_TANYA) {
				_playerSelectionLeft.GetComponent<Animator>().Play("FadeOut");
			} else {
				_playerSelectionLeft.GetComponent<Animator>().Play("FadeOut");
				_playerSelectionRight.GetComponent<Animator>().Play("FadeOut");
			}
		}

		// When moving onto the game difficulty row enable the appropriate selection arrows
		if (to == _MENU_DIFFICULTY && (from != _MENU_DIFFICULTY || (from == _MENU_DIFFICULTY && (input == MM_Input.LEFT || input == MM_Input.RIGHT)))) {
			if (input == MM_Input.LEFT) {
				if (_currentGameDifficulty > MM_Constants.DIFFICULTY_NORMAL) {
					if (_currentGameDifficulty == MM_Constants.DIFFICULTY_INSANE) {
						_difficultySelectionRight.GetComponent<Animator>().Play("FadeIn");
					}
					_currentGameDifficulty--;
					if (_currentGameDifficulty == MM_Constants.DIFFICULTY_NORMAL) {
						_difficultySelectionLeft.GetComponent<Animator>().Play("FadeOut");
					}
					_populateNewGamePanel_Difficulty();
				} else {
					return _MENU_INVALID;
				}
			} else if (input == MM_Input.RIGHT) {
				if (_currentGameDifficulty < MM_Constants.DIFFICULTY_INSANE) {
					if (_currentGameDifficulty == MM_Constants.DIFFICULTY_NORMAL) {
						_difficultySelectionLeft.GetComponent<Animator>().Play("FadeIn");
					}
					_currentGameDifficulty++;
					if (_currentGameDifficulty == MM_Constants.DIFFICULTY_INSANE) {
						_difficultySelectionRight.GetComponent<Animator>().Play("FadeOut");
					}
					_populateNewGamePanel_Difficulty();
				} else {
					return _MENU_INVALID;
				}
			}
			if (to != from) {
				if (_currentGameDifficulty == MM_Constants.DIFFICULTY_NORMAL) {
					_difficultySelectionRight.GetComponent<Animator>().Play("FadeIn");
				} else if (_currentGameDifficulty == MM_Constants.DIFFICULTY_INSANE) {
					_difficultySelectionLeft.GetComponent<Animator>().Play("FadeIn");
				} else {
					_difficultySelectionLeft.GetComponent<Animator>().Play("FadeIn");
					_difficultySelectionRight.GetComponent<Animator>().Play("FadeIn");
				}
			}
		}
		// Fade the difficulty selection arrows if moving to another row
		else if (from == _MENU_DIFFICULTY && to != from) {
			if (_currentGameDifficulty == MM_Constants.DIFFICULTY_NORMAL) {
				_difficultySelectionRight.GetComponent<Animator>().Play("FadeOut");
			} else if (_currentGameDifficulty == MM_Constants.DIFFICULTY_INSANE) {
				_difficultySelectionLeft.GetComponent<Animator>().Play("FadeOut");
			} else {
				_difficultySelectionLeft.GetComponent<Animator>().Play("FadeOut");
				_difficultySelectionRight.GetComponent<Animator>().Play("FadeOut");
			}
		}

		return to;
	}

	private int _checkCursorPosition_Options(int from, int to) {
		// When moving on a volume slider update the game volume
		if (to >= _MENU_VOLUME_MUSIC_0 && to <= _MENU_VOLUME_MUSIC_0 + 4) {
			MM.gameManager.VolumeMusic = (to - _MENU_VOLUME_MUSIC_0) / 4f;
		} else if (to >= _MENU_VOLUME_SFX_0 && to <= _MENU_VOLUME_SFX_0 + 4) {
			MM.gameManager.VolumeSFX = (to - _MENU_VOLUME_SFX_0) / 4f;
		}
		// When moving onto the language row automatically move to the current selected language
		if (to >= _MENU_LANGUAGE_ENGLISH && to <= _MENU_LANGUAGE_ENGLISH && (from < _MENU_LANGUAGE_ENGLISH || from > _MENU_LANGUAGE_ENGLISH)) {
			to = (MM.gameManager.Language == MM_Language.ENGLISH ? _MENU_LANGUAGE_ENGLISH : _MENU_LANGUAGE_ENGLISH);
		}
		// Update the actions list
		if ((to >= _MENU_MUSIC && to <= _MENU_SFX && (from < _MENU_MUSIC || from > _MENU_SFX))
			|| (to >= _MENU_LANGUAGE_ENGLISH && to <= _MENU_LANGUAGE_ENGLISH && (from < _MENU_LANGUAGE_ENGLISH || from > _MENU_LANGUAGE_ENGLISH))
			|| (to >= _MENU_VOLUME_MUSIC_0 && to <= _MENU_VOLUME_SFX_0 + 4 && (from < _MENU_VOLUME_MUSIC_0 || from > _MENU_VOLUME_SFX_0 + 4))) {
			_updateActionsList(to);
		}
		return to;
	}

	private void _inputReceived(MM_Input input) {
		MM_MainMenu_MenuItem currentMenuItem;
		InputDirection movingDirection = InputDirection.NONE;
		int oldCursorPosition = _currentCursorPosition;

		// Determine the panel, whether the cursor can move, and finally if input is allowed
		// Input is checked last in order to be more efficient as it needs to check the tween component status
		switch (_currentPanelType) {
			case PanelType.MAIN:
				if (input == MM_Input.UP && _currentCursorPosition > (_isMainPanelContinueAvailable ? _MAIN_MENU_DEFAULT_SELECTION - 1 : _MAIN_MENU_DEFAULT_SELECTION) && (!_isInputLocked() || _ignoreLock)) {
					movingDirection = InputDirection.UP;
					_setCursorPosition(_currentPanelType, _currentCursorPosition - 1, true);
				} else if (input == MM_Input.DOWN && _currentCursorPosition < (_MAIN_MENU_DEFAULT_SELECTION + 1) && (!_isInputLocked() || _ignoreLock)) {
					movingDirection = InputDirection.DOWN;
					_setCursorPosition(_currentPanelType, _currentCursorPosition + 1, true);
				} else if (input == MM_Input.JUMP && !_isInputLocked()) {
					_actionCurrentSelection();
				} else if (input == MM_Input.ABILITY && !_isInputLocked()) {
					openDebugPanel();
				}
				break;
			case PanelType.NEW_GAME:
				currentMenuItem = _newGameMenuItems[_currentCursorPosition];
				if (input == MM_Input.UP && currentMenuItem.upIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_NewGame, input, _currentPanelType, currentMenuItem.upIndex, true);
				} else if (input == MM_Input.DOWN && currentMenuItem.downIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_NewGame, input, _currentPanelType, currentMenuItem.downIndex, true);
				} else if (input == MM_Input.LEFT && currentMenuItem.leftIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_NewGame, input, _currentPanelType, currentMenuItem.leftIndex, true);
				} else if (input == MM_Input.RIGHT && currentMenuItem.rightIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_NewGame, input, _currentPanelType, currentMenuItem.rightIndex, true);
				} else if (input == MM_Input.MENU && !_isInputLocked()) {
					_actionCurrentSelection();
				} else if (input == MM_Input.INVENTORY && !_isInputLocked()) {
					_cancelCurrentSelection();
				}
				break;
			case PanelType.CONTINUE:
				if (input == MM_Input.UP && _currentCursorPosition > 0 && (!_isInputLocked() || _ignoreLock)) {
					movingDirection = InputDirection.UP;
					_setCursorPosition(_currentPanelType, _currentCursorPosition - 1, true);
				} else if (input == MM_Input.DOWN && _currentCursorPosition < (_continueSavedGamesCount - 1) && (!_isInputLocked() || _ignoreLock)) {
					movingDirection = InputDirection.DOWN;
					_setCursorPosition(_currentPanelType, _currentCursorPosition + 1, true);
				} else if (input == MM_Input.MENU && !_isInputLocked()) {
					_actionCurrentSelection();
				} else if (input == MM_Input.INVENTORY && !_isInputLocked()) {
					_cancelCurrentSelection();
				}
				break;
			case PanelType.OPTIONS:
				currentMenuItem = _optionsMenuItems[_currentCursorPosition];
				if (input == MM_Input.UP && currentMenuItem.upIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, currentMenuItem.upIndex, true);
				} else if (input == MM_Input.DOWN && currentMenuItem.downIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, currentMenuItem.downIndex, true);
				} else if (input == MM_Input.LEFT && currentMenuItem.leftIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, currentMenuItem.leftIndex, true);
				} else if (input == MM_Input.RIGHT && currentMenuItem.rightIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_checkCursorPosition_Options, _currentPanelType, currentMenuItem.rightIndex, true);
				} else if (input == MM_Input.JUMP && !_isInputLocked()) {
					_actionCurrentSelection();
				} else if (input == MM_Input.INVENTORY && !_isInputLocked()) {
					_cancelCurrentSelection();
				}
				break;
			case PanelType.DEBUG:
				currentMenuItem = _debugMenuItems[_currentCursorPosition];
				if (input == MM_Input.UP && currentMenuItem.upIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_currentPanelType, currentMenuItem.upIndex, true);
				} else if (input == MM_Input.DOWN && currentMenuItem.downIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_currentPanelType, currentMenuItem.downIndex, true);
				} else if (input == MM_Input.LEFT && currentMenuItem.leftIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_currentPanelType, currentMenuItem.leftIndex, true);
				} else if (input == MM_Input.RIGHT && currentMenuItem.rightIndex > _MENU_INVALID && !_isInputLocked()) {
					_setCursorPosition(_currentPanelType, currentMenuItem.rightIndex, true);
				} else if (input == MM_Input.MENU && !_isInputLocked()) {
					_actionCurrentSelection();
				} else if (input == MM_Input.INVENTORY && !_isInputLocked()) {
					_cancelCurrentSelection();
				}
				break;
		}
		// Start a coroutine to manage continuous cursor movement if directional buttons are still held when cursor tweening is complete
		if (movingDirection != InputDirection.NONE && oldCursorPosition != _currentCursorPosition) {
			if (_cursorMovementCoroutineRunning) {
				StopCoroutine(_cursorMovementCoroutine);
			}
			_cursorMovementCoroutine = _checkDirectionButtonHeld(movingDirection);
			StartCoroutine(_cursorMovementCoroutine);
			_cursorMovementCoroutineRunning = true;
		}
		_ignoreLock = false;
	}

	private IEnumerator _checkDirectionButtonHeld(InputDirection movingDirection) {
		yield return new WaitForSecondsRealtime(MM_Constants.CURSOR_DIRECTION_HELD_CHECK);
		_cursorMovementCoroutineRunning = false;
		if (movingDirection == InputDirection.UP && MM.inputReader.IsButtonHeld(MM_Input.UP)) {
			_ignoreLock = true;
			_inputReceived(MM_Input.UP);
		} else if (movingDirection == InputDirection.DOWN && MM.inputReader.IsButtonHeld(MM_Input.DOWN)) {
			_ignoreLock = true;
			_inputReceived(MM_Input.DOWN);
		}
	}

	public void NewGame() {
		if (!MM.IsMenuInputLocked) {
			// Lock input
			MM.SetMenuInputLocked(true);
			// Stop playing music
			MM.music.Pause(true);
			MM.soundEffects.Play(MM_SFX.MENU_CREDIT);

			MM.player = null;
			MM.gameManager.CreateNewGame(_currentHeroSelection, _currentGameDifficulty);
			if (MM.player != null) {
				Debug.Log("MM_MainMenuController:NewGame : new game created, moving to " + MM_Constants.SCENE_LOADING);
				_fadeIntoLoadingScreen();
			} else {
				Debug.LogError("MM_MainMenuController:NewGame : MM.player property is null");
			}
		}
	}

	public void LoadGame() {
		if (!MM.IsMenuInputLocked) {
			// Lock input
			MM.SetMenuInputLocked(true);
			// Stop playing music
			MM.music.Pause(true);
			MM.soundEffects.Play(MM_SFX.MENU_CREDIT);

			_selectedSavedGameSlot = _continueSavedGamesKeys[_currentCursorPosition];
			MM.player = null;
			MM.gameManager.LoadExistingGame(_selectedSavedGameSlot);
		}
	}

	private void _loadExistingGame() {
		if (MM.player != null) {
			Debug.Log("MM_MainMenuController:_loadExistingGame : game in saveslot [" + _selectedSavedGameSlot + "] loaded, moving to " + MM_Constants.SCENE_LOADING);
			_fadeIntoLoadingScreen();
		} else {
			Debug.LogError("MM_MainMenuController:_loadExistingGame : MM.player property is null");
		}
	}

	public void nightGlow_show() {
		if (!_skipNextGlowAnimation) {
			_glow.Play("GlowOn");
		} else {
			_skipNextGlowAnimation = false;
		}
	}

	public void nightGlow_hide() {
		if (!_skipNextGlowAnimation) {
			_glow.Play("GlowOff");
		} else {
			_skipNextGlowAnimation = false;
		}
	}

	public void openNewGamePanel() {
		if (!MM.IsMenuInputLocked) {
			// Turn off weather sounds
			_playAmbientSFX(false);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			// Disable the other panels
			OptionsPanel.SetActive(false);
			ContinuePanel.SetActive(false);
			DebugPanel.SetActive(false);
			// Play the animation that removes the parent panel
			_anim.Play("buttonTweenAnims_on_newGame");
			// Enable the blur on the main camera
			_blur.Play("BlurOn");
		}
	}

	public void openNewGamePanel_complete() {
		// Set the cursor position
		_setCursorPosition(PanelType.NEW_GAME, _NEW_GAME_DEFAULT_SELECTION, false);
		// Play menu music
		MM.music.PlayFromPosition(MusicalTrack.COMMON_MENU);
		// Populate the new game panel with the current preferences
		_populateNewGamePanel_Hero();
		_populateNewGamePanel_Difficulty();
		// Enable this panel
		NewGamePanel.SetActive(true);
		if (_currentHeroSelection == MM_Constants.PLAYER_BOCK) {
			_playerSelectionRight.GetComponent<Animator>().Play("FadeIn");
		} else if (_currentHeroSelection == MM_Constants.PLAYER_TANYA) {
			_playerSelectionLeft.GetComponent<Animator>().Play("FadeIn");
		} else {
			_playerSelectionLeft.GetComponent<Animator>().Play("FadeIn");
			_playerSelectionRight.GetComponent<Animator>().Play("FadeIn");
		}
		_currentPanelType = PanelType.NEW_GAME;
		_updateActionsList();
		MM.SetMenuInputLocked(false);
	}

	public void closeNewGamePanel() {
		if (!MM.IsMenuInputLocked) {
			// Stop playing music
			MM.music.Pause(true);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			_updateActionsList();
			// Remove the blur of the main camera
			_blur.Play("BlurOff");
			// Disable this panel
			NewGamePanel.SetActive(false);
			// Set the cursor position on the parent panel
			_setCursorPosition(PanelType.MAIN, (_isMainPanelContinueAvailable ? _MAIN_MENU_DEFAULT_SELECTION - 1 : _MAIN_MENU_DEFAULT_SELECTION), false);
			// Play the animation that restores the parent panel
			_anim.Play("buttonTweenAnims_off_newGame");
		}
	}

	public void closeNewGamePanel_complete() {
		// Allow the main panel to take input again
		_currentPanelType = PanelType.MAIN;
		MM.SetMenuInputLocked(false);
		// Turn on weather sounds
		_playAmbientSFX(true);
	}

	public void openContinuePanel() {
		if (!MM.IsMenuInputLocked) {
			// Turn off weather sounds
			_playAmbientSFX(false);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			// Disable the other panels
			NewGamePanel.SetActive(false);
			OptionsPanel.SetActive(false);
			DebugPanel.SetActive(false);
			// Play the animation that removes the parent panel
			_anim.Play("buttonTweenAnims_on_continue");
			// Enable the blur on the main camera
			_blur.Play("BlurOn");
		}
	}

	public void openContinuePanel_complete() {
		// Populate the continue panel with the saved game list
		_populateContinuePanel();
		// Set the cursor position
		_continueSavedGamesContent.anchoredPosition = new Vector3(_continueSavedGamesContent.anchoredPosition.x, 0f);
		_setCursorPosition(PanelType.CONTINUE, _CONTINUE_DEFAULT_SELECTION, false);
		// Play menu music
		MM.music.PlayFromPosition(MusicalTrack.COMMON_MENU);
		// Enable this panel
		ContinuePanel.SetActive(true);
		_currentPanelType = PanelType.CONTINUE;
		_updateActionsList();
		MM.SetMenuInputLocked(false);
	}

	public void closeContinuePanel() {
		if (!MM.IsMenuInputLocked) {
			// Stop playing music
			MM.music.Pause(true);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			_updateActionsList();
			// Remove the blur of the main camera
			_blur.Play("BlurOff");
			// Disable this panel
			ContinuePanel.SetActive(false);
			// Set the cursor position on the parent panel
			_setCursorPosition(PanelType.MAIN, _MAIN_MENU_DEFAULT_SELECTION, false);
			// Play the animation that restores the parent panel
			_anim.Play("buttonTweenAnims_off_continue");
		}
	}

	public void closeContinuePanel_complete() {
		// Allow the main panel to take input again
		_currentPanelType = PanelType.MAIN;
		MM.SetMenuInputLocked(false);
		// Turn on weather sounds
		_playAmbientSFX(true);
	}

	public void openOptionsPanel() {
		if (!MM.IsMenuInputLocked) {
			// Turn off weather sounds
			_playAmbientSFX(false);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			// Disable the other panels
			NewGamePanel.SetActive(false);
			ContinuePanel.SetActive(false);
			DebugPanel.SetActive(false);
			// Play the animation that removes the parent panel
			_anim.Play("buttonTweenAnims_on_options");
			// Enable the blur on the main camera
			_blur.Play("BlurOn");
		}
	}

	public void openOptionsPanel_complete() {
		// Populate the options panel with the current preferences
		_populateOptionsPanel();
		// Set the cursor position
		_setCursorPosition(PanelType.OPTIONS, _OPTIONS_DEFAULT_SELECTION, false);
		// Play menu music
		MM.music.PlayFromPosition(MusicalTrack.COMMON_MENU);
		// Enable this panel
		OptionsPanel.SetActive(true);
		_currentPanelType = PanelType.OPTIONS;
		_updateActionsList(_currentCursorPosition);
		MM.SetMenuInputLocked(false);
	}

	public void closeOptionsPanel() {
		if (!MM.IsMenuInputLocked) {
			// Stop playing music
			MM.music.Pause(true);
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			_updateActionsList();
			// Remove the blur of the main camera
			_blur.Play("BlurOff");
			// Disable this panel
			OptionsPanel.SetActive(false);
			// Set the cursor position on the parent panel
			_setCursorPosition(PanelType.MAIN, _MAIN_MENU_DEFAULT_SELECTION + 1, false);
			// Play the animation that restores the parent panel
			_anim.Play("buttonTweenAnims_off_options");
		}
	}

	public void closeOptionsPanel_complete() {
		// Allow the main panel to take input again
		_currentPanelType = PanelType.MAIN;
		MM.SetMenuInputLocked(false);
		// Turn on weather sounds
		_playAmbientSFX(true);
	}

	public void openDebugPanel() {
		if (!MM.IsMenuInputLocked) {
			// Lock input
			MM.SetMenuInputLocked(true);
			MM.soundEffects.Play(MM_SFX.MENU_SELECT);

			_currentPanelType = PanelType.NONE;
			// Disable the other panels
			NewGamePanel.SetActive(false);
			ContinuePanel.SetActive(false);
			OptionsPanel.SetActive(false);
			// Play the animation that removes the parent panel
			_anim.Play("buttonTweenAnims_on_debug");
			// Enable the blur on the main camera
			_blur.Play("BlurOn");
		}
	}

	public void openDebugPanel_complete() {
		// Set the cursor position
		_setCursorPosition(PanelType.DEBUG, _DEBUG_DEFAULT_SELECTION, false);
		// Enable this panel
		DebugPanel.SetActive(true);
		_currentPanelType = PanelType.DEBUG;
		_updateActionsList();
		MM.SetMenuInputLocked(false);
	}

	public void closeDebugPanel() {
		if (!MM.IsMenuInputLocked) {
			// Lock input
			MM.SetMenuInputLocked(true);
			_currentPanelType = PanelType.NONE;
			_updateActionsList();
			// Remove the blur of the main camera
			_blur.Play("BlurOff");
			// Disable this panel
			DebugPanel.SetActive(false);
			// Set the cursor position on the parent panel
			_setCursorPosition(PanelType.MAIN, _MAIN_MENU_DEFAULT_SELECTION, false);
			// Play the animation that restores the parent panel
			_anim.Play("buttonTweenAnims_off_debug");
		}
	}

	public void closeDebugPanel_complete() {
		// Allow the main panel to take input again
		_currentPanelType = PanelType.MAIN;
		MM.SetMenuInputLocked(false);
	}

	public void Quit() {
		Application.Quit();
	}
}
