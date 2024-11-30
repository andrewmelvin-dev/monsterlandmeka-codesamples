using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpawnObject {
	NOTHING = 0,

	GOLD_COIN_SMALL = 1,

	GOLD_WATER_JUG = 11,

	ABILITY_ITEM_FIREBALL = 21,
}

public class MM_Objects_Spawner : MonoBehaviour {

	private bool _forceSpawnLeft = false;

	private const float _SPAWN_OFFSET_X = 1.25f;
	private const float _SPAWN_OFFSET_Y = 1.25f;

	public void SpawnSequence(List<SpawnObject> spawnObjects, SceneAreaLayer layer, Vector3 position, bool randomX, bool randomY, bool horizontalForce, float initialDelay = 0f, float interval = 0f) {
		if (position != null) {
			StartCoroutine(_playSpawnSequence(spawnObjects, layer, position, randomX, randomY, horizontalForce, initialDelay, interval));
		}
	}

	public void SpawnEnemy(MM_SceneControllerEnemyPrefabs enemy, SceneAreaLayer layer, Vector3 position, bool spawnFacingLeft, List<MMPathMovementElement> pathElements, MM_Objects_SpawnerEnemy parentSpawner, int spawnCount, float initialDelay = 0f, Automator onDeathAutomator = Automator.NONE) {
		StartCoroutine(_playSpawnSingleEnemy(enemy, layer, position, spawnFacingLeft, pathElements, parentSpawner, spawnCount, initialDelay, onDeathAutomator));
	}

	public void SpawnGold(int amount, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft) {
		_playSpawnSingleGold(amount, layer, position, startWithHorizontalMovement, forceLeft);
	}

	public void SpawnHeart(SceneAreaLayer layer, Vector3 position, int directionChangeLimit, bool startWithHorizontalMovement) {
		_playSpawnSingleHeart(layer, position, directionChangeLimit, startWithHorizontalMovement);
	}

	public void SpawnPoints(SpawnObject spawnObject, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft, bool extraForce) {
		_playSpawnSinglePoints(spawnObject, layer, position, startWithHorizontalMovement, forceLeft, extraForce);
	}

	public void SpawnPowerup(MM_SceneControllerPowerupPrefabs powerup, SceneAreaLayer layer, Vector3 position, MM_Objects_SpawnerPowerup parentSpawner, SpawnFacingDirections forceFacingDirection, float initialDelay = 0f) {
		StartCoroutine(_playSpawnSinglePowerup(powerup, layer, position, parentSpawner, forceFacingDirection, initialDelay));
	}

	public void SpawnAbilityItem(SpawnObject spawnObject, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft) {
		_playSpawnSingleAbilityItem(spawnObject, layer, position, startWithHorizontalMovement, forceLeft);
	}

	private void _checkSpawnObjectOnComplete() {
		// Check if the current automator should be triggered when the GraphicEffect effect completes
		if (MM.player.CurrentAutomator != null && MM_Constants_Automators.Automators.ContainsKey(MM.player.CurrentAutomator) && MM_Constants_Automators.Automators[MM.player.CurrentAutomator].trigger == AutomatorTrigger.ON_OBJECT_SPAWNER_COMPLETE) {
			MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[MM.player.CurrentAutomator]);
		}
	}

	private IEnumerator _playSpawnSequence(List<SpawnObject> spawnObjects, SceneAreaLayer layer, Vector3 position, bool randomX, bool randomY, bool horizontalForce, float initialDelay = 0f, float interval = 0f) {
		float xOffset;
		float yOffset;
		int amount;

		_forceSpawnLeft = (UnityEngine.Random.value > 0.5f);
		yield return new WaitForSeconds(initialDelay);
		for (int i = 0; i < spawnObjects.Count; i++) {
			xOffset = randomX ? UnityEngine.Random.Range(-_SPAWN_OFFSET_X, _SPAWN_OFFSET_X) : 0;
			yOffset = randomY ? UnityEngine.Random.Range(-_SPAWN_OFFSET_Y, _SPAWN_OFFSET_Y) : 0;
			switch (spawnObjects[i]) {
				case SpawnObject.GOLD_COIN_SMALL:
					amount = UnityEngine.Random.Range(1, MM_Constants.GOLD_COIN_SMALL_MAX);
					_playSpawnSingleGold(amount, layer, new Vector3(position.x + xOffset, position.y + yOffset, position.z), horizontalForce, true);
					break;
				case SpawnObject.GOLD_WATER_JUG:
					_playSpawnSinglePoints(spawnObjects[i], layer, new Vector3(position.x + xOffset, position.y + yOffset, position.z), horizontalForce, true, false);
					break;
				case SpawnObject.ABILITY_ITEM_FIREBALL:
					_playSpawnSingleAbilityItem(spawnObjects[i], layer, new Vector3(position.x + xOffset, position.y + yOffset, position.z), horizontalForce, true);
					break;
			}
			yield return new WaitForSeconds(interval);
		}
		_checkSpawnObjectOnComplete();
	}

	private IEnumerator _playSpawnSingleEnemy(MM_SceneControllerEnemyPrefabs enemy, SceneAreaLayer layer, Vector3 position, bool spawnFacingLeft, List<MMPathMovementElement> pathElements, MM_Objects_SpawnerEnemy parentSpawner, int spawnCount, float initialDelay = 0f, Automator onDeathAutomator = Automator.NONE) {
		yield return new WaitForSeconds(initialDelay);

		GameObject newEnemy = MM.SceneController.GetEnemyFromPool(enemy);
		if (newEnemy != null) {
			newEnemy.transform.position = position;
			if (pathElements != null && pathElements.Count > 0) {
				AIActionFlyPatrolPath aiFlyPath = newEnemy.gameObject.MMGetComponentNoAlloc<AIActionFlyPatrolPath>();
				if (aiFlyPath != null) {
					aiFlyPath.PathElements = new List<MMPathMovementElement>(pathElements);
				}
				AIActionPatrolJump aiPatrolPath = newEnemy.gameObject.MMGetComponentNoAlloc<AIActionPatrolJump>();
				if (aiPatrolPath != null) {
					aiPatrolPath.BoundsExtentsLeft = Mathf.Abs(pathElements[0].PathElementPosition.x);
					aiPatrolPath.BoundsExtentsRight = Mathf.Abs(pathElements[1].PathElementPosition.x);
				}
				AIActionPatrolWithinBounds aiPatrolBoundsPath = newEnemy.gameObject.MMGetComponentNoAlloc<AIActionPatrolWithinBounds>();
				if (aiPatrolBoundsPath != null) {
					aiPatrolBoundsPath.BoundsExtentsLeft = Mathf.Abs(pathElements[0].PathElementPosition.x);
					aiPatrolBoundsPath.BoundsExtentsRight = Mathf.Abs(pathElements[1].PathElementPosition.x);
				}
			}
			// Revive the character
			Health objectHealth = newEnemy.gameObject.MMGetComponentNoAlloc<Health>();
			if (objectHealth != null) {
				objectHealth.Revive();
			}
			// Restore the facing direction enemy AI state
			Character character = newEnemy.GetComponent<Character>();
			character?.Face(spawnFacingLeft ? Character.FacingDirections.Left : Character.FacingDirections.Right);
			AIBrain aiBrain = newEnemy.gameObject.MMGetComponentNoAlloc<AIBrain>();
			if (aiBrain != null) {
				aiBrain.TransitionToState(aiBrain.States[0].StateName, true);
			}
			// Activate the enemy
			newEnemy.gameObject.SetActive(true);
			newEnemy.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			newEnemy.transform.GetComponentInChildren<MM_EnemyController>().Spawn(layer, parentSpawner, spawnCount, onDeathAutomator);
		}
	}

	private void _playSpawnSingleGold(int amount, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft) {
		GameObject newGold = MM.SceneController.GetGoldFromPool(amount);
		if (newGold != null) {
			newGold.transform.position = position;
			newGold.gameObject.SetActive(true);
			newGold.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			if (forceLeft != null) {
				forceLeft = _forceSpawnLeft;
				_forceSpawnLeft = !_forceSpawnLeft;
			}
			newGold.transform.GetComponentInChildren<MM_Objects_GoldCoin>().Spawn(amount, layer, startWithHorizontalMovement, forceLeft);
		}
	}

	private void _playSpawnSingleHeart(SceneAreaLayer layer, Vector3 position, int directionChangeLimit, bool startWithHorizontalMovement) {
		GameObject newHeart = MM.SceneController.GetHeartFromPool();
		if (newHeart != null) {
			newHeart.transform.position = position;
			newHeart.gameObject.SetActive(true);
			newHeart.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			newHeart.transform.GetComponentInChildren<MM_Objects_Heart>().Spawn(layer, directionChangeLimit, startWithHorizontalMovement);
		}
	}

	private void _playSpawnSinglePoints(SpawnObject spawnObject, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft, bool extraForce) {
		GameObject newPoints = MM.SceneController.GetPointsFromPool(spawnObject);
		if (newPoints != null) {
			newPoints.transform.position = position;
			newPoints.gameObject.SetActive(true);
			newPoints.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			if (forceLeft != null) {
				forceLeft = _forceSpawnLeft;
				_forceSpawnLeft = !_forceSpawnLeft;
			}
			newPoints.transform.GetComponentInChildren<MM_Objects_PointsItem>().Spawn(layer, startWithHorizontalMovement, forceLeft, extraForce);
		}
	}

	private void _playSpawnSingleAbilityItem(SpawnObject spawnObject, SceneAreaLayer layer, Vector3 position, bool startWithHorizontalMovement, bool? forceLeft) {
		GameObject abilityItem = MM.SceneController.GetAbilityItemFromPool(spawnObject);
		if (abilityItem != null) {
			abilityItem.transform.position = position;
			abilityItem.gameObject.SetActive(true);
			abilityItem.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			if (forceLeft != null) {
				forceLeft = _forceSpawnLeft;
				_forceSpawnLeft = !_forceSpawnLeft;
			}
			abilityItem.transform.GetComponentInChildren<MM_Objects_AbilityItem>().Spawn(layer, startWithHorizontalMovement, forceLeft);
		}
	}

	private IEnumerator _playSpawnSinglePowerup(MM_SceneControllerPowerupPrefabs powerup, SceneAreaLayer layer, Vector3 position, MM_Objects_SpawnerPowerup parentSpawner, SpawnFacingDirections forceFacingDirection, float initialDelay = 0f) {
		yield return new WaitForSeconds(initialDelay);

		GameObject newPowerup = MM.SceneController.GetPowerupFromPool(powerup);
		if (newPowerup != null) {
			newPowerup.transform.position = position;
			newPowerup.gameObject.SetActive(true);
			newPowerup.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			switch (powerup) {
				case MM_SceneControllerPowerupPrefabs.SKATEBOARD:
					newPowerup.transform.GetComponentInChildren<MM_Objects_Skateboard>().Spawn(layer, parentSpawner, forceFacingDirection);
					break;
				case MM_SceneControllerPowerupPrefabs.WINGED_BOOTS:
					newPowerup.transform.GetComponentInChildren<MM_Objects_WingedBoots>().Spawn(layer, parentSpawner, forceFacingDirection);
					break;
			}
		}
	}
}
