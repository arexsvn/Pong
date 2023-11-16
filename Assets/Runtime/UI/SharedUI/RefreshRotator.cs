using UnityEngine;
using UnityEngine.UI;

public class RefreshRotator : MonoBehaviour
{
	public Image refreshImage;
	public float rotateSpeed = 5f;
	private bool _rotate = false;
	private Vector3 _rotationOffset;
	private float _currentRotation = 0f;
	/*
	public float xPos = .5f;
	public float yPos = .5f;
	public float rotX = 0f;
	public float rotY = 0f;
*/

	void Start()
	{
		_rotationOffset = new Vector3(0, 0, -rotateSpeed);
	}

	public void showRefresh()
	{
		refreshImage.gameObject.SetActive(true);
	}

	public void animateRefresh(bool start = true)
	{
		_rotate = start;
	}
	
	// Attach to an object that has a Renderer component,
	// and use material with the shader below.
	public void Update()
	{
		if (refreshImage != null && _rotate || !_rotate && _currentRotation != 0f)
		{
			refreshImage.transform.Rotate(_rotationOffset);
			_currentRotation += rotateSpeed;

			if (_currentRotation >= 360f)
			{
				_currentRotation = 0f;
				refreshImage.transform.rotation = Quaternion.identity;
			}
		}
		/*
		if (refreshImage == null)
		{
			return;
		}
		// Construct a rotation matrix and set it for the shader
		Quaternion rot = Quaternion.Euler(0, 0, Time.time * rotateSpeed);
		Matrix4x4 m = Matrix4x4.TRS(new Vector3(.5f, .5f, 0), rot, Vector3.one);
		refreshImage.material.SetMatrix("_TextureRotation", m);

		//refreshImage.material.SetTextureOffset("_TextureRotation", new Vector2(rotX, rotY));
		*/
	}
}

