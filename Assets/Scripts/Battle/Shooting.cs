using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooting : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] GameObject firePoint;
    [SerializeField] Transform firePointTransform;
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] float bulletForce = 20f;

    public void PullTrigger(InputAction.CallbackContext context)
    {
        Shoot();
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePointTransform.position, firePointTransform.rotation);
        bullet.transform.SetParent(firePoint.transform, false);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.rotation = player.GetAimAngle();
        Debug.Log(player.GetAimAngle());
        //rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
    }
}
