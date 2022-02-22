using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealMove", menuName = "Move/Create Heal Move")]
public class HealMoveBase : MoveBase
{

    public float healMultiplier;
    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        sfx = bs.audioSources[sfxValue];
        yield return bs.UpdateInfoText(attacker.unit.name + " " + attackDescription + " " + target.unit.name + "...");

        yield return attacker.UseMana(manaCost);
        int healAmount = Mathf.FloorToInt(healMultiplier * attacker.unit.Magic);

        yield return target.HealDamage(healAmount);
        yield return bs.UpdateInfoText("It healed " + healAmount + " HP!");

        
    }
}
