using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : MonoBehaviour
{
    public Skill skill;
    public int cooldownTurns = 3;
    private int lastUsedTurn = -1;
    private bool isOnCooldown = false;
    public int cooldowncount = 3;

    private void Awake()
    {
        cooldowncount = cooldownTurns;
    }

    public void UseSkill(int currentTurn, UnitScript user)
    {
        Debug.Log(skill.skillName);
        if (!isOnCooldown)
        {
            if (skill.skillName == "Heal")
            {
                user.heal(skill.skillpower);
            }
            else if (skill.skillName == "dealdamage")
            {
                user.dealDamage(skill.skillpower);
                Debug.Log("Sample2");
            }
            else
            {
                Debug.Log("FK U");
            }

            lastUsedTurn = currentTurn;
            isOnCooldown = true;
        }
        else
        {
            Debug.Log("Skill on cooldown");
        }
    }

    public void UpdateCooldown(int currentTurn)
    {
        if (isOnCooldown && currentTurn >= lastUsedTurn + cooldownTurns)
        {
            isOnCooldown = false;
            if (cooldowncount > 0)
            {
                cooldowncount--;
            }
            else
            {
                cooldowncount = cooldownTurns;
            }
        }
    }

    public int SkillCD(int currentTurn)
    {
        return cooldowncount;
    }
}
