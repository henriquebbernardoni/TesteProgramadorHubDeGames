using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

//Controlador geral do jogo aqui estão guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    //Prefabs e GameObjects a serem usados no set-up do nível.
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private GameObject medicinePrefab;
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private GameObject gunPrefab;

    private Transform[] playerSpawnPoints;
    private NavMeshSurface surface;

    //Listagem de GameObjects em cena que serão diretamente afetados.
    [SerializeField] private List<SurvivorController> survivors = new();
    [SerializeField] private List<ZombieController> zombies = new();
    [SerializeField] private List<HidingSpot> hidingSpots = new();
    [SerializeField] private List<Item> items = new();

    private CameraController cameraController;
    private PlayerController playerController;

    private List<SurvivorController> survivorsOnScene = new();
    private List<Item> itemsOnScene = new();

    private float mapSize = 30f;

    public List<SurvivorController> Survivors { get => survivors; private set => survivors = value; }
    public List<ZombieController> Zombies { get => zombies; private set => zombies = value; }
    public List<HidingSpot> HidingSpots { get => hidingSpots; private set => hidingSpots = value; }
    public List<Item> Items { get => items; private set => items = value; }

    private void Awake()
    {
        surface = FindObjectOfType<NavMeshSurface>();
        playerSpawnPoints = GameObject.Find("StartingFloor").transform.Cast<Transform>().ToArray();

        cameraController = FindObjectOfType<CameraController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        SetUpLevel(2, 2, 1, 1);
    }

    //FUNÇÕES E ROTINAS RELACIONADSS A SET-UP DE NÍVEL - INÍCIO

    //Essa rotina controla o set-up do nível como um todo.
    public void SetUpLevel(int survivorsAmount, int zombiesAmount, int woodAmount, int gunAmount)
    {
        StoreObjectsOnScene();

        SetUpScenario(survivorsAmount);

        SetUpRescued(survivorsAmount);

        SetUpCollectables(woodAmount, gunAmount, survivorsAmount);

        SetUpZombies(zombiesAmount);
    }

    //Objetos que serão mantidos entre uma cena e outra são organizados aqui. 
    //O que não se é organizado é deletado da cena.
    private void StoreObjectsOnScene()
    {
        StoreSurvivors();

        StoreItems();
    }

    //Os sobreviventes que foram resgatados e conseguiram chegar ao final são guardados.
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

    //Os itens que estavam no inventário quando o jogador passa de fase são mantidos.
    private void StoreItems()
    {
        itemsOnScene.Clear();
        List<Item> previousItems =
            FindObjectsByType<Item>(FindObjectsSortMode.None).ToList();
        foreach (Item item in previousItems)
        {
            if (GetComponent<InventoryController>().InventoryItems.Contains(item))
            {
                itemsOnScene.Add(item);
            }
        }
    }

    //O cenário a ser adicionado no mapa tem valores proporcionais
    //a quantidade de sobrevivente que podem ser resgatados no nível.
    private void SetUpScenario(int survivorsAmount)
    {
        //Os esconderijos são intanciados e tem suas posições randomizadas.
        for (int i = 0; i < (1.5f * (survivorsOnScene.Count + survivorsAmount)); i++)
        {
            HidingSpots.Add(Instantiate(hidingSpotPrefab).GetComponent<HidingSpot>());
        }
        RandomizeHidingSpots();

        //Dada a natureza flexível das fases, é necessário realizar
        //o Bake do NavMesh, sempre que uma fase se inicia,
        //logo após os itens de cenário serem instanciados e postos.
        surface.BuildNavMesh();
    }

    //Os locais e rotações dos esconderijos são randomizados,
    //assegurando que eles não fiquem muito perto uns dos outros.
    private void RandomizeHidingSpots()
    {
        for (int i = 0; i < HidingSpots.Count; i++)
        {
            HidingSpots[i].transform.position =
                new(Random.Range(-(mapSize-5) / 2, (mapSize - 5) / 2),
                HidingSpots[i].transform.position.y,
                Random.Range(-(mapSize - 5) / 2, (mapSize - 5) / 2));

            HidingSpots[i].transform.rotation =
                Quaternion.Euler(transform.rotation.eulerAngles.x,
                Random.Range(0f, 360f),
                transform.rotation.eulerAngles.z);
        }
        for (int i = 0; i < HidingSpots.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (Vector3.Distance(HidingSpots[i].transform.position,
                    HidingSpots[j].transform.position) <= 5f)
                {
                    RandomizeHidingSpots();
                }
            }
        }
    }

    //Os sobreviventes a serem resgatados (e os já resgatados em outras fases) são organizados.
    private void SetUpRescued(int rescuedAmount)
    {
        //Os sobreviventos a serem resgatados são instanciados no mapa.
        for (int i = 0; i < rescuedAmount; i++)
        {
            Survivors.Add(Instantiate(survivorPrefab).GetComponent<SurvivorController>());
        }

        //Os sobreviventes já resgatados são colocados na área inicial.
        //O primeiro sobrevivente resgatado é declarado Personagem Jogador.
        //Os resgatados são então teleportado para a posição inicial.
        for (int i = 0; i < survivorsOnScene.Count; i++)
        {
            survivorsOnScene[i].GetComponent<SurvivorController>().
                SetState(SurvivorController.SurvivorState.INITIAL);
            survivorsOnScene[i].Agent.Warp(playerSpawnPoints[i].position);
        }
        SurvivorController initialPlayer = survivorsOnScene.FirstOrDefault();
        playerController.SetPlayerCharacter(initialPlayer);
        cameraController.SetCustomPosition(initialPlayer.transform.position);

        //Os esconderijos onde os resgates inicialmente estão são randomizados
        //e tem suas vidas organizadas para o valor inicial.
        RandomizeSurvivorsStart();
    }

    //Essa função aleatoriza as posições iniciais dos Sobreviventes a serem resgatados.
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

    //Essa função gera um número aleatório de ints a serem usadas para
    //se escolher onde os sobreviventes a serem resgatados irão se esconder.
    //Isso visa gerar uma aleatorização maior de possíveis inícios.
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

    //Os colecionáveis são adicionados ao mapa.
    private void SetUpCollectables(int woodAmount, int gunAmount, int survivorsAmount)
    {
        Vector3 point;
        List<Item> itemsToBeAdded = new();

        //Os mantimentos são adicionados ao mapa.
        //A quantidade instanciada é o necessário para o time mais 2).
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

        //Os remédios são adicionados ao mapa.
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

        //As armas são finalmente adicionadas a fase.
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

    //Os zumbis são instanciados e suas posições iniciais randomizadas.
    private void SetUpZombies(int zombiesAmount)
    {
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

    //FUNÇÕES E ROTINAS RELACIONADSS A SET-UP DE NÍVEL - FIM

    //ORGANIZAÇÃO DE FLOW DE GAMEPLAYS - INÍCIO

    public IEnumerator GameOver()
    {
        yield return null;
    }

    public IEnumerator LevelUp()
    {
        yield return null;
    }

    //ORGANIZAÇÃO DE FLOW DE GAMEPLAYS - FIM
}