using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Move/Create new taunt move")]
public class TauntMove : MoveBase
{
    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        yield return bs.UpdateInfoText(attacker.unit.name + " cockily taunts " + target.unit.name + " from behind " + attacker.protector.unit.name);
        yield return bs.UpdateInfoText(target.unit.name + " only wants to attack The Chosen One but abandons all technique");

        if (target.isCharging)
        {
            target.isCharging = false;
            yield return bs.UpdateInfoText(target.unit.name + " is no longer focused on " + target.lastTarget.unit.name);
        }

        target.isEnraged = true;
    }
}
