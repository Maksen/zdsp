using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
# endif

namespace ViewPlayer
{
    public class ViewPlayeEquipementControl : MonoBehaviour
    {

        public GameObject primaryWeaponPath;
        public GameObject primaryWeaponFile;
        public GameObject mainObject;

        private GameObject targetModel;

        void Start()
        {

        }

        void Update()
        {

        }

        public void OnLoadPrimaryWeapon()
        {
            mainObject.SendMessage("ShowDialog", "This not working, please use 'LoadFile'", SendMessageOptions.DontRequireReceiver);

            string assetPath = primaryWeaponPath.GetComponent<InputField>().text;//"Models_Characters_weapon_prefabs/";
            if (assetPath.Length == 0)
            {
                mainObject.SendMessage("ShowDialog", "Primary weapon assets path is empty!", SendMessageOptions.DontRequireReceiver);
                return;
            }
            string fileName = primaryWeaponFile.GetComponent<InputField>().text;
            if (fileName.Length == 0)
            {
                mainObject.SendMessage("ShowDialog", "Primary weapon is empty!", SendMessageOptions.DontRequireReceiver);
                return;
            }

            GameObject model = ViewPlayerModelControl.Instance.GetMainModel();
            //AvatarAdaptorScript avataradaptor = model.GetComponent<AvatarAdaptorScript>();
            //if (avataradaptor)
            //{
            //    mainObject.SendMessage("ShowDialog", "This not working, please use 'LoadFile'", SendMessageOptions.DontRequireReceiver);
            //    EquippedInventoryData eqinvdata = userCharacters[btnindex].EquippedInventory;
            //    avataradaptor.OnCharacterEquipmentsChange(eqinvdata, 0);
            //    avataradaptor.InitAttachList();
            //    int layer = avataradaptor.gameObject.layer;
            //    ClientUtils.SetLayerRecursively(avataradaptor.gameObject, layer);
            //    AdaptorAsyncLoadManager.Instance.LoadAdaptor(avataradaptor, assetPath + "/" + fileName, "", "", "weapon_r", layer, EquipmentType.Weapon);
            //}
        }

        public void LoadPrimartWeaponFromFile()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("select .prefeb file of model", "Assets/", "prefab");
            if (path.Length > 0)
            {
                int subLen = Application.dataPath.Length - 6;
                string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");
                GameObject weapon = AssetDatabase.LoadAssetAtPath<GameObject>(assetRelativePath) as GameObject;
                if (weapon)
                {
                    GameObject model = ViewPlayerModelControl.Instance.GetMainModel();
                    //AvatarAdaptorScript avataradaptor = model.GetComponent<AvatarAdaptorScript>();
                    //if (avataradaptor)
                    //{
                    //    avataradaptor.InitAttachList();
                    //    int layer = avataradaptor.gameObject.layer;
                    //    ClientUtils.SetLayerRecursively(avataradaptor.gameObject, layer);
                    //    avataradaptor.AttachByBodyPart("weapon_r", weapon, "", layer);
                    //}
                    var weaponTransformParentTransform = model.transform.Find("root").Find("weapon");
                    if (weaponTransformParentTransform)
                    {
                        var go = Instantiate(weapon);
                        go.transform.SetParent(weaponTransformParentTransform.gameObject.transform);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        go.transform.localScale = Vector3.one;
                    }
                    
                }

            }
#endif
        }

        void OnLoadedModel(GameObject model)
        {

        }

        void OnDestroy()
        {
            primaryWeaponPath = null;
            primaryWeaponFile = null;
            mainObject = null;
            targetModel = null;
        }
    }
}