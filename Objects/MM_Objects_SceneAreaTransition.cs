using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class MM_Objects_SceneAreaTransition : MM_SceneObject {

	public PersistentObject persistentObject = PersistentObject.NOTHING;
	public bool triggerOnUp;
	public bool triggerOnDown;
	public bool triggerOnEnter;
	public bool triggerNeedsGrounded;
	public bool updateLayer = true;
	public GameObject connectedObjectLayer;
	public SceneArea destinationArea;
	public string destinationObjectName;
	public int destinationAreaEntrance;
	public Automator destinationAutomator = Automator.NONE;
	public bool maintainSpeed = true;
	public bool playerOffsetX = true;
	public bool playerOffsetY = true;
	public float xOffset = 0f;
	public float yOffset = 0f;
	public MM_SFX openSoundEffect = MM_SFX.DOOR1_OPEN;
	public MM_SFX closeSoundEffect = MM_SFX.DOOR1_CLOSE;
	public TransitionDisplayType transitionType = TransitionDisplayType.OPEN_WALK_CLOSE;

	private PersistentObjectState _state;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private SpriteRenderer _connectedSpriteRenderer;
	private bool _isTriggerable;
	private bool _animationDirectionForward;
	private bool _isTransitionSource;
	private SceneAreaLayer _layer;
	private bool _resetLayerOnEnable;
	private bool _awaitingPlayerAnimationFinished;
	private bool _isInitialised = false;
	private Vector2 _playerOffset;

	private string _ANIMATOR = "Animator";
	private string _ANIMATION_IDLE = "idle";
	private string _ANIMATION_OPEN = "open";
	private string _ANIMATION_OPENED = "opened";
	private string _ANIMATION_CLOSE = "close";
	private string _ANIMATION_INACTIVE = "inactive";
	private const float _DELAY_CLOSE_SFX_TIME = 0.25f;
	private float _PLAYER_MOVE_INWARD_CENTER_TIME = 0.5f;

	private void Start() {
		if (transform.Find(_ANIMATOR) != null) {
			_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
			_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
			if (connectedObjectLayer != null) {
				_connectedSpriteRenderer = connectedObjectLayer.GetComponent<SpriteRenderer>();
			}
		}
		if (persistentObject != PersistentObject.NOTHING && !_isInitialised) {
			_updateState();
		}
		_updateLayer();
		_updateAppearance();
	}

	private void OnValidate() {
		if (destinationAreaEntrance < 0) {
			destinationAreaEntrance = 0;
		}
	}

	private void OnEnable() {
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnLocationTransitionContinue += _locationTransitionContinue;
		MM_Events.OnPlayerAnimationFinished += _playerAnimationFinished;
		if (transform.Find(_ANIMATOR) != null) {
			_animator = transform.Find(_ANIMATOR).GetComponent<Animator>();
			_spriteRenderer = transform.Find(_ANIMATOR).GetComponent<SpriteRenderer>();
			if (connectedObjectLayer != null) {
				_connectedSpriteRenderer = connectedObjectLayer.GetComponent<SpriteRenderer>();
			}
		}
		if (persistentObject != PersistentObject.NOTHING) {
			_updateState();
		}
		_updateLayer();
		_updateAppearance();
	}

	private void OnDisable() {
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnLocationTransitionContinue -= _locationTransitionContinue;
		MM_Events.OnPlayerAnimationFinished -= _playerAnimationFinished;
		if (persistentObject != PersistentObject.NOTHING && MM.player.ObjectData.ContainsKey(persistentObject) && MM.player.ObjectData[persistentObject].state == PersistentObjectState.DEACTIVATING) {
			// If the door is in the process of deactivating then set it as inactive now
			MM.player.SetPersistentObjectState(persistentObject, PersistentObjectState.INACTIVE);
		}
		// Prior to disabling the object make sure the layer is reset to be behind the player
		SetLayer(false);
	}

	public override void ActivateSceneObject() {
		base.ActivateSceneObject();
	}

	public override void DeactivateSceneObject() {
		base.DeactivateSceneObject();
	}

	public int GetId() {
		return transform.gameObject.GetInstanceID();
	}

	public SceneAreaLayer GetLayer() {
		return _layer;
	}

	public void SetLayer(bool front) {
		if (_spriteRenderer != null) {
			switch (LayerMask.LayerToName(_spriteRenderer.gameObject.layer)) {
				case MM_Constants.LAYER_1_BACK:
				case MM_Constants.LAYER_1_FRONT:
					if (front) {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
							}
						}
					} else {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
							}
						}
					}
					break;
				case MM_Constants.LAYER_2_BACK:
				case MM_Constants.LAYER_2_FRONT:
					if (front) {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
							}
						}
					} else {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
							}
						}
					}
					break;
				case MM_Constants.LAYER_3_BACK:
				case MM_Constants.LAYER_3_FRONT:
					if (front) {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
							}
						}
					} else {
						_spriteRenderer.gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
						_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
						if (connectedObjectLayer != null) {
							connectedObjectLayer.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
							if (_connectedSpriteRenderer != null) {
								_connectedSpriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
							}
						}
					}
					break;
			}
		}
	}

	public void SetState(PersistentObjectState state, float initialDelay = 0f, bool processUpdate = false) {
		if (persistentObject != PersistentObject.NOTHING) {
			if (initialDelay == 0f) {
				_state = state;
				MM.player.SetPersistentObjectState(persistentObject, state);
				if (processUpdate) {
					_updateState();
				}
			} else {
				StartCoroutine(_delaySetState(state, initialDelay, processUpdate));
			}
		}
	}

	public void SetProperty(PersistentObjectProperty property, int value, float initialDelay = 0f, bool processUpdate = false) {
		if (persistentObject != PersistentObject.NOTHING) {
			if (initialDelay == 0f) {
				MM.player.SetPersistentObjectProperty(persistentObject, property, value);
				if (processUpdate) {
					_updateState();
				}
			} else {
				StartCoroutine(_delaySetProperty(property, value, initialDelay, processUpdate));
			}
		}
	}

	public void OpenDoor(bool playerLookAway = true, float initialDelay = 0f) {
		if (initialDelay == 0f) {
			_triggerLocationTransitionStart(playerLookAway);
		} else {
			StartCoroutine(_delayOpenDoor(playerLookAway, initialDelay));
		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (persistentObject == PersistentObject.NOTHING || _state == PersistentObjectState.READY) {
			if (MM_Helper.IsPlayerColliding(collider) && triggerOnEnter) {
				MM.events.Trigger(MM_Event.SCENE_AREA_TRANSITION, new MM_EventData {
					automator = destinationAutomator,
					objectName = destinationObjectName,
					sceneAreaTransition_destinationArea = destinationArea,
					sceneAreaTransition_destinationAreaEntrance = destinationAreaEntrance,
					sceneAreaTransition_spawn = false,
					sceneAreaTransition_offset = new Vector2(xOffset, yOffset),
					sceneAreaTransition_offsetX = playerOffsetX,
					sceneAreaTransition_offsetY = playerOffsetY,
					sceneAreaTransition_speed = maintainSpeed ? MM.Player._controller.Speed : new Vector2(0f, 0f)
				});
			} else if (MM_Helper.IsPlayerInteractColliding(collider)) {
				_isTriggerable = true;
			}
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

	public void StartAnimation_atStartFrame() {
		// Only process the animation event when animation is moving backward and has arrived at the first frame
		if (_animationDirectionForward == false) {
			_closeAnimationComplete();
		}
	}

	public void StartAnimation_atEndFrame() {
		// Only process the animation event when animation is moving forward and has arrived at the last frame
		if (_animationDirectionForward == true) {
			_openAnimationComplete();
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
		if (_resetLayerOnEnable) {
			// Move the object to the correct layer
			SetLayer(false);
		}
	}

	private void _updateState() {
		_awaitingPlayerAnimationFinished = false;
		_isInitialised = true;

		PersistentObjectState state = PersistentObjectState.INACTIVE;
		if (MM.player.ObjectData.ContainsKey(persistentObject)) {
			state = MM.player.ObjectData[persistentObject].state;
			_processPersistentObjectProperties(MM.player.ObjectData[persistentObject].properties);
		} else if (MM_PersistentObjects.ObjectData.ContainsKey(persistentObject)) {
			state = MM_PersistentObjects.ObjectData[persistentObject].state;
			_processPersistentObjectProperties(MM_PersistentObjects.ObjectData[persistentObject].properties);
		} else {
			_processPersistentObjectProperties(new Dictionary<PersistentObjectProperty, int>());
		}
		_state = state;

		switch (_state) {
			case PersistentObjectState.HIDDEN:
				// Hide the door
				_spriteRenderer.enabled = false;
				break;
			case PersistentObjectState.READY:
			case PersistentObjectState.DEACTIVATING:
				// Show the door
				_spriteRenderer.enabled = true;
				break;
			case PersistentObjectState.INACTIVE:
				// Show the door in the inactive state
				_spriteRenderer.enabled = true;
				if (_animator != null) {
					_animator.Play(_ANIMATION_INACTIVE);
				}
				break;
		}
	}

	private void _updateAppearance() {
		if (transitionType == TransitionDisplayType.WALK_ONLY && _animator != null) {
			_animator.Play(_ANIMATION_OPENED);
		}
	}

	private void _inputReceived(MM_Input input) {
		if (MM.Player == null) {
			return;
		}
		if (_isTriggerable && MM.player.CurrentSceneAreaLayer == _layer && MM.playerController.IsInputAvailable_game() && (persistentObject == PersistentObject.NOTHING || _state == PersistentObjectState.READY)) {
			if (!triggerNeedsGrounded || MM.Player._controller.State.IsGrounded) {
				// Trigger the scene area transition if the player presses input matching the specified triggering event
				if ((input == MM_Input.UP && triggerOnUp) || (input == MM_Input.DOWN && triggerOnDown)) {
					_playerOffset = new Vector2(transform.position.x - MM.Player.transform.position.x, transform.position.y - MM.Player.transform.position.y);
					_triggerLocationTransitionStart(input == MM_Input.UP);
				}
			}
		}
	}

	private void _locationTransitionContinue(MM_EventData data) {
		// Check if the camera moved event is specific to this gameobject
		if (data != null && data.destinationId == GetId()) {
			// Continue the location transition from the previous scene area
			// Move the destination object in front of the player
			if (updateLayer) {
				SetLayer(true);
			}
			// Play the open animation: _startAnimation_atEndFrame will be called when the animation has completed
			_isTransitionSource = false;
			if (transitionType == TransitionDisplayType.WALK_ONLY) {
				_openAnimationComplete();
			} else if (_animator != null) {
				if (transitionType == TransitionDisplayType.OPEN_WALK_CLOSE) {
					_animationDirectionForward = true;
					_animator.Play(_ANIMATION_OPEN);
					MM.soundEffects.Play(openSoundEffect);
				} else if (transitionType == TransitionDisplayType.OPEN_WALK_FADE_OR_WALK_CLOSE) {
					_animationDirectionForward = false;
					_animator.Play(_ANIMATION_CLOSE);
					StartCoroutine(_delayCloseSFX());
				} else if (transitionType == TransitionDisplayType.OPEN_WALK_FADE_OR_OPEN_WALK) {
					_animationDirectionForward = true;
					_animator.Play(_ANIMATION_OPEN);
					MM.soundEffects.Play(openSoundEffect);
				}
			} else if (transitionType == TransitionDisplayType.FORCE_IDLE) {
				_closeAnimationComplete();
			}
		}
	}

	private void _triggerLocationTransitionStart(bool playerLookAway) {
		// Start a location transition
		_isTransitionSource = true;
		MM.events.Trigger(MM_Event.LOCATION_TRANSITION_START, new MM_EventData {
			transition_lookAway = playerLookAway
		});
		if (transitionType == TransitionDisplayType.WALK_ONLY) {
			_openAnimationComplete();
		} else {
			// Play the open animation: _startAnimation_atEndFrame will be called when the animation has completed
			_animationDirectionForward = true;
			if (_animator != null) {
				_animator.Play(_ANIMATION_OPEN);
			}
			MM.soundEffects.Play(openSoundEffect);
		}
	}

	private void _openAnimationComplete() {
		if (_isTransitionSource) {
			if (transitionType == TransitionDisplayType.OPEN_WALK_CLOSE) {
				// The open animation has completed, so now move this object in front of the player
				if (updateLayer) {
					SetLayer(true);
				}
				// Trigger the event that will cause the player to walk through this object
				MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData { transition_lookAway = true });
				iTween.MoveBy(MM.Player.gameObject, _playerOffset, _PLAYER_MOVE_INWARD_CENTER_TIME);
				// Play the close animation: _startAnimation_atStartFrame will be called when the animation has completed
				_animationDirectionForward = false;
				if (_animator != null) {
					_animator.Play(_ANIMATION_CLOSE);
				}
				MM.soundEffects.Play(closeSoundEffect);
			} else if (transitionType == TransitionDisplayType.OPEN_WALK_FADE_OR_WALK_CLOSE || transitionType == TransitionDisplayType.FORCE_IDLE || transitionType == TransitionDisplayType.OPEN_WALK_FADE_OR_OPEN_WALK || transitionType == TransitionDisplayType.WALK_ONLY) {
				// Trigger the event that will cause the player to walk through this object
				MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData { transition_lookAway = true });
				// Trigger the scene area transition immediately
				MM.events.Trigger(MM_Event.SCENE_AREA_TRANSITION, new MM_EventData {
					automator = destinationAutomator,
					objectName = destinationObjectName,
					sceneAreaTransition_destinationArea = destinationArea,
					sceneAreaTransition_destinationAreaEntrance = destinationAreaEntrance,
					sceneAreaTransition_spawn = false,
					sceneAreaTransition_offset = new Vector2(xOffset, yOffset),
					sceneAreaTransition_offsetX = playerOffsetX,
					sceneAreaTransition_offsetY = playerOffsetY
				});
			}
		} else {
			// The open animation has completed, so now move this object into the layer behind the player
			if (updateLayer) {
				SetLayer(false);
			}
			// Trigger the event that will cause the player to walk through this object
			MM.events.Trigger(MM_Event.TRANSITION_PLAYER_ANIMATE, new MM_EventData { transition_lookAway = false });
			if (transitionType == TransitionDisplayType.OPEN_WALK_FADE_OR_OPEN_WALK || transitionType == TransitionDisplayType.WALK_ONLY) {
				_awaitingPlayerAnimationFinished = true;
			} else {
				// Play the close animation: _startAnimation_atStartFrame will be called when the animation has completed
				// The close animation is the open animation in reverse, so ensure the animation is playing backwards
				_animationDirectionForward = false;
				if (_animator != null) {
					_animator.Play(_ANIMATION_CLOSE);
				}
				MM.soundEffects.Play(closeSoundEffect);
			}
		}
	}

	private void _finishTransition() {
		// If there is an automation pending and the trigger is ON_SCENE_AREA_TRANSITION_COMPLETE then start the automation
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator) && MM_Constants_Automators.Automators[MM.player.CurrentAutomator].trigger == AutomatorTrigger.ON_SCENE_AREA_TRANSITION_COMPLETE) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
		}
		// Trigger the event indicating the location transition has successfully completed
		MM.events.Trigger(MM_Event.LOCATION_TRANSITION_FINISHED);
	}

	private void _closeAnimationComplete() {
		if (_animator != null) {
			_animator.Play(_ANIMATION_IDLE);
		}
		if (_isTransitionSource) {
			MM.events.Trigger(MM_Event.SCENE_AREA_TRANSITION, new MM_EventData {
				automator = destinationAutomator,
				objectName = destinationObjectName,
				sceneAreaTransition_destinationArea = destinationArea,
				sceneAreaTransition_destinationAreaEntrance = destinationAreaEntrance,
				sceneAreaTransition_spawn = false,
				sceneAreaTransition_offset = new Vector2(xOffset, yOffset),
				sceneAreaTransition_offsetX = playerOffsetX,
				sceneAreaTransition_offsetY = playerOffsetY
			});
		} else {
			_finishTransition();
		}
	}

	private void _playerAnimationFinished(MM_EventData data) {
		if (_awaitingPlayerAnimationFinished && data.objectName == "WalkForward") {
			// If the walk forward animation has finished then the transition has finished
			_finishTransition();
		}
	}

	private IEnumerator _delayCloseSFX() {
		yield return new WaitForSeconds(_DELAY_CLOSE_SFX_TIME);
		// Trigger the event that will cause the player to walk through this object
		MM.soundEffects.Play(closeSoundEffect);
	}

	private IEnumerator _delaySetState(PersistentObjectState state, float initialDelay, bool processUpdate) {
		yield return new WaitForSeconds(initialDelay);
		_state = state;
		MM.player.SetPersistentObjectState(persistentObject, state);
		if (processUpdate) {
			_updateState();
		}
	}

	private IEnumerator _delaySetProperty(PersistentObjectProperty property, int value, float initialDelay, bool processUpdate) {
		yield return new WaitForSeconds(initialDelay);
		MM.player.SetPersistentObjectProperty(persistentObject, property, value);
		if (processUpdate) {
			_updateState();
		}
	}
	
	private IEnumerator _delayOpenDoor(bool playerLookAway, float initialDelay) {
		yield return new WaitForSeconds(initialDelay);
		_triggerLocationTransitionStart(playerLookAway);
	}

	private void _processPersistentObjectProperties(Dictionary<PersistentObjectProperty, int> properties) {
		// Loop through the properties, pick out any that are relevant to this object and then apply the values
		if (properties.ContainsKey(PersistentObjectProperty.RESET)) {
			_resetLayerOnEnable = properties[PersistentObjectProperty.RESET] != 0;
		} else {
			_resetLayerOnEnable = true;
		}
		if (properties.ContainsKey(PersistentObjectProperty.TRANSITION_DISPLAY_TYPE)) {
			transitionType = (TransitionDisplayType)properties[PersistentObjectProperty.TRANSITION_DISPLAY_TYPE];
		}
	}
}
