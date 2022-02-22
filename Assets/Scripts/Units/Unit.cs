using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    [SerializeField] UnitBase _base;
    [SerializeField] int level;

    public UnitBase Base
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }
    bool isDead;

    bool isCharging;
    int turnsCharging;

    public string name;

    public List<Move> moves { get; set; }

    public int HP { get; set; }
    public int Mana { get; set; }

    public Animator Anim { get; set; }

    public void Init()
    {
        name = _base.name;

        HP = MaxHP;
        Mana = MaxMana;

        isDead = false;

        moves = new List<Move>();
        foreach (var move in _base.learnableMoves)
        {
            if(move.level <= level)
            {
                moves.Add(new Move(move.moveBase));
            }
        }

        if(_base.animator != null)
        {
            Anim = _base.animator;
        }
    }

    public int MaxHP
    {
        get { return Mathf.FloorToInt(_base.maxHP + (_base.hpPerLevel * level)); }
    }

    public int MaxMana
    {
        get { return Mathf.FloorToInt(_base.maxMana + (_base.manaPerLevel * level)); }
    }
    public int Attack
    {
        get {return Mathf.FloorToInt(_base.attack + (_base.attackPerLevel * level)); }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt(_base.defense + (_base.defensePerLevel * level)); }
    }
    public int Magic
    {
        get { return Mathf.FloorToInt(_base.magic + (_base.magicPerLevel * level)); }
    }
    public int Resist
    {
        get { return Mathf.FloorToInt(_base.resist + (_base.resistPerLevel * level)); }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt(_base.speed + (_base.speedPerLevel * level)); }
    }

    public float HitChance
    {
        get { return _base.hitChance; }
    }


    public Move GetRandomMove()
    {
        int r = Random.Range(0, moves.Count);
        return moves[r];
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
