﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TimeManager : MonoBehaviour {
    public static TimeManager instance;
    public int day;
    public int DayOfWeek;
    public int time;

    [Range(1, 31)]
    public int MaxDays;

    [Range(0, 7)]
    public int SkipDaysAmount;

    public EndTrainingPanel EndTrainingPanel;
    public EndMonthPanel EndMonthPanel;

    public GameObject HelloPanel;
    public Text HelloText;
    public HappeningType[] daysHappenings;

    public float SessionTime;
    public bool isLoaded;

    private FastPanelScript FastPanel;
    private bool IsTimerWorking;
    private PlayerController PlayerController;
    private SeedShopScript SeedShopScript;
    private SmartTilemap SmartTilemap;
    private Text TimeOfDayText;
    private TimePanel TimePanel;
    public DateTime tmpDate;
    private ToolShopPanel ToolShop;

    private UIScript UIScript;
    /**********/

    public void Awake() {
        if (instance == null) {
            instance = this;
            isLoaded = false;
            IsTimerWorking = false;
        } else if (instance == this) {
            Destroy(gameObject);
        }
    }

    private void Update() {
        SessionTime += Time.deltaTime;
    }

    public void Init() {
        UIScript = UIScript.instance;
        PlayerController = PlayerController.instance;
        SmartTilemap = SmartTilemap.instance;
        SeedShopScript = UIScript.ShopsPanel.seedShopScript;
        ToolShop = UIScript.ShopsPanel.ToolShopPanel;
        FastPanel = PlayerController.GetComponent<FastPanelScript>();
        TimePanel = UIScript.TimePanel;
        TimeOfDayText = TimePanel.TimeOfDayText;

        SessionTime = 0;
    }

    private IEnumerator IngameTimer() {
        IsTimerWorking = true;
        if (GameModeManager.instance.GameMode == GameMode.Training)
            yield break;

        for (;;) {
            if (isLoaded) {
                yield return CalculateTimeSpanCoroutine(tmpDate);
                tmpDate = DateTime.Now;
            }

            //Обновление времени в интерфейсе
            int minutesOfDay = RealTImeManager.TotalMinutes();
            TimeOfDayText.text = minutesOfDay / 60 + ":" + (minutesOfDay % 60 < 10 ? "0" : "") + minutesOfDay % 60;

            yield return new WaitForSeconds(3);
        }
    }

    /**********/

    public string[] GetDaysData() {
        string[] res = new string[daysHappenings.Length];
        for (int i = 0; i < res.Length; i++)
            res[i] = daysHappenings[i].ToString();

        return res;
    }

    public void CalculateTimeSpan(DateTime tmpDate) {
        StartCoroutine(CalculateTimeSpanCoroutine(tmpDate));
    }

    public IEnumerator CalculateTimeSpanCoroutine(DateTime tmpDate) {
        int daysGone = RealTImeManager.DaysGone(tmpDate);

        if (RealTImeManager.IsRefillingEnergy(tmpDate)) {
            PlayerController.RestoreEnergy();
            HelloPanel.SetActive(true);
            HelloText.text = "Здравствуй Фермер, энергия обновлена\n" + "до обновления энергии осталось " +
                             RealTImeManager.TimeToString(RealTImeManager.MinutesToEnergyRefill());
        }

        if (daysGone > 0) {
            SaveLoadManager.instance.Sequence(true);

            for (int i = 0; i < daysGone; i++)
                yield return DayPointCoroutine();

            SaveLoadManager.instance.Sequence(false);
        }
    }

    public void SetDaysWithData(string[] daysData, DateTime date) {
        ChangeDayPoint(SettingsManager.instance.GetDayPoint());

        if (daysData == null) {
            GenerateDays(GameModeManager.instance.GameMode == GameMode.Training, false);
            return;
        }

        MaxDays = daysData.Length;
        daysHappenings = new HappeningType[MaxDays];
        day = date.Day - 1;
        DayOfWeek = (int) date.DayOfWeek - 1;
        SkipDaysAmount = FirstDayInMonth(date.Year, date.Month);

        for (int i = 0; i < MaxDays; i++)
            daysHappenings[i] = (HappeningType) Enum.Parse(typeof(HappeningType), daysData[i]);

        TimePanel.CreateDays(daysHappenings, SkipDaysAmount);
        TimePanel.UpdateLilCalendar(day);

        if (GameModeManager.instance.GameMode != GameMode.Training) {
            if (daysHappenings[day] == HappeningType.Marketplace) {
                UIScript.OpenBuildingsShop();
            } else {
                UIScript.CloseBuildingsShop();
            }
        }

        tmpDate = date;
        isLoaded = true;
        StartCoroutine(IngameTimer());
    }

    public void GenerateDays(bool isTraining, bool isNewGame) {
        if (isTraining) {
            MaxDays = 31;
            SkipDaysAmount = 0;
        } else {
            DateTime date = DateTime.Now;
            MaxDays = DateTime.DaysInMonth(date.Year, date.Month);
            SkipDaysAmount = FirstDayInMonth(date.Year, date.Month);
            ChangeDayPoint(SettingsManager.instance.GetDayPoint());
        }

        day = 0;
        if (isNewGame && !isTraining)
            day = DateTime.Now.Day - 1;
        daysHappenings = new HappeningType[MaxDays];

        int love = -1;
        while ((love + SkipDaysAmount + 1) % 7 == 0 || (love + 1) % 5 == 0) love = Random.Range(8 + SkipDaysAmount, 20);

        for (int i = 0; i < MaxDays; i++) {
            int x = i + SkipDaysAmount + 1;
            if (x % 7 == 0 && x > 0 && GameModeManager.instance.GameMode != GameMode.Training) {
                daysHappenings[i] = HappeningType.Marketplace;
            } else if ((i + 1) % 5 == 0) {
                int rnd = Random.Range(0, 4);
                if (GameModeManager.instance.GameMode == GameMode.Training)
                    rnd = Random.Range(0, 2);

                daysHappenings[i] = rnd switch {
                    0 => HappeningType.Rain,
                    1 => HappeningType.Erosion,
                    2 => HappeningType.Wind,
                    3 => HappeningType.Insects,
                    _ => daysHappenings[i]
                };
            } else if (i == love) {
                daysHappenings[i] = HappeningType.Love;
            }
        }

        TimePanel.CreateDays(daysHappenings, SkipDaysAmount);

        TimePanel.UpdateLilCalendar(day);
        ToolShop.ChangeTools();

        if (GameModeManager.instance.GameMode != GameMode.Training) {
            if (daysHappenings[day] == HappeningType.Marketplace) {
                UIScript.OpenBuildingsShop();
            } else {
                UIScript.CloseBuildingsShop();
            }
        }

        isLoaded = true;
        tmpDate = DateTime.Now;

        if (!IsTimerWorking)
            StartCoroutine(IngameTimer());
    }

    /***********/

    public void AddDay() {
        StartCoroutine(DayPointCoroutine());
        PlayerController.RestoreEnergy();
    }

    public IEnumerator DayPointCoroutine() {
        day++;

        DayOfWeek = NextDay(DayOfWeek);
        if (day == MaxDays)
            EndMonth();

        SeedShopScript.ChangeSeedsNewDay();

        TimePanel.UpdateLilCalendar(day);

        InventoryManager.instance.BrokeTools();
        FastPanel.UpdateToolsImages();
        ToolShop.ChangeTools();

        if (GameModeManager.instance.GameMode != GameMode.Training) {
            if (daysHappenings[day] == HappeningType.Marketplace) {
                UIScript.OpenBuildingsShop();
            } else {
                UIScript.CloseBuildingsShop();
            }
        }

        if (day > 0)
            yield return StartCoroutine(SmartTilemap.EndDayEvent(daysHappenings[day - 1]));
        yield return StartCoroutine(SmartTilemap.NewDay());
    }

    public void SkipToEndMonth() {
        DayOfWeek = NextDay(LastDayInMonth(day, MaxDays, DayOfWeek));
        EndMonth();
    }

    private void EndMonth() {
        InventoryManager.instance.toolsInventory[ToolType.Weatherometr] = 0;
        GenerateDays(false, false);

        if (GameModeManager.instance.GameMode == GameMode.Training) {
            EndTrainingPanel.ShowEndPanel(InventoryManager.instance.cropsCollected, (int) SessionTime,
                InventoryManager.instance.cropsCollectedQueue);
            GPSManager.ReportScore(InventoryManager.instance.AllCropsCollected, "tutorialLeaderboard");
        } else {
            EndMonthPanel.ShowEndMonthPanel(InventoryManager.instance.cropsCollectedQueue,
                InventoryManager.instance.AllCropsCollected);
            InventoryManager.instance.cropsCollectedQueue = new Queue<CropsType>();
        }
    }

    public void ChangeDayPoint(TimeSpan timespan) {
    }

    private int NextDay(int currentDayOfWeek) {
        int next = (currentDayOfWeek + 1) % 7;
        return next;
    }

    private int FirstDayInMonth(int year, int month) {
        DateTime date = new(year, month, 1);
        return (int) date.DayOfWeek - 1;
    }

    private int LastDayInMonth(int currentday, int maxDays, int currentDayofWeek) {
        int tmp = currentday % 7; // какой сейчас должен был быть день недели, если бы начиналось с понедельника
        int tmp2 = currentDayofWeek - tmp;
        int tmp3 = (maxDays % 7 + tmp2 - 1) % 7;

        return tmp3;
    }
}