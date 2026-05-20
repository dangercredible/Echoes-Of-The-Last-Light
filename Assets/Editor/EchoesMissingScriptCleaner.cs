using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EchoesMissingScriptCleaner
{
    static EchoesMissingScriptCleaner()
    {
        EditorApplication.delayCall += CleanOpenScenes;
    }

    static void CleanOpenScenes()
    {
        int removed = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                removed += CleanHierarchy(root);

            if (removed > 0)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        if (removed > 0)
            Debug.Log($"EchoesMissingScriptCleaner removed {removed} missing script component(s) from open scene objects.");
    }

    static int CleanHierarchy(GameObject root)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        foreach (Transform child in root.transform)
            removed += CleanHierarchy(child.gameObject);

        return removed;
    }
}
