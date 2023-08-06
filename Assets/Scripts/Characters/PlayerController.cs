using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//Esse script controla tudo relacionado aos movimentos do jogador,
//incluindo movimentação, ataques, esconderijos e troca de PJ.
public class PlayerController : MonoBehaviour
{
    [SerializeField] private SurvivorController playerCharacter;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private TextMeshProUGUI actionDescription;

    public SurvivorController PlayerCharacter { get => playerCharacter; private set => playerCharacter = value; }

    private void Awake()
    {
        inventoryController = GetComponent<InventoryController>();
    }

    private void Update()
    {
        DescribeAction();
        if (Input.GetMouseButtonDown(1))
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
                playerCharacter.FullStop();
                StartCoroutine(hit.collider.GetComponent<HidingSpot>().EnterHidingSpot(playerCharacter));
            }
            else if (hit.collider.GetComponent<Item>())
            {
                StopAllCoroutines();
                playerCharacter.FullStop();
                StartCoroutine(GetItem(hit.collider.GetComponent<Item>()));
            }
            else if (hit.collider.GetComponent<ZombieController>())
            {
                if (!playerCharacter.GetWeapon())
                {
                    WarningText.Instance.SetWarningText("Nenhuma arma selecionada!");
                }
                else if (playerCharacter.IsRecharging)
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
                    playerCharacter.FullStop();
                    StartCoroutine(playerCharacter.GetWeapon().
                        WeaponBehaviour(playerCharacter, hit.collider.GetComponent<ZombieController>()));
                }
            }
        }
    }

    //A rotina utilizada para mover o jogador,
    //Inclui uma chacagem se no destino há algum esconderijo por perto.
    private IEnumerator WalkHere(Vector3 destination)
    {
        playerCharacter.SetSurvivorDestination(destination);
        yield return new WaitUntil(() => playerCharacter.Agent.hasPath);
        yield return new WaitWhile(() => playerCharacter.Agent.hasPath);
        yield return new WaitForEndOfFrame();

        HidingSpot hidingSpot = playerCharacter.DetectNearbyHidingSpot();
        if (hidingSpot)
        {
            StartCoroutine(hidingSpot.EnterHidingSpot(playerCharacter));
        }
    }

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

    public void SetAsPlayerCharacter(SurvivorController newPlayer)
    {
        if (playerCharacter)
        {
            PlayerCharacter.SetAsPlayerCharacter(false);
        }
        newPlayer.SetAsPlayerCharacter(true);
        PlayerCharacter = newPlayer;
    }
}