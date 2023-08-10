using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SurvivorController;

//Esconderijos estão espalhados pela fase,
//Quando algum dos sobreviventes estiver em um deles, não será visto.
public class HidingSpot : MonoBehaviour
{
    //Este é o ponto em que o Sobrevivente ficará para se esconder.
    [SerializeField] private Transform standingPoint;

    [SerializeField] private SurvivorController survivorHere;

    public IEnumerator EnterHidingSpot(SurvivorController survivorController)
    {
        if (survivorController.HuntedBy.Count > 0)
        {
            WarningText.Instance.SetWarningText("Você não pode se esconder!\nSeu grupo está sendo perseguido!");
            yield break;
        }
        else if (IsSurvivorHere())
        {
            WarningText.Instance.SetWarningText("Esconderijo ocupado!");
            yield break;
        }

        survivorController.StopAllCoroutines();
        survivorController.SetSurvivorDestination(standingPoint.position);
        yield return new WaitUntil(() => survivorController.Agent.hasPath);
        yield return new WaitWhile(() => survivorController.Agent.hasPath);
        yield return new WaitForEndOfFrame();
        SetHidingSpot(survivorController);
    }

    public void ExitHidingSpot()
    {
        SurvivorController wasHere = survivorHere;
        survivorHere.SetHidingSpot(null);
        survivorHere = null;

        if (wasHere.PlayerCharacter == wasHere)
        {
            wasHere.SetState(SurvivorState.WANDER);

            List<SurvivorController> allButPlayer = wasHere.SurvivorGroup.
                Where(survivor => survivor != wasHere && survivor.GetHidingSpot()).ToList();
            for (int i = 0; i < allButPlayer.Count; i++)
            {                
                allButPlayer[i].GetHidingSpot().ExitHidingSpot();
            }
        }
        else
        {
            wasHere.SetState(SurvivorState.FOLLOW);
        }
    }

    public bool IsSurvivorHere()
    {
        return survivorHere;
    }

    public void SetHidingSpot(SurvivorController survivorController)
    {
        if (survivorController.HuntedBy.Count > 0)
        {
            WarningText.Instance.SetWarningText("Você não pode se esconder!\nSeu grupo está sendo perseguido!");
            return;
        }

        survivorHere = survivorController;
        survivorHere.SetHidingSpot(this);
        survivorController.SetState(SurvivorController.SurvivorState.HIDE);
    }
}