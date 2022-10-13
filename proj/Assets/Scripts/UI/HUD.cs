using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private static HUD instance;
    
    private void Awake()
    {
        instance = this;
    }

    /*
    public CanvasGroup masterGroup;

    [Space]
    public GameObject interactIcon;

    [Space]
    public Image interactCooldownFill;

    [Space]
    public CanvasGroup injuredVignette;
    */
    [Space]
    public CanvasGroup suppressedVignette;
    public float spFadeSpeed = 5;
    public float spFadePickup = 2;
    public float spFade = 0;

    //private static float stamina = 1;

    //static float maxInteractCooldown;
    //static float interactCooldown;

    public bool HUDVisible = true;

    private void Start()
    {
        //interactIcon.SetActive(false);
        //injuredVignette.alpha = 0;
        spFade = 0;
    }

    private void Update()
    {
        //masterGroup.alpha = HUDVisible ? 1 : 0;

        //interactCooldown -= Time.deltaTime;

        //interactCooldownFill.fillAmount = Remap.Float(Mathf.Clamp(interactCooldown, 0, maxInteractCooldown), 0, maxInteractCooldown, 0, 1);
        //interactCooldownFill.gameObject.SetActive(interactCooldown > 0);

        spFade += spFadePickup * Time.deltaTime;
        suppressedVignette.alpha = Mathf.Lerp(suppressedVignette.alpha, 0, Time.deltaTime * spFadeSpeed * spFadePickup);
    }

    /*
    public static void SetInteract(bool enabled)
    {
        instance.interactIcon.SetActive(enabled);
    }

    public static void SetInteractCooldown(float cooldown)
    {
        maxInteractCooldown = cooldown;
        interactCooldown = cooldown;
    }
    */

    public static void SetInteract(bool enabled) { }

    public static void SetInteractCooldown(float cooldown) { }

    public static void AddSuppressedVignette(float amountToAdd01)
    {
        instance.suppressedVignette.alpha += amountToAdd01;
        instance.spFade = 0;
    }
}
