using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillComponent : MonoBehaviour
{
    public Skill skill;
<<<<<<< HEAD
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
        // Debug.Log(skill.skillName);
        if (!isOnCooldown)
        {
            if (skill.skillName == "Heal")
            {
                user.heal(skill.skillpower);
                // GameObject skillEffectInstance = Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
                // skillEffectInstance.transform.SetParent(transform); // optional: attach the effect to the character's transform for easier management
            }
            else if (skill.skillName == "dealdamage")
            {
                user.dealDamage(skill.skillpower);
                // Debug.Log("Sample2");
            }
            else
            {
                // Debug.Log("FK U");
            }
            cooldowncount = cooldownTurns;
            lastUsedTurn = currentTurn;
            isOnCooldown = true;
        }
        else
        {
=======
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
>>>>>>> 5401f40893fe1502d18a0e9a73eb145a0e221980
            Debug.Log("Skill on cooldown");
        }
    }

    public void UpdateCooldown(int currentTurn)
    {
<<<<<<< HEAD
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
=======
        if (isOnCooldown && currentTurn >= lastUsedTurn + cooldownTurns)
        {
            isOnCooldown = false;
            cooldowncount--;
            if (cooldowncount < 0) {
                cooldowncount = 2;
>>>>>>> 5401f40893fe1502d18a0e9a73eb145a0e221980
            }
        }
    }

    public int SkillCD(int currentTurn)
    {
        return cooldowncount;
    }
}
