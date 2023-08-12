using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//Script controlador geral dos Sobrevivente (PJ ou não)
//Caso sejam PJ eles terão acesso a funções mais avançadas
public class SurvivorController : GenCharacterController
{
    public enum SurvivorState { INITIAL, WANDER, RESCUE, FOLLOW, HIDE, FINAL, DEATH }
    [SerializeField] private SurvivorState currentState;

    //Esses materiais mostram qual o jogador atual.
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material survivorMaterial;
    [SerializeField] private Material playerMaterial;

    //Objetos relacionados a esconderijos.
    //O rosto do Sobrevivente muda de cor se estiver escondido.
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;
    public HidingSpot CurrentHidingSpot { get; private set; }

    //A arma que o jogador está segurando agora.
    private Weapon selectedWeapon;

    //Os zumbis que estão perseguindo o jogador e seu grupo.
    public List<ZombieController> HuntedBy { get; private set; }

    //O grupo refere às pessoas que o jogador já encontrou, segue ele,
    //e podemvirar personagens jogáveis também.
    public List<SurvivorController> SurvivorGroup { get; private set; }

    private void OnEnable()
    {
        PlayerController.OnPlayerCharacterChanged += SetPlayerCharacter;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerCharacterChanged -= SetPlayerCharacter;
    }

    protected override void Awake()
    {
        base.Awake();

        SurvivorGroup = new List<SurvivorController>();
        if (!SurvivorGroup.Contains(this))
        {
            SurvivorGroup.Add(this);
        }

        HuntedBy = new List<ZombieController>();
    }

    //Use essas funções para alterar e retornar o estado atual do Sobrevivente.
    public void SetState(SurvivorState newState)
    {
        if (currentState == newState) return;
        StopAllCoroutines();
        FullStop();
        currentState = newState;

        switch (newState)
        {
            case SurvivorState.INITIAL:
                break;
            case SurvivorState.WANDER:
                hiddenRenderer.material = nonHiddenMaterial;
                Agent.speed = 5f;
                break;
            case SurvivorState.RESCUE:
                hiddenRenderer.material = hiddenMaterial;
                StartCoroutine(InitialRoutine());
                break;
            case SurvivorState.FOLLOW:
                hiddenRenderer.material = nonHiddenMaterial;
                Agent.speed = 4f;
                StopAllCoroutines();
                StartCoroutine(FollowRoutine());
                break;
            case SurvivorState.HIDE:
                hiddenRenderer.material = hiddenMaterial;
                break;
            case SurvivorState.FINAL:
                break;
            case SurvivorState.DEATH:
                hiddenRenderer.material = nonHiddenMaterial;
                if (PlayerController.PC == this)
                {
                    SurvivorController initialSurvivor = GameController.Survivors
                    .FirstOrDefault(x => x.GetState() == SurvivorState.INITIAL);
                    if (initialSurvivor != null)
                    {
                        PlayerController.SetPlayerCharacter(initialSurvivor);
                    }
                    else
                    {
                        LevelController.Instance.DisplayDeathScreen();
                    }
                }
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

        if (CurrentHidingSpot)
        {
            CurrentHidingSpot.ExitHidingSpot();
        }
        Agent.SetDestination(destination);
    }

    //Essa rotina é usada por sobreviventes não resgatados.
    //O sobrevivente ficará parado até o jogador chegar perto dele.
    //A partir daí o sobrevivente começa a seguir o jogador.
    private IEnumerator InitialRoutine()
    {
        yield return null;

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
            destination = new Vector3
                (PlayerController.PC.transform.position.x,
                1,
                PlayerController.PC.transform.position.z);
            direction = destination - origin;
            hits = Physics.RaycastAll(origin, direction, 5f);

            wallHit = false;
            playerHit = false;

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
                if (hit.collider == PlayerController.PC.GetComponent<Collider>())
                {
                    playerHit = true;
                    break;
                }
            }
            if (!wallHit && playerHit)
            {
                isDetecting = true;
            }

            yield return null;
        }

        //LevelController.Instance.AddToRescuedSurvivors();
        PlayerController.PC.AddToSurvivorGroup(this, true);
        CurrentHidingSpot.ExitHidingSpot();
    }

    //Essa função detecta algum ponto de esconderijo próximo ao Sobrevivente.
    public HidingSpot DetectNearbyHidingSpot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f);

        return colliders.FirstOrDefault
            (collider => collider.GetComponent<HidingSpot>())?
            .GetComponent<HidingSpot>();
    }

    //Use essas funções para alterar e retornar o esconderijo do Sobrevivente.
    public void SetHidingSpot(HidingSpot newHidingSpot)
    {
        CurrentHidingSpot = newHidingSpot;
    }
    public HidingSpot GetHidingSpot()
    {
        return CurrentHidingSpot;
    }

    //Esta rotina guia o sobrevivente seguindo o jogador, fazendo ele parar de se mexer
    //ao chegar relativamente perto do personajem jogável.
    private IEnumerator FollowRoutine()
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, 
                PlayerController.PC.transform.position) > 3.5f)
            {
                SetSurvivorDestination(PlayerController.PC.transform.position);
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

    //Use esses códigos para alterar e retornar a arma atual do Sobrevivente.
    public void SetWeapon(Weapon weapon)
    {
        selectedWeapon = weapon;
    }
    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }

    //Use essas funções para modificar a lista de zumbis
    //seguindo o jogador e outros sobreviventes.
    public void AddToHuntedList(ZombieController zombie, bool recursiveCheck)
    {
        if (!HuntedBy.Contains(zombie))
        {
            HuntedBy.Add(zombie);
        }

        if (recursiveCheck)
        {
            foreach (SurvivorController survivorController in SurvivorGroup)
            {
                survivorController.AddToHuntedList(zombie, false);
            }
        }
    }
    public void RemoveFromHuntedList(ZombieController zombie, bool recursiveCheck)
    {
        if (HuntedBy.Contains(zombie))
        {
            HuntedBy.Remove(zombie);
        }

        if (recursiveCheck)
        {
            foreach (SurvivorController survivorController in SurvivorGroup)
            {
                survivorController.RemoveFromHuntedList(zombie, false);
            }
        }
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

        SupplyCounter.Instance.UpdateText();
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

        SupplyCounter.Instance.UpdateText();
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
        SurvivorState currentState = GetState();

        if (newPlayer == this)
        {
            bodyRenderer.material = playerMaterial;
            if (currentState == SurvivorState.FOLLOW)
            {
                SetState(SurvivorState.WANDER);
            }
        }
        else
        {
            bodyRenderer.material = survivorMaterial;
            if (currentState == SurvivorState.WANDER)
            {
                SetState(SurvivorState.FOLLOW);
            }
        }
    }
}