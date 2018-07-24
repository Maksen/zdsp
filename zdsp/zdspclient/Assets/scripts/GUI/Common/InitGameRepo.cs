using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class InitGameRepo : MonoBehaviour
{

    public TextAsset gameData;

    void Awake()
    {
#if UNITY_EDITOR
        if (gameData != null)
            GameRepo.InitSkillRepo(gameData.text);
#endif
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
