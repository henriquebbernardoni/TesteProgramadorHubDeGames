using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : Weapon
{
    public override IEnumerator WeaponBehaviour(SurvivorController attacker, ZombieController defender)
    {
        attacker.transform.LookAt(defender.transform.position);
        Vector3 originTransform = attacker.transform.Find("Body/Face").position;
        Vector3 targetTransform = defender.transform.Find("Body").position;
        targetTransform = new Vector3(targetTransform.x, originTransform.y, targetTransform.z);
        Vector3 direction = targetTransform - originTransform;
        Ray ray = new Ray(originTransform, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == defender.gameObject)
        {
            float distance = Vector3.Distance(originTransform, targetTransform);
            bool willHit = false;

            if (distance >= 9f)
            {
                if (Random.value < 0.2f)
                {
                    willHit = true;
                }
            }
            else if (distance >= 6f)
            {
                if (Random.value < 0.5f)
                {
                    willHit = true;
                }
            }
            else if (distance >= 3f)
            {
                if (Random.value < 0.8f)
                {
                    willHit = true;
                }
            }
            else
            {
                willHit = true;
            }

            if (willHit)
            {
                defender.ModifyHealth(-2);
            }
            else
            {
                Debug.Log("Errou!");
            }
        }
        else
        {
            Debug.Log("Fora de alcance");
        }

        yield return base.WeaponBehaviour(attacker, defender);
    }
}