using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

//Esconderijos est�o espalhados pela fase,
//Quando algum dos sobreviventes estiver em um deles, n�o ser� visto.
public class HidingSpot : MonoBehaviour
{
    //Este � o ponto em que o Sobrevivente ficar� para se esconder.
    [SerializeField] protected Transform standingPoint;

    public virtual IEnumerator EnterHidingSpot(SurvivorController survivorController)
    {
        survivorController.SetSurvivorDestination(standingPoint.position);
        yield return new WaitUntil(() => survivorController.Agent.hasPath);
        yield return new WaitWhile(() => survivorController.Agent.hasPath);
        yield return new WaitForEndOfFrame();
        survivorController.SetState(SurvivorController.SurvivorStates.HIDE);
    }
}