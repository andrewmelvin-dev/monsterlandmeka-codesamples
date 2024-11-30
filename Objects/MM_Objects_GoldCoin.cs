using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MM_Objects_GoldCoin : MonoBehaviour {

	private const float _SPAWN_VERTICAL_FORCE = 55000f;
	private const float _BOUNCE_HORIZONTAL_FORCE = 15000f;
	private const float _MINIMUM_VELOCITY = 4f;
	private const float _ROLLING_FORCE = 1.25f;
	private const float _READY_STATE_DELAY = 0.5f;
	private const float _INACTIVE_DELAY = 1.5f;

	public Animator animator;
	public SpriteRenderer spriteRenderer;
	public GameObject sparkles1;
	public GameObject activate1;
	public GameObject sparkles2;
	public GameObject activate2;
	public Collider2D goldCollider;
	public Rigidbody2D goldRigidbody;
	public int amount;
	public SceneAreaLayer layer;

	private PersistentObjectState _state;
	private int _id;
	private bool _firstBounceOccurred;
	private Vector2 _currentVelocity;
	private float _currentAngularVelocity;
	private Vector2 _currentPosition;
	private Vector2 _previousVelocity;
	private float _previousAngularVelocity;
	private Vector2 _previousPosition;
	private List<Collider2D> _colliders = new List<Collider2D>();
	private string _ANIMATION_SPIN = "CoinSpin";

	private void OnEnable() {
		_id = gameObject.GetInstanceID();
		_firstBounceOccurred = false;
		_colliders.Clear();
		_updateState(PersistentObjectState.HIDDEN);
	}

	private void OnDisable() {
		_updateState(PersistentObjectState.INACTIVE);
	}

	private void Update() {
		if (_state == PersistentObjectState.READY || _state == PersistentObjectState.SPAWNING) {
			_previousVelocity = _currentVelocity;
			_previousAngularVelocity = _currentAngularVelocity;
			_previousPosition = _currentPosition;
			_currentVelocity = goldRigidbody.velocity;
			_currentAngularVelocity = goldRigidbody.angularVelocity;
			_currentPosition = transform.position;
		}
	}

	public void Spawn(int spawnAmount, SceneAreaLayer spawnLayer, bool startWithHorizontalMovement, bool? forceLeft = null) {

		// Set the object to the ready state, set the amount and layer, and play the spawning sound effect
		_updateState(PersistentObjectState.SPAWNING);
		amount = spawnAmount;
		SetLayer(spawnLayer, false);
		MM.soundEffects.Play(MM_SFX.COIN_SPAWN);
		// Apply a vertical force upwards as part of the spawn process
		goldRigidbody.AddForce(new Vector2(0, _SPAWN_VERTICAL_FORCE));
		// If required apply a horizontal force when the object spawns
		if (startWithHorizontalMovement) {
			_firstBounceOccurred = true;
			goldRigidbody.AddForce(_getRandomHorizontalForce(forceLeft));
		}
		// Start a coroutine that will make this coin retrievable by the player after a short interval
		StartCoroutine(_changeStateToReady());
	}

	public void SetLayer(SceneAreaLayer newLayer, bool front) {
		layer = newLayer;
		// Update the sprite renderer to the correct layer
		switch (layer) {
			case SceneAreaLayer.LAYER_1:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_1_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_2:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_2_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_3:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
					sparkles1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					sparkles2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					activate1.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
					activate2.GetComponent<ParticleSystemRenderer>().sortingLayerName = MM_Constants.LAYER_3_BACK;
				}
				break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collider) {
		if (MM_Helper.IsPlayerInteractColliding(collider) && MM.player.CurrentSceneAreaLayer == layer && _state == PersistentObjectState.READY) {
			_updateState(PersistentObjectState.ACTIVATING);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		// Ignore the collision if:
		// 1. the object is not a platform/wall, and return the item to the velocity and position from the last frame
		// 2. the object is on another SceneAreaLayer
		// 3. the object has a SceneObject tag
		if (!MM_Helper.IsPlatformCollision(collision, _currentVelocity.y) || (MM_Helper.GetObjectLayer(collision.gameObject.layer) != layer && collision.gameObject.layer != MM_Constants.LAYERS_ALL_INT) || collision.gameObject.tag == MM_Constants.SCENE_OBJECT) {
			Debug.LogWarning("MM_Objects_GoldCoin:OnCollisionEnter2D : ignoring collisions between " + gameObject.name + " and " + collision.gameObject.name);
			Physics2D.IgnoreCollision(goldCollider, collision.collider);
			_colliders.Add(collision.collider);
			goldRigidbody.velocity = _previousVelocity;
			goldRigidbody.angularVelocity = _previousAngularVelocity;
			if (_previousVelocity.x != 0f || _previousVelocity.y != 0f) {
				transform.position = _previousPosition - goldRigidbody.velocity * Time.deltaTime;
			}
			return;
		}
		if (!_firstBounceOccurred) {
			// Make the coin bounce in a random horizontal direction and random force
			_firstBounceOccurred = true;
			goldRigidbody.AddForce(_getRandomHorizontalForce());
		} else {
			// Check if drag should be triggered: if so then the coin should start transitioning to a flat state
			float speed = goldRigidbody.velocity.y;
			if (speed > 0 && speed < _MINIMUM_VELOCITY) {
				// Force the object to the platform below it
				goldRigidbody.AddForce(Vector2.down * _ROLLING_FORCE);
			}
		}
	}

	private void _updateState(PersistentObjectState state) {
		_state = state;
		switch (_state) {
			case PersistentObjectState.HIDDEN:
				// Hide the coin and sparkle elements
				goldCollider.enabled = false;
				spriteRenderer.enabled = false;
				animator.enabled = false;
				sparkles1.SetActive(false);
				activate1.SetActive(false);
				break;
			case PersistentObjectState.SPAWNING:
				// Show the coin and sparkle elements
				goldCollider.enabled = false;
				spriteRenderer.enabled = true;
				animator.enabled = true;
				sparkles1.SetActive(true);
				activate1.SetActive(false);
				// Play the idle animation state
				animator.Play(_ANIMATION_SPIN);
				break;
			case PersistentObjectState.READY:
				// Show the coin and sparkle elements
				goldCollider.enabled = true;
				spriteRenderer.enabled = true;
				animator.enabled = true;
				sparkles1.SetActive(true);
				activate1.SetActive(false);
				break;
			case PersistentObjectState.ACTIVATING:
				// Hide the coin and sparkle elements but play the loot graphic and sound effects
				goldCollider.enabled = false;
				spriteRenderer.enabled = false;
				animator.enabled = false;
				sparkles1.SetActive(false);
				activate1.SetActive(true);
				MM.soundEffects.Play(MM_SFX.COIN_RECEIVE);
				// Add the gold
				MM.player.AddGold(amount);
				// Deactivate the object after a short period
				StartCoroutine(_inactive());
				break;
			case PersistentObjectState.INACTIVE:
				// Hide the coin and sparkle elements
				goldCollider.enabled = false;
				spriteRenderer.enabled = false;
				animator.enabled = false;
				sparkles1.SetActive(false);
				activate1.SetActive(false);
				transform.gameObject.SetActive(false);
				break;
		}
	}

	private Vector2 _getRandomHorizontalForce(bool? forceLeft = null) {
		if (forceLeft != null) {
			return ((bool)forceLeft ? Vector2.left : Vector2.right) * _BOUNCE_HORIZONTAL_FORCE * (UnityEngine.Random.value + 0.5f);
		}
		return ((UnityEngine.Random.value > 0.5f) ? Vector2.left : Vector2.right) * _BOUNCE_HORIZONTAL_FORCE * (UnityEngine.Random.value + 0.5f);
	}

	private IEnumerator _changeStateToReady() {
		yield return new WaitForSeconds(_READY_STATE_DELAY);
		// Allow the coin to be acquired by the player
		_updateState(PersistentObjectState.READY);
	}

	private IEnumerator _inactive() {
		for (int i = 0; i < _colliders.Count; i++) {
			if (_colliders[i] != null) {
				Physics2D.IgnoreCollision(goldCollider, _colliders[i], false);
			}
		}
		yield return new WaitForSeconds(_INACTIVE_DELAY);
		_updateState(PersistentObjectState.INACTIVE);
	}
}
