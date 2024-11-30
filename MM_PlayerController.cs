using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_PlayerController : MonoBehaviour {

	private MM_AIController _AI;
	private bool _playerAnimationBlocked;
	private bool _transition_lookAway;
	private bool _gameInteractionActive;
	private DialogType? _currentDialogType;
	private IEnumerator _playerKnockbackCoroutine;
	private bool _playerKnockbackCoroutineRunning;

	private const float _DELAY_WALK_FORWARD_TIME = 0.25f;

	void OnEnable() {
		MM_Events.OnPlayerReady += _playerReady;
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnPlayerDamaged += _playerDamaged;
		MM_Events.OnPlayerDead += _playerDead;
		MM_Events.OnPlayerDamageRevive += _playerDamageRevive;
		MM_Events.OnStoredGameLoaded += _storedGameLoaded;
		MM_Events.OnDialogDisplayStart += _dialogDisplayStart;
		MM_Events.OnDialogDisplayComplete += _dialogDisplayComplete;
		MM_Events.OnDialogDisplayEnd += _returnToIdleEnableGameInput;
		MM_Events.OnTransitionPlayerAnimate += _transitionPlayerAnimate;
		MM_Events.OnSceneAreaTransition += _sceneAreaTransition;
		MM_Events.OnLocationTransitionStart += _locationTransitionStart;
		MM_Events.OnLocationTransitionStarted += _locationTransitionStarted;
		MM_Events.OnLocationTransitionPlayerMoved += _locationTransitionPlayerMoved;
		MM_Events.OnLocationTransitionContinue += _locationTransitionContinue;
		MM_Events.OnLocationTransitionFinished += _locationTransitionFinished;
		MM_Events.OnLayerTransitionStart += _layerTransitionStart;
		MM_Events.OnLayerTransitionStarted += _layerTransitionStarted;
		MM_Events.OnLayerTransitionPlayerMoved += _layerTransitionPlayerMoved;
		MM_Events.OnLayerTransitionFinished += _layerTransitionFinished;
		MM_Events.OnObjectActivateStart += _objectActivateStart;
	}

	void OnDisable() {
		MM_Events.OnPlayerReady -= _playerReady;
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnPlayerDamaged -= _playerDamaged;
		MM_Events.OnPlayerDead -= _playerDead;
		MM_Events.OnPlayerDamageRevive -= _playerDamageRevive;
		MM_Events.OnStoredGameLoaded -= _storedGameLoaded;
		MM_Events.OnDialogDisplayStart -= _dialogDisplayStart;
		MM_Events.OnDialogDisplayComplete -= _dialogDisplayComplete;
		MM_Events.OnDialogDisplayEnd -= _returnToIdleEnableGameInput;
		MM_Events.OnTransitionPlayerAnimate -= _transitionPlayerAnimate;
		MM_Events.OnSceneAreaTransition -= _sceneAreaTransition;
		MM_Events.OnLocationTransitionStart -= _locationTransitionStart;
		MM_Events.OnLocationTransitionStarted -= _locationTransitionStarted;
		MM_Events.OnLocationTransitionPlayerMoved -= _locationTransitionPlayerMoved;
		MM_Events.OnLocationTransitionContinue -= _locationTransitionContinue;
		MM_Events.OnLocationTransitionFinished -= _locationTransitionFinished;
		MM_Events.OnLayerTransitionStart -= _layerTransitionStart;
		MM_Events.OnLayerTransitionStarted -= _layerTransitionStarted;
		MM_Events.OnLayerTransitionPlayerMoved -= _layerTransitionPlayerMoved;
		MM_Events.OnLayerTransitionFinished -= _layerTransitionFinished;
		MM_Events.OnObjectActivateStart -= _objectActivateStart;
	}

	void Start() {
		_AI = new MM_AIController();

		if (_playerKnockbackCoroutineRunning) {
			StopCoroutine(_playerKnockbackCoroutine);
		}
		_playerKnockbackCoroutineRunning = false;
	}

	public MM_AIController AI {
		get {
			return _AI;
		}
	}

	public bool PlayerAnimationBlocked {
		get {
			return _playerAnimationBlocked;
		}
		set {
			_playerAnimationBlocked = value;
		}
	}

	public void AddInputFocus(InputFocus focus) {
		MM.AddInputFocus(focus);
	}

	public void RevokeLastInputFocus() {
		MM.RevokeLastInputFocus();
	}

	public void PauseForGameMenu(bool locked) {
		if (locked) {
			GameManager.Instance.Pause();
			MM.playerController.LockAnimation_game();
			AddInputFocus(InputFocus.MENU);
		} else {
			GameManager.Instance.UnPause();
			StartCoroutine(_delayUnlockAnimation());
			RevokeLastInputFocus();
		}
	}

	public void ToggleGameInteraction(bool interact) {
		// Sets whether the player can currently interact with the game environment
		_gameInteractionActive = interact;
	}

	public bool IsGameInteractionActive() {
		return _gameInteractionActive;
	}

	public void ControlPlayer_game() {
		MM.Player.ControlMovement();
	}

	public void FreezePlayer_game() {
		MM.Player.Freeze();
	}

	public void LockAnimation_game() {
		MM.playerController.PlayerAnimationBlocked = true;
	}

	public void UnfreezePlayer_game() {
		MM.Player.UnFreeze();
	}

	public void UnlockAnimation_game() {
		MM.playerController.PlayerAnimationBlocked = false;
	}

	public bool IsInputAvailable_game() {
		// Determine if the game receives input
		return (!MM.IsGameInputLocked && MM.CurrentInputFocus == InputFocus.GAME);
	}

	public bool IsInputAvailable_menu() {
		// Determine if the menu receives input
		return (!MM.IsMenuInputLocked && MM.CurrentInputFocus == InputFocus.MENU);
	}

	public bool IsInputAvailable_dialog() {
		// Determine if a dialog receives input
		return (MM.CurrentInputFocus == InputFocus.DIALOG);
	}

	public bool IsPlayerControlAvailable() {
		// Determine if the player is controllable (e.g. in a knockback from damage the player cannot be controlled)
		return !MM.IsPlayerControlLocked;
	}

	public void ActivatePowerupEffect(PlayerItem powerup, bool active) {
		switch (powerup) {
			case PlayerItem.POWERUP_WINGED_BOOTS:
				// Turn on/off the player's fall multiplier
				MM.SceneController.PlayerSwitchManager()?.ApplyToAll_FallMultiplier(active ? MM_Constants.PLAYER_WINGED_BOOTS_FALL_MULTIPLIER : MM_Constants.PLAYER_DEFAULT_FALL_MULTIPLIER);
				if (active) {
					MM.SceneController.PlayerSwitchManager()?.ApplyToAll_VerticalForce(0f);
				}
				break;
		}
	}

	private void _playerReady() {
		if (MM.Player == null) {
			return;
		}
		UnfreezePlayer_game();
		ToggleGameInteraction(true);
		MM.SetGameInputLocked(false, InputLockType.SCENE_INITIALISING);
		MM.SetGameInputLocked(false, InputLockType.SCENEAREA_TRANSITION);
		MM.SetGameInputLocked(false, InputLockType.LOCATION_TRANSITION);
		MM.SetGameInputLocked(false, InputLockType.LOADING_SCENE);
		// Wait a few frames to reset the AnimationBlocked character parameter
		StartCoroutine(_delayUnlockAnimation());
		MM.ClearPlayerControlLocked();
		MM.ClearPlayerInvulnerability();
		
		if (MM.IsGameInputLocked) {
			Debug.LogWarning("MM_PlayerController:_playerReady : game input is still locked [" + MM.DebugGetGameInputLockedString() + "]");
		}
	}

	private void _inputReceived(MM_Input input) {
		if (MM.Player == null) {
			return;
		}
		if (IsInputAvailable_game()) {
			switch (input) {
				case MM_Input.INVENTORY:
					if (IsPlayerControlAvailable() && MM.player.EquippedItem != PlayerItem.EQUIPPED_NONE) {
						MM.player.UseItem(MM.player.EquippedItem);
					}
					break;
				case MM_Input.ABILITY:
					break;
				case MM_Input.PREVIOUS:
					MM.player.CycleEquippedAbility(MM_Constants.DIRECTION_BACKWARD);
					break;
				case MM_Input.NEXT:
					MM.player.CycleEquippedAbility(MM_Constants.DIRECTION_FORWARD);
					break;
				case MM_Input.PREVIOUS_2:
					MM.player.CycleEquippedItem(MM_Constants.DIRECTION_BACKWARD);
					break;
				case MM_Input.NEXT_2:
					MM.player.CycleEquippedItem(MM_Constants.DIRECTION_FORWARD);
					break;
				case MM_Input.MAP:
					MM.events.Trigger(MM_Event.TOGGLE_MAP);
					break;
				case MM_Input.MENU:
					MM.events.Trigger(MM_Event.TOGGLE_MENU);
					break;
			}
		} else if (IsInputAvailable_menu()) {
			switch (input) {
				case MM_Input.MAP:
					MM.events.Trigger(MM_Event.TOGGLE_MAP);
					break;
				case MM_Input.MENU:
					MM.events.Trigger(MM_Event.TOGGLE_MENU);
					break;
			}
		} else if (IsInputAvailable_dialog()) {
			_dialogActionInput(input);
		}
	}

	private IEnumerator _delayUnlockAnimation() {
		yield return new WaitForSeconds(MM_Constants.PLAYER_ANIM_UNBLOCK_DURATION);
		UnlockAnimation_game();
	}

	private void _playerKnockback() {
		if (_playerKnockbackCoroutineRunning) {
			StopCoroutine(_playerKnockbackCoroutine);
		} else {
			MM.SetPlayerControlLocked(true, PlayerControlLockType.DAMAGE_KNOCKBACK);
		}
		_playerKnockbackCoroutine = _continuePlayerKnockback();
		StartCoroutine(_playerKnockbackCoroutine);
		_playerKnockbackCoroutineRunning = true;
	}

	private IEnumerator _continuePlayerKnockback() {
		yield return new WaitForSeconds(MM_Constants.PLAYER_DAMAGED_NO_CONTROL_DURATION);
		_playerKnockbackCoroutineRunning = false;
		MM.SetPlayerControlLocked(false, PlayerControlLockType.DAMAGE_KNOCKBACK);
		MM.Player.FlickerSprite();
	}

	private void _playerDamaged() {
		if (MM.Player == null) {
			return;
		}
		// In most cases getting damaged will result in a knockback and temporary loss of player control
		// More scenarios will be added to this function as needed
		_playerKnockback();
	}

	private void _playerDead() {
		if (MM.Player == null) {
			return;
		}
		MM.SetPlayerControlLocked(true, PlayerControlLockType.PLAYER_DEATH);
		StartCoroutine(_delayPlayerDeath());
	}

	private IEnumerator _delayPlayerDeath() {
		// Tell objects within the scene to reset their state
		MM.events.Trigger(MM_Event.OBJECT_RESET);
		// Let the death animation complete
		yield return new WaitForSecondsRealtime(MM_Constants.PLAYER_DEATH_DELAY);
		// Reload from the last save point
		Debug.Log("MM_PlayerController:_playerDead : reloading last save point");
		MM.SceneController.StopSceneAreaMusicSFX(true);
		MM.gameManager.SetPlayedTimeOnLoad = MM.player.PlayedTime + (uint)(DateTime.Now - MM.player.LastLoadTime).TotalSeconds;
		MM.gameManager.LoadExistingGame(MM_Constants.SCENE_RELOAD_INDEX);
	}

	private void _playerDamageRevive() {
		if (MM.Player == null) {
			return;
		}
		MM.SetGameInputLocked(true, InputLockType.PLAYER_ANIMATION_SEQUENCE);
		AddInputFocus(InputFocus.GAME_BUSY);
		MM.SetPlayerControlLocked(true, PlayerControlLockType.USING_ITEM);
		MM.Player.IsReviving = true;
		MM.Player._controller.GravityActive(false);
		MM.Player._controller.SetHorizontalForce(0f);
		MM.Player._controller.SetVerticalForce(0f);
		MM.soundEffects.Play(MM_SFX.BOCK_DRINK1, true, MM_Constants.PLAYER_ELIXIR_DRINK_DELAY + MM_Constants.BARTENDER_DRINK_CHUG1_DELAY);
		MM.soundEffects.Play(MM_SFX.BOCK_DRINK1, true, MM_Constants.PLAYER_ELIXIR_DRINK_DELAY + MM_Constants.BARTENDER_DRINK_CHUG2_DELAY);
		MM.soundEffects.Play(MM_SFX.BOCK_SIGH, true, MM_Constants.PLAYER_ELIXIR_DRINK_DELAY + MM_Constants.BARTENDER_DRINK_SIGH_DELAY);
		MM.soundEffects.Play(MM_SFX.BOCK_HEAL2, true, MM_Constants.PLAYER_ELIXIR_DRINK_DELAY + MM_Constants.PLAYER_ELIXIR_HEAL_DELAY);
		StartCoroutine(_continuePlayerDamageRevive());
	}

	private IEnumerator _continuePlayerDamageRevive() {
		yield return new WaitForSecondsRealtime(MM_Constants.PLAYER_ELIXIR_DRINK_DELAY);
		MM.player.RemoveInventory(PlayerItem.ELIXIR, 1);

		// Stop time while the player is reviving and adjust the player animation to play in unscaled time
		MM.Player.CharacterAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
		GameManager.Instance.Pause();

		yield return new WaitForSecondsRealtime(MM_Constants.PLAYER_ELIXIR_HEAL_DELAY);
		MM.uiEffects.Play(GraphicEffect.HEAL_2, MM.Player.transform.position, 0, 0, 0, 0, true);
		MM.Player.ResetHealthToMaxHealth(MM_Constants.PLAYER_ELIXIR_HEAL_SPEED);
		yield return new WaitForSecondsRealtime(MM_Constants.PLAYER_ELIXIR_HEAL_SPEED);
		MM.soundEffects.StopDedicated();
		MM.Player.IsReviving = false;
		MM.SetPlayerControlLocked(false, PlayerControlLockType.USING_ITEM);
		MM.SetGameInputLocked(false, InputLockType.PLAYER_ANIMATION_SEQUENCE);
		RevokeLastInputFocus();
		MM.SetPlayerInvulnerability(false, PlayerInvulnerabilityType.DAMAGED);
		MM.SetPlayerInvulnerability(false, PlayerInvulnerabilityType.REVIVING);

		// Restore normal time and revert the player animation to play in scaled time
		MM.SetPlayerInvulnerability(true, PlayerInvulnerabilityType.DAMAGED);
		MM.Player._controller.GravityActive(true);
		MM.Player.FlickerSprite();
		MM.Player.StartDamagedCoroutine(MM_Constants.PLAYER_DEFAULT_INVULNERABILITY_DURATION);
		MM.Player.CharacterAnimator.updateMode = AnimatorUpdateMode.Normal;
		GameManager.Instance.UnPause();
	}

	private void _storedGameLoaded() {
		if (MM.Player == null) {
			return;
		}
		FreezePlayer_game();
		MM.events.Trigger(MM_Event.SCENE_AREA_TRANSITION, new MM_EventData {
			objectName = MM.player.CurrentSceneAreaEntranceObjectName,
			sceneAreaTransition_destinationArea = MM.player.CurrentSceneArea,
			sceneAreaTransition_destinationAreaEntrance = MM.player.CurrentSceneAreaEntrance,
			sceneAreaTransition_spawn = true
		});
	}

	private void _prepareDialog(bool? transition_lookAway, bool? toggleState) {
		// Make the player look forward or away
		if (transition_lookAway != null) {
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
				forcePlayerMovementState = ((bool)transition_lookAway ? CharacterStates.MovementStates.IdleAway : CharacterStates.MovementStates.IdleForwardUnraised)
			});
		}
		// Lock input and player movement during the dialog display
		AddInputFocus(InputFocus.DIALOG);
		if (toggleState == null || toggleState == true) {
			ToggleGameInteraction(false);
		}
	}

	private void _returnToIdleEnableGameInput() {
		if (MM.Player == null) {
			return;
		}
		// Return the player to an idle state
		MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
			forcePlayerMovementState = CharacterStates.MovementStates.Idle
		});
		ToggleGameInteraction(true);
	}

	private void _dialogDisplayStart(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			_currentDialogType = (DialogType)data.dialogType;
			_prepareDialog(data.transition_lookAway, data.toggleState);
			switch (_currentDialogType) {
				case DialogType.SIGNPOST:
				case DialogType.TALKING:
				case DialogType.PURCHASE:
					MM.hud.ShowDialog((DialogType)_currentDialogType, MM.lang.GetUILabel((MM_UILabel)data.uiLabelId));
					break;
				case DialogType.ITEM_DETAILS:
					MM.hud.ShowDialog((DialogType)_currentDialogType, ((int)data.playerItem).ToString(), (data.count != null ? (int)data.count : 0));
					break;
			}
		}
	}

	private void _dialogActionInput(MM_Input input) {
		switch (_currentDialogType) {
			case DialogType.SIGNPOST:
				if (input == MM_Input.JUMP || input == MM_Input.ATTACK || input == MM_Input.ABILITY || input == MM_Input.INVENTORY) {
					MM.hud.HideDialog(DialogType.SIGNPOST, HUDFadeType.QUICK);
				}
				break;
			case DialogType.TALKING:
				MM.hud.ProcessTalkDialogInput(input);
				break;
			case DialogType.ITEM_DETAILS:
				if (input == MM_Input.JUMP || input == MM_Input.ATTACK || input == MM_Input.ABILITY || input == MM_Input.INVENTORY) {
					MM.hud.HideDialog(DialogType.ITEM_DETAILS, HUDFadeType.QUICK);
				}
				break;
			case DialogType.PURCHASE:
				MM.hud.ProcessPurchaseDialogInput(input);
				break;
		}
	}

	private void _dialogDisplayComplete(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			RevokeLastInputFocus();
			if (data.toggleState == null || (bool)data.toggleState) {
				MM.hud.ResetDialogs();
				_returnToIdleEnableGameInput();
			}
			_currentDialogType = null;
		}
	}

	private void _transitionPlayerAnimate(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Make the player walk forward or away
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
				forcePlayerMovementState = ((bool)data.transition_lookAway ? CharacterStates.MovementStates.WalkAway : CharacterStates.MovementStates.WalkForward)
			});
		}
	}

	private void _sceneAreaTransition(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Start the transition to a new scene/area: set the destination
			MM.player.CurrentSceneArea = (SceneArea)data.sceneAreaTransition_destinationArea;
			MM.player.CurrentSceneAreaEntrance = (int)data.sceneAreaTransition_destinationAreaEntrance;
			MM.player.CurrentSceneAreaEntranceObjectName = data.objectName;
			if (data.automator != null && data.automator != Automator.NONE) {
				MM.player.SetAutomator((Automator)data.automator);
			}
			// Lock input, player movement and player animation during the scene/area transition
			MM.SetGameInputLocked(true, InputLockType.SCENEAREA_TRANSITION);
			if (data.objectName.Length > 0) {
				ControlPlayer_game();
			} else if (MM.Player.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead) {
				FreezePlayer_game();
			}
			LockAnimation_game();
		}
	}

	private void _locationTransitionStart(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Lock input and player movement during the location transition
			MM.SetGameInputLocked(true, InputLockType.LOCATION_TRANSITION);
			// Make the player look forward or away
			_transition_lookAway = (bool)data.transition_lookAway;
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = (_transition_lookAway ? CharacterStates.MovementStates.IdleAway : CharacterStates.MovementStates.IdleForward) });
		}
	}

	private void _locationTransitionStarted(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Location transition start animations have now completed: set damping on the
			// camera so the movement to the new player location is not instantaneous
			MM.cameraController.SetCameraMoving(0f, true);
			// Trigger the event that will move the player
			MM.events.Trigger(MM_Event.LOCATION_TRANSITION_PLAYER_MOVE, data);
		}
	}

	private void _locationTransitionPlayerMoved(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Make the player look forward or away
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = (_transition_lookAway ? CharacterStates.MovementStates.IdleForward : CharacterStates.MovementStates.IdleAway) });
		}
	}

	private void _locationTransitionContinue(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			if (data.transition_displayType == TransitionDisplayType.OPEN_WALK_FADE_OR_WALK_CLOSE) {
				// Make the player look forward
				MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = CharacterStates.MovementStates.IdleForward });
				StartCoroutine(_delayWalkForward());
			} else if (data.transition_displayType == TransitionDisplayType.OPEN_WALK_CLOSE) {
				// Make the player look forward
				MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = CharacterStates.MovementStates.IdleForward });
			}
		}
	}

	private void _locationTransitionFinished() {
		if (MM.Player == null) {
			return;
		}
		// Lock the camera back onto the player
		MM.cameraController.LockCamera();
		// Return the player to an idle state
		MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
			forcePlayerMovementState = CharacterStates.MovementStates.Idle
		});
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator) && MM_Constants_Automators.Automators[MM.player.CurrentAutomator].trigger == AutomatorTrigger.ON_LOCATION_TRANSITION_RETURN_PLAYER_INPUT) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
		} else {
			// Allow player input
			MM.SetGameInputLocked(false, InputLockType.LOCATION_TRANSITION);
			MM.events.Trigger(MM_Event.PLAYER_READY);
		}
	}

	private void _layerTransitionStart(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Lock input and player movement during the layer transition
			MM.SetGameInputLocked(true, InputLockType.LAYER_TRANSITION);
			// Make the player look forward or away
			_transition_lookAway = (bool)data.transition_lookAway;
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
				forcePlayerMovementState = (_transition_lookAway ? CharacterStates.MovementStates.IdleAway : CharacterStates.MovementStates.IdleForwardUnraised)
			});
		}
	}

	private void _layerTransitionStarted(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Trigger the event that will move the player
			MM.events.Trigger(MM_Event.LAYER_TRANSITION_PLAYER_MOVE, data);
		}
	}

	private void _layerTransitionPlayerMoved(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
	}

	private void _layerTransitionFinished() {
		if (MM.Player == null) {
			return;
		}
		// Return the player to an idle state
		MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
			forcePlayerMovementState = CharacterStates.MovementStates.Idle
		});
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator) && MM_Constants_Automators.Automators[MM.player.CurrentAutomator].trigger == AutomatorTrigger.ON_LAYER_TRANSITION_RETURN_PLAYER_INPUT) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
		} else {
			// Release input now that the layer transition has finished
			MM.SetGameInputLocked(false, InputLockType.LAYER_TRANSITION);
			UnfreezePlayer_game();
		}
	}

	private void _objectActivateStart(MM_EventData data) {
		if (MM.Player == null) {
			return;
		}
		if (data != null) {
			// Lock input and player movement during the object interaction
			MM.SetGameInputLocked(true, InputLockType.OBJECT_INTERACTION_SEQUENCE);
			if (data.toggleState == true) {
				// Start a scene/location/layer transition
				ControlPlayer_game();
			} else {
				FreezePlayer_game();
			}
			LockAnimation_game();
			// Make the player look forward or away
			_transition_lookAway = (bool)data.transition_lookAway;
			MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData { forcePlayerMovementState = (_transition_lookAway ? CharacterStates.MovementStates.IdleAway : CharacterStates.MovementStates.IdleForward) });
		}
	}

	private IEnumerator _delayWalkForward() {
		yield return new WaitForSeconds(_DELAY_WALK_FORWARD_TIME);
		// Make the player walk forward
		MM.events.Trigger(MM_Event.FORCE_PLAYER_MOVEMENT_STATE, new MM_EventData {
			forcePlayerMovementState = CharacterStates.MovementStates.WalkForward
		});
	}
}
