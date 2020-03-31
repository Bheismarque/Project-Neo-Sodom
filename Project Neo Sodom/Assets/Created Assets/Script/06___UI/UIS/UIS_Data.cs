using System;
using System.Data;
using System.Collections.Generic;

public enum UIS_Data_Type { Entity, Numeric, String }
public class UIS_Data
{
    private string dataName = "";

    private Object data = null;

    private UIS_Data_Type dataType = UIS_Data_Type.Entity;
    private bool isTemporary = false;

    public UIS_Data(string dataName)
    {
        this.dataName = dataName;
    }

    public UIS_Data(string dataName, Object data, UIS_Data_Type dataType)
    {
        this.dataName = dataName;
        this.data = data;
        this.dataType = dataType;
    }

    public string toString()
    {
        if (!dataType.Equals(typeof(float)) && !dataType.Equals(typeof(string))) { return dataName + " " + dataType; }
        return dataName + " " + Convert.ChangeType(data, typeof(string));
    }

    public string getDataName() { return dataName; }
    public Object getData() { return data; }


    public void setData(UIS_Data data)
    {
        if (data == null) { data = UIS_Keyword.UISKeywords[2].data; }
        this.data = data.getData();
        dataType = data.getDataType();
    }

    public void setData(Object data, UIS_Data_Type dataType)
    {
        this.data = data;
        this.dataType = dataType;
    }

    public UIS_Data_Type getDataType() { return dataType; }
    public void setDataType(UIS_Data_Type dataType) { this.dataType = dataType; }
}