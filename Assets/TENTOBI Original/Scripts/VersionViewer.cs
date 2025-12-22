
using UnityEngine;
using TMPro;

public class VersionViewer : MonoBehaviour
{
	private TextMeshProUGUI _versionText;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		this._versionText = this.GetComponent<TextMeshProUGUI>();
		this._versionText.text = "Version " + Application.version;
	}
}
