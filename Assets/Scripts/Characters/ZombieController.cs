using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SurvivorController;

//Código de controle dos Zumbis, controlando as diferentes rotinas
public class ZombieController : GenCharacterController
{
    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    [SerializeField] private ZombieState currentState;

    private List<Transform> wanderPoints;
    [SerializeField] private Transform face;

    protected override void Awake()
    {
        base.Awake();

        //Esse código encontra os pontos de rota pelos quais o Zumbi irá passar.
        Transform wanderPointsParent = GameObject.Find("ZombieWanderPoints").transform;
        wanderPoints = wanderPointsParent.Cast<Transform>().ToList();
    }

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
                PlayerController.PC.AddToHuntedList(this, true);
                StartCoroutine(HuntingRoutine());
                break;
            case ZombieState.DEATH:
                PlayerController.PC.RemoveFromHuntedList(this, true);
                LevelController.Instance.AddToKilledZombies();
                if (LevelController.Instance.AllZombiesDead())
                {
                    LevelController.Instance.NextLevelZombiesDead();
                }
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
            yield return null;

            while (Agent.hasPath)
            {
                if (DetectSurvivors())
                {
                    WarningText.Instance.SetWarningText("Você foi detectado! Fuja ou ataque!");
                    SetState(ZombieState.ATTACK);
                }
                yield return null;
            }
        }
    }

    //Essa função gera um novo ponto de rota, garantindo que
    //o Agente não repita dois pontos de Rota seguidos.
    private Vector3 GetRandomWanderPoint()
    {
        int value = Random.Range(0, wanderPoints.Count);
        return wanderPoints[value].position;
    }

    //Essa função detecta se algum sobrevivente cruzar diretamente a frente do zumbi.
    private bool DetectSurvivors()
    {
        Vector3 origin = face.position;
        Vector3 direction = face.forward;
        Ray ray = new(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            SurvivorController survivorController = hit.collider.GetComponent<SurvivorController>();
            if (survivorController != null)
            {
                SurvivorState state = survivorController.GetState();
                if (state == SurvivorState.FOLLOW || state == SurvivorState.WANDER)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //Quando o zumbi estiver atacando, sua velocidade aumenta consideravelmente.
    //O sobrevivente mais próximo que não é o jogador é escolhido para o ataque.
    private IEnumerator HuntingRoutine()
    {
        SurvivorController nearestSurvivor;
        while (true)
        {
            if (PlayerController.PC.SurvivorGroup.Count <= 1)
            {
                nearestSurvivor = PlayerController.PC;
                if (nearestSurvivor.GetState() == SurvivorState.FINAL)
                {
                    nearestSurvivor = null;
                }
            }
            else
            {
                nearestSurvivor = PlayerController.PC.SurvivorGroup
                    .Where(survivor => survivor != PlayerController.PC)
                    .OrderBy(survivor => Vector3.Distance(transform.position, survivor.transform.position))
                    .FirstOrDefault();
            }

            if (!nearestSurvivor)
            {
                SetState(ZombieState.WANDER);
                yield break;
            }

            if (Vector3.Distance(transform.position, nearestSurvivor.transform.position) >= 2f)
            {
                Agent.SetDestination(nearestSurvivor.transform.position);
            }
            else
            {
                yield return StartCoroutine(ZombieAttackRoutine(nearestSurvivor));
            }


            yield return new WaitForEndOfFrame();
        }
    }

    //Ao chegar perto o zumbi ataca. Se o defensor estiver segurando uma arma,
    //ele revida ao ataque do zumbi. O comportamento normal da arma é realizado.
    private IEnumerator ZombieAttackRoutine(SurvivorController survivor)
    {
        IEnumerator attackCoroutine = ZombieAttack(survivor);
        yield return StartCoroutine(attackCoroutine);

        FullStop();
        survivor.FullStop();
        yield return new WaitForSeconds(rechargeTime);
    }

    private IEnumerator ZombieAttack(SurvivorController survivor)
    {
        if (survivor.GetWeapon() && !IsRecharging)
        {
            WarningText.Instance.SetWarningText("Um zumbi tentou atacar mas o sobrevivente revidou!");
            survivor.transform.LookAt(transform.position);
            yield return StartCoroutine(survivor.GetWeapon().WeaponBehaviour(survivor, this));
        }
        else
        {
            WarningText.Instance.SetWarningText("Um zumbi atacou um sobrevivente!");
            float hitChance = 0.5f;
            if (Random.Range(0f, 1f) <= hitChance)
            {
                survivor.ModifyHealth(-1);
            }
            else
            {
                WarningText.Instance.AddToWarningText("O ataque do zumbi errou!");
            }
        }
    }

    protected override void DeathBehaviour()
    {
        SetState(ZombieState.DEATH);
    }
}