using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region 变量
    [Header("--- Info ---")]
    [SerializeField] int hp = 100;
    [Header("--- View ---")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotateSpeed = 2f;
    float moveVerticalInput;
    float moveHorizontalInput;
    float mouseXInput;
    float mouseYInput;
    float angleLeftRight = 0f;
    float angleUpDown = 0f;
    [Header("--- Jump ---")]
    [SerializeField] float jumpForce = 300f;
    [Header("--- Shoot ---")]
    [SerializeField] GUNTYPE gunType;
    [SerializeField] float shootCD = 0.3f;
    [SerializeField] Transform muzzle;
    [SerializeField] GameObject bloodEffectPrefab;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] Transform shootEffectPos;
    [SerializeField] GameObject shootEffectPrefab;
    float lastShootTime = 0f;
    [Header("--- Bullet ---")]
    [SerializeField] int bulletSingleMagezineMax = 10;
    [SerializeField] int bulletSingleBag = 30;
    [SerializeField] int bulletAutoMagezineMax = 30;
    [SerializeField] int bulletAutoBag = 90;
    [SerializeField] int bulletSniperMagezineMax = 1;
    [SerializeField] int bulletSniperBag = 10;
    Dictionary<GUNTYPE, int> bulletBag = new Dictionary<GUNTYPE, int>(); // 子弹字典，存储不同类型子弹的数量
    Dictionary<GUNTYPE, int> bulletMagezine = new Dictionary<GUNTYPE, int>(); // 子弹字典，存储不同类型子弹的数量
    bool isReloading;
    [Header("--- Weapons ---")]
    [SerializeField] GameObject[] gunModels; // 武器模型数组
    [SerializeField] GameObject scope;
    [SerializeField] int damageSingle = 5;
    [SerializeField] int damageAuto = 2;
    [SerializeField] int damageSniper = 20;
    Dictionary<GUNTYPE, int> weaponDamage = new Dictionary<GUNTYPE, int>(); // 武器字典，存储不同类型武器的伤害
    [Header("--- Sound ---")]
    [SerializeField] AudioSource sound;
    [SerializeField] AudioClip singleShootSound;
    [SerializeField] AudioClip autoShootSound;
    [SerializeField] AudioClip sniperShootSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip hitGroundSound;
    [SerializeField] AudioClip moveSound;
    [SerializeField] AudioClip jumpSound;
    [Header("--- UI ---")]
    [SerializeField] GameObject[] gunUIIcons;
    [SerializeField] Text playerHpText;
    [SerializeField] Text bulletText;
    [SerializeField] GameObject bloodUIImage;
    [SerializeField] GameObject scopeUIImage;
    [SerializeField] GameObject gameOverUI;


    Rigidbody rb; // 刚体组件
    Animator animator; // 动画控制器
    #endregion
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // 获取刚体组件
        animator = GetComponent<Animator>(); // 获取动画控制器
    }
    void Start()
    {
        Cursor.visible = false; // 隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标

        bulletBag.Add(GUNTYPE.SINGLE, bulletSingleBag);
        bulletBag.Add(GUNTYPE.AUTO, bulletAutoBag);
        bulletBag.Add(GUNTYPE.SNIPER, bulletSniperBag);

        bulletMagezine.Add(GUNTYPE.SINGLE, bulletSingleMagezineMax);
        bulletMagezine.Add(GUNTYPE.AUTO, bulletAutoMagezineMax);
        bulletMagezine.Add(GUNTYPE.SNIPER, bulletSniperMagezineMax);

        weaponDamage.Add(GUNTYPE.SINGLE, damageSingle);
        weaponDamage.Add(GUNTYPE.AUTO, damageAuto);
        weaponDamage.Add(GUNTYPE.SNIPER, damageSniper);
    }
    void Update()
    {
        PlayerMove();
        LookAround();
        Shoot();
        SwitchScope();
        Jump();
        ChangeWeapon();
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }
    void PlayerMove()
    {
        //接收输入
        moveVerticalInput = Input.GetAxis("Vertical");//3d游戏里的前后
        moveHorizontalInput = Input.GetAxis("Horizontal");//3d游戏里的左右
        //控制移动
        transform.position += transform.forward * moveVerticalInput * moveSpeed * Time.deltaTime;
        transform.position += transform.right * moveHorizontalInput * moveSpeed * Time.deltaTime;
        animator.SetFloat("MoveX", moveHorizontalInput);
        animator.SetFloat("MoveY", moveVerticalInput);
        if (moveVerticalInput > 0 || moveHorizontalInput > 0)
        {
            if (!sound.isPlaying)
            {
                PlaySound(moveSound);
            }
        }
    }
    void LookAround()
    {
        //接收输入
        mouseXInput = Input.GetAxis("Mouse X");
        mouseYInput = -Input.GetAxis("Mouse Y");
        //计算视角
        angleLeftRight += mouseXInput * rotateSpeed;
        angleUpDown = Mathf.Clamp(angleUpDown + mouseYInput * rotateSpeed, -60, 60);
        //应用视角
        transform.eulerAngles = new Vector3(angleUpDown, angleLeftRight, 0);
    }
    void Shoot()
    {
        if (!isReloading)
        {
            switch (gunType)
            {
                case GUNTYPE.SINGLE:
                    SingleShoot();
                    break;
                case GUNTYPE.AUTO:
                    AutoShoot();
                    break;
                case GUNTYPE.SNIPER:
                    SniperShoot();
                    break;
                default:
                    break;
            }
        }
    }
    void SwitchScope()
    {
        if (Input.GetMouseButton(1) && gunType == GUNTYPE.SNIPER)
        {
            scope.SetActive(true);
            scopeUIImage.SetActive(true);
        }
        else
        {
            scope.SetActive(false);
            scopeUIImage.SetActive(false);
        }
    }
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(jumpForce * Vector3.up);
            sound.PlayOneShot(jumpSound);
        }
    }
    void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            gunType++;
            if (gunType > GUNTYPE.SNIPER)
            {
                gunType = GUNTYPE.SINGLE;
            }
        }
        switch (gunType)
        {
            case GUNTYPE.SINGLE:
                shootCD = 0.3f;
                ChangeWeaponModel(0);
                break;
            case GUNTYPE.AUTO:
                shootCD = 0.1f;
                ChangeWeaponModel(1);
                break;
            case GUNTYPE.SNIPER:
                shootCD = 1.5f;
                ChangeWeaponModel(2);
                break;
            default:
                break;
        }
    }
    void ChangeWeaponModel(int gunNumber)
    {
        for (int i = 0; i < gunModels.Length; i++)
        {
            gunModels[i].SetActive(false);
            gunUIIcons[i].SetActive(false);
        }
        gunModels[gunNumber].SetActive(true);
        gunUIIcons[gunNumber].SetActive(true);
        bulletText.text = bulletMagezine[gunType].ToString() + "/" + bulletBag[gunType].ToString();
    }
    void GunShoot()
    {
        //播放射击动画
        animator.SetTrigger("Shoot");
        //播放射击特效
        Instantiate(shootEffectPrefab, shootEffectPos.position, Quaternion.LookRotation(-transform.forward));
        //记录时间
        lastShootTime = Time.time;
        //射击检测
        RaycastHit hit;
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, 15f))
        {
            Quaternion rotation = Quaternion.LookRotation(-transform.forward);
            if (hit.collider.CompareTag("Enemy"))
            {
                Instantiate(bloodEffectPrefab, hit.point, rotation/*Quaternion.identity*/);
                hit.transform.GetComponent<Enemy>().TakeDamage(weaponDamage[gunType]);
            }
            else
            {
                PlaySound(hitGroundSound);
                Instantiate(hitEffectPrefab, hit.point, rotation/*Quaternion.identity*/);
            }
        }
    }
    void RecoverShootState()
    {
        isReloading = false;
    }
    void Reload()
    {
        switch (gunType)
        {
            case GUNTYPE.SINGLE:
                if (bulletMagezine[gunType] >= bulletSingleMagezineMax)
                {
                    return;
                }
                break;
            case GUNTYPE.AUTO:
                if (bulletMagezine[gunType] >= bulletAutoMagezineMax)
                {
                    return;
                }
                break;
            case GUNTYPE.SNIPER:
                if (bulletMagezine[gunType] >= bulletSniperMagezineMax)
                {
                    return;
                }
                break;
            default:
                break;
        }
        if (bulletBag[gunType] > 0)
        {
            PlaySound(reloadSound);
            isReloading = true;
            Invoke("RecoverShootState", 2.667f);
            animator.SetTrigger("Reload");
            switch (gunType)
            {
                // 当枪支类型为单发时
                case GUNTYPE.SINGLE:
                    //弹药包的子弹数大于单发射击弹匣的最大值时
                    if (bulletBag[gunType] >= bulletSingleMagezineMax)
                    {
                        if (bulletMagezine[gunType] > 0)
                        {
                            int bulletNum = bulletSingleMagezineMax - bulletMagezine[gunType];
                            bulletBag[gunType] -= bulletNum;
                            bulletMagezine[gunType] += bulletNum;
                        }
                        else
                        {
                            // 从弹药包中减去单发子弹的最大值
                            bulletBag[gunType] -= bulletSingleMagezineMax;
                            // 将单发子弹的最大值添加到弹匣中
                            bulletMagezine[gunType] += bulletSingleMagezineMax;
                        }
                    }
                    //dagage包的子弹数小于单发射击弹匣的最大值时
                    else
                    {
                        // 从弹药包中减去所有子弹
                        bulletMagezine[gunType] += bulletBag[gunType];
                        bulletBag[gunType] = 0;
                    }
                    break;
                case GUNTYPE.AUTO:
                    if (bulletBag[gunType] >= bulletAutoMagezineMax)
                    {
                        if (bulletMagezine[gunType] > 0)
                        {
                            int bulletNum = bulletAutoMagezineMax - bulletMagezine[gunType];
                            bulletBag[gunType] -= bulletNum;
                            bulletMagezine[gunType] += bulletNum;
                        }
                        else
                        {
                            bulletBag[gunType] -= bulletAutoMagezineMax;
                            bulletMagezine[gunType] += bulletAutoMagezineMax;
                        }

                    }
                    else
                    {
                        bulletMagezine[gunType] += bulletBag[gunType];
                        bulletBag[gunType] = 0;
                    }
                    break;
                case GUNTYPE.SNIPER:
                    if (bulletBag[gunType] >= bulletSniperMagezineMax)
                    {
                        if (bulletMagezine[gunType] > 0)
                        {
                            int bulletNum = bulletSniperMagezineMax - bulletMagezine[gunType];
                            bulletBag[gunType] -= bulletNum;
                            bulletMagezine[gunType] += bulletNum;
                        }
                        else
                        {
                            bulletBag[gunType] -= bulletSniperMagezineMax;
                            bulletMagezine[gunType] += bulletSniperMagezineMax;
                        }
                    }
                    else
                    {
                        bulletMagezine[gunType] += bulletBag[gunType];
                        bulletBag[gunType] = 0;
                    }
                    break;
                default:
                    break;
            }
        }

    }
    void SingleShoot()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastShootTime >= shootCD)
        {
            if (bulletMagezine[gunType] > 0)
            {
                PlaySound(singleShootSound);
                bulletMagezine[gunType]--;
                GunShoot();
            }
            else
            {
                Reload();
            }
        }
    }
    void AutoShoot()
    {
        if (Input.GetMouseButton(0) && Time.time - lastShootTime >= shootCD)
        {
            if (bulletMagezine[gunType] > 0)
            {
                PlaySound(autoShootSound);
                bulletMagezine[gunType]--;
                GunShoot();
            }
            else
            {
                Reload();
            }
        }
    }
    void SniperShoot()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastShootTime >= shootCD)
        {
            if (bulletMagezine[gunType] > 0)
            {
                PlaySound(sniperShootSound);
                bulletMagezine[gunType]--;
                GunShoot();
            }
            else
            {
                Reload();
            }
        }
    }
    public void TakeDamage(int value)
    {
        bloodUIImage.SetActive(true);
        hp -= value;
        if (hp <= 0)
        {
            Die();
        }
        playerHpText.text = hp.ToString();
        Invoke("HideBloodUI", 0.5f);
    }
    void HideBloodUI()
    {
        bloodUIImage.SetActive(false);
    }
    void PlaySound(AudioClip audio)
    {
        sound.PlayOneShot(audio);
    }
    void Die()
    {
        hp = 0;
        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Replay()
    {
        SceneManager.LoadScene(0);
    }
}

enum GUNTYPE
{
    SINGLE,
    AUTO,
    SNIPER
}
