using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        const string DEFAULT_PREFIX = "MagSF.";
        const string PARAM_PREFIX = $"{DEFAULT_PREFIX}parameters";

        foreach (KeyValuePair<string, object> arg in args)
        {
            // Check if the key starts with the defined DEFAULT_PREFIX
            if (arg.Key.StartsWith(DEFAULT_PREFIX))
            {
                // If it does, you can then access the key and value
                bool isPersisted = arg.Key.StartsWith(PARAM_PREFIX);
                CPH.SetGlobalVar(arg.Key, arg.Value, isPersisted);
                // CPH.LogDebug($"({isPersisted}){arg.Key}|{arg.Value}");
            }
        }
        return true;
    }
}