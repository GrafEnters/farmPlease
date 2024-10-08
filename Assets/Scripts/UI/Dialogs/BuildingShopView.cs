﻿using System.Collections.Generic;
using Abstract;
using Managers;
using ScriptableObjects;
using Tables;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuildingShopView : MonoBehaviour, ISoundStarter {
        public Text cropsCollectedText;

        [Header("Buildings")]
        public GameObject BuildingPanelButton;

        public int[] buildingPriceProgression;
        public int cropPrice;
        public int toolPrice;

        public Transform BuildingsGrid;
        public Transform ToolsGrid;
        public Transform CropsGrid;

        public GameObject BuildingPrefab;
        public GameObject ToolPrefab;
        public GameObject CropPrefab;

        [Header("Confirm Page")]
        [SerializeField] 
        private GameObject _confirmPage;
        [SerializeField] 
        private GameObject _notChoosedPage;

        public Button ConfirmButton;
        public Text nameText;
        public Text explanationText;
        public Text costText;
        public Image confirmImage;
        private Dictionary<BuildingType, Button> _buildingButtonsD;

        private Dictionary<Crop, Button> _cropButtonsD;

        private int _currentBuildingPrice;
        private Dictionary<ToolBuff, Button> _toolButtonsD;

        public void InitializeWithData(int buildingPrice) {
            _currentBuildingPrice = buildingPrice;

            //Если куплена хотя бы 1 постройка - то кнопка перестраивания построек становится активной
            BuildingPanelButton.SetActive(false);
            foreach (bool build in InventoryManager.Instance.IsBuildingsBoughtD.Values)
                if (build) {
                    BuildingPanelButton.SetActive(true);
                    break;
                }

            GenerateButtons();
            UpdateButtonsInteractable();
        }
        
        public void GenerateButtons() {
            GenerateBuildingsButtons();
            GenerateCropsButtons();
            GenerateToolsButtons();
        }

        private void GenerateToolsButtons() {
            ToolConfig[] tools = ToolsTable.Instance.ToolsSO;
            _toolButtonsD = new Dictionary<ToolBuff, Button>();

            for (int i = 0; i < tools.Length; i++) {
                if (tools[i].isAlwaysAvailable)
                    continue;

                GameObject obj = Instantiate(ToolPrefab, ToolsGrid);
                obj.SetActive(true);
                FoodMarketOffer offer = obj.GetComponent<FoodMarketOffer>();
                offer.name.text = tools[i].header;
                offer.image.sprite = tools[i].FoodMarketSprite;

                Button button = obj.GetComponent<Button>();

                ToolBuff tmpBuff = tools[i].buff;
                button.onClick.AddListener(() => OpenConfirmPage(tmpBuff));
                _toolButtonsD.Add(tmpBuff, button);
            }
        }

        private void GenerateCropsButtons() {
            CropConfig[] crops = CropsTable.Instance.Crops;
            _cropButtonsD = new Dictionary<Crop, Button>();

            for (int i = 0; i < crops.Length; i++) {
                if (crops[i].CanBeBought || crops[i].type == Crop.Weed)
                    continue;

                GameObject obj = Instantiate(CropPrefab, CropsGrid);
                obj.SetActive(true);
                Button button = obj.GetComponent<Button>();

                FoodMarketOffer offer = obj.GetComponent<FoodMarketOffer>();
                offer.name.text = crops[i].header;
                offer.image.sprite = crops[i].VegSprite;

                Crop tmp = crops[i].type;
                button.onClick.AddListener(() => OpenConfirmPage(tmp));

                _cropButtonsD.Add(tmp, button);
            }
        }

        private void GenerateBuildingsButtons() {
            BuildingConfig[] buildings = BuildingsTable.Instance.Buildings;
            _buildingButtonsD = new Dictionary<BuildingType, Button>();

            for (int i = 0; i < buildings.Length; i++) {
                if (buildings[i].IsFakeBuilding)
                    continue;
                GameObject offerObject = Instantiate(BuildingPrefab, BuildingsGrid);
                offerObject.SetActive(true);
                FoodMarketOffer offer = offerObject.GetComponent<FoodMarketOffer>();
                offer.name.text = buildings[i].offerHeader;
                offer.image.sprite = buildings[i].offerSprite;
                BuildingType type = buildings[i].type;
                Button button = offer.GetComponent<Button>();
                _buildingButtonsD.Add(type, button);

                button.onClick.AddListener(() => OpenConfirmPage(type));
            }
        }

        public int GetBuildingPrice() {
            return _currentBuildingPrice;
        }

        public void UpdateButtonsInteractable() {
            foreach (Crop item in _cropButtonsD.Keys)
                if (InventoryManager.Instance.IsCropsBoughtD.ContainsKey(item))
                    _cropButtonsD[item].interactable = !InventoryManager.Instance.IsCropsBoughtD[item];
                else
                    _cropButtonsD[item].interactable = true;

            foreach (ToolBuff item in _toolButtonsD.Keys)
                if (InventoryManager.Instance.IsToolsBoughtD.ContainsKey(item))
                    _toolButtonsD[item].interactable = !InventoryManager.Instance.IsToolsBoughtD[item];
                else
                    _toolButtonsD[item].interactable = true;

            foreach (BuildingType item in _buildingButtonsD.Keys)
                if (InventoryManager.Instance.IsBuildingsBoughtD.ContainsKey(item))
                    _buildingButtonsD[item].interactable = !InventoryManager.Instance.IsBuildingsBoughtD[item];
                else
                    _buildingButtonsD[item].interactable = true;
        }

        public void UpdateCropsCollected() {
            cropsCollectedText.text = SaveLoadManager.CurrentSave.CropPoints.ToString();
        }

        private void OpenConfirmPage(Crop type) {
            ConfirmButton.onClick.RemoveAllListeners();
            if (InventoryManager.Instance.EnoughCrops(cropPrice)) {
                ConfirmButton.onClick.AddListener(() => BuyCropButton(type));
                ConfirmButton.interactable = true;
            } else {
                ConfirmButton.interactable = false;
            }

            CropConfig crop = CropsTable.CropByType(type);

            confirmImage.sprite = crop.VegSprite;
            nameText.text = crop.header;
            explanationText.text = crop.explainText;
            costText.text = "Открыть за " + cropPrice;

            SetConfirmPageActive(true);
        }

        private void OpenConfirmPage(ToolBuff buff) {
            ConfirmButton.onClick.RemoveAllListeners();
            if (InventoryManager.Instance.EnoughCrops(toolPrice)) {
                ConfirmButton.onClick.AddListener(() => BuyToolButton(buff));
                ConfirmButton.interactable = true;
            } else {
                ConfirmButton.interactable = false;
            }

            ToolConfig tool = ToolsTable.ToolByType(buff);

            confirmImage.sprite = tool.FoodMarketSprite;
            nameText.text = tool.header;
            explanationText.text = tool.explainText;
            costText.text = "Открыть за " + toolPrice;

            SetConfirmPageActive(true);
        }

        private void OpenConfirmPage(BuildingType type) {
            ConfirmButton.onClick.RemoveAllListeners();
            if (InventoryManager.Instance.EnoughCrops(buildingPriceProgression[_currentBuildingPrice])) {
                ConfirmButton.onClick.AddListener(() => StartBuyingBuilding(type));
                ConfirmButton.interactable = true;
            } else {
                ConfirmButton.interactable = false;
            }

            BuildingConfig building = BuildingsTable.BuildingByType(type);

            confirmImage.sprite = building.offerSprite;
            nameText.text = building.offerHeader;
            explanationText.text = building.offerText;
            costText.text = "Открыть за " + buildingPriceProgression[_currentBuildingPrice];

            SetConfirmPageActive(true);
        }

        public void StartBuyingBuilding(BuildingType type) {
            PlayerController.Instance.StartStopBuilding();
            PlayerController.Instance.InitializeBuilding(type, buildingPriceProgression[_currentBuildingPrice]);
            SetConfirmPageActive(false);
            gameObject.SetActive(false);
        }

        public void BuyCropButton(Crop type) {
            InventoryManager.Instance.BuyFoodMarket(type, cropPrice);
            UpdateButtonsInteractable();
            SetConfirmPageActive(false);
        }

        public void BuyToolButton(ToolBuff buff) {
            InventoryManager.Instance.BuyFoodMarket(buff, toolPrice);

            UpdateButtonsInteractable();
            SetConfirmPageActive(false);
        }

        public void BuyBuildingButton(BuildingType type) {
            InventoryManager.Instance.BuyFoodMarket(type, buildingPriceProgression[_currentBuildingPrice]);
            _currentBuildingPrice++;

            UpdateButtonsInteractable();
            SetConfirmPageActive(false);
            BuildingPanelButton.SetActive(true);
        }

        public void SetConfirmPageActive(bool isActive) {
            _confirmPage.SetActive(isActive);
            _notChoosedPage.SetActive(!isActive);
        }
    }
}