using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialWeaponsManager : MonoBehaviour
{
    private MegaManActions megaMan;
    private PlayerInfo info;

    private float reloadTime = 0f;

    private byte weaponId;

    private byte laserWeaponId;
    private GameObject laserPref;

    // Start is called before the first frame update
    void Start()
    {
        megaMan = GetComponent<MegaManActions>();
        info = GetComponent<PlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponId = (byte)megaMan.weaponId;

        if (reloadTime > 0f)
        {
            reloadTime -= Time.deltaTime;
        }

        int count = GameObject.FindGameObjectsWithTag("SpecialWeapon").Length;
        
        // if (info.ButtonsDown["X"] && (weaponId >= 0 && weaponId <= 7)){
        if (info.ButtonsDown["X"] && weaponId != 0 && !info.giant){
            SpecialWeapon weapon = megaMan.weaponList[weaponId];
            // bool canShoot = (weapon.isSingle && count < 1) || !weapon.isSingle;

            // if (megaMan.isWeaponConsumable() && canShoot) {
            if (megaMan.isWeaponConsumable() && reloadTime <= 0 && (count < weapon.limitCount || weapon.limitCount == 0))
            {
                megaMan.gameObject.GetComponent<PlayerVoiceManager>().Attack();
                
                megaMan.ConsumeWeaponEnergy();

                if (weapon.prefab != null)
                {
                    if (weapon.Type == WeaponTypes.Laser)
                    {
                        //ワイドレーザー
                        laserWeaponId = weaponId;
                        WeaponMovements wp = Instantiate(weapon.prefab, this.transform.position + info.skin.rotation * Vector3.forward * 3.5f, info.skin.rotation, info.skin).GetComponent<WeaponMovements>();
                        wp.player = info;
                        laserPref = wp.gameObject;
                    }
                    else if (weapon.Type == WeaponTypes.Bomb)
                    {
                        //スプレッドダイナマイト
                        for (int i = 0; i < 3; i++)
                        {
                            Vector3 rot = info.skin.rotation.eulerAngles;

                            switch (i)
                            {
                                case 1:
                                    rot += new Vector3(0f, -30f, 0f);
                                    break;

                                case 2:
                                    rot += new Vector3(0f, 30f, 0f);
                                    break;
                            }
                            WeaponMovements wp = Instantiate(weapon.prefab, this.transform.position + Quaternion.Euler(rot) * Vector3.forward * 1.5f, Quaternion.Euler(rot)).GetComponent<WeaponMovements>();
                            wp.VelocityChange(1, (i > 0) ? 30f : 40f);
                            wp.player = info;
                        }
                    }
                    else if (weapon.Type == WeaponTypes.Dash)
                    {
                        //フラッシュダッシュ
                        ;
                    }
                    else
                    {
                        WeaponMovements wp = Instantiate(weapon.prefab, this.transform.position + info.skin.rotation * Vector3.forward * 1.5f, info.skin.rotation).GetComponent<WeaponMovements>();
                        wp.player = info;
                    }
                }
                reloadTime = weapon.reloadTime;
            }
        }

        if (laserPref != null)
        {
            megaMan.ConsumeWeaponEnergy();
            if (!info.Buttons["X"] || laserWeaponId != weaponId || !megaMan.isWeaponConsumable())
            {
                Destroy(laserPref);
            }
        }
    }
}
