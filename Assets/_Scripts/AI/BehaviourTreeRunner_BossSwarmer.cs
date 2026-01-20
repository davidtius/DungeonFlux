using UnityEngine;
using DungeonFlux.AI;
using DungeonFlux.Tasks;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Condition_IsPlayerInSight))]
[RequireComponent(typeof(Action_BossSwarmer_ChaseAndRest))]
[RequireComponent(typeof(Action_Patrol))]

public class BehaviourTreeRunner_BossSwarmer : MonoBehaviour
{
    private Node rootNode;
    private Vector2 lastKnownPlayerPosition;
    private bool hasSeenPlayerBefore = false;
    private Health health;
    private Condition_IsPlayerInSight task_IsPlayerInSight;
    private Action_Patrol task_Patrol;
    private Action_BossSwarmer_ChaseAndRest task_BossSwarmer;

    void Start()
    {

        health = GetComponent<Health>();
        task_IsPlayerInSight = GetComponent<Condition_IsPlayerInSight>();
        task_BossSwarmer = GetComponent<Action_BossSwarmer_ChaseAndRest>();
        task_Patrol = GetComponent<Action_Patrol>();

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
        rootNode = new Selector(new List<Node>
        {

            new Sequence(new List<Node>
            {
                new ActionNode(task_IsPlayerInSight.Check),
                new ActionNode(task_BossSwarmer.ExecuteTask)
            }),

            new ActionNode(task_Patrol.ExecuteTask)
        });
    }

    public Vector2 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }
}