using UnityEngine;
using UnityEngine.UI;

public class DamageLabelChangeFont_Pili : MonoBehaviour {

    public GameObject DamageLabel;
    public Font NormalDamage;
    public Font Heal;
    public Font Critical;
    public Font DebuffBleed;
    public Font Miss;
    public Font Dodge;
	public Font NormalDamageFriendly;
	public Font CriticalFriendly;
	public Font DOTFriendly;

    Text newFont;

    // Use this for initialization
    void Awake()
    {
        newFont = DamageLabel.GetComponent<Text>();
    }

    public void setNormalDamage() {
        newFont.font = NormalDamage;
    }

    public void setDebuffBleed()
    {
        newFont.font = DebuffBleed;
    }

    public void setCritical()
    {
        newFont.font = Critical;
    }

    public void setDodge()
    {
        newFont.font = Dodge;
    }

    public void setMiss()
    {
        newFont.font = Miss;
    }

    public void setHeal()
    {
        newFont.font = Heal;
    }
	
	public void setNormalDamageFriendly() {
        newFont.font = NormalDamageFriendly;
    }
	
	public void setCriticalFriendly()
    {
        newFont.font = CriticalFriendly;
    }
	
	public void setDOTFriendly()
    {
        newFont.font = DOTFriendly;
    }

}
