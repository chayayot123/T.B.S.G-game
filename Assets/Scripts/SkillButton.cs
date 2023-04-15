using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillButton : MonoBehaviour
{
    // public int skillIndex;
    public UnitScript target;
    UnitScript user;
    public GameObject someObject;
    public tileMapScript TMS;
    public Skill skill;
    public int turn;
    public int cooldownDisplay;

    void Start()
    {
        someObject.SetActive(false);
        TMS = GetComponent<tileMapScript>();
        // Button button = GetComponent<Button>();
        // button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        // get selected character
        Debug.Log(user);

        // get skill of that character
        SkillComponent skillComponent = user.GetComponent<SkillComponent>();
        if (skillComponent != null && skillComponent.skill != null)
        {
            skillComponent.UseSkill(turn, user);
        }
    }

    public void GetUnitData(UnitScript unit)
    {
        user = unit;
    }

    public void SendCurrentTurn(int receiveturn)
    {
        turn = receiveturn;
    }

    public void RevealButton()
    {
        someObject.SetActive(true);
    }

    public void HideButton()
    {
        someObject.SetActive(false);
    }

    public int GetSkillCooldown(int currentTurn)
    {
        SkillComponent skillComponent = user.GetComponent<SkillComponent>();
        return skillComponent.SkillCD(currentTurn);
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 5401f40893fe1502d18a0e9a73eb145a0e221980
