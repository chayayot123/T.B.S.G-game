using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : MonoBehaviour
{
    public Skill skill;
    public int cooldownTurns = 2;
    private int lastUsedTurn = -1;
    private bool isOnCooldown = false;
    public int cooldowncount = 0;

    public void UseSkill(int currentTurn, UnitScript user)
    {
        Debug.Log(skill.skillName);
        if (!isOnCooldown)
        {
            if (skill.skillName == "Heal") {
                user.dealDamage(skill.skillpower);
            } else if (skill.skillName == "Sample2") {
                Debug.Log("Sample2");
            } else {
                Debug.Log("FK U");
            }

            lastUsedTurn = currentTurn;
            isOnCooldown = true;
        } else {
            Debug.Log("Skill on cooldown");
        }
    }

    public void UpdateCooldown(int currentTurn)
    {
        if (isOnCooldown && currentTurn >= lastUsedTurn + cooldownTurns)
        {
            isOnCooldown = false;
            cooldowncount--;
            if (cooldowncount < 0) {
                cooldowncount = 2;
            }
        }
    }

    public int SkillCD(int currentTurn)
    {
        return cooldowncount;
    }
}
