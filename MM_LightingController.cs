using Aura2API;
using System;
using UnityEngine;

public class MM_LightingController : MonoBehaviour {

	private Color _ENVIRONMENT_LIGHTING_AMBIENT_COLOR = Color.white;

	private Transform _currentSceneAreaLightsource;
	private bool _currentSceneAreaLightsourceChanged;

	private void Awake() {
	}

	private void OnEnable() {
	}

	private void OnDisable() {
	}

	public void SetSceneAreaLightsource(Transform lightsource) {
		_currentSceneAreaLightsource = lightsource;
		_currentSceneAreaLightsourceChanged = true;
	}

	public void ApplySceneAreaLighting(bool forceUpdate = false) {
		MM_SceneArea area = MM_Locations.SceneAreaData[MM.player.CurrentSceneArea];
		try {
			Debug.Log("MM_LightingController:ApplySceneAreaLighting : applying lighting settings for current scene area");
			if (area.lightingSkybox != LightingSkybox.NOTHING) {
				// Set the skybox
				if (MM_Constants_Lighting.SkyboxFilenames.ContainsKey(area.lightingSkybox)) {
					RenderSettings.skybox = MM.areaManager.GetLightingSkybox(area.lightingSkybox);
					LevelManager.Instance.ResetSkybox(area.skyboxRotation);
				} else {
					throw new Exception("skybox not found in current area asset bundle");
				}
			} else {
				RenderSettings.skybox = null;
			}

			// Apply the current scene area lightsource
			if (_currentSceneAreaLightsourceChanged) {
				RenderSettings.sun = _currentSceneAreaLightsource.GetComponent<Light>();
				_currentSceneAreaLightsourceChanged = false;
			}

			// Apply ambient lighting
			if (area.ambientLighting != null) {
				MM_AmbientLighting ambientLighting = (MM_AmbientLighting)area.ambientLighting;
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientLight = ambientLighting.color != null ? (Color)ambientLighting.color : _ENVIRONMENT_LIGHTING_AMBIENT_COLOR;
			} else {
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientLight = _ENVIRONMENT_LIGHTING_AMBIENT_COLOR;
			}

			// TODO: Apply environment reflections

			// TODO: Apply realtime lighting

			// TODO: Apply mixed lighting

			// TODO: Apply lightmapping settings

			// Apply fog
			if (area.fog != null) {
				MM_Fog fog = (MM_Fog)area.fog;
				RenderSettings.fogColor = fog.color;
				RenderSettings.fogMode = fog.mode;
				RenderSettings.fogStartDistance = fog.startDistance;
				RenderSettings.fogEndDistance = fog.endDistance;
				RenderSettings.fog = true;
			} else {
				RenderSettings.fog = false;
			}
		} catch (Exception ex) {
			Debug.LogError("MM_LightingController:ApplySceneAreaLighting : error loading skybox [" + area.lightingSkybox + "] exception [" + ex + "]");
		}
	}

	public string GetSkyboxFilename(LightingSkybox skybox) {
		return MM_Constants_Lighting.SkyboxFilenames.ContainsKey(skybox) ? MM_Constants_Lighting.SkyboxFilenames[skybox] : "";
	}
}
