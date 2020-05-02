using System;
using System.Data;
using System.Collections.Generic;
// ------------------- UI Setting -------------------
public class UIS_DataExtractionForm
{
    public UIS_Entity dataHolder = null;
    public UIS_Data  data = null;
    public UIS_Error error = null;
    public UIS_DataExtractionForm() { }
    public UIS_DataExtractionForm(UIS_Data data) : this(null, data, null) { }
    public UIS_DataExtractionForm(UIS_Entity dataHolder) : this(dataHolder, null, null) { }
    public UIS_DataExtractionForm(UIS_Error error) : this(null, null, error) { }
    public UIS_DataExtractionForm(UIS_ErrorType error, UIS_Entity dataHolder) : this(null, null, new UIS_Error(error, dataHolder)) { }
    public UIS_DataExtractionForm(UIS_Entity dataHolder, UIS_Data data, UIS_Error error)
    {
        this.dataHolder = dataHolder;
        this.data = data;
        this.error = error;
    }
}

public class UISO_IndexStack
{
    private class Node
    {
        public int index = -1;
        public Node next = null;
        public Node(int index) { this.index = index; }
    }

    private Node head = null;

    public void push(int index)
    {
        Node n = new Node(index);
        if (head == null) { head = n; }
        else
        {
            head.next = n;
            head = n;
        }
    }

    public int peak() { return head == null ? -1 : head.index; }
    public int pop()
    {
        if (head == null) { return -1; }
        int toReturn = head.index;
        head = head.next;
        return toReturn;
    }
}


// ------------------- UIS : User Interface Script -------------------
public interface UIS_Command
{
    UIS_DataExtractionForm execute();
    UIS_Command duplicate(UIS_Ability motherAbility, UIS_Statement motherStatement);
}

public interface UIS_CommandChunk
{
    List<UIS_Data> getDataFields();
    UIS_Data searchDataField(string word);
}

// ------------------- Preset Keywords-------------------
public class UIS_Keyword
{
    public static List<UIS_Keyword> UISKeywords = new List<UIS_Keyword>()
    {
        new UIS_Keyword("true", new UIS_Data("", 1f, UIS_Data_Type.Numeric)),
        new UIS_Keyword("false", new UIS_Data("", 0f, UIS_Data_Type.Numeric)),
        new UIS_Keyword("null", new UIS_Data("", -1, UIS_Data_Type.Entity)),
    };

    public string keyword;
    public UIS_Data data;
    public UIS_Keyword(string keyword, UIS_Data data)
    {
        this.keyword = keyword;
        this.data = data;
    }
}
    
// ------------------- UIS : Unified Interface Script -------------------
#region
public enum UIS_CompileProcess { None, Initialization, OnRun, CustomAbility }
public static class UIS
{
    public static UIS_Data globalEntity = null;
    public static List<UIS_Entity> UISEntityType = new List<UIS_Entity>();
    public static List<UIS_Entity> UISO_LIST = new List<UIS_Entity>(8192);
    public static int UISO_LIST_COUNT = 0;
    public static UISO_IndexStack UISO_LIST_INDEX_STACK = new UISO_IndexStack();

    public static void systemSetUp()
    {
        globalEntity = null;
        UISEntityType = new List<UIS_Entity>();
        UISO_LIST = new List<UIS_Entity>(8192);
        UISO_LIST_COUNT = 0;
        UISO_LIST_INDEX_STACK = new UISO_IndexStack();
    }

    public static UIS_Entity getUISEfromUISD(UIS_Data data)
    {
        int index = (int)data.getData();
        if(index == -1) { return null; }
        return UISO_LIST[index];
    }

    public static UIS_Entity findEntityType(string entityTypeName)
    {
        foreach (UIS_Entity entity in UISEntityType)
        {
            if (entity.entityTypeName.Equals(entityTypeName))
            {
                return entity;
            }
        }
        return null;
    }

    public static UIS_Entity createEntity(UIS_Object owner, string entityTypeName)
    {
        foreach(UIS_Entity entity in UISEntityType)
        {
            if (entity.entityTypeName.Equals(entityTypeName))
            {
                return entity.duplicate(owner);
            }
        }
        return null;
    }

    public static UIS_Entity compile(UIS_Object caller, string script, bool debugMode)
    {
        //Runs on the first time, but passed on the second run
        if (globalEntity == null)
        {
            //Mock-Up So in the next iteration, this section will be skipped.
            globalEntity = new UIS_Data("");
            globalEntity = new UIS_Data("global", compile(null, "globalEntity{create[]{}}", false).getID(), UIS_Data_Type.Entity);
        }

        // Class Parse
        UIS_Entity firstEntity = null;
        int lastUISSpot = 0;
        for (int i = 0; i < script.Length; i++)
        {
            if (script[i] == '{')
            {
                // Name Parse
                string entityName = script.Substring(lastUISSpot, i - lastUISSpot).Trim();

                // Duplicate Check
                UIS_Entity searchedEntity = createEntity(caller, entityName);
                if ( searchedEntity != null ) { return searchedEntity; }

                // Build a New Entity
                UIS_Entity entity = new UIS_Entity(caller, entityName);
                entity.debugMode = debugMode;
                UISEntityType.Add(entity);

                // Ability Parse
                int lastAbilitySpot = i + 1;
                for (i += 1; i < script.Length; i++)
                {
                    if (script[i] == '}') { lastUISSpot = i + 1; break; }
                    if (script[i] == '[')
                    {
                        // Name Parse
                        string abilityName = script.Substring(lastAbilitySpot, i - lastAbilitySpot).Trim();

                        int bracketStartIndex = i + 1;
                        int letterCount = 0;
                        for (i += 1; i < script.Length; i++)
                        {
                            if (script[i] == ']')
                            {
                                // Argument Parse
                                string[] argumentNames = script.Substring(bracketStartIndex, letterCount).Trim().Split(new char[] { ',', ' ' });
                                List<string> argumentsNames = new List<string>();
                                foreach (string argumentName in argumentNames)
                                {
                                    string trimmed = argumentName.Trim();
                                    if (trimmed.Length != 0) { argumentsNames.Add(trimmed); }
                                }

                                for (i += 1; i < script.Length; i++)
                                {
                                    if (script[i] == '{')
                                    {
                                        bracketStartIndex = i + 1;
                                        letterCount = 0;
                                        int bracketCount = 1;
                                        for (i += 1; i < script.Length; i++)
                                        {
                                            if (script[i] == '{') { bracketCount++; }
                                            if (script[i] == '}') { bracketCount--; }
                                            if (bracketCount == 0)
                                            {
                                                // Body Parse
                                                UIS_Ability ability = new UIS_Ability(entity, abilityName, script.Substring(bracketStartIndex, letterCount).Trim());
                                                ability.setArguments(argumentsNames);
                                                entity.addAbility(ability);
                                                lastAbilitySpot = i + 1;
                                                break;
                                            }
                                            else { letterCount++; }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                            else { letterCount++; }
                        }
                    }
                }
                firstEntity = firstEntity == null ? entity.duplicate(caller) : firstEntity;
            }
        }

        return firstEntity;
    }

    public static string determineString ( string input )
    {
        if (input == null || input.Length == 0) { return null; }

        if (input[0] == '"' && input[input.Length - 1] == '"')
        {
            return input.Substring(1, input.Length - 2);
        }
        else
        {
            return null;
        }
    }

    public static void initiateCommand(UIS_Ability motherAbility, UIS_Statement motherStatement, List<UIS_Command> commands, string sourceCode)
    {
        int bracketDepth = 0;
        int singleQuoteDepth = 0;
        int doubleQuoteDepth = 0;

        string currentString = "";

        int lineCount = 0;

        for (int i = 0; i < sourceCode.Length; i++)
        {
            char c = sourceCode[i];

            //Non-Accessable Area Definition
            if (c == '(') { bracketDepth++; }
            if (c == ')') { bracketDepth--; }

            if (c == '\'') { singleQuoteDepth++; }
            if (c == '\'') { singleQuoteDepth--; }

            if (c == '"') { doubleQuoteDepth++; }
            if (c == '"') { doubleQuoteDepth--; }

            //Accessable Area Zone
            if (bracketDepth == 0 && singleQuoteDepth == 0 && doubleQuoteDepth == 0)
            {
                //String Flush
                if (c == ';' || i == sourceCode.Length-1)
                {
                    currentString = currentString.Trim();
                    if (currentString.Length != 0)
                    {
                        commands.Add(new UIS_CommandLine(motherAbility, motherStatement, currentString, lineCount++));
                    }
                    currentString = "";
                    continue;
                }

                // Statement Parse
                if ((i + 1) < sourceCode.Length)
                {
                    string parsed = sourceCode.Substring(i, 2);

                    if (parsed.Equals("if"))
                    {
                        i += 2;

                        int innerbracketCount;
                        int startIndex;
                        int endIndex;

                        // Condition Parse ======================================================================================================
                        string condition = "";
                        innerbracketCount = 0;
                        startIndex = -1;
                        endIndex = -1;
                        for (; i < sourceCode.Length; i++)
                        {
                            if (sourceCode[i] == '(') { innerbracketCount++; if (innerbracketCount == 1) { startIndex = i + 1; } }
                            if (sourceCode[i] == ')') { innerbracketCount--; if (innerbracketCount == 0) { endIndex = i - 1; } }

                            /*Error!!*/
                            if (startIndex == -1 && endIndex != -1) { }
                            /*Error!!*/

                            if (endIndex != -1)
                            {
                                condition = sourceCode.Substring(startIndex, endIndex - startIndex + 1).Trim();
                                break;
                            }
                        }
                        //motherAbility.getMotherScript().print("Condition : " + condition);

                        // Body Parse ===========================================================================================================
                        string body = "";
                        innerbracketCount = 0;
                        startIndex = -1;
                        endIndex = -1;
                        for (; i < sourceCode.Length; i++)
                        {
                            if (sourceCode[i] == '{') { innerbracketCount++; if (innerbracketCount == 1) { startIndex = i + 1; } }
                            if (sourceCode[i] == '}') { innerbracketCount--; if (innerbracketCount == 0) { endIndex = i - 1; } }

                            /*Error!!*/
                            if (startIndex == -1 && endIndex != -1) { }
                            /*Error!!*/

                            if (endIndex != -1)
                            {
                                body = sourceCode.Substring(startIndex, endIndex - startIndex + 1).Trim();
                                break;
                            }
                        }

                        // Statement Compose =====================================================================================================
                        commands.Add(new UIS_Statement(motherAbility, motherStatement, condition, body));
                        continue;
                    }
                }
            }

            //Current CommandLine Update
            currentString += c;
        }
    }
}
#endregion

// ------------------- UIS Error Handling -------------------
#region
public enum UIS_ErrorType
{
    None,
    ExecutingNotCompiledScript,
    ComplieProcessSettingFailure,
    UnsetVariableReference,
    WrongUIScriptObjectReference,
    TryToComputeInconvertableData,
    ReferingToNonExistingAbility,
    InvalidArgumentPassing,
    NullPointerAccess,
    UndoneAbilityExpression,
    ForTestPurpose,
}
public class UIS_Error
{
    private UIS_ErrorType errorType = UIS_ErrorType.None;
    private UIS_CompileProcess complieProcess = UIS_CompileProcess.None;
    private string errorLine = null;
    private int lineNum = 0;
    private int wordNum = 0;

    public UIS_Error(UIS_ErrorType errorType)
    {
        this.errorType = errorType;
    }
    public UIS_Error(UIS_ErrorType errorType, UIS_Entity script)
        : this(script.UIS_Info_compileProcess, script.UIS_Info_commandLine, script.UIS_Info_lineIndex, script.UIS_Info_commandLineReader.getCurrentIndex())
    {
        this.errorType = errorType;
    }
    public UIS_Error(UIS_CompileProcess complieProcess, string errorLine, int lineNum, int wordNum)
    {
        this.complieProcess = complieProcess;
        this.errorLine = errorLine;
        this.lineNum = lineNum;
        this.wordNum = wordNum;
    }

    public UIS_ErrorType getErrorType() { return errorType; }
    public string getErrorLine() { return errorLine; }
    public int getLineNum() { return lineNum; }
    public int getWordNum() { return wordNum; }
    public string getErrorDescription()
    {
        if (errorType == UIS_ErrorType.None) { return "Ran Successfully"; }
        string toReturn = errorType.ToString() + " [<" + complieProcess.ToString() + "> Line : " + lineNum + "]     - ";
        string[] lineBreakDown = errorLine.Split(new char[] { ' ' });
        for (int i = 0; i < lineBreakDown.Length; i++)
        {
            if (i == wordNum) { toReturn += "*["; }
            toReturn += lineBreakDown[i] + " ";
            if (i == wordNum) { toReturn += "]*"; }
        }

        return toReturn;
    }
}
#endregion

// ------------------- Helper Class -------------------
#region
public class LineReader
{
    private string line = null;
    private List<string> words = null;
    private int wordIndex = -1;

    public LineReader(string line)
    {
        words = new List<string>();
        this.line = line;
        process(line);
    }

    private void process(string line)
    {
        string[] currentLine_temp = line.Split(new char[] { ' ' });
        foreach (string word in currentLine_temp) { words.Add(word); }
    }

    public string popNextWord()
    {
        if (wordIndex + 1 < words.Count) { wordIndex++; return words[wordIndex]; }
        else { return null; }
    }

    public string popAllLeft(bool inSentence)
    {
        string toReturn = "";
        while (hasNextWord())
        {
            toReturn += popNextWord();
            if (inSentence) { toReturn += " "; }
        }

        return toReturn.Trim();
    }

    public bool hasNextWord()
    {
        return (wordIndex + 1) < words.Count;
    }

    public void restoreAll()
    {
        wordIndex = -1;
    }

    public void restore()
    {
        if (wordIndex < -1) { return; }
        wordIndex--;
    }

    public int getCurrentIndex()
    {
        return wordIndex;
    }
}
#endregion