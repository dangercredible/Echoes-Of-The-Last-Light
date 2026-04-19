#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Disables Burst compilation in the Editor on project load (same as Jobs / Burst / Enable Compilation).
/// Use when Burst fails with "Unable to load the unmanaged library" (common on some Windows setups).
/// Delete this file or turn compilation back on in the Burst menu when the environment is fixed.
/// </summary>
[InitializeOnLoad]
internal static class DisableBurstCompilationWorkaround
{
    static DisableBurstCompilationWorkaround()
    {
        EditorPrefs.SetBool("BurstCompilation", false);
    }
}
#endif
