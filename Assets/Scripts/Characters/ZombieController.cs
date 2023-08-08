using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//Código de controle dos Zumbis, controlando as diferentes rotinas
public class ZombieController : GenCharacterController
{
    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    private ZombieState state;

    private List<Transform> wanderPoints;
    private int currentPoint;

    protected void Start()
    {
        FindWanderPoints();
        SetState(ZombieState.NULL);
    }

    //Essas duas funções tem que ser usadas para se alterar e checar o estado atual do zumbi.
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
            Agent.SetDestination(GetRandomWanderPoint());
            yield return new WaitForEndOfFrame();
            while (Agent.hasPath)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    //Esse código encontra os pontos de rota pelos quais o Zumbi irá passar.
    private void FindWanderPoints()
    {
        Transform wanderPointsParent = GameObject.Find("ZombieWanderPoints").transform;
        foreach (Transform child in wanderPointsParent)
        {
            wanderPoints.Add(child);
        }
    }

    //Essa função gera um novo ponto de rota, garantindo que
    //o Agente não repita dois pontos de Rota seguidos.
    private Vector3 GetRandomWanderPoint()
    {
        int value = Random.Range(0, wanderPoints.Count);
        while (value == currentPoint)
        {
            value = Random.Range(0, wanderPoints.Count);
        }
        currentPoint = value;
        return wanderPoints[value].position;
    }

    protected override void DeathBehaviour()
    {
        SetState(ZombieState.DEATH);
    }
}