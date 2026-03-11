using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


public class CPHInline
{
    public bool Execute()
    {
        const string STREAM_ASSET_NAME = "MagSFMulticamLayoutUpdater";
        const string LAYOUT = $"{STREAM_ASSET_NAME}.layout";
        const string MULTICAM_SCENE = $"{STREAM_ASSET_NAME}.multicamScene";
        const string MULTICAM_FILTERS_ONLY_1 = $"{STREAM_ASSET_NAME}.multicamFiltersOnly1";
        const string MULTICAM_FILTERS_ONLY_2 = $"{STREAM_ASSET_NAME}.multicamFiltersOnly2";
        const string MULTICAM_FILTERS_BOTH = $"{STREAM_ASSET_NAME}.multicamFiltersBoth";
        const string CAM1_SOURCE_NAME = $"{STREAM_ASSET_NAME}.cam1SourceName";
        const string CAM2_SOURCE_NAME = $"{STREAM_ASSET_NAME}.cam2SourceName";
        const string CAM1_FILTERS_NAME = $"{STREAM_ASSET_NAME}.cam1FiltersName";
        const string CAM2_FILTERS_NAME = $"{STREAM_ASSET_NAME}.cam2FiltersName";

        CPH.TryGetArg(LAYOUT, out string layout);
        CPH.TryGetArg(MULTICAM_SCENE, out string multicamScene);
        CPH.TryGetArg(CAM1_SOURCE_NAME, out string cam1SourceName);
        CPH.TryGetArg(CAM2_SOURCE_NAME, out string cam2SourceName);
        CPH.TryGetArg(CAM1_FILTERS_NAME, out string cam1FiltersName);
        CPH.TryGetArg(CAM2_FILTERS_NAME, out string cam2FiltersName);
        CPH.TryGetArg(MULTICAM_FILTERS_ONLY_1, out string multicamFiltersOnly1);
        CPH.TryGetArg(MULTICAM_FILTERS_ONLY_2, out string multicamFiltersOnly2);
        CPH.TryGetArg(MULTICAM_FILTERS_BOTH, out string multicamFiltersBoth);

        CPH.LogDebug($"{STREAM_ASSET_NAME} input params:");
        CPH.LogDebug($"{MULTICAM_SCENE}|{multicamScene}");
        CPH.LogDebug($"{MULTICAM_FILTERS_ONLY_1}|{multicamFiltersOnly1}");
        CPH.LogDebug($"{MULTICAM_FILTERS_ONLY_2}|{multicamFiltersOnly2}");
        CPH.LogDebug($"{MULTICAM_FILTERS_BOTH}|{multicamFiltersBoth}");
        CPH.LogDebug($"{LAYOUT}|{layout}");
        CPH.LogDebug($"{CAM1_SOURCE_NAME}|{cam1SourceName}");
        CPH.LogDebug($"{CAM1_FILTERS_NAME}|{cam1FiltersName}");
        CPH.LogDebug($"{CAM2_SOURCE_NAME}|{cam2SourceName}");
        CPH.LogDebug($"{CAM2_FILTERS_NAME}|{cam2FiltersName}");

        switch (layout.ToUpper())
        {
            case "ONLY_1":
                CPH.ObsSetSourceVisibility(multicamScene, cam1SourceName, true);
                CPH.ObsSetSourceVisibility(multicamScene, cam2SourceName, false);
                foreach (string _filter in multicamFiltersOnly1.Split(';'))
                {
                    CPH.ObsShowFilter(multicamScene, _filter);
                }
                foreach (string _filter in cam1FiltersName.Split(';'))
                {
                    CPH.ObsShowFilter(multicamScene, cam1SourceName, _filter);
                }
                break;

            case "ONLY_2":
                foreach (string _filter in multicamFiltersOnly2.Split(';'))
                {
                    CPH.ObsShowFilter(multicamScene, _filter);
                }
                foreach (string _filter in cam2FiltersName.Split(';'))
                {
                    CPH.ObsShowFilter(multicamScene, cam2SourceName, _filter);
                }
                CPH.ObsSetSourceVisibility(multicamScene, cam1SourceName, false);
                CPH.ObsSetSourceVisibility(multicamScene, cam2SourceName, true);
                break;

            case "BOTH":
                CPH.ObsSetSourceVisibility(multicamScene, cam1SourceName, true);
                CPH.ObsSetSourceVisibility(multicamScene, cam2SourceName, true);

                foreach (string _filter in multicamFiltersBoth.Split(';'))
                {
                    CPH.ObsShowFilter(multicamScene, _filter);
                    CPH.LogDebug($"\t\t filter [{_filter}] scene [{multicamScene}]");
                }
                break;
        }

        return true;
    }
}