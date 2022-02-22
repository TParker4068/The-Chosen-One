using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Multihit Move", menuName = "Move/Create multi hit move")]
public class MultihitMoveBase : MoveBase
{
    [SerializeField] int lowHits;
    [SerializeField] int highHits;

    public override IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
    {
        sfx = bs.audioSources[sfxValue];
        if (isChargingMove)
        {
            if (!attacker.isCharging)
            {
                Debug.Log("Started Charging");
                attacker.isCharging = true;
                attacker.turnsCharging = turnsToCharge - 1;
                attacker.lastTarget = target;
                yield return bs.UpdateInfoText(attacker.unit.name + " " + chargingDescription + " " + target.unit.name);
                yield break;
            }
            else if (attacker.turnsCharging != 0)
            {
                Debug.Log("Still Charging");
                yield return bs.UpdateInfoText(attacker.unit.name + " " + chargingDescription + " " + target.unit.name);
                attacker.turnsCharging--;
                yield break;
            }
            else
            {

                Debug.Log("Attacks");
                attacker.isCharging = false;
            }

        }

        if (target.isChosenOne && target.isProtected)
        {
            yield return bs.UpdateInfoText(attacker.unit.name + " targets The Chosen One...");
            yield return bs.UpdateInfoText("But " + target.protector.unit.name + " is protecting them!");
            target = target.protector;
        }

        yield return bs.UpdateInfoText(attacker.unit.name + " " + attackDescription + " " + target.unit.name + "...");
        yield return attacker.UseMana(manaCost);
        int hits = Mathf.FloorToInt(Random.Range(lowHits, highHits + 1));

        float enragedMultiplier = 1;
        if (attacker.isEnraged)
        {
            enragedMultiplier = 0.7f;
        }

        if (!checkAccuracy(target))
        {
            bs.UpdateInfoText("But they missed!");
            yield break;
        }

        for (int i = 0; i < hits; i++)
        {
            

            if (attacker.isPlayerUnit)
            {
                target.GetComponent<Animator>().SetTrigger("hit");
            }

            float typeMultiplier = checkTypeEffectiveness(target, attacker);

            //Checks for critical hit
            bool isCrit = checkCriticalHit();
            float critMultiplier = 1;
            if (isCrit)
            {
                critMultiplier = 2.5f;
                yield return bs.UpdateInfoText("Critical Hit!");
            }

            //minor random variance to add variety to damage numbers
            float variance = Random.Range(0.9f, 1.1f);
            float a = 1;
            if (damageType == DamageType.Physical)
            {

                a = (attacker.unit.Attack - (target.unit.Defense / 2));
                //Sets floor of damage that can be done
                if (a <= 2)
                {
                    a = 2;
                }
            }
            else if (damageType == DamageType.Magical)
            {
                a = (attacker.unit.Magic - (target.unit.Resist / 2));
                //Sets floor of damage that can be done
                if (a <= 2)
                {
                    a = 2;
                }
            }

            if (!attacker.isPlayerUnit)
            {
                attacker.GetComponent<Animator>().SetTrigger("attacks");
            }

            int damage = Mathf.CeilToInt(typeMultiplier * baseDamage * a * variance * critMultiplier * enragedMultiplier);
            HandleFX(target);
            yield return target.ShowDamage(damage, isCrit, isSuperEffective);
            yield return bs.UpdateInfoText("It deals " + damage + " damage");
            yield return target.TakeDamage(damage);
            if (isCrit)
            {
                yield return bs.UpdateInfoText("Critical Hit!");
            }
        }

        yield return bs.UpdateInfoText("It hit " + hits + " times");

        //inform player effectiveness
        if (isSuperEffective)
        {
           yield return bs.UpdateInfoText("It was super effective!");
        }
        else if (isNotEffective)
        {
           yield return bs.UpdateInfoText(target.unit.name + " resisted the damage!");
        }

        if (attacker.isEnraged)
        {
            attacker.isEnraged = false;
            yield return bs.UpdateInfoText(attacker.unit.name + " calms down");
        }
    }

}
