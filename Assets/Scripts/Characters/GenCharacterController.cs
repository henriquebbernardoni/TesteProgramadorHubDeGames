using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public abstract class GenCharacterController : MonoBehaviour
{
    //Atributos relacionados a vida do personagem.
    [SerializeField] private int maxHealth;
    private int currentHealth;
    private TextMeshPro healthText;
    private Quaternion previousHTextRotation;

    [SerializeField] protected float rechargeTime;

    private Transform _camera;

    public NavMeshAgent Agent { get; private set; }
    public bool IsRecharging { get; private set; }

    private void Awake()
    {
        healthText = GetComponentInChildren<TextMeshPro>();
        _camera = Camera.main.transform;
        Agent = GetComponent<NavMeshAgent>();
    }

    private void LateUpdate()
    {
        PointTextToCamera();
    }

    //Caso a rotação do text mude entre um frame e outro,
    //a rotação irá modificar para comportar a alteração.
    private void PointTextToCamera()
    {
        if (previousHTextRotation != healthText.transform.rotation)
        {
            previousHTextRotation = _camera.rotation;
            healthText.transform.rotation = previousHTextRotation;
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = currentHealth.ToString();
    }

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

    public void ModifyHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthText();
        if (currentHealth == 0)
        {
            DeathBehaviour();
        }
    }

    //O comportamento que é ativado quando esse personagem morre.
    protected virtual void DeathBehaviour()
    {

    }

    //Essa função para o personagem completamente.
    public void FullStop()
    {
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
    }
}