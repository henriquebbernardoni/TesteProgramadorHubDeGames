using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Esconderijos est�o espalhados pela fase,
//Quando algum dos sobreviventes estiver em um deles, n�o ser� visto.
public class HidingSpot : MonoBehaviour
{
    //Este � o ponto em que o Sobrevivente ficar� para se esconder.
    [SerializeField] private Transform standingPoint;

    private SurvivorController survivorHere;

    public IEnumerator EnterHidingSpot(SurvivorController survivorController)
    {
        if (IsSurvivorHere())
        {
            WarningText.Instance.SetWarningText("Esconderijo ocupado!");
            yield break;
        }

        survivorController.SetSurvivorDestination(standingPoint.position);
        yield return new WaitUntil(() => survivorController.Agent.hasPath);
        yield return new WaitWhile(() => survivorController.Agent.hasPath);
        yield return new WaitForEndOfFrame();
        SetHidingSpot(survivorController);
    }

    public void ExitHidingSpot()
    {
        survivorHere.SetHidingSpot(null);
        survivorHere = null;
    }

    public bool IsSurvivorHere()
    {
        return survivorHere;
    }

    public void SetHidingSpot(SurvivorController survivorController)
    {
        survivorHere = survivorController;
        survivorHere.SetHidingSpot(this);
        survivorController.SetState(SurvivorController.SurvivorState.HIDE);
    }
}