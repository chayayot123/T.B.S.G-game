using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Turn-Based Game/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public int skillpower;
    public string effect;
    // public int cooldown;
    // public int cooldowncount;

    // public SkillEffect effect;
    // public Sprite icon;
}