using CielaSpike;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum MM_SFX {
	NOTHING = 0,

	// Pool 0: menu

	MENU_CLOSE = 10,
	MENU_MOVE = 11,
	MENU_SELECT = 12,
	MENU_CREDIT = 13,
	MENU_ERROR = 14,
	MENU_OPEN = 15,
	MENU_TAB = 16,
	MENU_SWIPE = 17,
	MENU_PURCHASE = 18,

	DIALOG_APPEAR = 30,
	DIALOG_CHARACTER = 31,
	DIALOG_PURCHASE = 32,

	LOOT1_DISPLAY = 40,
	LOOT2_DISPLAY = 41,

	FANFARE1 = 50,

	COIN_SPAWN = 101,
	BAG_SPAWN = 102,
	SACK_SPAWN = 103,
	FLYING_SPAWN = 104,
	POINTS1_SPAWN = 105,

	OBJECT1_SPAWN = 111,
	OBJECT2_SPAWN = 112,

	// Pool 1: receive something

	COIN_RECEIVE = 201,
	BAG_RECEIVE = 202,
	SACK_RECEIVE = 203,
	HEART_RECEIVE = 204,
	HEART_FULL_RECOVERY = 205,
	ITEM1_RECEIVE = 206,
	ITEM2_RECEIVE = 207,
	SKATEBOARD_RECEIVE = 210,
	WINGEDBOOTS_RECEIVE = 211,

	// Pool 2: environment / damage

	DOOR1_OPEN = 301,
	DOOR1_CLOSE = 302,
	DOOR2_OPEN = 303,
	DOOR2_CLOSE = 304,
	GATE1_OPEN = 305,
	GATE2_OPEN = 306,
	DOOR_OPEN_LOCK = 310,
	SECRET_REVEAL = 311,
	HIDDEN_ITEM_REVEAL = 312,
	SWITCH_ACTIVATE = 313,
	SWITCH_FAIL = 314,

	CHEST1_OPEN = 320,

	REJECTION1 = 331,

	BOCK_SIGH = 341,
	BOCK_DRINK1 = 351,
	BOCK_HEAL1 = 361,
	BOCK_HEAL2 = 362,

	BOCK_SWING1 = 371,

	BOCK_DEATH = 400,
	BOCK_DAMAGED = 401,

	ENEMY_DAMAGE = 501,
	ENEMY_STUN_BOING = 502,

	SPRING_RELEASE = 601,
	SPLASH_WATER = 602,
	SAVE_POINT = 603,

	SKATEBOARD_LEAPER = 611,
	SKATEBOARD_BOUNCE = 612,

	EXPLOSION1 = 701,
	EXPLOSION2 = 702,
	EXPLOSION3 = 703,

	EXPLOSION1_LARGE = 711,

	BREAKWOODEN1 = 721,
	BREAKWOODEN2 = 722,
	BREAKWOODEN3 = 723,

	PUSHABLE_ROCK = 731,

	IMPACT_ROCK = 741,

	PORTAL1 = 801,

	ENVIRONMENT_BACKGROUND_RIVER1 = 901,
	ENVIRONMENT_BACKGROUND_WATERFALL1 = 906,

	// Pool 3: actions

	JUMP_NORMAL = 1001,

	SWORD = 1101,

	FIREBALL_CAST = 1201,
	FIREBALL_EXPLODE = 1202,
	FIRESTORM_THROW = 1203,
	FIRESTORM_EXPLODE = 1204,
	WINDSPOUT_THROW = 1205,
	TORNADO_THROW = 1206,
	BOMB_THROW = 1207,
	BOMB_EXPLODE = 1208,
	THUNDERFLASH_THROW = 1209,
	THUNDERFLASH_EXPLODE = 1210,
	LIGHTNING_THROW = 1211,
	LIGHTNING_EXPLODE = 1212,

	BOSS_DEATH_MASTER_HISS = 2001,
	BOSS_DEATH_MASTER_DEATH = 2002,
}

public enum SFX_Pool {
	MENU,
	RECEIVE,
	ENVIRONMENT,
	ACTION
}

public class MM_Audio_SFXData {
	public AudioClip clip;
	public bool isBasicSet = false;
	public string path;
	public SFX_Pool pool = 0;
	public bool useDedicatedAudioSource = false;
}

public class MM_Audio_SFX : MonoBehaviour {

	private AssetBundle _sfxBundle;
	public AudioSource audioSource;
	public AudioSource environmentAudioSource;
	public AudioSource audioSourceDedicated;

	private const string RESOURCES_SFX_PATH = "SFX/";
	private const string _SFX_BUNDLE = "sfx";
	private const float _ENVIRONMENT_VOLUME_SCALE = 0.6f;
	private const float _IDENTICAL_SFX_COOLDOWN = 0.01f;

	protected Dictionary<MM_SFX, MM_Audio_SFXData> _audioClips = new Dictionary<MM_SFX, MM_Audio_SFXData> {
		// Define the SFX that exist in the basic set which is needed for the main menu
		{ MM_SFX.MENU_CLOSE, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_close", isBasicSet = true } },
		{ MM_SFX.MENU_MOVE, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_move", isBasicSet = true } },
		{ MM_SFX.MENU_SELECT, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_select", isBasicSet = true } },
		{ MM_SFX.MENU_CREDIT, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_credit", isBasicSet = true } },
		{ MM_SFX.MENU_ERROR, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "WB4-DialogError", isBasicSet = true } },

		// Define the SFX that will be loaded asynchronously from the asset bundle
		// The full path is not needed for these SFX, as only the unique name is used to reference each clip
		{ MM_SFX.MENU_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_open"} },
		{ MM_SFX.MENU_TAB, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_tab"} },
		{ MM_SFX.MENU_SWIPE, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Menu_swipe"} },
		{ MM_SFX.MENU_PURCHASE, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "SH2-DialogPurchase"} },
		{ MM_SFX.DIALOG_APPEAR, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Dialog_appear"} },
		{ MM_SFX.DIALOG_CHARACTER, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "WB2-DialogCharacter"} },
		{ MM_SFX.DIALOG_PURCHASE, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Dialog_purchase"} },

		{ MM_SFX.LOOT1_DISPLAY, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Loot1_display"} },
		{ MM_SFX.LOOT2_DISPLAY, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Loot2_display"} },
		{ MM_SFX.FANFARE1, new MM_Audio_SFXData() { pool = SFX_Pool.MENU, path = "Fanfare1"} },

		{ MM_SFX.COIN_SPAWN, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "WB4-CoinSpawn"} },
		{ MM_SFX.FLYING_SPAWN, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Flying_spawn"} },
		{ MM_SFX.POINTS1_SPAWN, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Points1_spawn"} },
		{ MM_SFX.OBJECT1_SPAWN, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Object1_spawn"} },
		{ MM_SFX.OBJECT2_SPAWN, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Object2_spawn"} },
		{ MM_SFX.COIN_RECEIVE, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Coin_receive"} },
		{ MM_SFX.ITEM1_RECEIVE, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Item1_receive"} },
		{ MM_SFX.ITEM2_RECEIVE, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Item2_receive"} },
		{ MM_SFX.SKATEBOARD_RECEIVE, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "Skateboard_receive"} },
		{ MM_SFX.WINGEDBOOTS_RECEIVE, new MM_Audio_SFXData() { pool = SFX_Pool.RECEIVE, path = "WingedBoots_receive"} },

		{ MM_SFX.DOOR1_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Door1_open"} },
		{ MM_SFX.DOOR1_CLOSE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Door1_close"} },
		{ MM_SFX.DOOR2_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Door2_open"} },
		{ MM_SFX.DOOR2_CLOSE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Door2_close"} },
		{ MM_SFX.GATE1_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Gate1_open"} },
		{ MM_SFX.GATE2_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Gate2_open"} },
		{ MM_SFX.DOOR_OPEN_LOCK, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Door_openlock"} },
		{ MM_SFX.SECRET_REVEAL, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "SecretReveal"} },
		{ MM_SFX.HIDDEN_ITEM_REVEAL, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "HiddenItemReveal"} },
		{ MM_SFX.SWITCH_ACTIVATE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Switch_activate"} },
		{ MM_SFX.SWITCH_FAIL, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Switch_fail"} },
		{ MM_SFX.CHEST1_OPEN, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Chest1_open"} },

		{ MM_SFX.REJECTION1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Rejection1"} },
		{ MM_SFX.BOCK_SIGH, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Bock_sigh"} },
		{ MM_SFX.BOCK_DRINK1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Bock_drink1"} },
		{ MM_SFX.BOCK_HEAL1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Bock_heal1"} },
		{ MM_SFX.BOCK_HEAL2, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Bock_heal2", useDedicatedAudioSource = true} },

		{ MM_SFX.BOCK_SWING1, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Bock_swing"} },
		{ MM_SFX.BOCK_DEATH, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Bock_death"} },
		{ MM_SFX.BOCK_DAMAGED, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Bock_damaged"} },

		{ MM_SFX.ENEMY_DAMAGE, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "WB4-EnemyDamage"} },
		{ MM_SFX.ENEMY_STUN_BOING, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Stun_boing"} },		
		{ MM_SFX.SPRING_RELEASE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Spring1"} },
		{ MM_SFX.SPLASH_WATER, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "SplashWater"} },
		{ MM_SFX.SAVE_POINT, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Savepoint1"} },
		{ MM_SFX.SKATEBOARD_LEAPER, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "SkateboardLeaper"} },
		{ MM_SFX.SKATEBOARD_BOUNCE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Bock-Skateboard_bounce"} },		
		{ MM_SFX.EXPLOSION1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Explosion1"} },
		{ MM_SFX.EXPLOSION2, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Explosion2"} },
		{ MM_SFX.EXPLOSION3, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Explosion3"} },
		{ MM_SFX.EXPLOSION1_LARGE, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Explosion1Large"} },
		{ MM_SFX.BREAKWOODEN1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "BreakWooden1"} },
		{ MM_SFX.BREAKWOODEN2, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "BreakWooden2"} },
		{ MM_SFX.BREAKWOODEN3, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "BreakWooden3"} },
		{ MM_SFX.PUSHABLE_ROCK, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Pushable_rock"} },
		{ MM_SFX.IMPACT_ROCK, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Impact_rock"} },
		{ MM_SFX.PORTAL1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "Portal1"} },
		{ MM_SFX.ENVIRONMENT_BACKGROUND_RIVER1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "EnvironmentBackgroundRiver1"} },
		{ MM_SFX.ENVIRONMENT_BACKGROUND_WATERFALL1, new MM_Audio_SFXData() { pool = SFX_Pool.ENVIRONMENT, path = "EnvironmentBackgroundWaterfall1"} },

		{ MM_SFX.JUMP_NORMAL, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Bock_jump"} },
		{ MM_SFX.SWORD, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Sword1_whoosh"} },
		{ MM_SFX.FIREBALL_CAST, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Fireball_cast"} },
		{ MM_SFX.FIREBALL_EXPLODE, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "Fireball_explode"} },

		{ MM_SFX.BOSS_DEATH_MASTER_HISS, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "BossDeathMaster_hiss"} },
		{ MM_SFX.BOSS_DEATH_MASTER_DEATH, new MM_Audio_SFXData() { pool = SFX_Pool.ACTION, path = "BossDeathMaster_death"} },

	};

	// The maximum size of each audio pool. Different sound effects are assigned to different pools, and if
	// the pool is full then new sound effects will not be played. The purpose of the pools is to stop sounds
	// overlapping each other and creating too much noise for the player e.g. if too many coin spawn sound
	// effects play at the same time it may sound more like bells or ringing which is not a desired effect.
	private int[] poolMaxSizes = { 4, 8, 10, 8 }; // Total of 32 (excluding music + positional sfx audio sources)

	// Current size of each audio pool
	private int[] poolSizes = { 0, 0, 0, 0 };

	public bool IsBasicSetInitialised { get; private set; }
	public bool IsFullSetInitialised { get; private set; }
	private bool isEnvironmentPlaying = false;

	private Dictionary<MM_SFX, float> _sfxBuffer;
	private float _environmentSFXMultiplier = 1f;
	private float _currentVolume;

	void Awake() {
		IsBasicSetInitialised = false;
		IsFullSetInitialised = false;
		_sfxBuffer = new Dictionary<MM_SFX, float>();
	}

	void OnDestroy() {
		if (_sfxBundle != null) {
			_sfxBundle.Unload(true);
		}
	}

	private void OnEnable() {
		MM_Events.OnVolumeSFXChange += SetVolume;
	}

	private void OnDisable() {
		MM_Events.OnVolumeSFXChange -= SetVolume;
	}

	private AudioClip _getClip(MM_SFX sfx) {
		AudioClip clip = _audioClips.ContainsKey(sfx) ? _audioClips[sfx].clip : null;
		if (clip == null) {
			Debug.LogError("MM_Audio_SFX:_getClip : clip " + sfx + " not found");
		}
		return clip;
	}

	private IEnumerator _removeClipFromPool(int pool, float time) {
		yield return new WaitForSecondsRealtime(time);
		poolSizes[pool] = poolSizes[pool] - 1;
	}

	public void PreloadBasicSet() {
		// Synchronously set up the small set to SFX clips needed for the main menu
		if (!IsBasicSetInitialised) {
			IsBasicSetInitialised = true;
			// Preload each audio clip used in the game
			foreach (KeyValuePair<MM_SFX, MM_Audio_SFXData> sfx in _audioClips) {
				if (sfx.Value.isBasicSet && sfx.Value.path.Length > 0) {
					sfx.Value.clip = (AudioClip)Resources.Load(RESOURCES_SFX_PATH + sfx.Value.path);
				}
			}
			MM.events.Trigger(MM_Event.SFX_BASIC_PRELOAD_COMPLETE);
		}
	}

	public void PreloadFullSet() {
		// Load the basic set if it hasn't been already loaded
		// This will happen during development when the main menu scene isn't used
		if (!IsBasicSetInitialised) {
			PreloadBasicSet();
		}
		
		// Asynchronously load all SFX clips used in the game
		if (!IsFullSetInitialised) {
			IsFullSetInitialised = true;
			this.StartCoroutineAsync(_loadAssetBundle());
		}
	}

	public void Play(MM_SFX effect, bool realtime = false, float delay = 0f, float volumeScale = 1f) {
		if (delay > 0) {
			if (realtime) {
				StartCoroutine(_delayPlayRealtime(effect, delay, volumeScale));
			} else {
				StartCoroutine(_delayPlay(effect, delay, volumeScale));
			}
		} else {
			_playSFX(effect, volumeScale);
		}
	}

	public void PlayEnvironment(MM_SFX effect, bool loop = true, float delay = 0f, float volumeScale = 1f) {
		if (delay > 0) {
			StartCoroutine(_delayPlayEnvironment(effect, loop, delay, volumeScale));
		} else {
			_playEnvironmentSFX(effect, loop, volumeScale);
		}
	}

	public void Stop() {
		// Stop all playing sounds
		audioSource.Stop();
		environmentAudioSource.Stop();
		audioSourceDedicated.Stop();
		isEnvironmentPlaying = false;
	}

	public void StopEnvironment() {
		environmentAudioSource.Stop();
		isEnvironmentPlaying = false;
	}

	public void StopDedicated() {
		audioSourceDedicated.Stop();
	}

	public void SetVolume(float volume) {
		_currentVolume = volume;
		audioSource.volume = volume;
		environmentAudioSource.volume = volume * _ENVIRONMENT_VOLUME_SCALE * _environmentSFXMultiplier;
		audioSourceDedicated.volume = volume;
	}

	public void SetEnvironmentSFXMultiplier(float multiplier) {
		_environmentSFXMultiplier = multiplier;
		environmentAudioSource.volume = _currentVolume * _ENVIRONMENT_VOLUME_SCALE * _environmentSFXMultiplier;
	}

	public bool IsEnvironmentPlaying() {
		return isEnvironmentPlaying;
	}

	private IEnumerator _loadAssetBundle() {
		AssetBundleCreateRequest bundleRequest;
		AssetBundleRequest request;
		string filepath;
		int count = 0;

		// Load the SFX bundle
		filepath = Path.Combine(MM.StreamingAssetsPath, _SFX_BUNDLE);
		yield return Ninja.JumpToUnity;
		bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_sfxBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_sfxBundle != null) {
				// Preload each audio clip used in the game, apart from
				// the basic set which is loaded from the Resources folder
				foreach (KeyValuePair<MM_SFX, MM_Audio_SFXData> sfx in _audioClips) {
					if (sfx.Value.clip == null && sfx.Value.path.Length > 0) {
						if (!sfx.Value.isBasicSet) {
							yield return Ninja.JumpToUnity;
							request = _sfxBundle.LoadAssetAsync<AudioClip>(sfx.Value.path);
							yield return Ninja.JumpBack;
							yield return new WaitWhile(() => request.isDone == false);
							yield return Ninja.JumpToUnity;
							sfx.Value.clip = (AudioClip)request.asset;
							yield return Ninja.JumpBack;
							count++;
						}
					}
				}
				Debug.Log("MM_Audio_SFX:_loadAssetBundle : initialisation of " + count.ToString() + " clips complete");
				yield return Ninja.JumpToUnity;
				MM.events.Trigger(MM_Event.SFX_FULL_PRELOAD_COMPLETE);
			}
		} else {
			Debug.LogError("MM_Audio_SFX:_loadAssetBundle : failed to load SFX asset bundle");
		}
		yield return Ninja.JumpBack;
	}

	private void _playSFX(MM_SFX effect, float volumeScale) {
		if (effect != MM_SFX.NOTHING) {
			// Determine the "pool" that the sound effect falls into, and then play it if a slot is available
			if (_audioClips.ContainsKey(effect)) {
				SFX_Pool pool = _audioClips[effect].pool;

				if (_audioClips[effect].useDedicatedAudioSource) {
					audioSourceDedicated.clip = _getClip(effect);
					audioSourceDedicated.volume = audioSource.volume * volumeScale;
					audioSourceDedicated.Play();
				} else {
					if (pool >= 0 && poolSizes[(int)pool] < poolMaxSizes[(int)pool]) {
						AudioClip clip = _getClip(effect);
						if (clip != null) {
							if (_sfxBuffer.ContainsKey(effect) && _sfxBuffer[effect] < Time.unscaledTime) {
								_sfxBuffer.Remove(effect);
							}
							if (!_sfxBuffer.ContainsKey(effect)) {
								//Debug.Log("MM_Audio_SFX:Play : playing " + effect + " pool=[" + pool.ToString() + "] poolSize=[" + poolSizes[(int)pool] + "]");
								poolSizes[(int)pool] = poolSizes[(int)pool] + 1;
								audioSource.PlayOneShot(clip, volumeScale);
								StartCoroutine(_removeClipFromPool((int)pool, clip.length));
								_sfxBuffer.Add(effect, Time.unscaledTime + _IDENTICAL_SFX_COOLDOWN);
							}
						}
					} else {
						Debug.LogWarning("MM_Audio_SFX:Play : cannot play " + effect + " pool=[" + pool.ToString() + "] poolSize=[" + poolSizes[(int)pool] + "]");
					}
				}
			} else {
				Debug.LogError("MM_Audio_SFX:Play : effect " + effect + " is not defined");
			}
		}
	}

	private void _playEnvironmentSFX(MM_SFX effect, bool loop, float volumeScale) {
		if (effect != MM_SFX.NOTHING) {
			if (_audioClips.ContainsKey(effect)) {
				AudioClip clip = _getClip(effect);
				if (clip != null) {
					environmentAudioSource.Stop();
					environmentAudioSource.loop = loop;
					environmentAudioSource.clip = clip;
					environmentAudioSource.Play();
					isEnvironmentPlaying = true;
				}
			} else {
				Debug.LogError("MM_Audio_SFX:Play : effect " + effect + " is not defined");
			}
		}
	}

	private IEnumerator _delayPlay(MM_SFX effect, float delay, float volumeScale) {
		yield return new WaitForSeconds(delay);
		_playSFX(effect, volumeScale);
	}

	private IEnumerator _delayPlayRealtime(MM_SFX effect, float delay, float volumeScale) {
		yield return new WaitForSecondsRealtime(delay);
		_playSFX(effect, volumeScale);
	}

	private IEnumerator _delayPlayEnvironment(MM_SFX effect, bool loop, float delay, float volumeScale) {
		yield return new WaitForSeconds(delay);
		_playEnvironmentSFX(effect, loop, volumeScale);
	}
}
