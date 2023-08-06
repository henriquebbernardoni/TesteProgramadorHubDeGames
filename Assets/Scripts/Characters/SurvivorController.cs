using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//Script controlador geral dos Sobrevivente (PJ ou não)
//Caso sejam PJ eles terão acesso a funções mais avançadas
public class SurvivorController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }

    public enum SurvivorState { WANDER, HIDE, DEATH }
    private SurvivorState currentState;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente está escondido
    //Caso esse fosse um projeto mais avançado, eu implementaria uma solução melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;

    private Weapon selectedWeapon;
    private float rechargeTime = 3f;
    private bool isRecharging = false;

    private Quaternion previousRotation;
    private int health = 3;
    private TextMeshPro healthText;
    private Transform _camera;

    public bool IsRecharging { get => isRecharging; private set => isRecharging = value; }


    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        healthText = GetComponentInChildren<TextMeshPro>();
        _camera = Camera.main.transform;
    }

    private void Start()
    {
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
            previousRotation = Camera.main.transform.rotation;
            healthText.transform.rotation = previousRotation;
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString();
    }

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

    public void FullStop()
    {
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
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
            SetState(SurvivorState.DEATH);
        }
    }
}