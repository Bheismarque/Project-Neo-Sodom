using UnityEngine;
using System.Collections;

public abstract class scr_2DObject : MonoBehaviour
{
    [SerializeField] protected GameObject spritePrefab = null;
    [SerializeField] [HideInInspector] protected GameObject sprite;
    [SerializeField] [HideInInspector] protected scr_Sprite scr_sprite;

    void Start()
    {
        create();
    }

    protected abstract void create();

    public scr_Sprite getScrSprite() { return scr_sprite; }
    public GameObject getSpritePrefab() { return spritePrefab; }
    public GameObject getSprite() { return sprite; }
    public GameObject setSprite(GameObject newSprite)
    {
        //Destory Older Sprite
        if (sprite != null) { DestroyImmediate(sprite); }

        //Replace with the New Sprite
        sprite = Instantiate(newSprite);
        sprite.transform.parent = gameObject.transform;
        sprite.transform.localPosition = Vector3.zero;

        //Replace the Sprite Script
        scr_sprite = sprite.GetComponent<scr_Sprite>();

        //Return the New Sprite If the Operation Was Successful}
        if (scr_sprite != null) { scr_sprite.create(); return newSprite; }
        sprite = null;
        return null;
    }

    public int getImageIndex()
    {
        if (scr_sprite == null) { return -1; }
        else
        {
            return scr_sprite.getImageIndex();
        }
    }
    public int setImageIndex(int image_index)
    {
        if (scr_sprite == null) { return -1; }
        else
        {
            scr_sprite.setImageIndex(image_index);
            return image_index;
        }
    }

    public float getImageSpeed()
    {
        if (scr_sprite == null) { return -1; }
        else
        {
            return scr_sprite.getImageSpeed();
        }
    }

    public float setImageSpeed(float image_speed)
    {
        if (scr_sprite == null) { return -1; }
        else
        {
            scr_sprite.setImageSpeed(image_speed);
            return image_speed;
        }
    }
}
