using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MM_Input {
	UP,
	DOWN,
	LEFT,
	RIGHT,
	JUMP,
	ATTACK,
	ABILITY,
	INVENTORY,
	MENU,
	MAP,
	EXIT,
	PREVIOUS,
	PREVIOUS_2,
	NEXT,
	NEXT_2
}

public enum InputDirection {
	NONE,
	UP,
	DOWN,
	LEFT,
	RIGHT
}

public class MM_Input_Reader : MonoBehaviour {

	public Player player;
	private bool isMovingPlayer = false;
	private bool isXAxisInUse = false;
	private bool isYAxisInUse = false;
	private const float _AXIS_THRESHOLD = 0.5f;

	public bool IsMovingPlayer {
		get {
			return isMovingPlayer;
		}
		set {
			isMovingPlayer = value;
		}
	}

	void Awake() {
		player = ReInput.players.GetPlayer(0);
	}

	void Update() {
		if (!MM.IsAllInputLocked) {
			// Do not allow input if control of the player isn't currently allowed
			if (MM.EngineReady && !MM.playerController.IsPlayerControlAvailable()) {
				return;
			}

			bool isUpPressed = player.GetAxis("MoveVertical") > _AXIS_THRESHOLD;
			bool isDownPressed = player.GetAxis("MoveVertical") < -_AXIS_THRESHOLD;
			bool isLeftPressed = player.GetAxis("MoveHorizontal") < -_AXIS_THRESHOLD;
			bool isRightPressed = player.GetAxis("MoveHorizontal") > _AXIS_THRESHOLD;
			bool isJumpPressed = player.GetButtonDown("Jump");
			bool isAttackPressed = player.GetButtonDown("Attack");
			bool isAbilityPressed = player.GetButtonDown("Ability");
			bool isInventoryPressed = player.GetButtonDown("Inventory");
			bool isMenuPressed = player.GetButtonDown("Menu");
			bool isMapPressed = player.GetButtonDown("Map");
			bool isPreviousPressed = player.GetButtonDown("Previous");
			bool isPrevious2Pressed = player.GetButtonDown("Previous2");
			bool isNextPressed = player.GetButtonDown("Next");
			bool isNext2Pressed = player.GetButtonDown("Next2");

			if (isUpPressed) {
				if (IsMovingPlayer) {
				} else {
					if (!isYAxisInUse) {
						isYAxisInUse = true;
						MM.events.Trigger(MM_Event.INPUT_UP_PRESSED);
					}
				}
			}
			if (isDownPressed) {
				if (IsMovingPlayer) {
				} else {
					if (!isYAxisInUse) {
						isYAxisInUse = true;
						MM.events.Trigger(MM_Event.INPUT_DOWN_PRESSED);
					}
				}
			}
			if (isLeftPressed) {
				if (IsMovingPlayer) {
				} else {
					if (!isXAxisInUse) {
						isXAxisInUse = true;
						MM.events.Trigger(MM_Event.INPUT_LEFT_PRESSED);
					}
				}
			}
			if (isRightPressed) {
				if (IsMovingPlayer) {
				} else {
					if (!isXAxisInUse) {
						isXAxisInUse = true;
						MM.events.Trigger(MM_Event.INPUT_RIGHT_PRESSED);
					}
				}
			}
			if (isJumpPressed) {
				MM.events.Trigger(MM_Event.INPUT_JUMP_PRESSED);
			}
			if (isAttackPressed) {
				MM.events.Trigger(MM_Event.INPUT_ATTACK_PRESSED);
			}
			if (isAbilityPressed) {
				MM.events.Trigger(MM_Event.INPUT_ABILITY_PRESSED);
			}
			if (isInventoryPressed) {
				MM.events.Trigger(MM_Event.INPUT_INVENTORY_PRESSED);
			}
			if (isMenuPressed) {
				MM.events.Trigger(MM_Event.INPUT_MENU_PRESSED);
			}
			if (isMapPressed) {
				MM.events.Trigger(MM_Event.INPUT_MAP_PRESSED);
			}
			if (isPreviousPressed) {
				MM.events.Trigger(MM_Event.INPUT_PREVIOUS_PRESSED);
			}
			if (isPrevious2Pressed) {
				MM.events.Trigger(MM_Event.INPUT_PREVIOUS_2_PRESSED);
			}
			if (isNextPressed) {
				MM.events.Trigger(MM_Event.INPUT_NEXT_PRESSED);
			}
			if (isNext2Pressed) {
				MM.events.Trigger(MM_Event.INPUT_NEXT_2_PRESSED);
			}

			if (player.GetAxis("MoveVertical") == 0) {
				isYAxisInUse = false;
			}
			if (player.GetAxis("MoveHorizontal") == 0) {
				isXAxisInUse = false;
			}
		}
	}

	public bool IsNoButtonPressed() {
		return (
			!player.GetButtonDown("Jump") &&
			!player.GetButtonDown("Attack") &&
			!player.GetButtonDown("Ability") &&
			!player.GetButtonDown("Inventory") &&
			!player.GetButtonDown("Menu") &&
			!player.GetButtonDown("Map") &&
			!player.GetButtonDown("Previous") &&
			!player.GetButtonDown("Previous2") &&
			!player.GetButtonDown("Next") &&
			!player.GetButtonDown("Next2")
		);
	}

	public bool IsButtonHeld(MM_Input input) {
		bool held = false;
		switch (input) {
			case MM_Input.UP:
				held = player.GetAxis("MoveVertical") > 0;
				break;
			case MM_Input.DOWN:
				held = player.GetAxis("MoveVertical") < 0;
				break;
			case MM_Input.LEFT:
				held = player.GetAxis("MoveHorizontal") < 0;
				break;
			case MM_Input.RIGHT:
				held = player.GetAxis("MoveHorizontal") > 0;
				break;
			case MM_Input.JUMP:
				held = player.GetButton("Jump");
				break;
			case MM_Input.ATTACK:
				held = player.GetButton("Attack");
				break;
			case MM_Input.ABILITY:
				held = player.GetButton("Ability");
				break;
			case MM_Input.INVENTORY:
				held = player.GetButton("Inventory");
				break;
			case MM_Input.MENU:
				held = player.GetButton("Menu");
				break;
			case MM_Input.MAP:
				held = player.GetButton("Map");
				break;
			case MM_Input.PREVIOUS:
				held = player.GetButton("Previous");
				break;
			case MM_Input.PREVIOUS_2:
				held = player.GetButton("Previous2");
				break;
			case MM_Input.NEXT:
				held = player.GetButton("Next");
				break;
			case MM_Input.NEXT_2:
				held = player.GetButton("Next2");
				break;
		}
		return held;
	}
}
