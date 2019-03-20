using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameProtocol;
using GamePlay;

public class InputTest : MonoBehaviour 
{
    void Start()
    {
        GameInput.onVKey = OnVKey;
    }


    void OnVKey(int vkey, float arg)
    {
        Debug.LogFormat("OnVKey: {0}, {1}", ((GameVKey)vkey).ToString(), arg.ToString());
    }
}
