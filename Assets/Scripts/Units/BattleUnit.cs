using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] public bool isPlayerUnit;
    [SerializeField] public bool isChosenOne;
    public bool isProtector = false;
    public bool isProtected = false;
    public BattleUnit protector;
    public bool isDead = false;
    public bool isPoisoned;
    public bool isEnraged = false;
    [SerializeField] public int playerValue;
    [SerializeField] GameObject hitFlashAnim;


    [SerializeField] public new Text name;
    [SerializeField] Text HP;
    [SerializeField] Text Mana;
    [SerializeField] public Text Level;
    [SerializeField] Text damageText;
    [SerializeField] AudioSource hitSFX;


    public Unit unit;

    public Move lastMove;
    public BattleUnit lastTarget;
    public bool isCharging = false;
    public int turnsCharging;

    public Image uiBox;

    public UnitBase.Types imbue;
    
    public void Setup(Unit passedUnit)
    {  
        unit = passedUnit;
        imbue = UnitBase.Types.NONE;
        isPoisoned = false;
        isCharging = false;
        isEnraged = false;
        if (isPlayerUnit)
        {
            name.text = unit.Base.name;
            Level.text = "Lv. " + unit.Level;
            uiBox = gameObject.GetComponent<Image>();
        } else
        {
            unit.HP = unit.MaxHP;
            unit.Mana = unit.MaxMana;
        }

        if (unit.HP == 0)
        {
            isDead = true;
        } else
        {
            isDead = false;
        }

        lastMove = null;
    }

    public IEnumerator TakePoisonDamage(BattleSystem bs)
    {
        yield return bs.UpdateInfoText(unit.name + " takes " + Mathf.FloorToInt(unit.MaxHP / 16) + " poison damage");
        yield return TakeDamage(Mathf.FloorToInt(unit.MaxHP / 16));
    }

    public IEnumerator TakeDamage(int damage)
    {
        
        float timeToWait = 0.5f / damage;
        if (isPlayerUnit)
        {
            while (damage > 0 && unit.HP > 0)
            {
                unit.HP--;
                damage--;
                yield return new WaitForSeconds(timeToWait);
            }
        } else
        {
            unit.HP -= damage;
        }

        if(unit.HP <= 0)
        {
            Debug.Log("The unit is dead mate");
            if (!isPlayerUnit)
            {
                GetComponent<Animator>().SetTrigger("dies");
            }
            isDead = true;
        }
        
    }

    public IEnumerator ShowDamage(int damage, bool isCrit, bool isSuperEffective)
    {
        hitSFX.Play();
        GameObject HitFlash = Instantiate(hitFlashAnim, damageText.transform.position, Quaternion.identity);
        HitFlash.transform.localScale = new Vector3 (Mathf.FloorToInt(1 + damage / 50), Mathf.FloorToInt(1 + damage / 50), 1);
        damageText.color = Color.white;
        damageText.fontSize = Mathf.Clamp(80 + Mathf.FloorToInt(damage), 80, 200);
        damageText.text = damage.ToString();
        damageText.enabled = true;

        if (isPlayerUnit)
        {
            yield return PlayerShowDamage();
        }

        if (isCrit || isSuperEffective)
        {
            Time.timeScale = 0.6f;
            yield return new WaitForSeconds(1f);
            Time.timeScale = 1f;
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        damageText.enabled = false;
    }

    private IEnumerator PlayerShowDamage()
    {
        for (int i = 0; i < 4; i++)
        {
            gameObject.SetActive(false);

            yield return new WaitForSeconds(0.05f);

            gameObject.SetActive(true);

            yield return new WaitForSeconds(0.05f);
        }
    }

    public IEnumerator HealDamage(int heal)
    {
        float timeToWait = 0.5f / heal;
        if (isPlayerUnit)
        {
            while (heal > 0 && unit.HP < unit.MaxHP)
            {
                unit.HP++;
                heal--;
                yield return new WaitForSeconds(timeToWait);
            }
        } else
        {
            unit.HP += heal;
            if (unit.HP > unit.MaxHP)
            {
                unit.HP = unit.MaxHP;
            }
        }
        
    }

    public IEnumerator UseMana(int manaCost)
    {
        float timeToWait = 0.3f / manaCost;
        if(isPlayerUnit)
        {
            while (manaCost > 0)
            {
                unit.Mana--;
                manaCost--;
                yield return new WaitForSeconds(timeToWait);
            }
        }
        
    }

    private void Update()
    {
        if (isPlayerUnit)
        {
            HP.text = "HP: " + unit.HP;
            Mana.text = "MP: " + unit.Mana;

            if (unit.HP < unit.MaxHP/8)
            {
                HP.color = Color.red;
            } else
            {
                HP.color = Color.white;
            }

            if (unit.Mana < unit.MaxMana/8)
            {
                Mana.color = Color.red;
            }
            else
            {
                Mana.color = Color.white;
            }

        }

        if (unit.HP <= 0)
        {
            isDead = true;
        }
    }
}
