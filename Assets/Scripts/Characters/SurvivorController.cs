using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//Script controlador geral dos Sobrevivente (PJ ou não)
//Caso sejam PJ eles terão acesso a funções mais avançadas
public class SurvivorController : GenCharacterController
{
    public enum SurvivorState { INITIAL, WANDER, RESCUE, FOLLOW, HIDE, DEATH }
    [SerializeField] private SurvivorState currentState;

    [SerializeField] private Material survivorMaterial;
    [SerializeField] private Material playerMaterial;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente está escondido
    //Caso esse fosse um projeto mais avançado, eu implementaria uma solução melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;
    [SerializeField] private HidingSpot currentHidingSpot;

    private Weapon selectedWeapon;

    [SerializeField] private List<SurvivorController> survivorGroup;

    //O grupo refere às pessoas que o jogador já encontrou, segue ele, e podem
    //virar personagens jogáveis também.
    public List<SurvivorController> SurvivorGroup { get => survivorGroup; private set => survivorGroup = value; }

    //Os zumbis que estão perseguindo o sobrevivente e seu grupo.
    public List<ZombieController> HuntedBy { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SurvivorGroup = new();
        if (!SurvivorGroup.Contains(this))
        {
            SurvivorGroup.Add(this);
        }

        HuntedBy = new();
    }

    //Use esses códigos para alterar e retornar o estado atual do Sobrevivente.
    public void SetState(SurvivorState newState)
    {
        if (currentState == newState) return;

        StopAllCoroutines();
        FullStop();
        currentState = newState;

        switch (newState)
        {
            case SurvivorState.WANDER:
                hiddenRenderer.material = nonHiddenMaterial;
                Agent.speed = 5f;
                Agent.avoidancePriority = 40;
                break;
            case SurvivorState.RESCUE:
                hiddenRenderer.material = hiddenMaterial;
                StartCoroutine(InitialRoutine());
                break;
            case SurvivorState.HIDE:
                hiddenRenderer.material = hiddenMaterial;
                break;
            case SurvivorState.FOLLOW:
                hiddenRenderer.material = nonHiddenMaterial;
                Agent.speed = 4f;
                Agent.avoidancePriority = 50;
                StopAllCoroutines();
                StartCoroutine(FollowRoutine());
                break;
            case SurvivorState.DEATH:
                hiddenRenderer.material = nonHiddenMaterial;
                break;
        }
    }
    public SurvivorState GetState()
    {
        return currentState;
    }

    //Essa função é usada quando um Sobrevivente está se mexendo, garantindo
    //que ele sempre entre no modo Wander (ou Follow) quando se movimenta.
    public void SetSurvivorDestination(Vector3 destination)
    {
        if (GetState() == SurvivorState.INITIAL)
        {
            SetState(SurvivorState.WANDER);
        }

        if (currentHidingSpot)
        {
            currentHidingSpot.ExitHidingSpot();
        }
        Agent.SetDestination(destination);
    }

    //Essa função detecta algum ponto de esconderijo próximo ao Sobrevivente.
    public HidingSpot DetectNearbyHidingSpot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f);

        return colliders.FirstOrDefault(collider => collider.GetComponent<HidingSpot>())?.GetComponent<HidingSpot>();
    }

    public void SetHidingSpot(HidingSpot newHidingSpot)
    {
        currentHidingSpot = newHidingSpot;
    }
    public HidingSpot GetHidingSpot()
    {
        return currentHidingSpot;
    }

    //Use esses códigos para alterar e retornar a arma atual do Sobrevivente.
    public void SetWeapon(Weapon weapon)
    {
        selectedWeapon = weapon;
    }
    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }

    //Essa rotina é usada por sobreviventes não resgatados.
    //O sobrevivente ficará parado até o jogador chegar perto dele.
    //A partir daí o sobrevivente começa a seguir o jogador.
    private IEnumerator InitialRoutine()
    {
        Vector3 origin;
        Vector3 destination;
        Vector3 direction;
        RaycastHit[] hits;

        bool wallHit;
        bool playerHit;
        bool isDetecting = false;
        while (!isDetecting)
        {
            origin = new Vector3(transform.position.x, 1, transform.position.z);
            destination = new Vector3(PlayerCharacter.transform.position.x, 1, PlayerCharacter.transform.position.z);
            direction = destination - origin;
            hits = Physics.RaycastAll(origin, direction, 5f);

            wallHit = false;
            playerHit = false;

            Debug.DrawRay(origin, direction.normalized * 5f, Color.red);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("StaticScenery"))
                {
                    wallHit = true;
                    break;
                }
            }
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == PlayerCharacter.GetComponent<Collider>())
                {
                    playerHit = true;
                    break;
                }
            }
            if (!wallHit && playerHit)
            {
                isDetecting = true;
            }

            yield return new WaitForEndOfFrame();
        }

        LevelController.Instance.AddToRescuedSurvivors();
        PlayerCharacter.AddToSurvivorGroup(this, true);
        currentHidingSpot.ExitHidingSpot();
    }

    //Essas funções adiciona/remove um novo sobrevivente ao grupo, assegurando recursivamente
    //que este seja adicionado/removido também ao grupo de outros grupos.
    public void AddToSurvivorGroup(SurvivorController survivor, bool recursiveCheck)
    {
        if (!SurvivorGroup.Contains(survivor))
        {
            SurvivorGroup.Add(survivor);
        }

        if (recursiveCheck)
        {
            foreach (SurvivorController survivorController in SurvivorGroup)
            {
                foreach (SurvivorController controller in SurvivorGroup)
                {
                    survivorController.AddToSurvivorGroup(controller, false);
                }
            }
        }
    }
    public void RemoveFromSurvivorGroup(SurvivorController survivor, bool recursiveCheck)
    {
        if (SurvivorGroup.Contains(survivor))
        {
            SurvivorGroup.Remove(survivor);
        }

        if (recursiveCheck)
        {
            foreach (SurvivorController survivorController in SurvivorGroup)
            {
                    survivorController.RemoveFromSurvivorGroup(survivor, false);
            }
        }
    }

    //Esta rotina guia o sobrevivente seguindo o jogador, fazendo ele parar de se mexer
    //ao chegar relativamente perto do personajem jogável.
    private IEnumerator FollowRoutine()
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, PlayerCharacter.transform.position) > 3.5f)
            {
                SetSurvivorDestination(PlayerCharacter.transform.position);
            }
            else
            {
                if (Agent.velocity != Vector3.zero)
                {
                    Agent.ResetPath();
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void AddToHuntedList(ZombieController zombie)
    {
        if (!HuntedBy.Contains(zombie))
        {
            HuntedBy.Add(zombie);
        }
    }

    protected override void DeathBehaviour()
    {
        WarningText.Instance.SetWarningText("Um sobrevivente morreu!");
        RemoveFromSurvivorGroup(this, true);
        SetState(SurvivorState.DEATH);
    }

    //Essa função assegura que a mudança de estado e material jogador ocorra de forma correta.
    public void SetPlayerCharacter(SurvivorController newPlayer)
    {
        PlayerCharacter = newPlayer;
        SurvivorState currentState = GetState();

        if (PlayerCharacter == this)
        {
            GetComponentInChildren<Renderer>().material = playerMaterial;
            if (currentState == SurvivorState.FOLLOW)
            {
                SetState(SurvivorState.WANDER);
            }
        }
        else
        {
            GetComponentInChildren<Renderer>().material = survivorMaterial;
            if (currentState == SurvivorState.WANDER)
            {
                SetState(SurvivorState.FOLLOW);
            }
        }
    }
}