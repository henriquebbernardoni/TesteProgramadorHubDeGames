using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static ZombieController;

//Script controlador geral dos Sobrevivente (PJ ou n�o)
//Caso sejam PJ eles ter�o acesso a fun��es mais avan�adas
public class SurvivorController : GenCharacterController
{
    public enum SurvivorState { WANDER, HIDE, INITIAL, FOLLOW, DEATH }
    [SerializeField] private SurvivorState currentState;

    [SerializeField] private Material survivorMaterial;
    [SerializeField] private Material playerMaterial;

    //Essa foi uma maneira que eu encontrei de mostrar que um Sobrevivente est� escondido
    //Caso esse fosse um projeto mais avan�ado, eu implementaria uma solu��o melhor
    [SerializeField] private Renderer hiddenRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material nonHiddenMaterial;
    private HidingSpot currentHidingSpot;

    private Weapon selectedWeapon;

    [SerializeField] private SurvivorController playerCharacter;
    [SerializeField] private List<SurvivorController> survivorGroup;

    //O grupo refere �s pessoas que o jogador j� encontrou, segue ele, e podem
    //virar personagens jog�veis tamb�m.
    public List<SurvivorController> SurvivorGroup { get => survivorGroup; set => survivorGroup = value; }

    private void Start()
    {
        SurvivorGroup = new();
        if (!SurvivorGroup.Contains(this))
        {
            SurvivorGroup.Add(this);
        }
    }

    //Use esses c�digos para alterar e retornar o estado atual do Sobrevivente.
    public void SetState(SurvivorState state)
    {
        currentState = state;

        switch (currentState)
        {
            case SurvivorState.WANDER:
                hiddenRenderer.material = nonHiddenMaterial;
                break;
            case SurvivorState.INITIAL:
                hiddenRenderer.material = hiddenMaterial;
                StartCoroutine(InitialRoutine());
                break;
            case SurvivorState.HIDE:
                hiddenRenderer.material = hiddenMaterial;
                break;
            case SurvivorState.FOLLOW:
                hiddenRenderer.material = nonHiddenMaterial;
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

    //Essa fun��o � usada quando um Sobrevivente est� se mexendo, garantindo
    //que ele sempre entre no modo Wander (ou Follow) quando se movimenta.
    public void SetSurvivorDestination(Vector3 destination)
    {
        if (currentHidingSpot)
        {
            currentHidingSpot.ExitHidingSpot();
        }
        if (playerCharacter == this)
        {
            SetState(SurvivorState.WANDER);
        }
        else
        {
            SetState(SurvivorState.FOLLOW);
        }
        Agent.SetDestination(destination);
    }

    //Essa fun��o detecta algum ponto de esconderijo pr�ximo ao Sobrevivente.
    public HidingSpot DetectNearbyHidingSpot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f);

        return colliders.FirstOrDefault(collider => collider.GetComponent<HidingSpot>())?.GetComponent<HidingSpot>();
    }

    public void SetHidingSpot(HidingSpot newHidingSpot)
    {
        currentHidingSpot = newHidingSpot;
    }

    //Use esses c�digos para alterar e retornar a arma atual do Sobrevivente.
    public void SetWeapon(Weapon weapon)
    {
        selectedWeapon = weapon;
    }
    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }

    //Essa rotina � usada por sobreviventes n�o resgatados.
    //O sobrevivente ficar� parado at� o jogador chegar perto dele.
    //A partir da� o sobrevivente come�a a seguir o jogador.
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
            destination = new Vector3(playerCharacter.transform.position.x, 1, playerCharacter.transform.position.z);
            direction = destination - origin;
            hits = Physics.RaycastAll(origin, direction, 5f);

            wallHit = false;
            playerHit = false;

            Debug.DrawRay(origin, direction * 2.5f, Color.red);

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
                if (hit.collider == playerCharacter.GetComponent<Collider>())
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

        playerCharacter.AddToSurvivorGroup(this, true);
        SetState(SurvivorState.FOLLOW);
    }

    //Essa fun��o adiciona um novo sobrevivente ao grupo, assegurando recursivamente
    //que este seja adicionado tamb�m ao grupo de outros grupos.
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

    private IEnumerator FollowRoutine()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void DeathBehaviour()
    {
        SetState(SurvivorState.DEATH);
    }

    //Essa fun��o assegura que a mudan�a de personagem possa ocorrer de forma correta.
    public void SetPlayerCharacter(SurvivorController newPlayer)
    {
        playerCharacter = newPlayer;

        if (playerCharacter == this)
        {
            GetComponentInChildren<Renderer>().material = playerMaterial;
            SetState(SurvivorState.WANDER);
        }
        else
        {
            GetComponentInChildren<Renderer>().material = survivorMaterial;
            if (GetState() == SurvivorState.WANDER)
            {
                SetState(SurvivorState.FOLLOW);
            }
        }
    }
}