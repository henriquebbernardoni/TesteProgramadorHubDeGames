using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public abstract class GenCharacterController : MonoBehaviour
{
    //Atributos relacionados a vida do personagem.
    [SerializeField] private TextMeshPro healthText;
    [SerializeField] private int maxHealth;
    private int currentHealth;

    //Vari�veis relacionadas a recargas de ataque.
    [SerializeField] protected float rechargeTime;
    public bool IsRecharging { get; private set; }

    [SerializeField] private NavMeshAgent _agent;
    public NavMeshAgent Agent { get => _agent; }

    protected virtual void Awake()
    {
        ModifyHealth(maxHealth);
    }

    private void Update()
    {
    }

    //Quando esse personagem atacar, ele ter� de esperar
    //o tempo de recarga para atacar novamente.
    public IEnumerator RechargeAttack()
    {
        if (IsRecharging)
        {
            yield break;
        }

        IsRecharging = true;
        yield return new WaitForSeconds(rechargeTime);
        IsRecharging = false;
    }

    //Use essa fun��o para alterar a sa�de do personagem,
    //seja para aumentar ou diminuir.
    public void ModifyHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthText.text = currentHealth.ToString();

        if (currentHealth == 0)
        {
            DeathBehaviour();
        }
    }

    //O comportamento que � ativado quando esse personagem morre.
    //Deixe em branco, esse comportamento s� � modificado por classes herdeiras.
    protected virtual void DeathBehaviour()
    {

    }

    //Essa fun��o para o personagem completamente.
    public void FullStop()
    {
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
    }
}