using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    private void LateUpdate()
    {
        if (!IsOwner) return;

        Vector2 aimScreenPosition = inputReader.AimPosition;
        Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);

        //set turret's up vector to point at the cursor
        //difference between the cursor and the turret position
        turretTransform.up = new Vector2(
            aimWorldPosition.x - turretTransform.position.x,
            aimWorldPosition.y - turretTransform.position.y);
    }
}
