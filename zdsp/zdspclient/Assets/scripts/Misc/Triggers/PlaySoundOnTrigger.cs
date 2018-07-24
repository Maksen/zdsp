// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class PlaySoundOnTrigger : MonoBehaviour {
	public bool onlyPlayOnce = true;
	private bool playedOnce = false;
	
	void  OnTriggerEnter (Collider other){
		if (other.CompareTag("LocalPlayer")) {
			if (playedOnce && onlyPlayOnce)
				return;
			GetComponent<AudioSource> ().Play ();
			playedOnce = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("LocalPlayer"))
			GetComponent<AudioSource>().Stop();
	}
}
