using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
	public int targetFrameRate;

	public bool Disable;

	private void Start()
	{
		QualitySettings.vSyncCount = 0;
		if (!Disable) Application.targetFrameRate = targetFrameRate;
	}
}
