using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/Create New Enemy")]
public class EnemyBase : UnitBase
{
    public Sprite sprite;

    public int[] oddsOfTargeting;

    public virtual BattleUnit ChooseTarget(List<BattleUnit> players, List<int> playersAlive, BattleUnit self)
    {
        if (self.isEnraged)
        {
            return players[3];
        }

        List<int> aliveOdds = new List<int>();
        float totalOdds = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].isDead)
            {
                aliveOdds.Add(oddsOfTargeting[i]);
                totalOdds += oddsOfTargeting[i];
            }
        }
        Debug.Log("ChooseTarget called");
        BattleUnit target = players[2];
        int targetValue = Mathf.FloorToInt(Random.Range(0, totalOdds + 1));
        for (int i = 0; i < aliveOdds.Count; i++)
        {
            if (targetValue < aliveOdds[i])
            {
                target = players[playersAlive[i]];
                return target;
            } else
            {
                targetValue -= aliveOdds[i];
            }
        }

        return target;      
    }
}
