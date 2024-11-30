using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MM_SceneTransition : MonoBehaviour {
	private static string LoadingScreenSceneName = "LoadingScreenQuick";

	public float ExitFadeDuration = 0.2f;
	/// the delay (in seconds) before leaving the scene when complete
	public float LoadCompleteDelay = 0f;

	protected AsyncOperation _asyncOperation;
	protected static string _sceneToLoad = "";

	/// <summary>
	/// Call this static method to load a scene from anywhere
	/// </summary>
	/// <param name="sceneToLoad">Level name.</param>
	public static void LoadScene(string sceneToLoad) {
		_sceneToLoad = sceneToLoad;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		if (LoadingScreenSceneName != null) {
			SceneManager.LoadScene(LoadingScreenSceneName);
		}
	}

	/// <summary>
	/// On Start(), we start loading the new level asynchronously
	/// </summary>
	protected virtual void Start() {
		if (_sceneToLoad != "") {
			MM.SceneController = null;
			StartCoroutine(LoadAsynchronously());
		}
	}

	/// <summary>
	/// Loads the scene to load asynchronously.
	/// </summary>
	protected virtual IEnumerator LoadAsynchronously() {
		// Start loading the new scene
		_asyncOperation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Single);
		_asyncOperation.allowSceneActivation = false;

		while (_asyncOperation.progress < 0.9f) {
			yield return null;
		}
		yield return new WaitForSecondsRealtime(LoadCompleteDelay);
		yield return new WaitForSecondsRealtime(ExitFadeDuration);

		// Switch to the new scene
		_asyncOperation.allowSceneActivation = true;
	}
}