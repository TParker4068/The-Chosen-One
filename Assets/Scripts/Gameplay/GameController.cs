using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Pause }

public class GameController : MonoBehaviour
{
    GameState state;
    [SerializeField]PlayerController playerController;
    [SerializeField]BattleSystem battleController;
    [SerializeField] AudioSource battleMusic;
    [SerializeField] Camera worldCamera;
    [SerializeField] AudioSource worldMusic;
    [SerializeField] Canvas worldCanvas;

    private void Start()
    {
        playerController.OnEncounter += StartBattle;
        battleController.onBattleOver += EndBattle;
        worldMusic.Play();
    }

    private void StartBattle()
    {
        state = GameState.Battle;
        worldMusic.Stop();
        worldCanvas.enabled = false;
        battleController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var enemy = FindObjectOfType<MapArea>().GetRandomEnemy();

        battleMusic.Play();

        battleController.SetupBattle(playerParty, enemy);
    }

    private void EndBattle(bool battleWon)
    {
        state = GameState.FreeRoam;
        battleMusic.Stop();
        worldMusic.Play();
        worldCanvas.enabled = true;
        battleController.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        //TODO: Handle loss condition
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleController.HandleUpdate();
        }
    }
}
