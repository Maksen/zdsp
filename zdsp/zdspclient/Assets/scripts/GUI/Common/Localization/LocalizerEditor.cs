using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class LocalizerEditor : MonoBehaviour
{
    public TextAsset gameData;

    void Awake()
    {
#if UNITY_EDITOR
        if (gameData != null)
           GameRepo.InitLocalizerRepo(gameData.text);
#endif
    }
}
