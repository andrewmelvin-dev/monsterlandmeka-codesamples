using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_Graphics_ScreenshotHandler : MonoBehaviour {
	public Texture2D _screenshotTexture { get; private set; }

	private int _width;
	private int _height;

	public byte[] GetScreenshot() {
		return _screenshotTexture.GetRawTextureData();
	}

	public void TakeScreenshot(int width, int height, Action onComplete) {
		_width = width;
		_height = height;
		_screenshotTexture = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
		_updateScreenshotTexture(onComplete);
	}

	private void _updateScreenshotTexture(Action onComplete) {
		RenderTexture transformedRenderTexture = null;
		RenderTexture renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
		try {
			ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
			transformedRenderTexture = RenderTexture.GetTemporary(_width, _height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
			Graphics.Blit(renderTexture, transformedRenderTexture, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
			RenderTexture.active = transformedRenderTexture;
			_screenshotTexture.ReadPixels(new Rect(0, 0, _screenshotTexture.width, _screenshotTexture.height), 0, 0, false);
		} catch (Exception e) {
			Debug.LogError("MM_Graphics_ScreenshotHandler:_updateScreenshotTexture : exception: " + e);
		} finally {
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(renderTexture);
			if (transformedRenderTexture != null) {
				RenderTexture.ReleaseTemporary(transformedRenderTexture);
			}
		}
		_screenshotTexture.Apply();
		onComplete?.Invoke();
	}
}
