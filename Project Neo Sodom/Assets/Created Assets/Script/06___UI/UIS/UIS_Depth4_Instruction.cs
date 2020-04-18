using UnityEngine;
using System;
using System.Collections.Generic;

public enum UIS_Instruction_type
{
    // Set Control
    Set, Equals,

    // Calculation Control
    Add, Subtract, Multiply, Divide, Remainder,

    // Comparison Control
    Compare_Equal, Compare_NotEqual, Compare_EqualOrSmaller, Compare_EqualOrLarger, Compare_Smaller, Compare_Larger,

    // Logic Control
    And, Or
}

public class UIS_Instruction_Set : UIS_Instruction
{
    public UIS_Instruction_Set(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //string p = "";
        //foreach (string c in this.operand2) { p += c + "."; }
        //Debug.Log("<" + p + ">");
        //debugPrint("Set : " + "\"" + destination.getDataName() + "\" <== " + operand2.getData());

        destination.setData(operand2);
    }
}
public class UIS_Instruction_Equals : UIS_Instruction
{
    public UIS_Instruction_Equals(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        // * Keyword Operation =============================================================================================================================================================================================================
        if (operand1 == null)
        {
            UIS_Entity motherScript = commander.getMotherScript();
            UIS_Ability motherAbility = commander.getMotherAbiltiy();
            UIS_Statement motherStatement = commander.getMotherStatement();

            string newVariableName = this.operand1[this.operand1.Length - 1];

            // New --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("new"))
            {
                UIS_Entity newEntity = UIS.createEntity(null, operandStr2);
                if (newEntity == null) { Debug.Log("Entity Type Not Found"); return; }
                destination.setData(newEntity.getID(), UIS_Data_Type.Entity);
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Duplicate --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("duplicate"))
            {
                UIS_Entity newEntity = UIS.findEntityType(operandStr2).getOwner().duplicate(motherScript.getOwner().getSystem());
                if (newEntity == null) { Debug.Log("Entity Type Not Found"); return; }
                destination.setData(newEntity.getID(), UIS_Data_Type.Entity);
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Create --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("load"))
            {
                UI_System createdObject = GameObject.Instantiate((GameObject)Resources.Load("Items/"+operandStr2, typeof(GameObject))).GetComponent<UI_System>();
                UIS_Entity newEntity = createdObject == null ? null : createdObject.initiate().getController().setUp().getUISE();
                if (newEntity == null) { Debug.Log("Entity Type Not Found"); return; } 
                destination.setData(newEntity.getID(), UIS_Data_Type.Entity);
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Delete --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("delete"))
            {
                if(operand2.getDataType() != UIS_Data_Type.Entity) {}
                (UIS.UISO_LIST[(int)operand2.getData()]).getOwner().delete();
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Skin --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("skin"))
            {
                if (operand2.getDataType() != UIS_Data_Type.Entity) { }
                (UIS.UISO_LIST[(int)operand2.getData()]).getOwner().skin();
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Skin --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("replace"))
            {
                if (operand2.getDataType() != UIS_Data_Type.Entity) { }
                commander.getMotherScript().getOwner().setUISE(UIS.UISO_LIST[(int)operand2.getData()]);
                //debugPrint("New : " + "\"" + destination.getDataName());

                return;
            }
            // Print --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("print"))
            {
                UIS_Object owner = motherScript.getOwner();
                string ownerName = owner == null ? "(unknown): " : owner.name + ": ";
                if (operandStr2 != null) { Debug.Log(ownerName + operandStr2); }
                else if (operandIsNum2) { Debug.Log(ownerName + operandNum2); }
                else if (operand2.getDataType() == UIS_Data_Type.Entity)
                {
                    ownerName += (int)operand2.getData() == -1 ? "null" :
                                UIS.getUISEfromUISD(operand2).entityTypeName;
                    Debug.Log(ownerName);
                }
                else { Debug.Log("Unprintable"); }
                return;
            }
            // Break --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("break"))
            {
                int breakCount = 0;
                if (operandIsNum2) { breakCount = (int)operandNum2; }
                else if (operandStr2 != null) { breakCount = 1; }

                motherAbility.breakCount = breakCount;
                return;
            }
            // Loop --------------------------------------------------------------------------------------------------------
            if (newVariableName.Equals("loop"))
            {
                motherStatement.loop = true;
                return;
            }
            // Virtual Variable Calculation --------------------------------------------------------------------------------------------------------
            if (newVariableName[0] == '\'' && newVariableName[newVariableName.Length - 1] == '\'')
            {
                UIS_CommandLine tempCommandLine = new UIS_CommandLine(motherAbility, motherStatement, newVariableName.Substring(1, newVariableName.Length - 2), 0);
                tempCommandLine.execute();
                UIS_DataExtractionForm extractedData = tempCommandLine.dataLoad(new string[] { tempCommandLine.outcomeTempVar });
                newVariableName = (string)extractedData.data.getData();
            }

            // Data Addition --------------------------------------------------------------------------------------------------------
            UIS_Data newData = new UIS_Data(newVariableName, operand2.getData(), operand2.getDataType());

            if (this.operand1.Length == 1 && !this.operand1[0].Equals("this"))
            {
                if (motherStatement != null) { motherStatement.getDataFields().Add(newData); }
                else if (motherAbility != null) { motherAbility.getDataFields().Add(newData); }
            }
            else
            {
                operandForm1.dataHolder.getData().Add(newData);
            }

            // Destination Set --------------------------------------------------------------------------------------------------------
            destination.setData(newData);
            //debugPrint("Equals Case 1 : " + "\"" + newData.getDataName() + "\" <== " + operand2.getDataName());
        }

        // Good Ol' Equals Operation
        else
        {
            operand1.setData(operand2);
            destination.setData(operand1);
            //debugPrint("Equals Case 2 : " + "\"" + operand1.getDataName() + "\" <== " + operand2.getDataName());
        }
        return;
    }
}
public class UIS_Instruction_Add : UIS_Instruction
{
    public UIS_Instruction_Add(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //String Addition
        if (operandStr1 != null && operandIsNum2) { /*debugPrint("Add Case 1 : (" + destination.getDataName() +","+ operand1.getDataName() + "," + operand2.getDataName() + ")  \"" + operandStr1 + "\" + " + operandNum2);*/ destination.setData(operandStr1 + operandNum2, UIS_Data_Type.String); return; }
        if (operandIsNum1 && operandStr2 != null) { /*debugPrint("Add Case 2 : (" + destination.getDataName() + "," + operand1.getDataName() + "," + operand2.getDataName() + ")  " + operandNum1 + " + \"" + operandStr2 + "\"");*/ destination.setData(operandNum1 + operandStr2, UIS_Data_Type.String); return; }
        if (operandStr1 != null && operandStr2 != null) { /*debugPrint("Add Case 3 : (" + destination.getDataName() + "," + operand1.getDataName() + "," + operand2.getDataName() + ")    " + operandStr1 + " + " + operandStr2 );*/ destination.setData(operandStr1 + operandStr2, UIS_Data_Type.String); return; }

        //Number Addition
        if (operandIsNum1 && operandIsNum2) {/* debugPrint("Add Case 4 : (" + destination.getDataName() + "," + operand1.getDataName() + "," + operand2.getDataName() + ")  " + operandNum1 + " + " + operandNum2);*/ destination.setData(operandNum1 + operandNum2, UIS_Data_Type.Numeric); return; }

    }
}
public class UIS_Instruction_Subtract : UIS_Instruction
{
    public UIS_Instruction_Subtract(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //Number Subtraction
        if (operandIsNum1 && operandIsNum2) { /*debugPrint("Subtract Case 1 : " + operandNum1 + " - " + operandNum2);*/ destination.setData(operandNum1 - operandNum2, UIS_Data_Type.Numeric); return; }
    }
}
public class UIS_Instruction_Multiply : UIS_Instruction
{
    public UIS_Instruction_Multiply(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //Number Addition
        if (operandIsNum1 && operandIsNum2) { /*debugPrint("Multiply Case 1 : " + operandNum1 + " * " + operandNum2);*/ destination.setData(operandNum1 * operandNum2, UIS_Data_Type.Numeric); return; }
    }
}
public class UIS_Instruction_Divide : UIS_Instruction
{
    public UIS_Instruction_Divide(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //Number Addition
        if (operandIsNum1 && operandIsNum2) { /*debugPrint("Division Case 1 : " + operandNum1 + " / " + operandNum2);*/ destination.setData(operandNum1 / operandNum2, UIS_Data_Type.Numeric); return; }
    }
}
public class UIS_Instruction_Remainder : UIS_Instruction
{
    public UIS_Instruction_Remainder(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        //Number Addition
        if (operandIsNum1 && operandIsNum2) { /*debugPrint("Division Case 1 : " + operandNum1 + " / " + operandNum2);*/ destination.setData(operandNum1 % operandNum2, UIS_Data_Type.Numeric); return; }
    }
}
public class UIS_Instruction_Compare_Equal : UIS_Instruction
{
    public UIS_Instruction_Compare_Equal(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        // Number Comparision
        if (operand1.getDataType() == UIS_Data_Type.Numeric)
        {
            destination.setData(operandNum1 == operandNum2 ? 1f : 0f, UIS_Data_Type.Numeric); return;
        }

        // String Comparision
        if (operand1.getDataType() == UIS_Data_Type.String)
        {
            destination.setData(operandStr1.Equals(operandStr2) ? 1f : 0f, UIS_Data_Type.Numeric); return;
        }

        // UIS Object Comparision
        if (operand1.getDataType() == UIS_Data_Type.Entity)
        {
            destination.setData(operand1.getData() == operand2.getData() ? 1f : 0f, UIS_Data_Type.Numeric); return;
        }
    }
}
public class UIS_Instruction_Compare_NotEqual : UIS_Instruction
{
    public UIS_Instruction_Compare_NotEqual(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        // Number Comparision
        if (operand1.getDataType() == UIS_Data_Type.Numeric)
        {
            destination.setData(operandNum1 == operandNum2 ? 0f : 1f, UIS_Data_Type.Numeric); return;
        }

        // String Comparision
        if (operand1.getDataType() == UIS_Data_Type.String)
        {
            destination.setData(operandStr1.Equals(operandStr2) ? 0f : 1f, UIS_Data_Type.Numeric); return;
        }

        // UIS Object Comparision
        if (operand1.getDataType() == UIS_Data_Type.Entity)
        {
            destination.setData(operand1.getData() == operand2.getData() ? 0f : 1f, UIS_Data_Type.Numeric); return;
        }
    }
}
public class UIS_Instruction_Compare_EqualOrSmaller : UIS_Instruction
{
    public UIS_Instruction_Compare_EqualOrSmaller(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        // Number Comparision
        if (operand1.getDataType() == UIS_Data_Type.Numeric)
        {
            destination.setData(operandNum1 <= operandNum2 ? 1f : 0f, UIS_Data_Type.Numeric);
        }
    }
}
public class UIS_Instruction_Compare_EqualOrLarger : UIS_Instruction
{
    public UIS_Instruction_Compare_EqualOrLarger(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        destination.setData(operandNum1 >= operandNum2 ? 1f : 0f, UIS_Data_Type.Numeric);
    }
}
public class UIS_Instruction_Compare_Smaller : UIS_Instruction
{
    public UIS_Instruction_Compare_Smaller(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        destination.setData(operandNum1 < operandNum2 ? 1f : 0f, UIS_Data_Type.Numeric);
    }
}
public class UIS_Instruction_Compare_Larger : UIS_Instruction
{
    public UIS_Instruction_Compare_Larger(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        destination.setData(operandNum1 > operandNum2 ? 1f : 0f, UIS_Data_Type.Numeric);
    }
}
public class UIS_Instruction_And : UIS_Instruction
{
    public UIS_Instruction_And(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        bool and1 = !((operandIsNum1 && operandNum1 == 0) || (operand1.getData() == null));
        bool and2 = !((operandIsNum2 && operandNum2 == 0) || (operand2.getData() == null));

        destination.setData(and1 && and2 ? 1f : 0f, UIS_Data_Type.Numeric);
    }
}
public class UIS_Instruction_Or : UIS_Instruction
{
    public UIS_Instruction_Or(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2) : base(commander, destination, operand1, instructionType, operand2) { }
    public override void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2)
    {
        bool or1 = !((operandIsNum1 && operandNum1 == 0) || (operand1.getData() == null));
        bool or2 = !((operandIsNum2 && operandNum2 == 0) || (operand2.getData() == null));

        destination.setData(or1 || or2 ? 1f : 0f, UIS_Data_Type.Numeric);
    }
}

public abstract class UIS_Instruction
{
    private UIS_CommandLine commander = null;
    private static char[] divide = new char[] { '.' };
    public string[] destination = null;
    public string[] operand1 = null;
    public UIS_Instruction_type instructionType;
    public string[] operand2 = null;

    public UIS_Instruction(UIS_CommandLine commander, string destination, string operand1, UIS_Instruction_type instructionType, string operand2)
    {
        // Destination Parse
        this.destination = destination.Split(divide); for (int i = 0; i < this.destination.Length; i++) { this.destination[i] = this.destination[i].Trim(); }
        this.commander = commander;

        // Instruction Parse
        this.instructionType = instructionType;

        // Operand 1 Parse
        if (operand1 != null)
        {
            // Step 1 : Check if the data is Numeric
            try
            {
                float n = float.Parse(operand1);
                this.operand1 = new string[] { operand1.Trim() };
            }
            catch (Exception e)
            {
                e.GetType();
                // Step 2 : Check if the data is String
                if (UIS.determineString(operand1) != null) { this.operand1 = new string[] { operand1.Trim() }; }
                else
                {
                    this.operand1 = operand1.Split(divide);
                    for (int i = 0; i < this.operand1.Length; i++) { this.operand1[i] = this.operand1[i].Trim(); }
                }
            }
        }

        // Operand 2 Parse
        if (operand2 != null)
        {
            // Step 1 : Check if the data is Numeric
            try
            {
                float n = float.Parse(operand2);
                this.operand2 = new string[] { operand2.Trim() };
            }
            catch (Exception e)
            {
                e.GetType();
                // Step 2 : Check if the data is String
                if (UIS.determineString(operand2) != null) { this.operand2 = new string[] { operand2.Trim() }; }
                else
                {
                    this.operand2 = operand2.Split(divide);
                    for (int i = 0; i < this.operand2.Length; i++) { this.operand2[i] = this.operand2[i].Trim(); }
                }
            }
        }

        // Print
        //UnityEngine.Debug.Log("** INSTRUCTION ** <" + destination + ">   ==   <" + (operand1!=null?operand1 : "NULL") + ">   " + instructionType + "   <" + (operand2 != null ? operand2 : "NULL") + ">");
    }

    public abstract void execute(UIS_CommandLine commander, UIS_DataExtractionForm destinationForm, UIS_DataExtractionForm operandForm1, UIS_DataExtractionForm operandForm2,
                                 UIS_Data destination, UIS_Data operand1, UIS_Data operand2,
                                 string operandStr1, string operandStr2, bool operandIsNum1, bool operandIsNum2, float operandNum1, float operandNum2);
}