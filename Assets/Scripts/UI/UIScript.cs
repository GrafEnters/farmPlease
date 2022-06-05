﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class UIScript : MonoBehaviourSoundStarter
{
    public static UIScript instance;
    public Battery BatteryScript;
    public CoinsScript CoinsScript;
    public FastPanelScript FastPanelScript;
    public TimePanel TimePanel;
    public Backpack Backpack;
    public ShopsPanel ShopsPanel;

    public GameObject CroponomPanel, CroponomButton;
    public GameObject BuildingPanel;
    public GraphicRaycaster GraphicRaycaster;

    

    private void Awake()
    {
        instance = this;
    }

    public void ClosePanel()
    {
        if (SettingsManager.instance.SettingsPanel.gameObject.activeSelf)
            SettingsManager.instance.SettingsPanel.gameObject.SetActive(false);
        else if (CroponomPanel.activeSelf)
            CroponomPanel.SetActive(false);
        else if (TimePanel.isOpen)
            TimePanel.CalendarPanelOpenClose();
        else if (ShopsPanel.ToolShopPanel.gameObject.activeSelf)
            ShopsPanel.ToolShopPanel.gameObject.SetActive(false);
        else if (ShopsPanel.seedShopScript.gameObject.activeSelf)
            ShopsPanel.seedShopScript.gameObject.SetActive(false);
        else if (Backpack.isOpen)
            Backpack.OpenClose();

    }


    public void ChangeBattery(int amount)
    {
        BatteryScript.UpdateCharge(amount);
    }

    public void NoEnergy()
    {
        BatteryScript.NoEnergy();
    }


    public void ChangeCoins(int amount)
    {
        CoinsScript.UpdateCoins(amount);
    }

    public void UpdateBigCalendar()
    {
        TimePanel.UpdateBigCalendar(TimeManager.instance.day);
    }

    public void ChangeInventoryHover(int index)
    {
        FastPanelScript.UpdateHover(index);
    }

    public void ChangeFastPanel(CropsType crop, int amount)
    {
        FastPanelScript.UpdateSeedFastPanel(crop, amount);
    }

    public void OpenBuildingsShop()
    {
        ShopsPanel.BuildingShopButton.SetActive(true);
    }

    public void CloseBuildingsShop()
    {
        if (!GameModeManager.instance.IsBuildingsShopAlwaysOpen)
            ShopsPanel.BuildingShopButton.SetActive(false);
    }


    public void OpenMarketPlace()
    {
        ShopsPanel.ToolShopButton.GetComponent<Button>().interactable = true;
        ShopsPanel.SeedShopButton.GetComponent<Button>().interactable = false;
    }

    public void CloseMarketPlace()
    {
        ShopsPanel.ToolShopButton.GetComponent<Button>().interactable = false;
        ShopsPanel.SeedShopButton.GetComponent<Button>().interactable = true;
    }

    public void OpenSettings()
    {
        SettingsManager.instance.SettingsPanel.gameObject.SetActive(true);
    }


    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GlobalRecordsButton()
    {
        GPSManager.instance.ShowLeaderBoard();
    }

}
