using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MM_Objects_Chest : MM_SceneObject {

	public PersistentObject persistentObject;
	public List<PlayerItem> items = new List<PlayerItem>();
	public List<int> amounts = new List<int>();
	public bool spawnOnEnable = false;
	public Automator onActivateAutomator = Automator.NONE;

	private PersistentObjectState _state;
	private int _id;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private Rigidbody2D _rigidbody;
	private GameObject _sparkles1;
	private bool _isTriggerable;
    private SceneAreaLayer _layer;
	private int _currentLootIndex;
	private List<PlayerItem> _newPlayerItems = new List<PlayerItem>();
	private bool _isInitialised = false;

	private string _ANIMATION_IDLE = "idle";
	private string _ANIMATION_OPEN = "open";
	private string _ANIMATION_OPENED = "opened";
	private const float _ITEM_PROCESS_DELAY = 0.25f;
	private const int _DEFAULT_LOOT_AMOUNT = 1;
	private const float _SPAWN_VERTICAL_FORCE = 100000f;

	private void Start() {
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.GetChild(1).GetComponent<Animator>();
		_spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
		_rigidbody = transform.GetComponent<Rigidbody2D>();
		_sparkles1 = transform.GetChild(0).gameObject;
		if (!_isInitialised) {
			_updateState();
		}
		_updateLayer();
	}

	private void OnEnable() {
		MM_Events.OnInputPressed += _inputReceived;
		MM_Events.OnSpawnLootDisplay += _spawnLootDisplay;
		MM_Events.OnSpawnLootDisplayComplete += _spawnLootDisplayComplete;
		_id = transform.gameObject.GetInstanceID();
		_animator = transform.GetChild(1).GetComponent<Animator>();
		_spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
		_rigidbody = transform.GetComponent<Rigidbody2D>();
		_sparkles1 = transform.GetChild(0).gameObject;
		_updateState();
		_updateLayer();
		if (spawnOnEnable) {
			MM.uiEffects.Play(GraphicEffect.SPAWN_3, transform.position);
			_rigidbody.AddForce(new Vector2(0, _SPAWN_VERTICAL_FORCE));
		}
	}

	private void OnDisable() {
		MM_Events.OnInputPressed -= _inputReceived;
		MM_Events.OnSpawnLootDisplay -= _spawnLootDisplay;
		MM_Events.OnSpawnLootDisplayComplete -= _spawnLootDisplayComplete;
	}

	public override void ActivateSceneObject() {
		base.ActivateSceneObject();
	}

	public override void DeactivateSceneObject() {
		base.DeactivateSceneObject();
	}

	public SceneAreaLayer GetLayer() {
		return _layer;
	}

	public void SetLayer(bool front) {
		// Update the sprite renderer to the correct layer
		switch (LayerMask.LayerToName(gameObject.layer)) {
			case MM_Constants.LAYER_1_BACK:
			case MM_Constants.LAYER_1_FRONT:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_1_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_1_BACK;
				}
				break;
			case MM_Constants.LAYER_2_BACK:
			case MM_Constants.LAYER_2_FRONT:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_2_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_2_BACK;
				}
				break;
			case MM_Constants.LAYER_3_BACK:
			case MM_Constants.LAYER_3_FRONT:
				if (front) {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_FRONT);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_FRONT;
				} else {
					gameObject.layer = LayerMask.NameToLayer(MM_Constants.LAYER_3_BACK);
					_spriteRenderer.sortingLayerName = MM_Constants.LAYER_3_BACK;
				}
				break;
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
		SetLayer(false);
	}

	private void _updateState() {
		_isInitialised = true;

		PersistentObjectState state = PersistentObjectState.INACTIVE;
		if (MM.player.ObjectData.ContainsKey(persistentObject)) {
			state = MM.player.ObjectData[persistentObject].state;
		} else if (MM_PersistentObjects.ObjectData.ContainsKey(persistentObject)) {
			state = MM_PersistentObjects.ObjectData[persistentObject].state;
		}
		_state = state;
		switch (_state) {
			case PersistentObjectState.HIDDEN:
				// Hide the chest and sparkle elements
				_spriteRenderer.enabled = false;
				_sparkles1.SetActive(false);
				break;
			case PersistentObjectState.SPAWNING:
			case PersistentObjectState.READY:
				// Show the chest and sparkle elements
				_spriteRenderer.enabled = true;
				_sparkles1.SetActive(true);
				// Play the idle animation state
				_animator.Play(_ANIMATION_IDLE);
				break;
			case PersistentObjectState.ACTIVATING:
				// Show the chest but hide the sparkle elements
				_spriteRenderer.enabled = true;
				_sparkles1.SetActive(false);
				// Play the open animation state and chest open sound
				_animator.Play(_ANIMATION_OPEN);
				MM.soundEffects.Play(MM_SFX.CHEST1_OPEN);
				// Record that the player has now opened this chest
				MM.player.SetPersistentObjectState(persistentObject, PersistentObjectState.INACTIVE);
				// Process the contents of the chest
				StartCoroutine(_processItems());
				break;
			case PersistentObjectState.INACTIVE:
				// Show the chest but hide the sparkle elements
				_spriteRenderer.enabled = true;
				_sparkles1.SetActive(false);
				// Play the opened animation state
				_animator.Play(_ANIMATION_OPENED);
				break;
		}
	}

	private void _inputReceived(MM_Input input) {
		if (MM.Player == null) {
			return;
		}
		if (input == MM_Input.UP && _isTriggerable && _state == PersistentObjectState.READY && MM.player.CurrentSceneAreaLayer == _layer && MM.playerController.IsInputAvailable_game() && MM.Player._controller.State.IsGrounded) {
			MM.player.SetPersistentObjectState(persistentObject, PersistentObjectState.ACTIVATING);
			_updateState();
		}
	}

	private void _spawnLootDisplay(MM_EventData data) {
		if (data.sourceId == _id) {
			// Decide which spawn effect will be shown
			GameObject lootDisplayEffect = MM.prefabs.uiFX_lootDisplay1;
			if (data.playerItem != null && MM_Items.ItemData.ContainsKey((PlayerItem)data.playerItem) && MM_Items.ItemData[(PlayerItem)data.playerItem].showItemDetailsOnDiscovery) {
				lootDisplayEffect = MM.prefabs.uiFX_lootDisplay2;
			}
			// Spawn the prefab that will show the looted item rising from the chest
			GameObject lootDisplay = Instantiate(lootDisplayEffect, transform.position, Quaternion.identity) as GameObject;
			lootDisplay.transform.parent = transform;
			lootDisplay.transform.GetComponentInChildren<MM_Animator_LootDisplay>().Spawn((int)data.sourceId, (PlayerItem)data.playerItem, _layer, data.count);
		}
	}

	private void _spawnLootDisplayComplete(MM_EventData data) {
		if (data.sourceId == _id) {
			if (data.playerItem != null && data.playerItem != PlayerItem.EQUIPPED_NONE) {
				PlayerItem item = (PlayerItem)data.playerItem;
				if (MM_Items.ItemData.ContainsKey(item) && MM_Items.ItemData[item].important) {
					// If the looted item is rare then display the HUD's info dialog
					string itemName = MM.lang.GetItemName((int)item).Replace(MM.lang.GetItemDescription(MM_Constants.TEXT_PLURAL_TAG_INDEX), "");
					MM.hud.AddInfoDialog(new MM_InfoDialog(InfoDialogType.FOUND_ITEM, InfoDialogPriority.HIGH, itemName, item, (data.count != null ? (int)data.count : 1)));
				}
			}
			// Process the next item from the batch of items held in this chest
			_processNextItem();
		}
	}

	private IEnumerator _processItems() {
		yield return new WaitForSeconds(_ITEM_PROCESS_DELAY);
		_newPlayerItems = new List<PlayerItem>();
		_currentLootIndex = -1;
		_processNextItem();
	}

	private void _processNextItem() {
		_currentLootIndex++;
		if (_currentLootIndex < items.Count) {
			bool displayLoot = false;
			switch (items[_currentLootIndex]) {
				case PlayerItem.GOLD_COIN_SMALL:
					MM.spawner.SpawnGold(Random.Range(1, MM_Constants.GOLD_COIN_SMALL_MAX), _layer, transform.position, false, null);
					break;
				case PlayerItem.FIREBALL:
					MM.spawner.SpawnAbilityItem(SpawnObject.ABILITY_ITEM_FIREBALL, _layer, transform.position, false, null);
					break;
				case PlayerItem.POINTS_GOLD_WATER_JUG:
					MM.spawner.SpawnPoints(SpawnObject.GOLD_WATER_JUG, _layer, transform.position, false, null, true);
					break;
				default:
					displayLoot = true;
					break;
			}
			if (displayLoot) {
				// Add the item to the player inventory
				_addItemToPlayerInventory(items[_currentLootIndex], amounts.Contains(_currentLootIndex) ? amounts[_currentLootIndex] : _DEFAULT_LOOT_AMOUNT);
				// Decide how to process the current item being looted e.g. spawn a coin spawner or display a UI overlay
				MM.events.Trigger(MM_Event.SPAWN_LOOT_DISPLAY, new MM_EventData() {
					sceneAreaLayer = _layer,
					sourceId = _id,
					playerItem = items[_currentLootIndex]
				});
			} else {
				StartCoroutine(_delayNextLoot());
			}
		} else {
			// Now that all the items have spawned trigger the on activate automator
			if (onActivateAutomator != Automator.NONE && MM_Constants_Automators.Automators.ContainsKey(onActivateAutomator) && MM_Constants_Automators.Automators[onActivateAutomator].trigger == AutomatorTrigger.ON_OBJECT_ACTIVATE) {
				MM.automatorController.ActionAutomator(MM_Constants_Automators.Automators[onActivateAutomator]);
			}
			if (_newPlayerItems.Count > 0) {
				StartCoroutine(_addInventoryUpdatedItems());
			}
		}
	}

	private void _addItemToPlayerInventory(PlayerItem item, int amount) {
		Debug.Log("MM_Objects_Chest:_addItemToPlayerInventory: adding [" + item + "] to player inventory");

		// If the item has not yet been discovered by the player then initialise it
		int index = (int)item;
		if (index >= MM_Items.ITEM_INVENTORY_MIN && index <= MM_Items.ITEM_INVENTORY_MAX && (!MM.player.Inventory.ContainsKey(item) || MM.player.Inventory[item] == MM_Items.ITEM_NOT_FOUND)) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_ITEMS_MIN && index <= MM_Items.ITEM_ITEMS_MAX && (!MM.player.Items.ContainsKey(item) || !MM.player.Items[item])) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_WEAPONS_MIN && index <= MM_Items.ITEM_WEAPONS_MAX && (!MM.player.Weapons.ContainsKey(item) || !MM.player.Weapons[item])) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_ARMOR_MIN && index <= MM_Items.ITEM_ARMOR_MAX && (!MM.player.Armor.ContainsKey(item) || !MM.player.Armor[item])) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_SHIELDS_MIN && index <= MM_Items.ITEM_SHIELDS_MAX && (!MM.player.Shields.ContainsKey(item) || !MM.player.Shields[item])) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_BOOTS_MIN && index <= MM_Items.ITEM_BOOTS_MAX && (!MM.player.Boots.ContainsKey(item) || !MM.player.Boots[item])) {
			_newPlayerItems.Add(item);
		} else if (index >= MM_Items.ITEM_ABILITIES_MIN && index <= MM_Items.ITEM_ABILITIES_MAX && (!MM.player.Abilities.ContainsKey(item) || MM.player.Abilities[item] == MM_Items.ITEM_NOT_FOUND)) {
			_newPlayerItems.Add(item);
		}
		MM.player.AddItem(item, amount);
	}

	private IEnumerator _addInventoryUpdatedItems() {
		yield return new WaitForSeconds(MM_Constants.NEW_ITEM_NOTIFICATION_DELAY);
		foreach (PlayerItem item in _newPlayerItems) {
			MM.hud.ShowInventoryUpdatedDialog(item);
		}
		MM.soundEffects.Play(MM_SFX.MENU_TAB);
		_newPlayerItems.Clear();
	}

	private IEnumerator _delayNextLoot() {
		yield return new WaitForSeconds(_ITEM_PROCESS_DELAY);
		_processNextItem();
	}
}
