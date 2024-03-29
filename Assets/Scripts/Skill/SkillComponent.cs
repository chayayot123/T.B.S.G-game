using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : MonoBehaviour
{
    public Skill skill;
    public int cooldownTurns;
    private int lastUsedTurn = -1;
    private bool isOnCooldown = false;
    public int cooldowncount;
    // public GameObject skillEffectPrefab;


    // private void Awake()
    // {
    //     cooldowncount = cooldownTurns;
    // }

    public void UseSkill(int currentTurn, UnitScript user)
    {
        Debug.Log(skill.skillName);
        if (!isOnCooldown)
        {
            if (skill.skillName == "Heal")
            {
                user.heal(skill.skillpower);
                // GameObject skillEffectInstance = Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
                // skillEffectInstance.transform.SetParent(transform); // optional: attach the effect to the character's transform for easier management
            }
            else if (skill.skillName == "berserkMode")
            {
                Debug.Log("Sample2");
                user.berserkMode(skill.skillpower);
            }
            else if (skill.skillName == "AtkBuff")
            {
                // Debug.Log("Attack buff");
                user.atkBuff(skill.skillpower);
            }
            cooldowncount = cooldownTurns;
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
        // if (isOnCooldown && currentTurn >= lastUsedTurn + cooldownTurns)
        // {
        //     isOnCooldown = false;
        //     if (cooldowncount > 0)
        //     {
        //         cooldowncount--;
        //     }
        //     else
        //     {
        //         cooldowncount = cooldownTurns;
        //     }
        // }
        if (isOnCooldown) {
            cooldowncount--;
            if (cooldowncount == 0) {
                isOnCooldown = false;
            }
        }
    }

    public int SkillCD(int currentTurn)
    {
        return cooldowncount;
    }
}
