using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName ="Move/Create new move")]
public class MoveBase : ScriptableObject
{
    public new string name;
    
    [TextArea]
    public string description;
    public string attackDescription;
    public int manaCost;
    public float baseDamage;
    public int accuracy = 1;

    protected bool isSuperEffective;
    protected bool isNotEffective;

    public bool targetsEnemy;
    public bool targetsAlly;
    public bool targetsSelf;
    public bool targetsAll;

    protected AudioSource sfx;
    [SerializeField] protected int sfxValue;
    [SerializeField] GameObject animation;


    [SerializeField] public bool isChargingMove;
    [SerializeField] public int turnsToCharge;
    [SerializeField] bool isSpeedMove;
    [SerializeField] protected string chargingDescription;
    [SerializeField] float critChance = 5;

    protected enum DamageType { Physical, Magical}

    [SerializeField] protected DamageType damageType;

    public UnitBase.Types elementalType;

    public virtual IEnumerator ExecuteMove(BattleUnit target, BattleUnit attacker, BattleSystem bs)
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

        float enragedMultiplier = 1;
        if (attacker.isEnraged)
        {
            enragedMultiplier = 0.7f;
        }

        if (!attacker.isPlayerUnit)
        {
            attacker.GetComponent<Animator>().SetTrigger("attacks");
        }

        //Check if it misses

        if (!checkAccuracy(target))
        {
            bs.UpdateInfoText("But they missed!");
            yield break;
        }



        float typeMultiplier = checkTypeEffectiveness(target, attacker);

        //Checks for critical hit
        bool isCrit = checkCriticalHit();
        float critMultiplier = 1;
        if (isCrit)
        {
            critMultiplier = 2.5f;
        }

        //minor random variance to add variety to damage numbers
        float variance = Random.Range(0.9f, 1.1f);
        float a = 1;
        if (isSpeedMove)
        {
            a = (attacker.unit.Speed - (target.unit.Speed / 2));
            //Sets floor of damage that can be done
            if (a <= 2)
            {
                a = 2;
            }
        }
        else if (damageType == DamageType.Physical)
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

        if (attacker.isPlayerUnit)
        {
            target.GetComponent<Animator>().SetTrigger("hit");
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
        //inform player effectiveness
        if (isSuperEffective)
        {
            yield return bs.UpdateInfoText("It was super effective!");
        }
        else if (isNotEffective)
        {
            yield return bs.UpdateInfoText(target.unit.name + " resisted the damage!");
        }

        //reset enrage
        if (attacker.isEnraged)
        {
            attacker.isEnraged = false;
            yield return bs.UpdateInfoText(attacker.unit.name + " calms down");
        }

    }

    protected void HandleFX(BattleUnit target)
    {
        sfx.Play();
        if (!target.isPlayerUnit)
        {
            Instantiate(animation, target.gameObject.transform.position, Quaternion.identity);
        }
    }

    protected float checkTypeEffectiveness(BattleUnit target, BattleUnit attacker)
    {
        float multiplier = 1;
        UnitBase.Types type = elementalType;

        if (elementalType == UnitBase.Types.NONE && damageType == DamageType.Physical)
        {
            elementalType = attacker.imbue;
        }

        Debug.Log(elementalType);

        isSuperEffective = false;
        foreach (UnitBase.Types t in target.unit.Base.typesWeakTo)
        {
            if (t == elementalType)
            {
                Debug.Log("super effective");
                multiplier *= 2;
                isSuperEffective = true;
            }
        }

        isNotEffective = false;
        foreach (UnitBase.Types t in target.unit.Base.typesResisted)
        {
            if (t == elementalType)
            {
                Debug.Log("not effective");
                multiplier /= 2;
                isNotEffective = true;
            }
        }

        return multiplier;
    }

    protected bool checkCriticalHit()
    {
        bool isCrit = false;
        int critRoll = Mathf.FloorToInt(Random.Range(0, 101));
        if (critRoll <= critChance)
        {
            isCrit = true;
        }
        return isCrit;
    }

    protected bool checkAccuracy(BattleUnit target)
    {
        bool hits = false;
        float hitRoll = Random.Range(0, 100) / 100;
        float accuracyThreshold = accuracy * target.unit.HitChance;
        if (hitRoll <= accuracyThreshold)
        {
            hits = true;
        }

        return hits;
    }

    protected void takeMana(BattleUnit attacker)
    {
        attacker.unit.Mana -= manaCost;
    }
}
