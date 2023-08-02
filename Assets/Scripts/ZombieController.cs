using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] wanderPoints;
    private int currentPoint;

    enum ZombieState { WANDER, ATTACK, DEATH }
    private ZombieState state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        SetZombieState(ZombieState.WANDER);
    }

    private void SetZombieState(ZombieState newState)
    {
        StopAllCoroutines();
        state = newState;

        switch (newState)
        {
            case ZombieState.WANDER:
                StartCoroutine(WanderRoutine());
                break;
            case ZombieState.ATTACK:
                break;
            case ZombieState.DEATH:
                break;
        }
    }

    //No modo Wander, o Zumbi caminhará aleoriamente entre diferentes pontos do mapa
    //Os pontos são fixos, porém espaçados e em quantidade suficiente para suprir a necessidade.
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            agent.SetDestination(ChooseNewWanderPoint());
            yield return new WaitForEndOfFrame();
            while (agent.hasPath)
            {
                yield return new WaitForEndOfFrame();
            }
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
            value = Random.Range(0, wanderPoints.Length);
        }
        vector3 = wanderPoints[value].position;
        return vector3;
    }
}