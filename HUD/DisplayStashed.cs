﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Matcha.Game.Tweens;


public class DisplayStashed : CacheBehaviour
{

    private float offset;
    public Sprite weapon;
    private Camera mainCamera;
    private SpriteRenderer HUDWeapon;

    void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        HUDWeapon = spriteRenderer;
        HUDWeapon.sprite = weapon;
        HUDWeapon.DOKill();
        FadeInWeapon();

        Invoke("PositionHUDElements", .01f);
        // HUDWeapon.color = new Color (1f, 1f, 1f, .3f);
    }

    void PositionHUDElements()
    {
        // shift to left or right, depending on name of GameObject
        offset = (name == "StashedWeapon_L") ? -HUD_STASHED_WEAPON_OFFSET : HUD_STASHED_WEAPON_OFFSET;

        transform.position = mainCamera.ScreenToWorldPoint(new Vector3(
            Screen.width / 2 + offset,
            Screen.height - HUD_WEAPON_TOP_MARGIN,
            HUD_Z));
    }

    void FadeInWeapon()
    {
        // fade weapon to zero instantly, then fade up slowly
        MTween.FadeOut(HUDWeapon, 0, 0);
        MTween.FadeIn(HUDWeapon, HUD_STASHED_TRANSPARENCY, HUD_FADE_IN_AFTER, HUD_TIME_TO_FADE);
    }

    void OnFadeHud(bool status)
    {
        MTween.FadeOut(HUDWeapon, HUD_FADE_OUT_AFTER, HUD_TIME_TO_FADE);
    }

    void OnScreenSizeChanged(float vExtent, float hExtent)
    {
        PositionHUDElements();
    }

    void OnEnable()
    {
        Messenger.AddListener<bool>("fade hud", OnFadeHud);
        Messenger.AddListener<float, float>( "screen size changed", OnScreenSizeChanged);
    }

    void OnDestroy()
    {
        Messenger.RemoveListener<bool>("fade hud", OnFadeHud);
        Messenger.RemoveListener<float, float>( "screen size changed", OnScreenSizeChanged);
    }
}