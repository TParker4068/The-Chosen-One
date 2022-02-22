using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Unit", menuName = "Unit/Create New Unit")]
public class UnitBase : ScriptableObject
{
    public new string name;

    public int maxHP;
    public int currentHP;
    public int maxMana;
    public int currentMana;
    public int attack;
    public int defense;
    public int magic;
    public int resist;
    public int speed;
    public float hitChance = 0.95f;
    

    public float hpPerLevel;
    public float manaPerLevel;
    public float attackPerLevel;
    public float defensePerLevel;
    public float magicPerLevel;
    public float resistPerLevel;
    public float speedPerLevel;

    public List<LearnableMove> learnableMoves;

    public Text Health;

    public Animator animator;

    public Types[] typesWeakTo;
    public Types[] typesResisted;

    public enum Types
    {
        NONE,
        FIRE,
        ICE,
        LIGHTNING,
        WIND,
        DARK,
        LIGHT
    }
    // Start is called before the first frame update
    private void Update()
    {
        
    }

    [System.Serializable]
    public class LearnableMove
    {
        [SerializeField]public MoveBase moveBase;
        [SerializeField]public int level;
    }
    public bool TakeDamage (int damage)
    {
        currentHP -= damage;
        
        if (currentHP <= 0 )
        {
            Health.text = "HP: 0";
            HandleDeath();
            return true;
        } else
        {
            Health.text = "HP: " + currentHP;
            return false;
        }
    }

    public void Heal(int healAmount)
    {
        currentHP += healAmount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public virtual void HandleDeath() { }
}
