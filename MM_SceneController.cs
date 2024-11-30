using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Engine.Character;

public enum MM_SceneControllerEnemyPrefabs {
	ENEMY_1 = 1,
	ENEMY_2 = 2,
	ENEMY_3 = 3,
	ENEMY_4 = 4,
	ENEMY_5 = 5
}

public enum MM_SceneControllerPowerupPrefabs {
	SKATEBOARD = 1,
	WINGED_BOOTS = 2
}

public class MM_SceneController : MonoBehaviour {

	public Transform areaContainer;

	public MMSimpleObjectPooler enemy1Pool;
	public MMSimpleObjectPooler enemy2Pool;
	public MMSimpleObjectPooler enemy3Pool;
	public MMSimpleObjectPooler enemy4Pool;
	public MMSimpleObjectPooler enemy5Pool;
	public MMSimpleObjectPooler goldCoinPool_small;
	public MMSimpleObjectPooler heartPool_small;
	public MMSimpleObjectPooler goldWaterJugPool;
	public MMSimpleObjectPooler powerupSkateboardPool;
	public MMSimpleObjectPooler powerupWingedBootsPool;
	public MMSimpleObjectPooler abilityItemFireballPool;

	// Whether the scene is ready for Update methods on gameobjects to be run
	public bool SceneReady = false;
	// The current position on the map in format of: x,y
	public string CurrentMapPosition { get; set; }
	public MM_Objects_MapUpdateCollider CurrentMapUpdateCollider { get; set; }

	private Transform _levelStart;
	private Transform _sceneAreaEntranceObject;
	private Transform _sceneAreaObjectsContainer;
	private Transform _playerManager;
	private GameObject[] _sceneObjects;
	private String _currentLevelStartPath;
	private Vector2 _defaultCharacterOffset = new Vector2(0f, 0f);
	private bool _defaultCharacterOffsetX = false;
	private bool _defaultCharacterOffsetY = false;
	private Vector2 _defaultCharacterSpeed = new Vector2(0f, 0f);
	private bool _spawnNextCharacter;
	private Vector2 _spawnNextCharacterOffset;
	private bool _spawnNextCharacterOffsetX;
	private bool _spawnNextCharacterOffsetY;
	private Vector2 _spawnNextCharacterSpeed;
	private MM_SceneArea _currentSceneArea;
	private Transform _currentSceneAreaTransform;
	private MusicalTrack _currentTrack;
	private MusicalTrack? _backupTrack;
	private MM_SFX _currentEnvironmentSFX;
	private float _backupTrackPlayheadTime;


	private void OnEnable() {
		MM_Events.OnSceneReady += _sceneReady;
		MM_Events.OnFadeToBlackComplete += _sceneTransitionFadeOutComplete;
		MM_Events.OnSceneAreaTransition += _sceneAreaTransition;
		MM_Events.OnLocationTransitionPlayerMove += _locationTransitionPlayerMove;
		MM_Events.OnLayerTransitionPlayerMove += _layerTransitionPlayerMove;
	}

	private void OnDisable() {
		MM_Events.OnSceneReady -= _sceneReady;
		MM_Events.OnFadeToBlackComplete -= _sceneTransitionFadeOutComplete;
		MM_Events.OnSceneAreaTransition -= _sceneAreaTransition;
		MM_Events.OnLocationTransitionPlayerMove -= _locationTransitionPlayerMove;
		MM_Events.OnLayerTransitionPlayerMove -= _layerTransitionPlayerMove;
	}

	private void Awake() {
		Cursor.visible = false;
		// Disable all scene areas and the components they contain
		_disableUnusedSceneAreas(true);
	}

	private void Start() {
		// Setup references to important objects contained in this scene
		SceneReady = false;
		MM.SceneController = this;
		MM.cameraController.SetSceneCameras();
		_spawnNextCharacter = true;
		_spawnNextCharacterOffset = _defaultCharacterOffset;
		_spawnNextCharacterOffsetX = _defaultCharacterOffsetX;
		_spawnNextCharacterOffsetY = _defaultCharacterOffsetY;
		_spawnNextCharacterSpeed = _defaultCharacterSpeed;

		// Force the references to the player manager in the _GameManagerment gameobject to be set
		PlayerManager(true);

		// Inform listeners that a new scene has been loaded
		MM.events.Trigger(MM_Event.SCENE_LOADED);

		// Remove game input lock for the gane inactive lock type now that a scene containing a game area has been loaded 
		MM.SetGameInputLocked(false, InputLockType.GAME_INACTIVE);

		// Determine the asset bundle that is being used for this scene/area
		_currentSceneArea = MM_Locations.SceneAreaData[MM.player.CurrentSceneArea];
		AreaAssetBundle areaBundle = _currentSceneArea.bundle;
		if (!MM.areaManager.IsAreaInitialised || MM.areaManager.CurrentAreaInitialised != areaBundle) {
			Debug.LogError("MM_SceneController:Start : area bundle has not been initialised");
		}

		// Setup an event group listener to trigger a SCENE_READY event when everything is ready
		List<MM_Event> setupList = new List<MM_Event> { };
		if (MM.EngineReady == false) {
			setupList.Add(MM_Event.ENGINE_READY);
		}
		if (!MM.areaManager.IsAreaInitialised || MM.areaManager.CurrentAreaInitialised != areaBundle) {
			setupList.Add(MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE);
		}
		if (setupList.Count > 0) {
			MM.events.AddEventGroupListener(setupList, MM_Event.SCENE_READY);
		}

		// Set the reference to the current scene area and the transform containing it, and activate it in order to enable the level manager
		_setCurrentSceneAreaReference();
		CurrentMapUpdateCollider = null;
		_currentSceneAreaTransform?.gameObject.SetActive(true);
		_resetScenePools();

		// If everything is ready then trigger the SCENE_READY event now
		if (setupList.Count == 0) {
			MM.events.Trigger(MM_Event.SCENE_READY, null, false);
		}

		// Perform all other tasks that need to be performed before fading the scene in
		// Each call is responsible for triggering the appropriate event as listed above
		MM.areaManager.LoadAreaBundle(areaBundle, (!MM.areaManager.IsAreaInitialised || MM.areaManager.CurrentAreaInitialised != areaBundle));
	}

	private void _sceneReady() {
		// Disable the camera that blurs the background when the menu is active
		MM_Graphics_Utilities.ShowPausedBackground(false, true);
		// Turn off cameras and rendered layers not used in this scene area
		MM.cameraController.SetSceneAreaCamerasUsed(_currentSceneArea.layers[0], _currentSceneArea.layers[1], _currentSceneArea.layers[2]);
		MM.layerController.SetSceneAreaLayersUsed(_currentSceneArea.layers[0], _currentSceneArea.layers[1], _currentSceneArea.layers[2]);

		// Reset the HUD (end animations, etc)
		MM.hud.ResetState();
		_refreshHUD();
		_resetGameLockStates();

		// Revive the player if dead
		if (MM.Player.IsPlayerDead) {
			MM.Player.Revive();
		}

		// Lock input / freeze the player while the scene loads. Must be done here.
		MM.SetGameInputLocked(true, InputLockType.SCENE_INITIALISING);
		MM.SetPlayerControlLocked(true, PlayerControlLockType.PLAYER_INITIALISING);
		MM.playerController.FreezePlayer_game();
		MM.playerController.ControlPlayer_game();
		MM.playerController.LockAnimation_game();
		MM.playerController.AI.Initialise();

		// Set the player position for this scene/area/entrance
		_sceneAreaEntranceObject = null;
		if (MM.player.CurrentSceneAreaEntranceObjectName.Length > 0) {
			// Locate the entrance by gameobject name
			_findSceneAreaEntranceByName();
		}
		if (MM.player.CurrentSceneAreaEntrance < 0 || MM.player.CurrentSceneAreaEntrance >= _currentSceneArea.entrances.Count) {
			Debug.LogError("MM_SceneController:_sceneReady : area entrance [" + MM.player.CurrentSceneAreaEntrance + "] cannot be found");
			return;
		}
		MM_SceneAreaEntrance entrance = _currentSceneArea.entrances[MM.player.CurrentSceneAreaEntrance];
		MM.player.SetCurrentSceneAreaLayer(entrance.layer);

		// Set the Cinemachine scene camera bounds to the current area bounds
		Transform cameraBounds = _currentSceneAreaTransform.Find(MM_Constants.SCENE_CAMERA_BOUNDS);
		if (cameraBounds != null) {
			MM.cameraController.SetCinemachineConfinerBounds(_currentSceneAreaTransform.Find(MM_Constants.SCENE_CAMERA_BOUNDS));
		} else {
			Debug.LogError("MM_SceneController:_sceneReady : camera bounds for scene area [" + _currentSceneArea.area + "] cannot be found");
			return;
		}

		// Apply settings for the current scene area (e.g. lighting, volume)
		MM.lightingController.SetSceneAreaLightsource(_currentSceneAreaTransform.Find(MM_Constants.SCENE_LIGHTSOURCE));
		MM.lightingController.ApplySceneAreaLighting();
		MM.gameManager.SetVolumeModifier(_currentSceneArea.volumeModifier);

		// Set the spawn location to the position of the entrance
		_updateLevelStartReference();
		// If necessary set the entrance position to the same coordinates of an entrance gameobject (e.g. save point)
		if (_sceneAreaEntranceObject) {
			_levelStart.localPosition = new Vector3(_sceneAreaEntranceObject.localPosition.x, _sceneAreaEntranceObject.localPosition.y);
		} else {
			_levelStart.localPosition = new Vector3(entrance.x, entrance.y);
		}
		// Spawn the player at the appropriate entrance
		if (_spawnNextCharacter) {
			MM.Player.RespawnAt(_levelStart, (FacingDirections)(entrance.dir != null ? entrance.dir : MM.player.CurrentSceneFacingDirection));
		} else {
			MM.Player.Move(_levelStart, _spawnNextCharacterOffset, _spawnNextCharacterOffsetX, _spawnNextCharacterOffsetY);
			MM.Player._controller.SetHorizontalForce(_spawnNextCharacterSpeed.x);
			MM.Player._controller.SetVerticalForce(_spawnNextCharacterSpeed.y);
		}
		MM.Player.UpdateHealth();
		// Correct the player state for certain character abilities
		if (MM.Player.MovementState.CurrentState == CharacterStates.MovementStates.LadderClimbing) {
			MM.Player.gameObject.MMGetComponentNoAlloc<CharacterLadder>().StartClimbing();
		}
		// If the entrance has a map position then mark the position as explored
		if (entrance.mapPosition != "") {
			MM.player.AddExploredMapCoordinate(entrance.mapPosition);
			CurrentMapPosition = entrance.mapPosition;
		}

		// Reinitialise any objects managed by the proximity manager
		_currentSceneAreaTransform?.GetComponent<ProximityManager>()?.GrabControlledObjects(MM.Player.transform.position);

		// Update player viewport masking so the layer the player is currently on is displayed
		MM.layerController.UpdateLayerViewports();
		_updatePlayerLayerOrder(MM.player.CurrentSceneAreaLayer);

		// Reset spawn parameters to defaults
		_spawnNextCharacter = true;
		_spawnNextCharacterOffset = _defaultCharacterOffset;
		_spawnNextCharacterOffsetX = _defaultCharacterOffsetX;
		_spawnNextCharacterOffsetY = _defaultCharacterOffsetY;
		_spawnNextCharacterSpeed = _defaultCharacterSpeed;
		_backupTrack = null;

		// Set the camera distance for this entrance and lock the camera onto the player
		MM.cameraController.SetCameraDistanceImmediately(entrance.cameraDistance);
		MM.cameraController.LockCamera();

		SceneReady = true;

		// If there is an automation pending and the trigger is ON_SCENE_READY then start the automation
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator) && MM_Constants_Automators.Automators[MM.player.CurrentAutomator].trigger == AutomatorTrigger.ON_SCENE_READY) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
		} else {
			// Perform the remaining tasks needed to prepare the scene when automation is not needed
			_resetSceneObjects(true);
			StartMusicFromPosition(_currentSceneArea.musicalTrack, _currentSceneArea.environmentSFX, entrance.musicalTrackStart);
			ShowAreaNotification(entrance);
			if (!_triggerLocationTransitionContinue()) {
				// Allow player input
				MM.events.Trigger(MM_Event.PLAYER_READY);
			}
			// Remove the overlay fader
			MM.hud.TriggerFadeOut(0f, MM_Constants.SCENE_TRANSITION_DURATION);
		}
	}

	public void ResetSceneObjects(bool activate) {
		_resetSceneObjects(activate);
	}

	public void StartMusicFromPositionAfterDelay(MusicalTrack? track, MM_SFX? environmentSFX, float delay, float playheadTime = 0f) {
		StartCoroutine(_startMusic(track, environmentSFX, delay, playheadTime));
	}

	public void StartMusicFromPosition(MusicalTrack? musicalTrack, MM_SFX? environmentSFX, float playheadTime = 0f, bool overrideCurrentTrack = false, bool overrideCurrentEnvironment = false) {
		// Start the music for the new area. If there is no music playing then start the track for this area.
		if (musicalTrack != null && (!MM.music.IsPlaying() || overrideCurrentTrack)) {
			_currentTrack = (MusicalTrack)musicalTrack;
			MM.music.Preload(_currentTrack);
			MM.music.PlayFromPositionAfterDelay(_currentTrack, 0f, playheadTime);
		}
		if (environmentSFX == null && _currentSceneArea.environmentSFX != null) {
			environmentSFX = _currentSceneArea.environmentSFX;
		}
		if (environmentSFX != null && (!MM.soundEffects.IsEnvironmentPlaying() || overrideCurrentEnvironment)) {
			_currentEnvironmentSFX = (MM_SFX)environmentSFX;
			MM.soundEffects.PlayEnvironment(_currentEnvironmentSFX);
		}
	}

	public void SetMusicRestorationPoint() {
		_backupTrack = _currentTrack;
		_backupTrackPlayheadTime = MM.music.GetPlayheadTime();
	}

	public void RestartMusicFromRestorationPoint() {
		if (_backupTrack != null) {
			StartMusicFromPosition(_backupTrack, null, _backupTrackPlayheadTime);
			_backupTrack = null;
		}
	}

	public void StopSceneAreaMusicSFX(bool forceStop = false) {
		// Either maintain the existing musical track, or fade out and stop music playback
		if (_currentSceneArea.musicalTrack == null || _currentSceneArea.musicalTrack != _currentTrack || forceStop) {
			MM.music.Stop();
		}
		if (_currentSceneArea.environmentSFX == null || _currentSceneArea.environmentSFX != _currentEnvironmentSFX || forceStop) {
			MM.soundEffects.StopEnvironment();
		}
	}

	public void AddSceneOverlay(string scene) {
		SceneManager.LoadScene(scene, LoadSceneMode.Additive);
	}

	public void RemoveSceneOverlay(string scene, Action onUnloadComplete) {
		StartCoroutine(_removeSceneOverlay(scene, onUnloadComplete));
	}

	public Transform PlayerManager(bool forceSet = false) {
		if (_playerManager == null || forceSet) {
			_playerManager = transform.Find(MM_Constants.GAMEOBJECT_PLAYER_MANAGER);
		}
		return _playerManager;
	}

	public PlayerCharacterSwitchManager PlayerSwitchManager() {
		if (_playerManager == null) {
			_playerManager = transform.Find(MM_Constants.GAMEOBJECT_PLAYER_MANAGER);
		}
		return _playerManager.GetComponent<PlayerCharacterSwitchManager>();
	}

	public Transform FindSceneObject(string name) {
		return _findSceneObjectByName(name);
	}

	public void SetSceneObjectProperty(string name, PersistentObject persistentObject, PersistentObjectProperty property, int value) {
		_setSceneObjectProperty(_findSceneObjectByName(name), persistentObject, property, value);
	}

	public void SetSceneObjectProperties(string name, PersistentObject persistentObject, Dictionary<PersistentObjectProperty, int> properties) {
		_setSceneObjectProperties(_findSceneObjectByName(name), persistentObject, properties);
	}

	public void ShowAreaNotification(MM_SceneAreaEntrance entrance) {
		// Display the area notification if required, otherwise display the regular HUD
		if (entrance.areaLabel != null) {
			MM.hud.ShowAreaLabelNotification(entrance.areaLabel);
		} else {
			MM.hud.Show(HUDFadeType.IMMEDIATE);
		}
	}

	public bool TriggerLocationTransitionContinue() {
		return _triggerLocationTransitionContinue();
	}

	public Vector2 FindNearestEnemyPosition(Vector2 position) {
		return _findNearestEnemyPosition(position);
	}

	public GameObject GetEnemyFromPool(MM_SceneControllerEnemyPrefabs index) {
		GameObject enemy;
		switch (index) {
			case MM_SceneControllerEnemyPrefabs.ENEMY_5:
				enemy = enemy5Pool.GetPooledGameObject();
				break;
			case MM_SceneControllerEnemyPrefabs.ENEMY_4:
				enemy = enemy4Pool.GetPooledGameObject();
				break;
			case MM_SceneControllerEnemyPrefabs.ENEMY_3:
				enemy = enemy3Pool.GetPooledGameObject();
				break;
			case MM_SceneControllerEnemyPrefabs.ENEMY_2:
				enemy = enemy2Pool.GetPooledGameObject();
				break;
			case MM_SceneControllerEnemyPrefabs.ENEMY_1:
			default:
				enemy = enemy1Pool.GetPooledGameObject();
				break;
		}
		return enemy;
	}

	public GameObject GetGoldFromPool(int amount) {
		GameObject gold = null;
		if (amount <= MM_Constants.GOLD_COIN_SMALL_MAX) {
		} else if (amount <= MM_Constants.GOLD_COIN_LARGE_MAX) {
		} else if (amount <= MM_Constants.GOLD_BAG_SMALL_MAX) {
		} else {
		}
		gold = goldCoinPool_small.GetPooledGameObject();
		return gold;
	}

	public GameObject GetHeartFromPool() {
		return heartPool_small.GetPooledGameObject();
	}

	public GameObject GetPointsFromPool(SpawnObject spawnObject) {
		GameObject points = null;
		switch (spawnObject) {
			case SpawnObject.GOLD_WATER_JUG:
				points = goldWaterJugPool.GetPooledGameObject();
				break;
		}
		return points;
	}

	public GameObject GetPowerupFromPool(MM_SceneControllerPowerupPrefabs index) {
		GameObject powerup;
		switch (index) {
			case MM_SceneControllerPowerupPrefabs.WINGED_BOOTS:
				powerup = powerupWingedBootsPool.GetPooledGameObject();
				break;
			case MM_SceneControllerPowerupPrefabs.SKATEBOARD:
			default:
				powerup = powerupSkateboardPool.GetPooledGameObject();
				break;
		}
		return powerup;
	}

	
	public GameObject GetAbilityItemFromPool(SpawnObject spawnObject) {
		GameObject abilityItem = null;
		switch (spawnObject) {
			case SpawnObject.ABILITY_ITEM_FIREBALL:
				abilityItem = abilityItemFireballPool.GetPooledGameObject();
				break;
		}
		return abilityItem;
	}

	public Transform GetObjectsContainer() {
		return _sceneAreaObjectsContainer;
	}

	public void SetPlayerAsProximityManagerTarget() {
		_currentSceneAreaTransform?.GetComponent<ProximityManager>()?.SetPlayerAsTarget();
	}

	private void _sceneTransitionFadeOutComplete() {
		if (MM.CurrentInputFocus == InputFocus.NONE) {
			// If fading out when input focus is unset then the scene is transitioning back to the main menu
			return;
		}
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator)) {
			MM_Automator automator = MM_Constants_Automators.Automators[MM.player.CurrentAutomator];
			// If the pending automation trigger is ON_SCENE_READY then the fade completion should be ignored
			if (automator.trigger == AutomatorTrigger.ON_SCENE_READY && automator.ignoreChecks) {
				return;
			}
			// If the pending automation trigger is ON_FADE_COMPLETE then trigger the automation
			if (automator.trigger == AutomatorTrigger.ON_FADE_COMPLETE) {
				MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
				return;
			}
		}
		if (MM.CurrentInputFocus != InputFocus.MENU && MM.CurrentInputFocus != InputFocus.NONE) {

			// Deactivate all SceneObjects and proximity managed objects
			_resetSceneObjects(false);
			_currentSceneAreaTransform?.GetComponent<ProximityManager>()?.DisableAll();

			SceneReady = false;
			_resetScenePools();

			_setCurrentSceneAreaReference();
			_disableUnusedSceneAreas(MM.Player.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead);
			CurrentMapUpdateCollider = null;
			_currentSceneAreaTransform?.gameObject.SetActive(true);

			// If required, load a new asset bundle for the scene area to transition to
			// Always check to see if additional common musical tracks need to be loaded for the new scene area
			MM.events.AddEventGroupListener(new List<MM_Event> { MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE }, MM_Event.SCENE_READY);
			MM.areaManager.LoadAreaBundle(_currentSceneArea.bundle, (!MM.areaManager.IsAreaInitialised || MM.areaManager.CurrentAreaInitialised != _currentSceneArea.bundle));
		}
	}

	private IEnumerator _removeSceneOverlay(string scene, Action onUnloadComplete) {
		AsyncOperation ao = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		yield return ao;
		onUnloadComplete?.Invoke();
	}

	private IEnumerator _startMusic(MusicalTrack? track, MM_SFX? environmentSFX, float delay, float playheadTime) {
		yield return new WaitForSecondsRealtime(delay);
		StartMusicFromPosition(track != null ? track : _currentSceneArea.musicalTrack, environmentSFX != null ? environmentSFX : _currentSceneArea.environmentSFX, playheadTime);
	}

	private bool _triggerLocationTransitionContinue() {
		if (_sceneAreaEntranceObject) {
			// Trigger the camera moved event which will continue the location transition from the previous scene area
			MM_Objects_SceneAreaTransition sceneAreaTransition = _sceneAreaEntranceObject.GetComponent<MM_Objects_SceneAreaTransition>();
			MM.events.Trigger(MM_Event.LOCATION_TRANSITION_CONTINUE, new MM_EventData {
				destinationId = sceneAreaTransition.GetId(),
				transition_displayType = sceneAreaTransition.transitionType
			});
		}
		return _sceneAreaEntranceObject != null;
	}

	private void _updateLevelStartReference() {
		String levelStartPath = MM_Constants.GAMEOBJECT_AREA_CONTAINER + "/" + _currentSceneArea.area + "/" + MM_Constants.GAMEOBJECT_LEVEL_START;
		if (_currentLevelStartPath != levelStartPath) {
			_currentLevelStartPath = levelStartPath;
			_levelStart = transform.Find(levelStartPath);
		}
		if (_levelStart == null) {
			throw new Exception("Level start gameobject cannot be found at path [" + levelStartPath + "]");
		}
	}

	private void _findSceneAreaEntranceByName() {
		// Locate a scene area entrance by gameobject name and update the stored coordinates to the transform position
		Transform searchSceneArea = transform.Find(MM_Constants.GAMEOBJECT_AREA_CONTAINER + "/" + _currentSceneArea.area);
		bool found = false;
		int count = 0;

		if (searchSceneArea == null) {
			Debug.LogError("MM_SceneController:_findSceneAreaEntranceByName : cannot find scene area [" + _currentSceneArea.area + "]");
			return;
		}

		MM.player.CurrentSceneAreaEntrance = MM_Constants.SCENE_AREA_INVALID;
		while (count < _currentSceneArea.entrances.Count && !found) {
			if (MM.player.CurrentSceneAreaEntranceObjectName == _currentSceneArea.entrances[count].entranceName) {
				_sceneAreaEntranceObject = searchSceneArea.Find(MM_Constants.GAMEOBJECT_AREA_OBJECTS + "/" + MM.player.CurrentSceneAreaEntranceObjectName);
				if (_sceneAreaEntranceObject != null) {
					found = true;
					MM.player.CurrentSceneAreaEntrance = count;
					Debug.Log("MM_SceneController:_findSceneAreaEntranceByName : found scene area entrance [" + MM.player.CurrentSceneAreaEntrance + "] from object name [" + MM.player.CurrentSceneAreaEntranceObjectName + "]");
				}
			}
			count++;
		}
		if (!found) {
			Debug.LogError("MM_SceneController:_findSceneAreaEntranceByName : cannot find object name [" + MM.player.CurrentSceneAreaEntranceObjectName + "]");
		}
	}

	private Transform _findSceneObjectByName(string name) {
		// Locate an object in the current SceneArea under the "Main/Objects/" path and return the transform
		return transform.Find(MM_Constants.GAMEOBJECT_AREA_CONTAINER + "/" + _currentSceneArea.area + "/Main/Objects/" + name);
	}

	private void _setSceneObjectProperty(Transform sceneObject, PersistentObject persistentObject, PersistentObjectProperty property, int value) {
		// Apply the updated property value to the collection of persistent objects encountered by the player
		MM.player.SetPersistentObjectProperty(persistentObject, property, value);
		// Apply the update directly to the relevant component
		// Only properties modified by automators need to be covered here
		switch (property) {
			case PersistentObjectProperty.TALK_UI_LABEL_ID:
				sceneObject.GetComponent<MM_Objects_Talk>().uiLabelId = (UILabelTalk)value;
				break;
		}
	}

	private void _setSceneObjectProperties(Transform sceneObject, PersistentObject persistentObject, Dictionary<PersistentObjectProperty, int> properties) {
		foreach (KeyValuePair<PersistentObjectProperty, int> property in properties) {
			_setSceneObjectProperty(sceneObject, persistentObject, property.Key, property.Value);
		}
	}

	private void _sceneAreaTransition(MM_EventData data) {
		if (data != null) {
			// Start a transition to a new scene/area
			_spawnNextCharacter = (data.sceneAreaTransition_spawn != null) ? (bool)data.sceneAreaTransition_spawn : false;
			_spawnNextCharacterOffset = (data.sceneAreaTransition_offset != null) ? (Vector2)data.sceneAreaTransition_offset : _defaultCharacterOffset;
			_spawnNextCharacterOffsetX = (data.sceneAreaTransition_offsetX != null) ? (bool)data.sceneAreaTransition_offsetX : _defaultCharacterOffsetX;
			_spawnNextCharacterOffsetY = (data.sceneAreaTransition_offsetY != null) ? (bool)data.sceneAreaTransition_offsetY : _defaultCharacterOffsetY;
			_spawnNextCharacterSpeed = (data.sceneAreaTransition_speed != null) ? (Vector2)data.sceneAreaTransition_speed : _defaultCharacterSpeed;
			if (MM_Locations.SceneAreaData.ContainsKey((SceneArea)data.sceneAreaTransition_destinationArea)) {
				_currentSceneArea = MM_Locations.SceneAreaData[(SceneArea)data.sceneAreaTransition_destinationArea];
				StopSceneAreaMusicSFX();
				// Trigger the fade out to black which triggers the MM_Event.FADE_TO_BLACK_COMPLETE event, resulting in a call to _sceneTransitionFadeOutComplete
				MM.hud.TriggerFadeIn(0f, MM_Constants.SCENE_TRANSITION_DURATION);
			}
		}
	}

	private void _locationTransitionPlayerMove(MM_EventData data) {
		if (data != null && data.transition_position != null) {
			// Move the player to the appropriate position
			_updateLevelStartReference();
			Vector2 position = (Vector2)data.transition_position;
			_levelStart.localPosition = new Vector3(position.x, position.y);
			MM.Player.Move(_levelStart, _defaultCharacterOffset, _defaultCharacterOffsetX, _defaultCharacterOffsetY);

			// Signal that the player has finished moving
			MM.events.Trigger(MM_Event.LOCATION_TRANSITION_PLAYER_MOVED, data);
			Debug.Log("MM_SceneController:_locationTransitionPlayerMove : moved player to position " + _levelStart.localPosition);
		}
	}

	private void _layerTransitionPlayerMove(MM_EventData data) {
		if (data != null && data.sceneAreaLayer > 0) {
			bool movingFurtherFromCamera = (int)MM.player.CurrentSceneAreaLayer < (int)data.sceneAreaLayer;
			if (data.transition_position != null) {
				// If moving closer to the camera then make the character use the raised idle animation
				if (!movingFurtherFromCamera) {
					MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = CharacterStates.MovementStates.IdleForward });
				}
				StartCoroutine(_layerTransitionPlayerMoveCoroutine(data, movingFurtherFromCamera));
			} else {
				SceneAreaLayer moveToLayer = (SceneAreaLayer)data.sceneAreaLayer;
				if (moveToLayer != MM.player.CurrentSceneAreaLayer) {
					if (!movingFurtherFromCamera) {
						data.sceneAreaLayer = MM.player.CurrentSceneAreaLayer;
					}
					MM.events.Trigger(movingFurtherFromCamera ? MM_Event.LAYER_REVEAL : MM_Event.LAYER_CONCEAL, data);
					MM.player.SetCurrentSceneAreaLayer(moveToLayer);
					_updatePlayerLayerOrder(moveToLayer);
				}
			}
		}
	}

	private IEnumerator _layerTransitionPlayerMoveCoroutine(MM_EventData data, bool movingFurtherFromCamera) {

		// Move the player to the appropriate position
		_updateLevelStartReference();
		Vector2 position = (Vector2)data.transition_position;
		_levelStart.localPosition = new Vector3(position.x, position.y);
		MM.cameraController.SetCameraMoving(MM_Constants.CAMERA_LAYER_DAMPING, true);
		MM.Player.Move(_levelStart, _defaultCharacterOffset, _defaultCharacterOffsetX, _defaultCharacterOffsetY);

		SceneAreaLayer layer = (SceneAreaLayer)data.sceneAreaLayer;
		MM.player.SetCurrentSceneAreaLayer(layer);
		if (!movingFurtherFromCamera) {
			// Wait a few frames so the player isn't rendered in the wrong position before the animation starts
			yield return new WaitForSeconds(MM_Constants.PLAYER_ANIM_UNBLOCK_DURATION);
		}
		_updatePlayerLayerOrder(layer);

		// Only trigger a layer reveal if moving to a layer further from the camera
		if (movingFurtherFromCamera) {
			MM.events.Trigger(MM_Event.LAYER_REVEAL, data);
			yield return new WaitForSecondsRealtime(MM_Constants.LAYER_REVEAL_MOVE_DELAY);
		}

		// Signal that the player has finished moving
		MM.events.Trigger(MM_Event.LAYER_TRANSITION_PLAYER_MOVED, data);
		Debug.Log("MM_SceneController:_layerTransitionPlayerMoveCoroutine : moved player to layer [" + MM.player.CurrentSceneAreaLayer + "] position " + _levelStart.localPosition);
	}

	private IEnumerator _delaySceneReload() {
		yield return new WaitForSecondsRealtime(1f);
	}

	private void _refreshHUD() {
		MM.mapController.PrepareAllExploredCoordinatesForRendering();
		MM.hud.SetMaximumHearts(MM.player.GetHeartContainersAmount(true));
		MM.hud.SetItemBarAbility(MM.player.EquippedAbility);
		MM.hud.SetItemBarItem(MM.player.EquippedItem);
		MM.hud.SetItemBarAmounts(
			MM.player.Gold,
			MM.player.Elixirs,
			(MM.player.EquippedAbility != PlayerItem.EQUIPPED_NONE && MM.player.Abilities.ContainsKey(MM.player.EquippedAbility)) ? MM.player.Abilities[MM.player.EquippedAbility] : 0
		);
	}

	private void _updatePlayerLayerOrder(SceneAreaLayer layer) {
		MM.layerController.UpdatePlayerLayerOrder(layer);
	}

	private Transform _getSceneArea(string sceneAreaName) {
		Transform sceneAreaTransform = null;
		for (int i = 0; i < areaContainer.childCount; i++) {
			if (areaContainer.GetChild(i).name == sceneAreaName) {
				sceneAreaTransform = areaContainer.GetChild(i);
				break;
			}
		}
		return sceneAreaTransform;
	}

	private void _disableUnusedSceneAreas(bool disableAll = false) {
		for (int i = 0; i < areaContainer.childCount; i++) {
			GameObject child = areaContainer.GetChild(i).gameObject;
			if (child != null) {
				if (disableAll || child.name != _currentSceneArea.area) {
					child.SetActive(false);
				}
			}
		}
	}

	private void _setCurrentSceneAreaReference() {
		// Locate the current area within the scene
		if (MM_Locations.SceneAreaData.ContainsKey(MM.player.CurrentSceneArea)) {
			_currentSceneArea = MM_Locations.SceneAreaData[MM.player.CurrentSceneArea];
			// Activate the area gameobject that matches the current player location
			_currentSceneAreaTransform = _getSceneArea(_currentSceneArea.area);
			if (_currentSceneAreaTransform != null) {
				_sceneAreaObjectsContainer = _currentSceneAreaTransform.Find(MM_Constants.GAMEOBJECT_AREA_OBJECTS);
			} else {
				Debug.LogError("MM_SceneController:_activateCurrentSceneArea : scene area game object [" + _currentSceneArea.area + "] cannot be found");
				return;
			}
		} else {
			Debug.LogError("MM_SceneController:_activateCurrentSceneArea : scene area [" + MM.player.CurrentSceneArea + "] cannot be found");
			return;
		}
	}

	private void _resetSceneObjects(bool activate) {
		// This will activate any ActivateSceneObject/DeactivateSceneObject functions on objects (e.g. used when resetting a save point)
		if (_sceneObjects == null) {
			_sceneObjects = GameObject.FindGameObjectsWithTag(MM_Constants.SCENE_OBJECT);
		}
		foreach (GameObject obj in _sceneObjects) {
			MM_SceneObject sceneObject = obj.GetComponent<MM_SceneObject>();
			if (sceneObject != null) {
				if (activate) {
					sceneObject.ActivateSceneObject();
				} else {
					sceneObject.DeactivateSceneObject();
				}
			}
		}
		if (!activate) {
			// If deactivating scene objects then empty the collection so it is
			// reinitialised to match the current SceneArea on the next call
			_sceneObjects = null;
		}
	}

	private void _resetGameLockStates() {
		// Reset certain game lock types back to false
		MM.SetGameInputLocked(false, InputLockType.OBJECT_INTERACTION_SEQUENCE);
	}

	private void _resetScenePools() {
		enemy1Pool.DisablePoolObjects();
		enemy2Pool.DisablePoolObjects();
		enemy3Pool.DisablePoolObjects();
		enemy4Pool.DisablePoolObjects();
		enemy5Pool.DisablePoolObjects();
		goldCoinPool_small.DisablePoolObjects();
		heartPool_small.DisablePoolObjects();
		goldWaterJugPool.DisablePoolObjects();
		powerupSkateboardPool.DisablePoolObjects();
		powerupWingedBootsPool.DisablePoolObjects();
	}

	private Vector2 _findNearestEnemyPosition(Vector2 position) {
		List<Vector2> allEnemyPositions = new List<Vector2>();
		allEnemyPositions.AddRange(enemy1Pool.GetPoolObjectPositions());
		allEnemyPositions.AddRange(enemy2Pool.GetPoolObjectPositions());
		allEnemyPositions.AddRange(enemy3Pool.GetPoolObjectPositions());
		allEnemyPositions.AddRange(enemy4Pool.GetPoolObjectPositions());
		allEnemyPositions.AddRange(enemy5Pool.GetPoolObjectPositions());

		float distance;
		float nearestDistance = 0;
		Vector2 nearestPosition = new Vector2(0, 0);
		for (int i = 0; i < allEnemyPositions.Count; i++) {
			distance = (allEnemyPositions[i] - position).magnitude;
			if ((nearestDistance == 0 || distance < nearestDistance) && distance < MM_Constants.ABILITY_FIREBALL_MAXIMUM_DISTANCE) {
				nearestDistance = distance;
				nearestPosition = allEnemyPositions[i];
			}
		}
		return nearestPosition;
	}
}
