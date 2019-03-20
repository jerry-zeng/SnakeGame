using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class MathTest : MonoBehaviour 
{

	// Use this for initialization
	void Start () {
        Test();
	}

    void Test()
    {
        //Vector2i vec2 = new Vector2i();
        //Vector3f vec3 = new Vector3f();

        Vector2 pos = new Vector2(100f, 100f);
        Vector2 size = new Vector2(50f, 50f);
        Rect rect = new Rect(pos, size);
        Debug.LogWarningFormat( "min: {0}, max: {1}", rect.min.ToString(), rect.max.ToString() );
    
        string str = "KA";
        Debug.LogFormat( "{0} -> {1}", str, str.Substring(1,str.Length-1) );
    }
}
