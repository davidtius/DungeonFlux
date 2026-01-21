using UnityEngine;
using DungeonFlux.AI;
using DungeonFlux.Tasks;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Action_MoveToLastKnownPosition))]
[RequireComponent(typeof(Condition_IsPlayerInSight))]
[RequireComponent(typeof(Condition_IsPlayerInMeleeRange))]
[RequireComponent(typeof(Condition_IsTargetedByPlayer))]
[RequireComponent(typeof(Action_Patrol))]
[RequireComponent(typeof(Action_Chase))]
[RequireComponent(typeof(Action_Flee))]
[RequireComponent(typeof(Action_JagaJarak))]
[RequireComponent(typeof(Action_Evade))]
[RequireComponent(typeof(Action_UseSkill))]
[RequireComponent(typeof(Action_AttackMelee))]

public class BehaviourTreeRunner : MonoBehaviour
{
    private Node rootNode;
    private Vector2 lastKnownPlayerPosition;
    private bool hasSeenPlayerBefore = false;
    private Health health;
    private Action_MoveToLastKnownPosition task_MoveToLastKnownPosition;
    private Condition_IsPlayerInSight task_IsPlayerInSight;
    private Condition_IsPlayerInMeleeRange task_IsPlayerInMeleeRange;
    private Condition_IsTargetedByPlayer task_IsTargetedByPlayer;
    private Action_Patrol task_Patrol;
    private Action_Chase task_Chase;
    private Action_Flee task_Flee;
    private Action_JagaJarak task_JagaJarak;
    private Action_Evade task_Evade;
    private Action_UseSkill task_UseSkill;
    private Action_AttackMelee task_AttackMelee;

    [Header("BT Tactic Settings")]

    [Tooltip("Cooldown dasar untuk taktik SkillOriented")]
    [SerializeField] private float skillCooldown = 5.0f;

    [Tooltip("Cooldown dasar untuk taktik Evading")]
    [SerializeField] private float evadeCooldown = 3.0f;

    [SerializeField] private UtilitySelector utilitySelector_BattleTactics;
    [SerializeField] private List<float> baseUtilities = new List<float> { 1.8f, 0.3f, 1.0f, 1.4f };

    void Start()
    {

        health = GetComponent<Health>();
        task_MoveToLastKnownPosition = GetComponent<Action_MoveToLastKnownPosition>();
        task_IsPlayerInSight = GetComponent<Condition_IsPlayerInSight>();
        task_IsPlayerInMeleeRange = GetComponent<Condition_IsPlayerInMeleeRange>();
        task_IsTargetedByPlayer = GetComponent<Condition_IsTargetedByPlayer>();
        task_Patrol = GetComponent<Action_Patrol>();
        task_Chase = GetComponent<Action_Chase>();
        task_Flee = GetComponent<Action_Flee>();
        task_JagaJarak = GetComponent<Action_JagaJarak>();
        task_Evade = GetComponent<Action_Evade>();
        task_UseSkill = GetComponent<Action_UseSkill>();
        task_AttackMelee = GetComponent<Action_AttackMelee>();
        BuildBehaviorTree();
    }

    void Update()
    {
        if (rootNode != null)
        {
            rootNode.Execute();
        }
    }

    private void BuildBehaviorTree()
    {
        Node tactic_Aggressive_Tank = new Selector(new List<Node>
        {

            new Sequence(new List<Node>
            {
                new ActionNode(task_IsPlayerInMeleeRange.Check),
                new ActionNode(task_AttackMelee.ExecuteTask)
            }),

            new Sequence(new List<Node>
            {
                new CooldownDecorator(new ActionNode(task_UseSkill.ExecuteTask), skillCooldown),
                new ActionNode(() => NodeState.SUCCESS)
            }),

            new ActionNode(task_Chase.ExecuteTask)
        });

        Node tactic_Aggressive = new Selector(new List<Node>
        {

            new Sequence(new List<Node>
            {
                new ActionNode(task_IsPlayerInMeleeRange.Check),
                new ActionNode(task_AttackMelee.ExecuteTask)
            }),

            new Sequence(new List<Node>
            {
                new Selector(new List<Node>
                {
                    new CooldownDecorator(
                        new ActionNode(task_UseSkill.ExecuteTask),
                        skillCooldown
                    ),
                    new ActionNode(() => NodeState.SUCCESS)
                }),
                new ActionNode(task_Chase.ExecuteTask)
            })
        });

        Node tactic_JagaJarak = new Sequence(new List<Node>
        {
            new Selector(new List<Node>
             {
                 new CooldownDecorator(
                    new ActionNode(task_UseSkill.ExecuteTask),
                    skillCooldown
                ),
                new ActionNode(() => NodeState.SUCCESS)
             }),

             new ActionNode(task_JagaJarak.ExecuteTask)
        });

        Node tactic_SkillOriented = new Sequence(new List<Node>
        {
             new CooldownDecorator(
                new ActionNode(task_UseSkill.ExecuteTask),
                skillCooldown
            )
        });

        Node tactic_Evading = new Sequence(new List<Node>
        {
             new ActionNode(task_IsTargetedByPlayer.Check),
             new CooldownDecorator(
                new ActionNode(task_Evade.ExecuteTask),
                evadeCooldown
            )
        });

        List<Node> battleTactics = new List<Node> { tactic_Aggressive, tactic_JagaJarak, tactic_SkillOriented, tactic_Evading};
        List<string> tacticNames = new List<string> { "Aggressive", "JagaJarak", "SkillOriented", "Evading" };
        utilitySelector_BattleTactics = new UtilitySelector(battleTactics, tacticNames, baseUtilities);

        rootNode = new Selector(new List<Node>
        {

            new Sequence(new List<Node>
            {
                new ActionNode(() => health.GetCurrentHealthPercentage() < 0.25f ? NodeState.SUCCESS : NodeState.FAILURE),
                new ActionNode(task_Flee.ExecuteTask)
            }),

            new Sequence(new List<Node>
            {

                new ActionNode(() => {
                    NodeState sightCheck = task_IsPlayerInSight.Check();
                    if (sightCheck == NodeState.SUCCESS)
                    {
                        Transform playerPos = task_IsPlayerInSight.getPlayerTransform();
                        if (playerPos != null) {
                            lastKnownPlayerPosition = playerPos.position;
                        }
                        hasSeenPlayerBefore = true;
                    }
                    return sightCheck;
                }),

                new Selector(new List<Node>
                {

                    new Sequence(new List<Node>
                    {

                        new ActionNode(() => (health.enemyVariant == Health.EnemyVariant.Tank) ? NodeState.SUCCESS : NodeState.FAILURE),

                        tactic_Aggressive_Tank
                    }),

                    utilitySelector_BattleTactics
                })
            }),

            new Sequence(new List<Node>
            {
                new ActionNode(() => hasSeenPlayerBefore ? NodeState.SUCCESS : NodeState.FAILURE),
                new InverterDecorator(new ActionNode(task_IsPlayerInSight.Check)),
                new ActionNode(task_MoveToLastKnownPosition.ExecuteTask)
            }),

            new ActionNode(() => {
                hasSeenPlayerBefore = false;
                return task_Patrol.ExecuteTask();
            })
        });
    }

    public Vector2 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }

    public void UpdateDDAWeights(Dictionary<string, float> newWeights)
    {
        if (utilitySelector_BattleTactics != null)
        {
            utilitySelector_BattleTactics.SetWeights(newWeights);
             Debug.Log($"[{gameObject.name}] Menerima bobot DDA baru.");
        }
    }
}