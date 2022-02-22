using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { SETUP, GETNEXTACTOR, PLAYERCHOOSEACTION, PLAYERCHOOSESKILL, CHOOSEALLYTARGET, AITAKETURN, CHECKVICTORY, BUSY}

public class BattleSystem : MonoBehaviour
{
    public BattleState state;
    //public Enemy Enemy;
    //public PlayerCharacterBase Player;

    [SerializeField] List<BattleUnit> units;
    [SerializeField] List<BattleUnit> enemies;
    [SerializeField] List<BattleUnit> players;
    List<int> playersAlive;
    BattleUnit lastPlayer;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject allySelector;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> skillTexts;
    [SerializeField] Color highlightedColor;

    [SerializeField] Text MoveDetailText;
    [SerializeField] Text ManaCostText;
    [SerializeField] Text ChargeTurnsText;
    [SerializeField] Text moveDescriptionText;

    [SerializeField] List<GameObject> allyArrows;

    [SerializeField] GameObject poisonAnim;
    [SerializeField] public AudioSource[] audioSources;
    [SerializeField] AudioSource winMusic;
    [SerializeField] AudioSource loseMusic;

    public Text infoText;
    String introText = "An enemy appeared";

    private int turnIndex;
    BattleUnit chosenOne;
    int currentAction;
    int currentMove;
    private bool allyChosen = false;
    private int allyTargetIndex;

    Party playerParty;
    Unit enemy;

    bool firstMove;

    public event Action<bool> onBattleOver;
    // Start is called before the first frame update
    public void SetupBattle(Party party, Unit enemy)
    {
        state = BattleState.SETUP;
        firstMove = true;
        checkPlayersAlive();
        foreach (int i in playersAlive)
        {
            Debug.Log(i);
        } 
        this.enemy = enemy;
        playerParty = party;
        units = new List<BattleUnit>();
        foreach (var unit in players)
        {

            if (unit.isChosenOne)
            {
                chosenOne = unit;
            }
            units.Add(unit);  
        }
        foreach (var unit in enemies)
        {
            units.Add(unit);
        }
        
        int playerIndex = 0;
        foreach (BattleUnit unit in players)
        {
            Debug.Log("Player " + playerIndex + " is being set up");
            unit.Setup(playerParty.partyMembers[playerIndex]);
            playerIndex++;
        }
        enemies[0].Setup(this.enemy);
        enemies[0].GetComponent<Animator>().runtimeAnimatorController = enemy.Anim.runtimeAnimatorController as RuntimeAnimatorController;
        StartCoroutine(StartBattle());
    }

    private void checkPlayersAlive()
    {
        playersAlive = new List<int>();
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].isDead)
            {
                playersAlive.Add(i);
            }
        }
    }
    public void DecideUnitTurnOrder()
    {
        //enhanced bubble sort to order based on their speed stat
        for (int i = 0; i < units.Count - 1; i++)
        {
            bool swapped = false;

            for(int j = 0; j < units.Count - 1; j++)
            {
                if (units[j].unit.Speed < units[j + 1].unit.Speed)
                {
                    var temp = units[j];
                    units[j] = units[j + 1];
                    units[j + 1] = temp;

                    swapped = true;
                }
            }

            if (!swapped)
            {
                break;
            }
        }
    }

    public IEnumerator UpdateInfoText(String s)
    {
        infoText.text = "";
        foreach (var letter in s.ToCharArray())
        {

            infoText.text += letter;
            yield return new WaitForSeconds(1/30f);
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator ChosenOnePanicAttack()
    {
        state = BattleState.BUSY;
        yield return UpdateInfoText("The Chosen One is unprotected and panics");
        //TODO: Expand for moves that target team/self
        var move = units[turnIndex].unit.GetRandomMove();
        move._base.ExecuteMove(enemies[0], units[turnIndex], this);

        yield return CheckBattleWon(enemies[0]);
    }

    IEnumerator StartBattle()
    {
        DecideUnitTurnOrder();
        turnIndex = -1;
        yield return StartCoroutine(UpdateInfoText(introText));
        yield return new WaitForSeconds(1f);
        state = BattleState.GETNEXTACTOR;
        yield return GetNextActor();        
    }

    IEnumerator ResetProtector()
    {
        yield return UpdateInfoText(units[turnIndex].unit.name + " is no longer protecting The Chosen One");
        chosenOne.isProtected = false;
        chosenOne.protector = null;
        units[turnIndex].isProtector = false;
        
    }

    IEnumerator GetNextActor()
    {
        if (!firstMove)
        {
            lastPlayer = units[turnIndex];
            Debug.Log(lastPlayer.unit.name);
        } else
        {
            firstMove = false;
        }
        if (lastPlayer != null && lastPlayer.isPlayerUnit)
        {
            lastPlayer.uiBox.color = Color.white;
            lastPlayer.name.color = Color.white;
            lastPlayer.Level.color = Color.white;
        }


        if (turnIndex >= units.Count - 1)
        {
            DecideUnitTurnOrder();
            turnIndex = 0;
        } else
        {
            turnIndex++;
        }

        if (units[turnIndex].isDead)
        {
            yield return GetNextActor();
        } else if (units[turnIndex].isCharging)
        {
            if (units[turnIndex].isPlayerUnit)
            {
                units[turnIndex].uiBox.color = Color.yellow;
                units[turnIndex].name.color = Color.yellow;
                units[turnIndex].Level.color = Color.yellow;

            }
            yield return UpdateInfoText("It's " + units[turnIndex].unit.name + "'s turn");

            if (units[turnIndex].isPoisoned)
            {
                if (!units[turnIndex].isPlayerUnit)
                {
                    Instantiate(poisonAnim, units[turnIndex].gameObject.transform.position, Quaternion.identity);
                }
                audioSources[1].Play();
                yield return units[turnIndex].TakePoisonDamage(this);
                if (units[turnIndex].isPlayerUnit && units[turnIndex].isDead)
                {
                    if (units[turnIndex].isChosenOne)
                    {
                        GetComponent<AudioSource>().Stop();
                        loseMusic.Play();
                        yield return UpdateInfoText("The Chosen One has died");
                        yield return new WaitForSeconds(0.5f);
                        yield return UpdateInfoText("You can't complete your adventure");
                        yield return new WaitForSeconds(1f);
                        onBattleOver(false);
                    }
                    else if (units[turnIndex].isDead)
                    {
                        units.Remove(units[turnIndex]);
                        yield return GetNextActor();
                        yield break;
                    }
                    else
                    {
                        yield return GetNextActor();
                        yield break;
                    }
                }
                else if (!units[turnIndex].isPlayerUnit && units[turnIndex].isDead)
                {
                    yield return CheckBattleWon(enemies[0]);
                }
            }

            if (units[turnIndex].isChosenOne && !units[turnIndex].isProtected)
            {
                UpdateInfoText("The Chosen One panics and loses focus");
                units[turnIndex].isCharging = false;
            } else
            {
                yield return units[turnIndex].lastMove._base.ExecuteMove(units[turnIndex].lastTarget, units[turnIndex], this);
                Debug.Log(units[turnIndex].lastTarget);
                Debug.Log(units[turnIndex].lastTarget.isDead);
                yield return new WaitForSeconds(0.05f);

                if (units[turnIndex].lastTarget.isPlayerUnit && units[turnIndex].lastTarget.isDead)
                {
                    if (units[turnIndex].lastTarget.isChosenOne)
                    {
                        GetComponent<AudioSource>().Stop();
                        loseMusic.Play();
                        yield return UpdateInfoText("The Chosen One has died");
                        yield return new WaitForSeconds(0.5f);
                        yield return UpdateInfoText("You can't complete your adventure");
                        yield return new WaitForSeconds(1f);
                        onBattleOver(false);
                    } else
                    {
                        units.Remove(units[turnIndex].lastTarget);
                        playersAlive.Remove(units[turnIndex].playerValue);
                    }
                } else if (!units[turnIndex].lastTarget.isPlayerUnit && units[turnIndex].lastTarget.isDead)
                {
                    Debug.Log("Checking Battle Won");
                    yield return CheckBattleWon(enemies[0]);
                }
            }
            
            yield return GetNextActor();
        } else
        {
            if (units[turnIndex].isPlayerUnit)
            {
                units[turnIndex].uiBox.color = Color.yellow;
                units[turnIndex].name.color = Color.yellow;
                units[turnIndex].Level.color = Color.yellow;
            }
            yield return UpdateInfoText("It's " + units[turnIndex].unit.name + "'s turn");

            if (units[turnIndex].isPoisoned)
            {
                if (!units[turnIndex].isPlayerUnit)
                {
                    Instantiate(poisonAnim, units[turnIndex].gameObject.transform.position, Quaternion.identity);
                }
                audioSources[1].Play();
                yield return units[turnIndex].TakePoisonDamage(this);
                if (units[turnIndex].isPlayerUnit && units[turnIndex].isDead)
                {
                    if(units[turnIndex].isChosenOne)
                    {
                        GetComponent<AudioSource>().Stop();
                        loseMusic.Play();
                        yield return UpdateInfoText("The Chosen One has died");
                        yield return new WaitForSeconds(0.5f);
                        yield return UpdateInfoText("You can't complete your adventure");
                        yield return new WaitForSeconds(1f);
                        onBattleOver(false);
                    }
                    else if (units[turnIndex].isDead)
                    {
                        units.Remove(units[turnIndex]);
                        yield return GetNextActor();
                        yield break;
                    }
                    else
                    {
                        yield return GetNextActor();
                        yield break;
                    }
                }
                else if (!units[turnIndex].isPlayerUnit && units[turnIndex].isDead)
                {
                    yield return CheckBattleWon(enemies[0]);
                }
            }

            if (units[turnIndex].isChosenOne && !units[turnIndex].isProtected)
            {
                yield return ChosenOnePanicAttack();
            }
            else if (units[turnIndex].isPlayerUnit)
            {
                if (units[turnIndex].isProtector)
                {
                    yield return ResetProtector();
                }
                yield return PlayerAction();
            }
            else
            {
                state = BattleState.AITAKETURN;
                StartCoroutine(EnemyMove());
            }
        } 
    }

   

    IEnumerator PlayerAction()
    {
        yield return UpdateInfoText("Choose an action");
        ActionsEnabled();
        state = BattleState.PLAYERCHOOSEACTION;
    }

    IEnumerator PlayerSkill()
    {
        yield return new WaitForSeconds(0.1f);

        state = BattleState.PLAYERCHOOSESKILL;
        currentMove = 0;
        SkillsEnabled();
        SetMoveNames(units[turnIndex].unit.moves);
    }

    IEnumerator HandleNotEnoughMana()
    {
        state = BattleState.BUSY;
        InfoEnabled();
        string msg = units[turnIndex].unit.name + " does not have enough mana to perform " + units[turnIndex].unit.moves[currentMove];
        yield return UpdateInfoText(msg);
        yield return new WaitForSeconds(1f);
        SkillsEnabled();
        state = BattleState.PLAYERCHOOSESKILL;
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.BUSY;
        InfoEnabled();
        Move move;
        if (units[turnIndex].isCharging)
        {
            move = units[turnIndex].lastMove;
            
        } else
        {
            move = units[turnIndex].unit.moves[currentMove + 1];
        }
        //TODO: Handle getting hit animations/effects
        if (move._base.targetsAlly)
        {
            Debug.Log("Move is targeting an ally");
            allyChosen = false;
          
            yield return UpdateInfoText("Choose a target");
            AllyEnabled();
            allyTargetIndex = 0;
            state = BattleState.CHOOSEALLYTARGET;
            while (!allyChosen)
            {
                Debug.Log("waiting for selection");
                yield return null;
            }
            //TODO: Select ally target
            yield return move._base.ExecuteMove(players[allyTargetIndex], units[turnIndex], this);
            allyChosen = false;
        } else if (move._base.targetsEnemy)
        {
            yield return move._base.ExecuteMove(enemies[0], units[turnIndex], this);
        }
        units[turnIndex].lastTarget = enemies[0];
        units[turnIndex].lastMove = move;
        yield return CheckBattleWon(enemies[0]);

        
    }

    IEnumerator CheckBattleWon(BattleUnit target)
    {
        Debug.Log("Check Battle won is called");
        if (target.isDead)
        {
            Debug.Log("Enemy is dead");
            yield return UpdateInfoText(target.unit.name + " has been defeated");
            //TODO:Give EXP

            bool allDead = true;

            foreach (var enemy in enemies)
            {
                if (!enemy.isDead)
                {
                    allDead = false;
                }
            }

            if (allDead)
            {
                GetComponent<AudioSource>().Stop();
                winMusic.Play();
                yield return UpdateInfoText("You win the battle!");
                yield return new WaitForSeconds(1f);
                onBattleOver(true);
            }
            else
            {
                yield return GetNextActor();
            }
        }
        else
        {
            yield return GetNextActor();
        }
    }

    IEnumerator EnemyMove()
    {
        var move = units[turnIndex].unit.GetRandomMove();

        //TODO: Handle getting hit animations/effects
        EnemyBase enemy = (EnemyBase)units[turnIndex].unit.Base;
        BattleUnit hitPlayer = enemy.ChooseTarget(players, playersAlive, units[turnIndex]);
        
        yield return move._base.ExecuteMove(hitPlayer, units[turnIndex], this);
        units[turnIndex].lastTarget = hitPlayer;
        units[turnIndex].lastMove = move;

        if (hitPlayer.isDead && hitPlayer.isChosenOne) {
            GetComponent<AudioSource>().Stop();
            loseMusic.Play();
            yield return UpdateInfoText("The Chosen One has died");
            yield return new WaitForSeconds(0.5f);
            yield return UpdateInfoText("You can't complete your adventure"); 
            yield return new WaitForSeconds(1f);
            onBattleOver(false);
        } else if (hitPlayer.isDead)
        {
            units.Remove(hitPlayer);
            playersAlive.Remove(hitPlayer.playerValue);
            yield return GetNextActor();
        } else
        {
            yield return GetNextActor();
        }
    }

    IEnumerator ProtectChosenOne()
    {
        state = BattleState.BUSY;
        InfoEnabled();
        if (chosenOne.isProtected)
        {
            chosenOne.protector.isProtector = false;
            yield return UpdateInfoText(chosenOne.protector.unit.name + " is no longer protecting the chosen one");
        }
        yield return UpdateInfoText(units[turnIndex].unit.name + " is now protecting The Chosen One");
        chosenOne.isProtected = true;
        chosenOne.protector = units[turnIndex];
        units[turnIndex].isProtector = true;
        yield return GetNextActor();
    }

    IEnumerator ChosenOneProtectError()
    {
        state = BattleState.BUSY;
        InfoEnabled();
        yield return UpdateInfoText("The Chosen One can't protect themself");
        yield return UpdateInfoText("Choose an action");
        ActionsEnabled();
        state = BattleState.PLAYERCHOOSEACTION;
    }
    public void InfoEnabled()
    {
        infoText.enabled = true;
        actionSelector.SetActive(false);
        moveSelector.SetActive(false);
        moveDetails.SetActive(false);
        allySelector.SetActive(false);
    }

    public void ActionsEnabled()
    {
        infoText.enabled = true;
        actionSelector.SetActive(true);
        moveSelector.SetActive(false);
        moveDetails.SetActive(false);
        allySelector.SetActive(false);
    }

    public void SkillsEnabled()
    {
        infoText.enabled = false;
        actionSelector.SetActive(false);
        moveSelector.SetActive(true);
        moveDetails.SetActive(true);
        allySelector.SetActive(false);
    }

    public void AllyEnabled()
    {
        infoText.enabled = true;
        actionSelector.SetActive(false);
        moveSelector.SetActive(false);
        moveDetails.SetActive(false);
        allySelector.SetActive(true);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.white;
            }
        }
    }

    public void UpdateSkillSelection(int selectedAction)
    {
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                skillTexts[i].color = highlightedColor;
            }
            else
            {
                skillTexts[i].color = Color.white;
            }
        }

        if (selectedAction + 1 < units[turnIndex].unit.moves.Count)
        {
            ManaCostText.text = "MP Cost: " + units[turnIndex].unit.moves[selectedAction + 1]._base.manaCost;
            ChargeTurnsText.text = "Turns to Charge: " + units[turnIndex].unit.moves[selectedAction + 1]._base.turnsToCharge;
            moveDescriptionText.text = units[turnIndex].unit.moves[selectedAction + 1]._base.description;
        } else
        {
            ManaCostText.text = "";
            ChargeTurnsText.text = "";
            moveDescriptionText.text = "";
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PLAYERCHOOSEACTION)
        {
            HandleActionSelection();
        }

        if (state == BattleState.PLAYERCHOOSESKILL)
        {
            HandleSkillSelection();
        }

        if (state == BattleState.CHOOSEALLYTARGET)
        {
            HandleAllySelection();
        }
    }

    private void HandleAllySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (allyTargetIndex < 3)
            {
                audioSources[0].Play();
                allyTargetIndex++;
            }
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (allyTargetIndex > 0)
            {
                audioSources[0].Play();
                allyTargetIndex--;
            }
        }

        UpdateAllySelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            audioSources[0].Play();
            state = BattleState.BUSY;
            InfoEnabled();
            allyChosen = true;
        }
    }

    private void UpdateAllySelection()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i == allyTargetIndex)
            {
                allyArrows[i].SetActive(true);
            } else
            {
                allyArrows[i].SetActive(false);
            }
        }
    }

    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction % 2 == 0)
            {
                audioSources[0].Play();
                currentAction++;
            }
                
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction % 2 == 1)
            {
                audioSources[0].Play();
                currentAction--;
            }
                
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 2)
            {
                audioSources[0].Play();
                currentAction += 2;
            }
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 1)
            {
                audioSources[0].Play();
                currentAction -= 2;
            }
        }

        UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            audioSources[0].Play();
            switch (currentAction)
            {
                case 0:
                    //Does default attack
                    currentMove = -1;
                    StartCoroutine(PerformPlayerMove());
                    break;
                case 1:
                    StartCoroutine(PlayerSkill());
                    //open skills menu
                    break;
                case 2:
                    if (units[turnIndex].isChosenOne)
                    {
                        //alertplayer invalid move
                        StartCoroutine(ChosenOneProtectError());
                    } else
                    {
                        StartCoroutine(ProtectChosenOne());
                    }
                    //become protector
                    break;
                case 3:
                    //run away
                default:
                    break;
            }
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i=0; i < skillTexts.Count; i++)
        {
            if (i < moves.Count - 1)
            {
                skillTexts[i].text = moves[i + 1]._base.name;
            } 
            else
            {
                skillTexts[i].text = "-";
            }
        }
    }

    private void HandleSkillSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && currentMove % 2 != 1)
        {
            audioSources[0].Play();
            currentMove++;
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentMove % 2 != 0)
        {
            audioSources[0].Play();
            currentMove--;
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove != 0 && currentMove != 1)
            {
                audioSources[0].Play();
                currentMove -= 2;
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove != 6 && currentMove != 7)
            {
                audioSources[0].Play();
                currentMove += 2;
                return;
            }
        }

        UpdateSkillSelection(currentMove);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentMove + 1 < units[turnIndex].unit.moves.Count)
            {
                audioSources[0].Play();
                if (units[turnIndex].unit.moves[currentMove]._base.manaCost < units[turnIndex].unit.Mana)
                {
                    StartCoroutine(PerformPlayerMove());

                } else
                {
                    StartCoroutine(HandleNotEnoughMana());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            audioSources[0].Play();
            currentMove = 0;
            currentAction = 0;
            ActionsEnabled();
            state = BattleState.PLAYERCHOOSEACTION;
        }
    }

}
