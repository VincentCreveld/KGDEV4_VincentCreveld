using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginFieldManager : MonoBehaviour {

	public Selectable[] inputElements;
	private int currentSelection = 0;

	public void Awake() {
		if(inputElements == null)
			this.enabled = false;
	}

	public void Start() {
		inputElements[0].Select();
	}

	public void Update() {
		if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)) {
			if(currentSelection > 0)
				currentSelection--;
			else
				currentSelection = inputElements.Length - 1;
			inputElements[currentSelection].Select();
		}
		else if(Input.GetKeyDown(KeyCode.Tab)) {
			if(currentSelection < inputElements.Length - 1)
				currentSelection++;
			else
				currentSelection = 0;
			inputElements[currentSelection].Select();
		}
	}

}
