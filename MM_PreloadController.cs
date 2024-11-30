using UnityEngine;

public class MM_PreloadController : MonoBehaviour {

	void Awake() {
		Cursor.visible = false;
		DontDestroyOnLoad(this.gameObject);
	}

	void Start() {
		// The preload scene should be loaded first. In order to do this, scenes need to explicitly check for the root _app gameobject
		UnityEngine.SceneManagement.SceneManager.LoadScene(MM_Constants.SCENE_DEFAULT);
		return;

		// Shortcut to directly enter new game:
		MM.gameManager.CreateNewGame(MM_Constants.PLAYER_BOCK, MM_Constants.DIFFICULTY_NORMAL);
		MM.player.SetAutomator(null);
		MM.player.CurrentSceneArea = SceneArea.START_01;
		MM.player.CurrentSceneArea = SceneArea.VALLEY_OF_PEACE_04;
		MM.player.CurrentSceneAreaEntrance = 1;
		UnityEngine.SceneManagement.SceneManager.LoadScene(MM_Constants.SCENE_LOADING);
	}
}
