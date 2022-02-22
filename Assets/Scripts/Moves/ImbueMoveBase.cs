using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Imbue Move", menuName = "Move/Create imbue move")]
public class ImbueMoveBase : MoveBase
{
    [SerializeField] UnitBase.Types imbueType;
    [SerializeField] string imbueTypeAsString;

    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        sfx = bs.audioSources[sfxValue];
        HandleFX(target);
        yield return bs.UpdateInfoText(attacker.unit.name + " imbues " + target.unit.name + "'s weapon with " + imbueTypeAsString);
        yield return attacker.UseMana(manaCost);
        target.imbue = imbueType;
    }
}
