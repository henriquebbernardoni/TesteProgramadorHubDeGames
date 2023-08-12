using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SurvivorController;

//Esconderijos est�o espalhados pela fase,
//Quando algum dos sobreviventes estiver em um deles, n�o ser� visto.
public class HidingSpot : MonoBehaviour
{
    //Este � o ponto em que o Sobrevivente ficar� para se esconder.
    [SerializeField] private Transform standingPoint;

    private SurvivorController survivorHere;

    public IEnumerator EnterHidingSpot(SurvivorController survivorController)
    {
        if (survivorController.HuntedBy.Count > 0)
        {
            WarningText.Instance.SetWarningText("Voc� n�o pode se esconder!\nSeu grupo est� sendo perseguido!");
            yield break;
        }
        else if (IsSurvivorHere())
        {
            WarningText.Instance.SetWarningText("Esconderijo ocupado!");
            yield break;
        }

        survivorController.StopAllCoroutines();
        survivorController.SetSurvivorDestination(standingPoint.position);
        yield return StartCoroutine(WaitForAgentPath(survivorController));
        SetHidingSpot(survivorController);
    }

    private IEnumerator WaitForAgentPath(SurvivorController survivorController)
    {
        yield return new WaitUntil(() => survivorController.Agent.hasPath);
        yield return new WaitWhile(() => survivorController.Agent.hasPath);
    }

    public void ExitHidingSpot()
    {
        SurvivorController wasHere = survivorHere;
        survivorHere.SetHidingSpot(null);
        survivorHere = null;

        if (PlayerController.PC == wasHere)
        {
            wasHere.SetState(SurvivorState.WANDER);

            List<SurvivorController> allButPlayer =
                wasHere.SurvivorGroup
                .Except(new List<SurvivorController> { wasHere })
                .Where(survivor => survivor.GetHidingSpot()).ToList();

            foreach (SurvivorController survivor in allButPlayer)
            {
                survivor.GetHidingSpot().ExitHidingSpot();
            }
        }
        else
        {
            wasHere.SetState(SurvivorState.FOLLOW);
        }
    }


    public bool IsSurvivorHere()
    {
        return survivorHere!= null;
    }

    public void SetHidingSpot(SurvivorController survivorController)
    {
        if (survivorController.HuntedBy.Count > 0)
        {
            WarningText.Instance.SetWarningText("Voc� n�o pode se esconder!\nSeu grupo est� sendo perseguido!");
            return;
        }

        survivorHere = survivorController;
        survivorHere.SetHidingSpot(this);
        survivorController.SetState(SurvivorState.HIDE);
    }
}