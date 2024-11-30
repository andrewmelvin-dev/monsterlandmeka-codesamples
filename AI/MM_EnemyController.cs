using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_EnemyController : MonoBehaviour {

	public Animator animator;
	public SpriteRenderer spriteRenderer;
	public Collider2D enemyCollider;
	public SceneAreaLayer layer;

	public LootTypeChance[] firstDropLoot = new LootTypeChance[] { new LootTypeChance() { type = LootType.GOLD_SMALL, percent = 100 } };
	public LootTypeChance[] secondDropLoot = new LootTypeChance[] { new LootTypeChance() { type = LootType.POINTS_SMALL, percent = 100 } };
	public LootTypeChance[] ongoingDropLoot = new LootTypeChance[] { new LootTypeChance() { type = LootType.GOLD_SMALL, percent = 100 } };
	public LootType backupDropLoot = LootType.NOTHING;

	public GameObject[] connectedObjects;

	private MM_Objects_SpawnerEnemy _parentSpawner = null;
	private PersistentObjectState _state;
	private int _id;
	private Rigidbody2D _rigidbody;
	private Automator _onDeathAutomator = Automator.NONE;
	private int _spawnCount = 1;

	private void OnEnable() {
		_id = gameObject.GetInstanceID();
		_rigidbody = transform.GetComponent<Rigidbody2D>();
		_updateState(PersistentObjectState.HIDDEN);
	}

	private void OnDisable() {
		_updateState(PersistentObjectState.INACTIVE);
	}

	public void SetInactive() {
		_updateState(PersistentObjectState.INACTIVE);
	}

	public void Spawn(SceneAreaLayer spawnLayer, MM_Objects_SpawnerEnemy parentSpawner, int count, Automator onDeathAutomator = Automator.NONE) {
		// Set the object to the ready state and set the layer
		_parentSpawner = parentSpawner;
		_spawnCount = count;
		_onDeathAutomator = onDeathAutomator;
		_updateState(PersistentObjectState.SPAWNING);
		SetLayer(spawnLayer, false);
		// Give the parent spawner a reference to this object
		if (_parentSpawner != null) {
			if (!_parentSpawner.isActiveAndEnabled) {
				// If there is a parent spawner but it is disabled then unspawn this enemy
				SetInactive();
				_parentSpawner.ReversePreviousSpawn();
			} else {
				_parentSpawner.SetCurrentEnemy(this);
			}
		}
	}

	public void SetLayer(SceneAreaLayer newLayer, bool front) {
		layer = newLayer;
		// Update the sprite renderer to the correct layer
		switch (layer) {
			case SceneAreaLayer.LAYER_1:
				if (front) {
					gameObject.layer = MM_Constants.LAYER_1_FRONT_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
				} else {
					gameObject.layer = MM_Constants.LAYER_1_BACK_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_2:
				if (front) {
					gameObject.layer = MM_Constants.LAYER_2_FRONT_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
				} else {
					gameObject.layer = MM_Constants.LAYER_2_BACK_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
				}
				break;
			case SceneAreaLayer.LAYER_3:
				if (front) {
					gameObject.layer = MM_Constants.LAYER_3_FRONT_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
				} else {
					gameObject.layer = MM_Constants.LAYER_3_BACK_INT;
					spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
				}
				break;
		}
		spriteRenderer.gameObject.layer = gameObject.layer;
		for (int i = 0; i < connectedObjects.Length; i++) {
			connectedObjects[i].layer = gameObject.layer;
		}
	}

	public void Stun() {
		AIBrain aiBrain = gameObject.MMGetComponentNoAlloc<AIBrain>();
		if (aiBrain != null) {
			aiBrain.TransitionToState(MM_Constants.PLAYER_SKATEBOARD_BOUNCE_STATE, true);
		}
		MM.soundEffects.Play(MM_SFX.ENEMY_STUN_BOING);
		GameObject spawn = Instantiate(MM.prefabs.uiFX_stunStars, transform.position + MM_Constants.PLAYER_SKATEBOARD_STUN_OFFSET, transform.rotation * Quaternion.Euler(MM_Constants.PLAYER_SKATEBOARD_STUN_ROTATION_X, 0, 0));
		spawn.transform.SetParent(transform);
	}

	public void Kill() {
		// Trigger an automator if onDeathAutomator is set
		// This allows an action to always occur when a character is killed
		if (_onDeathAutomator != Automator.NONE && MM_Constants_Automators.Automators.ContainsKey(_onDeathAutomator) && MM_Constants_Automators.Automators[_onDeathAutomator].trigger == AutomatorTrigger.ON_PLAYER_CHARACTER_DEATH) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[_onDeathAutomator]);
		}

		// Determine the collection of possible drops
		LootTypeChance[] dropCollection;
		switch (_spawnCount) {
			case 1:
				dropCollection = firstDropLoot;
				break;
			case 2:
				dropCollection = secondDropLoot;
				break;
			default:
				dropCollection = ongoingDropLoot;
				break;
		}
		if (dropCollection.Length > 0) {
			// Play the destruction effect
			MM.uiEffects.Play(GraphicEffect.DESTROY_1, transform.position);

			Dictionary<LootType, float> drops = new Dictionary<LootType, float>();
			for (int i = 0; i < dropCollection.Length; i++) {
				drops.Add(dropCollection[i].type, (dropCollection[i].percent / 100f));
			}
			LootType lootType = drops.RandomElementByWeight(e => e.Value).Key;

			// Check whether points items can be spawned in the current SceneArea
			if (lootType == LootType.POINTS_SMALL || lootType == LootType.POINTS_LARGE) {
				MM_SceneArea area = MM_Locations.SceneAreaData[MM.player.CurrentSceneArea];
				if (area.maxEnemyDropPoints <= 0 || (MM.player.LootData.ContainsKey(MM.player.CurrentSceneArea) && MM.player.LootData[MM.player.CurrentSceneArea].pointsAcquired >= area.maxEnemyDropPoints)) {
					lootType = backupDropLoot;
				}
			}

			// Spawn the loot
			Transform parentTransform = MM.SceneController.GetObjectsContainer();
			switch (lootType) {
				case LootType.GOLD_SMALL:
					MM.spawner.SpawnGold(MM_LootProfiles.GetLootForSceneArea_goldSmallAmount(MM.player.CurrentSceneArea), layer, transform.position, false, null);
					break;
				case LootType.GOLD_LARGE:
					MM.spawner.SpawnGold(MM_LootProfiles.GetLootForSceneArea_goldLargeAmount(MM.player.CurrentSceneArea), layer, transform.position, false, null);
					break;
				case LootType.POINTS_SMALL:
					MM.spawner.SpawnPoints(MM_LootProfiles.GetPointsSpawnObject(MM_LootProfiles.GetLootForSceneArea_pointsSmallAmount(MM.player.CurrentSceneArea)), layer, transform.position, false, null, false);
					break;
				case LootType.POINTS_LARGE:
					MM.spawner.SpawnPoints(MM_LootProfiles.GetPointsSpawnObject(MM_LootProfiles.GetLootForSceneArea_pointsLargeAmount(MM.player.CurrentSceneArea)), layer, transform.position, false, null, false);
					break;
				case LootType.HEALTH_SMALL:
					MM.spawner.SpawnHeart(layer, transform.position, 0, false);
					break;
				case LootType.GROUP_1_RANDOM:
					_spawnObject(MM_LootProfiles.GetRandomGroup1(MM.player.CurrentSceneArea));
					break;
			}
		}

		// Update the kill count on the parent spawner
		if (_parentSpawner != null) {
			_parentSpawner.EnemyKilled();
		}
	}

	private void _updateState(PersistentObjectState state) {
		_state = state;
		switch (_state) {
			case PersistentObjectState.HIDDEN:
				// Hide the enemy
				spriteRenderer.enabled = false;
				enemyCollider.enabled = false;
				break;
			case PersistentObjectState.SPAWNING:
				// Show the enemy
				spriteRenderer.enabled = true;
				enemyCollider.enabled = true;
				break;
			case PersistentObjectState.READY:
				// Show the enemy
				spriteRenderer.enabled = true;
				enemyCollider.enabled = true;
				break;
			case PersistentObjectState.INACTIVE:
				// Hide the enemy
				spriteRenderer.enabled = false;
				enemyCollider.enabled = false;
				transform.gameObject.SetActive(false);
				break;
		}
	}

	private void _spawnObject(SpawnObject spawnObject) {
		switch (spawnObject) {
			case SpawnObject.ABILITY_ITEM_FIREBALL:
				MM.spawner.SpawnAbilityItem(SpawnObject.ABILITY_ITEM_FIREBALL, layer, transform.position, false, null);
				break;
		}
	}
}
