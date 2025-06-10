using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DataTest : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnInt1Changed))]
    public int int1 = 66;

    [SyncVar]
    public int int2 = 23487;

    [SyncVar]
    public string MyString = "Example string";

    void OnInt1Changed(int oldValue, int newValue)
    {
       
        // do something here
        Debug.Log($"newValue:{newValue}");
    }


    
}
