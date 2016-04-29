using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        Debug.LogFormat(this.transform.position+"******"+this.transform.childCount);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
