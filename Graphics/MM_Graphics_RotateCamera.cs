using UnityEngine;

public class MM_Graphics_RotateCamera : MonoBehaviour
{
#pragma warning disable CS0649
	[SerializeField] GameObject target;
#pragma warning restore CS0649

	[Header("Speed")]
    [SerializeField] float moveSpeed = 300f;
    [SerializeField] float zoomSpeed = 100f;

    [Header("Zoom")]
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 5f;

	[Header("Unfixed Rotation")]
	[SerializeField] float rotateXSpeed = 0f;
	[SerializeField] float rotateXEnd = 0f;

	[Header("Fixed Rotation")]
	[SerializeField] float xRotation = 10f;
	[SerializeField] float yPosition = 300f;

	private float _ratio = 1f;
	private float _rotationStep = 0f;
	private bool _rotationActive = false;

	void Update () {
        CameraControl();
    }

    void CameraControl() {
		// Rotate the camera upwards if unfixed rotation has been activated
		if (_rotationActive) {
			if (rotateXSpeed > 0) {
				_rotationStep += (_rotationStep > rotateXSpeed) ? 0f : 0.25f;
			} else if (rotateXSpeed < 0) {
				_rotationStep -= (_rotationStep < rotateXSpeed) ? 0f : 0.25f;
			}
			xRotation += Time.deltaTime * _rotationStep;
			transform.eulerAngles = new Vector3(xRotation, transform.eulerAngles.y, transform.eulerAngles.z);
			if ((rotateXSpeed > 0 && xRotation > rotateXEnd) || (rotateXSpeed < 0 && xRotation < rotateXEnd)) {
				_rotationActive = false;
			}
		} else {
			// Rotate the camera around a specific point
			transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime * moveSpeed);
			transform.eulerAngles = new Vector3(xRotation, transform.eulerAngles.y, transform.eulerAngles.z);
			transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);
			// Zoom the camera in
			_zoomCamera();
		}
	}

	public void Rotate(bool rotate) {
		_rotationStep = (rotateXSpeed > 0) ? 2.5f : -2.5f;
		_rotationActive = rotate;
	}

	void _zoomCamera() {
		// If we are already close enough for the min distance and we try to zoom in, dont, return instead. Similarly for zooming out.
		float distance = Vector3.Distance(transform.position, target.transform.position);

		if (distance <= minDistance || _ratio <= 0f) {
			return;
		} else if (distance <= (minDistance + 500)) {
			_ratio -= 0.005f;
		}
        if (distance >= maxDistance) { return; }

		// Only move in the Z relative to the Camera (so forward and back)
		transform.Translate(0f, 0f, Time.deltaTime * zoomSpeed * _ratio, Space.Self );
	}
}
