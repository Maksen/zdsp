using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCEffect : MonoBehaviour
{
    public GameObject basicAttack;
    public List<GameObject> skillEffects;
    public GameObject DyingEffect;

#if UNITY_EDITOR
    public void AddSkillEffects(List<GameObject> newSkillEf)
    {
        if (newSkillEf.Count == 0)
            return;

        if (skillEffects == null)
            skillEffects = new List<GameObject>();

        if (!Enumerable.SequenceEqual(skillEffects.OrderBy(x => x.name), newSkillEf.OrderBy(x => x.name)))
        {
            skillEffects = newSkillEf;
        }
    }

    public void ClearEffects()
    {
        basicAttack = null;
        if (skillEffects != null)
            skillEffects.Clear();
        DyingEffect = null;
    }
#endif

    public GameObject GetSkillEffect(string efname)
    {
        if(skillEffects != null)
        {
            return skillEffects.FirstOrDefault(x => x.name == efname.ToLower());
        }
        return null;
    }

    public GameObject GetEffect(string effectname)
    {
        if (effectname == "atk1")
        {
            return basicAttack;
        }
        else if (effectname == "dying")
        {
            return DyingEffect;
        }
        else
        {
            return GetSkillEffect(effectname);
        }
    }
}
