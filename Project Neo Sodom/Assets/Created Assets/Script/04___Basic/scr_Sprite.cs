using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Sprite : MonoBehaviour
{
    [SerializeField] [HideInInspector] private Animator anim = null;
    private int image_index = -1;
    private int image_number = -1;

    public void create()
    {
        anim = GetComponent<Animator>();
        anim.speed = 0;
        
        AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);
        image_number = (int)((animationClip[0].clip.length * animationClip[0].clip.frameRate));

        setImageIndex(image_index);
        setImageSpeed(0);
        setImageIndex(0);
    }

    public Sprite getSprite()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public int getImageNumber()
    {
        return image_number;
    }

    public int getImageIndex()
    {
        AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);
        AnimatorStateInfo animationState = anim.GetCurrentAnimatorStateInfo(0);
        return Mathf.FloorToInt(animationState.normalizedTime * (animationClip[0].clip.length * animationClip[0].clip.frameRate));
    }
    public void setImageIndex(int imageIndex)
    {
        image_index = imageIndex;
        anim.Play("default", 0, (float)image_index / image_number);
    }

    public float getImageSpeed()
    {
        return anim.speed*12;
    }
    public void setImageSpeed(float speed)
    {
        anim.speed = speed / 12;
    }
}
