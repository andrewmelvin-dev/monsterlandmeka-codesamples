using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PersistentLootData contains stored player loot that should not be acquired more than once. The player will store PersistentLootData
// for each SceneArea including a tally of points and items acquired in each SceneArea. Other code can check MM.player.LootData to see
// if a SceneArea has already dropped an item or if the points acquired in that SceneArea has exceeded the cap.
[Serializable]
public class PersistentLootData {
	public int pointsAcquired;
	public List<PlayerItem> itemsAcquired;
	public PersistentLootData() {
		this.pointsAcquired = 0;
		this.itemsAcquired = new List<PlayerItem>();
	}
}

// Configuration for loot dropped by enemies in various areas of the game

public enum LootType {
	NOTHING = 0,
	GOLD_SMALL = 1,
	GOLD_LARGE = 2,
	POINTS_SMALL = 3,
	POINTS_LARGE = 4,
	HEALTH_SMALL = 5,
	HEALTH_LARGE = 6,
	GROUP_1_RANDOM = 7,
	GROUP_2_RANDOM = 8,
	SPECIFIC_ITEM = 9,
	AUTOMATION = 10
}

[Serializable]
public struct LootTypeChance {
	public LootType type;
	public int percent;
}

// A SceneAreaLootProfile contains data on the min/max for loot dropped in the SceneArea: two different types of gold and points drops, each small & large
public class SceneAreaLootProfile {
	public int smallGold_min;
	public int smallGold_max;
	public int largeGold_min;
	public int largeGold_max;
	public int smallPoints_min;
	public int smallPoints_max;
	public int largePoints_min;
	public int largePoints_max;
	public List<SpawnObject> group1;
	public SceneAreaLootProfile(int smallGold_min, int smallGold_max, int largeGold_min, int largeGold_max, int smallPoints_min, int smallPoints_max, int largePoints_min, int largePoints_max, List<SpawnObject> group1) {
		this.smallGold_min = smallGold_min;
		this.smallGold_max = smallGold_max;
		this.largeGold_min = largeGold_min;
		this.largeGold_max = largeGold_max;
		this.smallPoints_min = smallPoints_min;
		this.smallPoints_max = smallPoints_max;
		this.largePoints_min = largePoints_min;
		this.largePoints_max = largePoints_max;
		this.group1 = group1;
	}
}

public static class MM_LootProfiles {

	// Details for all persistent objects in the game
	public static Dictionary<SceneArea, SceneAreaLootProfile> LootProfile { get { return _lootProfileData; } }
	private static Dictionary<SceneArea, SceneAreaLootProfile> _lootProfileData = new Dictionary<SceneArea, SceneAreaLootProfile>() {
		{ SceneArea.START_02, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_01, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_02, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_03, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_04, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_06, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
		{ SceneArea.VALLEY_OF_PEACE_07, new SceneAreaLootProfile(1, 4, 5, 8, 500, 500, 500, 500, new List<SpawnObject>() { SpawnObject.ABILITY_ITEM_FIREBALL }) },
	};

	public static int GetLootForSceneArea_goldSmallAmount(SceneArea sceneArea) {
		return _lootProfileData.ContainsKey(sceneArea) ? MM_Helper.RandomNumber(_lootProfileData[sceneArea].smallGold_min, _lootProfileData[sceneArea].smallGold_max) : 1;
	}

	public static int GetLootForSceneArea_goldLargeAmount(SceneArea sceneArea) {
		return _lootProfileData.ContainsKey(sceneArea) ? MM_Helper.RandomNumber(_lootProfileData[sceneArea].largeGold_min, _lootProfileData[sceneArea].largeGold_max) : 1;
	}

	public static int GetLootForSceneArea_pointsSmallAmount(SceneArea sceneArea) {
		return _lootProfileData.ContainsKey(sceneArea) ? MM_Helper.RandomNumber(_lootProfileData[sceneArea].smallPoints_min, _lootProfileData[sceneArea].smallPoints_max) : 1;
	}

	public static int GetLootForSceneArea_pointsLargeAmount(SceneArea sceneArea) {
		return _lootProfileData.ContainsKey(sceneArea) ? MM_Helper.RandomNumber(_lootProfileData[sceneArea].largePoints_min, _lootProfileData[sceneArea].largePoints_max) : 1;
	}

	public static SpawnObject GetPointsSpawnObject(int points) {
		SpawnObject spawnObject;
		if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLD_WATER_JUG]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLD_NECKLACE]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLDEN_SCALE]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLDEN_MIRROR]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLDEN_HARP]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else if (points <= MM_Constants.POINTS_ITEMS[PointsItemType.GOLDEN_CROWN]) {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		} else {
			spawnObject = SpawnObject.GOLD_WATER_JUG;
		}
		return spawnObject;
	}

	public static SpawnObject GetRandomGroup1(SceneArea sceneArea) {
		return _lootProfileData.ContainsKey(sceneArea) && _lootProfileData[sceneArea].group1.Count > 0 ? _lootProfileData[sceneArea].group1[MM_Helper.RandomNumber(0, _lootProfileData[sceneArea].group1.Count)] : SpawnObject.NOTHING;
	}
}
