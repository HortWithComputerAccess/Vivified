using UnityEngine;

public class RefreshRotationsButtonController : MonoBehaviour
{
	[SerializeField]
	private RotationCallbackController rotationCallbackController;

	[SerializeField]
	private TracksManager tracksManager;

	private void Start()
	{
		base.gameObject.SetActive(rotationCallbackController.IsActive);
	}

	public void RefreshRotations()
	{
		tracksManager.RefreshTracks();
	}
}
