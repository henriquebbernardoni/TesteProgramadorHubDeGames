using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

//Controlador geral do jogo e das fases,
//aqui estão guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject hidingSpotPrefab;
    private GameObject playerSpawnPoint;

    private NavMeshSurface surface;
    private List<GameObject> hidingSpots = new();
    private List<GameObject> survivors = new();
    private List<GameObject> zombies = new();

    private PlayerController playerController;
    private CameraController cameraController;

    private float mapSize = 25;

    private void Awake()
    {
        surface = FindObjectOfType<NavMeshSurface>();
        playerSpawnPoint = GameObject.Find("PlayerSpawnPoint");
        playerController = GetComponent<PlayerController>();
        cameraController = FindObjectOfType<CameraController>();
    }

    private void Start()
    {
        StartCoroutine(SetUpLevel(2, 0));
    }

    //Dada a natureza flexível das fases, é necessário realizar
    //o Bake do NavMesh, sempre que uma fase se inicia.
    private void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }

    //Essa rotina controla a arrumação do nível para levar em conta diferentes fatores
    private IEnumerator SetUpLevel(int survivorsAmount, int zombiesAmount)
    {
        for (int i = 0; i < (2 * survivorsAmount); i++)
        {
            hidingSpots.Add(Instantiate(hidingSpotPrefab));
        }
        RandomizeHidingSpots();

        for (int i = 0; i < survivorsAmount; i++)
        {
            survivors.Add(Instantiate(survivorPrefab));
        }
        playerController.SetAsPlayerCharacter(survivors[0].GetComponent<SurvivorController>());
        playerController.PlayerCharacter.Agent.Warp(playerSpawnPoint.transform.position);
        RandomizeSurvivorsStart();

        for (int i = 0; i < zombiesAmount; i++)
        {
            zombies.Add(Instantiate(zombiePrefab));
        }

        BakeNavMesh();
        cameraController.SetCustomPosition(survivors[0].transform.position);
        yield return null;
    }

    private void RandomizeHidingSpots()
    {
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            hidingSpots[i].transform.position =
                new(Random.Range(-mapSize / 2, mapSize / 2),
                hidingSpots[i].transform.position.y,
                Random.Range(-mapSize / 2, mapSize / 2));

            hidingSpots[i].transform.rotation =
                Quaternion.Euler(transform.rotation.eulerAngles.x,
                Random.Range(0f, 360f),
                transform.rotation.eulerAngles.x);
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

    private void RandomizeSurvivorsStart()
    {
        int[] positions = GenerateRandomNumbers(survivors.Count - 1, hidingSpots.Count);

        for (int i = 1; i < survivors.Count; i++)
        {
            survivors[i].GetComponent<SurvivorController>().Agent.Warp(hidingSpots[i].transform.position);
            survivors[i].GetComponent<SurvivorController>().SetState(SurvivorController.SurvivorState.HIDE);
        }
    }

    public static int[] GenerateRandomNumbers(int count, int max)
    {
        System.Random random = new();
        int[] numbers = new int[count];

        for (int i = 0; i < count; i++)
        {
            numbers[i] = random.Next(max);
        }

        return numbers;
    }
}