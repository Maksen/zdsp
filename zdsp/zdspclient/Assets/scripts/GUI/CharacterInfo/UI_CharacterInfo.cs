using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class UI_CharacterInfo : BaseWindowBehaviour
{
    public UI_CharacterInfoLeftSide mLeftWindow;
    public UI_CharacterInfoTabOne mTabOne;
    public UI_CharacterInfoTabTwo mTabTwo;
    public UI_CharacterInfoTabThreeFour mTabThreeFour;

    //Avatar

    public override void OnShowWindow()
    {
        if (mTabOne.gameObject.GetActive())
        {
            mTabOne.OnRegainWindowContext();
        }
        else if (mTabTwo.gameObject.GetActive())
        {
            mTabTwo.OnRegainWindowContext();
        }
        else if (mTabThreeFour.gameObject.GetActive())
        {
            mTabThreeFour.OnRegainWindowContext();
        }

        mLeftWindow.OnRegainWindowContext();
    }

    public override void OnLevelChanged()
    {
        //CombatHiearchy not destroyed on level changed, therefore do what u need here
        base.OnLevelChanged();
    }
}
