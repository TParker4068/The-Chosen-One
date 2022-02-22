using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Move/Create new scan move")]
public class ScanMoveBase : MoveBase
{
    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        string resistances = "";
        string weaknesses = "";
        for (int i = 0; i < target.unit.Base.typesWeakTo.Length; i++)
        {
            if (i != target.unit.Base.typesWeakTo.Length - 1)
            {
                weaknesses += target.unit.Base.typesWeakTo[i] + ", ";
            } else
            {
                weaknesses += target.unit.Base.typesWeakTo[i];
            }
        }

        for (int i = 0; i < target.unit.Base.typesResisted.Length; i++)
        {
            if (i != target.unit.Base.typesResisted.Length - 1)
            {
                resistances += target.unit.Base.typesResisted[i] + ", ";
            }
            else
            {
                resistances += target.unit.Base.typesResisted[i];
            }
        }

        yield return bs.UpdateInfoText(target.unit.name + ":   HP: " + target.unit.HP + "/" + target.unit.MaxHP);
        yield return new WaitForSeconds(0.5f);
        yield return bs.UpdateInfoText("Resistances: " + resistances);
        yield return new WaitForSeconds(0.5f);
        yield return bs.UpdateInfoText("Weaknesses: " + weaknesses);
        yield return new WaitForSeconds(0.5f);
    }
}
