using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Selectable : MonoBehaviour
{
    private Vector3 scale = Vector3.one;
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if(God.CAMERA != null)
        {
            transform.rotation = God.CAMERA.transform.rotation;
            transform.localScale = Util.smoothChange(transform.localScale, scale, 10, 1);
        }
    }

    public void activate() { this.scale = Vector3.one; }
    public void deactivate() { this.scale = Vector3.zero; }
}
