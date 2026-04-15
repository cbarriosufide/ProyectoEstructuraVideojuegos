using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithFloor : MonoBehaviour
{
    CharacterController player;
    Vector3 groundPosition;
    Vector3 lastGroundPosition;
    string groundName;
    string lastGroundName;

    void Start()
    {
        player = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;

        if (player.isGrounded)
        {
           RaycastHit hit;
              if (Physics.SphereCast(transform.position, player.height / 4.2f, -transform.up, out hit))
              {
                GameObject groundedIn = hit.collider.gameObject;
                groundName = groundedIn.name;
                groundPosition = groundedIn.transform.position;

                if (groundName != lastGroundName && groundName != lastGroundName)
                {
                    this.transform.position += groundPosition - lastGroundPosition;
                }
                
                lastGroundName = groundName;
                lastGroundPosition = groundPosition;
                
              }
        }
        else if(!player.isGrounded)
        {
            lastGroundName = null;
            lastGroundPosition = Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        player = GetComponent<CharacterController>();
        if (player != null)
        {
            Gizmos.DrawWireSphere(transform.position, player.height / 4.2f);
        }
    }
}
