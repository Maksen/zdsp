using UnityEngine;
using UnityEngine.UI;

public class Hero_BondData : MonoBehaviour
{
    [SerializeField] Text bondNameText;
    [SerializeField] Text bondLevelText;
    [SerializeField] Text heroCountText;
    [SerializeField] Transform heroDataParent;
    [SerializeField] GameObject heroDataPrefab;
    [SerializeField] Transform bondLevelDataParent;
    [SerializeField] GameObject bondLevelDataPrefab;
    [SerializeField] ScrollRect bondScrollRect;
}
