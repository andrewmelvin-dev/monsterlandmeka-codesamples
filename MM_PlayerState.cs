using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MM_PlayerState {

	private const int _NOT_FOUND = -1;
	private const int _EXISTS_AND_ENABLED = 1;
	private const int _EXISTS_AND_DISABLED = 0;

	public MM_PlayerState() {
		// Initialise all inventory to empty dictionaries and unequip everything
		_inventory = new Dictionary<PlayerItem, int>();
		_weapons = new Dictionary<PlayerItem, bool>();
		_armor = new Dictionary<PlayerItem, bool>();
		_shields = new Dictionary<PlayerItem, bool>();
		_boots = new Dictionary<PlayerItem, bool>();
		_items = new Dictionary<PlayerItem, bool>();
		_abilities = new Dictionary<PlayerItem, int>();
		_powers = new Dictionary<PlayerItem, int>();
		_itemAbilities = new Dictionary<PlayerItem, bool>();
		_equippedWeapon = PlayerItem.EQUIPPED_NONE;
		_equippedArmor = PlayerItem.EQUIPPED_NONE;
		_equippedShield = PlayerItem.EQUIPPED_NONE;
		_equippedBoots = PlayerItem.EQUIPPED_NONE;
		_equippedItem = PlayerItem.EQUIPPED_NONE;
		_equippedAbility = PlayerItem.EQUIPPED_NONE;

		// Initialise any other collections used in MM_PlayerState
		_exploredMapCoordinates = new List<String>();
		_exploredMapIconCoordinates = new Dictionary<string, TileType>();
		_actionedAutomators = new List<Automator>();
		_objectData = new Dictionary<PersistentObject, PersistentObjectData>();
		_lootData = new Dictionary<SceneArea, PersistentLootData>();
		_powerupActiveTimeLeft = new Dictionary<PlayerItem, float>();
	}

	public MM_PlayerState(int hero, int difficulty) : this() {
		// Only allow the following to be set in the constructor
		id = Guid.NewGuid();
		_hero = hero;
		_difficulty = difficulty;
		_points = 0;
		_health = GetHeartContainersAmount() * MM_Constants.PLAYER_HEALTH_BAR_MULTIPLIER;
		_gold = 0;

		// Setup conditions for a new game
		OnLoadHandler();
		CurrentCharacterForm = CharacterForm.NORMAL;
		CurrentScene = MM_Constants.SCENE_NEW_GAME;
		CurrentSceneArea = SceneArea.START_01;
		CurrentSceneAreaEntrance = MM_Constants.SCENE_NEW_GAME_ENTRANCE;
		CurrentSceneAreaEntranceObjectName = "";
		CurrentSceneFacingDirection = FacingDirections.Left;
		CurrentCompanion = PlayerItem.COMPANION_NONE;
		LastLoadTime = DateTime.Now;
		SetAutomator(Automator.NEW_GAME);
		IsUnderwater = false;
		// Equip the player
		switch (_hero) {
			case MM_Constants.PLAYER_BOCK:
				EnablePower(PlayerItem.POWER_MAGIC);
				break;
		}

		CurrentScenePositionX = MM_Constants.SCENE_NEW_GAME_POSITION_X;
		CurrentScenePositionY = MM_Constants.SCENE_NEW_GAME_POSITION_Y;

		AddWeapon(PlayerItem.KNIGHT_SWORD);
		AddArmor(PlayerItem.KNIGHT_ARMOR);
		AddShield(PlayerItem.KNIGHT_SHIELD);
		AddBoots(PlayerItem.KNIGHT_GREAVES);
		EquippedWeapon = PlayerItem.KNIGHT_SWORD;
		EquippedArmor = PlayerItem.KNIGHT_ARMOR;
		EquippedShield = PlayerItem.KNIGHT_SHIELD;
		EquippedBoots = PlayerItem.KNIGHT_GREAVES;
	}

	public void OnLoadHandler() {
		Debug.Log("MM_PlayerState:OnLoadHandler : setting secondary variables (point thresholds, heart containers, player stats)");
		GetPointsProgress();
		GetAttackPower(true);
		GetDefensePower(true);
		GetMobilityPower(true);
		_refreshEquippableItems();
		_refreshEquippableAbilities();
		MM.gameMenu.Initialise();
	}

	// Private properties that store values determined from other variables
	private List<int> _pointsThresholds;
	private int _heartContainers;
	private int _attackPower;
	private int _defensePower;
	private int _mobilityPower;
	private int _mobilityWalk;
	private int _mobilityJump;
	private int _mobilityClimb;
	private int _mobilitySwim;
	private int _mobilityCurrent;
	// Maintain a list of current useable items and abilities to allow quickswap functionality to work
	private List<PlayerItem> _equippableAbilities = new List<PlayerItem>();
	private List<PlayerItem> _equippableItems = new List<PlayerItem>();

	// Identifies each new game that is created so that save slots can be grouped together on the main menu continue panel
	public Guid id { get; internal set; }

	// The hero character for this playthrough (only settable through constructor)
	private int _hero;
	public int Hero {
		get {
			return _hero;
		}
	}

	// Difficulty for this playthrough (only settable through constructor)
	private int _difficulty;
	public int Difficulty {
		get {
			return _difficulty;
		}
	}

	private CharacterForm _currentCharacterForm;
	public CharacterForm CurrentCharacterForm {
		get {
			return _currentCharacterForm;
		}
		set {
			_currentCharacterForm = value;
		}
	}

	// The current scene filename
	private String _currentScene;
	public String CurrentScene {
		get {
			return _currentScene;
		}
		set {
			_currentScene = value;
		}
	}

	// A specific area within a scene
	private SceneArea _currentSceneArea;
	public SceneArea CurrentSceneArea {
		get {
			return _currentSceneArea;
		}
		set {
			_currentSceneArea = value;
		}
	}

	// The index of the last/current area entrance in use by the player
	private int _currentSceneAreaEntrance;
	public int CurrentSceneAreaEntrance {
		get {
			return _currentSceneAreaEntrance;
		}
		set {
			if (value >= 0) {
				_currentSceneAreaEntrance = value;
			}
		}
	}

	// A string representing a gameobject name that is used as an entrance instead of x/y coordinates
	private string _currentSceneAreaEntranceObjectName;
	public string CurrentSceneAreaEntranceObjectName {
		get {
			return _currentSceneAreaEntranceObjectName;
		}
		set {
			_currentSceneAreaEntranceObjectName = value;
		}
	}

	// The x position within the current scene
	private float _currentScenePositionX;
	public float CurrentScenePositionX {
		get {
			return _currentScenePositionX;
		}
		set {
			_currentScenePositionX = value;
		}
	}

	// The y position within the current scene
	private float _currentScenePositionY;
	public float CurrentScenePositionY {
		get {
			return _currentScenePositionY;
		}
		set {
			_currentScenePositionY = value;
		}
	}

	// The layer within the current scene
	private SceneAreaLayer _currentSceneAreaLayer;
	public SceneAreaLayer CurrentSceneAreaLayer {
		get {
			return _currentSceneAreaLayer;
		}
	}

	public void SetCurrentSceneAreaLayer(SceneAreaLayer layer) {
		_currentSceneAreaLayer = layer;
	}

	// The facing direction within the current scene
	private FacingDirections _currentSceneFacingDirection;
	public FacingDirections CurrentSceneFacingDirection {
		get {
			return _currentSceneFacingDirection;
		}
		set {
			_currentSceneFacingDirection = value;
		}
	}

	// A list of explored map coordinates
	private List<String> _exploredMapCoordinates;
	public List<String> ExploredMapCoordinates {
		get {
			return _exploredMapCoordinates;
		}
	}

	// A list of explored map icon coordinates
	private Dictionary<String, TileType> _exploredMapIconCoordinates;
	public Dictionary<String, TileType> ExploredMapIconCoordinates {
		get {
			return _exploredMapIconCoordinates;
		}
	}

	// A list of completed automators experienced by the player
	private List<Automator> _actionedAutomators;
	public List<Automator> ActionedAutomators {
		get {
			return _actionedAutomators;
		}
	}

	// The state (e.g. active, hidden, etc) and property collection of every persistent object encountered by the player
	private Dictionary<PersistentObject, PersistentObjectData> _objectData;
	public Dictionary<PersistentObject, PersistentObjectData> ObjectData {
		get {
			return _objectData;
		}
	}

	// The loot profile of every SceneArea encountered by the player
	private Dictionary<SceneArea, PersistentLootData> _lootData;
	public Dictionary<SceneArea, PersistentLootData> LootData {
		get {
			return _lootData;
		}
	}

	private Automator? _currentAutomator;
	public Automator? CurrentAutomator {
		get {
			return _currentAutomator;
		}
	}

	// The current companion
	private PlayerItem _currentCompanion;
	public PlayerItem CurrentCompanion {
		get {
			return _currentCompanion;
		}
		set {
			_currentCompanion = value;
		}
	}

	private Dictionary<PlayerItem, float> _powerupActiveTimeLeft;
	public Dictionary<PlayerItem, float> PowerupActiveTimeLeft {
		get {
			return _powerupActiveTimeLeft;
		}
	}

	// The screenshot of where the game was last saved
	private byte[] _saveScreenshot;
	public byte[] SaveScreenshot {
		get {
			return _saveScreenshot;
		}
		set {
			_saveScreenshot = value;
		}
	}

	// The last loaded time
	private DateTime _lastLoadTime;
	public DateTime LastLoadTime {
		get {
			return _lastLoadTime;
		}
		set {
			_lastLoadTime = value;
		}
	}

	// The last saved datetime
	private DateTime _lastSaveTime;
	public DateTime LastSaveTime {
		get {
			return _lastSaveTime;
		}
		set {
			_lastSaveTime = value;
		}
	}

	// The total played time
	private uint _playedTime;
	public uint PlayedTime {
		get {
			return _playedTime;
		}
		set {
			_playedTime = value;
		}
	}

	// Whether the player is currently underwater
	private bool _isUnderwater;
	public bool IsUnderwater {
		get {
			return _isUnderwater;
		}
		set {
			_isUnderwater = value;
		}
	}

	// Player health
	private int _health;
	public int Health {
		get {
			return _health;
		}
	}

	// Player gold
	private int _gold;
	public int Gold {
		get {
			return _gold;
		}
	}

	// Player elixirs
	public int Elixirs {
		get {
			return (_inventory.ContainsKey(PlayerItem.ELIXIR) && _inventory[PlayerItem.ELIXIR] != _NOT_FOUND) ? _inventory[PlayerItem.ELIXIR] : 0;
		}
	}

	// Player charmstones
	public int Charmstones {
		get {
			return (_inventory.ContainsKey(PlayerItem.CHARMSTONE) && _inventory[PlayerItem.CHARMSTONE] != _NOT_FOUND) ? _inventory[PlayerItem.CHARMSTONE] : 0;
		}
	}

	// Player points
	private int _points;
	public int Points {
		get {
			return _points;
		}
	}

	// Whether the player has obtained each item type
	private Dictionary<PlayerItem, int> _inventory;
	public Dictionary<PlayerItem, int> Inventory {
		get {
			return _inventory;
		}
	}

	// Whether the player has obtained each weapon type
	private Dictionary<PlayerItem, bool> _weapons;
	public Dictionary<PlayerItem, bool> Weapons {
		get {
			return _weapons;
		}
	}

	// Whether the player has obtained each armor type
	private Dictionary<PlayerItem, bool> _armor;
	public Dictionary<PlayerItem, bool> Armor {
		get {
			return _armor;
		}
	}

	// Whether the player has obtained each shield type
	private Dictionary<PlayerItem, bool> _shields;
	public Dictionary<PlayerItem, bool> Shields {
		get {
			return _shields;
		}
	}

	// Whether the player has obtained each boots type
	private Dictionary<PlayerItem, bool> _boots;
	public Dictionary<PlayerItem, bool> Boots {
		get {
			return _boots;
		}
	}

	// The currently selected item
	private PlayerItem _equippedItem;
	public PlayerItem EquippedItem {
		get {
			return _equippedItem;
		}
		set {
			int index = (int)value;
			if (_equippedItem != value && index >= MM_Items.ITEM_ITEMS_MIN && index <= MM_Items.ITEM_ITEMS_MAX) {
				_equippedItem = value;
				MM.events.Trigger(MM_Event.EQUIPPED_ITEM_UPDATED);
			}
		}
	}

	// The currently equipped weapon
	private PlayerItem _equippedWeapon;
	public PlayerItem EquippedWeapon {
		get {
			return _equippedWeapon;
		}
		set {
			int index = (int)value;
			if (_equippedWeapon != value && index >= MM_Items.ITEM_WEAPONS_MIN && index <= MM_Items.ITEM_WEAPONS_MAX) {
				_equippedWeapon = value;
				MM.events.Trigger(MM_Event.EQUIPPED_WEAPON_UPDATED);
			}
		}
	}

	// The currently equipped armor
	private PlayerItem _equippedArmor;
	public PlayerItem EquippedArmor {
		get {
			return _equippedArmor;
		}
		set {
			int index = (int)value;
			if (_equippedArmor != value && index >= MM_Items.ITEM_ARMOR_MIN && index <= MM_Items.ITEM_ARMOR_MAX) {
				_equippedArmor = value;
				MM.events.Trigger(MM_Event.EQUIPPED_ARMOR_UPDATED);
			}
		}
	}

	// The currently equipped shield
	private PlayerItem _equippedShield;
	public PlayerItem EquippedShield {
		get {
			return _equippedShield;
		}
		set {
			int index = (int)value;
			if (_equippedShield != value && index >= MM_Items.ITEM_SHIELDS_MIN && index <= MM_Items.ITEM_SHIELDS_MAX) {
				_equippedShield = value;
				MM.events.Trigger(MM_Event.EQUIPPED_SHIELD_UPDATED);
			}
		}
	}

	// The currently equipped boots
	private PlayerItem _equippedBoots;
	public PlayerItem EquippedBoots {
		get {
			return _equippedBoots;
		}
		set {
			int index = (int)value;
			if (_equippedBoots != value && index >= MM_Items.ITEM_BOOTS_MIN && index <= MM_Items.ITEM_BOOTS_MAX) {
				_equippedBoots = value;
				MM.events.Trigger(MM_Event.EQUIPPED_BOOTS_UPDATED);
			}
		}
	}

	// The currently equipped ability
	private PlayerItem _equippedAbility;
	public PlayerItem EquippedAbility {
		get {
			return _equippedAbility;
		}
		set {
			int index = (int)value;
			if (_equippedAbility != value && index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
				_equippedAbility = value;
				MM.events.Trigger(MM_Event.EQUIPPED_ABILITY_UPDATED);
			}
		}
	}

	// Counts for each useable item type
	private Dictionary<PlayerItem, bool> _items;
	public Dictionary<PlayerItem, bool> Items {
		get {
			return _items;
		}
	}

	// Counts for each ability type
	private Dictionary<PlayerItem, int> _abilities;
	public Dictionary<PlayerItem, int> Abilities {
		get {
			return _abilities;
		}
	}

	// Whether the player has obtained each power
	private Dictionary<PlayerItem, int> _powers;
	public Dictionary<PlayerItem, int> Powers {
		get {
			return _powers;
		}
	}

	// Whether the player has obtained each item ability
	private Dictionary<PlayerItem, bool> _itemAbilities;
	public Dictionary<PlayerItem, bool> ItemAbilities {
		get {
			return _itemAbilities;
		}
	}

	/*
	 * Public methods
	 */

	public void AddExploredMapCoordinate(string position) {
		if (!_exploredMapCoordinates.Contains(position)) {
			Debug.Log("MM_PlayerState:AddExploredMapCoordinate : Adding explored map position [" + position + "]");
			_exploredMapCoordinates.Add(position);
			MM.mapController.AddExploredMapCoordinate(position);
		}
	}

	public void AddExploredMapIconCoordinate(int x, int y, TileType icon) {
		string position = x.ToString() + "," + y.ToString();
		if (!_exploredMapIconCoordinates.ContainsKey(position)) {
			Debug.Log("MM_PlayerState:AddExploredMapIconCoordinate : Adding icon [" + icon + "] at explored map position [" + position + "]");
			_exploredMapIconCoordinates.Add(position, icon);
			MM.mapController.AddExploredMapCoordinate(position);
		} else {
			Debug.Log("MM_PlayerState:AddExploredMapIconCoordinate : Updating icon [" + icon + "] at explored map position [" + position + "]");
			_exploredMapIconCoordinates[position] = icon;
			MM.mapController.AddExploredMapCoordinate(position);
		}
	}

	public void SetAutomator(Automator? index) {
		_currentAutomator = index;
	}

	public void SetAutomatorActioned(Automator automator) {
		if (!_actionedAutomators.Contains(automator)) {
			_actionedAutomators.Add(automator);
		}
	}

	public void SetPowerupActiveTimeLeft(PlayerItem item, float time) {
		_powerupActiveTimeLeft[item] = time;
	}

	public void SetPersistentObjectState(PersistentObject persistentObject, PersistentObjectState state) {
		Debug.Log("MM_PlayerState:SetPersistentObjectState : Setting [" + persistentObject.ToString() + "] to [" + state.ToString() + "]");
		if (_objectData.ContainsKey(persistentObject)) {
			_objectData[persistentObject].state = state;
		} else if (MM_PersistentObjects.ObjectData.ContainsKey(persistentObject)) {
			_objectData.Add(persistentObject, new PersistentObjectData(state, new Dictionary<PersistentObjectProperty, int>(MM_PersistentObjects.ObjectData[persistentObject].properties)));
		} else {
			_objectData.Add(persistentObject, new PersistentObjectData(state));
		}
	}

	public void SetPersistentObjectProperty(PersistentObject persistentObject, PersistentObjectProperty property, int value) {
		if (_objectData.ContainsKey(persistentObject)) {
			Debug.Log("MM_PlayerState:SetPersistentObjectProperty : Setting [" + persistentObject.ToString() + "] [" + property + "] to [" + value.ToString() + "]");
			if (_objectData[persistentObject].properties.ContainsKey(property)) {
				_objectData[persistentObject].properties[property] = value;
			} else {
				_objectData[persistentObject].properties.Add(property, value);
			}
		} else if (MM_PersistentObjects.ObjectData.ContainsKey(persistentObject)) {
			_objectData.Add(persistentObject, new PersistentObjectData(MM_PersistentObjects.ObjectData[persistentObject].state, new Dictionary<PersistentObjectProperty, int>(MM_PersistentObjects.ObjectData[persistentObject].properties)));
			if (_objectData[persistentObject].properties.ContainsKey(property)) {
				_objectData[persistentObject].properties[property] = value;
			} else {
				_objectData[persistentObject].properties.Add(property, value);
			}
		} else {
			Debug.LogError("MM_PlayerState:SetPersistentObjectProperty : Object [" + persistentObject.ToString() + "] is not defined");
		}
	}

	public void SetSceneAreaLootData(SceneArea sceneArea, int? points, PlayerItem? playerItem) {
		Debug.Log("MM_PlayerState:SetSceneAreaLootData : Setting loot data for [" + sceneArea.ToString() + "]");
		if (!_lootData.ContainsKey(sceneArea)) {
			_lootData.Add(sceneArea, new PersistentLootData());
		}
		if (points != null) {
			_lootData[sceneArea].pointsAcquired += (int)points;
		}
		if (playerItem != null) {
			_lootData[sceneArea].itemsAcquired.Add((PlayerItem)playerItem);
		}
	}

	public void PreSaveHandler() {
		LastSaveTime = DateTime.Now;
		PlayedTime = PlayedTime + (uint)(DateTime.Now - LastLoadTime).TotalSeconds;
		Debug.Log("MM_PlayerState:PreSaveHandler : DateTime.Now=[" + DateTime.Now + "]");
		Debug.Log("MM_PlayerState:PreSaveHandler : _lastLoadedTime=[" + LastLoadTime + "]");
		Debug.Log("MM_PlayerState:PreSaveHandler : diff=[" + (DateTime.Now - LastLoadTime).TotalSeconds + "]");
		LastLoadTime = DateTime.Now;
		Debug.Log("MM_PlayerState:PreSaveHandler : updating playtime to [" + PlayedTime + "]");
	}

	public void CycleEquippedAbility(int direction) {
		if (_equippableAbilities.Count > 0) {
			int currentAbilityIndex = _equippableAbilities.IndexOf(MM.player.EquippedAbility);
			if (currentAbilityIndex == _NOT_FOUND) {
				currentAbilityIndex = (direction > 0) ? _NOT_FOUND : _equippableAbilities.Count;
			}
			currentAbilityIndex += direction;
			currentAbilityIndex = (currentAbilityIndex < 0) ? (_equippableAbilities.Count - 1) : ((currentAbilityIndex >= _equippableAbilities.Count) ? 0 : currentAbilityIndex);
			EquippedAbility = _equippableAbilities[currentAbilityIndex];
			MM.soundEffects.Play(MM_SFX.MENU_SWIPE);
		} else {
			MM.soundEffects.Play(MM_SFX.MENU_ERROR);
		}
	}

	public void CycleEquippedItem(int direction) {
		if (_equippableItems.Count > 0) {
			int currentItemIndex = _equippableItems.IndexOf(MM.player.EquippedItem);
			if (currentItemIndex == _NOT_FOUND) {
				currentItemIndex = (direction > 0) ? _NOT_FOUND : _equippableItems.Count;
			}
			currentItemIndex += direction;
			currentItemIndex = (currentItemIndex < 0) ? (_equippableItems.Count - 1) : ((currentItemIndex >= _equippableItems.Count) ? 0 : currentItemIndex);
			EquippedItem = _equippableItems[currentItemIndex];
			MM.soundEffects.Play(MM_SFX.MENU_SWIPE);
		} else {
			MM.soundEffects.Play(MM_SFX.MENU_ERROR);
		}
	}

	public bool IsItemEquippable(PlayerItem item) {
		return (MM_Items.ItemData.ContainsKey(item) ? MM_Items.ItemData[item].equippable : false);
	}

	public void SetHealth(int amount) {
		int maximum = GetHeartContainersAmount() * MM_Constants.PLAYER_HEALTH_BAR_MULTIPLIER;
		if (amount > maximum) {
			amount = maximum;
		} else if (amount < 0f) {
			amount = 0;
		}
		_health = amount;
	}

	public void AddHealth(int amount) {
		_health += (amount > 0) ? amount : 0;
		int maximum = GetHeartContainersAmount() * MM_Constants.PLAYER_HEALTH_BAR_MULTIPLIER;
		if (_health > maximum) {
			_health = maximum;
		}
	}

	public void RemoveHealth(int amount) {
		_health -= (amount > 0) ? amount : 0;
		if (_health < 0) {
			_health = 0;
		}
	}

	public void SetGold(int amount) {
		_gold = amount;
	}

	public void AddGold(int amount) {
		int previousGold = _gold;
		_gold += (amount > 0) ? amount : 0;
		if (_gold > MM_Constants.MAXIMUM_GOLD) {
			_gold = MM_Constants.MAXIMUM_GOLD;
		}
		if (_gold != previousGold) {
			MM.events.Trigger(MM_Event.GOLD_UPDATED);
			MM.hud.SetItemBarAddedGold(_gold - previousGold);
		}
	}

	public void RemoveGold(int amount) {
		int previousGold = _gold;
		_gold -= (amount > 0) ? amount : 0;
		if (_gold < 0) {
			_gold = 0;
		}
		if (_gold != previousGold) {
			MM.events.Trigger(MM_Event.GOLD_UPDATED);
		}
	}

	public void AddPoints(int amount) {
		_points += (amount > 0) ? amount : 0;
		if (_points > MM_Constants.MAXIMUM_POINTS) {
			_points = MM_Constants.MAXIMUM_POINTS;
		}
		SetSceneAreaLootData(CurrentSceneArea, amount, null);
	}

	public void AddPoints(PlayerItem item) {
		if (MM_Constants.ITEM_TO_POINTS_TYPE.ContainsKey(item)) {
			AddPoints(MM_Constants.POINTS_ITEMS[MM_Constants.ITEM_TO_POINTS_TYPE[item]]);
		}
	}

	public void RemovePoints(int amount) {
		_points -= (amount > 0) ? amount : 0;
		if (_points < 0) {
			_points = 0;
		}
	}

	public void AddItem(PlayerItem item, int amount) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX) {
			if (!_inventory.ContainsKey(item) || _inventory[item] == _NOT_FOUND) {
				Debug.Log("MM_PlayerState:AddItem: enabling [" + item + "] for the first time");
				EnableInventory(item);
			}
			AddInventory(item, amount);
		} else if (index >= MM_Items.ITEM_ITEMS_MIN && index <= MM_Items.ITEM_ITEMS_MAX) {
			AddUseableItem(item, true);
		} else if (index >= MM_Items.ITEM_WEAPONS_MIN && index <= MM_Items.ITEM_WEAPONS_MAX) {
			AddWeapon(item);
		} else if (index >= MM_Items.ITEM_ARMOR_MIN && index <= MM_Items.ITEM_ARMOR_MAX) {
			AddArmor(item);
		} else if (index >= MM_Items.ITEM_SHIELDS_MIN && index <= MM_Items.ITEM_SHIELDS_MAX) {
			AddShield(item);
		} else if (index >= MM_Items.ITEM_BOOTS_MIN && index <= MM_Items.ITEM_BOOTS_MAX) {
			AddBoots(item);
		} else if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
			if (!_abilities.ContainsKey(item) || _abilities[item] == _NOT_FOUND) {
				Debug.Log("MM_PlayerState:AddItem: enabling [" + item + "] for the first time");
				EnableAbility(item, true);
			}
			AddAbility(item, amount);
		}
	}

	public void EnableInventory(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX && !_inventory.ContainsKey(item)) {
			_inventory[item] = 0;
		}
	}

	public void DisableInventory(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX && _inventory.ContainsKey(item)) {
			_inventory[item] = _NOT_FOUND;
		}
	}

	public void AddInventory(PlayerItem item, int amount) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX && _inventory.ContainsKey(item) && _inventory[item] != _NOT_FOUND) {
			_inventory[item] += (amount > 0) ? amount : 0;
			switch (item) {
				case PlayerItem.ELIXIR:
					if (_inventory[item] > MM_Constants.MAXIMUM_ELIXIRS) {
						_inventory[item] = MM_Constants.MAXIMUM_ELIXIRS;
					} else {
						MM.events.Trigger(MM_Event.ELIXIRS_UPDATED);
					}
					break;
				default:
					if (_inventory[item] > MM_Constants.MAXIMUM_INVENTORY) {
						_inventory[item] = MM_Constants.MAXIMUM_INVENTORY;
					}
					break;
			}
		}
	}

	public void RemoveInventory(PlayerItem item, int amount) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX && _inventory.ContainsKey(item) && _inventory[item] != _NOT_FOUND) {
			_inventory[item] -= (amount > 0) ? amount : 0;
			if (_inventory[item] < 0) {
				_inventory[item] = 0;
			} else {
				switch (item) {
					case PlayerItem.ELIXIR:
						MM.events.Trigger(MM_Event.ELIXIRS_UPDATED);
						break;
					default:
						break;
				}
			}
		}
	}

	public void AddWeapon(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_WEAPONS_MIN && index <= MM_Items.ITEM_WEAPONS_MAX) {
			_weapons[item] = true;
			_setDefaultItemAbilities(item);
		}
	}

	public void RemoveWeapon(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_WEAPONS_MIN && index <= MM_Items.ITEM_WEAPONS_MAX) {
			_weapons[item] = false;
			_removeItemAbilities(item);
		}
	}

	public void AddArmor(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ARMOR_MIN && index <= MM_Items.ITEM_ARMOR_MAX) {
			_armor[item] = true;
			_setDefaultItemAbilities(item);
		}
	}

	public void RemoveArmor(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ARMOR_MIN && index <= MM_Items.ITEM_ARMOR_MAX) {
			_armor[item] = false;
			_removeItemAbilities(item);
		}
	}

	public void AddShield(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_SHIELDS_MIN && index <= MM_Items.ITEM_SHIELDS_MAX) {
			_shields[item] = true;
			_setDefaultItemAbilities(item);
		}
	}

	public void RemoveShield(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_SHIELDS_MIN && index <= MM_Items.ITEM_SHIELDS_MAX) {
			_shields[item] = false;
			_removeItemAbilities(item);
		}
	}

	public void AddBoots(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_BOOTS_MIN && index <= MM_Items.ITEM_BOOTS_MAX) {
			_boots[item] = true;
			_setDefaultItemAbilities(item);
		}
	}

	public void RemoveBoots(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_BOOTS_MIN && index <= MM_Items.ITEM_BOOTS_MAX) {
			_boots[item] = false;
			_removeItemAbilities(item);
		}
	}

	public void AddUseableItem(PlayerItem item, bool refreshEquippableItems = true) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ITEMS_MIN && index <= MM_Items.ITEM_ITEMS_MAX) {
			_items[item] = true;
			if (refreshEquippableItems) {
				_refreshEquippableItems();
			}
		}
	}

	public void RemoveUseableItem(PlayerItem item, bool refreshEquippableItems = true) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ITEMS_MIN && index <= MM_Items.ITEM_ITEMS_MAX) {
			_items[item] = false;
			if (refreshEquippableItems) {
				_refreshEquippableItems();
			}
		}
	}

	public void EnableAbility(PlayerItem item, bool refreshEquippableAbilities = true) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
			_abilities[item] = 0;
			if (refreshEquippableAbilities) {
				_refreshEquippableAbilities();
			}
		}
	}

	public void DisableAbility(PlayerItem item, bool refreshEquippableAbilities = true) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
			if (_abilities.ContainsKey(item)) {
				_abilities[item] = _NOT_FOUND;
			}
			if (refreshEquippableAbilities) {
				_refreshEquippableAbilities();
			}
		}
	}

	public void AddAbility(PlayerItem item, int amount) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
			if (_abilities.ContainsKey(item) && _abilities[item] != _NOT_FOUND) {
				_abilities[item] += (amount > 0) ? amount : 0;
				if (_abilities[item] > MM_Constants.MAXIMUM_ABILITIES) {
					_abilities[item] = MM_Constants.MAXIMUM_ABILITIES;
				}
				if (item == EquippedAbility) {
					MM.events.Trigger(MM_Event.EQUIPPED_ABILITY_UPDATED);
				}
			}
		}
	}

	public void RemoveAbility(PlayerItem item, int amount) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX) {
			if (_abilities.ContainsKey(item) && _abilities[item] != _NOT_FOUND) {
				_abilities[item] -= (amount > 0) ? amount : 0;
				if (_abilities[item] < 0) {
					_abilities[item] = 0;
				}
				if (item == EquippedAbility) {
					MM.events.Trigger(MM_Event.EQUIPPED_ABILITY_UPDATED);
				}
			}
		}
	}

	public void AddItemAbility(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_IA_MIN && index <= MM_Items.ITEM_IA_MAX) {
			_itemAbilities[item] = true;
		}
	}

	public void RemoveItemAbility(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_IA_MIN && index <= MM_Items.ITEM_IA_MAX) {
			_itemAbilities[item] = false;
		}
	}

	public void EnablePower(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_POWERS_MIN && index <= MM_Items.ITEM_POWERS_MAX) {
			_powers[item] = _EXISTS_AND_ENABLED;
		}
	}

	public void DisablePower(PlayerItem item) {
		int index = (int)item;
		if (index >= MM_Items.ITEM_POWERS_MIN && index <= MM_Items.ITEM_POWERS_MAX) {
			if (_powers.ContainsKey(item)) {
				_powers[item] = _EXISTS_AND_DISABLED;
			}
		}
	}

	public float GetPowerupActiveTimeLeft(PlayerItem item) {
		float time = 0f;
		switch (item) {
			case PlayerItem.POWERUP_WINGED_BOOTS:
				time = MM.player.PowerupActiveTimeLeft.ContainsKey(item) ? MM.player.PowerupActiveTimeLeft[item] : 0f;
				break;
		}
		return time;
	}

	public int GetHeartContainersAmount(bool refresh = false) {
		int amount;

		if (_pointsThresholds == null || refresh) {
			// Determine which points threshold list to use and the
			// minimum number of hearts based on the game diffifulty
			switch (_difficulty) {
				case MM_Constants.DIFFICULTY_NORMAL:
					_pointsThresholds = MM_Constants.POINTS_THRESHOLDS_NORMAL;
					amount = MM_Constants.MINIMUM_HEARTS_NORMAL;
					break;
				default:
					_pointsThresholds = MM_Constants.POINTS_THRESHOLDS_LEGENDARY;
					amount = MM_Constants.MINIMUM_HEARTS_LEGENDARY;
					break;
			}
			// Tally the number of points thresholds that have been crossed
			for (int i = 1; i < _pointsThresholds.Count; i++) {
				if (_points >= _pointsThresholds[i]) {
					amount++;
				}
			}
			_heartContainers = amount;
		}
		return _heartContainers;
	}

	public float GetPointsProgress() {
		float progress = 0f;

		if (_pointsThresholds == null) {
			GetHeartContainersAmount();
		}

		// Determine the band that the player exists within on the points scale
		// Calculate the current progression to the next points threshold
		for (int i = 0; i < _pointsThresholds.Count; i++) {
			if (i < (_pointsThresholds.Count - 1) && _points >= _pointsThresholds[i] && _points < _pointsThresholds[i + 1]) {
				// Progress = (points - base_point_threshold) / (next_point_threshold - base_point_threshold)
				progress = (_points - _pointsThresholds[i]) / (float)((_pointsThresholds[i + 1] - _pointsThresholds[i]));
			} else if (i == (_pointsThresholds.Count - 1) && _points >= _pointsThresholds[i]) {
				progress = 1f;
			}
		}
		return progress;
	}

	public int GetAttackPower(bool refresh = false) {
		if (refresh) {
			_attackPower = CalculateAttackPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _attackPower;
	}

	public int CalculateAttackPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots, bool considerRelatedEquipmentChanges = false) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;
		int total = (weaponData != null ? weaponData.attack : 0) + (armorData != null ? armorData.attack : 0) + (shieldData != null ? shieldData.attack : 0) + (bootsData != null ? bootsData.attack : 0);
		return (total >= 0 ? total : 0);
	}

	public int GetDefensePower(bool refresh = false) {
		if (refresh) {
			_defensePower = CalculateDefensePower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _defensePower;
	}

	public int CalculateDefensePower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots, bool considerRelatedEquipmentChanges = false) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;
		int total = (weaponData != null ? weaponData.defense : 0) + (armorData != null ? armorData.defense : 0) + (shieldData != null ? shieldData.defense : 0) + (bootsData != null ? bootsData.defense : 0);
		return (total >= 0 ? total : 0);
	}

	public int GetMobilityPower(bool refresh = false) {
		if (refresh) {
			_mobilityWalk = CalculateMobilityWalkPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
			_mobilityJump = CalculateMobilityJumpPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
			_mobilityClimb = CalculateMobilityClimbPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
			_mobilitySwim = CalculateMobilitySwimPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
			_mobilityCurrent = CalculateMobilityCurrentPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
			_mobilityPower = (_mobilityWalk + _mobilityJump + _mobilityClimb + _mobilitySwim + _mobilityCurrent) / 4;
		}
		return _mobilityPower;
	}

	public int GetMobilityWalkPower(bool refresh = false) {
		if (refresh) {
			_mobilityWalk = CalculateMobilityWalkPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _mobilityWalk;
	}

	public int GetMobilityJumpPower(bool refresh = false) {
		if (refresh) {
			_mobilityJump = CalculateMobilityJumpPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _mobilityJump;
	}

	public int GetMobilityClimbPower(bool refresh = false) {
		if (refresh) {
			_mobilityClimb = CalculateMobilityClimbPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _mobilityClimb;
	}

	public int GetMobilitySwimPower(bool refresh = false) {
		if (refresh) {
			_mobilitySwim = CalculateMobilitySwimPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _mobilitySwim;
	}

	public int GetMobilityCurrentPower(bool refresh = false) {
		if (refresh) {
			_mobilityCurrent = CalculateMobilityCurrentPower(EquippedWeapon, EquippedArmor, EquippedShield, EquippedBoots);
		}
		return _mobilityCurrent;
	}

	public int GetMobilityLabel() {
		MM_PlayerItem boots = (EquippedBoots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[EquippedBoots] : null;
		MM_PlayerItem armor = (EquippedArmor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[EquippedArmor] : null;
		int total = ((boots != null) ? boots.mobilityLabel : 0) + ((armor != null) ? armor.mobilityLabel : 0);
		return (total >= 0 ? total : 0);
	}

	public int CalculateMobilityPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		return (CalculateMobilityWalkPower(weapon, armor, shield, boots)
			+ CalculateMobilityJumpPower(weapon, armor, shield, boots)
			+ CalculateMobilityClimbPower(weapon, armor, shield, boots)
			+ CalculateMobilitySwimPower(weapon, armor, shield, boots)
			+ CalculateMobilityCurrentPower(weapon, armor, shield, boots)) / 4;
	}

	public int CalculateMobilityWalkPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;

		int total = (weaponData != null ? weaponData.mobilityWalk : 0) + (armorData != null ? armorData.mobilityWalk : 0) + (shieldData != null ? shieldData.mobilityWalk : 0) + (bootsData != null ? bootsData.mobilityWalk : 0);
		return (total >= 0 ? total : 0);
	}

	public int CalculateMobilityJumpPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;

		int total = (weaponData != null ? weaponData.mobilityJump : 0) + (armorData != null ? armorData.mobilityJump : 0) + (shieldData != null ? shieldData.mobilityJump : 0) + (bootsData != null ? bootsData.mobilityJump : 0);
		return (total >= 0 ? total : 0);
	}

	public int CalculateMobilityClimbPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;

		int total = (weaponData != null ? weaponData.mobilityClimb : 0) + (armorData != null ? armorData.mobilityClimb : 0) + (shieldData != null ? shieldData.mobilityClimb : 0) + (bootsData != null ? bootsData.mobilityClimb : 0);
		return (total >= 0 ? total : 0);
	}

	public int CalculateMobilitySwimPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;

		int total = (weaponData != null ? weaponData.mobilitySwim : 0) + (armorData != null ? armorData.mobilitySwim : 0) + (shieldData != null ? shieldData.mobilitySwim : 0) + (bootsData != null ? bootsData.mobilitySwim : 0);
		return (total >= 0 ? total : 0);
	}

	public int CalculateMobilityCurrentPower(PlayerItem weapon, PlayerItem armor, PlayerItem shield, PlayerItem boots) {
		MM_PlayerItem weaponData = (weapon != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[weapon] : null;
		MM_PlayerItem armorData = (armor != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[armor] : null;
		MM_PlayerItem shieldData = (shield != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[shield] : null;
		MM_PlayerItem bootsData = (boots != PlayerItem.EQUIPPED_NONE) ? MM_Items.ItemData[boots] : null;

		int total = (weaponData != null ? weaponData.mobilityCurrent : 0) + (armorData != null ? armorData.mobilityCurrent : 0) + (shieldData != null ? shieldData.mobilityCurrent : 0) + (bootsData != null ? bootsData.mobilityCurrent : 0);
		return (total >= 0 ? total : 0);
	}

	// Private methods

	private void _setDefaultItemAbilities(PlayerItem item) {
		foreach (PlayerItem itemAbility in MM_Items.ItemData[item].itemAbilities) {
			_itemAbilities[itemAbility] = MM_Items.ItemData.ContainsKey(itemAbility) ? !MM_Items.ItemData[itemAbility].locked : false;
		}
	}

	private void _removeItemAbilities(PlayerItem item) {
		foreach (PlayerItem itemAbility in MM_Items.ItemData[item].itemAbilities) {
			_itemAbilities[itemAbility] = false;
		}
	}

	private void _refreshEquippableItems() {
		List<PlayerItem> items = new List<PlayerItem>();
		_equippableItems.Clear();
		foreach (KeyValuePair<PlayerItem, bool> item in Items) {
			if (item.Value == true && MM_Items.ItemData.ContainsKey(item.Key) && MM_Items.ItemData[item.Key].equippable) {
				items.Add(item.Key);
			}
		}
		_equippableItems = items.OrderByDescending(x => (int)(x)).ToList();
	}

	private void _refreshEquippableAbilities() {
		List<PlayerItem> abilities = new List<PlayerItem>();
		_equippableAbilities.Clear();
		foreach (KeyValuePair<PlayerItem, int> ability in Abilities) {
			if (ability.Value >= 0) {
				abilities.Add(ability.Key);
			}
		}
		_equippableAbilities = abilities.OrderByDescending(x => (int)(x)).ToList();
	}
}
