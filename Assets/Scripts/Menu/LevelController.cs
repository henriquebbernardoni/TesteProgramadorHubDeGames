using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    private GameController gameController;

    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject levelScreen;
    [SerializeField] private GameObject victoryScreen;

    private int currentLevel = 0;

    private void Awake()
    {
        Instance = this;
        gameController = FindObjectOfType<GameController>();
    }

    private void Start()
    {
        deathScreen.SetActive(false);
        levelScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }

    //Dados a serem mostrados na pontuação
    private int collectedItems;
    private int rescuedSurvivors;
    private int killedZombies;

    //Funções usadas para alterar os valores da pontuação
    public void AddToCollectedItems()
    {
        collectedItems++;
    }
    public void AddToRescuedSurvivors()
    {
        rescuedSurvivors++;
    }
    public void AddToKilledZombies()
    {
        killedZombies++;
    }

    //Funções usadas para mostras as telas de mudança de final
    public void DisplayDeathScreen()
    {
        deathScreen.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text =
            "Itens coletados: " + collectedItems + " / " +
            "Sobreviventes resgatados: " + rescuedSurvivors + " / " +
            "Zumbis mortos: " + killedZombies;
        deathScreen.SetActive(true);
    }
    public void DisplayVictoryScreen()
    {
        victoryScreen.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text =
            "Itens coletados: " + collectedItems + " / " +
            "Sobreviventes resgatados: " + rescuedSurvivors + " / " +
            "Zumbis mortos: " + killedZombies;
        victoryScreen.SetActive(true);
    }
    public void DisplayNextLevelScreen()
    {

    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel == 1)
        {
            gameController.SetUpLevel(6, 4, 3, 1);
        }
        else if (currentLevel == 2)
        {
            gameController.SetUpLevel(9, 6, 3, 1);
        }
        else if (currentLevel == 3)
        {
            DisplayVictoryScreen();
        }
    }

    //Funções relacionadas a transição de fases
    public void RestartGame()
    {
        collectedItems = 0;
        rescuedSurvivors = 0;
        killedZombies = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public bool AllZombiesDead()
    {
        return gameController.Zombies.All
            (x => x.GetState() == ZombieController.ZombieState.DEATH);
    }
    public void NextLevelZombiesDead()
    {

    }

    public bool MinSurvivorsRescued()
    {
        return gameController.Survivors.Where(x => x.GetState() == 
            SurvivorController.SurvivorState.RESCUE).ToArray().Length <=
            gameController.SurvivorsAdded - 2;
    }
    public void NextLevelSurvivorsRescued()
    {

    }
}