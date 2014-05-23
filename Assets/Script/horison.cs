//--------------------------------------------------------------------------------
// Author	   : 
// Date		   : 
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : 
//
//--------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;

public class horison : MonoBehaviour {
	public UIGrid target;
	public UISprite tt;
	public UIScrollBar SD;
	public UIScrollBar SDs;
	public float xpo;
	public float xco;
	public float xc2o;
	
	// Use this for initialization
	//void Start () {
	//    xpo = this.transform.localPosition.x;
	//    //xc2o = target.transform.localPosition.x;
	//    Debug.Log(transform.localPosition.x.ToString());
		
	//    SDs.barSize = SD.barSize;
	//}
	
	//// Update is called once per frame
	//void Update () {
	//    xco = this.transform.localPosition.x;
	//    target.transform.localPosition = new Vector3(xc2o + (xpo - xco), target.transform.localPosition.y, target.transform.localPosition.z);
	//}
}
