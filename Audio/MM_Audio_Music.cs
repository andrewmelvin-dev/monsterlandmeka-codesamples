using E7.Introloop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicalTrack {
	NOTHING = 0,
	COMMON_MENU = 1,
	BEGINNING = 2,
	WELCOME_TO_MONSTERLAND = 3,
	COMMON_MAYOR = 4,
	THE_COAST = 5,
	COMMON_PUB = 6,
	COMMON_BOSS_1 = 7,
	VALLEY_OF_PEACE = 8,
	COMMON_TREASURE = 9,
	COMMON_STORE1 = 11,
	DEBUG_ZONE = 99
}

public class MM_Audio_Music : MonoBehaviour {

	private Dictionary<MusicalTrack, String> _trackFilenames = new Dictionary<MusicalTrack, String> {
		{ MusicalTrack.COMMON_MENU, "MM-01-Menu" },
		{ MusicalTrack.BEGINNING, "MM-02-Beginning" },
		{ MusicalTrack.WELCOME_TO_MONSTERLAND, "MM-03-WelcomeToMonsterland" },
		{ MusicalTrack.COMMON_MAYOR, "MM-04-Mayor" },
		{ MusicalTrack.THE_COAST, "MM-05-TheCoast" },
		{ MusicalTrack.COMMON_PUB, "MM-06-Pub" },
		{ MusicalTrack.COMMON_BOSS_1, "MM-07-Boss1" },
		{ MusicalTrack.VALLEY_OF_PEACE, "MM-08-MainTheme1"},
		{ MusicalTrack.COMMON_TREASURE, "MM-09-Treasure" },
		{ MusicalTrack.COMMON_STORE1, "MM-11-Store1" },
		{ MusicalTrack.DEBUG_ZONE, "TMP_DebugZone" }
	};

	private const float _PLAY_IMMEDIATE = 0f;
	private const float _MUSIC_FADE_IN_TIME = 0.5f;
	private const float _MUSIC_FADE_OUT_TIME = 0.5f;
	private MusicalTrack? _currentTrack = null;
	private bool _isPlaying = false;

	public void Preload(MusicalTrack track) {
		try {
			if (track != MusicalTrack.NOTHING) {
				if (_trackFilenames.ContainsKey(track)) {
					IntroloopPlayer.Instance.Preload(MM.areaManager.GetIntroloopAudioTrack(track));
				} else {
					throw new Exception();
				}
			}
		} catch (Exception ex) {
			Debug.LogError("MM_Audio_Music:Preload : error loading track [" + track + "] exception [" + ex + "]");
		}
	}

	public String GetTrackFilename(MusicalTrack track) {
		return _trackFilenames[track];
	}

	public bool IsPlaying() {
		return _isPlaying;
	}

	public MusicalTrack? GetCurrentTrack() {
		return _currentTrack;
	}

	public float GetPlayheadTime() {
		return IntroloopPlayer.Instance.GetPlayheadTime();
	}

	public void PlayFromPosition(MusicalTrack track, float playheadTime = 0f) {
		if (track == MusicalTrack.NOTHING) {
			IntroloopPlayer.Instance.Pause();
			_isPlaying = false;
			_currentTrack = null;
		} else {
			ApplyCurrentVolumeModifier();
			if (playheadTime > 0f) {
				IntroloopPlayer.Instance.Play(MM.areaManager.GetIntroloopAudioTrack(track), _MUSIC_FADE_IN_TIME, playheadTime);
			} else {
				IntroloopPlayer.Instance.Play(MM.areaManager.GetIntroloopAudioTrack(track), _PLAY_IMMEDIATE, playheadTime);
			}
			_isPlaying = true;
			_currentTrack = track;
		}
	}

	public void PlayFromPositionAfterDelay(MusicalTrack track, float delay, float playheadTime = 0f) {
		if (delay > 0) {
			StartCoroutine(_delayPlayFromPosition(track, delay, playheadTime));
		} else {
			PlayFromPosition(track, playheadTime);
		}
	}

	public void PlayCurrentTrackAfterDelay(float delay) {
		if (delay > 0) {
			StartCoroutine(_delayPlayFromPosition((MusicalTrack)_currentTrack, delay, GetPlayheadTime()));
		} else {
			PlayFromPosition((MusicalTrack)_currentTrack, GetPlayheadTime());
		}
	}

	public void Pause(bool fade = true) {
		if (fade) {
			IntroloopPlayer.Instance.Pause(_MUSIC_FADE_OUT_TIME);
		} else {
			IntroloopPlayer.Instance.Pause();
			MM.events.Trigger(MM_Event.MUSIC_TRANSITION_COMPLETE);
		}
		_isPlaying = false;
	}

	public void Resume(bool fade = false) {
		if (fade) {
			IntroloopPlayer.Instance.Resume(_MUSIC_FADE_OUT_TIME);
		} else {
			IntroloopPlayer.Instance.Resume();
		}
		_isPlaying = true;
	}

	public void Stop(bool fade = true) {
		if (fade) {
			IntroloopPlayer.Instance.Stop(_MUSIC_FADE_OUT_TIME);
		} else {
			IntroloopPlayer.Instance.Stop();
		}
		_isPlaying = false;
	}

	public void ApplyCurrentVolumeModifier() {
		IntroloopPlayer.Instance.ApplyVolumeSettingToAllTracks();
	}

	private IEnumerator _delayPlayFromPosition(MusicalTrack track, float delay, float playheadTime) {
		yield return new WaitForSeconds(delay);
		PlayFromPosition(track, playheadTime);
	}
}
