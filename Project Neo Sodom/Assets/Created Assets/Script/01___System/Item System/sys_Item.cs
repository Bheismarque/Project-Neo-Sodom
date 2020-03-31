using UnityEngine;
using System.Collections.Generic;

public class sys_Item : UIS_Object
{
    [SerializeField] private int size = 0;
    private scr_Person holder = null;
    private UI_System UISystem = null;

    private Transform target = null;

    protected override void create()
    {
        UISystem = GetComponentInParent<UI_System>();
        target = transform.Find("Target");
    }
    protected override void step()
    {
        if(holder != null)
        {
            UISystem.transform.position = holder.getBone("right hand").position;

            Transform parentSave = UISystem.transform.parent;

            UISystem.transform.parent = holder.transform;
            UISystem.transform.localRotation = Quaternion.identity;

            UISystem.transform.parent = parentSave;
        }
    }
    public void setHolder(scr_Person holder) { this.holder = holder; UISystem.getInteractable().setShinable(holder==null); }
    public void addSize(int size) { this.size += size; }
    public void setSize(int size) { this.size = size; }
    public virtual int getSize() { return size; }
}