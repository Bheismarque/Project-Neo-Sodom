using System.Collections.Generic;

public class UIS_Ability : UIS_CommandChunk
{
    // Face Information Variable
    private string abilityName = "";
    private List<string> arguments = new List<string>();

    // Internal Logic Variables
    private UIS_Entity motherScript = null;
    private List<UIS_Command> commands = new List<UIS_Command>();
    private List<UIS_Data> dataFields = new List<UIS_Data>();

    public int breakCount = 0;

    public UIS_Ability(UIS_Entity motherScript, string abilityName)
    {
        this.motherScript = motherScript;
        this.abilityName = abilityName;
    }
    public UIS_Ability(UIS_Entity motherScript, string abilityName, string sourceCode)
    {
        this.motherScript = motherScript;
        this.abilityName = abilityName;
        UIS.initiateCommand(this, null, commands, sourceCode);
    }

    public UIS_Ability duplicate(UIS_Entity motherScript)
    {
        UIS_Ability copy = new UIS_Ability(motherScript, abilityName);
        copy.arguments = arguments;
        foreach(UIS_Command command in commands)
        {
            copy.commands.Add(command.duplicate(copy, null));
        }

        return copy;
    }

    public void setArguments(List<string> names)
    {
        arguments = names;
    }
    public UIS_DataExtractionForm execute(List<UIS_Data> arguments)
    {
        UIS_DataExtractionForm returnValue = new UIS_DataExtractionForm();
        //Error
        if (arguments == null) { }
        else if (arguments.Count != this.arguments.Count)
        {
            UnityEngine.Debug.Log("Argument Count doesn't match:" + arguments.Count + " " + this.arguments.Count);
        }
        //Error
        else
        {
            int argumentCount = 0;
            foreach (UIS_Data argument in arguments)
            {
                UIS_Data newArgument = new UIS_Data(this.arguments[argumentCount++]);
                newArgument.setData(argument);
                dataFields.Add(newArgument);    
            }
        }

        // Execution
        UIS_Command final = null;
        foreach (UIS_Command command in commands)
        {
            final = command;
            returnValue = command.execute();
            
            /*Error*/
            if (returnValue.error != null) { return returnValue; }
            /*Error*/

            if (breakCount > 0) { breakCount = 0; break; }
        }

        // Empty Local Data Fields
        dataFields.Clear();

        return returnValue;
    }

    public UIS_Entity getMotherScript() { return motherScript; }

    public string getAbilityName() { return abilityName; }

    public List<UIS_Data> getDataFields() { return dataFields; }

    public UIS_Data searchDataField(string dataName)
    {
        foreach (UIS_Data dataField in dataFields)
        {
            if (dataField.getDataName().Equals(dataName)) { return dataField; }
        }
        return null;
    }
}