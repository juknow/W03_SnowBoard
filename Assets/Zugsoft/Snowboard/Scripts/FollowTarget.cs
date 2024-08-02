using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {
    public Transform target;
    Vector3 offset;
	// Use this for initialization
	void Start () {
        offset = transform.position - target.position;

    }
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(transform.position, target.position+offset,1f);
	}
}
