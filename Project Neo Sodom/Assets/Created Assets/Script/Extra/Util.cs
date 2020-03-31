using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static readonly int NORMAL_FRAMERATE = 60;

    public static readonly LayerMask BulletLayerMask = ~((1 << 8) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 18) | (1 << 19) | (1 << 20) | (1 << 22));
    //8 12 13 14 18 19 20 22
    public static readonly LayerMask BlockLayerMask = ~((1 << 9) | (1 << 12) | (1 << 15) | (1 << 16) | (1 << 18) | (1 << 19) | (1 << 21) | (1 << 22));
    //9 12 15 16 18 19 21 22
    public static readonly LayerMask VisibleLayerMask = ~((1 << 10) | (1 << 13) | (1 << 15) | (1 << 17) | (1 << 18) | (1 << 20) | (1 << 21) | (1 << 22));
    //10 13 15 17 18 20 21 22
    public static readonly LayerMask CameraLayerMask = ~((1 << 11) | (1 << 14) | (1 << 16) | (1 << 17) | (1 << 19) | (1 << 20) | (1 << 21) | (1 << 22));
    //11 14 16 17 19 20 21 22

    //HELPER METHODS

    private static float smoothChangeRatioCalculation(float slowness, float speed)
    {
        //speed *= 1 / (God.gameTime * NORMAL_FRAMERATE);
        speed *= NORMAL_FRAMERATE * God.gameTime;

        return (1 - Mathf.Pow(1 - 1 / slowness, speed));
    }
    public static float smoothChange(float originalValue, float goalValue, float slowness, float speed)
    {
        return originalValue + (goalValue - originalValue) * smoothChangeRatioCalculation(slowness, speed);
    }
    public static Vector2 smoothChange(Vector2 originalVector2, Vector2 goalVector2, float slowness, float speed)
    {
        return originalVector2 + (goalVector2 - originalVector2) * smoothChangeRatioCalculation(slowness, speed);
    }
    public static Vector3 smoothChange(Vector3 originalVector3, Vector3 goalVector3, float slowness, float speed)
    {
        return originalVector3 + (goalVector3 - originalVector3) * smoothChangeRatioCalculation(slowness, speed);
    }

    public static float angleCorrection(float angle)
    {
        while (angle < 0) { angle += 360; }
        while (angle >= 360) { angle -= 360; }
        return angle;
    }

    public static float smoothAngleChange(float originalAngle, float goalAngle, float slowness, float speed)
    {
        originalAngle = angleCorrection(originalAngle);
        goalAngle = angleCorrection(goalAngle);

        float differenceAngle = goalAngle - originalAngle;

        if (differenceAngle >= 180) { goalAngle -= 360; }
        if (differenceAngle <= -180) { goalAngle += 360; }

        return smoothChange(originalAngle, goalAngle, slowness, speed);
    }

    public static float smoothAngleChange(float originalAngle, float goalAngle, float slowness, float speed, bool side)
    {
        originalAngle = angleCorrection(originalAngle);
        goalAngle = angleCorrection(goalAngle);

        float differenceAngle = goalAngle - originalAngle;

        if (side) { while (goalAngle < originalAngle) { goalAngle += 360; } }
        if (!side) { while (goalAngle > originalAngle) { goalAngle -= 360; } }

        return smoothChange(originalAngle, goalAngle, slowness, speed);
    }

    public static float angleDifference(float angleFrom, float angleTo)
    {
        angleFrom = angleCorrection(angleFrom);
        angleTo = angleCorrection(angleTo);

        float differenceAngle = angleTo - angleFrom;

        if (differenceAngle >= 180) { differenceAngle -= 360; }
        if (differenceAngle <= -180) { differenceAngle += 360; }

        return differenceAngle;
    }

    public static Vector3 dirToVec3(Vector2 angles)
    {
        Vector3 toReturn;
        toReturn.y = Mathf.Sin(angles.y * Mathf.Deg2Rad);
        float unit = Mathf.Cos(angles.y * Mathf.Deg2Rad);

        toReturn.x = Mathf.Cos(angles.x * Mathf.Deg2Rad) * unit;
        toReturn.z = Mathf.Sin(angles.x * Mathf.Deg2Rad) * unit;

        return toReturn;
    }

    public static Vector2 vec3ToDir(Vector3 coord)
    {
        Vector2 toReturn;
        toReturn.x = Vector2.SignedAngle(Vector2.right, new Vector2(coord.x, coord.z));
        toReturn.y = Vector2.SignedAngle(Vector2.right, new Vector2(new Vector2(coord.x, coord.z).magnitude, coord.y));

        return toReturn;
    }

    public static Vector2 dirToVec2(float angle)
    {
        Vector2 toReturn;
        toReturn.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        toReturn.y = Mathf.Sin(angle * Mathf.Deg2Rad);

        return toReturn;
    }

    public static Vector3 dirToVec3(float angle)
    {
        Vector3 toReturn;
        toReturn.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        toReturn.y = 0;
        toReturn.z = Mathf.Sin(angle * Mathf.Deg2Rad);

        return toReturn;
    }

    public static float vec2ToDir(Vector2 coord)
    {
        float toReturn;
        toReturn = Vector2.SignedAngle(Vector2.right, coord);

        return toReturn;
    }

    public static float gradualChange(float originalValue, float goalValue, float changeSpeed, float speed)
    {
        changeSpeed = Mathf.Sign(goalValue - originalValue) * changeSpeed * speed * Time.deltaTime;

        if (changeSpeed > 0)
        {
            if (originalValue + changeSpeed > goalValue) { originalValue = goalValue; }
            else { originalValue += changeSpeed; }
        }
        else
        {
            if (originalValue + changeSpeed < goalValue) { originalValue = goalValue; }
            else { originalValue += changeSpeed; }
        }

        return originalValue;
    }
    
    public static T findTopComponent<T> (Transform o)
    {
        while ( o.parent != null )
        {
            o = o.parent;
            if ( o.GetComponent<T>() != null ) { return o.GetComponent<T>(); }
        }
        return o.GetComponent<T>();
    }
    public static GameObject findTopParent( GameObject o )
    {
        while (o.transform.parent != null)
        {
            o = o.transform.parent.gameObject;
        }

        return o;
    }
}
