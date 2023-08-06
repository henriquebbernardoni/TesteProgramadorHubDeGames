using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//Código de controle dos Zumbis, controlando as diferentes rotinas
public class ZombieController : GenCharacterController
{
    private List<Transform> wanderPoints;
    private int currentPoint;

    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    private ZombieState state;

    protected override void Start()
    {
        base.Start();
        FindWanderPoints();
        SetState(ZombieState.NULL);
    }

    public void SetState(ZombieState newState)
    {
        StopAllCoroutines();
        FullStop();
        state = newState;

        switch (newState)
        {
            case ZombieState.WANDER:
                Agent.speed = 2f;
                StartCoroutine(WanderRoutine());
                break;
            case ZombieState.ATTACK:
                Agent.speed = 5.5f;
                break;
            case ZombieState.DEATH:
                break;
            case ZombieState.NULL:
                break;
        }
    }
    public ZombieState GetState()
    {
        return state;
    }

    //No modo Wander, o Zumbi caminhará aleoriamente entre diferentes pontos do mapa
    //Os pontos são fixos, porém espaçados e em quantidade suficiente para suprir a necessidade.
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            Agent.SetDestination(ChooseNewWanderPoint());
            yield return new WaitForEndOfFrame();
            while (Agent.hasPath)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void FindWanderPoints()
    {
        Transform wanderPointsParent = GameObject.Find("ZombieWanderPoints").transform;
        for (int i = 0; i < wanderPointsParent.childCount; i++)
        {
            wanderPoints.Add(wanderPointsParent.GetChild(i));
        }
    }

    //Essa função gera um novo ponto de rota, garantindo que
    //o Agente não repita dois pontos de Rota seguidos.
    private Vector3 ChooseNewWanderPoint()
    {
        Vector3 vector3;
        int value;
        value = currentPoint;
        while (value == currentPoint)
        {
            value = Random.Range(0, wanderPoints.Count);
        }
        vector3 = wanderPoints[value].position;
        return vector3;
    }

    protected override void DeathBehaviour()
    {
        SetState(ZombieState.DEATH);
    }
}