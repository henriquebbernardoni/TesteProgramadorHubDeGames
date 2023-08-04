using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

//Esse script controla tudo relacionado aos movimentos do jogador,
//incluindo movimentação, ataques, esconderijos e troca de PJ.
public class PlayerController : MonoBehaviour
{
    [SerializeField] private SurvivorController playerCharacter;
    [SerializeField] private InventoryController inventoryController;

    private void Awake()
    {
        inventoryController = GetComponent<InventoryController>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RightButtonClick();
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
                StartCoroutine(hit.collider.GetComponent<HidingSpot>().EnterHidingSpot(playerCharacter));
            }
            else if (hit.collider.GetComponent<Item>())
            {
                StopAllCoroutines();
                StartCoroutine(GetItem(hit.collider.gameObject));
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

    private IEnumerator GetItem(GameObject item)
    {
        playerCharacter.SetSurvivorDestination(item.transform.position);
        yield return new WaitUntil(() => playerCharacter.Agent.hasPath);
        yield return new WaitWhile(() => playerCharacter.Agent.remainingDistance >= 1f);
        playerCharacter.Agent.ResetPath();
        playerCharacter.Agent.velocity = Vector3.zero;
        yield return new WaitForEndOfFrame();
        inventoryController.AddItemToInventory(item.GetComponent<Item>());
        item.SetActive(false);
    }
}