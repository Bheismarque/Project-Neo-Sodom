using UnityEngine;
using System.Collections;

public abstract class scr_2DObject_dynamic : scr_2DObject
{
    protected float Time_existed = 0;
    void Update()
    {
        step();
        Time_existed += God.gameTime;
    }

    protected abstract void step();
}
