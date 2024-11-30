using E7.Introloop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CielaSpike;

public enum AreaAssetBundle {
	COMMON = 0,
	BARABORO_COAST = 1,
	DEBUG_ZONE = 2,
	START = 3,
	VALLEY_OF_PEACE = 4
}

public class MM_AreaManager : MonoBehaviour {

	public bool IsCommonAreaInitialised { get; private set; }
	public bool IsAreaInitialised { get; private set; }
	public AreaAssetBundle? CurrentAreaInitialised { get; private set; }

	private const string _AREA_BUNDLE_PREFIX = "area_";
	private const string _AREA_COMMON_BUNDLE = _AREA_BUNDLE_PREFIX + "0";

	private AssetBundle _commonBundle;
	private AssetBundle _areaBundle;

	private Dictionary<MusicalTrack, IntroloopAudio> _commonMusicalTracks = new Dictionary<MusicalTrack, IntroloopAudio>();
	private Dictionary<MusicalTrack, IntroloopAudio> _areaMusicalTracks = new Dictionary<MusicalTrack, IntroloopAudio>();
	private Dictionary<LightingSkybox, Material> _areaSkyboxes = new Dictionary<LightingSkybox, Material>();

	private void Awake() {
		IsCommonAreaInitialised = false;
		IsAreaInitialised = false;
		CurrentAreaInitialised = null;
	}

	void OnDestroy() {
		if (_commonBundle != null) {
			_commonBundle.Unload(true);
		}
		if (_areaBundle != null) {
			_areaBundle.Unload(true);
		}
	}

	public void LoadCommonBundle() {
		this.StartCoroutineAsync(_loadCommonAssetBundle());
	}

	public void LoadAreaBundle(AreaAssetBundle index, bool refreshAreaBundle = true) {
		// An area bundle will contain an area's musical tracks and skyboxes
		this.StartCoroutineAsync(_loadAreaAssetBundle(index, refreshAreaBundle));
	}

	public IntroloopAudio GetIntroloopAudioTrack(MusicalTrack track) {
		if (_commonMusicalTracks.ContainsKey(track)) {
			return _commonMusicalTracks[track];
		} else if (_areaMusicalTracks.ContainsKey(track)) {
			return _areaMusicalTracks[track];
		} else {
			Debug.LogError("MM_AreaManager:GetIntroloopAudioTrack : cannot find track [" + track + "]");
		}
		return null;
	}

	public Material GetLightingSkybox(LightingSkybox skybox) {
		if (_areaSkyboxes.ContainsKey(skybox)) {
			return _areaSkyboxes[skybox];
		} else {
			Debug.LogError("MM_AreaManager:GetLightingSkybox : cannot find skybox [" + skybox + "]");
		}
		return null;
	}

	private IEnumerator _getSceneAreaMusicFromBundle(AssetBundle bundle, AreaAssetBundle index) {
		AssetBundleRequest request;
		IntroloopAudio asset;

		// Determine the list of tracks to load from the bundle
		List<MusicalTrack> tracks = new List<MusicalTrack>();
		List<MM_SceneArea> areas = MM_Locations.SceneAreaData.Select(i => i.Value).Where(v => v.bundle.Equals(index)).ToList<MM_SceneArea>();
		for (int i = 0; i < areas.Count; i++) {
			if (areas[i].musicalTrack != null && !tracks.Contains((MusicalTrack)areas[i].musicalTrack)) {
				tracks.Add((MusicalTrack)areas[i].musicalTrack);
			}
		}
		// Load each asset in the track list
		for (int i = 0; i < tracks.Count; i++) {
			if (tracks[i] != MusicalTrack.NOTHING) {
			   request = bundle.LoadAssetAsync<IntroloopAudio>(MM.music.GetTrackFilename(tracks[i]));
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					asset = (IntroloopAudio)request.asset;
					if (!_commonMusicalTracks.ContainsKey(tracks[i])) {
						if (index == AreaAssetBundle.COMMON) {
							_commonMusicalTracks.Add(tracks[i], asset);
						} else {
							_areaMusicalTracks.Add(tracks[i], asset);
						}
					}
				}
			}
		}
		Debug.Log("MM_AreaManager:_getSceneAreaMusicFromBundle : [" + tracks.Count + "] tracks loaded from asset bundle [" + index + "]");
	}

	private IEnumerator _getCommonMusicFromBundle(List<MusicalTrack> tracks) {
		AssetBundleRequest request;
		IntroloopAudio asset;
		int count = 0;

		// Load each asset in the track list
		for (int i = 0; i < tracks.Count; i++) {
			if (tracks[i] != MusicalTrack.NOTHING && !_commonMusicalTracks.ContainsKey(tracks[i])) {
				request = _commonBundle.LoadAssetAsync<IntroloopAudio>(MM.music.GetTrackFilename(tracks[i]));
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					asset = (IntroloopAudio)request.asset;
					_commonMusicalTracks.Add(tracks[i], asset);
					count++;
				}
			}
		}
		Debug.Log("MM_AreaManager:_getCommonMusicFromBundle : [" + count + "] tracks loaded from common asset bundle");
	}

	private IEnumerator _getLightingFromBundle(AssetBundle bundle, AreaAssetBundle index) {
		AssetBundleRequest request;
		Material materialAsset;
		Texture textureAsset;

		// Determine the list of skyboxes to load from the bundle
		List<LightingSkybox> skyboxes = new List<LightingSkybox>();
		List<MM_SceneArea> areas = MM_Locations.SceneAreaData.Select(i => i.Value).Where(v => v.bundle.Equals(index)).ToList<MM_SceneArea>();
		for (int i = 0; i < areas.Count; i++) {
			LightingSkybox skybox = (LightingSkybox)areas[i].lightingSkybox;
			if (!skyboxes.Contains(skybox)) {
				skyboxes.Add(skybox);
			}
		}
		// Load each asset in the skybox list
		for (int i = 0; i < skyboxes.Count; i++) {
			if (skyboxes[i] != LightingSkybox.NOTHING) {
				// Load the skybox material
				materialAsset = null;
				request = bundle.LoadAssetAsync<Material>(MM.lightingController.GetSkyboxFilename(skyboxes[i]));
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					materialAsset = (Material)request.asset;
				}
				// Load the skybox texture
				textureAsset = null;
				request = bundle.LoadAssetAsync<Texture>(MM.lightingController.GetSkyboxFilename(skyboxes[i]));
				yield return request;
				if (request != null) {
					yield return new WaitWhile(() => request.isDone == false);
					textureAsset = (Texture)request.asset;
				}

				// Link the skybox and texture
				if (materialAsset != null && textureAsset != null) {
					switch (textureAsset.dimension) {
						case UnityEngine.Rendering.TextureDimension.Cube:
							materialAsset.SetTexture("_Tex", textureAsset);
							break;
					}
					_areaSkyboxes.Add(skyboxes[i], materialAsset);
				}

			}
		}

		Debug.Log("MM_AreaManager:_getLightingFromBundle : [" + skyboxes.Count + "] skyboxes loaded from asset bundle [" + index + "]");
	}

	private IEnumerator _loadCommonAssetBundle(bool sendEvent = true) {
		// Load the common area asset bundle
		String filepath = Path.Combine(MM.StreamingAssetsPath, _AREA_COMMON_BUNDLE);
		yield return Ninja.JumpToUnity;
		if (_commonBundle != null) {
			_commonBundle.Unload(true);
		}
		AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
		yield return Ninja.JumpBack;
		yield return bundleRequest;
		if (bundleRequest != null) {
			yield return new WaitWhile(() => bundleRequest.isDone == false);
			yield return Ninja.JumpToUnity;
			_commonBundle = bundleRequest.assetBundle;
			yield return Ninja.JumpBack;
			if (_commonBundle != null) {
				_commonMusicalTracks.Clear();
				yield return _getSceneAreaMusicFromBundle(_commonBundle, AreaAssetBundle.COMMON);
			}
		} else {
			Debug.LogError("MM_AreaManager:_loadCommonAssetBundle : failed to load common area asset bundle");
		}

		IsCommonAreaInitialised = true;
		if (sendEvent) {
			yield return Ninja.JumpToUnity;
			MM.events.Trigger(MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE);
			yield return Ninja.JumpBack;
		}
	}

	private IEnumerator _loadAreaAssetBundle(AreaAssetBundle index, bool refreshAreaBundle) {
		IsAreaInitialised = false;

		if (!IsCommonAreaInitialised) {
			yield return _loadCommonAssetBundle(false);
		}

		if (refreshAreaBundle) {
			// Load the area asset bundle
			String filepath = Path.Combine(MM.StreamingAssetsPath, _AREA_BUNDLE_PREFIX + ((int)index).ToString());
			yield return Ninja.JumpToUnity;
			if (_areaBundle != null) {
				_areaBundle.Unload(true);
			}
			AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(filepath);
			yield return Ninja.JumpBack;
			yield return bundleRequest;
			if (bundleRequest != null) {
				yield return new WaitWhile(() => bundleRequest.isDone == false);
				yield return Ninja.JumpToUnity;
				_areaBundle = bundleRequest.assetBundle;
				yield return Ninja.JumpBack;
				if (_areaBundle != null) {
					_areaMusicalTracks.Clear();
					yield return _getSceneAreaMusicFromBundle(_areaBundle, index);
					_areaSkyboxes.Clear();
					yield return _getLightingFromBundle(_areaBundle, index);
				}
			} else {
				Debug.LogError("MM_AreaManager:_loadAreaAssetBundle : failed to load area " + index + " asset bundle");
			}
		}

		// Load additional common music tracks used in the current SceneArea
		if (MM_Locations.SceneAreaData.ContainsKey(MM.player.CurrentSceneArea) && MM_Locations.SceneAreaData[MM.player.CurrentSceneArea].commonMusicalTracks.Count > 0) {
			yield return _getCommonMusicFromBundle(MM_Locations.SceneAreaData[MM.player.CurrentSceneArea].commonMusicalTracks);
		}

		IsAreaInitialised = true;
		CurrentAreaInitialised = index;
		yield return Ninja.JumpToUnity;
		MM.events.Trigger(MM_Event.AREA_BUNDLE_PRELOAD_COMPLETE);
		yield return Ninja.JumpBack;
	}
}