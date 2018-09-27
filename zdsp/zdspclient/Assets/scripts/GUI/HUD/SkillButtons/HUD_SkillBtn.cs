using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HUD_SkillBtn : MonoBehaviour
{
    public Image skillImage;
    public GameObject emptySkillIconObj;
    public GameObject skillIconObj;
    public Image cdImage;
    public Text cdText;

    private Button button;
    private float cdTime;
    private float totalCdDuration;
    private Coroutine countdownTimer = null; //cooldown Coroutine is stopped when gameobject deactive;
    private bool resumeCD = false;
    public int skillid;
    private int index;
    private HUD_Skills parent;
    public bool HasSkill = false;


    public delegate void SkillCast(int param);

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Init(SkillCast CastSkill, int idx, HUD_Skills myParent)
    {
        button.onClick.AddListener(delegate { CastSkill(skillid); });
        skillid = idx;
        parent = myParent;
    }

    public void OnSkillUpdated(int idx)
    {
        skillid = idx;
    } 

    public void UpdateSprite(string path)
    {
        HasSkill = true;
        emptySkillIconObj.SetActive(false);
        skillIconObj.SetActive(true);
        skillImage.sprite = ClientUtils.LoadIcon(path) as Sprite;
    }

    public void SetEmptySkill()
    {
        HasSkill = false;
        emptySkillIconObj.SetActive(true);
        skillIconObj.SetActive(false);
    }

    public void SetSkillImage(Sprite sprite)
    {
        //HasSkill = sprite != null;
        //if (sprite == null)
        //    skillImage.sprite = emptySkillSprite;
        //else
        //    skillImage.sprite = sprite;
    }

    void OnEnable()
    {
        //if (resumeCD)
        {
            //resumeCD = false; //always refresh the cooldown will be ok. yuning
            PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
            if (cdstate == null)
                return;

            float remainingTime = cdstate.mCDEnd[index] - Time.time;
            if (remainingTime > 0)
            {
                cdTime = remainingTime; 
                countdownTimer = StartCoroutine(Cooldown(totalCdDuration));
            }
            else
                StopCooldown();  
        }
    }

    public void StartCooldown(float duration)
    {
        totalCdDuration = duration;
        cdTime = duration;

        cdImage.gameObject.SetActive(true);
        cdText.gameObject.SetActive(true);

        OnCoolDownChanged(duration);
    }

    private void OnCoolDownChanged(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            if (countdownTimer != null)
                StopCoroutine(countdownTimer);
            countdownTimer = StartCoroutine(Cooldown(duration));
        } 
    }

    private IEnumerator Cooldown(float duration)
    {
        while (cdTime > 0)
        {
            cdImage.fillAmount = cdTime / duration;
            cdText.text = ((int)Math.Ceiling(cdTime)).ToString();

            cdTime -= Time.deltaTime;
            yield return null;
        }
        yield return null;
        //if (!CheckGlobalCooldown())
        EndCooldown();
    }

    public void StopCooldown()
    {
        cdTime = 0;
        cdImage.fillAmount = 0;
        cdText.text = "0";
        cdImage.gameObject.SetActive(false);
        cdText.gameObject.SetActive(false);
    }

    public void IncreaseCooldown(float perct)
    {
        float changeamt = perct * 0.01f * totalCdDuration;
        PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
        cdstate.mCDEnd[index] += changeamt;
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
       
        if (cdTime > 0)
        {
            cdTime += changeamt; 
            //clear the timer and restart. do not change the totalcdDuration which is the original skillcooldown
            OnCoolDownChanged(cdTime);
        }
    }


    /*private bool CheckGlobalCooldown()
    {
        PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
        if (cdstate == null)
            return false;

        if (cdstate.HasGlobalCooldown())
        {
            float duration = cdstate.mGCDEnd - cdstate.mGCDStart;
            cdTime = cdstate.mGCDEnd - Time.time;
            countdownTimer = StartCoroutine(Cooldown(duration));
            return true;
        }

        return false;
    }*/

    private void EndCooldown()
    {
        Debug.Log("cooldown ending ......");
        cdImage.gameObject.SetActive(false);
        cdText.gameObject.SetActive(false);
        countdownTimer = null;
         
        parent.PlaySkillCDFlare(index);
    }

    void OnDisable()
    {
        //if (countdownTimer != null)
            //StopCoroutine(countdownTimer);
        //countdownTimer = null;
        //if (cdTime > 0) //skill cooldown maybe start when the skillbutton is hiding.  yuning.
        //    resumeCD = true;
        //else
        //    resumeCD = false;
    }

}
