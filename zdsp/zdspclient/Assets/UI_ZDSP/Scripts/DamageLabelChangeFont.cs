using UnityEngine;
using UnityEngine.UI;

public class DamageLabelChangeFont : MonoBehaviour {

    public GameObject DamageLabel;
    
    public Font NormalDamage_E;
	public Font NormalDamage_F;
    public Font DOT;
	public Font Heal;
    public Font Critical;
	public Font Total;
	public Font Miss;
	public Font Dodge;
	
    Text newFont;

    // Use this for initialization
    void Start()
    {
        newFont = DamageLabel.GetComponent<Text>();
    }

    public void setNormalDamage_E() {
        newFont.font = NormalDamage_E;
    }
	
	public void setNormalDamage_F() {
        newFont.font = NormalDamage_F;
    }
	
	public void setDOT() {
        newFont.font = DOT;
    }
	
	public void setHeal() {
        newFont.font = Heal;
    }
	
	public void setCritical() {
        newFont.font = Critical;
    }
	
	public void setTotal() {
        newFont.font = Total;
    }
	
	public void setMiss() {
        newFont.font = Miss;
    }
	
	public void setDodge() {
        newFont.font = Dodge;
    }

}
