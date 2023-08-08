using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

//Controlador geral do jogo e das fases,
//aqui est�o guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    //Prefabs e GameObjects a serem usados no set-up do n�vel.
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    private GameObject playerSpawnPoint;
    private NavMeshSurface surface;

    //Listagem de GameObjects em cena que ser�o diretamente afetados.
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

    //FUN��ES E ROTINAS RELACIONADSS A SET-UP DE N�VEL - START

    //Essa rotina controla o set-up do n�vel como um todo.
    private void SetUpLevel(int survivorsAmount, int zombiesAmount)
    {
        SetUpScenario(survivorsAmount);

        SetUpSurvivors(survivorsAmount);

        SetUpZombies(zombiesAmount);
    }

    //O cen�rio a ser adicionado no mapa tem valores proporcionais
    //a quantidade de sobrevivente que podem ser resgatados no n�vel.
    private void SetUpScenario(int survivorsAmount)
    {
        //Os esconderijos s�o intanciados e tem suas posi��es randomizadas.
        for (int i = 0; i < (2 * survivorsAmount); i++)
        {
            hidingSpots.Add(Instantiate(hidingSpotPrefab).GetComponent<HidingSpot>());
        }
        RandomizeHidingSpots();

        //Dada a natureza flex�vel das fases, � necess�rio realizar
        //o Bake do NavMesh, sempre que uma fase se inicia,
        //logo ap�s os itens de cen�rio serem instanciados e postos.
        surface.BuildNavMesh();
    }

    //Os locais e rota��es dos esconderijos s�o randomizados,
    //assegurando que eles n�o fiquem muito perto uns dos outros.
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

    //Os sobreviventes a serem resgatados (e os j� resgatados em outras fases) s�o organizados.
    private void SetUpSurvivors(int survivorsAmount)
    {
        //Os sobreviventos a serem resgatados s�o instanciados no mapa.
        for (int i = 0; i < survivorsAmount; i++)
        {
            Survivors.Add(Instantiate(survivorPrefab).GetComponent<SurvivorController>());
        }

        //Os sobreviventes j� resgatados s�o colocados na �rea inicial.
        //O primeiro sobrevivente resgatado � declarado Personagem Jogador.
        //Os resgatados s�o ent�o teleportado para a posi��o inicial.
        SurvivorController initialPlayer = Survivors.FirstOrDefault(survivor =>
            survivor.GetState() != SurvivorController.SurvivorState.INITIAL &&
            survivor.GetState() != SurvivorController.SurvivorState.DEATH);
        playerController.SetPlayerCharacter(initialPlayer);
        initialPlayer.Agent.Warp(playerSpawnPoint.transform.position);
        cameraController.SetCustomPosition(initialPlayer.transform.position);

        //Os esconderijos onde os resgates inicialmente est�o s�o randomizados.
        RandomizeSurvivorsStart();

        //Todos os sobreviventes tem suas vidas organizadas para o valor inicial.
        foreach (SurvivorController survivorController in Survivors)
        {
            survivorController.ModifyHealth(5);
        }
    }

    //Essa fun��o aleatoriza as posi��es iniciais dos Sobreviventes a serem resgatados.
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

    //Essa fun��o gera um n�mero aleat�rio de ints a serem usadas para
    //se escolher onde os sobreviventes a serem resgatados ir�o se esconder.
    //Isso visa gerar uma aleatoriza��o maior de poss�veis in�cios.
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

    //Os zumbis s�o instanciados e suas posi��es iniciais randomizadas.
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

    //FUN��ES E ROTINAS RELACIONADSS A SET-UP DE N�VEL - END
}