using UnityEngine;

public class MM_Animator_Door : MonoBehaviour
{
	private Component _parent;
	private TransitionType? _type;

	private void Awake() {
		_getParentComponent();
	}

	private void OnEnable() {
		_getParentComponent();
	}

	private void _getParentComponent() {
		_parent = transform.parent.GetComponent<MM_Objects_LayerTransition>();
		if (_parent != null) {
			_type = TransitionType.LAYER;
		} else {
			_parent = transform.parent.GetComponent<MM_Objects_LocationTransition>();
			if (_parent != null) {
				_type = TransitionType.LOCATION;
			} else {
				_parent = transform.parent.GetComponent<MM_Objects_SceneAreaTransition>();
				if (_parent != null) {
					_type = TransitionType.SCENE_AREA;
				} else {
					_type = null;
				}
			}
		}
	}

	public void _startAnimation_atStartFrame() {
		switch (_type) {
			case TransitionType.LAYER:
				((MM_Objects_LayerTransition)_parent).StartAnimation_atStartFrame();
				break;
			case TransitionType.LOCATION:
				((MM_Objects_LocationTransition)_parent).StartAnimation_atStartFrame();
				break;
			case TransitionType.SCENE_AREA:
				((MM_Objects_SceneAreaTransition)_parent).StartAnimation_atStartFrame();
				break;
		}
	}

	public void _startAnimation_atEndFrame() {
		switch (_type) {
			case TransitionType.LAYER:
				((MM_Objects_LayerTransition)_parent).StartAnimation_atEndFrame();
				break;
			case TransitionType.LOCATION:
				((MM_Objects_LocationTransition)_parent).StartAnimation_atEndFrame();
				break;
			case TransitionType.SCENE_AREA:
				((MM_Objects_SceneAreaTransition)_parent).StartAnimation_atEndFrame();
				break;
		}
	}
}
