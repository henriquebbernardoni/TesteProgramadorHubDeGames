using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using static SurvivorController;

//Código de controle dos Zumbis, controlando as diferentes rotinas
public class ZombieController : GenCharacterController
{
    public enum ZombieState { WANDER, ATTACK, DEATH, NULL }
    [SerializeField] private ZombieState currentState;

    [SerializeField] private Transform face;
    private List<Transform> wanderPoints;

    private bool isAttacking = false;

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
                LevelController.Instance.AddToKilledZombies();
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
                    WarningText.Instance.SetWarningText("Você foi detectado! Fuja ou ataque!");
                    SetState(ZombieState.ATTACK);
                }
                yield return new WaitForEndOfFrame();
            }
        }
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
                SurvivorController.SurvivorState state = survivorController.GetState();
                if (state == SurvivorController.SurvivorState.FOLLOW ||
                    state == SurvivorController.SurvivorState.WANDER)
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
            if (PlayerCharacter.SurvivorGroup.Count <= 1)
            {
                nearestSurvivor = PlayerCharacter;
            }
            else
            {
                nearestSurvivor = PlayerCharacter.SurvivorGroup.
                    Where(survivor => survivor != PlayerCharacter).OrderBy(survivor =>
                    Vector3.Distance(transform.position, survivor.transform.position)).FirstOrDefault();
            }

            if (Vector3.Distance(transform.position, nearestSurvivor.transform.position) >= 2f)
            {
                Agent.SetDestination(nearestSurvivor.transform.position);
            }
            else
            {
                StartCoroutine(ZombieAttack(nearestSurvivor));
                yield return new WaitUntil(() => isAttacking);
                yield return new WaitWhile(() => isAttacking);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //Ao chegar perto o zumbi ataca. Se o defensor estiver segurando uma arma,
    //ele revida ao ataque do zumbi. O comportamento normal da arma é realizado.
    private IEnumerator ZombieAttack(SurvivorController survivor)
    {
        isAttacking = true;
        if (survivor.GetWeapon() && !survivor.IsRecharging)
        {
            WarningText.Instance.SetWarningText("Um zumbi tentou atacar mas o sobrevivente revidou!");
            survivor.transform.LookAt(transform.position);
            StartCoroutine(survivor.GetWeapon().WeaponBehaviour(survivor, this));
        }
        else
        {
            WarningText.Instance.SetWarningText("Um zumbi atacou um sobrevivente!");
            if (Random.value <= 0.5f)
            {
                survivor.ModifyHealth(-1);
            }
            else
            {
                WarningText.Instance.AddToWarningText("O ataque do zumbi errou!");
            }
        }
        FullStop();
        survivor.FullStop();
        yield return new WaitForSeconds(rechargeTime);
        isAttacking = false;
    }

    protected override void DeathBehaviour()
    {
        SetState(ZombieState.DEATH);
    }

    //Essa função assegura que a mudança de estado e material jogador ocorra de forma correta.
    public void SetPlayerCharacter(SurvivorController newPlayer)
    {
        PlayerCharacter = newPlayer;
    }
}