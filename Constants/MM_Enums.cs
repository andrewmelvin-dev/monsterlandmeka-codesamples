using System;
using System.Collections.Generic;

public enum DialogType {
	SIGNPOST = 1,
	TALKING = 2,
	ITEM_DETAILS = 3,
	PURCHASE = 4
}

[Serializable]
public enum AreaLabel {
	THE_COAST = 101,
	THE_VALLEY_OF_PEACE = 111
}

[Serializable]
public enum SceneAreaLayer {
	NONE = 0,
	LAYER_1 = 1,
	LAYER_2 = 2,
	LAYER_3 = 3
}

public enum TransitionType {
	LAYER,
	LOCATION,
	SCENE_AREA
}

public enum TransitionDisplayType {
	OPEN_WALK_CLOSE = 1,							// Opens door > walks in/out > closes door
	OPEN_WALK_FADE_OR_WALK_CLOSE = 2,	// Opens door > walks in > fadeout OR walks through already open door > closes door
	FORCE_IDLE = 3,										// Forces idle state immediately when continuing a scenearea transition
	OPEN_WALK_FADE_OR_OPEN_WALK = 4,	// Opens door > walks in > fadeout OR Opens door > walks out
	WALK_ONLY = 5											// No animation, only walks in/out
}

public enum InputFocus {
	NONE = 0,
	GAME = 1,
	GAME_BUSY = 2,
	MENU = 3,
	DIALOG = 4
}

public enum InputLockType {
	GAME_INACTIVE,
	LOADING_SCENE,
	SCENE_INITIALISING,
	SCENEAREA_TRANSITION,
	LOCATION_TRANSITION,
	LAYER_TRANSITION,
	PLAYER_ANIMATION_SEQUENCE,
	OBJECT_INTERACTION_SEQUENCE
}

public enum PlayerControlLockType {
	PLAYER_INITIALISING,
	PLAYER_DEATH,
	DAMAGE_KNOCKBACK,
	USING_ITEM
}

public enum PlayerInvulnerabilityType {
	DAMAGED,
	REVIVING,
	ANIMATION_SEQUENCE
}

[Serializable]
public enum MM_GameMenuScreenType {
	MAP 			= MM_Constants.GAME_MENU_MAP_SCREEN,
	INVENTORY = MM_Constants.GAME_MENU_INVENTORY_SCREEN,
	EQUIPMENT = MM_Constants.GAME_MENU_EQUIPMENT_SCREEN,
	ABILITIES = MM_Constants.GAME_MENU_ABILITIES_SCREEN,
	SYSTEM 		= MM_Constants.GAME_MENU_SYSTEM_SCREEN
}

public struct MM_AreaLabel {
	public List<String> lines;
	public MM_AreaLabel(List<String> lines) {
		this.lines = lines;
	}
}
