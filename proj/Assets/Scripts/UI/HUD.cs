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

    public CanvasGroup masterGroup;

    [Space]
    public GameObject interactIcon;

    [Space]
    public Image interactCooldownFill;

    [Space]
    public CanvasGroup injuredVignette;

    //private static float stamina = 1;

    static float maxInteractCooldown;
    static float interactCooldown;

    public bool HUDVisible = true;

    private void Start()
    {
        interactIcon.SetActive(false);
        injuredVignette.alpha = 0;
    }

    private void Update()
    {
        masterGroup.alpha = HUDVisible ? 1 : 0;

        interactCooldown -= Time.deltaTime;

        interactCooldownFill.fillAmount = Remap.Float(Mathf.Clamp(interactCooldown, 0, maxInteractCooldown), 0, maxInteractCooldown, 0, 1);
        interactCooldownFill.gameObject.SetActive(interactCooldown > 0);
    }


    public static void SetInteract(bool enabled)
    {
        instance.interactIcon.SetActive(enabled);
    }

    public static void SetInteractCooldown(float cooldown)
    {
        maxInteractCooldown = cooldown;
        interactCooldown = cooldown;
    }
}
