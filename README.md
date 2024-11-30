# Monsterland Meka C# code samples

## Introduction

This repository contains a selection of source code used to develop the Monsterland Meka game demo. More details on this project can be found via the following links:
* [Monsterland Meka website](https://monsterlandmeka.com/)
* [Monsterland Meka YouTube](https://www.youtube.com/@monsterlandmeka)

This code is not intended to work on its own. It is a sample of the source code only. The required Unity project files, scene files, remaining source code, and external assets from the Unity Asset Store will not be made publicly available.

## File overview ##

### MM.cs ###

A class that provides global access to other modules used throughout the application. A Unity Scene named _preload (the entry point for the application) will contain an object named _app to which this script is attached. In the Unity object hierarchy the _app object itself will contain other objects which this script locates and provides references for. These references can then be used throughout the application e.g. if I want to trigger an event I can call the command `MM.events.Trigger(MM_Event.EVENT_NAME, new MM_EventData {})` from anywhere in the code.

### MM_AutomatorController.cs ###

Manages automations, which are sets of code that run in response to certain game actions, such as the player entering an area, opening a door, reading a sign, or a UI effect such as the completion of camera panning or masking animation. The automations themselves are defined in MM_Constants_Automations.cs.

### MM_CameraController.cs ###

Camera management that interacts with the Cinemachine asset to provide custom camera panning when the player changes location within a game area (such as when the player enters a door and reappears at another door elsewhere in the area), and enforcing camera boundaries where required. Also activates/deactivates the 6 cameras that render background/foreground objects as required to improve performance.

### MM_Events.cs ###

Event manager for the game. Other class modules can define listener and handler functions for when events are triggered. Events are defined in an enum, and data required by events is packaged into a MM_EventData object. This class provides the ability to define a set of different event listeners together as a single group, with the intention that the event group's onComplete handler is not triggered until every one of the listeners registers their event triggering. Events can also be delayed and trigger custom code.

### MM_GameManager.cs ###

Primarily contains data management for the current game session. Interacts with the Save Game Pro asset to serialize the data related to the current game session. Includes some multithreading management to avoid any graphical lockups and make the save/load process as seamless to the player as possible.

### MM_LayerController.cs ###

Controls the visible layers shown to the player. In this game's architecture the objects in the unity hierarchy are not directly rendered to the screen. Objects will sit in any of up to 6 graphical layers. These objects are then captured by 6 cameras, which then output to separate rendered 2D images in memory. These 2D images are then shown onscreen by being layered over each other. This allows masking to be performed on the 2D images, which results in a circular graphical effect where one layer will disappear while another appears when the player uses doors to move in and out of buildings. The code in this class controls how that masking occurs, and the animations that happen during that process.

### MM_LightingController.cs ###

Interacts with the Aura asset to control lighting within a game area e.g. fog, skybox, position of the lightsource.

### MM_LoadingScreenController.cs ###

Displays an appropriate loading screen (including helpful tips) when a game is loaded for the first time. This takes a bit longer than transitioning between game scenes due to asynchronous loading of data bundles in the background e.g. common graphics, sound effects, and language text.

### MM_MainMenuController.cs ###

This class manages the game's main menu. A 3D scene appears in the background with a menu in the foreground. The player can choose between starting a new game, continuing an existing one, configuring options, and accessing a debug menu.

### MM_MapController.cs ###

Renders an in-game map that the player can use to determine the areas they've explored, and various points of interest. Each game area is split into several colliders (a square segment) which is coded to have fixed coordinates on the map e.g. the player's starting area would be in grid position (80,24) on the map. The code in the map controller then uses the player's position within the collider to accurately position the player's icon within that grid position on the map. This class provides map functionality similar to what you would see in "Metroidvania" style games.

### MM_PlayerController.cs ###

Behaviour that is attached to the player character. Controls whether the player can currently interact with the game world (e.g. this is disabled when the player is in a menu or during automated movements). Also controls player spawning, links input to abilities, manages player damage/death/reviving, interactions with the game world (e.g. with objects, transition locations, NPCs).

### MM_PlayerState.cs ###

Manages the state of the player's data, including inventory, current location, history of interactions with the game world, equipped items, explored regions of the world map. This is the data that is serialized when the player saves their game session.

### MM_PreloadController.cs ###

This script is attached to the _preload scene, which in turn loads the main menu scene. Some unreachable code is included for testing purposes (when the code that loads the main menu is disabled) which quickly initialises a new game so the main menu loading process can be skipped. The DontDestroyOnLoad method is used to ensure that the _preload scene remains in the Unity object hierarchy for the lifetime of the application.

### MM_SceneController.cs ###

Manages entering and exiting of areas within the game. This involves management of the UI (fade in/out, refreshing the HUD, camera), music, waiting for Unity to load all objects that are contained within the game area (e.g. backgrounds, foregrounds, objects, NPCs), disabling/enabling input and animations during/after transitions. Enemies and objects use pools to improve performance (they are not destroyed, simply disabled and then later reused). A Unity scene can contain several game areas with a similar graphical/audio theme. The transition functionality can move the player not only between these local game areas, but also between different Unity scenes (e.g. moving the player from a beach-themed area to a forest-themed area).

### MM_SceneTransition.cs ###

Manages transitions between Unity scenes. The new scene is asynchronously loaded in the background while a fade-to-black is performed on the old scene. When the new scene has completed loading Unity will switch to it, causing the old scene to be removed from the object hierarchy.

### AI\MM_EnemyController.cs ###

Controls spawning, death and the appearance of loot for an enemy object. Enemies are managed by pools and are disabled/enabled so they can be reused.

### Audio\MM_Audio_Music.cs ###

Interacts with the IntroloopPlayer asset to play background music. Provides functionality to preload a track, manage fading in/out, and starting playback at specific points in time.

### Audio\MM_Audio_SFX.cs ###

Manages sound effects, which are split into several different pools that allow caps to be placed on specific types of effects. e.g. it is unlikely there would be a need for many menu sounds to play simultaneously, so the menu pool has a cap for 4 simultaneous sound effects. However, other pools such as environmental sound effects have a higher cap to allow more effects to be played at the same time. Includes code to load common sound effects as their own separate asset bundle when the game starts, and thread management so that the loading of data does not cause the UI to hang.

### Bundles\MM_AreaManager.cs ###

Loads the graphics and music for a specific area from an associated asset bundle.

### Bundles\MM_LanguageManager.cs ###

Loads text used within the game from an associated language asset bundle.

### Bundles\MM_SpriteManager.cs ###

Loads sprites used within the game UI from an asset bundle.

### Constants\MM_Constants.cs ###

Defines several constants used throughout the code.

### Constants\MM_Constants_Automators.cs ###

Defines automators used within the game. See the description for MM_AutomatorController.cs for an explanation of automator functionality.

### Constants\MM_Enums.cs ###

Defines several enums used throughout the code.

### Constants\MM_LootProfiles.cs ###

Defines the set of loot that an enemy is capable of dropping for a specific area. Includes the min/max amount of gold that can be dropped, points that can be received, and any special items.

### Graphics\MM_Animator_Door.cs ###

Manages the animation of a door opening and runs attached functions at the start and end of the animations. e.g. triggering the graphical effect of layers expanding/contracting when the player passes through a door.

### Graphics\MM_Animator_Layer.cs ###

Triggers events that indicate the layer expansion/contraction effect has started/finished when the animation starts/finishes.

### Graphics\MM_Graphics_RotateCamera.cs ###

Rotates a camera around a specific point in a 3D scene. Used in the main menu scene to control movement of the background.

### Graphics\MM_Graphics_ScreenshotHandler.cs ###

Captures a screenshot of the current screen. Used to provide saved games with a visual indication of where in the game world those sessions were saved.

### Input\MM_Input_Reader.cs ###

Interacts with the Rewired asset (responsible for detecting input from control pads) and triggers the appropriate input events in the event system.

### Objects\MM_Objects_Chest.cs ###

Manages the behaviour of treasure chests that contain items and other loot. Interacting with a chest will cause it to open and trigger a sequence of loot spawning.

### Objects\MM_Objects_GoldCoin.cs ###

Manages the behaviour of the gold coin objects, including their spawning, attached animation/UI effects, and movement.

### Objects\MM_Objects_Heart.cs ###

Manages the behaviour of the heart objects which restore the player's health, including its spawning, attached animation/UI effects, and movement.

### Objects\MM_Objects_LayerTransition.cs ###

Manages the transition that occurs when the player moves between two different layers in the game environment. This will include the graphical expansion/contraction effect, and may also include changing the background music and the zooming in/out of the camera.

### Objects\MM_Objects_LocationTransition.cs ###

Manages the movement of the player when they enter a door in one location within an area and move to another location. This will usually include movement of the camera during the transition.

### Objects\MM_Objects_SceneAreaTransition.cs ###

Manages the transition that occurs when the player moves between different game areas. This can be by entering a door, or moving to the edge of the current game area. The current area will be disabled in the Unity object hierarchy and the player will respawn in a separate area which becomes enabled.

### Objects\MM_Objects_Spawner.cs ###

Attached to a spawn point in the Unity object hierarchy, this class will manage the spawning of enemies and items, ensuring the appropriate object is pulled from the relevant object pool.

### Utilities\MM_Helper.cs ###

Several utilities that help other code in the application, such as random number generators, collision detection via tag checking, and getting attributes about objects in the game environment.

### Utilities\MM_TalkDialog.cs ###

Parses data from the language bundle into a format used to display dialogs onscreen when the player is talking to NPCs.

### Utilities\Utilities.cs ###

Utilities that perform XML serialization. Used to save and load game settings to/from file.
