using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum Automator {
	NONE = 0,
	NEW_GAME = 1,
	NEW_GAME_SCENE_INTRO = 101,
	START_DESTROY_BOSS_DOOR = 102,
	START_BOSS_CLOSE_DOOR = 103,
	START_BOSS_SPAWN = 104,
	START_BOSS_DIE = 105,
	START_BOSS_REOPEN_DOOR = 106,
	START_GIVE_FIREBALL = 107,
	START_GIVE_DIRECTIONS = 108,
	START_MARK_MAP = 109,
	START_SPAWN_PLATFORM = 110,
	START_ACTIVATE_PLATFORM = 114
}

public enum AutomatorTrigger {
	ON_SCENE_READY = 1,
	ON_FADE_COMPLETE = 2,
	ON_SCENE_AREA_TRANSITION_COMPLETE = 3,
	ON_LOCATION_TRANSITION_RETURN_PLAYER_INPUT = 4,
	ON_GRAPHIC_EFFECT_COMPLETE = 5,
	ON_DIALOG_COMPLETE = 6,
	ON_LAYER_TRANSITION_RETURN_PLAYER_INPUT = 7,
	ON_PLAYER_CHARACTER_DEATH = 8,
	ON_OBJECT_READY = 9,
	ON_OBJECT_ACTIVATE = 10,
	ON_OBJECT_SPAWNER_COMPLETE = 11,
}

public enum AutomatorType {
	SERIES = 1,
	CAMERA_RETURN_TO_PLAYER = 2,
	CAMERA_PANNING = 3,
	RETURN_INPUT_TO_PLAYER_ENABLE_HUD = 4,
	SET_STORY_NOTIFICATION = 5,
	RETURN_INPUT_TO_PLAYER = 6,
	CLEAR_AUTOMATOR = 7,
	TRIGGER_LOCATION_TRANSITION_CONTINUE = 8,
	CAMERA_MOVE_TO_POSITION = 9,
}

public struct MM_Automator {
	public Automator automator;										// The automator index
	public bool runOnce;													// Whether the automator will run once only
	public bool ignoreChecks;											// Determines whether checks on the trigger value should cause subsequent code to be ignored (e.g. fade completion should be ignored when starting a new game)
	public AutomatorTrigger? trigger;							// The trigger that will cause this automator to be actioned, if it is the current automator
	public AutomatorType? type;										// The type of action that will be performed
	public Func<bool> onBeforeStart;							// Code that is run before the automator starts (possibly cancelling further action)
	public AutomatorType? onCompleteActionType;		// The type of action that will be triggered when the automator completes
	public Automator? onCompleteActionAutomator;	// The specific automator to run when the automation's pending actions have completed
	public Vector3? cameraPosition;
	public float cameraMovementDelay;
	public float cameraMovementTime;
	public MM_Automator(Automator automator, bool runOnce, bool ignoreChecks, AutomatorTrigger? trigger, AutomatorType? type = null, Func<bool> onBeforeStart = null, AutomatorType? onCompleteActionType = null, Automator? onCompleteActionAutomator = null, Vector3? cameraPosition = null, float cameraMovementDelay = 0f, float cameraMovementTime = 0f) {
		this.automator = automator;
		this.runOnce = runOnce;
		this.ignoreChecks = ignoreChecks;
		this.trigger = trigger;
		this.type = type;
		this.onBeforeStart = onBeforeStart;
		this.onCompleteActionType = onCompleteActionType;
		this.onCompleteActionAutomator = onCompleteActionAutomator;
		this.cameraPosition = cameraPosition;
		this.cameraMovementDelay = cameraMovementDelay;
		this.cameraMovementTime = cameraMovementTime;
	}
}

public static class MM_Constants_Automators {

	private static bool _objectActivated(PersistentObject persistentObject) {
		return (MM.player.ObjectData.ContainsKey(persistentObject) && MM.player.ObjectData[persistentObject].properties.ContainsKey(PersistentObjectProperty.ACTIVATED) && MM.player.ObjectData[persistentObject].properties[PersistentObjectProperty.ACTIVATED] != 0);
	}

	// Details for all areas in the game
	// Scene entrances can either be defined by location (for edge of screen transitions) or by gameobject name (for door transitions)
	public static Dictionary<Automator?, MM_Automator> Automators { get { return _automators; } }
	private static Dictionary<Automator?, MM_Automator> _automators = new Dictionary<Automator?, MM_Automator>() {
		{ Automator.NEW_GAME, new MM_Automator(Automator.NEW_GAME, true, true, AutomatorTrigger.ON_SCENE_READY, null, delegate() {
			// Load the intro scene and start the music and story notifications
			MM.SceneController.AddSceneOverlay(MM_Constants.SCENE_STORY_INTRO);
			MM.SceneController.StartMusicFromPositionAfterDelay(null, null, 0.5f, 0f);
			MM.cameraController.ResetBlending();
			MM.cameraController.SetSceneCameraTargets(MM.SceneController.FindSceneObject("StartCameraPosition"));
			// Remove the overlay fader
			MM.hud.TriggerFadeOut(0f, 5f);
			// Queue the story notifications
			MM.hud.QueueStoryNotifications(new List<MM_StoryNotification> {
				new MM_StoryNotification(StoryNotificationAlignment.BOTTOM, MM.lang.GetUILabel(MM_UILabel.STORY_INTRO_1), 6f),
				new MM_StoryNotification(StoryNotificationAlignment.BOTTOM, MM.lang.GetUILabel(MM_UILabel.STORY_INTRO_2), 6f)
			});
			// Add a camera panning action that will be triggered when the story notifications have been completed
			MM.automatorController.AddPendingAction(AutomatorType.CAMERA_PANNING, delegate() {
				MM.StoryCamera.GetComponent<MM_Graphics_RotateCamera>().Rotate(true);
				MM.hud.SetFadeCanvasColor(Color.white);
				MM.hud.TriggerFadeIn(4f, 3f);
				MM.player.SetAutomator(Automator.NEW_GAME_SCENE_INTRO);
			});
			return false;
		})},
		{ Automator.NEW_GAME_SCENE_INTRO, new MM_Automator(
			Automator.NEW_GAME_SCENE_INTRO, true, false, AutomatorTrigger.ON_FADE_COMPLETE, AutomatorType.CAMERA_RETURN_TO_PLAYER, delegate() {
				MM.SceneController.RemoveSceneOverlay(MM_Constants.SCENE_STORY_INTRO, delegate() {
					MM.lightingController.ApplySceneAreaLighting(true);
					MM.hud.ShowStoryNotification(StoryNotificationAlignment.TOP, MM.lang.GetUILabel(MM_UILabel.STORY_INTRO_3), 10f);
					MM.SceneController.ResetSceneObjects(true);
					MM.hud.TriggerFadeOut(0f, 3f);
				});
				return true;
			}, AutomatorType.RETURN_INPUT_TO_PLAYER_ENABLE_HUD, null, new Vector2(142f, -40f), 0f, 10f
		)},
		{ Automator.START_DESTROY_BOSS_DOOR, new MM_Automator(Automator.START_DESTROY_BOSS_DOOR, true, false, AutomatorTrigger.ON_SCENE_AREA_TRANSITION_COMPLETE, null, delegate() {
			Transform door = MM.SceneController.FindSceneObject("DoorSceneAreaTransition1");
			door.GetComponent<MM_Objects_SceneAreaTransition>().SetState(PersistentObjectState.DEACTIVATING);
			door.GetComponent<MM_Objects_SceneAreaTransition>().SetState(PersistentObjectState.INACTIVE, 3.9f, true);
			MM.uiEffects.Play(GraphicEffect.EXPLOSION_SET_1, door.position, 0.25f, 3f, 0.1f, 0.4f);
			return false;
		})},
		{ Automator.START_BOSS_CLOSE_DOOR, new MM_Automator(Automator.START_BOSS_CLOSE_DOOR, true, false, AutomatorTrigger.ON_LOCATION_TRANSITION_RETURN_PLAYER_INPUT, AutomatorType.RETURN_INPUT_TO_PLAYER, delegate() {
			MM.SceneController.FindSceneObject("LightsourceDoor").gameObject.SetActive(true);
			MM.SceneController.FindSceneObject("DoorCollider").gameObject.SetActive(false);
			MM.playerController.AI.SetHorizontalMove(600, 1, delegate() {
				MM.SceneController.FindSceneObject("LightsourceDoor").gameObject.SetActive(false);
				MM.SceneController.FindSceneObject("DoorCollider").gameObject.SetActive(true);
				MM.soundEffects.Play(MM_SFX.DOOR2_CLOSE, false, 0.4f);
				MM.player.SetAutomator(Automator.START_BOSS_SPAWN);
				Vector3 boss1Position = MM.SceneController.FindSceneObject("BossEntryPortal").position - new Vector3(0.5f, 0.5f);
				MM.uiEffects.Play(GraphicEffect.DEATH_MASTER_ENTRANCE, boss1Position, 1f, 4f);
				MM.spawner.SpawnEnemy(MM_SceneControllerEnemyPrefabs.ENEMY_3, SceneAreaLayer.LAYER_1, boss1Position, true, null, null, 0, 3f, Automator.START_BOSS_DIE);
				MM.soundEffects.Play(MM_SFX.BOSS_DEATH_MASTER_HISS, false, 3f);
				return false;
			});
			return true;
		})},
		{ Automator.START_BOSS_SPAWN, new MM_Automator(Automator.START_BOSS_SPAWN, true, false, AutomatorTrigger.ON_GRAPHIC_EFFECT_COMPLETE, null, delegate() {
			MM.hud.ShowAreaLabelNotification(AreaLabel.THE_DEATH_MASTER, 0.5f);
			MM.SceneController.StartMusicFromPositionAfterDelay(MusicalTrack.COMMON_BOSS_1, null, 0.5f, 0f);
			return false;
		})},
		{ Automator.START_BOSS_DIE, new MM_Automator(Automator.START_BOSS_DIE, true, false, AutomatorTrigger.ON_PLAYER_CHARACTER_DEATH, null, delegate() {
			Vector2 boss1ChestPosition = MM.SceneController.transform.Find("/[SimpleObjectPooler] Enemy3Pool").GetChild(0).transform.position;// + new Vector3(0.5f, 0.5f);
			MM.music.Stop(true);
			MM.SceneController.FindSceneObject("ChestBossReward").transform.position = boss1ChestPosition;
			MM.uiEffects.Play(GraphicEffect.EXPLOSION_SET_2, boss1ChestPosition, 0.5f, 6f, 0.1f, 0.2f);
			MM.spawner.SpawnSequence(new List<SpawnObject>() { SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_COIN_SMALL, SpawnObject.GOLD_WATER_JUG, SpawnObject.GOLD_WATER_JUG, SpawnObject.GOLD_WATER_JUG, SpawnObject.GOLD_WATER_JUG }, SceneAreaLayer.LAYER_1, boss1ChestPosition + new Vector2(0, 2), true, true, true, 3f, 0.2f);
			MM.soundEffects.Play(MM_SFX.BOSS_DEATH_MASTER_DEATH, false, 6f);
			MM.events.RunCodeAfterDelay(7.25f, delegate() {
				// Show the reward chest and play the fanfare
				MM.SceneController.FindSceneObject("ChestBossReward").gameObject.SetActive(true);
				MM.SceneController.transform.Find("/[SimpleObjectPooler] Enemy3Pool").GetChild(0).gameObject.SetActive(false);
				MM.soundEffects.Play(MM_SFX.FANFARE1);
				return false;
			});
			return false;
		})},
		{ Automator.START_BOSS_REOPEN_DOOR, new MM_Automator(Automator.START_BOSS_REOPEN_DOOR, false, false, AutomatorTrigger.ON_OBJECT_ACTIVATE, null, delegate() {
			MM.events.RunCodeAfterDelay(0.5f, delegate() {
				MM.SceneController.FindSceneObject("LightsourceDoor").gameObject.SetActive(true);
				MM.SceneController.FindSceneObject("DoorCollider").gameObject.SetActive(false);
				return false;
			});
			MM.soundEffects.Play(MM_SFX.DOOR2_OPEN);
			MM.player.SetAutomator(Automator.NONE);
			return false;
		})},
		{ Automator.START_GIVE_FIREBALL, new MM_Automator(Automator.START_GIVE_FIREBALL, true, false, AutomatorTrigger.ON_DIALOG_COMPLETE, null, delegate() {
			MM.SetGameInputLocked(true, InputLockType.OBJECT_INTERACTION_SEQUENCE);
			MM.player.SetAutomator(Automator.START_GIVE_DIRECTIONS);
			MM.events.Trigger(MM_Event.SPAWN_LOOT_DISPLAY, new MM_EventData() { sourceId = MM_Constants.UI_EFFECTS_ID, playerItem = PlayerItem.FIREBALL, count = 5 });
			MM.player.EnableAbility(PlayerItem.FIREBALL);
			MM.player.AddAbility(PlayerItem.FIREBALL, 5);
			return false;
		})},
		{ Automator.START_GIVE_DIRECTIONS, new MM_Automator(Automator.START_GIVE_DIRECTIONS, true, false, AutomatorTrigger.ON_DIALOG_COMPLETE, null, delegate() {
			MM.events.RunCodeAfterDelay(0.25f, delegate() {
				MM.events.Trigger(MM_Event.DIALOG_DISPLAY_START, new MM_EventData { dialogType = DialogType.TALKING, uiLabelId = (int)UILabelTalk.START_FORTUNE_TELLER_MARK_MAP });
				MM.SetGameInputLocked(false, InputLockType.OBJECT_INTERACTION_SEQUENCE);
				return false;
			});
			return false;
		})},
		{ Automator.START_MARK_MAP, new MM_Automator(Automator.START_MARK_MAP, true, false, AutomatorTrigger.ON_DIALOG_COMPLETE, null, delegate() {
			MM.SceneController.SetSceneObjectProperty("Mayor", PersistentObject.START_MAYOR, PersistentObjectProperty.TALK_UI_LABEL_ID, (int)UILabelTalk.START_FORTUNE_TELLER_GET_GOING);
			MM.player.SetAutomator(Automator.START_SPAWN_PLATFORM);
			MM.events.Trigger(MM_Event.DIALOG_DISPLAY_END);
			MM.events.RunCodeAfterDelay(1f, delegate() {
				MM.soundEffects.Play(MM_SFX.MENU_TAB);
				MM.hud.ShowInventoryUpdatedDialog(PlayerItem.FIREBALL);
				MM.hud.ShowMapUpdatedDialog();
				return false;
			});
			return false;
		})},
		{ Automator.START_SPAWN_PLATFORM, new MM_Automator(Automator.START_SPAWN_PLATFORM, true, false, AutomatorTrigger.ON_LAYER_TRANSITION_RETURN_PLAYER_INPUT, AutomatorType.RETURN_INPUT_TO_PLAYER, delegate() {
			MM.SetGameInputLocked(false, InputLockType.LAYER_TRANSITION);
			MM.events.RunCodeAfterDelay(0.5f, delegate() {
				MM.uiEffects.Play(GraphicEffect.SPAWN_1, MM.SceneController.FindSceneObject("Platform1").position);
				MM.SceneController.FindSceneObject("Platform1").gameObject.SetActive(true);
				MM.player.SetPersistentObjectProperty(PersistentObject.START_PLATFORM, PersistentObjectProperty.ACTIVATED, 1);
				return false;
			});
			return true;
		})},
		{ Automator.START_ACTIVATE_PLATFORM, new MM_Automator(Automator.START_ACTIVATE_PLATFORM, false, false, AutomatorTrigger.ON_OBJECT_ACTIVATE, null, delegate() {
			MM.SceneController.FindSceneObject("Platform1").gameObject.SetActive(_objectActivated(PersistentObject.START_PLATFORM));
			return false;
		})}
	};
}
