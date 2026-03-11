using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        const string STREAM_ASSET_NAME = "MagSFGlobalVarsToArguments";
        const string PREFIXES = $"{STREAM_ASSET_NAME}.prefixes";

        CPH.TryGetArg(PREFIXES, out string prefixes);

        // CPH.LogDebug($"{STREAM_ASSET_NAME} input params:");
        // CPH.LogDebug($"{PREFIXES}|{prefixes}");

        List<GlobalVariableValue> globalVarList = CPH.GetGlobalVarValues(false);

        foreach (GlobalVariableValue globalVar in globalVarList)
        {
            //Get name of current globalVar
            string varName = globalVar.VariableName;
            // DateTime lastWrite = globalVar.LastWrite;

            string[] _prefixes = prefixes.Split('|');
            foreach (string prefix in _prefixes)
            {
                if (varName.StartsWith(prefix))
                {
                    var varValue = CPH.GetGlobalVar<string>(varName, false);

                    // CPH.LogDebug($"{varName}|{varValue}");
                    CPH.SetArgument(varName, varValue);
                }
            }
        }

        return true;
    }
}
