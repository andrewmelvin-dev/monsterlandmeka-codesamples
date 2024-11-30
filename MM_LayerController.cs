using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MM_LayerController : MonoBehaviour
{
	private string _ANIMATION_REVEAL = "reveal";
	private string _ANIMATION_CONCEAL = "conceal";
	private string _ANIMATION_REVEAL_FAST = "revealfast";
	private string _ANIMATION_CONCEAL_FAST = "concealfast";
	private string _ANIMATION_EXPANDED = "expanded";
	private string _ANIMATION_CONTRACTED = "contracted";
	private float _ANIMATION_DURATION = 1f;

	// LayerRenderer order:
	// 0: Layer3_back
	// 1: Effects back
	// 2: Player
	// 3: Effects front
	// 4: Layer3_front
	// 5: Layer2_back
	// 6: Effects back
	// 7: Player
	// 8: Effects front
	// 9: Layer2_front
	// 10: Layer1_back
	// 11: Effects back
	// 12: Player
	// 13: Effects front
	// 14: Layer1_front

	private int _ORDER_EFFECTS_BACK_LAYER_3 = 1;
	private int _ORDER_EFFECTS_BACK_LAYER_2 = 6;
	private int _ORDER_EFFECTS_BACK_LAYER_1 = 11;
	private int _ORDER_EFFECTS_FRONT_LAYER_1 = 13;
	private int _ORDER_PLAYER_LAYER_3 = 2;
	private int _ORDER_PLAYER_LAYER_2 = 7;
	private int _ORDER_PLAYER_LAYER_1 = 12;

	private Transform _layer1Back;
	private Transform _layer1Front;
	private Transform _layer2Back;
	private Transform _layer2Front;
	private Transform _layer3Back;
	private Transform _layer3Front;
	private Canvas _layerEffects_back;
	private Canvas _layerEffects_front;
	private Canvas _layerPlayer;

	private Animator _resizeLayerBackViewport;
	private Animator _resizeLayerFrontViewport;

	private Vector2? _movingToPosition;
	private int? _movingFromSourceId;
	private int? _movingToDestinationId;
	private SceneAreaLayer? _movingToLayer;
	private bool _movingToShouldStartTransition;
	private bool _isResizing;
	private bool _isRevealing;

	private void OnEnable() {
		MM_Events.OnSceneLoaded += _sceneLoaded;
		MM_Events.OnLayerReveal += _revealLayer;
		MM_Events.OnLayerConceal += _concealLayer;
		MM_Events.OnLayerResizeFinished += _layerResizeFinished;
	}

	private void OnDisable() {
		MM_Events.OnSceneLoaded -= _sceneLoaded;
		MM_Events.OnLayerReveal -= _revealLayer;
		MM_Events.OnLayerConceal -= _concealLayer;
		MM_Events.OnLayerResizeFinished -= _layerResizeFinished;
	}

	public bool IsResizing() {
		return _isResizing;
	}

	public bool IsRevealing() {
		return _isRevealing;
	}

	public Vector2? GetMovingToPosition() {
		return _movingToPosition;
	}

	public int? GetMovingFromSourceId() {
		return _movingFromSourceId;
	}

	public int? GetMovingToDestinationId() {
		return _movingToDestinationId;
	}

	public bool GetMovingToShouldStartTransition() {
		return _movingToShouldStartTransition;
	}

	public SceneAreaLayer? GetMovingToLayer() {
		return _movingToLayer;
	}

	public void SetSceneAreaLayersUsed(bool layer1, bool layer2, bool layer3) {
		Debug.Log("MM_LayerController:SetSceneAreaLayersUsed : setting active state for renderered layers 1, 2, 3 to [" + layer1 + "," + layer2 + "," + layer3 + "]");
		_layer1Back.gameObject.SetActive(layer1);
		_layer1Front.gameObject.SetActive(layer1);
		_layer2Back.gameObject.SetActive(layer2);
		_layer2Front.gameObject.SetActive(layer2);
		_layer3Back.gameObject.SetActive(layer3);
		_layer3Front.gameObject.SetActive(layer3);
	}

	public void UpdateLayerViewports() {
		switch (MM.player.CurrentSceneAreaLayer) {
			case SceneAreaLayer.LAYER_1:
				if (_layer1Back.gameObject.activeSelf) {
					_layer1Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer1Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				if (_layer2Back.gameObject.activeSelf) {
					_layer2Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer2Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				if (_layer3Back.gameObject.activeSelf) {
					_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				break;
			case SceneAreaLayer.LAYER_2:
				if (_layer1Back.gameObject.activeSelf) {
					_layer1Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
					_layer1Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
				}
				if (_layer2Back.gameObject.activeSelf) {
					_layer2Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer2Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				if (_layer3Back.gameObject.activeSelf) {
					_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				break;
			case SceneAreaLayer.LAYER_3:
				if (_layer1Back.gameObject.activeSelf) {
					_layer1Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
					_layer1Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
				}
				if (_layer2Back.gameObject.activeSelf) {
					_layer2Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
					_layer2Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
				}
				if (_layer3Back.gameObject.activeSelf) {
					_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
					_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				}
				break;
		}
	}

	public void UpdatePlayerLayerOrder(SceneAreaLayer layer) {
		switch (layer) {
			case SceneAreaLayer.LAYER_1:
				// Place the player between Layer1_back and Layer1_front
				_layerEffects_back.sortingOrder = _ORDER_EFFECTS_BACK_LAYER_1;
				_layerPlayer.sortingOrder = _ORDER_PLAYER_LAYER_1;
				break;
			case SceneAreaLayer.LAYER_2:
				// Place the player between Layer1_back and Layer1_front
				_layerEffects_back.sortingOrder = _ORDER_EFFECTS_BACK_LAYER_2;
				_layerPlayer.sortingOrder = _ORDER_PLAYER_LAYER_2;
				break;
			case SceneAreaLayer.LAYER_3:
				// Place the player between Layer1_back and Layer1_front
				_layerEffects_back.sortingOrder = _ORDER_EFFECTS_BACK_LAYER_3;
				_layerPlayer.sortingOrder = _ORDER_PLAYER_LAYER_3;
				break;
		}
		// Always place the front effects layer in front of all layers
		_layerEffects_front.sortingOrder = _ORDER_EFFECTS_FRONT_LAYER_1;
	}

	private void _sceneLoaded() {
		// Reset references to each layer
		try {
			_layer1Back = GameObject.Find(MM_Constants.LAYER_1_BACK_MASK).transform;
			_layer1Front = GameObject.Find(MM_Constants.LAYER_1_FRONT_MASK).transform;
			_layer2Back = GameObject.Find(MM_Constants.LAYER_2_BACK_MASK).transform;
			_layer2Front = GameObject.Find(MM_Constants.LAYER_2_FRONT_MASK).transform;
			_layer3Back = GameObject.Find(MM_Constants.LAYER_3_BACK_MASK).transform;
			_layer3Front = GameObject.Find(MM_Constants.LAYER_3_FRONT_MASK).transform;
			_layerEffects_back = GameObject.Find(MM_Constants.LAYER_EFFECTS_BACK).GetComponent<Canvas>();
			_layerEffects_front = GameObject.Find(MM_Constants.LAYER_EFFECTS_FRONT).GetComponent<Canvas>();
			_layerPlayer = GameObject.Find(MM_Constants.LAYER_PLAYER).GetComponent<Canvas>();
		} catch (Exception e) {
			Debug.LogError("MM_LayerController:_sceneLoaded : cannot get reference to layer : " + e.Message);
		}
	}

	private void _revealLayer(MM_EventData data) {
		bool fast = data.transition_position == null;
		bool startAnimation = false;
		float backAnimationStartTime = 0f;
		float frontAnimationStartTime = 0f;
		RectTransform layerRect = null;

		_resizeLayerBackViewport = null;
		_resizeLayerFrontViewport = null;
		// Reveal the 2nd layer that is under the 1st, or reveal the 3rd layer that is under the 2nd
		// Also ensure other layers have the correct state before the layer resizing begins
		if (data.sceneAreaLayer == SceneAreaLayer.LAYER_2) {
			startAnimation = true;
			layerRect = _layer1Back.GetComponent<RectTransform>();
			_resizeLayerBackViewport = _layer1Back.GetChild(1).GetComponent<Animator>();
			_resizeLayerFrontViewport = _layer1Front.GetChild(1).GetComponent<Animator>();
			if (_layer2Back.gameObject.activeSelf) {
				_layer2Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer2Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
			if (_layer3Back.gameObject.activeSelf) {
				_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
		} else if (data.sceneAreaLayer == SceneAreaLayer.LAYER_3) {
			startAnimation = true;
			layerRect = _layer2Back.GetComponent<RectTransform>();
			_resizeLayerBackViewport = _layer2Back.GetChild(1).GetComponent<Animator>();
			_resizeLayerFrontViewport = _layer2Front.GetChild(1).GetComponent<Animator>();
			if (_layer1Back.gameObject.activeSelf) {
				_layer1Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
				_layer1Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
			}
			if (_layer3Back.gameObject.activeSelf) {
				_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
		} else {
			Debug.LogError("MM_LayerController:_revealLayer : cannot reveal layer [" + data.sceneAreaLayer + "]");
		}

		if (startAnimation) {
			if (_isResizing) {
				// If interrupting an existing animation then adjust the new animation start time
				backAnimationStartTime = _ANIMATION_DURATION - _resizeLayerBackViewport.GetCurrentAnimatorStateInfo(0).normalizedTime;
				frontAnimationStartTime = _ANIMATION_DURATION - _resizeLayerFrontViewport.GetCurrentAnimatorStateInfo(0).normalizedTime;
			}

			// Centre the reveal on the player
			Vector2 viewportPosition = MM.MainCamera.WorldToViewportPoint(MM.Player.transform.position);
			Vector2 screenPosition = new Vector2(
				(viewportPosition.x * layerRect.sizeDelta.x) - (layerRect.sizeDelta.x * 0.5f),
				(viewportPosition.y * layerRect.sizeDelta.y) - (layerRect.sizeDelta.y * 0.5f)
			);

			_isResizing = true;
			_isRevealing = true;
			_movingToPosition = data.transition_position;
			_movingFromSourceId = data.sourceId;
			_movingToDestinationId = data.destinationId;
			_movingToLayer = (SceneAreaLayer)data.sceneAreaLayer;
			_movingToShouldStartTransition = false;
			if (_resizeLayerBackViewport) {
				_resizeLayerBackViewport.transform.localPosition = screenPosition;
				_resizeLayerBackViewport.Play(fast ? _ANIMATION_REVEAL_FAST : _ANIMATION_REVEAL, 0, backAnimationStartTime);
			}
			if (_resizeLayerFrontViewport) {
				_resizeLayerFrontViewport.transform.localPosition = screenPosition;
				_resizeLayerFrontViewport.Play(fast ? _ANIMATION_REVEAL_FAST : _ANIMATION_REVEAL, 0, frontAnimationStartTime);
			}
		}
	}

	private void _concealLayer(MM_EventData data) {
		bool fast = data.transition_position == null;
		bool startAnimation = false;
		float backAnimationStartTime = 0f;
		float frontAnimationStartTime = 0f;
		RectTransform canvasRect = null;

		_resizeLayerBackViewport = null;
		_resizeLayerFrontViewport = null;
		// Conceal the 3rd layer that is under the 2nd, or the 2nd layer that is under the 1st
		if (data.sceneAreaLayer == SceneAreaLayer.LAYER_3) {
			startAnimation = true;
			canvasRect = _layer2Back.GetComponent<RectTransform>();
			_resizeLayerBackViewport = _layer2Back.GetChild(1).GetComponent<Animator>();
			_resizeLayerFrontViewport = _layer2Front.GetChild(1).GetComponent<Animator>();
			if (_layer1Back.gameObject.activeSelf) {
				_layer1Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
				_layer1Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_EXPANDED);
			}
			if (_layer3Back.gameObject.activeSelf) {
				_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
		} else if (data.sceneAreaLayer == SceneAreaLayer.LAYER_2) {
			startAnimation = true;
			canvasRect = _layer1Back.GetComponent<RectTransform>();
			_resizeLayerBackViewport = _layer1Back.GetChild(1).GetComponent<Animator>();
			_resizeLayerFrontViewport = _layer1Front.GetChild(1).GetComponent<Animator>();
			if (_layer2Back.gameObject.activeSelf) {
				_layer2Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer2Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
			if (_layer3Back.gameObject.activeSelf) {
				_layer3Back.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
				_layer3Front.GetChild(1).GetComponent<Animator>().Play(_ANIMATION_CONTRACTED);
			}
		} else {
			Debug.LogError("MM_LayerController:_concealLayer : cannot conceal layer [" + data.sceneAreaLayer + "]");
		}

		if (startAnimation) {
			if (_isResizing) {
				// If interrupting an existing animation then adjust the new animation start time
				backAnimationStartTime = _ANIMATION_DURATION - _resizeLayerBackViewport.GetCurrentAnimatorStateInfo(0).normalizedTime;
				frontAnimationStartTime = _ANIMATION_DURATION - _resizeLayerFrontViewport.GetCurrentAnimatorStateInfo(0).normalizedTime;
			}

			// Conceal the reveal on the player
			Vector2 viewportPosition = MM.MainCamera.WorldToViewportPoint(MM.Player.transform.position);
			Vector2 screenPosition = new Vector2(
				(viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
				(viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
			);

			_isResizing = true;
			_isRevealing = false;
			_movingToPosition = data.transition_position;
			_movingFromSourceId = data.sourceId;
			_movingToDestinationId = data.destinationId;
			_movingToLayer = (SceneAreaLayer)((int)data.sceneAreaLayer - 1);
			_movingToShouldStartTransition = true;
			if (_resizeLayerBackViewport) {
				_resizeLayerBackViewport.transform.localPosition = screenPosition;
				_resizeLayerBackViewport.Play(fast ? _ANIMATION_CONCEAL_FAST : _ANIMATION_CONCEAL, 0, backAnimationStartTime);
			}
			if (_resizeLayerFrontViewport) {
				_resizeLayerFrontViewport.transform.localPosition = screenPosition;
				_resizeLayerFrontViewport.Play(fast ? _ANIMATION_CONCEAL_FAST : _ANIMATION_CONCEAL, 0, frontAnimationStartTime);
			}
		}
	}

	private void _layerResizeFinished() {
		_isResizing = false;
	}
}
