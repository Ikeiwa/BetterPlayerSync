using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class BetterPlayerSyncTab
{
    [MenuItem("Tools/BetterPlayerSync/Add To Scene")]
    public static void AddBetterPlayerSync()
    {
        PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/BetterPlayerSync"));
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
