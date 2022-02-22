using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Unit> enemiesInArea;

    public Unit GetRandomEnemy()
    {
        var enemy = enemiesInArea[Random.Range(0, enemiesInArea.Count)];
        enemy.Init();
        return enemy;
    }
}
