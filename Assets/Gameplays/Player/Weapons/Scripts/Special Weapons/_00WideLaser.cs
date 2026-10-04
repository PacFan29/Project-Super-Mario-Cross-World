using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _00WideLaser : WeaponMovements
{
    [Header("子オブジェ")]
    public ParticleWeapon childLaser;
    private void Awake() {
        weaponType = WeaponTypes.Laser;
    }

    // Update is called once per frame
    void Update()
    {
        childLaser.player = this.player;
    }
}
