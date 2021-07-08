using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ShootingRangeTarget : Destructables
{
    [SerializeField] float rotationSpeed;
    [SerializeField] float repairTime;

    Quaternion initialRotation;
    Quaternion targetRotation;
    bool requireRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    public override void Die()
    {
        base.Die();
        targetRotation = Quaternion.Euler(transform.right*90);

        requireRotation = true;
        GameManager.Instance.Timer.Add(() =>
        {
            targetRotation = initialRotation;
            requireRotation = true;
        },repairTime);

        
    }

    private void Update()
    {
        if (!requireRotation)
            return;

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (transform.rotation == targetRotation)
            requireRotation = true;
    }
}
