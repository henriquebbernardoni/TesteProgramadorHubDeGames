using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

//Controlador geral do jogo aqui est�o guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    //Prefabs de GameObjects a serem usados no set-up do n�vel.
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private GameObject medicinePrefab;
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private GameObject gunPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject[] sceneryPrefabs;

    private GameObject startingFloor;
    private Transform[] startingPoints;
    private GameObject endingFloor;

    private GameObject selectedScenery;
    private Transform[] hidingSpotSpawnPoints;

    private NavMeshSurface surface;

    //Listagem de GameObjects em cena que ser�o diretamente afetados.
    [SerializeField] private List<SurvivorController> survivors = new();
    [SerializeField] private List<ZombieController> zombies = new();
    [SerializeField] private List<HidingSpot> hidingSpots = new();
    [SerializeField] private List<Item> items = new();
    [SerializeField] private List<GameObject> obstacles = new();

    private CameraController cameraController;
    private PlayerController playerController;
    private InventoryController inventoryController;

    private List<SurvivorController> survivorsOnScene = new();
    private List<Item> itemsOnScene = new();

    private float mapSize = 50f;

    public List<SurvivorController> Survivors { get => survivors; private set => survivors = value; }
    public List<ZombieController> Zombies { get => zombies; private set => zombies = value; }
    public List<HidingSpot> HidingSpots { get => hidingSpots; private set => hidingSpots = value; }
    public List<Item> Items { get => items; private set => items = value; }

    public int SurvivorsAdded { get; private set; }

    private void Awake()
    {
        surface = FindObjectOfType<NavMeshSurface>();
        startingFloor = GameObject.Find("StartingFloor");
        startingPoints = startingFloor.transform.Find("Points").transform.Cast<Transform>().ToArray();
        endingFloor = GameObject.Find("EndingFloor");

        cameraController = FindObjectOfType<CameraController>();
        playerController = GetComponent<PlayerController>();
        inventoryController = FindObjectOfType<InventoryController>();
    }

    private void Start()
    {
        SetUpLevel(2, 2, 1, 1);
    }

    //FUN��ES E ROTINAS RELACIONADSS A SET-UP DE N�VEL - IN�CIO

    //Essa rotina controla o set-up do n�vel como um todo.
    public void SetUpLevel(int survivorsAmount, int zombiesAmount, int woodAmount, int gunAmount)
    {
        SurvivorsAdded = survivorsAmount;

        StoreObjectsOnScene();

        ClearObjects();

        SetUpScenario(survivorsAmount);

        SetUpRescued(survivorsAmount);

        SetUpCollectables(woodAmount, gunAmount, survivorsAmount);

        SetUpZombies(zombiesAmount);
    }

    //Objetos que ser�o mantidos entre uma cena e outra s�o organizados aqui. 
    //O que n�o se � organizado � deletado da cena.
    private void StoreObjectsOnScene()
    {
        StoreSurvivors();

        StoreItems();
    }

    //Os sobreviventes que foram resgatados e conseguiram chegar ao final s�o guardados.
    private void StoreSurvivors()
    {
        Survivors.Clear();
        survivorsOnScene.Clear();
        List<SurvivorController> previousSurvivors =
            FindObjectsByType<SurvivorController>(FindObjectsSortMode.None).ToList();
        previousSurvivors.RemoveAll(survivor => survivor.GetState() == SurvivorController.SurvivorState.RESCUE ||
            survivor.GetState() == SurvivorController.SurvivorState.DEATH);
        foreach (SurvivorController survivorController in previousSurvivors)
        {
            survivorsOnScene.Add(survivorController);
            survivors.Add(survivorController);
        }
    }

    //Os itens que estavam no invent�rio quando o jogador passa de fase s�o mantidos.
    private void StoreItems()
    {
        itemsOnScene.Clear();
        List<Item> previousItems =
            FindObjectsByType<Item>(FindObjectsSortMode.None).ToList();
        foreach (Item item in previousItems)
        {
            if (inventoryController.InventoryItems.Contains(item))
            {
                itemsOnScene.Add(item);
            }
        }
    }

    //Objetos que n�o se mant�m na cena s�o exclu�do para novos sendo instanciados.
    private void ClearObjects()
    {
        if (selectedScenery)
        {
            Destroy(selectedScenery.gameObject);
        }

        foreach (HidingSpot hidingSpot in HidingSpots)
        {
            Destroy(hidingSpot.gameObject);
        }

        foreach (GameObject obstable in obstacles)
        {
            Destroy(obstable.gameObject);
        }

        SurvivorController[] survivorsTBD = Survivors.Where(x => x.GetState() !=
            SurvivorController.SurvivorState.FINAL && x.GetState() !=
            SurvivorController.SurvivorState.INITIAL).ToArray();
        foreach (SurvivorController survivor in survivorsTBD)
        {
            Debug.Log(survivor.name);
            Destroy(survivor.gameObject);
        }

        foreach (Item item in Items)
        {
            if (!inventoryController.InventoryItems.Contains(item))
            {
                Destroy(item.gameObject);
            }
        }

        foreach (ZombieController zombie in Zombies)
        {
            Destroy(zombie.gameObject);
        }
    }

    //O cen�rio a ser adicionado no mapa tem valores proporcionais
    //a quantidade de sobrevivente que podem ser resgatados no n�vel.
    private void SetUpScenario(int survivorsAmount)
    {
        //Um dentre os poss�veis setups de cen�rio � adicionado na cena.
        selectedScenery = Instantiate(sceneryPrefabs[Random.Range(0, sceneryPrefabs.Length)]);

        //Os esconderijos s�o intanciados e tem suas posi��es randomizadas.
        HidingSpots.Clear();
        for (int i = 0; i < (2f * (survivorsOnScene.Count + survivorsAmount)); i++)
        {
            HidingSpots.Add(Instantiate(hidingSpotPrefab).GetComponent<HidingSpot>());
        }
        RandomizeHidingSpots();

        //Os obst�culos s�o intanciados e tem suas posi��es randomizadas.
        Vector3 point;
        obstacles.Clear();
        for (int i = 0; i < (survivorsAmount * 3); i++)
        {
            obstacles.Add(Instantiate(obstaclePrefab));

            point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                obstacles[i].transform.position.y,
                Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            obstacles[i].transform.position = point;
        }

        //Dada a natureza flex�vel das fases, � necess�rio realizar
        //o Bake do NavMesh, sempre que uma fase se inicia,
        //logo ap�s os itens de cen�rio serem instanciados e postos.
        surface.BuildNavMesh();
    }

    //Os locais e rota��es dos esconderijos s�o randomizados,
    //assegurando que eles n�o fiquem muito perto uns dos outros.
    private void RandomizeHidingSpots()
    {
        hidingSpotSpawnPoints = 
            selectedScenery.transform.Find("HidingPoints").transform.Cast<Transform>().ToArray();
        int[] positions = GenerateRandomNumbers(HidingSpots.Count, hidingSpotSpawnPoints.Length);

        for (int i = 0; i < HidingSpots.Count; i++)
        {
            HidingSpots[i].transform.SetPositionAndRotation
                (hidingSpotSpawnPoints[positions[i]].position,
                hidingSpotSpawnPoints[positions[i]].rotation);
        }
    }

    //Os sobreviventes a serem resgatados (e os j� resgatados em outras fases) s�o organizados.
    private void SetUpRescued(int rescuedAmount)
    {
        Survivors.Clear();
        //Os sobreviventos a serem resgatados s�o instanciados no mapa.
        for (int i = 0; i < rescuedAmount; i++)
        {
            Survivors.Add(Instantiate(survivorPrefab).GetComponent<SurvivorController>());
        }

        //Os sobreviventes j� resgatados s�o colocados na �rea inicial.
        //O primeiro sobrevivente resgatado � declarado Personagem Jogador.
        //Os resgatados s�o ent�o teleportado para a posi��o inicial.
        for (int i = 0; i < survivorsOnScene.Count; i++)
        {
            survivorsOnScene[i].SetState(SurvivorController.SurvivorState.INITIAL);
            survivorsOnScene[i].Agent.Warp(startingPoints[i].position);
            Survivors.Add(survivorsOnScene[i]);
        }
        SurvivorController initialPlayer = survivorsOnScene.FirstOrDefault();
        playerController.SetPlayerCharacter(initialPlayer);
        cameraController.SetCustomPosition(initialPlayer.transform.position);

        //Os esconderijos onde os resgates inicialmente est�o s�o randomizados
        //e tem suas vidas organizadas para o valor inicial.
        RandomizeSurvivorsStart();
    }

    //Essa fun��o aleatoriza as posi��es iniciais dos Sobreviventes a serem resgatados.
    private void RandomizeSurvivorsStart()
    {
        SurvivorController[] rescuees =
            Survivors.Where(survivor => survivor.
                GetState() != SurvivorController.SurvivorState.INITIAL).ToArray();
        int[] positions = GenerateRandomNumbers(rescuees.Length, HidingSpots.Count);

        for (int i = 0; i < rescuees.Length; i++)
        {
            rescuees[i].Agent.Warp(HidingSpots[positions[i]].transform.position);
            HidingSpots[positions[i]].SetHidingSpot(rescuees[i]);
            rescuees[i].SetState(SurvivorController.SurvivorState.RESCUE);
        }
    }

    //Essa fun��o gera um n�mero aleat�rio de ints a serem usadas para
    //se escolher onde os sobreviventes a serem resgatados ir�o se esconder.
    //Isso visa gerar uma aleatoriza��o maior de poss�veis in�cios.
    private static int[] GenerateRandomNumbers(int count, int max)
    {
        List<int> numbers = new();

        for (int i = 0; i < count; i++)
        {
            numbers.Add(Random.Range(0, max));
            while (numbers.Where(x => x == numbers[i]).ToList().Count > 1)
            {
                numbers[i] = Random.Range(0, max);
            }
        }

        return numbers.ToArray();
    }

    //Os colecion�veis s�o adicionados ao mapa.
    private void SetUpCollectables(int woodAmount, int gunAmount, int survivorsAmount)
    {
        Vector3 point;
        List<Item> itemsToBeAdded = new();

        //Os mantimentos s�o adicionados ao mapa.
        //A quantidade instanciada � o necess�rio para o time mais 2).
        for (int i = 0; i < ((Survivors.Count / 2) + 2); i++)
        {
            itemsToBeAdded.Add(Instantiate(supplyPrefab).GetComponent<Supply>());

            point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                itemsToBeAdded[i].transform.position.y,
                Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            while (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            {
                point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                    itemsToBeAdded[i].transform.position.y,
                    Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));
            }

            itemsToBeAdded[i].transform.position = point;
        }
        Items.AddRange(itemsToBeAdded);
        itemsToBeAdded.Clear();

        //Os rem�dios s�o adicionados ao mapa.
        for (int i = 0; i < survivorsAmount * 1.5f; i++)
        {
            itemsToBeAdded.Add(Instantiate(medicinePrefab).GetComponent<Medicine>());

            point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
               itemsToBeAdded[i].transform.position.y,
               Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            while (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            {
                point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                    Items[i].transform.position.y,
                    Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));
            }

            itemsToBeAdded[i].transform.position = point;
        }
        Items.AddRange(itemsToBeAdded);
        itemsToBeAdded.Clear();

        //As armas s�o finalmente adicionadas a fase.
        for (int i = 0; i < woodAmount; i++)
        {
            itemsToBeAdded.Add(Instantiate(woodPrefab).GetComponent<PieceOfWood>());

            point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
               itemsToBeAdded[i].transform.position.y,
               Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            while (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            {
                point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                    Items[i].transform.position.y,
                    Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));
            }

            itemsToBeAdded[i].transform.position = point;
        }
        Items.AddRange(itemsToBeAdded);
        itemsToBeAdded.Clear();
        for (int i = 0; i < gunAmount; i++)
        {
            itemsToBeAdded.Add(Instantiate(gunPrefab).GetComponent<Gun>());

            point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
               itemsToBeAdded[i].transform.position.y,
               Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            while (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            {
                point = new(Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2),
                    Items[i].transform.position.y,
                    Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));
            }

            itemsToBeAdded[i].transform.position = point;
        }
        Items.AddRange(itemsToBeAdded);
        itemsToBeAdded.Clear();
    }

    //Os zumbis s�o instanciados e suas posi��es iniciais randomizadas.
    private void SetUpZombies(int zombiesAmount)
    {
        Zombies.Clear();
        for (int i = 0; i < zombiesAmount; i++)
        {
            Zombies.Add(Instantiate(zombiePrefab).GetComponent<ZombieController>());
        }

        foreach (ZombieController zombieController in Zombies)
        {
            zombieController.FindWanderPoints();
            zombieController.SetState(ZombieController.ZombieState.WANDER);
            zombieController.SetPlayerCharacter(playerController.PlayerCharacter);
        }
    }

    //FUN��ES E ROTINAS RELACIONADSS A SET-UP DE N�VEL - FIM
}