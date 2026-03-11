using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// chupinhado de https://docs.streamer.bot:8443/examples/parse-json-utility

public class CPHInline
{
    public bool Execute()
    {
        const string STREAM_ASSET_NAME = "MagSFParseJSON";
        const string VAR_PREFIX = "MagSF";
        const string JSON_VARIABLE = "json";
        const string JSON_LINE_COUNT = "lineCount";

        if (!CPH.TryGetArg(JSON_VARIABLE, out string json))
        {
            json = string.Empty;
            CPH.LogWarn($"Parse JSON :: Missing argument 'json', trying from a possible file read throught variables lineCount and json#");
            if (!CPH.TryGetArg(JSON_LINE_COUNT, out int jsonLineCount))
            {
                CPH.LogWarn($"Parse JSON :: Missing arguments 'lineCount', returning...");
                return true;
            }

            // CPH.LogDebug($"Total Json Lines: {jsonLineCount}");
            for (int i = 0; i < jsonLineCount; i++)
            {
                CPH.TryGetArg($"{JSON_VARIABLE}{i}", out string _json);
                json += _json;
            }
            // CPH.LogDebug($"Loaded Json:\n {json}");
            CPH.SetArgument(JSON_VARIABLE, json);
        }

        if (!CPH.TryGetArg("prefix", out string prefix))
        {
            prefix = VAR_PREFIX;
        }

        JToken token = JToken.Parse(json);
        Dictionary<string, object> dict = new Dictionary<string, object>();
        JTokenToDict(dict, token, prefix);

        foreach (KeyValuePair<string, object> item in dict)
        {
            CPH.SetArgument(item.Key, item.Value);
        }

        return true;
    }

    private static void JTokenToDict(Dictionary<string, object> dict, JToken token, string prefix)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (JProperty property in token.Children<JProperty>())
                {
                    string newPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    JTokenToDict(dict, property.Value, newPrefix);
                }
                break;
            case JTokenType.Array:
                int index = 0;
                foreach (JToken arrayItem in token.Children())
                {
                    string newPrefix = $"{prefix}[{index}]";
                    JTokenToDict(dict, arrayItem, newPrefix);
                    index++;
                }
                break;
            default:
                if (!string.IsNullOrEmpty(prefix))
                {
                    dict[prefix] = ((JValue)token).Value;
                }
                break;
        }
    }
}