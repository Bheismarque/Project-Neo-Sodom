using UnityEngine;
using System.Collections.Generic;

public class sys_Item : UIS_Object
{
    [SerializeField] private int size = 0;
    private Character holder = null;
    private UI_System UISystem = null;
    private sys_Interactable interactable = null;

    protected override void setUpDetail()
    {
        UISystem = GetComponentInParent<UI_System>().setUp();
        interactable = UISystem == null ? null : UISystem.getInteractable();
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
    public void setHolder(Character holder) { this.holder = holder; }
    public void addSize(int size) { this.size += size; }
    public void setSize(int size) { this.size = size; }
    public virtual int getSize() { return size; }

    public sys_Interactable getInteractableSystem() { return interactable; }
}