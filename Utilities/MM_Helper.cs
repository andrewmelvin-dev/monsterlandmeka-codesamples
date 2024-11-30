using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MM_Helper {

	private static readonly System.Random random = new System.Random();
	private static readonly object syncLock = new object();

	public static int RandomNumber(int min, int max) {
		lock (syncLock) { // synchronize
			return random.Next(min, max);
		}
	}

	public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector) {
		float totalWeight = sequence.Sum(weightSelector);
		double itemWeightIndex = new System.Random().NextDouble() * totalWeight;
		float currentWeightIndex = 0;

		foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
			currentWeightIndex += item.Weight;
			// If we've hit or passed the weight we are after for this item then it's the one we want....
			if (currentWeightIndex >= itemWeightIndex)
				return item.Value;
		}
		return default(T);
	}

	public static bool IsPlayerColliding(Collider2D collider) {
		return collider.tag == "Player";
	}

	public static bool IsPlayerInteractColliding(Collider2D collider) {
		return collider.tag == "PlayerInteractCollider";
	}

	public static bool IsDamageable(Collider2D collider) {
		return collider.tag == "SceneObject";
	}

	public static bool IsPlatformCollision(Collision2D collision, float verticalVelocity) {
		string collisionTag = collision.gameObject.tag;
		return (collisionTag == "Platform" || collisionTag == "MovingPlatform" || ((collisionTag == "OneWayPlatform" || collisionTag == "MovingOneWayPlatform") && verticalVelocity <= 0));
	}

	public static Dictionary<PersistentObjectProperty, int> GetPersistentObjectProperties(PersistentObject? persistentObject) {
		if (persistentObject != null && persistentObject != PersistentObject.NOTHING) {
			if (MM.player.ObjectData.ContainsKey((PersistentObject)persistentObject)) {
				return MM.player.ObjectData[(PersistentObject)persistentObject].properties;
			} else if (MM_PersistentObjects.ObjectData.ContainsKey((PersistentObject)persistentObject)) {
				return MM_PersistentObjects.ObjectData[(PersistentObject)persistentObject].properties;
			}
		}
		return new Dictionary<PersistentObjectProperty, int>();
	}

	public static SceneAreaLayer GetObjectLayer(int objectLayer) {
		SceneAreaLayer sceneAreaLayer = SceneAreaLayer.NONE;
		if (objectLayer == MM_Constants.LAYER_1_BACK_INT || objectLayer == MM_Constants.LAYER_1_FRONT_INT || objectLayer == MM_Constants.LAYER_1_PLATFORMS_INT) {
			sceneAreaLayer = SceneAreaLayer.LAYER_1;
		} else if (objectLayer == MM_Constants.LAYER_2_BACK_INT || objectLayer == MM_Constants.LAYER_2_FRONT_INT || objectLayer == MM_Constants.LAYER_2_PLATFORMS_INT) {
			sceneAreaLayer = SceneAreaLayer.LAYER_2;
		} else if (objectLayer == MM_Constants.LAYER_3_BACK_INT || objectLayer == MM_Constants.LAYER_3_FRONT_INT || objectLayer == MM_Constants.LAYER_3_PLATFORMS_INT) {
			sceneAreaLayer = SceneAreaLayer.LAYER_3;
		} else if (objectLayer == MM_Constants.LAYER_PLAYER_INT) {
			sceneAreaLayer = MM.player.CurrentSceneAreaLayer;
		}
		return sceneAreaLayer;
	}
}
