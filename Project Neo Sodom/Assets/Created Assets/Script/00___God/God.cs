using UnityEngine;
using System.Collections.Generic;

public static class God
{
    public static GameObject god_update;
    public static float timeRatio = 1;
    public static float gameTime = 0.01f;
    public static float deltaTime = 0.01f;
    public static sys_Input input = new sys_Input();
    public static float gravityAcceleration = 9.8f / 2f;

    public static sys_Camera_ShoulderView CAMERA = null;
    public static CharacterHandler PLAYER = null;

    public static List<CharacterHandler> NPCs = new List<CharacterHandler>();
}
