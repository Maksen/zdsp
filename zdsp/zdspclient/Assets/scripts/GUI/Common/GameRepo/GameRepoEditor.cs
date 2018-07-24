using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class GameRepoEditor : MonoBehaviour
{
    public TextAsset gameData;

    void Awake()
    {
#if UNITY_EDITOR
        if (gameData != null)
            GameRepo.InitCommonEditor(gameData.text);
#endif
    }
}
