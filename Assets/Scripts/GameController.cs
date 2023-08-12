using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

//Controlador geral do jogo aqui estão guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    //Prefabs de GameObjects a serem usados no set-up do nível.
    [SerializeField] private GameObject[] sceneryPrefabs;
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private GameObject medicinePrefab;
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private GameObject gunPrefab;
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject obstaclePrefab;

    //Listagem de GameObjects em cena que serão diretamente afetados.
    private NavMeshSurface surface;
    private GameObject selectedScenery;
    private Transform[] hidingSpotSpawnPoints;
    private List<GameObject> obstacles;
    public static List<SurvivorController> Survivors { get; private set; }
    public static List<ZombieController> Zombies { get; private set; }
    public static List<Item> Items { get; private set; }
    public static List<HidingSpot> HidingSpots { get; private set; }

    //Objetos relacionados às plataformas de inícoo e fim do jogo.
    [SerializeField] private GameObject startingFloor;
    [SerializeField] private GameObject endingFloor;
    public static Transform[] StartingPoints { get; private set; }
    public static Transform[] EndingPoints { get; private set; }

    private float mapSize = 50f;

    public static int SurvivorsAdded { get; private set; }

    //Nesse evento, funcões relacionadas ao set-up que vem de fora do script são inscritas.
    public static event Action OnLevelSetUp;

    private void Awake()
    {
        surface = FindObjectOfType<NavMeshSurface>();

        StartingPoints = startingFloor.transform.Find("Points").transform.Cast<Transform>().ToArray();
        EndingPoints = endingFloor.transform.Find("Points").transform.Cast<Transform>().ToArray();

        obstacles = new List<GameObject>();
        Survivors = new List<SurvivorController>();
        Zombies = new List<ZombieController>();
        Items = new List<Item>();
        HidingSpots = new List<HidingSpot>();
    }

    private void Start()
    {
        LevelLoad(3, 2, 1, 1);
    }

    //FUNÇÕES E ROTINAS RELACIONADAS A SET-UP DE NÍVEL - INÍCIO

    //Nessa função tudo que precisa ser realizado para o setup do nível, na ordem correta.
    public void LevelLoad(int survivorsAmount, int zombiesAmount, int woodAmount, int gunAmount)
    {
        SurvivorsAdded = survivorsAmount;

        StoreObjectsOnScene();

        ClearObjects();

        SetUpScenario(survivorsAmount);

        SetUpRescued(survivorsAmount);

        SetUpCollectables(woodAmount, gunAmount, survivorsAmount);

        SetUpZombies(zombiesAmount);

        OnLevelSetUp?.Invoke();
    }

    //Objetos que serão mantidos entre uma cena e outra são guardados aqui. 
    //O que não se é organizado é deletado da cena.
    private void StoreObjectsOnScene()
    {
        StoreSurvivors();

        StoreItems();
    }

    //Os sobreviventes que foram resgatados e conseguiram chegar ao final são guardados.
    private void StoreSurvivors()
    {
        Survivors = FindObjectsOfType<SurvivorController>()
            .Where(survivor => survivor.GetState() == SurvivorController.SurvivorState.FINAL
                            || survivor.GetState() == SurvivorController.SurvivorState.INITIAL)
            .ToList();
    }

    //Os itens que estavam no inventário quando o jogador passa de fase são mantidos.
    private void StoreItems()
    {
        Items = FindObjectsOfType<Item>()
            .Where(item => InventoryController.InventoryItems.Contains(item))
            .ToList();
    }

    //Objetos que não se mantém na cena são excluído para novos sendo instanciados.
    private void ClearObjects()
    {
        if (selectedScenery)
        {
            Destroy(selectedScenery);
        }

        foreach (HidingSpot hidingSpot in HidingSpots)
        {
            Destroy(hidingSpot.gameObject);
        }

        foreach (GameObject obstable in obstacles)
        {
            Destroy(obstable);
        }

        foreach (SurvivorController survivor in FindObjectsOfType<SurvivorController>())
        {
            if (!Survivors.Contains(survivor))
            {
                Destroy(survivor.gameObject);
            }
        }

        foreach (ZombieController zombie in Zombies)
        {
            Destroy(zombie.gameObject);
        }

        foreach (Item item in FindObjectsOfType<Item>())
        {
            if (!Items.Contains(item))
            {
                Destroy(item.gameObject);
            }
        }
    }

    //O cenário a ser adicionado no mapa tem valores proporcionais
    //a quantidade de sobrevivente que podem ser resgatados no nível.
    private void SetUpScenario(int survivorsAmount)
    {
        //Um dentre os possíveis setups de cenário é adicionado na cena.
        selectedScenery = Instantiate(sceneryPrefabs[Random.Range(0, sceneryPrefabs.Length)]);

        //Os esconderijos são intanciados e tem suas posições randomizadas.
        HidingSpots.Clear();
        for (int i = 0; i < (2 * (Survivors.Count + survivorsAmount)); i++)
        {
            HidingSpots.Add(Instantiate(hidingSpotPrefab).GetComponent<HidingSpot>());
        }
        RandomizeHidingSpots();

        //Os obstáculos são intanciados e tem suas posições randomizadas.
        Vector3 point;
        GameObject obstacle;
        obstacles.Clear();
        float halfMapSize = (mapSize - 5) / 2;
        for (int i = 0; i < (survivorsAmount * 3); i++)
        {
            obstacle = Instantiate(obstaclePrefab);
            point = GetRandomPosition(halfMapSize, obstacle.transform.position.y);
            obstacle.transform.position = point;
            obstacles.Add(obstacle);
        }

        //Dada a natureza flexível das fases, é necessário realizar
        //o Bake do NavMesh, sempre que uma fase se inicia,
        //logo após os itens de cenário serem instanciados e postos.
        surface.BuildNavMesh();
    }

    //Os locais e rotações dos esconderijos são randomizados,
    //assegurando que eles não fiquem muito perto uns dos outros.
    private void RandomizeHidingSpots()
    {
        hidingSpotSpawnPoints = selectedScenery
            .transform
            .Find("HidingPoints")
            .GetComponentsInChildren<Transform>();
        int[] positions = GenerateRandomNumbers(HidingSpots.Count, hidingSpotSpawnPoints.Length);

        for (int i = 0; i < HidingSpots.Count; i++)
        {
            HidingSpots[i].transform
                .SetPositionAndRotation
                (hidingSpotSpawnPoints[positions[i]].position,
                hidingSpotSpawnPoints[positions[i]].rotation);
        }
    }

    //Essa função gera um número aleatório de ints a serem usadas para
    //se garantir uma randomização boa no posicionamento de algo.
    private static int[] GenerateRandomNumbers(int count, int max)
    {
        List<int> numbers = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int randomNumber = Random.Range(0, max);
            while (numbers.Contains(randomNumber))
            {
                randomNumber = Random.Range(0, max);
            }
            numbers.Add(randomNumber);
        }

        return numbers.ToArray();
    }

    //Os sobreviventes a serem resgatados (e os já resgatados em outras fases) são organizados.
    private void SetUpRescued(int survivorsAmount)
    {
        //Os sobreviventes já resgatados são colocados na área inicial.
        for (int i = 0; i < Survivors.Count; i++)
        {
            Survivors[i].SetState(SurvivorController.SurvivorState.INITIAL);
            Survivors[i].Agent.Warp(StartingPoints[i].position);
        }

        //Os sobreviventos a serem resgatados são instanciados no mapa.
        SurvivorController newSurvivor;
        for (int i = 0; i < survivorsAmount; i++)
        {
            newSurvivor = Instantiate(survivorPrefab).GetComponent<SurvivorController>();
            Survivors.Add(newSurvivor);
        }

        //O primeiro sobrevivente resgatado é declarado Personagem Jogador.
        SurvivorController initialPlayer = Survivors.Find
            (survivor => survivor.GetState() == SurvivorController.SurvivorState.INITIAL);
        PlayerController.SetPlayerCharacter(initialPlayer);
        CameraController.CenterCameraOnPlayer();

        //Os esconderijos onde os resgates inicialmente estão são randomizados
        //e tem suas vidas organizadas para o valor inicial.
        RandomizeRescuedStart();
    }

    //Essa função aleatoriza as posições iniciais dos Sobreviventes a serem resgatados.
    private void RandomizeRescuedStart()
    {
        SurvivorController[] rescuees = Survivors
            .Where(survivor => survivor
            .GetState() != SurvivorController.SurvivorState.INITIAL)
            .ToArray();
        int[] positions = GenerateRandomNumbers(rescuees.Length, HidingSpots.Count);

        for (int i = 0; i < rescuees.Length; i++)
        {
            rescuees[i].Agent.Warp(HidingSpots[positions[i]].transform.position);
            HidingSpots[positions[i]].SetHidingSpot(rescuees[i]);
            rescuees[i].SetState(SurvivorController.SurvivorState.RESCUE);
        }
    }

    //Os colecionáveis são adicionados ao mapa.
    private void SetUpCollectables(int woodAmount, int gunAmount, int survivorsAmount)
    {
        Vector3 point;
        List<Item> itemsToBeAdded = new List<Item>();

        int supplyCount = (Survivors.Count / 2) + 2;
        float medicineAmount = survivorsAmount * 1.5f;
        float halfMapSize = (mapSize - 5) / 2;

        //Os mantimentos são adicionados ao mapa.
        //A quantidade instanciada é o necessário para o time mais 2).
        for (int i = 0; i < supplyCount; i++)
        {
            Supply supply = Instantiate(supplyPrefab).GetComponent<Supply>();
            itemsToBeAdded.Add(supply);

            point = GetRandomPosition(halfMapSize, supply.transform.position.y);
            supply.transform.position = point;
        }


        //Os remédios são adicionados ao mapa.
        for (int i = 0; i < medicineAmount; i++)
        {
            Medicine medicine = Instantiate(medicinePrefab).GetComponent<Medicine>();
            itemsToBeAdded.Add(medicine);

            point = GetRandomPosition(halfMapSize, medicine.transform.position.y);
            medicine.transform.position = point;
        }

        //As armas são finalmente adicionadas a fase.
        for (int i = 0; i < woodAmount; i++)
        {
            PieceOfWood wood = Instantiate(woodPrefab).GetComponent<PieceOfWood>();
            itemsToBeAdded.Add(wood);

            point = GetRandomPosition(halfMapSize, wood.transform.position.y);
            wood.transform.position = point;
        }

        for (int i = 0; i < gunAmount; i++)
        {
            Gun gun = Instantiate(gunPrefab).GetComponent<Gun>();
            itemsToBeAdded.Add(gun);

            point = GetRandomPosition(halfMapSize, gun.transform.position.y);
            gun.transform.position = point;
        }

        Items.AddRange(itemsToBeAdded);
    }

    //Retorna uma posição aleatória no mapa para um objeto ser posto.
    private Vector3 GetRandomPosition(float halfMapSize, float yPosition)
    {
        Vector3 point;

        point = new Vector3
                (Random.Range(-halfMapSize, halfMapSize),
                yPosition,
                Random.Range(-halfMapSize, halfMapSize));

        return point;
    }

    //Os zumbis são instanciados e suas posições iniciais randomizadas.
    private void SetUpZombies(int zombiesAmount)
    {
        List<ZombieController> newZombies = new List<ZombieController>();

        for (int i = 0; i < zombiesAmount; i++)
        {
            newZombies.Add(Instantiate(zombiePrefab).GetComponent<ZombieController>());
        }

        Zombies.Clear();
        Zombies.AddRange(newZombies);

        foreach (ZombieController zombieController in newZombies)
        {
            zombieController.SetState(ZombieController.ZombieState.WANDER);
        }
    }

    //FUNÇÕES E ROTINAS RELACIONADAS A SET-UP DE NÍVEL - FIM
}