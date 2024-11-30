using System.Collections.Generic;
using UnityEngine;

public static class MM_Constants {
	public const float TIMESCALE_NORMAL						= 1f;

	public const int DIRECTION_FORWARD						= 1;
	public const int DIRECTION_BACKWARD						= -1;

	public const string	SCENE_DEFAULT							= "MainMenu";
	public const string SCENE_LOADING							= "LoadingScreen";
	public const string	SCENE_NEW_GAME						= "Scene1Start";
	public const string SCENE_STORY_INTRO					= "StoryIntro";
	public const string GAME_SETTINGS_FILENAME		= "GameSettings.txt";
	public const int SCENE_NEW_GAME_INDEX					= -1;
	public const int SCENE_AREA_INVALID						= -1;

	public const string PLAYER_DEBUG							= "/_Debug/Bock";
	public const string PLAYER_NAME								= "Bock";
	public const string GAMEOBJECT_AREA_CONTAINER	= "/AreaContainer";
	public const string GAMEOBJECT_PLAYER_MANAGER	= "/_GameManagement/PlayerManager";
	public const string GAMEOBJECT_LEVEL_START		= "Setup/LevelStart";
	public const string GAMEOBJECT_AREA_OBJECTS 	= "Main/Objects";
	public const string PLAYER_NORMAL							= "PlayerCharacter_normal";

	public const float FADE_DURATION								= 1f;
	public const float SCENE_TRANSITION_DURATION		= 1f;
	public const float TEXT_CHAR_ADDITION_DURATION	= 0.005f;
	public const float CURSOR_TWEEN_DURATION				= 0.2f;
	public const float CURSOR_DIRECTION_HELD_CHECK	= 0.25f;
	public const float VOLUME_MUSIC_MAX							= 1f;
	public const float VOLUME_MUSIC_QUIET_AREA			= 0.5f;
	public const float NEW_ITEM_NOTIFICATION_DELAY	= 3.25f;

	public const string CAMERA_MAIN							= "/_Cameras/MainCamera";
	public const string CAMERA_MAIN_CM					= "/_Cameras/MainCamera_CM";
	public const string CAMERA_TARGET_CM				= "/_Cameras/TargetCamera_CM";
	public const string CAMERA_PAUSED						= "/_Cameras/PausedCamera";
	public const string CAMERA_LAYER_PLAYER			= "LayerPlayer_Camera";
	public const string CAMERA_LAYER_1_BACK			= "Layer1_backCamera";
	public const string CAMERA_LAYER_2_BACK			= "Layer2_backCamera";
	public const string CAMERA_LAYER_3_BACK			= "Layer3_backCamera";
	public const string CAMERA_LAYER_1_FRONT		= "Layer1_frontCamera";
	public const string CAMERA_LAYER_2_FRONT		= "Layer2_frontCamera";
	public const string CAMERA_LAYER_3_FRONT		= "Layer3_frontCamera";

	public const float CAMERA_LOCKED_DAMPING_X	= 0;
	public const float CAMERA_LOCKED_DAMPING_Y	= 0;
	public const float CAMERA_LOCKED_DAMPING_Z	= 0;
	public const float CAMERA_MOVING_DAMPING_X	= 1.4f;
	public const float CAMERA_MOVING_DAMPING_Y	= 1.4f;
	public const float CAMERA_MOVING_DAMPING_Z	= 2f;
	public const float CAMERA_LAYER_DAMPING			= 0.2f;
	public const string SCENE_CAMERA_BOUNDS			= "Setup/CameraBounds";
	public const string SCENE_LIGHTSOURCE				= "Setup/Lightsource";
	public const string SCENE_OBJECT						= "SceneObject";
	public const string LAYER_1_BACK						= "Layer1_back";
	public const string LAYER_2_BACK						= "Layer2_back";
	public const string LAYER_3_BACK						= "Layer3_back";
	public const string LAYER_1_FRONT						= "Layer1_front";
	public const string LAYER_2_FRONT						= "Layer2_front";
	public const string LAYER_3_FRONT						= "Layer3_front";
	public const string LAYER_1_PLATFORMS				= "Layer1_platforms";
	public const string LAYER_2_PLATFORMS				= "Layer2_platforms";
	public const string LAYER_3_PLATFORMS				= "Layer3_platforms";
	public const string LAYER_PLAYER_NAME				= "Player";
	public const string LAYER_1_BACK_MASK				= "/_LayerRenderer/Layer1_backmask";
	public const string LAYER_2_BACK_MASK				= "/_LayerRenderer/Layer2_backmask";
	public const string LAYER_3_BACK_MASK				= "/_LayerRenderer/Layer3_backmask";
	public const string LAYER_1_FRONT_MASK			= "/_LayerRenderer/Layer1_frontmask";
	public const string LAYER_2_FRONT_MASK			= "/_LayerRenderer/Layer2_frontmask";
	public const string LAYER_3_FRONT_MASK			= "/_LayerRenderer/Layer3_frontmask";
	public const string LAYER_EFFECTS_BACK			= "/_LayerRenderer/LayerEffects_back";
	public const string LAYER_EFFECTS_FRONT			= "/_LayerRenderer/LayerEffects_front";
	public const string LAYER_PLAYER						= "/_LayerRenderer/LayerPlayer";
	public const string PAUSED_BACKGROUND				= "/_app/_uiCanvas/PausedBackground";
	public const float LAYER_REVEAL_MOVE_DELAY	= 0.05f;
	public const int SCREENSHOT_WIDTH						= 288;
	public const int SCREENSHOT_HEIGHT					= 162;

	public const string TEXT_NOT_FOUND						= "<#ff0000>NOT FOUND</color>";
	public const string GOLD_AMOUNT_FORMAT				= "N0";
	public const string ELIXIR_AMOUNT_FORMAT			= "N0";
	public const string CHARMSTONE_AMOUNT_FORMAT	= "D2";
	public const string HEART_AMOUNT_FORMAT				= "D2";
	public const string POINTS_AMOUNT_FORMAT			= "N0";
	public const string ATTACK_AMOUNT_FORMAT			= "N0";
	public const string DEFENSE_AMOUNT_FORMAT			= "N0";
	public const string MOBILITY_AMOUNT_FORMAT		= "P0";
	public const string ABILITIES_AMOUNT_FORMAT		= "D2";
	public const string GOLD_ADDED_PREFIX					= "+";
	public const string ARROW_UP									= "▲";
	public const string ARROW_DOWN								= "▼";
	public const int ONE_MINUTE_IN_SECONDS				= 60;
	public const int ONE_HOUR_IN_SECONDS					= 3600;
	public const int PLAYER_HEALTH_BAR_MULTIPLIER	= 100;
	public const float HEALTH_BAR_DAMAGE_SPEED		= 0.25f;
	public const float HEALTH_BAR_HEAL_SPEED			= 0.25f;

	public const float ABILITY_FIREBALL_MAXIMUM_DISTANCE = 24f;

	public static Color MENU_DIMMED_COLOR 				= new Color(1f, 1f, 1f, 0.25f);
	public static Color MENU_HIGHLIGHT_COLOR 			= new Color(1f, 1f, 1f, 1f);
	public static Color MENU_ACTIVE_COLOR 				= new Color(1f, 1f, 1f, 1f);
	public static Color MENU_INACTIVE_COLOR 			= new Color(0.5f, 0.5f, 0.5f);
	public static Color STATS_INCREASE_COLOR 			= new Color(0f, 1f, 0f);
	public static Color STATS_DECREASE_COLOR 			= new Color(1f, 0f, 0f);

	// Details for area label images
	public static Dictionary<int, MM_AreaLabel> AreaLabelData { get { return _areaLabelData; } }
	private static Dictionary<int, MM_AreaLabel> _areaLabelData = new Dictionary<int, MM_AreaLabel>() {
		{ (int)AreaLabel.THE_COAST, new MM_AreaLabel(new List<string>() { "The_title01_", "Coast_title02_" }) },
		{ (int)AreaLabel.THE_VALLEY_OF_PEACE, new MM_AreaLabel(new List<string>() { "The_title01_", "ValleyOfPeace_title02_" }) }
	};
}
