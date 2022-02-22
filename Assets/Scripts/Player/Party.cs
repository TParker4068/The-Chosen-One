using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] public List<Unit> partyMembers;

    private void Start()
    {
        foreach (var unit in partyMembers)
        {
            unit.Init();
        }
    }

    public void HealParty()
    {
        foreach (var unit in partyMembers)
        {
            unit.Init();
        }
    }
}
