using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

[RequireComponent (typeof (BoxCollider2D))]
public class MM_Objects_LocationTransition : MM_SceneObject {

	private string _ANIMATOR = "Animator";
	private string _ANIMATION_IDLE = "idle";
	private string _ANIMATION_OPEN = "open";
	private string _ANIMATION_CLOSE = "close";

	public bool triggerOnUp;
	public bool triggerOnDown;
	public bool triggerNeedsGrounded;
	public bool hideAfterUse = false;
	public GameObject destination;
	public float xOffset = 0f;
	public float yOffset = 0f;
	public MM_SFX? openSoundEffect = MM_SFX.DOOR1_OPEN;
	public MM_SFX? closeSoundEffect = MM_SFX.DOOR1_CLOSE;

	private int _id;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private bool _isTriggerable;
	private bool _animationDirectionForward;
	private bool _isTransitionSource;
	private bool _destinationAnimationAllowed;
	private SceneAreaLayer _layer;
	private Vector2 _playerOffset;
	private float _PLAYER_MOVE_INWARD_CENTER_TIME = 0.5f;

	private void Start() {
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
		_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
		_destinationAnimationAllowed = false;
		_updateLayer();
	}

	private void OnEnable() {
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnLocationTransitionPlayerMoved += _locationTransitionPlayerMoved;
		MM_Events.OnLocationTransitionCameraMoved += _locationTransitionCameraMoved;
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
		_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
		_destinationAnimationAllowed = false;
		_updateLayer();
	}

	private void OnDisable() {
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnLocationTransitionPlayerMoved -= _locationTransitionPlayerMoved;
		MM_Events.OnLocationTransitionCameraMoved -= _locationTransitionCameraMoved;
	}

	public override void ActivateSceneObject() {
		base.ActivateSceneObject();
	}

	public override void DeactivateSceneObject() {
		base.DeactivateSceneObject();
	}

	public void ShowDoor() {
		_spriteRenderer.enabled = true;
		SetLayer(true);
	}

	public SceneAreaLayer GetLayer() {
		return _layer;
	}

	public void SetLayer(bool front) {
		switch (LayerMask.LayerToName(_spriteRenderer.gameObject.layer)) {
			case MM_Constants.LAYER_1_BACK:
			case MM_Constants.LAYER_1_FRONT:
				if (front) {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
				} else {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
				}
				break;
			case MM_Constants.LAYER_2_BACK:
			case MM_Constants.LAYER_2_FRONT:
				if (front) {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
				} else {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
				}
				break;
			case MM_Constants.LAYER_3_BACK:
			case MM_Constants.LAYER_3_FRONT:
				if (front) {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
				} else {
					_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
				}
				break;
		}
	}

	public void StartAnimation_atStartFrame() {
		// Only process the animation event when animation is moving backward and has arrived at the first frame
		if (!_animationDirectionForward) {
			_closeAnimationComplete();
		}
	}

	public void StartAnimation_atEndFrame() {
		// Only process the animation event when animation is moving forward and has arrived at the last frame
		if (_animationDirectionForward) {
			_openAnimationComplete();
		}
	}

	private void OnTriggerEnter2D(Collider2D collider) {
		if (MM_Helper.IsPlayerInteractColliding(collider)) {
			_isTriggerable = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collider) {
		if (MM_Helper.IsPlayerInteractColliding(collider)) {
			_isTriggerable = false;
		}
	}

	private void OnTriggerStay2D(Collider2D collider) {
		if (MM_Helper.IsPlayerInteractColliding(collider)) {
			_isTriggerable = true;
		}
	}

	private void _updateLayer() {
		switch (LayerMask.LayerToName(gameObject.layer)) {
			case MM_Constants.LAYER_1_BACK:
			case MM_Constants.LAYER_1_FRONT:
				_layer = SceneAreaLayer.LAYER_1;
				break;
			case MM_Constants.LAYER_2_BACK:
			case MM_Constants.LAYER_2_FRONT:
				_layer = SceneAreaLayer.LAYER_2;
				break;
			case MM_Constants.LAYER_3_BACK:
			case MM_Constants.LAYER_3_FRONT:
				_layer = SceneAreaLayer.LAYER_3;
				break;
		}
		// Move the object to the correct layer
		SetLayer(false);
	}

	private void _inputReceived(MM_Input input) {
		if (MM.Player == null) {
			return;
		}
		if (_isTriggerable && MM.player.CurrentSceneAreaLayer == _layer && destination != null && MM.playerController.IsInputAvailable_game()) {
			if (!triggerNeedsGrounded || MM.Player._controller.State.IsGrounded) {
				// Trigger the location transition if the player presses input matching the specified triggering event
				if ((input == MM_Input.UP && triggerOnUp) || (input == MM_Input.DOWN && triggerOnDown)) {
					_playerOffset = new Vector2(transform.position.x - MM.Player.transform.position.x, transform.position.y - MM.Player.transform.position.y);
					_triggerLocationTransitionStart(input == MM_Input.UP);
				}
			}
		}
	}

	private void _locationTransitionPlayerMoved(MM_EventData data) {
		if (data != null) {
			if (data.destinationId == _id) {
				// This object is the destination of a transition
				_destinationAnimationAllowed = true;
			} else if (data.sourceId == _id) {
				// This object is the source of a transition
				// Now that the player has moved the source object layer can be reset to behind the player
				SetLayer(false);
			}
		}
	}

	private void _locationTransitionCameraMoved() {
		// Wait until the player has moved before acknowledging any camera moved events
		if (_destinationAnimationAllowed) {
			// Play the open animation: _startAnimation_atEndFrame will be called when the animation has completed
			_destinationAnimationAllowed = false;
			_isTransitionSource = false;
			_animationDirectionForward = true;
			_spriteRenderer.enabled = true;
			_animator.Play(_ANIMATION_OPEN);
			if (openSoundEffect != null) {
				MM.soundEffects.Play((MM_SFX)openSoundEffect);
			}
		}
	}

	private void _triggerLocationTransitionStart(bool playerLookAway) {
		// Start a location transition
		_isTransitionSource = true;
		MM.events.Trigger(MM_Event.LOCATION_TRANSITION_START, new MM_EventData {
			transition_lookAway = playerLookAway
		});
		// Play the open animation: _startAnimation_atEndFrame will be called when the animation has completed
		_animationDirectionForward = true;
		_spriteRenderer.enabled = true;
		_animator.Play(_ANIMATION_OPEN);
		if (openSoundEffect != null) {
			MM.soundEffects.Play((MM_SFX)openSoundEffect);
		}
	}

	private void _openAnimationComplete() {
		if (_isTransitionSource) {
			// The open animation has completed, so now move this object and the destination object in front of the player
			SetLayer(true);
			destination.GetComponent<MM_Objects_LocationTransition>().ShowDoor();
			// Trigger the event that will cause the player to walk through this object
			MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData { transition_lookAway = true });
			iTween.MoveBy(MM.Player.gameObject, _playerOffset, _PLAYER_MOVE_INWARD_CENTER_TIME);
			// Play the close animation: _startAnimation_atStartFrame will be called when the animation has completed
			_animationDirectionForward = false;
			_animator.Play(_ANIMATION_CLOSE);
		} else {
			// The open animation has completed, so now move this object into the layer behind the player
			SetLayer(false);
			// Trigger the event that will cause the player to walk through this object
			MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData { transition_lookAway = false });
			// Play the close animation: _startAnimation_atStartFrame will be called when the animation has completed
			_animationDirectionForward = false;
			_animator.Play(_ANIMATION_CLOSE);
		}
		if (closeSoundEffect != null) {
			MM.soundEffects.Play((MM_SFX)closeSoundEffect);
		}
	}

	private void _closeAnimationComplete() {
		_animator.Play(_ANIMATION_IDLE);
		if (_isTransitionSource) {
			// Trigger the event indicating the location transition has successfully started
			Vector2 position = new Vector2(destination.transform.localPosition.x + xOffset, destination.transform.localPosition.y + yOffset);
			MM.events.Trigger(MM_Event.LOCATION_TRANSITION_STARTED, new MM_EventData {
				transition_position = position,
				sourceId = _id,
				destinationId = destination.gameObject.GetInstanceID()
			});
		} else {
			// Trigger the event indicating the location transition has successfully been completed
			MM.events.Trigger(MM_Event.LOCATION_TRANSITION_FINISHED);
		}
		if (hideAfterUse) {
			_spriteRenderer.enabled = false;
		}
	}
}
