using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Starting,
        ChoosingCommands,
        ExecutingActions,
        Victory,
        Defeat
    }

    public enum ActionType
    {
        Attack,
        Skill,
        Item,
        Guard
    }

    [System.Serializable]
    private class PlannedAction
    {
        public BattleUnit user;
        public BattleUnit target;
        public ActionType actionType;

        public PlannedAction(
            BattleUnit user,
            BattleUnit target,
            ActionType actionType)
        {
            this.user = user;
            this.target = target;
            this.actionType = actionType;
        }
    }

    [Header("Units")]
    public BattleUnit[] players;
    public BattleUnit enemy;

    [Header("UI")]
    public BattleUI battleUI;

    [Header("Skill Settings")]
    public int skillMPCost = 10;
    public int skillBonusDamage = 15;

    [Header("Item Settings")]
    public int potionHealAmount = 30;
    public int potionCount = 3;

    [Header("Timing")]
    public float actionDelay = 0.7f;

    [Header("Debug")]
    public BattleState currentState;

    private int activePlayerIndex;
    private List<PlannedAction> plannedActions =
        new List<PlannedAction>();

    private void Start()
    {
        StartBattle();
    }

    // =====================================================
    // BATTLE START
    // =====================================================

    private void StartBattle()
    {
        currentState = BattleState.Starting;

        foreach (BattleUnit player in players)
        {
            if (player != null)
                player.ResetForBattle();
        }

        enemy.ResetForBattle();

        plannedActions.Clear();

        battleUI.RefreshAll(players, enemy);

        StartNewRound();
    }

    // =====================================================
    // ROUND
    // =====================================================

    private void StartNewRound()
    {
        currentState = BattleState.ChoosingCommands;

        plannedActions.Clear();

        activePlayerIndex = 0;

        FindNextLivingPlayer();

        battleUI.RefreshAll(players, enemy);

        ShowCurrentPlayerTurn();
    }

    private void ShowCurrentPlayerTurn()
    {
        BattleUnit player = GetCurrentPlayer();

        if (player == null)
            return;

        battleUI.SetMessage(
            $"{player.unitName}: Choose a command."
        );

        battleUI.SetCommandsInteractable(true);
    }

    private BattleUnit GetCurrentPlayer()
    {
        if (activePlayerIndex < 0 ||
            activePlayerIndex >= players.Length)
        {
            return null;
        }

        return players[activePlayerIndex];
    }

    // =====================================================
    // COMMAND BUTTONS
    // =====================================================

    public void OnAttackPressed()
    {
        if (!CanChooseAction())
            return;

        BattleUnit player = GetCurrentPlayer();

        plannedActions.Add(
            new PlannedAction(
                player,
                enemy,
                ActionType.Attack
            )
        );

        ConfirmPlayerAction();
    }

    public void OnSkillPressed()
    {
        if (!CanChooseAction())
            return;

        BattleUnit player = GetCurrentPlayer();

        if (player.currentMP < skillMPCost)
        {
            battleUI.SetMessage(
                $"{player.unitName} doesn't have enough MP!"
            );

            return;
        }

        plannedActions.Add(
            new PlannedAction(
                player,
                enemy,
                ActionType.Skill
            )
        );

        ConfirmPlayerAction();
    }

    public void OnItemPressed()
    {
        if (!CanChooseAction())
            return;

        if (potionCount <= 0)
        {
            battleUI.SetMessage(
                "No potions remaining!"
            );

            return;
        }

        BattleUnit player = GetCurrentPlayer();

        plannedActions.Add(
            new PlannedAction(
                player,
                player,
                ActionType.Item
            )
        );

        ConfirmPlayerAction();
    }

    public void OnGuardPressed()
    {
        if (!CanChooseAction())
            return;

        BattleUnit player = GetCurrentPlayer();

        plannedActions.Add(
            new PlannedAction(
                player,
                player,
                ActionType.Guard
            )
        );

        ConfirmPlayerAction();
    }

    private bool CanChooseAction()
    {
        return currentState ==
               BattleState.ChoosingCommands;
    }

    // =====================================================
    // NEXT PARTY MEMBER
    // =====================================================

    private void ConfirmPlayerAction()
    {
        battleUI.SetCommandsInteractable(false);

        activePlayerIndex++;

        FindNextLivingPlayer();

        if (activePlayerIndex >= players.Length)
        {
            StartCoroutine(ExecuteRound());
            return;
        }

        ShowCurrentPlayerTurn();
    }

    private void FindNextLivingPlayer()
    {
        while (activePlayerIndex < players.Length)
        {
            if (!players[activePlayerIndex].IsDead())
                break;

            activePlayerIndex++;
        }
    }

    // =====================================================
    // EXECUTE ROUND
    // =====================================================

    private IEnumerator ExecuteRound()
    {
        currentState = BattleState.ExecutingActions;

        battleUI.SetCommandsInteractable(false);

        foreach (PlannedAction action in plannedActions)
        {
            if (action.user.IsDead())
                continue;

            yield return StartCoroutine(
                ExecutePlayerAction(action)
            );

            battleUI.RefreshAll(players, enemy);

            if (enemy.IsDead())
            {
                Victory();
                yield break;
            }

            yield return new WaitForSeconds(actionDelay);
        }

        yield return StartCoroutine(EnemyTurn());

        battleUI.RefreshAll(players, enemy);

        if (AllPlayersDead())
        {
            Defeat();
            yield break;
        }

        yield return new WaitForSeconds(actionDelay);

        StartNewRound();
    }

    // =====================================================
    // PLAYER ACTIONS
    // =====================================================

    private IEnumerator ExecutePlayerAction(
        PlannedAction action)
    {
        switch (action.actionType)
        {
            case ActionType.Attack:
                yield return PlayerAttack(action);
                break;

            case ActionType.Skill:
                yield return PlayerSkill(action);
                break;

            case ActionType.Item:
                yield return PlayerItem(action);
                break;

            case ActionType.Guard:
                yield return PlayerGuard(action);
                break;
        }
    }

    private IEnumerator PlayerAttack(
        PlannedAction action)
    {
        battleUI.SetMessage(
            $"{action.user.unitName} attacks!"
        );

        yield return new WaitForSeconds(actionDelay);

        int damage =
            action.target.TakeDamage(
                action.user.attack
            );

        battleUI.SetMessage(
            $"{enemy.unitName} takes {damage} damage!"
        );
    }

    private IEnumerator PlayerSkill(
        PlannedAction action)
    {
        action.user.SpendMP(skillMPCost);

        battleUI.SetMessage(
            $"{action.user.unitName} uses Skill!"
        );

        battleUI.RefreshAll(players, enemy);

        yield return new WaitForSeconds(actionDelay);

        int damage =
            action.target.TakeDamage(
                action.user.attack +
                skillBonusDamage
            );

        battleUI.SetMessage(
            $"{enemy.unitName} takes {damage} damage!"
        );
    }

    private IEnumerator PlayerItem(
        PlannedAction action)
    {
        potionCount--;

        int healed =
            action.user.Heal(potionHealAmount);

        battleUI.SetMessage(
            $"{action.user.unitName} heals {healed} HP!"
        );

        yield return null;
    }

    private IEnumerator PlayerGuard(
        PlannedAction action)
    {
        action.user.isGuarding = true;

        battleUI.SetMessage(
            $"{action.user.unitName} is guarding!"
        );

        yield return null;
    }

    // =====================================================
    // ENEMY
    // =====================================================

    private IEnumerator EnemyTurn()
    {
        if (enemy.IsDead())
            yield break;

        BattleUnit target =
            GetRandomLivingPlayer();

        if (target == null)
            yield break;

        battleUI.SetMessage(
            $"{enemy.unitName} attacks {target.unitName}!"
        );

        yield return new WaitForSeconds(actionDelay);

        int damage =
            target.TakeDamage(enemy.attack);

        battleUI.SetMessage(
            $"{target.unitName} takes {damage} damage!"
        );

        battleUI.RefreshAll(players, enemy);
    }

    private BattleUnit GetRandomLivingPlayer()
    {
        List<BattleUnit> livingPlayers =
            new List<BattleUnit>();

        foreach (BattleUnit player in players)
        {
            if (!player.IsDead())
                livingPlayers.Add(player);
        }

        if (livingPlayers.Count == 0)
            return null;

        int randomIndex =
            Random.Range(0, livingPlayers.Count);

        return livingPlayers[randomIndex];
    }

    // =====================================================
    // END CONDITIONS
    // =====================================================

    private bool AllPlayersDead()
    {
        foreach (BattleUnit player in players)
        {
            if (!player.IsDead())
                return false;
        }

        return true;
    }

    private void Victory()
    {
        currentState = BattleState.Victory;

        battleUI.SetCommandsInteractable(false);

        battleUI.SetMessage(
            "VICTORY! The enemy has been defeated."
        );
    }

    private void Defeat()
    {
        currentState = BattleState.Defeat;

        battleUI.SetCommandsInteractable(false);

        battleUI.SetMessage(
            "DEFEAT..."
        );
    }
}