using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class battleManagerScript : MonoBehaviour
{
    public Camera CSS;
    public gameManagerScript GMS;
    private bool Status;
    
    
    public void Battle(GameObject starter, GameObject receiver)
    {
        Status = true;
        var startUnit = starter.GetComponent<UnitScript>();
        var receiverUnit = receiver.GetComponent<UnitScript>();
        int startAtt = startUnit.attackDamage;
        int receiverAtt = receiverUnit.attackDamage;

        if (startUnit.attackRange == receiverUnit.attackRange)
        {
            GameObject tempParticle = Instantiate( receiverUnit.GetComponent<UnitScript>().damagedParticle,receiver.transform.position, receiver.transform.rotation);
            Destroy(tempParticle, 2f);
            receiverUnit.dealDamage(startAtt);
            if (CheckIfDead(receiver))
            {
                receiver.transform.parent = null;
                receiverUnit.unitDie();
                Status = false;
                GMS.checkIfUnitsRemain(starter, receiver);
                return;
            }
            startUnit.dealDamage(receiverAtt);
            if (CheckIfDead(starter))
            {
                starter.transform.parent = null;
                startUnit.unitDie();
                Status = false;
                GMS.checkIfUnitsRemain(starter, receiver);
                return;
            }
        }
        //check unit doesn't have same attack range
        else
        {
            GameObject tempParticle = Instantiate(receiverUnit.GetComponent<UnitScript>().damagedParticle, receiver.transform.position, receiver.transform.rotation);
            Destroy(tempParticle, 2f);
           
            receiverUnit.dealDamage(startAtt);
            if (CheckIfDead(receiver))
            {
                receiver.transform.parent = null;
                receiverUnit.unitDie();
                Status = false;
                GMS.checkIfUnitsRemain(starter, receiver);
                return;
            }
        }
        Status = false;
    }

    public bool CheckIfDead(GameObject unitToCheck)
    {
        if (unitToCheck.GetComponent<UnitScript>().currentHealthPoints <= 0)
        {
            return true;
        }
        return false;
    }
    public void DestroyObject(GameObject unitToDestroy)
    {
        Destroy(unitToDestroy);
    }

    public IEnumerator Attack(GameObject unit, GameObject enemy)
    {
        Status = true;
        float elapsedTime = 0;
        Vector3 startingPos = unit.transform.position;
        Vector3 endingPos = enemy.transform.position;
        while (elapsedTime < .25f)
        {
           
            unit.transform.position = Vector3.Lerp(startingPos, startingPos+((((endingPos - startingPos) / (endingPos - startingPos).magnitude)).normalized*.5f), (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }
        while (Status)
        { 
            StartCoroutine(CSS.camShake(.2f,unit.GetComponent<UnitScript>().attackDamage,GetDirection(unit,enemy)));
            if(unit.GetComponent<UnitScript>().attackRange == enemy.GetComponent<UnitScript>().attackRange && enemy.GetComponent<UnitScript>().currentHealthPoints - unit.GetComponent<UnitScript>().attackDamage > 0)
            {
                StartCoroutine(unit.GetComponent<UnitScript>().displayDamageEnum(enemy.GetComponent<UnitScript>().attackDamage));
                StartCoroutine(enemy.GetComponent<UnitScript>().displayDamageEnum(unit.GetComponent<UnitScript>().attackDamage));
            }
           
            else
            {
                StartCoroutine(enemy.GetComponent<UnitScript>().displayDamageEnum(unit.GetComponent<UnitScript>().attackDamage));
            }
            
            Battle(unit, enemy);
            
            yield return new WaitForEndOfFrame();
        }
        
        if (unit != null)
        {
           StartCoroutine(RePosition(unit, startingPos)); 
        }
    }
    public IEnumerator RePosition(GameObject unit, Vector3 endPoint) {
        float elapsedTime = 0;
        while (elapsedTime < .30f)
        {
            unit.transform.position = Vector3.Lerp(unit.transform.position, endPoint, (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }   
        unit.GetComponent<UnitScript>().wait();
    }
    public Vector3 GetDirection(GameObject unit, GameObject enemy)
    {
        Vector3 startDirection = unit.transform.position;
        Vector3 lastDirection = enemy.transform.position;
        return (((lastDirection - startDirection) / (lastDirection - startDirection).magnitude)).normalized;
    }
}
