using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_Animator_Layer : MonoBehaviour
{
	public bool _sendLayerResizeFinishedEvent;

	public void _atStartFrame() {
		if (_sendLayerResizeFinishedEvent && MM.layerController.IsResizing() && !MM.layerController.IsRevealing()) {
			MM.events.Trigger(MM_Event.LAYER_RESIZE_FINISHED);
		}
	}

	public void _atEndFrame() {
		if (_sendLayerResizeFinishedEvent && MM.layerController.IsResizing() && MM.layerController.IsRevealing()) {
			MM.events.Trigger(MM_Event.LAYER_RESIZE_FINISHED);
		}
	}
}
