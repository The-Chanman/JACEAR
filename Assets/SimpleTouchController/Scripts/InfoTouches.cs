using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoTouches : MonoBehaviour {

	// PUBLIC
	public Text leftText;
	
	public SimpleTouchController leftController;
	

	// PRIVATE


	void Update()
	{
		leftText.text = "Left Touch:\n" +
			"x: " + leftController.GetTouchPosition.x + "\n" +
			"y: " + leftController.GetTouchPosition.y;


	}
}
