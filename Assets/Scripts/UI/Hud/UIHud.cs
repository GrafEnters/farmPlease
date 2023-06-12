﻿using Abstract;
using Managers;
using Tables;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZhukovskyGamesPlugin;

namespace UI
{
    public class UIHud : Singleton<UIHud>, ISoundStarter {
        public BatteryView BatteryView;
        [FormerlySerializedAs("CoinsScript")] public CoinsView coinsView;
        public FastPanelScript FastPanelScript;
        public TimePanel TimePanel;
        public Backpack Backpack;
        public ShopsPanel ShopsPanel;
        public HelloPanelView HelloPanel;

        public GameObject CroponomButton;
        public GameObject BuildingPanel;
        public GraphicRaycaster GraphicRaycaster;

        public ClockView ClockView;
        public SpotlightWithText SpotlightWithText;
        public KnowledgeCanSpeak KnowledgeCanSpeak;

        public void ClosePanel() {
            if (Settings.Instance.SettingsPanel.gameObject.activeSelf)
                Settings.Instance.SettingsPanel.gameObject.SetActive(false);
            else if (TimePanel.isOpen)
                TimePanel.CalendarPanelOpenClose();
            else if (ShopsPanel.toolShopView.gameObject.activeSelf)
                ShopsPanel.toolShopView.gameObject.SetActive(false);
            else if (ShopsPanel.seedShopView.gameObject.activeSelf)
                ShopsPanel.seedShopView.gameObject.SetActive(false);
            else if (Backpack.isOpen)
                Backpack.OpenClose();
        }

        public void SetBattery(int amount) {
            BatteryView.UpdateCharge(amount);
        }

        public void NoEnergy() {
            BatteryView.NoEnergy();
        }

        public void ChangeCoins(int amount) {
            coinsView.UpdateCoins(amount);
        }

        public void ChangeInventoryHover(int index) {
            FastPanelScript.UpdateHover(index);
        }

        public void ChangeFastPanel(Crop crop, int amount) {
            FastPanelScript.UpdateSeedFastPanel(crop, amount);
        }

        public void OpenBuildingsShop() {
            ShopsPanel.BuildingShopButton.interactable = true;
        }

        public void CloseBuildingsShop() {
            if (!GameModeManager.Instance.IsBuildingsShopAlwaysOpen)
                ShopsPanel.BuildingShopButton.interactable = false;
        }

        public void OpenMarketPlace() {
            ShopsPanel.ToolShopButton.GetComponent<Button>().interactable = true;
            ShopsPanel.SeedShopButton.GetComponent<Button>().interactable = false;
        }

        public void CloseMarketPlace() {
            ShopsPanel.ToolShopButton.GetComponent<Button>().interactable = false;
            ShopsPanel.SeedShopButton.GetComponent<Button>().interactable = true;
        }

        public void OpenCroponom() {
            Croponom.Instance.Open();
        }

        public void OpenSettings() {
            Settings.Instance.SettingsPanel.gameObject.SetActive(true);
        }

        public void LoadLevel(string sceneName) {
            SceneManager.LoadScene(sceneName);
        }

        public void GlobalRecordsButton() {
            GpsManager.Instance.ShowLeaderBoard();
        }
    }
}