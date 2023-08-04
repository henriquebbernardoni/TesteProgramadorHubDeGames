using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Script controlador geral dos Sobrevivente (PJ ou n�o)
//Caso sejam PJ eles ter�o acesso a fun��es mais avan�adas
public class SurvivorController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    public enum SurvivorStates { WANDER, HIDE }
    private SurvivorStates currentState;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente est� escondido
    //Caso esse fosse um projeto mais avan�ado, eu implementaria uma solu��o melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    //Esse c�digo DEVE ser usado para se alterar o estado atual do Sobrevivente.
    public void SetState(SurvivorStates state)
    {
        currentState = state;

        switch (currentState)
        {
            case SurvivorStates.HIDE:
                hiddenRenderer.material = hiddenMaterial;
                break;
            default:
                hiddenRenderer.material = nonHiddenMaterial;
                break;
        }
    }

    //Essa fun��o � usada quando um Sobrevivente est� se mexendo,
    //garantindo que ele sempre entre no modo Wander quando se movimenta.
    public void SetSurvivorDestination(Vector3 destination)
    {
        if (currentState != SurvivorStates.WANDER)
        {
            SetState(SurvivorController.SurvivorStates.WANDER);
        }
        Agent.SetDestination(destination);
    }

    //Essa fun��o detecta algum ponto de esconderijo extremamente pr�ximo do Sobrevivente.
    public HidingSpot DetectNearbyHidingSpot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f);

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<HidingSpot>())
            {
                return collider.GetComponent<HidingSpot>();
            }
        }
        return null;
    }
}