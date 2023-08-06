using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] wanderPoints;
    private int currentPoint;

    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    private ZombieState state;

    private Quaternion previousRotation;
    private int health = 2;
    private TextMeshPro healthText;
    private Transform _camera;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        healthText = GetComponentInChildren<TextMeshPro>();
        _camera = Camera.main.transform;
    }

    private void Start()
    {
        SetState(ZombieState.NULL);
        UpdateHealthText();
    }

    private void LateUpdate()
    {
        PointTextToCamera();
    }

    private void PointTextToCamera()
    {
        if (previousRotation != transform.rotation)
        {
            previousRotation =
                Quaternion.LookRotation(-(_camera.transform.position - healthText.transform.position));
            healthText.transform.rotation = previousRotation;
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString();
    }

    public void SetState(ZombieState newState)
    {
        StopAllCoroutines();
        state = newState;

        switch (newState)
        {
            case ZombieState.WANDER:
                agent.speed = 2f;
                StartCoroutine(WanderRoutine());
                break;
            case ZombieState.ATTACK:
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

    public void ModifyHealth(int amount)
    {
        health += amount;

        if (health < 0)
        {
            health = 0;
        }

        UpdateHealthText();

        if (health == 0)
        {
            SetState(ZombieState.DEATH);
        }
    }
}