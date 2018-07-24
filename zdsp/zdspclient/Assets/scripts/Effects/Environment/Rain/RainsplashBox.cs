// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class RainsplashBox : MonoBehaviour {
	private MeshFilter mf;	
	private RainsplashManager manager;
	
	public void  Start (){
		transform.localRotation = Quaternion.identity;	
		manager = transform.parent.GetComponent<RainsplashManager> ();
		mf = GetComponent<MeshFilter> ();		
		mf.sharedMesh = manager.GetPreGennedMesh ();		
		enabled = false;
	}
	
	void  OnBecameVisible (){
    	enabled = true;
	}

	void  OnBecameInvisible (){
    	enabled = false;
	}
	
	void  OnDrawGizmos (){
		if (transform.parent) {
			manager = transform.parent.GetComponent<RainsplashManager> (); 
			Gizmos.color = new Color(0.5f,0.5f,0.65f,0.5f);
			if(manager)
				Gizmos.DrawWireCube (	transform.position + transform.up * manager.areaHeight * 0.5f, 
										new Vector3 (manager.areaSizeX, manager.areaHeight, manager.areaSizeZ) );
		}
	}
}