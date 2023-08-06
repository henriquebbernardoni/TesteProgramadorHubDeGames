using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static ZombieController;

//Script controlador geral dos Sobrevivente (PJ ou não)
//Caso sejam PJ eles terão acesso a funções mais avançadas
public class SurvivorController : GenCharacterController
{
    public enum SurvivorState { WANDER, HIDE, FOLLOW, DEATH }
    private SurvivorState currentState;

    private bool isPlayerCharacter = false;
    [SerializeField] private Material survivorMaterial;
    [SerializeField] private Material playerMaterial;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente está escondido
    //Caso esse fosse um projeto mais avançado, eu implementaria uma solução melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;

    private Weapon selectedWeapon;
    private float rechargeTime = 3f;
    private bool isRecharging = false;

    public bool IsRecharging { get => isRecharging; private set => isRecharging = value; }

    //Esse código DEVE ser usado para se alterar o estado atual do Sobrevivente.
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

    //Essa função é usada quando um Sobrevivente está se mexendo,
    //garantindo que ele sempre entre no modo Wander quando se movimenta.
    public void SetSurvivorDestination(Vector3 destination)
    {
        if (currentState != SurvivorState.WANDER)
        {
            SetState(SurvivorController.SurvivorState.WANDER);
        }
        Agent.SetDestination(destination);

    }

    //Essa função detecta algum ponto de esconderijo próximo ao Sobrevivente.
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