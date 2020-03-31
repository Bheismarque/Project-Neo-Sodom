using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Blink : MonoBehaviour
{
    private scr_PixelObjectController poc = null;
    [SerializeField] private Vector2 OnDuration = new Vector2(1, 1);
    [SerializeField] private Vector2 OffDuration = new Vector2(0, 0);
    [SerializeField] private Vector2 OnBrightness = new Vector2(2, 2);
    [SerializeField] private Vector2 OffBrightness = new Vector2(0, 0);
    private float onTime = 0;
    private float offTime = 0;
    private bool on = true;

    void Start()
    {
        poc = GetComponent<scr_PixelObjectController>();
        poc.makeMaterialUnique();

        onTime = OnDuration.y;
        offTime = OffDuration.x;
    }

    void Update()
    {
        if (on)
        {
            if (onTime > 0)
            {
                onTime -= Time.deltaTime;
            }
            else
            {
                onTime = Random.Range(OnDuration.x, OnDuration.y);
                poc.setEmissionIntensity(Random.Range(OffBrightness.x, OffBrightness.y));
                on = false;
            }
        }
        else
        {
            if (offTime > 0)
            {
                offTime -= Time.deltaTime;
            }
            else
            {
                offTime = Random.Range(OffDuration.x, OffDuration.y);
                poc.setEmissionIntensity(Random.Range(OnBrightness.x, OnBrightness.y));
                on = true;
            }
        }
    }
}
