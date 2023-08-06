using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static ZombieController;

//Script controlador geral dos Sobrevivente (PJ ou n�o)
//Caso sejam PJ eles ter�o acesso a fun��es mais avan�adas
public class SurvivorController : GenCharacterController
{
    public enum SurvivorState { WANDER, HIDE, FOLLOW, DEATH }
    private SurvivorState currentState;

    private bool isPlayerCharacter = false;
    [SerializeField] private Material survivorMaterial;
    [SerializeField] private Material playerMaterial;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente est� escondido
    //Caso esse fosse um projeto mais avan�ado, eu implementaria uma solu��o melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;

    private Weapon selectedWeapon;
    private float rechargeTime = 3f;
    private bool isRecharging = false;

    public bool IsRecharging { get => isRecharging; private set => isRecharging = value; }

    //Esse c�digo DEVE ser usado para se alterar o estado atual do Sobrevivente.
    public void SetState(SurvivorState state)
    {
        currentState = state;

        switch (currentState)
        {
            case SurvivorState.HIDE:
                hiddenRenderer.material = hiddenMaterial;
                break;
            default:
                hiddenRenderer.material = nonHiddenMaterial;
                break;
        }
    }
    public SurvivorState GetState()
    {
        return currentState;
    }

    //Essa fun��o � usada quando um Sobrevivente est� se mexendo,
    //garantindo que ele sempre entre no modo Wander quando se movimenta.
    public void SetSurvivorDestination(Vector3 destination)
    {
        if (currentState != SurvivorState.WANDER)
        {
            SetState(SurvivorController.SurvivorState.WANDER);
        }
        Agent.SetDestination(destination);

    }

    //Essa fun��o detecta algum ponto de esconderijo pr�ximo ao Sobrevivente.
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

    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }

    public void SetWeapon(Weapon weapon)
    {
        selectedWeapon = weapon;
    }

    public IEnumerator RechargeAttack()
    {
        if (isRecharging)
        {
            yield break;
        }

        float waitTime = 0f;
        isRecharging = true;
        while (waitTime <= rechargeTime)
        {
            waitTime += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        isRecharging = false;
    }

    protected override void DeathBehaviour()
    {
        SetState(SurvivorState.DEATH);
    }

    public void SetAsPlayerCharacter(bool isPlayer)
    {
        isPlayerCharacter = isPlayer;

        if (isPlayer)
        {
            GetComponentInChildren<Renderer>().material = playerMaterial;
        }
        else
        {
            GetComponentInChildren<Renderer>().material = survivorMaterial;
        }
    }
}