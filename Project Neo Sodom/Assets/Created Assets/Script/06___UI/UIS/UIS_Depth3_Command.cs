using System;
using System.Collections.Generic;

// CommandLine ====================================================================================================================================================================
#region
public class UIS_CommandLine : UIS_Command
{
    //Static Variables
    static private char[] ABILITY_DEFINERS = new char[] { '[', ',', ']' };
    static private char[] OPERATE_DEFINERS = new char[] { '+', '-', '*', '/' };

    private UIS_Entity motherScript = null;
    private UIS_Ability motherAbility = null;
    private UIS_Statement motherStatement = null;
    private string originalLine = null;
    private int lineNum = 0;

    //Logic Variable (Hard to Understand)

    private List<UIS_Instruction> instructions = new List<UIS_Instruction>();
    private List<UIS_Data> tempVars = new List<UIS_Data>();
    public string outcomeTempVar = "";

    public UIS_CommandLine(UIS_Ability motherAbility, UIS_Statement motherStatement)
    {
        motherScript = motherAbility.getMotherScript();
        this.motherAbility = motherAbility;
        this.motherStatement = motherStatement;
        this.motherAbility = motherAbility;
    }
    public UIS_CommandLine(UIS_Ability motherAbility, UIS_Statement motherStatement, string originalLine, int lineNum)
    {
        motherScript = motherAbility.getMotherScript();
        this.motherAbility = motherAbility;
        this.motherStatement = motherStatement;
        this.motherAbility = motherAbility;
        this.originalLine = originalLine;
        this.lineNum = lineNum;
        outcomeTempVar = interpret(originalLine);
    }

    public UIS_Command duplicate(UIS_Ability motherAbility, UIS_Statement motherStatement)
    {
        UIS_CommandLine copy = new UIS_CommandLine(motherAbility, motherStatement);
        copy.originalLine = originalLine;
        copy.lineNum = lineNum;

        copy.instructions = instructions;
        foreach(UIS_Data tempVar in tempVars)
        {
            UIS_Data temporaryVariable = new UIS_Data("T_Var_" + copy.tempVars.Count);
            copy.tempVars.Add(temporaryVariable);
        }
        copy.outcomeTempVar = outcomeTempVar;

        return copy;
    }

    // Intepretation ==============================================================================================================================================================
    #region
    public string interpret(string line)
    {
        string interpretedLine;
        int bracketCount;
        int bracket_start;
        int bracket_close;
        bool quoteOpened;

        // * Step : Bracket Substitude ============================================================================================================================================
        #region
        //----------------------------- Ability Bracket Process ----------------------------------
        #region
        interpretedLine = "";
        bracketCount = 0;
        bracket_start = -1;
        bracket_close = -1;
        quoteOpened = false;
        string toPrint = line;
        for (int i = 0; i < line.Length; i++)
        {
            char letter = line[i];
            if (letter == '"') { quoteOpened = !quoteOpened; }
            if (!quoteOpened)
            {
                //Error Check-------------------------------------------------
                if (letter == ',' && bracketCount == 0) { }
                //Error Check-------------------------------------------------

                if (letter == '[')
                {
                    bool quoteOpened2 = false;
                    for (; i < line.Length; i++)
                    {
                        letter = line[i];
                        if (letter == '"') { quoteOpened2 = !quoteOpened2; }

                        if (!quoteOpened2)
                        {
                            // Bracket Start
                            if (letter == '[') { bracketCount++; if (bracketCount == 1) { bracket_start = i; } interpretedLine += letter; }

                            // Argument Parse
                            if ((letter == ',' || letter == ']') && bracketCount == 1)
                            {
                                string argument = line.Substring(bracket_start + 1, i - bracket_start - 1).Trim();
                                if (argument.Length != 0) { interpretedLine += interpret(argument); bracket_start = i; }
                                interpretedLine += letter;
                            }

                            // Bracket End
                            if (letter == ']') { bracketCount--; if (bracketCount == 0) { bracket_close = i; } break; }
                        }

                        //Error Check-------------------------------------------------
                        if (bracket_start == -1 && bracket_close != -1) { }
                        if (bracket_start != -1 && bracket_close == -1 && i == line.Length - 1) { }
                        //Error Check-------------------------------------------------
                    }
                }
                else
                {
                    interpretedLine += letter;
                }
            }
            else
            {
                interpretedLine += letter;
            }
        }
        line = interpretedLine;
        //UnityEngine.Debug.Log(toPrint + " --> " + line);
        #endregion

        //----------------------------- Normal Bracket Process ----------------------------------
        #region
        interpretedLine = "";
        bracketCount = 0;
        bracket_start = -1;
        bracket_close = -1;
        quoteOpened = false;
        for (int i = 0; i < line.Length; i++)
        {
            char letter = line[i];

            if (letter == '"') { quoteOpened = !quoteOpened; }
            if (!quoteOpened)
            {
                // Bracket Count
                if (letter == '(') { if (bracketCount == 0 && bracket_start == -1) { bracket_start = i; } bracketCount++; }
                if (letter == ')') { if (bracketCount == 1 && bracket_close == -1) { bracket_close = i; } bracketCount--; }
            }
            // Bracket Error Check
            if (bracket_start == -1 && bracket_close != -1) { }
            if (bracket_start != -1 && bracket_close == -1 && i == line.Length - 1) { }

            // Bracket Interpretation
            if (bracket_start != -1 && bracket_close != -1)
            {
                // Recurse
                string toInterpret = line.Substring(bracket_start + 1, bracket_close - bracket_start - 1);
                interpretedLine += interpret(toInterpret);

                // Bracket Indice Reset
                bracket_start = -1;
                bracket_close = -1;
            }

            // Letter Addition
            else if (bracketCount == 0) { interpretedLine += letter; }
        }
        line = interpretedLine;
        #endregion
        #endregion

        // * Step : Equals Parse ==================================================================================================================================================
        #region 
        for (int i = 1; i < line.Length-1; i++)
        {
            char letter = line[i];
            if (letter == '=')
            {
                bool equalsOperate = true;
                switch (line[i - 1])
                {
                    case '=': case '!': case '<': case '>': equalsOperate = false; break;
                    default: break;
                }
                switch (line[i + 1])
                {
                    case '=': case '!': case '<': case '>': equalsOperate = false; break;
                    default: break;
                }

                if (equalsOperate)
                {
                    UIS_Instruction_type UISInstructionType = UIS_Instruction_type.Equals;
                    int previousWorldLength = i;
                    switch (line[i - 1])
                    {
                        case '+': UISInstructionType = UIS_Instruction_type.Add; previousWorldLength--; break;
                        case '-': UISInstructionType = UIS_Instruction_type.Subtract; previousWorldLength--; break;
                        case '*': UISInstructionType = UIS_Instruction_type.Multiply; previousWorldLength--; break;
                        case '/': UISInstructionType = UIS_Instruction_type.Divide; previousWorldLength--; break;
                    }

                    string destination = makeTemporaryVariable();
                    string subject = line.Substring(0, previousWorldLength);
                    string target = interpret(" " + line.Substring(i + 1));

                    addInstruction(destination, subject, UISInstructionType, target);
                    if (previousWorldLength != i)
                    {
                        addInstruction(destination, subject, UIS_Instruction_type.Equals, destination);
                    }
                    return destination;
                }
            }
            /*Error!!*/
            /*Error!!*/
        }
        #endregion

        // * Step : Calculation & Return ==========================================================================================================================================
        #region
        List<string> componentList = new List<string>();
        List<char> operatorList = new List<char>();

        string currentComponent = "";
        for (int i = 0; i < line.Length; i++)
        {
            // Quote Parse
            if (line[i] == '"')
            {
                int startIndex = i;
                int count = 2;
                for (i++;  i < line.Length; i++)
                {
                    if (line[i] == '"') { i++; break; }
                    count++;

                    /*Error!!*/
                    if (i == line.Length - 1)
                    {
                    }
                    /*Error!!*/
                }
                currentComponent = line.Substring(startIndex, count);
                if (i >= line.Length) { componentList.Add(currentComponent); break; }
            }

            // Common Parse
            char c = line[i];

            if (i != 0)
            {
                if (c == '+' || c == '-')
                {
                    if (currentComponent.Equals(""))
                    {
                        currentComponent += c;
                    }
                    else
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add(c);
                    }
                    continue;
                }
                if (c == '*' || c == '/' || c == '%')
                {
                    componentList.Add(currentComponent);
                    currentComponent = "";
                    operatorList.Add(c);
                    continue;
                }

                if (line[i] == '=' && i >= 1)
                {
                    //Equals
                    if (line[i - 1] == '=')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('e');
                    }

                    //Not Equals
                    if (line[i - 1] == '!')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('E');
                    }

                    //Equal or Smaller
                    if (line[i - 1] == '<')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('s');
                    }

                    //Equal or Larger
                    if (line[i - 1] == '>')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('l');
                    }
                }

                if (line[i] != '=' && i >= 1)
                {
                    //Smaller
                    if (line[i - 1] == '<')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('S');
                    }

                    //Larger
                    if (line[i - 1] == '>')
                    {
                        componentList.Add(currentComponent);
                        currentComponent = "";
                        operatorList.Add('L');
                    }
                }

                if (line[i] == '&' && line[i - 1] == '&')
                {
                    componentList.Add(currentComponent);
                    currentComponent = "";
                    operatorList.Add('A');
                }

                if (line[i] == '|' && line[i - 1] == '|')
                {
                    componentList.Add(currentComponent);
                    currentComponent = "";
                    operatorList.Add('O');
                }
            }
            if (i == line.Length - 1)
            {
                currentComponent += (c == ' ' ? "" : "" + c);
                componentList.Add(currentComponent);
                continue;
            }
            currentComponent += (c == ' ' || c == '!' || c == '<' || c == '>' || c == '=' || c == '&' || c == '|' ? "" : "" + c);
        }
        string[] components = new string[componentList.Count];
        for (int i = 0; i < components.Length; i++) { components[i] = componentList[i]; }
        char[] operators = new char[operatorList.Count];
        for (int i = 0; i < operators.Length; i++) { operators[i] = operatorList[i]; }

        /*
        string prnt = "";
        for (int i = 0; i < components.Length; i++)
        {
            if ( i > 0 ) { prnt += " " + operators[i - 1] + " "; }
            prnt += " <" + components[i] + ">";
        }
        UnityEngine.Debug.Log(prnt);
        */


        //Multiplyer & Divider Grouping
        string tempVar_1 = makeTemporaryVariable();
        string tempVar_2 = makeTemporaryVariable();

        UIS_Instruction_type instructionType = UIS_Instruction_type.Set;
        if (operators.Length > 0 && (operators[0] == '*' || operators[0] == '/'))
        {
            addInstruction(tempVar_1, null, instructionType, "0");
        }
        else if (components.Length > 0)
        {
            addInstruction(tempVar_1, null, instructionType, components[0]);
        }
        char operator_cur = ' ';
        for (int i = 0; i < operators.Length; i++)
        {
            char operator_pre = operator_cur;
            operator_cur = operators[i];

            //Multiplication & Division
            if (operator_cur == '*' || operator_cur == '/')
            {
                //Set TempVar2 to the First Operand
                addInstruction(tempVar_2, null, UIS_Instruction_type.Set, components[i]);

                //Multiply or Divide the whole thing until it ends
                for (; i < operators.Length; i++)
                {
                    operator_cur = operators[i];
                    if (operator_cur != '*' && operator_cur != '/') { break; }

                    instructionType = operator_cur == '*' ? UIS_Instruction_type.Multiply : UIS_Instruction_type.Divide;
                    addInstruction(tempVar_2, tempVar_2, instructionType, components[i + 1]);
                }

                //Addition & Subtraction
                instructionType = operator_pre == '+' ? UIS_Instruction_type.Add : operator_pre == '-'? UIS_Instruction_type.Subtract : UIS_Instruction_type.Set;
                addInstruction(tempVar_1, tempVar_1, instructionType, tempVar_2);
            }

            //Addition & Subtraction & Comparision
            if ((i + 1 < operators.Length && operators[i + 1] != '*' && operators[i + 1] != '/')||
                (i == operators.Length-1))
            {
                switch (operator_cur)
                {
                    case '+': instructionType = UIS_Instruction_type.Add; break;
                    case '-': instructionType = UIS_Instruction_type.Subtract; break;
                    case '%': instructionType = UIS_Instruction_type.Remainder; break;
                    case 'e': instructionType = UIS_Instruction_type.Compare_Equal; break;
                    case 'E': instructionType = UIS_Instruction_type.Compare_NotEqual; break;
                    case 's': instructionType = UIS_Instruction_type.Compare_EqualOrSmaller; break;
                    case 'l': instructionType = UIS_Instruction_type.Compare_EqualOrLarger; break;
                    case 'S': instructionType = UIS_Instruction_type.Compare_Smaller; break;
                    case 'L': instructionType = UIS_Instruction_type.Compare_Larger; break;
                    case 'A': instructionType = UIS_Instruction_type.And; break;
                    case 'O': instructionType = UIS_Instruction_type.Or; break;
                    default: break;
                }
                addInstruction(tempVar_1, tempVar_1, instructionType, components[i + 1]);
            }
        }

        // ---------------------------------**** RETURN TYPE : NON-VOID ****---------------------------------
        return tempVar_1;
        // ---------------------------------**** RETURN TYPE : NON-VOID ****---------------------------------
        #endregion
    }
    private string makeTemporaryVariable()
    {
        UIS_Data temporaryVariable = new UIS_Data("T_Var_" + tempVars.Count);
        tempVars.Add(temporaryVariable);
        return temporaryVariable.getDataName();
    }
    #endregion

    // Instruction Sets ===========================================================================================================================================================
    #region
    private void addInstruction(string destination, string operand1, UIS_Instruction_type instructionType, string operand2)
    {
        UIS_Instruction instruction = null;

        switch (instructionType)
        {
            case UIS_Instruction_type.Set : instruction = new UIS_Instruction_Set(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Equals: instruction = new UIS_Instruction_Equals(this, destination, operand1, instructionType, operand2); break;

            case UIS_Instruction_type.Add: instruction = new UIS_Instruction_Add(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Subtract: instruction = new UIS_Instruction_Subtract(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Multiply: instruction = new UIS_Instruction_Multiply(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Divide: instruction = new UIS_Instruction_Divide(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Remainder: instruction = new UIS_Instruction_Remainder(this, destination, operand1, instructionType, operand2); break;

            case UIS_Instruction_type.Compare_Equal: instruction = new UIS_Instruction_Compare_Equal(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Compare_NotEqual: instruction = new UIS_Instruction_Compare_NotEqual(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Compare_EqualOrLarger: instruction = new UIS_Instruction_Compare_EqualOrLarger(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Compare_EqualOrSmaller: instruction = new UIS_Instruction_Compare_EqualOrSmaller(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Compare_Smaller: instruction = new UIS_Instruction_Compare_Smaller(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Compare_Larger: instruction = new UIS_Instruction_Compare_Larger(this, destination, operand1, instructionType, operand2); break;

            case UIS_Instruction_type.And: instruction = new UIS_Instruction_And(this, destination, operand1, instructionType, operand2); break;
            case UIS_Instruction_type.Or: instruction = new UIS_Instruction_Or(this, destination, operand1, instructionType, operand2); break;
        }
        instructions.Add(instruction);
    }
    #endregion

    // Execution ==================================================================================================================================================================
    #region
    public UIS_DataExtractionForm execute()
    {
        //Error Handling not Implemented
        foreach(UIS_Instruction instruction in instructions)
        {
            UIS_DataExtractionForm destinationForm = dataLoad(instruction.destination);
            UIS_DataExtractionForm operandForm1 = dataLoad(instruction.operand1);
            UIS_DataExtractionForm operandForm2 = dataLoad(instruction.operand2);
            
            UIS_Data destination = destinationForm.data;
            UIS_Data operand1 = operandForm1.data;
            UIS_Data operand2 = operandForm2.data;

            string operandStr1 = operand1 == null ? null : operand1.getDataType() == UIS_Data_Type.String ? (string)operand1.getData() : null;
            string operandStr2 = operand2 == null ? null : operand2.getDataType() == UIS_Data_Type.String ? (string)operand2.getData() : null;

            bool operandIsNum1 = operand1 != null && operand1.getDataType() == UIS_Data_Type.Numeric;
            bool operandIsNum2 = operand2 != null && operand2.getDataType() == UIS_Data_Type.Numeric;

            float operandNum1 = operandIsNum1 ? (float)operand1.getData() : 0;
            float operandNum2 = operandIsNum2 ? (float)operand2.getData() : 0;

            instruction.execute(this, destinationForm, operandForm1, operandForm2, destination, operand1, operand2, operandStr1, operandStr2, operandIsNum1, operandIsNum2, operandNum1, operandNum2);
        }

        return dataLoad(new string[] { outcomeTempVar });
    }
    #endregion

    // Data Load ==================================================================================================================================================================
    #region
    public UIS_DataExtractionForm dataLoad(string[] word)
    {
        UIS_DataExtractionForm extractedData = new UIS_DataExtractionForm();
        if (word == null) { return extractedData; }

        UIS_DataExtractionForm tempForm = new UIS_DataExtractionForm();
        UIS_Data data = null;

        // * Preset Keywords
        
        // * Step : Data Initialization 
        if (word[0].Length > 6 && word[0].Substring(0, 6).Equals("T_Var_"))
        {
            data = tempVars[int.Parse(word[0].Substring(6))];
        }
        else
        {
            foreach(UIS_Keyword keyword in UIS_Keyword.UISKeywords)
            {
                if (word[0].Equals(keyword.keyword)) { data = keyword.data; break; }
            }

            if (data == null)
            {
                // Step 1 : Check if the data is Numeric
                try
                {
                    float n = float.Parse(word[0]);
                    data = new UIS_Data("", n, UIS_Data_Type.Numeric);
                }
                catch (Exception e)
                {
                    e.GetType();
                    // Step 2 : Check if the data is String
                    string s = UIS.determineString(word[0]);
                    if (s != null)
                    {
                        data = new UIS_Data("", s, UIS_Data_Type.String);
                    }
                    else
                    {
                        // Step 3 : Check if the data is Registered
                        if (word[0].Equals("this")) { tempForm.data = motherScript.thisData; }
                        else { tempForm = extractData(motherScript, word[0], true); }

                        /*Error!!*/
                        if (tempForm.error != null)
                        {
                            tempForm.dataHolder = motherScript;
                            return tempForm;
                        }
                        /*Error!!*/

                        else { data = tempForm.data; }
                    }
                }
            }
        }

        UIS_Entity currentMotherScript = motherScript;
        for (int i = 1; i < word.Length; i++)
        {
            if (data == null || data.getDataType() != UIS_Data_Type.Entity)
            {
                /*Error!!*/
                /*Error!!*/
            }
            else
            {
                currentMotherScript = UIS.UISO_LIST[(int)data.getData()];
                if (currentMotherScript == null) { UnityEngine.Debug.Log(data.getDataName()); }

                tempForm = extractData(currentMotherScript, word[i], false);

                /*Error!!*/
                if (tempForm.error != null)
                {
                    tempForm.dataHolder = currentMotherScript;
                    return tempForm;
                }
                /*Error!!*/

                else { data = tempForm.data; }
            }
        }

        extractedData.dataHolder = currentMotherScript;
        extractedData.data = data;
        return extractedData;
    }

    private UIS_DataExtractionForm extractData(UIS_Entity motherEntity, string word, bool includeLocalData)
    {
        //If the word is "Ability/Array"
        if (word[word.Length - 1] == ']')
        {
            //motherScript.print("----------------" + mom.entityTypeName + "." + word);
            string[] abilityComponents = word.Split(ABILITY_DEFINERS);
            List<UIS_Data> argumentList = new List<UIS_Data>();
            for (int i = 0; i < abilityComponents.Length; i++)
            {
                abilityComponents[i] = abilityComponents[i].Trim();

                if (abilityComponents[i].Length == 0 || i == 0) { continue; }

                argumentList.Add(tempVars[int.Parse(abilityComponents[i].Substring(6))]);
            }

            UIS_Ability ability = motherEntity.searchAbility(abilityComponents[0]);
            UIS_DataExtractionForm toReturn = ability.execute(argumentList);
            return toReturn;
        }

        //If the word is a common Variable
        else
        {
            UIS_DataExtractionForm dataExtractionForm = new UIS_DataExtractionForm();

            if (includeLocalData)
            {
                UIS_Statement currentStatement = motherStatement;
                while(currentStatement != null)
                {
                    dataExtractionForm.data = currentStatement.searchDataField(word);
                    if (dataExtractionForm.data != null) { return dataExtractionForm; }
                    currentStatement = currentStatement.getMotherStatement();
                }

                dataExtractionForm.data = motherAbility.searchDataField(word);
                if (dataExtractionForm.data != null) { return dataExtractionForm; }
            }
            dataExtractionForm.data = motherEntity.searchDataField(word);
            return dataExtractionForm;
        }
    }
    #endregion

    // Helper Class ==================================================================================================================================================================
    #region
    public UIS_Entity getMotherScript() { return motherScript; }
    public UIS_Ability getMotherAbiltiy() { return motherAbility; }
    public UIS_Statement getMotherStatement() { return motherStatement; }
    #endregion
}
#endregion