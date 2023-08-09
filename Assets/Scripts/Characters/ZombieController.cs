using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//Código de controle dos Zumbis, controlando as diferentes rotinas
public class ZombieController : GenCharacterController
{
    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    [SerializeField] private ZombieState currentState;

    [SerializeField] private Transform face;
    private List<Transform> wanderPoints;

    private List<SurvivorController> huntedSurvivors;

    //Essas duas funções tem que ser usadas para se alterar e checar o estado atual do zumbi.
    public void SetState(ZombieState newState)
    {
        if (currentState == newState) return;

        StopAllCoroutines();
        FullStop();
        currentState = newState;

        switch (newState)
        {
            case ZombieState.WANDER:
                Agent.speed = 2f;
                StartCoroutine(WanderRoutine());
                break;
            case ZombieState.ATTACK:
                Agent.speed = 5.5f;
                StartCoroutine(HuntingRoutine());
                break;
            case ZombieState.DEATH:
                break;
            case ZombieState.NULL:
                break;
        }
    }
    public ZombieState GetState()
    {
        return currentState;
    }

    //No modo Wander, o Zumbi caminhará aleoriamente entre diferentes pontos do mapa
    //Os pontos são fixos, porém espaçados e em quantidade suficiente para suprir a necessidade.
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            Vector3 point = GetRandomWanderPoint();
            Agent.SetDestination(point);
            yield return new WaitForEndOfFrame();
            while (Agent.hasPath)
            {
                if (DetectSurvivors())
                {
                    SetState(ZombieState.ATTACK);
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

    //Essa função detecta se algum sobrevivente cruzar diretamente a frente do zumbi.
    private SurvivorController DetectSurvivors()
    {
        Vector3 origin = face.position;
        Vector3 direction = face.forward;
        Ray ray = new(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, 7.5f))
        {
            SurvivorController survivorController = hit.collider.GetComponent<SurvivorController>();
            if (survivorController != null)
            {
                SurvivorController.SurvivorState state = survivorController.GetState();
                if (state == SurvivorController.SurvivorState.FOLLOW || 
                    state == SurvivorController.SurvivorState.WANDER)
                {
                    return survivorController;
                }
            }
        }

        return null;
    }

    //Esse código encontra os pontos de rota pelos quais o Zumbi irá passar.
    public void FindWanderPoints()
    {
        Transform wanderPointsParent = GameObject.Find("ZombieWanderPoints").transform;
        wanderPoints = wanderPointsParent.Cast<Transform>().ToList();
    }

    //Essa função gera um novo ponto de rota, garantindo que
    //o Agente não repita dois pontos de Rota seguidos.
    private Vector3 GetRandomWanderPoint()
    {
        int value = Random.Range(0, wanderPoints.Count);
        return wanderPoints[value].position;
    }

    private IEnumerator HuntingRoutine()
    {
        Debug.Log("Caçando!");
        yield return null;
    }

    protected override void DeathBehaviour()
    {
        SetState(ZombieState.DEATH);
    }
}