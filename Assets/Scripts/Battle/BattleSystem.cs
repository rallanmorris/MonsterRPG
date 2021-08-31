using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, PlayerAim, EnemyAim, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] Oscillator playerAiming;
    [SerializeField] Oscillator enemyAiming;
    [SerializeField] GameObject aimParent;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] MonsterController playerMonster;
    [SerializeField] MonsterController enemyMonster;

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
        enemyAiming.EnableAim(false);

        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Monster);

        dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        yield return dialogBox.TypeDialog($"Oh noooo a Wild {enemyUnit.Monster.Base.Name} appeared");

        PlayerAction();
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Do your worst"));
        dialogBox.EnableActionSelector(true);
    }

    void MovePlayer()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
    }

    void PlayerAttack()
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
                enemyAiming.Setup();
                state = BattleState.EnemyAim;
            }
        }
        else
        {
            yield return dialogBox.TypeDialog("FAILURE!");
            enemyAiming.Setup();
            state = BattleState.EnemyAim;
        }
        
    }

    IEnumerator EnemyMove()
    {
        yield return new WaitForSeconds(1f);
        enemyAiming.EnableAim(false);

        state = BattleState.Busy;

        /*
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
        */


        if (enemyAiming.GetHits() > 0)
        {
            yield return dialogBox.TypeDialog($"SUCCESS! a {enemyAiming.GetStrike()} Strike");
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
        else
        {
            yield return dialogBox.TypeDialog("FAILURE!");
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
            enemyMonster.HandleEnemyUpdate();
            HandlePlayerAim();
        }

        else if(state == BattleState.EnemyMove)
        {
            //HandlePlayerMovement();
        }

        else if (state == BattleState.EnemyAim)
        {
            HandlePlayerMovement();
            HandleEnemyAim();
        }

    }

    void HandlePlayerMovement()
    {
        playerMonster.HandlePlayerUpdate();
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 2)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
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
                //Attack
                PlayerAttack();
            }
            else if (currentAction == 1)
            {
                MovePlayer();
            }
            else if (currentAction == 2)
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

            playerAiming.Setup();
            state = BattleState.PlayerAim;
        }
    }

    void HandlePlayerAim()
    {
        playerAiming.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            playerAiming.PauseMoving();
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformAimCheck(playerAiming));
        }
    }

    void HandleEnemyAim()
    {
        //Wiggles enemy crosshair
        enemyAiming.HandleUpdate();

        if (enemyAiming.WillShoot())
        {
            enemyAiming.SetCurrentlyShooting(true);
            enemyAiming.PauseMoving();
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformAimCheck(enemyAiming));
        }
    }

    IEnumerator PerformAimCheck(Oscillator attacker)
    {
        if (attacker.HitEnemy())
        {
            attacker.AddTry(true);
            yield return dialogBox.TypeDialog($"HIT! x {attacker.GetHits()}");
        }
        else
        {
            attacker.AddTry(false);
            yield return dialogBox.TypeDialog("MISS!");
        }

        //Debug.Log(attacker.GetTries());
        if ((attacker.GetTries() >= 3)) //|| attacker.HasMissed())
        {
            if (attacker.IsPlayer())
                StartCoroutine(PerformPlayerMove());
            else
            {
                state = BattleState.EnemyMove;
                playerMonster.StopMoveAnimation();
                StartCoroutine(EnemyMove());
            }
                
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            attacker.ContinueMoving();
        }
    }
}
