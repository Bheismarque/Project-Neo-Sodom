using System;
using System.Data;
using System.Collections.Generic;

// ------------------- UIS : Unified Interface Script -------------------
#region
public class UIS_Entity
{
    private int id;
    private UIS_Object owner = null;

    public string entityTypeName = "";
    public UIS_Data thisData;

    public bool debugMode = false;

    private List<UIS_Data> dataFields = new List<UIS_Data>();
    private List<UIS_Data> localDataFields = new List<UIS_Data>();
    private List<UIS_Ability> abilites = new List<UIS_Ability>();

    public UIS_Entity(UIS_Object owner, string entityTypeName)
    {
        //if(owner == null) { UnityEngine.Debug.Log("noOwner"); }
        id = UIS.UISO_LIST_INDEX_STACK.pop();
        if (id != -1) { UIS.UISO_LIST[id] = this; }
        else { id = UIS.UISO_LIST_COUNT++; UIS.UISO_LIST.Add(this); }

        this.owner = owner;
        this.entityTypeName = entityTypeName;
        thisData = new UIS_Data("this", id, UIS_Data_Type.Entity);
        dataFields.Add(new UIS_Data("typeName", entityTypeName, UIS_Data_Type.String));
    }

    public void delete()
    {
        UIS.UISO_LIST[id] = null;
        UIS.UISO_LIST_INDEX_STACK.push(id);
    }

    public UIS_Entity duplicate(UIS_Object owner)
    {
        UIS_Entity copy = new UIS_Entity(owner, entityTypeName);
        foreach (UIS_Ability ability in abilites) { copy.addAbility(ability); }

        return copy;
    }

    public UIS_CompileProcess UIS_Info_compileProcess = UIS_CompileProcess.None;
    public string UIS_Info_commandLine = null;
    public int UIS_Info_lineIndex;
    public LineReader UIS_Info_commandLineReader = null;

    private class textObject { public string text = ""; public textObject(string text) { this.text = text; } }

    public List<UIS_Data> getLocalDataFields() { return localDataFields; }

    public void addAbility(UIS_Ability ability) { abilites.Add(ability); }

    // ------------------- Encapsulation Methods -------------------
    #region
    public UIS_Data searchDataField(string dataName)
    {
        foreach (UIS_Data dataField in dataFields)
        {
            if (dataField.getDataName().Equals(dataName))
            {
                return dataField;
            }
        }
        return null;
    }

    public UIS_Ability searchAbility(string abilityName)
    {
        foreach (UIS_Ability ability in abilites)
        {
            if (ability.getAbilityName().Equals(abilityName))
            {
                return ability.duplicate(this);
            }
        }
        return null;
    }

    public void executeAbility(string abilityName, List<UIS_Data> arguments)
    {
        UIS_Ability ability = searchAbility(abilityName);
        if (ability != null)
        {
            ability.execute(arguments);
        }
    }

    public List<UIS_Data> getData() { return dataFields; }

    public void printAllData()
    {
        foreach (UIS_Data data in getData()) { owner.printOnConsole(data.toString()); }
    }

    public UIS_Object getOwner() { return owner; }
    public void setOwner(UIS_Object owner) { this.owner = owner; }
    #endregion

    public int getID() { return id; }
}
#endregion
