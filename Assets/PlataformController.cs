using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformController : MonoBehaviour
{
    public Rigidbody plataformRB;
    public Transform[] plataforPositions;
    public float plataformSpeed;

    private int actualPosition = 0;
    private int nextPosition = 1;

    public bool moveToTheNext = true;
    public float waitTime;
    // Update is called once per frame
    void Update()
    {
        MovePlataform();
    }

    void MovePlataform()
    {

        if (moveToTheNext)
        {
            StopCoroutine(WaitToMove(0));
            plataformRB.MovePosition(Vector3.MoveTowards(plataformRB.position, plataforPositions[nextPosition].position, plataformSpeed * Time.deltaTime));
        }
        plataformRB.MovePosition(Vector3.MoveTowards(plataformRB.position, plataforPositions[nextPosition].position, plataformSpeed * Time.deltaTime));

        if (Vector3.Distance(plataformRB.position, plataforPositions[nextPosition].position) <= 0)
        {
            StartCoroutine(WaitToMove(waitTime));
            actualPosition = nextPosition;
            nextPosition++;
            
            if (nextPosition >= plataforPositions.Length)
            {
                nextPosition = 0;
            }
        }
    }

    IEnumerator WaitToMove(float time)
    {
        moveToTheNext = false;
        yield return new WaitForSeconds(time);
        moveToTheNext = true;
    }
}
