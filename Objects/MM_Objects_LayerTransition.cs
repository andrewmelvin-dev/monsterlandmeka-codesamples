using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class MM_Objects_LayerTransition : MM_SceneObject {

	public bool triggerOnUp;
	public bool triggerOnDown;
	public bool triggerNeedsGrounded;
	public GameObject destination;
	public float xOffset = 0f;
	public float yOffset = 0.3f;
	public float cameraDistance = 0f;
	public MM_SFX openSoundEffect = MM_SFX.DOOR1_OPEN;
	public MM_SFX closeSoundEffect = MM_SFX.DOOR1_CLOSE;
	public MusicalTrack startMusicalTrack = MusicalTrack.NOTHING;
	public bool restoreMusicalTrack = false;
	public float environmentSFXMultiplier = 1f;

	private int _id;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	MM_Objects_LayerTransition _destinationLayerTransition;
	private bool _isTriggerable;
	private bool _animationDirectionForward;
	private bool _isTransitionSource;
	//private bool _destinationAnimationAllowed;
	private SceneAreaLayer _layer;
	private Vector2 _playerOffset;

	private string _ANIMATOR = "Animator";
	private string _ANIMATION_IDLE = "idle";
	private string _ANIMATION_OPEN = "open";
	private string _ANIMATION_CLOSE = "close";
	private string _ANIMATION_HIDE = "hide";
	private float _PLAYER_MOVE_INWARD_CENTER_TIME = 0.5f;
	private float _PLAYER_MOVE_OUTWARD_CENTER_TIME = 1f;

	private void Start() {
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
		_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
		_destinationLayerTransition = destination.GetComponent<MM_Objects_LayerTransition>();
		_updateLayer();
	}

	private void OnEnable() {
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnLayerTransitionPlayerMoved += _layerTransitionPlayerMoved;
		MM_Events.OnLayerResizeFinished += _layerResizeFinished;
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
		_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
		_destinationLayerTransition = destination.GetComponent<MM_Objects_LayerTransition>();
		_updateLayer();
	}

	private void OnDisable() {
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnLayerTransitionPlayerMoved -= _layerTransitionPlayerMoved;
		MM_Events.OnLayerResizeFinished -= _layerResizeFinished;
	}

	public override void ActivateProximityManaged() {
		base.ActivateProximityManaged();
	}

	public override void ActivateSceneObject() {
		base.ActivateSceneObject();
	}

	public override void DeactivateProximityManaged() {
		base.DeactivateProximityManaged();
	}

	public override void DeactivateSceneObject() {
		base.DeactivateSceneObject();
	}

	public SceneAreaLayer GetLayer() {
		return _layer;
	}

	public void SetLayer(bool front) {

		// If the destination layer is closer to the camera then this object should appear in front of the player all the time
		if ((int)_destinationLayerTransition.GetLayer() < (int)_layer) {
			front = true;
		}

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

	public void Hide() {
		_animator.Play(_ANIMATION_HIDE);
	}

	public void Show() {
		_animator.Play(_ANIMATION_IDLE);
	}

	public void SetTransitionSource(bool isSource) {
		_isTransitionSource = isSource;
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
				// Trigger the layer transition if the player presses input matching the specified triggering event
				if ((input == MM_Input.UP && triggerOnUp) || (input == MM_Input.DOWN && triggerOnDown)) {
					_playerOffset = new Vector2(transform.position.x - MM.Player.transform.position.x, transform.position.y - MM.Player.transform.position.y);
					_triggerLayerTransitionStart(input == MM_Input.UP);
				}
			}
		}
	}

	private void _layerTransitionPlayerMoved(MM_EventData data) {
		if (data != null) {
			if (data.destinationId == _id) {
				// This object is the destination of a transition
				//_destinationAnimationAllowed = true;
				_isTransitionSource = false;

				if ((int)_layer < (int)_destinationLayerTransition.GetLayer()) {
					// If moving from a layer further from the camera to one that is closer then play the door open animation
					// Hide the source so it won't appear when the door opens
					_destinationLayerTransition.Hide();
					_animationDirectionForward = true;
					_animator.Play(_ANIMATION_OPEN);
					if (openSoundEffect != MM_SFX.NOTHING) {
						MM.soundEffects.Play((MM_SFX)openSoundEffect);
					}
					// Check if the music needs to be restored
					if (restoreMusicalTrack) {
						MM.SceneController.RestartMusicFromRestorationPoint();
					}
				} else {
					// The player has moved to a layer further from the camera, so no animation is needed
					// Trigger the event indicating the layer transition has successfully been completed
					MM.events.Trigger(MM_Event.LAYER_TRANSITION_FINISHED);
				}

			} else if (data.sourceId == _id) {
				// This object is the source of a transition
				// Do nothing here: the object needs to remain on the same layer so it disappears when the layer is resized
			}
		}
	}

	private void _triggerLayerTransitionStart(bool playerLookAway) {
		// Start a layer transition
		SetTransitionSource(true);
		_destinationLayerTransition.SetTransitionSource(false);
		if (!playerLookAway) {
			// If moving to a layer closer to the camera then don't perform the door animation on this object
			// Ensure the destination object is in front of the player first
			_destinationLayerTransition.SetLayer(true);
			// Trigger the layer conceal event
			Vector2 position = new Vector2(destination.transform.localPosition.x + xOffset, destination.transform.localPosition.y + yOffset);
			MM.events.Trigger(MM_Event.LAYER_CONCEAL, new MM_EventData {
				transition_position = position,
				sourceId = _id,
				destinationId = destination.gameObject.GetInstanceID(),
				sceneAreaLayer = _layer
			});
			// Centre the player on the door but do not adjust the vertical position
			_playerOffset.y = 0;
			iTween.MoveBy(MM.Player.gameObject, _playerOffset, _PLAYER_MOVE_OUTWARD_CENTER_TIME);
			// Check if the music needs to fade out
			if (restoreMusicalTrack) {
				MM.music.Pause();
			}
			// Check if the camera needs to zoom in/out
			if (cameraDistance != 0f) {
				MM.cameraController.SetCameraDistance(cameraDistance, MM_Constants.CAMERA_MOVING_DAMPING_Z);
			}
			// Set the volume of environmental sound effects
			if (MM.soundEffects.IsEnvironmentPlaying()) {
				MM.soundEffects.SetEnvironmentSFXMultiplier(environmentSFXMultiplier);
			}
		} else {
			// Hide the destination so it won't appear when the door opens
			_destinationLayerTransition.Hide();
			// Play the open animation: StartAnimation_atEndFrame will be called when the animation has completed
			_animationDirectionForward = true;
			_animator.Play(_ANIMATION_OPEN);
			if (openSoundEffect != MM_SFX.NOTHING) {
				MM.soundEffects.Play((MM_SFX)openSoundEffect);
			}
			// Check if the music needs to fade out
			if (startMusicalTrack != MusicalTrack.NOTHING) {
				MM.music.Pause();
			}
			// Set the volume of environmental sound effects
			if (MM.soundEffects.IsEnvironmentPlaying()) {
				MM.soundEffects.SetEnvironmentSFXMultiplier(environmentSFXMultiplier);
			}
		}
		MM.events.Trigger(MM_Event.LAYER_TRANSITION_START, new MM_EventData {
			transition_lookAway = playerLookAway
		});
	}

	private void _openAnimationComplete() {
		if (_isTransitionSource) {
			// The open animation has completed so now move this object in front of the player
			SetLayer(true);
			_destinationLayerTransition.SetLayer(true);
			// Trigger the event that will cause the player to walk through this object
			MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData {
				transition_lookAway = true
			});
			iTween.MoveBy(MM.Player.gameObject, _playerOffset, _PLAYER_MOVE_INWARD_CENTER_TIME);
			// Play the close animation: StartAnimation_atStartFrame will be called when the animation has completed
			_animationDirectionForward = false;
			_animator.Play(_ANIMATION_CLOSE);
		} else {
			// The open animation has completed, so now move this object into the layer behind the player
			SetLayer(false);
			// Trigger the event that will cause the player to walk through this object
			MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData {
				transition_lookAway = false
			});
			// Play the close animation: StartAnimation_atStartFrame will be called when the animation has completed
			_animationDirectionForward = false;
			_animator.Play(_ANIMATION_CLOSE);
		}
		if (closeSoundEffect != MM_SFX.NOTHING) {
			MM.soundEffects.Play((MM_SFX)closeSoundEffect);
		}
	}

	private void _closeAnimationComplete() {
		_animator.Play(_ANIMATION_IDLE);
		// Restore visibility of the destination door
		_destinationLayerTransition.Show();
		if (_isTransitionSource) {
			// Trigger the event indicating the layer transition has started
			Vector2 position = new Vector2(destination.transform.localPosition.x + xOffset, destination.transform.localPosition.y + yOffset);
			MM.events.Trigger(MM_Event.LAYER_TRANSITION_STARTED, new MM_EventData {
				transition_position = position,
				sourceId = _id,
				destinationId = destination.gameObject.GetInstanceID(),
				sceneAreaLayer = (SceneAreaLayer)((int)_layer + 1)
			});
			// Check if the music needs to change
			if (startMusicalTrack != MusicalTrack.NOTHING) {
				MM.SceneController.SetMusicRestorationPoint();
				MM.SceneController.StartMusicFromPosition(startMusicalTrack, null, 0f, true);
			}
			// Check if the camera needs to zoom in/out
			if (cameraDistance != 0f) {
				MM.cameraController.SetCameraDistance(cameraDistance, MM_Constants.CAMERA_MOVING_DAMPING_Z);
			}
		} else {
			// Trigger the event indicating the layer transition has successfully been completed
			MM.events.Trigger(MM_Event.LAYER_TRANSITION_FINISHED);
		}
	}

	private void _layerResizeFinished() {
		if (_id == MM.layerController.GetMovingToDestinationId() && MM.layerController.GetMovingToShouldStartTransition()) {
			MM.events.Trigger(MM_Event.LAYER_TRANSITION_STARTED, new MM_EventData {
				transition_position = MM.layerController.GetMovingToPosition(),
				sourceId = MM.layerController.GetMovingFromSourceId(),
				destinationId = MM.layerController.GetMovingToDestinationId(),
				sceneAreaLayer = MM.layerController.GetMovingToLayer()
			});
		}
	}
}
