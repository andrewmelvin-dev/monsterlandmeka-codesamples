using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MM_CameraState {
	LOCKED,
	MOVING,
	RETURN_TO_PLAYER,
	MOVE_TO_POSITION
}

public class MM_CameraController : MonoBehaviour {

	private string _CAMERA_TARGET = "CameraTarget";

	private int _MOVING_MINIMUM_FRAMES = 2;
	private int _MOVING_MAXIMUM_FRAMES = 120;
	private float _MOVING_RANGE = 1f;
	private float _DISTANCE_RANGE = 0.0001f;

	private Camera _mainCamera;
	private CinemachineBrain _mainCameraBrain;
	private CinemachineVirtualCamera _mainCameraCM;
	private CinemachineVirtualCamera _targetCameraCM;
	private Camera _layerPlayer;
	private GameObject _layer1BackCamera;
	private GameObject _layer2BackCamera;
	private GameObject _layer3BackCamera;
	private GameObject _layer1FrontCamera;
	private GameObject _layer2FrontCamera;
	private GameObject _layer3FrontCamera;

	private GameObject _cameraTarget;
	private MM_CameraState _cameraState;

	IEnumerator _distanceCoroutine;
	private int _distanceCoroutineRunning;
	IEnumerator _movingCoroutine;
	private int _COROUTINE_MOVING = 1;
	private int _COROUTINE_RETURNING = 2;
	private int _COROUTINE_MOVING_TO_POSITION = 3;
	private int _movingCoroutineRunning;

	private void Awake() {
		_cameraTarget = new GameObject(_CAMERA_TARGET);
		DontDestroyOnLoad(_cameraTarget);
		_distanceCoroutineRunning = 0;
		_movingCoroutineRunning = 0;
	}

	private void OnEnable() {
		if (_distanceCoroutineRunning != 0) {
			StopCoroutine(_distanceCoroutine);
		}
		_distanceCoroutineRunning = 0;
		if (_movingCoroutineRunning != 0) {
			StopCoroutine(_movingCoroutine);
		}
		_movingCoroutineRunning = 0;
	}

	private void OnDisable() {
		if (_distanceCoroutineRunning != 0) {
			StopCoroutine(_distanceCoroutine);
			_distanceCoroutineRunning = 0;
		}
		if (_movingCoroutineRunning != 0) {
			StopCoroutine(_movingCoroutine);
			_movingCoroutineRunning = 0;
		}
	}

	public CinemachineBrain MainCameraBrain {
		get {
			return _mainCameraBrain;
		}
	}

	public CinemachineVirtualCamera MainCameraCM {
		get {
			return _mainCameraCM;
		}
	}

	public CinemachineVirtualCamera TargetCameraCM {
		get {
			return _targetCameraCM;
		}
	}

	public MM_CameraState CameraState {
		get {
			return _cameraState;
		}
	}

	public void ResetBlending() {
		_mainCameraBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
		_mainCameraBrain.m_CustomBlends.m_CustomBlends[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.Cut;
		_mainCameraBrain.m_CustomBlends.m_CustomBlends[2].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
	}

	public void SetSceneCameras() {
		// Setup references to the native main camera, paused camera, and cinemachine brain used in the current scene
		MM.MainCamera = Camera.main;
		_mainCamera = GameObject.Find(MM_Constants.CAMERA_MAIN).GetComponent<Camera>();
		_mainCameraBrain = _mainCamera.transform.GetComponent<CinemachineBrain>();
		MM_Graphics_Utilities.SetPausedCamera(transform);
		// Set the main camera to sort rendered objects in orthographic mode (i.e. based on distance along the camera's view)
		_mainCamera.transparencySortMode = TransparencySortMode.Orthographic;
		// Setup references to the cinemachine virtual cameras
		if (_mainCameraCM == null) {
			_mainCameraCM = transform.Find(MM_Constants.CAMERA_MAIN_CM).GetComponent<CinemachineVirtualCamera>();
		}
		if (_targetCameraCM == null) {
			_targetCameraCM = transform.Find(MM_Constants.CAMERA_TARGET_CM).GetComponent<CinemachineVirtualCamera>();
		}
		// Setup references to each layer camera
		_layerPlayer = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_PLAYER).GetComponent<Camera>();
		_layer1BackCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_1_BACK).gameObject;
		_layer2BackCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_2_BACK).gameObject;
		_layer3BackCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_3_BACK).gameObject;
		_layer1FrontCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_1_FRONT).gameObject;
		_layer2FrontCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_2_FRONT).gameObject;
		_layer3FrontCamera = _mainCamera.transform.Find(MM_Constants.CAMERA_LAYER_3_FRONT).gameObject;
	}

	public void SetSceneAreaCamerasUsed(bool cameraLayer1, bool cameraLayer2, bool cameraLayer3) {
		Debug.Log("MM_CameraController:SetSceneAreaCamerasUsed : setting active state for camera layers 1, 2, 3 to [" + cameraLayer1 + "," + cameraLayer2 + "," + cameraLayer3 + "]");
		_layer1BackCamera.SetActive(cameraLayer1);
		_layer1FrontCamera.SetActive(cameraLayer1);
		_layer2BackCamera.SetActive(cameraLayer2);
		_layer2FrontCamera.SetActive(cameraLayer2);
		_layer3BackCamera.SetActive(cameraLayer3);
		_layer3FrontCamera.SetActive(cameraLayer3);
		// Reset the player RenderTexture
		RenderTexture rt = UnityEngine.RenderTexture.active;
		UnityEngine.RenderTexture.active = _layerPlayer.targetTexture;
		GL.Clear(true, true, Color.clear);
		UnityEngine.RenderTexture.active = rt;
	}

	public void SetCinemachineConfinerBounds(Transform cameraBounds) {
		Collider2D bounds = cameraBounds.GetComponent<Collider2D>();

		MainCameraCM.PreviousStateIsValid = false;
		CinemachineConfiner main = MainCameraCM.GetComponent<CinemachineConfiner>();
		main.m_BoundingShape2D = bounds;
		main.InvalidatePathCache();

		TargetCameraCM.PreviousStateIsValid = false;
		CinemachineConfiner target = MainCameraCM.GetComponent<CinemachineConfiner>();
		target.m_BoundingShape2D = bounds;
		target.InvalidatePathCache();
	}

	public void SetSceneCameraTargets(Transform follow) {
		if (_mainCameraCM == null) {
			_mainCameraCM = transform.Find(MM_Constants.CAMERA_MAIN_CM).GetComponent<CinemachineVirtualCamera>();
		}
		_mainCameraCM.Follow = follow;
		if (_targetCameraCM == null) {
			_targetCameraCM = transform.Find(MM_Constants.CAMERA_TARGET_CM).GetComponent<CinemachineVirtualCamera>();
		}
		_targetCameraCM.Follow = null;
		_targetCameraCM.gameObject.SetActive(false);
	}

	public void LockCamera() {
		_cameraState = MM_CameraState.LOCKED;
	}

	public void SetCameraMoving(float time = 0f, bool relockOnComplete = false) {
		_cameraState = MM_CameraState.MOVING;
		_updateCameraProperties(null, 0f, 0f, time, relockOnComplete);
	}

	public void SetCameraReturnToPlayer(Vector3? position, float delay = 0f, float time = 0f) {
		_cameraState = MM_CameraState.RETURN_TO_PLAYER;
		_updateCameraProperties((Vector3)position, 0f, delay, time);
	}

	public void SetCameraMoveToPosition(Vector3? position, float delay = 0f, float time = 0f) {
		_cameraState = MM_CameraState.MOVE_TO_POSITION;
		_updateCameraProperties((Vector3)position, 0f, delay, time);
	}

	public void SetCameraDistance(float distance, float time) {
		_cameraState = MM_CameraState.MOVING;
		_updateCameraProperties(null, distance, 0f, time, true);
	}

	public void SetCameraDistanceImmediately(float distance) {
		CinemachineFramingTransposer sceneMainCameraFramingTransposer = null;
		if (MainCameraCM != null) {
			sceneMainCameraFramingTransposer = MainCameraCM.GetCinemachineComponent<CinemachineFramingTransposer>();
			sceneMainCameraFramingTransposer.m_ZDamping = 0f;
			sceneMainCameraFramingTransposer.m_CameraDistance = distance;
		}
	}

	private void _updateCameraProperties(Vector3? position = null, float distance = 0f, float delay = 0f, float time = 0f, bool relockOnComplete = false) {
		CinemachineFramingTransposer sceneMainCameraFramingTransposer = null;
		if (MainCameraCM != null) {
			sceneMainCameraFramingTransposer = MainCameraCM.GetCinemachineComponent<CinemachineFramingTransposer>();
		}

		switch (CameraState) {
			case MM_CameraState.LOCKED:
				if (sceneMainCameraFramingTransposer != null) {
					sceneMainCameraFramingTransposer.m_XDamping = MM_Constants.CAMERA_LOCKED_DAMPING_X;
					sceneMainCameraFramingTransposer.m_YDamping = MM_Constants.CAMERA_LOCKED_DAMPING_Y;
				}
				break;
			case MM_CameraState.MOVING:
				if (sceneMainCameraFramingTransposer != null) {
					sceneMainCameraFramingTransposer.m_XDamping = time == 0f ? MM_Constants.CAMERA_MOVING_DAMPING_X : time;
					sceneMainCameraFramingTransposer.m_YDamping = time == 0f ? MM_Constants.CAMERA_MOVING_DAMPING_Y : time;
					if (distance != 0f) {
						sceneMainCameraFramingTransposer.m_ZDamping = time == 0f ? MM_Constants.CAMERA_MOVING_DAMPING_Z : time;
						sceneMainCameraFramingTransposer.m_CameraDistance = distance;
						if (_distanceCoroutineRunning != 0) {
							StopCoroutine(_distanceCoroutine);
						}
						_distanceCoroutine = _cameraChangingDistance(distance, relockOnComplete);
						StartCoroutine(_distanceCoroutine);
					}
				}
				if (_movingCoroutineRunning == 0 || _movingCoroutineRunning != _COROUTINE_MOVING) {
					if (_movingCoroutineRunning != 0) {
						StopCoroutine(_movingCoroutine);
					}
					_movingCoroutine = _cameraMoving(relockOnComplete);
					StartCoroutine(_movingCoroutine);
				}
				break;
			case MM_CameraState.RETURN_TO_PLAYER:
				if (position != null) {
					Debug.Log("MM_CameraController:_updateCameraProperties : setting camera to position [" + position.ToString() + "] and returning to player at time [" + time.ToString() + "]");
					_cameraTarget.transform.localPosition = (Vector3)position;
					MainCameraCM.Follow = MM.Player.transform;
					TargetCameraCM.Follow = _cameraTarget.transform;
					// Set the blend speed to the specified value (index 0 = target > main camera blend)
					_mainCameraBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Time = time;
					_mainCameraBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
					MainCameraCM.gameObject.SetActive(true);
					TargetCameraCM.gameObject.SetActive(true);
				}
				if (_movingCoroutineRunning == 0 || _movingCoroutineRunning != _COROUTINE_RETURNING) {
					if (_movingCoroutineRunning != 0) {
						StopCoroutine(_movingCoroutine);
					}
					_movingCoroutine = _cameraReturning(delay);
					StartCoroutine(_movingCoroutine);
				}
				break;
			case MM_CameraState.MOVE_TO_POSITION:
				if (position != null) {
					Debug.Log("MM_CameraController:_updateCameraProperties : moving camera to position [" + position.ToString() + "] at time [" + time.ToString() + "]");
					// Set the target cinemachine camera to lock onto the specified position
					_cameraTarget.transform.localPosition = (Vector3)position;
					TargetCameraCM.Follow = _cameraTarget.transform;
					// Set the blend speed to the specified value (index 1 = main > target camera blend)
					_mainCameraBrain.m_CustomBlends.m_CustomBlends[1].m_Blend.m_Time = time;
					_mainCameraBrain.m_CustomBlends.m_CustomBlends[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
					TargetCameraCM.gameObject.SetActive(true);
					if (_movingCoroutineRunning == 0 || _movingCoroutineRunning != _COROUTINE_MOVING_TO_POSITION) {
						if (_movingCoroutineRunning != 0) {
							StopCoroutine(_movingCoroutine);
						}
						_movingCoroutine = _cameraMovingToPosition();
						StartCoroutine(_movingCoroutine);
					}
				}
				break;
		}
	}

	private IEnumerator _cameraMoving(bool relockOnComplete = false) {
		_movingCoroutineRunning = _COROUTINE_MOVING;

		bool finished = false;
		int _movingElapsedFrames = 0;
		// Check every frame to see if the camera has caught up with the player
		while (!finished && _movingElapsedFrames++ < _MOVING_MAXIMUM_FRAMES) {
			yield return new WaitForEndOfFrame();
			Vector3 cameraPosition = MainCameraCM.transform.localPosition,
					playerPosition = MM.Player.transform.localPosition;
			bool inRange = cameraPosition.x >= (playerPosition.x - _MOVING_RANGE) && cameraPosition.x <= (playerPosition.x + _MOVING_RANGE) &&
				cameraPosition.y >= (playerPosition.y - _MOVING_RANGE) && cameraPosition.y <= (playerPosition.y + _MOVING_RANGE);

			if (inRange && _movingElapsedFrames > _MOVING_MINIMUM_FRAMES) {
				finished = true;
			}
		}
		if (relockOnComplete == true) {
			LockCamera();
			_updateCameraProperties();
		}
		// Trigger the event indicating the camera has caught up with the player
		MM.events.Trigger(MM_Event.LOCATION_TRANSITION_CAMERA_MOVED);
		_movingCoroutineRunning = 0;
	}

	private IEnumerator _cameraChangingDistance(float distance, bool relockOnComplete = false) {
		_distanceCoroutineRunning = _COROUTINE_MOVING;

		bool finished = false;
		int _movingElapsedFrames = 0;
		// Check every frame to see if the camera has caught up with the player
		while (!finished && _movingElapsedFrames++ < _MOVING_MAXIMUM_FRAMES) {
			yield return new WaitForEndOfFrame();
			Vector3 cameraPosition = MainCameraCM.transform.localPosition;
			bool inRange = cameraPosition.z >= (distance - _DISTANCE_RANGE) && cameraPosition.z <= (distance + _DISTANCE_RANGE);
			if (inRange && _movingElapsedFrames > _MOVING_MINIMUM_FRAMES) {
				finished = true;
			}
		}
		if (relockOnComplete == true) {
			if (MainCameraCM != null) {
				CinemachineFramingTransposer sceneMainCameraFramingTransposer = MainCameraCM.GetCinemachineComponent<CinemachineFramingTransposer>();
				if (sceneMainCameraFramingTransposer != null) {
					sceneMainCameraFramingTransposer.m_ZDamping = MM_Constants.CAMERA_LOCKED_DAMPING_Z;
				}
			}
		}
		_distanceCoroutineRunning = 0;
	}

	private IEnumerator _cameraReturning(float delay = 0f) {
		if (delay > 0) {
			yield return new WaitForSecondsRealtime(delay);
		}
		_movingCoroutineRunning = _COROUTINE_RETURNING;
		yield return new WaitForEndOfFrame();
		// Disable the target camera, allowing the blend to the main camera to begin
		TargetCameraCM.gameObject.SetActive(false);

		bool finished = false;
		// Check every frame to see if the camera has caught up with the player
		while (!finished) {
			yield return new WaitForEndOfFrame();
			if (!CinemachineCore.Instance.IsLive(TargetCameraCM)) {
				finished = true;
			}
		}
		// Trigger the event indicating the camera has caught up with the player
		MM.events.Trigger(MM_Event.CAMERA_RETURNED_TO_PLAYER);
		_movingCoroutineRunning = 0;
	}

	private IEnumerator _cameraMovingToPosition() {
		_movingCoroutineRunning = _COROUTINE_MOVING_TO_POSITION;
		yield return new WaitForEndOfFrame();
		// Disable the main camera, allowing the blend to the target camera to begin
		MainCameraCM.gameObject.SetActive(false);

		bool finished = false;
		// Check every frame to see if the camera has caught up with the player
		while (!finished) {
			yield return new WaitForEndOfFrame();
			if (!CinemachineCore.Instance.IsLive(MainCameraCM)) {
				finished = true;
			}
		}
		// Trigger the event indicating the camera has caught up with the player
		MM.events.Trigger(MM_Event.CAMERA_MOVED_TO_POSITION);
		_mainCameraBrain.m_CustomBlends.m_CustomBlends[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.Cut;
		_movingCoroutineRunning = 0;
	}
}
