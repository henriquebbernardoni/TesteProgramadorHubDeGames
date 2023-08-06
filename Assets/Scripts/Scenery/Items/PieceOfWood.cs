using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PieceOfWood : Weapon
{
    public override IEnumerator WeaponBehaviour(SurvivorController attacker, ZombieController defender)
    {
        attacker.SetSurvivorDestination(defender.transform.position);
        yield return new WaitUntil(() => attacker.Agent.hasPath);
        yield return new WaitWhile(() => attacker.Agent.remainingDistance >= 1.5f);
        attacker.Agent.ResetPath();
        attacker.Agent.velocity = Vector3.zero;
        yield return new WaitForEndOfFrame();

        Vector3 forward = defender.transform.TransformDirection(Vector3.forward);
        Vector3 direction = attacker.transform.position - defender.transform.position;
        if (Vector3.Dot(forward, direction) < 0)
        {
            defender.ModifyHealth(-2);
            WarningText.Instance.SetWarningText("Ataque acertou!");
        }
        else
        {
            if (Random.value < 0.5f)
            {
                defender.ModifyHealth(-1);
                WarningText.Instance.SetWarningText("Ataque acertou!");
            }
            else
            {
                WarningText.Instance.SetWarningText("Ataque acertou!");
            }
        }

        yield return base.WeaponBehaviour(attacker, defender);
    }
}