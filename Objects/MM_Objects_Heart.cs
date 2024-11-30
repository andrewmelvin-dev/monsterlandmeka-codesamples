using UnityEngine;
using System.Collections;

public class MM_Objects_Heart : MonoBehaviour {

	private const float _SPAWN_HORIZONTAL_FORCE = 250f;
	private const float _SPAWN_FLYING_FORCE = 15f;
	private const float _SPAWN_VERTICAL_FORCE = 90000f;
	private const float _MASS = 100f;
	private const float _BOUNCE_HORIZONTAL_FORCE = 150f;
	private const float _READY_STATE_DELAY = 0.5f;
	private const float _INACTIVE_DELAY = 1.5f;
	private const float _GRAVITY_FALLING = 1f;
	private const string _ANIMATOR_SPEED = "Speed";
	private const float _ANIMATOR_NORMAL_SPEED = 1f;
	private const float _ANIMATOR_FAST_SPEED = 8f;
	private const float _WINGS_FAST_TIME = 0.6f;
	private const float _WINGS_NORMAL_TIME = 2.4f;
	private const float _FADING_ALPHA_SPEED = 2f;

	public SceneAreaLayer layer;
	public int healAmount = 50;
	public Animator animator;
	public SpriteRenderer spriteRenderer;
	public SpriteRenderer wings1;
	public SpriteRenderer wings2;
	public Rigidbody2D heartRigidbody;
	public Collider2D heartCollider;
	public GameObject sparkles1;
	public GameObject sparkles2;
	public GameObject activate1;

	private PersistentObjectState _state;
	private int _id;
	private IEnumerator _directionChangeCoroutine;
	private bool _directionChangeCoroutineRunning;
	private int _directionChangeLimit;
	private int _directionChangeIterations;
	private bool _isFloating;
	private bool _movingRight;
	private bool _fadingOut = false;
	private float _fadingAlpha;

	private void OnEnable() {
		_id = gameObject.GetInstanceID();
		_directionChangeCoroutineRunning = false;
		_updateState(PersistentObjectState.HIDDEN);
	}

	private void OnDisable() {
		if (_directionChangeCoroutineRunning) {
			StopCoroutine(_directionChangeCoroutine);
			_directionChangeCoroutineRunning = false;
		}
		_updateState(PersistentObjectState.INACTIVE);
	}

	private void Update() {
		if (_fadingOut) {
			_fadingAlpha -= _FADING_ALPHA_SPEED * Time.deltaTime;
			if (_fadingAlpha <= 0f) {
				_fadingOut = false;
				SetSpriteAlpha(0);
			} else {
				SetSpriteAlpha(_fadingAlpha);
			}
		}
	}

	public void Spawn(SceneAreaLayer spawnLayer, int directionChangeLimit, bool startWithHorizontalMovement) {
		// Set the object to the ready state, set the layer, and play the spawning sound effect
		_updateState(PersistentObjectState.SPAWNING);
		SetLayer(spawnLayer, false);

		if (directionChangeLimit > 0) {
			MM.soundEffects.Play(MM_SFX.FLYING_SPAWN);
			// Apply a vertical force upwards as part of the spawn process
			heartRigidbody.AddForce(new Vector2(startWithHorizontalMovement ? (_SPAWN_HORIZONTAL_FORCE / 2) : 0, _SPAWN_FLYING_FORCE));
			animator.SetFloat(_ANIMATOR_SPEED, _ANIMATOR_FAST_SPEED);
			_directionChangeIterations = 0;
			_directionChangeLimit = directionChangeLimit;
			_isFloating = true;
			_movingRight = true;
			// Start a coroutine that will make this heart retrievable by the player after a short interval
			StartCoroutine(_changeStateToReady());
			// Start a coroutine that will change the floating direction after a short period
			if (_directionChangeCoroutineRunning) {
				StopCoroutine(_directionChangeCoroutine);
			}
			_directionChangeCoroutine = _changeFloatingDirection();
			StartCoroutine(_directionChangeCoroutine);
			_directionChangeCoroutineRunning = true;
		} else {
			// Apply a vertical force upwards as part of the spawn process
			heartRigidbody.gravityScale = _GRAVITY_FALLING;
			heartRigidbody.mass = _MASS;
			heartRigidbody.AddForce(new Vector2(startWithHorizontalMovement ? (_SPAWN_HORIZONTAL_FORCE / 2) : 0, _SPAWN_VERTICAL_FORCE));
			animator.enabled = false;
			_isFloating = false;
			SetSpriteAlpha(0);
			// Start a coroutine that will make this heart retrievable by the player after a short interval
			StartCoroutine(_changeStateToReady());
		}
	}

	public void SetLayer(SceneAreaLayer newLayer, bool front) {
		layer = newLayer;
		// Update the sprite renderer to the correct layer
		switch (layer) {
			case SceneAreaLayer.LAYER_1:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
					wings1.sortingLayerName = MM_Constants.LAYER_1_FRONT;
					wings2.sortingLayerName = MM_Constants.LAYER_1_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
					wings1.sortingLayerName = MM_Constants.LAYER_1_BACK;
					wings2.sortingLayerName = MM_Constants.LAYER_1_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_2:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
					wings1.sortingLayerName = MM_Constants.LAYER_2_FRONT;
					wings2.sortingLayerName = MM_Constants.LAYER_2_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
					wings1.sortingLayerName = MM_Constants.LAYER_2_BACK;
					wings2.sortingLayerName = MM_Constants.LAYER_2_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_3:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
					wings1.sortingLayerName = MM_Constants.LAYER_3_FRONT;
					wings2.sortingLayerName = MM_Constants.LAYER_3_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
					wings1.sortingLayerName = MM_Constants.LAYER_3_BACK;
					wings2.sortingLayerName = MM_Constants.LAYER_3_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					activate1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
				}
				break;
		}
	}

	public void SetSpriteAlpha(float alpha) {
		wings1.color = new Color(1f, 1f, 1f, alpha);
		wings2.color = new Color(1f, 1f, 1f, alpha);
	}

	private void OnTriggerEnter2D(Collider2D collider) {
		if (MM_Helper.IsPlayerInteractColliding(collider) && MM.player.CurrentSceneAreaLayer == layer && _state == PersistentObjectState.READY) {
			_updateState(PersistentObjectState.ACTIVATING);
		}
	}

	private void _updateState(PersistentObjectState state) {
		_state = state;
		switch (_state) {
			case PersistentObjectState.HIDDEN:
				// Hide the heart and sparkle elements
				spriteRenderer.enabled = false;
				sparkles1.SetActive(false);
				break;
			case PersistentObjectState.SPAWNING:
				// Show the heart and sparkle elements
				spriteRenderer.enabled = true;
				sparkles1.SetActive(true);
				SetSpriteAlpha(1);
				break;
			case PersistentObjectState.READY:
				// Show the heart and sparkle elements
				spriteRenderer.enabled = true;
				sparkles1.SetActive(true);
				break;
			case PersistentObjectState.ACTIVATING:
				// Hide the heart and sparkle elements but play the loot graphic and sound effects
				spriteRenderer.enabled = false;
				sparkles1.SetActive(false);
				activate1.SetActive(true);
				SetSpriteAlpha(0);
				MM.soundEffects.Play(MM_SFX.BOCK_HEAL1);
				MM.Player.AddHealth(healAmount);
				if (_directionChangeCoroutineRunning) {
					StopCoroutine(_directionChangeCoroutine);
				}
				_directionChangeCoroutineRunning = false;
				// Deactivate the object after a short period
				StartCoroutine(_inactive());
				break;
			case PersistentObjectState.INACTIVE:
				// Deactivate the object
				spriteRenderer.enabled = false;
				sparkles1.SetActive(false);
				transform.gameObject.SetActive(false);
				break;
		}
	}

	private Vector2 _getRandomHorizontalForce() {
		return ((UnityEngine.Random.value > 0.5f) ? Vector2.left : Vector2.right) * _BOUNCE_HORIZONTAL_FORCE;
	}

	private IEnumerator _changeStateToReady() {
		yield return new WaitForSeconds(_READY_STATE_DELAY);
		// Allow the heart to be acquired by the player
		_updateState(PersistentObjectState.READY);
		// Activate the collider
		heartCollider.enabled = true;
	}

	private IEnumerator _inactive() {
		yield return new WaitForSeconds(_INACTIVE_DELAY);
		_updateState(PersistentObjectState.INACTIVE);
	}

	private IEnumerator _changeFloatingDirection() {
		while (_isFloating) {
			yield return new WaitForSeconds(_WINGS_FAST_TIME);
			animator.SetFloat(_ANIMATOR_SPEED, _ANIMATOR_NORMAL_SPEED);
			yield return new WaitForSeconds(_WINGS_NORMAL_TIME);

			_directionChangeIterations++;
			if (_directionChangeIterations >= _directionChangeLimit) {
				// Let the heart fall to the ground when it has reached its direction change limit
				heartRigidbody.gravityScale = _GRAVITY_FALLING;
				heartRigidbody.mass = _MASS;
				animator.enabled = false;
				_isFloating = false;
				_fadingAlpha = 1f;
				_fadingOut = true;
			} else {
				_movingRight = !_movingRight;
				heartRigidbody.AddForce(new Vector2(_SPAWN_HORIZONTAL_FORCE * (_movingRight ? 1 : -1), _SPAWN_FLYING_FORCE));
				animator.SetFloat(_ANIMATOR_SPEED, _ANIMATOR_FAST_SPEED);
			}
		}
	}
}
