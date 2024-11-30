using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum MM_Event {
	FADE_TO_BLACK_COMPLETE = 1,
	SCENE_LOADED = 2,
	SCENE_READY = 3,
	ENGINE_READY = 4,
	PLAYER_READY = 5,
	OBJECT_RESET = 6,
	FORCE_PLAYER_MOVEMENT_STATE = 11,
	STORED_GAME_LOADED = 12,
	VOLUME_SFX_CHANGE = 13,
	VOLUME_MUSIC_CHANGE = 14,
	INPUT_UP_PRESSED = 21,
	INPUT_DOWN_PRESSED = 22,
	INPUT_LEFT_PRESSED = 23,
	INPUT_RIGHT_PRESSED = 24,
	INPUT_JUMP_PRESSED = 25,
	INPUT_ATTACK_PRESSED = 26,
	INPUT_ABILITY_PRESSED = 27,
	INPUT_INVENTORY_PRESSED = 28,
	INPUT_MENU_PRESSED = 29,
	INPUT_MAP_PRESSED = 30,
	INPUT_PREVIOUS_PRESSED = 31,
	INPUT_PREVIOUS_2_PRESSED = 32,
	INPUT_NEXT_PRESSED = 33,
	INPUT_NEXT_2_PRESSED = 34,
	TIPS_TEXT_LOADED = 51,
	TIPS_ICONS_LOADED = 52,
	RESOURCES_LOADED = 53,
	SPRITES_LOADED = 54,
	LANGUAGE_LOADED = 55,
	SFX_BASIC_PRELOAD_COMPLETE = 56,
	SFX_FULL_PRELOAD_COMPLETE = 57,
	AREA_BUNDLE_PRELOAD_COMPLETE = 58,
	MUSIC_PRELOAD_COMPLETE = 59,
	MUSIC_TRANSITION_COMPLETE = 60,
	DELAY_COMPLETE = 61,
	TOGGLE_MAP = 71,
	TOGGLE_MENU = 72,
	SAVE_SUCCESSFUL = 73,
	HEALTH_BAR_DEPLETED = 74,
	HEALTH_BAR_FULL = 75,
	EQUIPPED_ITEM_UPDATED = 81,
	EQUIPPED_ABILITY_UPDATED = 82,
	EQUIPPED_WEAPON_UPDATED = 83,
	EQUIPPED_ARMOR_UPDATED = 84,
	EQUIPPED_SHIELD_UPDATED = 85,
	EQUIPPED_BOOTS_UPDATED = 86,
	GOLD_UPDATED = 87,
	ELIXIRS_UPDATED = 88,
	PLAYER_DAMAGED = 101,
	PLAYER_DEAD = 102,
	PLAYER_DAMAGE_REVIVE = 103,
	DIALOG_DISPLAY_START = 111,
	DIALOG_DISPLAY_COMPLETE = 112,
	DIALOG_DISPLAY_END = 113,
	SCENE_AREA_TRANSITION = 114,
	TRANSITION_PLAYER_ANIMATE = 115,          // Signals that the player should now show a specific animation
	LOCATION_TRANSITION_START = 116,          // Signals the start of a location transition
	LOCATION_TRANSITION_STARTED = 117,        // Signals that the location transition has successfully started e.g. door open&close animations complete
	LOCATION_TRANSITION_PLAYER_MOVE = 118,    // Signals that the player should now move
	LOCATION_TRANSITION_PLAYER_MOVED = 119,   // Signals that the player has successfully moved
	LOCATION_TRANSITION_CAMERA_MOVED = 120,   // Signals that the camera has successfully moved
	LOCATION_TRANSITION_CONTINUE = 121,       // Signals that a transition from a previous scene should continue
	LOCATION_TRANSITION_FINISHED = 122,
	LAYER_REVEAL = 151,
	LAYER_CONCEAL = 152,
	LAYER_RESIZE_FINISHED = 153,
	LAYER_TRANSITION_START = 161,
	LAYER_TRANSITION_STARTED = 162,
	LAYER_TRANSITION_PLAYER_MOVE = 163,       // Signals that the player should now move
	LAYER_TRANSITION_PLAYER_MOVED = 164,      // Signals that the player has successfully moved to another layer
	LAYER_TRANSITION_FINISHED = 165,
	CAMERA_RETURNED_TO_PLAYER = 171,
	CAMERA_MOVED_TO_POSITION = 172,
	STORY_NOTIFICATIONS_COMPLETED = 181,
	SPAWN_LOOT_DISPLAY = 182,
	SPAWN_LOOT_DISPLAY_COMPLETE = 183,
	OBJECT_ACTIVATE_START = 191,
	PLAYER_ANIMATION_FINISHED = 201
}

public class MM_EventData {
	public Automator? automator;
	public CharacterStates.MovementStates? forcePlayerMovementState;
	public string objectName;
	public SceneArea? sceneAreaTransition_destinationArea;
	public int? sceneAreaTransition_destinationAreaEntrance;
	public Vector2? sceneAreaTransition_offset;
	public bool? sceneAreaTransition_offsetX;
	public bool? sceneAreaTransition_offsetY;
	public Vector2? sceneAreaTransition_speed;
	public bool? sceneAreaTransition_spawn;
	public Vector2? transition_position;
	public int? sourceId;
	public int? destinationId;
	public bool? transition_lookAway;
	public TransitionDisplayType? transition_displayType;
	public SceneAreaLayer? sceneAreaLayer;
	public DialogType? dialogType;
	public int? uiLabelId;
	public PlayerItem? playerItem;
	public int? count;
	public bool? toggleState;
}

public class MM_Events : MonoBehaviour {

	class MM_Events_Group {
		public Dictionary<MM_Event, bool> eventStatus;
		public MM_Event onComplete;
	}

	// Declare delegates for each event
	public delegate void FadeToBlackComplete();
	public delegate void SceneLoaded();
	public delegate void SceneReady();
	public delegate void EngineReady();
	public delegate void PlayerReady();
	public delegate void ObjectReset();
	public delegate void ForcePlayerMovementState(MM_EventData data);
	public delegate void StoredGameLoaded();
	public delegate void VolumeSFXChange(float volume);
	public delegate void VolumeMusicChange(float volume);
	public delegate void InputPressed(MM_Input input);
	public delegate void TipsTextLoaded();
	public delegate void TipsIconsLoaded();
	public delegate void ResourcesLoaded();
	public delegate void SpritesLoaded();
	public delegate void LanguageLoaded();
	public delegate void SFXBasicPreloadComplete();
	public delegate void SFXFullPreloadComplete();
	public delegate void AreaBundlePreloadComplete();
	public delegate void MusicPreloadComplete();
	public delegate void MusicTransitionComplete();
	public delegate void DelayComplete();
	public delegate void ToggleMap();
	public delegate void ToggleMenu();
	public delegate void SaveSuccessful();
	public delegate void HealthBarDepleted();
	public delegate void HealthBarFull();
	public delegate void EquippedItemUpdated();
	public delegate void EquippedAbilityUpdated();
	public delegate void EquippedWeaponUpdated();
	public delegate void EquippedArmorUpdated();
	public delegate void EquippedShieldUpdated();
	public delegate void EquippedBootsUpdated();
	public delegate void GoldUpdated();
	public delegate void ElixirsUpdated();
	public delegate void PlayerDamaged();
	public delegate void PlayerDead();
	public delegate void PlayerDamageRevive();
	public delegate void DialogDisplayStart(MM_EventData data);
	public delegate void DialogDisplayComplete(MM_EventData data);
	public delegate void DialogDisplayEnd();
	public delegate void TransitionPlayerAnimate(MM_EventData data);
	public delegate void SceneAreaTransition(MM_EventData data);
	public delegate void LocationTransitionStart(MM_EventData data);
	public delegate void LocationTransitionStarted(MM_EventData data);
	public delegate void LocationTransitionPlayerMove(MM_EventData data);
	public delegate void LocationTransitionPlayerMoved(MM_EventData data);
	public delegate void LocationTransitionCameraMoved();
	public delegate void LocationTransitionContinue(MM_EventData data);
	public delegate void LocationTransitionFinished();
	public delegate void LayerReveal(MM_EventData data);
	public delegate void LayerConceal(MM_EventData data);
	public delegate void LayerResizeFinished();
	public delegate void LayerTransitionStart(MM_EventData data);
	public delegate void LayerTransitionStarted(MM_EventData data);
	public delegate void LayerTransitionPlayerMove(MM_EventData data);
	public delegate void LayerTransitionPlayerMoved(MM_EventData data);
	public delegate void LayerTransitionFinished();
	public delegate void CameraReturnedToPlayer(MM_EventData data);
	public delegate void CameraMovedToPosition(MM_EventData data);
	public delegate void StoryNotificationsCompleted();
	public delegate void SpawnLootDisplay(MM_EventData data);
	public delegate void SpawnLootDisplayComplete(MM_EventData data);
	public delegate void ObjectActivateStart(MM_EventData data);
	public delegate void PlayerAnimationFinished(MM_EventData data);

	// Declare events
	public static event FadeToBlackComplete OnFadeToBlackComplete;
	public static event SceneLoaded OnSceneLoaded;
	public static event SceneReady OnSceneReady;
	public static event EngineReady OnEngineReady;
	public static event PlayerReady OnPlayerReady;
	public static event ObjectReset OnObjectReset;
	public static event ForcePlayerMovementState OnForcePlayerMovementState;
	public static event StoredGameLoaded OnStoredGameLoaded;
	public static event VolumeSFXChange OnVolumeSFXChange;
	public static event VolumeMusicChange OnVolumeMusicChange;
	public static event InputPressed OnInputPressed;
	public static event TipsTextLoaded OnTipsTextLoaded;
	public static event TipsIconsLoaded OnTipsIconsLoaded;
	public static event ResourcesLoaded OnResourcesLoaded;
	public static event SpritesLoaded OnSpritesLoaded;
	public static event LanguageLoaded OnLanguageLoaded;
	public static event SFXBasicPreloadComplete OnSFXBasicPreloadComplete;
	public static event SFXFullPreloadComplete OnSFXFullPreloadComplete;
	public static event AreaBundlePreloadComplete OnAreaBundlePreloadComplete;
	public static event MusicPreloadComplete OnMusicPreloadComplete;
	public static event MusicTransitionComplete OnMusicTransitionComplete;
	public static event DelayComplete OnDelayComplete;
	public static event ToggleMap OnToggleMap;
	public static event ToggleMenu OnToggleMenu;
	public static event SaveSuccessful OnSaveSuccessful;
	public static event HealthBarDepleted OnHealthBarDepleted;
	public static event HealthBarFull OnHealthBarFull;
	public static event EquippedItemUpdated OnEquippedItemUpdated;
	public static event EquippedAbilityUpdated OnEquippedAbilityUpdated;
	public static event EquippedWeaponUpdated OnEquippedWeaponUpdated;
	public static event EquippedArmorUpdated OnEquippedArmorUpdated;
	public static event EquippedShieldUpdated OnEquippedShieldUpdated;
	public static event EquippedBootsUpdated OnEquippedBootsUpdated;
	public static event GoldUpdated OnGoldUpdated;
	public static event ElixirsUpdated OnElixirsUpdated;
	public static event PlayerDamaged OnPlayerDamaged;
	public static event PlayerDead OnPlayerDead;
	public static event PlayerDamageRevive OnPlayerDamageRevive;
	public static event DialogDisplayStart OnDialogDisplayStart;
	public static event DialogDisplayComplete OnDialogDisplayComplete;
	public static event DialogDisplayEnd OnDialogDisplayEnd;
	public static event TransitionPlayerAnimate OnTransitionPlayerAnimate;
	public static event SceneAreaTransition OnSceneAreaTransition;
	public static event LocationTransitionStart OnLocationTransitionStart;
	public static event LocationTransitionStarted OnLocationTransitionStarted;
	public static event LocationTransitionPlayerMove OnLocationTransitionPlayerMove;
	public static event LocationTransitionPlayerMoved OnLocationTransitionPlayerMoved;
	public static event LocationTransitionCameraMoved OnLocationTransitionCameraMoved;
	public static event LocationTransitionContinue OnLocationTransitionContinue;
	public static event LocationTransitionFinished OnLocationTransitionFinished;
	public static event LayerReveal OnLayerReveal;
	public static event LayerConceal OnLayerConceal;
	public static event LayerResizeFinished OnLayerResizeFinished;
	public static event LayerTransitionStart OnLayerTransitionStart;
	public static event LayerTransitionStarted OnLayerTransitionStarted;
	public static event LayerTransitionPlayerMove OnLayerTransitionPlayerMove;
	public static event LayerTransitionPlayerMoved OnLayerTransitionPlayerMoved;
	public static event LayerTransitionFinished OnLayerTransitionFinished;
	public static event CameraReturnedToPlayer OnCameraReturnedToPlayer;
	public static event CameraMovedToPosition OnCameraMovedToPosition;
	public static event StoryNotificationsCompleted OnStoryNotificationsCompleted;
	public static event SpawnLootDisplay OnSpawnLootDisplay;
	public static event SpawnLootDisplayComplete OnSpawnLootDisplayComplete;
	public static event ObjectActivateStart OnObjectActivateStart;
	public static event PlayerAnimationFinished OnPlayerAnimationFinished;

	private Dictionary<int, MM_Events_Group> _eventGroupListeners = new Dictionary<int, MM_Events_Group>();

	public void Trigger(MM_Event e, float delay, MM_EventData data = null, bool checkEventGroupListeners = true) {
		if (delay <= 0f) {
			Trigger(e, data, checkEventGroupListeners);
		} else {
			StartCoroutine(_delayTrigger(e, delay, data, checkEventGroupListeners));
		}
	}

	public void Trigger(MM_Event e, MM_EventData data = null, bool checkEventGroupListeners = true) {

		Debug.Log("MM_Events:Trigger : triggering event " + e.ToString());

		// Trigger the appropriate event which will call all event handlers that are listening
		switch (e) {
			case MM_Event.FADE_TO_BLACK_COMPLETE:
				if (OnFadeToBlackComplete != null) { OnFadeToBlackComplete(); }
				break;
			case MM_Event.SCENE_LOADED:
				if (OnSceneLoaded != null) { OnSceneLoaded(); }
				break;
			case MM_Event.SCENE_READY:
				if (OnSceneReady != null) { OnSceneReady(); }
				break;
			case MM_Event.ENGINE_READY:
				if (OnEngineReady != null) { OnEngineReady(); }
				break;
			case MM_Event.PLAYER_READY:
				if (OnPlayerReady != null) { OnPlayerReady(); }
				break;
			case MM_Event.OBJECT_RESET:
				if (OnObjectReset != null) { OnObjectReset(); }
				break;
			case MM_Event.FORCE_PLAYER_MOVEMENT_STATE:
				if (OnForcePlayerMovementState != null) { OnForcePlayerMovementState(data); }
				break;
			case MM_Event.STORED_GAME_LOADED:
				if (OnStoredGameLoaded != null) { OnStoredGameLoaded(); }
				break;
			case MM_Event.VOLUME_SFX_CHANGE:
				if (OnVolumeSFXChange != null) { OnVolumeSFXChange(MM.gameManager.VolumeSFX); }
				break;
			case MM_Event.VOLUME_MUSIC_CHANGE:
				if (OnVolumeMusicChange != null) { OnVolumeMusicChange(MM.gameManager.VolumeMusic); }
				break;
			case MM_Event.INPUT_UP_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.UP); }
				break;
			case MM_Event.INPUT_DOWN_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.DOWN); }
				break;
			case MM_Event.INPUT_LEFT_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.LEFT); }
				break;
			case MM_Event.INPUT_RIGHT_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.RIGHT); }
				break;
			case MM_Event.INPUT_JUMP_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.JUMP); }
				break;
			case MM_Event.INPUT_ATTACK_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.ATTACK); }
				break;
			case MM_Event.INPUT_ABILITY_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.ABILITY); }
				break;
			case MM_Event.INPUT_INVENTORY_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.INVENTORY); }
				break;
			case MM_Event.INPUT_MENU_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.MENU); }
				break;
			case MM_Event.INPUT_MAP_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.MAP); }
				break;
			case MM_Event.INPUT_PREVIOUS_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.PREVIOUS); }
				break;
			case MM_Event.INPUT_PREVIOUS_2_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.PREVIOUS_2); }
				break;
			case MM_Event.INPUT_NEXT_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.NEXT); }
				break;
			case MM_Event.INPUT_NEXT_2_PRESSED:
				if (OnInputPressed != null) { OnInputPressed(MM_Input.NEXT_2); }
				break;
			case MM_Event.TIPS_TEXT_LOADED:
				if (OnTipsTextLoaded != null) { OnTipsTextLoaded(); }
				break;
			case MM_Event.TIPS_ICONS_LOADED:
				if (OnTipsIconsLoaded != null) { OnTipsIconsLoaded(); }
				break;
			case MM_Event.RESOURCES_LOADED:
				if (OnResourcesLoaded != null) { OnResourcesLoaded(); }
				break;
			case MM_Event.SPRITES_LOADED:
				if (OnSpritesLoaded != null) { OnSpritesLoaded(); }
				break;
			case MM_Event.LANGUAGE_LOADED:
				if (OnLanguageLoaded != null) { OnLanguageLoaded(); }
				break;
			case MM_Event.SFX_BASIC_PRELOAD_COMPLETE:
				if (OnSFXBasicPreloadComplete != null) { OnSFXBasicPreloadComplete(); }
				break;
			case MM_Event.SFX_FULL_PRELOAD_COMPLETE:
				if (OnSFXFullPreloadComplete != null) { OnSFXFullPreloadComplete(); }
				break;
			case MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE:
				if (OnAreaBundlePreloadComplete != null) { OnAreaBundlePreloadComplete(); }
				break;
			case MM_Event.MUSIC_PRELOAD_COMPLETE:
				if (OnMusicPreloadComplete != null) { OnMusicPreloadComplete(); }
				break;
			case MM_Event.MUSIC_TRANSITION_COMPLETE:
				if (OnMusicTransitionComplete != null) { OnMusicTransitionComplete(); }
				break;
			case MM_Event.DELAY_COMPLETE:
				if (OnDelayComplete != null) { OnDelayComplete(); }
				break;
			case MM_Event.TOGGLE_MAP:
				if (OnToggleMap != null) { OnToggleMap(); }
				break;
			case MM_Event.TOGGLE_MENU:
				if (OnToggleMenu != null) { OnToggleMenu(); }
				break;
			case MM_Event.SAVE_SUCCESSFUL:
				if (OnSaveSuccessful != null) { OnSaveSuccessful(); }
				break;
			case MM_Event.HEALTH_BAR_DEPLETED:
				if (OnHealthBarDepleted != null) { OnHealthBarDepleted(); }
				break;
			case MM_Event.HEALTH_BAR_FULL:
				if (OnHealthBarFull != null) { OnHealthBarFull(); }
				break;
			case MM_Event.EQUIPPED_ITEM_UPDATED:
				if (OnEquippedItemUpdated != null) { OnEquippedItemUpdated(); }
				break;
			case MM_Event.EQUIPPED_ABILITY_UPDATED:
				if (OnEquippedAbilityUpdated != null) { OnEquippedAbilityUpdated(); }
				break;
			case MM_Event.EQUIPPED_WEAPON_UPDATED:
				if (OnEquippedWeaponUpdated != null) { OnEquippedWeaponUpdated(); }
				break;
			case MM_Event.EQUIPPED_ARMOR_UPDATED:
				if (OnEquippedArmorUpdated != null) { OnEquippedArmorUpdated(); }
				break;
			case MM_Event.EQUIPPED_SHIELD_UPDATED:
				if (OnEquippedShieldUpdated != null) { OnEquippedShieldUpdated(); }
				break;
			case MM_Event.EQUIPPED_BOOTS_UPDATED:
				if (OnEquippedBootsUpdated != null) { OnEquippedBootsUpdated(); }
				break;
			case MM_Event.GOLD_UPDATED:
				if (OnGoldUpdated != null) { OnGoldUpdated(); }
				break;
			case MM_Event.ELIXIRS_UPDATED:
				if (OnElixirsUpdated != null) { OnElixirsUpdated(); }
				break;
			case MM_Event.PLAYER_DAMAGED:
				if (OnPlayerDamaged != null) { OnPlayerDamaged(); }
				break;
			case MM_Event.PLAYER_DEAD:
				if (OnPlayerDead != null) { OnPlayerDead(); }
				break;
			case MM_Event.PLAYER_DAMAGE_REVIVE:
				if (OnPlayerDamageRevive != null) { OnPlayerDamageRevive(); }
				break;
			case MM_Event.DIALOG_DISPLAY_START:
				if (OnDialogDisplayStart != null) { OnDialogDisplayStart(data); }
				break;
			case MM_Event.DIALOG_DISPLAY_COMPLETE:
				if (OnDialogDisplayComplete != null) { OnDialogDisplayComplete(data); }
				break;
			case MM_Event.DIALOG_DISPLAY_END:
				if (OnDialogDisplayEnd != null) { OnDialogDisplayEnd(); }
				break;
			case MM_Event.TRANSITION_PLAYER_ANIMATE:
				if (OnTransitionPlayerAnimate != null) { OnTransitionPlayerAnimate(data); }
				break;
			case MM_Event.SCENE_AREA_TRANSITION:
				if (OnSceneAreaTransition != null) { OnSceneAreaTransition(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_START:
				if (OnLocationTransitionStart != null) { OnLocationTransitionStart(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_STARTED:
				if (OnLocationTransitionStarted != null) { OnLocationTransitionStarted(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_PLAYER_MOVE:
				if (OnLocationTransitionPlayerMove != null) { OnLocationTransitionPlayerMove(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_PLAYER_MOVED:
				if (OnLocationTransitionPlayerMoved != null) { OnLocationTransitionPlayerMoved(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_CAMERA_MOVED:
				if (OnLocationTransitionCameraMoved != null) { OnLocationTransitionCameraMoved(); }
				break;
			case MM_Event.LOCATION_TRANSITION_CONTINUE:
				if (OnLocationTransitionContinue != null) { OnLocationTransitionContinue(data); }
				break;
			case MM_Event.LOCATION_TRANSITION_FINISHED:
				if (OnLocationTransitionFinished != null) { OnLocationTransitionFinished(); }
				break;
			case MM_Event.LAYER_REVEAL:
				if (OnLayerReveal != null) { OnLayerReveal(data); }
				break;
			case MM_Event.LAYER_CONCEAL:
				if (OnLayerConceal != null) { OnLayerConceal(data); }
				break;
			case MM_Event.LAYER_RESIZE_FINISHED:
				if (OnLayerResizeFinished != null) { OnLayerResizeFinished(); }
				break;
			case MM_Event.LAYER_TRANSITION_START:
				if (OnLayerTransitionStart != null) { OnLayerTransitionStart(data); }
				break;
			case MM_Event.LAYER_TRANSITION_STARTED:
				if (OnLayerTransitionStarted != null) { OnLayerTransitionStarted(data); }
				break;
			case MM_Event.LAYER_TRANSITION_PLAYER_MOVE:
				if (OnLayerTransitionPlayerMove != null) { OnLayerTransitionPlayerMove(data); }
				break;
			case MM_Event.LAYER_TRANSITION_PLAYER_MOVED:
				if (OnLayerTransitionPlayerMoved != null) { OnLayerTransitionPlayerMoved(data); }
				break;
			case MM_Event.LAYER_TRANSITION_FINISHED:
				if (OnLayerTransitionFinished != null) { OnLayerTransitionFinished(); }
				break;
			case MM_Event.CAMERA_RETURNED_TO_PLAYER:
				if (OnCameraReturnedToPlayer != null) { OnCameraReturnedToPlayer(data); }
				break;
			case MM_Event.CAMERA_MOVED_TO_POSITION:
				if (OnCameraMovedToPosition != null) { OnCameraMovedToPosition(data); }
				break;
			case MM_Event.STORY_NOTIFICATIONS_COMPLETED:
				if (OnStoryNotificationsCompleted != null) { OnStoryNotificationsCompleted(); }
				break;
			case MM_Event.SPAWN_LOOT_DISPLAY:
				if (OnSpawnLootDisplay != null) { OnSpawnLootDisplay(data); }
				break;
			case MM_Event.SPAWN_LOOT_DISPLAY_COMPLETE:
				if (OnSpawnLootDisplayComplete != null) { OnSpawnLootDisplayComplete(data); }
				break;
			case MM_Event.OBJECT_ACTIVATE_START:
				if (OnObjectActivateStart != null) { OnObjectActivateStart(data); }
				break;
			case MM_Event.PLAYER_ANIMATION_FINISHED:
				if (OnPlayerAnimationFinished != null) { OnPlayerAnimationFinished(data); }
				break;
		}

		// Check the _eventGroupListeners collection and update status for any groups containing this event
		if (checkEventGroupListeners) {
			bool allStatusTrue;
			List<int> groupKeys = new List<int>(_eventGroupListeners.Keys);
			foreach (int groupId in groupKeys) {
				allStatusTrue = true;
				if (_eventGroupListeners.ContainsKey(groupId)) {
					Dictionary<MM_Event, bool> eventStatus = new Dictionary<MM_Event, bool>(_eventGroupListeners[groupId].eventStatus);
					foreach (KeyValuePair<MM_Event, bool> status in eventStatus) {
						// Update the status for this event to true
						if (status.Key == e) {
							_eventGroupListeners[groupId].eventStatus[status.Key] = true;
						}
						// Check each event to see if the group has been completed
						if (_eventGroupListeners[groupId].eventStatus[status.Key] == false) {
							allStatusTrue = false;
						}
					}
					if (allStatusTrue) {
						MM_Event groupCompleteEvent = _eventGroupListeners[groupId].onComplete;
						// Remove the group from the collection
						_eventGroupListeners.Remove(groupId);

						Debug.Log("MM_Events:Trigger : event group " + groupId.ToString() + " has completed, triggering onComplete " + groupCompleteEvent.ToString());
						Trigger(groupCompleteEvent, null, false);
					}
				}
			}
		}
	}

	public int AddEventGroupListener(List<MM_Event> events, MM_Event onComplete) {
		int eventGroupId = 0;
		MM_Events_Group group = new MM_Events_Group { eventStatus = new Dictionary<MM_Event, bool>() };

		// Determine ID of new event group
		while (_eventGroupListeners.ContainsKey(eventGroupId++)) ;

		Debug.Log("MM_Events:AddEventGroupListener : adding new event group listener " + eventGroupId.ToString() + " containing " + String.Join(",", events.Select(s => s.ToString()).ToArray()));

		// Build the event status collection with false statuses to indicate the events have not been called yet
		foreach (MM_Event eventType in events) {
			group.eventStatus.Add(eventType, false);
		}

		// Define the event to trigger when all events in the group have been called
		group.onComplete = onComplete;

		// Add the event group to the list
		_eventGroupListeners.Add(eventGroupId, group);

		return eventGroupId;
	}

	public void RunCodeAfterDelay(float delay, Func<bool> code) {
		StartCoroutine(_delayCode(delay, code));
	}

	public void StartDelay(float delay) {
		StartCoroutine(_processDelay(delay));
	}

	public void EndDelay() {
		Trigger(MM_Event.DELAY_COMPLETE);
	}

	private IEnumerator _processDelay(float delay) {
		yield return new WaitForSecondsRealtime(delay);
		EndDelay();
	}

	private IEnumerator _delayTrigger(MM_Event e, float delay, MM_EventData data, bool checkEventGroupListeners) {
		yield return new WaitForSecondsRealtime(delay);
		Trigger(e, data, checkEventGroupListeners);
	}

	private IEnumerator _delayCode(float delay, Func<bool> code) {
		yield return new WaitForSecondsRealtime(delay);
		code();
	}
}
