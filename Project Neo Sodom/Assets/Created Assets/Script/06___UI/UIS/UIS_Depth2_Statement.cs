using System.Collections.Generic;

// Statement ====================================================================================================================================================================
#region
public class UIS_Statement : UIS_Command, UIS_CommandChunk
{
    private UIS_Ability motherAbility;
    private UIS_Statement motherStatement;

    private UIS_CommandLine conditionCommand;
    private List<UIS_Command> commands = new List<UIS_Command>();
    private List<UIS_Data> dataFields = new List<UIS_Data>();

    public bool loop = false;

    public UIS_Statement(UIS_Ability motherAbility, UIS_Statement motherStatement)
    {
        this.motherAbility = motherAbility;
        this.motherStatement = motherStatement;
    }
    public UIS_Statement(UIS_Ability motherAbility, UIS_Statement motherStatement, string conditionSource, string sourceCode)
    {
        this.motherAbility = motherAbility;
        this.motherStatement = motherStatement;

        conditionCommand = new UIS_CommandLine(motherAbility, this, conditionSource, 0);

        UIS.initiateCommand(motherAbility, this, commands, sourceCode);
    }

    public UIS_Command duplicate(UIS_Ability motherAbility, UIS_Statement motherStatement)
    {
        UIS_Statement copy = new UIS_Statement(motherAbility, motherStatement);
        copy.conditionCommand = (UIS_CommandLine)conditionCommand.duplicate(motherAbility, copy);
        foreach(UIS_Command command in commands) { copy.commands.Add(command.duplicate(motherAbility,copy)); }

        return copy;
    }

    private string[] executionLoader = new string[1];
    private UIS_DataExtractionForm outcome;
    private UIS_DataExtractionForm conditionOutcome;
    public UIS_DataExtractionForm execute()
    {
        outcome = new UIS_DataExtractionForm();
        conditionOutcome = new UIS_DataExtractionForm();

        while (true)
        {
            // Empty Local Data Fields
            dataFields.Clear();

            //Condition Calcuation
            executionLoader[0] = conditionCommand.outcomeTempVar;
            outcome = conditionCommand.execute();
            if (outcome.error != null) { return outcome; }
            conditionOutcome = conditionCommand.dataLoad(executionLoader);

            //Condition Check

            if (conditionOutcome.data == null) { break; }
            if (conditionOutcome.data.getDataType() == UIS_Data_Type.Numeric)
            {
                if ((float)(conditionOutcome.data.getData()) == 0) { break; }
            }

            //Execution
            foreach (UIS_Command command in commands)
            {
                outcome = command.execute();
                if (outcome.error != null) { return outcome; }

                // Iteration Control
                if (motherAbility.breakCount > 0) { break; }
                if (loop) { break; }
            }

            // Iteration Control
            if (motherAbility.breakCount > 0) { motherAbility.breakCount--; break; }
            if (loop) { loop = false; continue; }
            else { break; }
        }
        return outcome;
    }

    public List<UIS_Data> getDataFields() { return dataFields; }
    public UIS_Data searchDataField(string dataName)
    {
        foreach (UIS_Data dataField in dataFields)
        {
            if (dataField.getDataName().Equals(dataName)) { return dataField; }
        }
        return null;
    }

    public UIS_Ability getMotherAbility() { return motherAbility; }
    public UIS_Statement getMotherStatement() { return motherStatement; }
}
#endregion
