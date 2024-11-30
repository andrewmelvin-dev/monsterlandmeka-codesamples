using Cinemachine;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MM_AutomatorController : MonoBehaviour {

	private Dictionary<AutomatorType, Action> _pendingActions;

	private void Awake() {
		_pendingActions = new Dictionary<AutomatorType, Action>();
	}

	private void OnEnable() {
		MM_Events.OnCameraReturnedToPlayer += _cameraReturnToPlayerComplete;
		MM_Events.OnCameraMovedToPosition += _cameraMoveToPositionComplete;
		MM_Events.OnStoryNotificationsCompleted += _storyNotificationsCompleted;
	}

	private void OnDisable() {
		MM_Events.OnCameraReturnedToPlayer -= _cameraReturnToPlayerComplete;
		MM_Events.OnCameraMovedToPosition -= _cameraMoveToPositionComplete;
		MM_Events.OnStoryNotificationsCompleted -= _storyNotificationsCompleted;
	}

	public bool ActionAutomator(MM_Automator automator) {
		bool continuePerformingAction = true;
		if (!automator.runOnce || !MM.player.ActionedAutomators.Contains(automator.automator)) {
			Debug.Log("MM_AutomatorController:ActionAutomator : actioning automator [" + automator.automator + "]");
			if (automator.runOnce) {
				MM.player.SetAutomatorActioned(automator.automator);
			}
			// Run any code that needs to be run when the automator is initially triggered
			if (automator.onBeforeStart != null) {
				continuePerformingAction = automator.onBeforeStart();
			}
			if (continuePerformingAction && automator.type != null) {
				// Perform any custom onStart actions required for specific automators
				switch (automator.automator) {
					default:
						// If nothing has been done so far then try to action the automator by its type rather than id
						ActionAutomatorType(automator.type, automator);
						break;
				}
			}
		}
		return continuePerformingAction;
	}

	public void ActionAutomatorType(AutomatorType? type, MM_Automator? automatorNullable = null) {
		bool completed = false;
		// Perform actions by automator type
		Debug.Log("MM_AutomatorController:ActionAutomatorType : actioning automator type [" + type + "]");
		switch (type) {
			case AutomatorType.CAMERA_RETURN_TO_PLAYER:
			case AutomatorType.CAMERA_MOVE_TO_POSITION:
				if (automatorNullable != null) {
					MM_Automator automatorCameraMovement = (MM_Automator)automatorNullable;
					if (automatorCameraMovement.cameraPosition != null) {
						_pendingActions.Add((AutomatorType)type, delegate() {
							if (automatorCameraMovement.onCompleteActionType != null) {
								ActionAutomatorType(automatorCameraMovement.onCompleteActionType);
							}
							if (automatorCameraMovement.onCompleteActionAutomator != null && MM_Constants_Automators.Automators.ContainsKey((Automator)automatorCameraMovement.onCompleteActionAutomator)) {
								ActionAutomator(MM_Constants_Automators.Automators[(Automator)automatorCameraMovement.onCompleteActionAutomator]);
							}
						});
						if (type == AutomatorType.CAMERA_RETURN_TO_PLAYER) {
							MM.cameraController.SetCameraReturnToPlayer(automatorCameraMovement.cameraPosition, automatorCameraMovement.cameraMovementDelay, automatorCameraMovement.cameraMovementTime);
						} else {
							MM.cameraController.SetCameraMoveToPosition(automatorCameraMovement.cameraPosition, automatorCameraMovement.cameraMovementDelay, automatorCameraMovement.cameraMovementTime);
						}
					}
				}
				break;
			case AutomatorType.RETURN_INPUT_TO_PLAYER_ENABLE_HUD:
				MM.hud.SetFadeCanvasColor(Color.black);
				MM.hud.Show(HUDFadeType.NORMAL);
				MM.Player.MovementState.ChangeState(Engine.CharacterStates.MovementStates.Idle);
				MM.events.Trigger(MM_Event.PLAYER_READY);
				completed = true;
				break;
			case AutomatorType.RETURN_INPUT_TO_PLAYER:
				MM.events.Trigger(MM_Event.PLAYER_READY);
				completed = true;
				break;
			case AutomatorType.CLEAR_AUTOMATOR:
				completed = true;
				break;
			case AutomatorType.TRIGGER_LOCATION_TRANSITION_CONTINUE:
				MM.SceneController.TriggerLocationTransitionContinue();
				break;
		}
		// If the automator has completed then turn it off
		if (completed) {
			MM.player.SetAutomator(null);
		}
	}

	public void AddPendingAction(AutomatorType type, Action action) {
		_pendingActions.Add(type, action);
	}

	// Handle any pending actions that are triggered by events

	private void _cameraReturnToPlayerComplete(MM_EventData data) {
		if (_pendingActions.ContainsKey(AutomatorType.CAMERA_RETURN_TO_PLAYER)) {
			_pendingActions[AutomatorType.CAMERA_RETURN_TO_PLAYER]();
			_pendingActions.Remove(AutomatorType.CAMERA_RETURN_TO_PLAYER);
		}
	}

	private void _cameraMoveToPositionComplete(MM_EventData data) {
		if (_pendingActions.ContainsKey(AutomatorType.CAMERA_MOVE_TO_POSITION)) {
			_pendingActions[AutomatorType.CAMERA_MOVE_TO_POSITION]();
			_pendingActions.Remove(AutomatorType.CAMERA_MOVE_TO_POSITION);
		}
	}

	private void _storyNotificationsCompleted() {
		if (_pendingActions.ContainsKey(AutomatorType.CAMERA_PANNING)) {
			_pendingActions[AutomatorType.CAMERA_PANNING]();
			_pendingActions.Remove(AutomatorType.CAMERA_PANNING);
		}
	}
}
