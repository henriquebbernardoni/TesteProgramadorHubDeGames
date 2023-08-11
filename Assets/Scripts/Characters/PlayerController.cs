using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//Esse script controla tudo relacionado aos movimentos do jogador,
//incluindo movimentação, ataques, esconderijos e troca de PJ.
public class PlayerController : MonoBehaviour
{
    private GameController gameController;
    private InventoryController inventoryController;
    [SerializeField] private SurvivorController playerCharacter;

    [SerializeField] private TextMeshProUGUI actionDescription;

    private Transform[] endingPoints;
    private int currentEndingPoint = 0;

    public SurvivorController PlayerCharacter { get => playerCharacter; private set => playerCharacter = value; }

    private void Awake()
    {
        inventoryController = GetComponent<InventoryController>();
        gameController = GetComponent<GameController>();
        endingPoints = GameObject.Find("EndingFloor").
            transform.Find("Points").transform.Cast<Transform>().ToArray();
    }

    private void Update()
    {
        DescribeAction();
        if (Input.GetMouseButtonDown(0))
        {
            LeftButtonClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RightButtonClick();
        }
    }

    private void DescribeAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                actionDescription.text = "Ir Aqui";
            }
            else if (hit.collider.GetComponent<HidingSpot>())
            {
                actionDescription.text = "Esconder-se";
            }
            else if (hit.collider.GetComponent<Item>())
            {
                actionDescription.text = "Pegar " + hit.collider.GetComponent<Item>().ObjectName;
            }
            else if (hit.collider.GetComponent<ZombieController>())
            {
                actionDescription.text = "Atacar!";
            }
            else if (hit.collider.CompareTag("Final"))
            {
                actionDescription.text = "Fugir!";
            }
            else if (hit.collider.GetComponent<SurvivorController>() &&
                     hit.collider != PlayerCharacter.GetComponent<Collider>())
            {
                actionDescription.text = "Trocar de personagem";
            }
            else
            {
                actionDescription.text = string.Empty;
            }
        }
    }

    private void RightButtonClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                StopAllCoroutines();
                StartCoroutine(WalkHere(hit.point));
            }
            else if (hit.collider.GetComponent<HidingSpot>())
            {
                StopAllCoroutines();
                StartCoroutine(HideHere(hit.collider.GetComponent<HidingSpot>()));
            }
            else if (hit.collider.GetComponent<Item>())
            {
                StopAllCoroutines();
                playerCharacter.FullStop();
                StartCoroutine(GetItem(hit.collider.GetComponent<Item>()));
            }
            else if (hit.collider.GetComponent<ZombieController>())
            {
                if (!PlayerCharacter.GetWeapon())
                {
                    WarningText.Instance.SetWarningText("Nenhuma arma selecionada!");
                }
                else if (PlayerCharacter.IsRecharging)
                {
                    WarningText.Instance.SetWarningText("Recarregando!");
                }
                else if (hit.collider.GetComponent<ZombieController>().GetState() == ZombieController.ZombieState.DEATH)
                {
                    WarningText.Instance.SetWarningText("Inimigo já morto!");
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(PlayerCharacter.GetWeapon().
                        WeaponBehaviour(PlayerCharacter, hit.collider.GetComponent<ZombieController>()));
                }
            }
            else if (hit.collider.CompareTag("Final"))
            {
                if (LevelController.Instance.MinSurvivorsRescued())
                {
                    StopAllCoroutines();
                    StartCoroutine(HeadToExit());
                }
                else
                {
                    WarningText.Instance.SetWarningText("Resgate pelo menos 2 Sobreviventes para poder fugir!");
                }
            }
        }
    }

    //A rotina utilizada para mover o jogador,
    //Inclui uma chacagem se no destino há algum esconderijo por perto.
    private IEnumerator WalkHere(Vector3 destination)
    {
        List<SurvivorController> allButPlayer = PlayerCharacter.SurvivorGroup.
            Where(survivor => survivor != PlayerCharacter && survivor.GetHidingSpot()).ToList();
        for (int i = 0; i < allButPlayer.Count; i++)
        {
            allButPlayer[i].GetHidingSpot().ExitHidingSpot();
        }

        PlayerCharacter.StopAllCoroutines();
        PlayerCharacter.SetSurvivorDestination(destination);
        yield return new WaitUntil(() => PlayerCharacter.Agent.hasPath);
        yield return new WaitWhile(() => PlayerCharacter.Agent.hasPath);
        yield return new WaitForEndOfFrame();

        HidingSpot hidingSpot = PlayerCharacter.DetectNearbyHidingSpot();
        if (hidingSpot)
        {
            StartCoroutine(HideHere(hidingSpot));
        }
    }

    //Essa rotina leva o jogador a se esconder no local escolhido.
    //Se eles estiverem mais Sobreviventes no grupo eles também se escondem.
    private IEnumerator HideHere(HidingSpot hidingSpot)
    {
        yield return null;
        playerCharacter.FullStop();
        StartCoroutine(hidingSpot.EnterHidingSpot(playerCharacter));
        if (playerCharacter.SurvivorGroup.Count > 1 && !hidingSpot.IsSurvivorHere())
        {
            List<SurvivorController> allButPlayer = playerCharacter.SurvivorGroup.
                Where(survivor => survivor != playerCharacter).ToList();
            List<HidingSpot> hidingOthers = NearestHidingSpots(hidingSpot, allButPlayer.Count);
            for (int i = 0; i < hidingOthers.Count; i++)
            {
                StartCoroutine(hidingOthers[i].EnterHidingSpot(allButPlayer[i]));
            }
        }
    }

    //Os X esconderijos mais próximos do jogador. Esse dado é usado pra esconder os sobreviventes.
    public List<HidingSpot> NearestHidingSpots(HidingSpot excludedHidingSpot, int quantity)
    {
        List<HidingSpot> allHidingSpots = gameController.HidingSpots.
            Where(spot => spot != excludedHidingSpot && !spot.IsSurvivorHere()).ToList();
        List<HidingSpot> returnedSpots = new();
        //allHidingSpots.Remove(excludedHidingSpot);
        //allHidingSpots.RemoveAll(spot => spot.IsSurvivorHere());
        allHidingSpots = allHidingSpots.OrderBy(spot =>
            GetPathLength(excludedHidingSpot.transform.position, spot.transform.position)).ToList();
        for (int i = 0; i < quantity; i++)
        {
            returnedSpots.Add(allHidingSpots[i]);
        }
        return returnedSpots;
    }

    private float GetPathLength(Vector3 origin, Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(origin, destination, NavMesh.AllAreas, path);

        float length = 0f;
        Vector3 previousCorner = path.corners[0];

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 currentCorner = path.corners[i];
            length += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
        }

        return length;
    }

    //Essa rotina é utilizada para pegar algum item.
    private IEnumerator GetItem(Item item)
    {
        playerCharacter.SetSurvivorDestination(item.transform.position);
        yield return new WaitUntil(() => playerCharacter.Agent.hasPath);
        yield return new WaitWhile(() => playerCharacter.Agent.remainingDistance >= 1f);
        playerCharacter.FullStop();
        yield return new WaitForEndOfFrame();

        inventoryController.AddItemToInventory(item);
        item.GetComponent<MeshRenderer>().enabled = false;
        item.GetComponent<Collider>().enabled = false;
    }

    private IEnumerator HeadToExit()
    {
        StartCoroutine(WalkHere(endingPoints[currentEndingPoint].position));
        yield return new WaitUntil(() => PlayerCharacter.Agent.hasPath);
        yield return new WaitWhile(() => PlayerCharacter.Agent.hasPath);
        yield return new WaitForEndOfFrame();

        currentEndingPoint++;
        if (currentEndingPoint >= endingPoints.Length)
        {
            currentEndingPoint = 0;
        }
        PlayerCharacter.SetState(SurvivorController.SurvivorState.FINAL);

        SurvivorController survivor;
        if (PlayerCharacter.SurvivorGroup.Count > 1)
        {
            List<SurvivorController> allButPlayer = playerCharacter.SurvivorGroup.
                Where(survivor => survivor != playerCharacter).ToList();
            survivor = allButPlayer[Random.Range(0, allButPlayer.Count)];
            PlayerCharacter.RemoveFromSurvivorGroup(PlayerCharacter, true);
            PlayerCharacter.SurvivorGroup.Clear();
            SetPlayerCharacter(survivor);
            StartCoroutine(HeadToExit());
        }
        else if (gameController.Survivors.Any(x => x.GetState() == SurvivorController.SurvivorState.INITIAL))
        {
            survivor = gameController.Survivors.
                Where(x => x.GetState() == SurvivorController.SurvivorState.INITIAL)
                .FirstOrDefault();
            PlayerCharacter.RemoveFromSurvivorGroup(PlayerCharacter, true);
            PlayerCharacter.SurvivorGroup.Clear();
            SetPlayerCharacter(survivor);
        }
        else
        {
            LevelController.Instance.NextLevelSurvivorsRescued();
        }
    }

    private void LeftButtonClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.GetComponent<SurvivorController>() &&
                hit.collider != playerCharacter.GetComponent<Collider>())
            {
                if (!playerCharacter.SurvivorGroup.Contains(hit.collider.GetComponent<SurvivorController>()))
                {
                    WarningText.Instance.SetWarningText("Esse sobrevivente não faz parte do seu grupo!" +
                        "\nChegue perto de para acrescentá-lo ao seu grupo.");
                }
                else
                {
                    SetPlayerCharacter(hit.collider.GetComponent<SurvivorController>());
                }
            }
        }
    }

    //Esse é o método principal para trocar o jogador personagem.
    public void SetPlayerCharacter(SurvivorController newPlayer)
    {
        foreach (SurvivorController survivor in gameController.Survivors)
        {
            survivor.SetPlayerCharacter(newPlayer);
        }

        foreach (ZombieController zombie in gameController.Zombies)
        {
            zombie.SetPlayerCharacter(newPlayer);
        }

        PlayerCharacter = newPlayer;
        GetComponent<InventoryController>().SetPlayerCharacter(PlayerCharacter);
    }
}