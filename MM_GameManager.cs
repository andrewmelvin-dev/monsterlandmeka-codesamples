using BayatGames.SaveGamePro;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using CielaSpike;

[Serializable]
public enum MM_Language {
	[XmlEnum(Name = "English")]
	ENGLISH,
	[XmlEnum(Name = "Japanese")]
	JAPANESE
}

// Declare the settings structure for the general game settings
[Serializable]
public class MM_GameManager_GameSettings {
	// Options
	public float volumeMusic;
	public float volumeSFX;
	public MM_Language language;
	// Save slot to use
	public int currentSaveSlot;
	public int nextAvailableSaveSlot;
	// All saved player states
	public Dictionary<int, MM_PlayerState> savedStates;

	public MM_GameManager_GameSettings() {
		volumeMusic = 0.5f;
		volumeSFX = 0.5f;
		language = MM_Language.ENGLISH;
		currentSaveSlot = 0;
		nextAvailableSaveSlot = 1;
		savedStates = new Dictionary<int, MM_PlayerState>();
	}
}

public class MM_GameManager : MonoBehaviour {

	private const string GAME_SETTINGS_KEY = "GameSettings";

	private MM_GameManager_GameSettings _gameSettings = new MM_GameManager_GameSettings();

	public GameObject Player;
	public uint? SetPlayedTimeOnLoad;
	private bool _isSaving = false;
	private bool _loadGameOnSaveComplete = false;
	private int _loadGameOnSaveCompleteSlot;

	void Start() {
		if (!SaveGame.Exists(MM_Constants.GAME_SETTINGS_FILENAME)) {
			Debug.LogWarning("MM_GameManager:Start : save file doesn't exist, creating it now");
			SaveGame.Save(MM_Constants.GAME_SETTINGS_FILENAME, _gameSettings);
		}
		// Load the general game settings
		LoadGameSettings();
	}

	// Load the general game settings
	public void LoadGameSettings() {
		_gameSettings = SaveGame.Load<MM_GameManager_GameSettings>(MM_Constants.GAME_SETTINGS_FILENAME);
		// Trigger update events for data that has been loaded
		MM.events.Trigger(MM_Event.VOLUME_MUSIC_CHANGE);
		MM.events.Trigger(MM_Event.VOLUME_SFX_CHANGE);
		Debug.Log("MM_GameManager:LoadGameSettings : loaded game settings with savedState.Keys=[" + String.Join(",", _gameSettings.savedStates.Keys.ToList().Select(i=>i.ToString()).ToArray()) + "] and currentSaveSlot=[" + _gameSettings.currentSaveSlot + "]");
	}

	// Save the general game settings to preferences
	public void SaveGameSettings() {
		SaveGame.Save(MM_Constants.GAME_SETTINGS_FILENAME, _gameSettings);
		Debug.Log("MM_GameManager:SaveGameSettings : game settings have been saved successfully");
	}

	public void CreateNewGame(int hero, int difficulty) {
		// Assign the current saveslot to the one assigned for new games
		_gameSettings.currentSaveSlot = MM_Constants.SCENE_NEW_GAME_INDEX;
		// Create a new player state to store the new game data and
		// set it to be the state used for the current game session
		MM.player = new MM_PlayerState(hero, difficulty);
	}

	public void LoadExistingGame(int saveSlot) {
		if (_isSaving) {
			_loadGameOnSaveComplete = true;
			_loadGameOnSaveCompleteSlot = saveSlot;
		} else {
			// Set the save slot to use for this game session
			_gameSettings.currentSaveSlot = (saveSlot == MM_Constants.SCENE_RELOAD_INDEX) ? _gameSettings.currentSaveSlot : saveSlot;
			if (_gameSettings.currentSaveSlot == MM_Constants.SCENE_NEW_GAME_INDEX) {
				MM.gameMenu.QuitGame();
			} else if (_gameSettings.savedStates.ContainsKey(_gameSettings.currentSaveSlot)) {
				this.StartCoroutineAsync(_loadExistingGame(saveSlot));
			} else {
				Debug.LogError("MM_GameManager:LoadExistingGame : cannot load game from save slot [" + _gameSettings.currentSaveSlot.ToString() + "]");
			}
		}
	}

	public void SaveCurrentGame() {
		// Update the current save slot and increment the next one
		_gameSettings.currentSaveSlot = _gameSettings.nextAvailableSaveSlot++;
		if (MM.player != null && !_gameSettings.savedStates.ContainsKey(_gameSettings.currentSaveSlot)) {
			this.StartCoroutineAsync(_saveCurrentGame());
		} else {
			Debug.LogError("MM_GameManager:SaveCurrentGame : MM.player property is null");
		}
	}

	// Used to lower volume of background music in certain areas
	private float _volumeModifier = 1f;
	public float VolumeModifier {
		get {
			return _volumeModifier;
		}
	}

	public void SetVolumeModifier(float volumeModifier) {
		if (volumeModifier >= 0f && volumeModifier <= 1f) {
			_volumeModifier = volumeModifier;
		}
		MM.music.ApplyCurrentVolumeModifier();
	}

	public float VolumeMusic {
		get {
			return _gameSettings.volumeMusic;
		}
		set {
			if (value >= 0f && value <= 1f) {
				_gameSettings.volumeMusic = value;
				MM.events.Trigger(MM_Event.VOLUME_MUSIC_CHANGE);
			}
		}
	}

	public float VolumeSFX {
		get {
			return _gameSettings.volumeSFX;
		}
		set {
			if (value >= 0f && value <= 1f) {
				_gameSettings.volumeSFX = value;
				MM.events.Trigger(MM_Event.VOLUME_SFX_CHANGE);
			}
		}
	}

	public MM_Language Language {
		get {
			return _gameSettings.language;
		}
	}

	public bool IsNewGame {
		get {
			return _gameSettings.currentSaveSlot == MM_Constants.SCENE_NEW_GAME_INDEX;
		}
	}

	public bool IsSavedGamesAvailable {
		get {
			return _gameSettings.savedStates.Count > 0;
		}
	}

	public int SavedGamesCount {
		get {
			return _gameSettings.savedStates.Count;
		}
	}

	public List<int> SavedGamesByDateDesc {
		get {
			List<int> orderedList = _gameSettings.savedStates.OrderByDescending(o => o.Value.LastSaveTime).Select(o => o.Key).ToList();
			return orderedList;
		}
	}

	public MM_PlayerState GetSavedGame(int slot) {
		return _gameSettings.savedStates.ContainsKey(slot) ? _gameSettings.savedStates[slot] : null;
	}

	private IEnumerator _saveCurrentGame() {
		_isSaving = true;
		// Add the current playerState into the collection of save slots using the currentSaveSlot as a key
		MM.player.PreSaveHandler();
		Debug.Log("MM_GameManager:_saveCurrentGame : saving game currentSaveSlot=[" + _gameSettings.currentSaveSlot + "] savetime=[" + MM.player.LastSaveTime.ToString() + "]");
		_gameSettings.savedStates.Add(_gameSettings.currentSaveSlot, Utilities.DeepCopy<MM_PlayerState>(MM.player));

		// Remove past saved games with the same id
		KeyValuePair<int, MM_PlayerState>[] matchingIds = _gameSettings.savedStates.Where(s => s.Value.id == MM.player.id).ToArray();
		foreach (KeyValuePair<int, MM_PlayerState> saveSlot in matchingIds) {
			if (saveSlot.Key != _gameSettings.currentSaveSlot) {
				_gameSettings.savedStates.Remove(saveSlot.Key);
				Debug.Log("MM_GameManager:_saveCurrentGame : removing duplicate save slot [" + saveSlot.Key + "] for game id [" + saveSlot.Value.id + "]");
			}
		}

		// Only keep 3 saved games in total for now
		int count = 1;
		KeyValuePair<int, MM_PlayerState>[] orderedSavedGames = _gameSettings.savedStates.OrderByDescending(s => s.Value.LastSaveTime).ToArray();
		foreach (KeyValuePair<int, MM_PlayerState> saveSlot in orderedSavedGames) {
			if (count > 3) {
				Debug.Log("MM_GameManager:_saveCurrentGame : removing expired save slot [" + saveSlot.Key + "] for game id [" + saveSlot.Value.id + "]");
				_gameSettings.savedStates.Remove(saveSlot.Key);
			}
			count++;
		}

		// Save the current game settings into preferences so they will be correct for the next session
		SaveGameSettings();
		yield return Ninja.JumpToUnity;
		MM.events.Trigger(MM_Event.SAVE_SUCCESSFUL);
		yield return Ninja.JumpBack;
		_isSaving = false;
		if (_loadGameOnSaveComplete) {
			_loadGameOnSaveComplete = false;
			LoadExistingGame(_loadGameOnSaveCompleteSlot);
		}
	}

	private IEnumerator _loadExistingGame(int saveSlot) {
		// Save the current game settings into preferences so that this newly
		// selected save slot is the one that will be selected by default next time
		SaveGameSettings();
		// Retrieve the player state from the collection and set it to be the one used for this game session
		if (_gameSettings.savedStates.ContainsKey(_gameSettings.currentSaveSlot)) {
			MM.player = Utilities.DeepCopy<MM_PlayerState>(_gameSettings.savedStates[_gameSettings.currentSaveSlot]);
			MM.player.LastLoadTime = DateTime.Now;
			if (SetPlayedTimeOnLoad != null) {
				// Update the played time if reloading after player death
				MM.player.PlayedTime = (uint)SetPlayedTimeOnLoad;
				SetPlayedTimeOnLoad = null;
			}
			Debug.Log("MM_GameManager:LoadExistingGame : loaded game currentSaveSlot=[" + _gameSettings.currentSaveSlot + "] id=[" + MM.player.id + "] savetime=[" + MM.player.LastSaveTime.ToString() + "]");
			yield return Ninja.JumpToUnity;
			MM.events.Trigger(MM_Event.STORED_GAME_LOADED);
			MM.player.OnLoadHandler();
			yield return Ninja.JumpBack;
		} else {
			Debug.LogError("MM_GameManager:LoadExistingGame : cannot load game from save slot [" + _gameSettings.currentSaveSlot.ToString() + "]");
		}
	}
}
