using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    private bool isMoving;
    private Vector2 input;

    private Animator animator;

    public LayerMask collisionMask;
    public LayerMask encounterLayer;
    public int encounterChance;

    public event Action OnEncounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void HandleUpdate()
    {
        animator.SetBool("isMoving", isMoving);
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            

            if (input != Vector2.zero)
            {
                animator.SetFloat("MoveX", input.x);
                animator.SetFloat("MoveY", input.y);
                

                var targetpos = transform.position;
                targetpos.x += input.x;
                targetpos.y += input.y;
                if(IsWalkable(targetpos))
                {
                    StartCoroutine(Move(targetpos));
                }
                
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, encounterLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= encounterChance)
            {
                OnEncounter();
            }
        }
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.3f, collisionMask) != null)
        {
            return false;
        } else
        {
            return true;
        }
    }
}
