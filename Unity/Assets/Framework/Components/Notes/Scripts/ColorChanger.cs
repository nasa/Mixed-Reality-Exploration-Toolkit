using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorChanger : MonoBehaviour {

	public Color color {
		get {
			if (GetComponent<Note> () != null) {
				return GetComponent<Note> ().color;
			} else {
				return GetComponent<Renderer> ().material.color;
			}
		}
		set {
			if (GetComponent<Note> () != null) {
				GetComponent<Note> ().color = value;
			} else {
				GetComponent<Renderer> ().material.SetColor("_Color", value);
			}
		}
	}
}
