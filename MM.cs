using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MM {
	// Components
	public static MM_Events events;
	public static MM_Input_Reader inputReader;
	public static MM_GameManager gameManager;
	public static MM_AutomatorController automatorController;
	public static MM_CameraController cameraController;
	public static MM_LightingController lightingController;
	public static MM_LayerController layerController;
	public static MM_MapController mapController;
	public static MM_SpriteManager spriteManager;
	public static MM_AreaManager areaManager;
	public static MM_LanguageManager lang;
	public static MM_Audio_SFX soundEffects;
	public static MM_Audio_Music music;
	public static MM_Graphics_Effects uiEffects;
	public static MM_Objects_Spawner spawner;
	public static MM_HUD hud;
	public static MM_GameMenu gameMenu;
	public static MM_PlayerController playerController;
	public static MM_Prefabs prefabs;

	private static Camera _mainCamera;
	private static Camera _uiCamera;
	private static Camera _storyCamera;
	private static Stack<InputFocus> _inputFocusStack = new Stack<InputFocus>();
	private static bool _menuInputLocked = true;
	private static bool _gameInputLocked = true;
	private static List<InputLockType> _gameInputLockType = new List<InputLockType>();
	private static bool _gameInputLockChanged = false;
	private static bool _playerControlLocked = false;
	private static List<PlayerControlLockType> _playerControlLockType = new List<PlayerControlLockType>();
	private static List<PlayerInvulnerabilityType> _playerInvulnerabilityType = new List<PlayerInvulnerabilityType>();
	private static bool _playerControlLockChanged = false;
	private static bool _engineReady = false;
	private static Character _player = null;
	private static bool _engineAnimationBlocked = false;
	private static MM_SceneController _sceneController = null;
	private static string _streamingAssetsPath = Application.streamingAssetsPath;

	// Current player state
	public static MM_PlayerState player { get; set; }

	private static GameObject MM_SafeFind(string s) {
		GameObject g = GameObject.Find(s);
		if (g == null)
			MM_LogError("The " + s + " game object is not in this scene");
		return g;
	}

	private static Component MM_SafeComponent(GameObject g, string s) {
		Component c = g.GetComponent(s);
		if (c == null)
			MM_LogError("The " + s + " component is not there");
		return c;
	}

	private static void MM_LogError(string error) {
		Debug.LogError(error);
		Debug.Break();
	}

	static MM() {
		GameObject g;
		g = MM_SafeFind("_app");

		// Set references to each component that exists on _app
		events = (MM_Events)MM_SafeComponent(g, "MM_Events");
		if (events == null) {
			Debug.LogError("MM: no MM_Events component found");
		}
		inputReader = (MM_Input_Reader)MM_SafeComponent(g, "MM_Input_Reader");
		if (inputReader == null) {
			Debug.LogError("MM: no MM_Input_Reader component found");
		}
		gameManager = (MM_GameManager)MM_SafeComponent(g, "MM_GameManager");
		if (gameManager == null) {
			Debug.LogError("MM: no MM_GameManager component found");
		}
		automatorController = (MM_AutomatorController)MM_SafeComponent(g, "MM_AutomatorController");
		if (automatorController == null) {
			Debug.LogError("MM: no MM_AutomatorController component found");
		}
		cameraController = (MM_CameraController)MM_SafeComponent(g, "MM_CameraController");
		if (cameraController == null) {
			Debug.LogError("MM: no MM_CameraController component found");
		}
		lightingController = (MM_LightingController)MM_SafeComponent(g, "MM_LightingController");
		if (lightingController == null) {
			Debug.LogError("MM: no MM_LightingController component found");
		}
		layerController = (MM_LayerController)MM_SafeComponent(g, "MM_LayerController");
		if (layerController == null) {
			Debug.LogError("MM: no MM_LayerController component found");
		}
		mapController = (MM_MapController)MM_SafeComponent(g, "MM_MapController");
		if (mapController == null) {
			Debug.LogError("MM: no MM_MapController component found");
		}
		spriteManager = (MM_SpriteManager)MM_SafeComponent(g, "MM_SpriteManager");
		if (spriteManager == null) {
			Debug.LogError("MM: no MM_SpriteManager component found");
		}
		areaManager = (MM_AreaManager)MM_SafeComponent(g, "MM_AreaManager");
		if (areaManager == null) {
			Debug.LogError("MM: no MM_AreaManager component found");
		}
		lang = (MM_LanguageManager)MM_SafeComponent(g, "MM_LanguageManager");
		if (lang == null) {
			Debug.LogError("MM: no MM_LanguageManager component found");
		}
		soundEffects = (MM_Audio_SFX)MM_SafeComponent(g, "MM_Audio_SFX");
		if (soundEffects == null) {
			Debug.LogError("MM: no MM_Audio_SFX component found");
		}
		music = (MM_Audio_Music)MM_SafeComponent(g, "MM_Audio_Music");
		if (music == null) {
			Debug.LogError("MM: no MM_Audio_Music component found");
		}
		uiEffects = (MM_Graphics_Effects)MM_SafeComponent(g, "MM_Graphics_Effects");
		if (uiEffects == null) {
			Debug.LogError("MM: no MM_Graphics_Effects component found");
		}
		spawner = (MM_Objects_Spawner)MM_SafeComponent(g, "MM_Objects_Spawner");
		if (spawner == null) {
			Debug.LogError("MM: no MM_Objects_Spawner component found");
		}
		hud = (MM_HUD)MM_SafeComponent(g, "MM_HUD");
		if (hud == null) {
			Debug.LogError("MM: no MM_HUD component found");
		}
		gameMenu = (MM_GameMenu)MM_SafeComponent(g, "MM_GameMenu");
		if (gameMenu == null) {
			Debug.LogError("MM: no MM_GameMenu component found");
		}
		playerController = (MM_PlayerController)MM_SafeComponent(g, "MM_PlayerController");
		if (playerController == null) {
			Debug.LogError("MM: no MM_PlayerController component found");
		}
		prefabs = (MM_Prefabs)MM_SafeComponent(g, "MM_Prefabs");
		if (prefabs == null) {
			Debug.LogError("MM: no MM_Prefabs component found");
		}

		// Set a reference to the UI camera
		_uiCamera = (Camera)MM_SafeComponent(MM_SafeFind("_app/_uiCamera"), "Camera");
		if (_uiCamera == null) {
			Debug.LogError("MM: no UI Camera component found");
		}

		MM.SetGameInputLocked(true, InputLockType.GAME_INACTIVE);
	}

	public static string StreamingAssetsPath {
		get {
			return _streamingAssetsPath;
		}
	}

	public static Camera MainCamera {
		get {
			if (_mainCamera == null) {
				_mainCamera = Camera.main;
			}
			return _mainCamera;
		}
		set {
			_mainCamera = value;
		}
	}

	public static Camera UICamera {
		get {
			return _uiCamera;
		}
		set {
			_uiCamera = value;
		}
	}

	public static Camera StoryCamera {
		get {
			return _storyCamera;
		}
		set {
			_storyCamera = value;
		}
	}

	public static InputFocus CurrentInputFocus {
		get {
			return _inputFocusStack.Count > 0 ? _inputFocusStack.Peek() : InputFocus.NONE;
		}
	}

	public static string DebugGetGameInputLockedString() {
		return _gameInputLockType.Count > 0 ? string.Join(",", _gameInputLockType.Select(s => s.ToString()).ToArray()) : "";
	}

	public static bool IsGameInputLocked {
		get {
			if (_gameInputLockChanged) {
				_gameInputLocked = _gameInputLockType.Count > 0;
				_gameInputLockChanged = false;
			}
			return _gameInputLocked;
		}
	}

	public static bool IsMenuInputLocked {
		get {
			return _menuInputLocked;
		}
	}

	public static bool IsAllInputLocked {
		get {
			if (_gameInputLockChanged) {
				_gameInputLocked = _gameInputLockType.Count > 0;
				_gameInputLockChanged = false;
			}
			return _gameInputLocked && _menuInputLocked;
		}
	}

	public static bool IsPlayerControlLocked {
		get {
			if (_playerControlLockChanged) {
				_playerControlLocked = _playerControlLockType.Count > 0;
				_playerControlLockChanged = false;
			}
			return _playerControlLocked;
		}
	}

	public static bool IsPlayerInvulnerable {
		get {
			return _playerInvulnerabilityType.Count > 0;
		}
	}

	public static bool EngineReady {
		get {
			return _engineReady;
		}
		set {
			_engineReady = value;
		}
	}

	public static Character Player {
		get {
			return _player;
		}
		set {
			_player = value;
		}
	}

	public static bool EngineAnimationBlocked {
		get {
			return _engineAnimationBlocked;
		}
		set {
			_engineAnimationBlocked = value;
		}
	}

	public static MM_SceneController SceneController {
		get {
			return _sceneController;
		}
		set {
			_sceneController = value;
		}
	}

	public static void AddInputFocus(InputFocus focus) {
		_inputFocusStack.Push(focus);
	}

	public static void RevokeLastInputFocus() {
		_inputFocusStack.Pop();
	}

	public static void SetMenuInputLocked(bool locked) {
		_menuInputLocked = locked;
	}

	public static void SetGameInputLocked(bool locked, InputLockType type) {
		if (locked) {
			_gameInputLockType.Add(type);
		} else if (_gameInputLockType.Contains(type)) {
			_gameInputLockType.Remove(type);
		}
		_gameInputLockChanged = true;
	}

	public static void SetPlayerControlLocked(bool locked, PlayerControlLockType type) {
		if (locked) {
			_playerControlLockType.Add(type);
		} else if (_playerControlLockType.Contains(type)) {
			_playerControlLockType.Remove(type);
		}
		_playerControlLockChanged = true;
	}

	public static void ClearPlayerControlLocked() {
		_playerControlLockType.Clear();
		_playerControlLockChanged = true;
	}

	public static void SetPlayerInvulnerability(bool active, PlayerInvulnerabilityType type) {
		if (active) {
			_playerInvulnerabilityType.Add(type);
		} else if (_playerInvulnerabilityType.Contains(type)) {
			_playerInvulnerabilityType.Remove(type);
		}
		if (_playerInvulnerabilityType.Count == 0) {
			MM.Player?.DamageEnabled();
		} else {
			MM.Player?.DamageDisabled();
		}
	}

	public static void ClearPlayerInvulnerability() {
		_playerInvulnerabilityType.Clear();
		MM.Player?.DamageEnabled();
	}
}
