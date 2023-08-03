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
            if (hit.collider.GetComponent<NavMeshSurface>())
            {
                StartCoroutine(WalkHere(hit.point));
            }
            else if (hit.collider.GetComponent<HidingSpot>())
            {
                StartCoroutine(hit.collider.GetComponent<HidingSpot>().EnterHidingSpot(playerCharacter));
            }
        }
    }

    //A rotina utilizada para mover o jogador,
    //Inclui uma chacagem se há algum esconderijo por perto.
    private IEnumerator WalkHere(Vector3 destination)
    {
        playerCharacter.SetSurvivorDestination(destination);
        yield return new WaitWhile(() => playerCharacter.Agent.hasPath);
        while (playerCharacter.Agent.hasPath)
        {
            yield return new WaitForEndOfFrame();
        }

        HidingSpot hidingSpot = playerCharacter.DetectNearbyHidingSpot();
        if (hidingSpot)
        {
            StartCoroutine(hidingSpot.EnterHidingSpot(playerCharacter));
        }
    }
}