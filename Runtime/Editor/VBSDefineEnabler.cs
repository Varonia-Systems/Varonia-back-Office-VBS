#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class VBSDefineEnabler
{
    static VBSDefineEnabler()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!symbols.Contains("VBO_VBS"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                symbols + ";VBO_VBS"
            );
        }
    }
}
#endif