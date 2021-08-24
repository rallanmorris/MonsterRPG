using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, PlayerAim, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] Oscillator playerAiming;
    [SerializeField] GameObject aimParent;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] MonsterController playerMonster;

    //Bool: true for win false for defeat
    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    public void StartBattle()
    {
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Monster);
        playerAiming.EnableAim(false);

        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Monster);

        dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        yield return dialogBox.TypeDialog($"Oh noooo a Wild {enemyUnit.Monster.Base.Name} appeared");

        PlayerAction();
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Are you going to fight or run like a COWARD"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        yield return new WaitForSeconds(1f);
        playerAiming.EnableAim(false);

        state = BattleState.Busy;

        /*
        //Switch for whether hit enemy 1 - 3 times
        switch (playerAiming.GetHits())
        {
            case 1:
                // code block
                break;
            case y:
                // code block
                break;
            default:
                // code block
                break;
        } */

        if (playerAiming.GetHits() > 0)
        {
            yield return dialogBox.TypeDialog($"SUCCESS! a {playerAiming.GetStrike()} Strike");
            var move = playerUnit.Monster.Moves[currentMove];
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} used {move.Base.Name}");

            playerUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            enemyUnit.PlayHitAnimation();

            var damageDetails = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
            yield return enemyHud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);

            if (damageDetails.Fainted)
            {
                yield return dialogBox.TypeDialog($"Nice Job! {enemyUnit.Monster.Base.Name} has been annihilated");
                enemyUnit.PlayDefeatAnimation();

                yield return new WaitForSeconds(2f);
                OnBattleOver(true);
            }
            else
            {
                StartCoroutine(EnemyMove());
            }
        }
        else
        {
            yield return dialogBox.TypeDialog("FAILURE!");
            StartCoroutine(EnemyMove());
        }
        
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Monster.GetRandomMove();

        yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Monster.TakeDamage(move, enemyUnit.Monster);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"Welp looks like your {playerUnit.Monster.Base.Name}s dead");
            playerUnit.PlayDefeatAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }

    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A Critical Hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Owie! It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("Nice try! It's not very effective");
    }

    public void HandleUpdate()
    {
        Debug.Log(state);
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }

        else if(state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }

        else if(state == BattleState.PlayerAim)
        {
            HandlePlayerAim();
        }

        else if(state == BattleState.EnemyMove)
        {
            HandlePlayerMovement();
        }
    }

    void HandlePlayerMovement()
    {
        playerMonster.HandleUpdate();
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
            {
                --currentAction;
            }
        }
        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Monster.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
            {
                --currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Monster.Moves.Count - 2)
            {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
            {
                currentMove -= 2;
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

        //When move is selected
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(dialogBox.TypeDialog("Aim well"));

            playerAiming.ResetTries();
            playerAiming.EnableAim(true);
            playerAiming.ContinueMoving();
            state = BattleState.PlayerAim;
        }
    }

    void HandlePlayerAim()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            playerAiming.PauseMoving();
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformAimCheck());
        }
    }

    IEnumerator PerformAimCheck()
    {
        if (playerAiming.HitEnemy())
        {
            playerAiming.AddTry(true);
            yield return dialogBox.TypeDialog($"HIT! x {playerAiming.GetHits()}");
        }
        else
        {
            playerAiming.AddTry(false);
            yield return dialogBox.TypeDialog("MISS!");
        }

        if((playerAiming.GetTries() == 3) || playerAiming.HasMissed())
            StartCoroutine(PerformPlayerMove());
        else
        {
            yield return new WaitForSeconds(1f);
            playerAiming.ContinueMoving();
        }
    }
}
