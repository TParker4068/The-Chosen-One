using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Status Move", menuName = "Move/Create status move")]
public class StatusMoveBase : MoveBase
{
    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        sfx = bs.audioSources[sfxValue];
        if (target.isChosenOne && target.isProtected)
        {
            yield return bs.UpdateInfoText(attacker.unit.name + " targets The Chosen One...");
            yield return bs.UpdateInfoText("But " + target.protector.unit.name + " is protecting them!");
            target = target.protector;
        }

        HandleFX(target);
        yield return bs.UpdateInfoText(attacker.unit.name + " poisons " + target.unit.name);

        yield return attacker.UseMana(manaCost);
        if (target.isPoisoned)
        {
            yield return bs.UpdateInfoText("But " + target.unit.name + " is already poisoned");

        } else
        {
            target.isPoisoned = true;
        }
    }
}
