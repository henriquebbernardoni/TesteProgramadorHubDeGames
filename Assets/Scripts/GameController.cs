using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

//Controlador geral do jogo e das fases,
//aqui estão guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    //Prefabs e GameObjects a serem usados no set-up do nível.
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    private GameObject playerSpawnPoint;
    private NavMeshSurface surface;

    //Listagem de GameObjects em cena que serão diretamente afetados.
    [SerializeField] private List<SurvivorController> survivors = new();
    private List<ZombieController> zombies = new();
    private List<HidingSpot> hidingSpots = new();

    private CameraController cameraController;
    private PlayerController playerController;

    public List<SurvivorController> Survivors { get => survivors; private set => survivors = value; }

    private void Awake()
    {
        surface = FindObjectOfType<NavMeshSurface>();
        playerSpawnPoint = GameObject.Find("PlayerSpawnPoint");

        cameraController = FindObjectOfType<CameraController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        SetUpLevel(2, 0);
    }

    //FUNÇÕES E ROTINAS RELACIONADSS A SET-UP DE NÍVEL - START

    //Essa rotina controla o set-up do nível como um todo.
    private void SetUpLevel(int survivorsAmount, int zombiesAmount)
    {
        SetUpScenario(survivorsAmount);

        SetUpSurvivors(survivorsAmount);

        SetUpZombies(zombiesAmount);
    }

    //O cenário a ser adicionado no mapa tem valores proporcionais
    //a quantidade de sobrevivente que podem ser resgatados no nível.
    private void SetUpScenario(int survivorsAmount)
    {
        //Os esconderijos são intanciados e tem suas posições randomizadas.
        for (int i = 0; i < (2 * survivorsAmount); i++)
        {
            hidingSpots.Add(Instantiate(hidingSpotPrefab).GetComponent<HidingSpot>());
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
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            hidingSpots[i].transform.position =
                new(Random.Range(-25 / 2, 25 / 2),
                hidingSpots[i].transform.position.y,
                Random.Range(-25 / 2, 25 / 2));

            hidingSpots[i].transform.rotation =
                Quaternion.Euler(transform.rotation.eulerAngles.x,
                Random.Range(0f, 360f),
                transform.rotation.eulerAngles.z);
        }
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (Vector3.Distance(hidingSpots[i].transform.position,
                    hidingSpots[j].transform.position) <= 5f)
                {
                    RandomizeHidingSpots();
                }
            }
        }
    }

    //Os sobreviventes a serem resgatados (e os já resgatados em outras fases) são organizados.
    private void SetUpSurvivors(int survivorsAmount)
    {
        //Os sobreviventos a serem resgatados são instanciados no mapa.
        for (int i = 0; i < survivorsAmount; i++)
        {
            Survivors.Add(Instantiate(survivorPrefab).GetComponent<SurvivorController>());
        }

        //Os sobreviventes já resgatados são colocados na área inicial.
        //O primeiro sobrevivente resgatado é declarado Personagem Jogador.
        //Os resgatados são então teleportado para a posição inicial.
        SurvivorController initialPlayer = Survivors.FirstOrDefault(survivor =>
            survivor.GetState() != SurvivorController.SurvivorState.INITIAL &&
            survivor.GetState() != SurvivorController.SurvivorState.DEATH);
        playerController.SetPlayerCharacter(initialPlayer);
        initialPlayer.Agent.Warp(playerSpawnPoint.transform.position);
        cameraController.SetCustomPosition(initialPlayer.transform.position);

        //Os esconderijos onde os resgates inicialmente estão são randomizados.
        RandomizeSurvivorsStart();

        //Todos os sobreviventes tem suas vidas organizadas para o valor inicial.
        foreach (SurvivorController survivorController in Survivors)
        {
            survivorController.ModifyHealth(5);
        }
    }

    //Essa função aleatoriza as posições iniciais dos Sobreviventes a serem resgatados.
    private void RandomizeSurvivorsStart()
    {
        SurvivorController[] rescuees = 
            Survivors.Where(survivor => survivor.GetState() == SurvivorController.SurvivorState.INITIAL).ToArray();
        int[] positions = GenerateRandomNumbers(rescuees.Length, hidingSpots.Count);

        for (int i = 0; i < rescuees.Length; i++)
        {
            rescuees[i].Agent.Warp(hidingSpots[positions[i]].transform.position);
            hidingSpots[positions[i]].SetHidingSpot(rescuees[i]);
            rescuees[i].SetState(SurvivorController.SurvivorState.INITIAL);
        }
    }

    //Essa função gera um número aleatório de ints a serem usadas para
    //se escolher onde os sobreviventes a serem resgatados irão se esconder.
    //Isso visa gerar uma aleatorização maior de possíveis inícios.
    public static int[] GenerateRandomNumbers(int count, int max)
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

    //Os zumbis são instanciados e suas posições iniciais randomizadas.
    public void SetUpZombies(int zombiesAmount)
    {
        for (int i = 0; i < zombiesAmount; i++)
        {
            zombies.Add(Instantiate(zombiePrefab).GetComponent<ZombieController>());
        }

        foreach (ZombieController zombieController in zombies)
        {
            zombieController.ModifyHealth(5);
        }
    }

    //FUNÇÕES E ROTINAS RELACIONADSS A SET-UP DE NÍVEL - END
}