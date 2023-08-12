using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject levelScreen;
    [SerializeField] private GameObject victoryScreen;

    //Dados a serem mostrados na pontuação
    private int collectedItems;
    private int rescuedSurvivors;
    private int killedZombies;

    private int currentLevel = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        deathScreen.SetActive(false);
        levelScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }

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

    public bool MinSurvivorsRescued()
    {
        return GameController.Survivors.Where(x => x.GetState() ==
            SurvivorController.SurvivorState.RESCUE).ToArray().Length <=
            GameController.SurvivorsAdded - 2;
    }
    public void NextLevelSurvivorsRescued()
    {
        SurvivorController[] survivorsRescued = GameController.Survivors.
            Where(x => (x.GetState() != SurvivorController.SurvivorState.DEATH
            || x.GetState() != SurvivorController.SurvivorState.RESCUE)
            && x != PlayerController.PC).ToArray();
        Supply supply = InventoryController.InventoryItems.OfType<Supply>().FirstOrDefault();

        levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text =
            "Você conseguiu escapar com os sobreviventes e itens que conseguiu coletar.";
        if (supply != null)
        {
            if (supply.Quantity < survivorsRescued.Length / 2)
            {
                levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                    "\nInfelizmente, você não tem mantimentos o suficiente para suprir suas necessidades.";
                if (Random.value <= 0.5f)
                {
                    for (int i = 0; i < (survivorsRescued.Length - 2 * supply.Quantity); i++)
                    {
                        survivorsRescued.FirstOrDefault(x => x.GetState() != SurvivorController.SurvivorState.DEATH)
                            .SetState(SurvivorController.SurvivorState.DEATH);
                        levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                        "\nSobreviventes pereceram.";
                    }
                }
                else
                {
                    levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                    "\nVocê tem sorte que ninguém morreu...";
                }
            }
        }
        else
        {
            levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                "\nInfelizmente, você não tem mantimentos o suficiente para suprir suas necessidades.";
            if (Random.value <= 0.5f)
            {
                for (int i = 0; i < (survivorsRescued.Length - 2 * supply.Quantity); i++)
                {
                    survivorsRescued.FirstOrDefault(x => x.GetState() != SurvivorController.SurvivorState.DEATH)
                        .SetState(SurvivorController.SurvivorState.DEATH);
                    levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                    "\nSobreviventes pereceram.";
                }
            }
            else
            {
                levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text +=
                "\nVocê tem sorte que ninguém morreu...";
            }
        }

        survivorsRescued = GameController.Survivors.
            Where(x => (x.GetState() != SurvivorController.SurvivorState.DEATH
            || x.GetState() != SurvivorController.SurvivorState.RESCUE)
            && x != PlayerController.PC).ToArray();
        for (int i = 0; i < survivorsRescued.Length / 2; i++)
        {
            InventoryController.RemoveItemFromInventory(supply);
        }

        levelScreen.SetActive(true);
    }

    //Com todos os zumbis mortos, todos os sobreviventes serão automaticamente resgatados
    //e todos os itens coletados.
    public bool AllZombiesDead()
    {
        return GameController.Zombies.All
            (x => x.GetState() == ZombieController.ZombieState.DEATH);
    }
    public void NextLevelZombiesDead()
    {
        foreach (Item item in GameController.Items)
        {
            if (!InventoryController.InventoryItems.Contains(item))
            {
                if (item.isActiveAndEnabled)
                {
                    AddToCollectedItems();
                    InventoryController.AddItemToInventory(item);
                }
            }
        }

        foreach (SurvivorController survivor in GameController.Survivors)
        {
            if (survivor.GetState() != SurvivorController.SurvivorState.RESCUE ||
                survivor.GetState() != SurvivorController.SurvivorState.DEATH)
            {
                AddToRescuedSurvivors();
                PlayerController.PC.AddToSurvivorGroup(survivor, true);
                if (survivor.CurrentHidingSpot)
                {
                    survivor.CurrentHidingSpot.ExitHidingSpot();
                }
                survivor.SetState(SurvivorController.SurvivorState.FINAL);
            }
        }

        levelScreen.transform.Find("ResultsText").GetComponent<TextMeshProUGUI>().text =
            "Com todos os zumbis mortos, você consegue resgatar os sobreviventes " +
            "e coletar os itens sem se preocupar.";

        levelScreen.SetActive(true);
    }

    //Funções usadas para mostrar as telas de mudança de final
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


    //Funções relacionadas a transição de fases
    public void NextLevel()
    {
        levelScreen.SetActive(false);

        currentLevel++;
        if (currentLevel == 1)
        {
            FindObjectOfType<GameController>().LevelLoad(6, 4, 3, 1);
        }
        else if (currentLevel == 2)
        {
            FindObjectOfType<GameController>().LevelLoad(6, 6, 3, 1);
        }
        else if (currentLevel == 3)
        {
            DisplayVictoryScreen();
        }
    }
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
}