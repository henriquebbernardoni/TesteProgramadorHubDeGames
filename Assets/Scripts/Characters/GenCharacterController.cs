using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public abstract class GenCharacterController : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int currentHealth;
    private TextMeshPro healthText;
    private Transform _camera;
    private Quaternion previousTextRotation;

    public NavMeshAgent Agent { get; private set; }

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        healthText = GetComponentInChildren<TextMeshPro>();
        _camera = Camera.main.transform;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthText();
    }

    private void LateUpdate()
    {
        PointTextToCamera();
    }

    private void PointTextToCamera()
    {
        if (previousTextRotation != transform.rotation)
        {
            previousTextRotation = _camera.rotation;
            healthText.transform.rotation = previousTextRotation;
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = currentHealth.ToString();
    }

    public void ModifyHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthText();

        if (currentHealth == 0)
        {
            DeathBehaviour();
        }
    }

    protected virtual void DeathBehaviour()
    {

    }

    public void FullStop()
    {
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
    }
}