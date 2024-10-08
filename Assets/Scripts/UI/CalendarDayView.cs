﻿using Tables;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CalendarDayView : MonoBehaviour {
        public Image Date;
        public Image Happening;
        public GameObject Finished;

        public Sprite[] DateSprites;

        public void Clear() {
            Date.gameObject.SetActive(false);
            Happening.gameObject.SetActive(false);
            Finished.gameObject.SetActive(false);
        }

        public void DayOver() {
            Finished.SetActive(true);
            Date.color = new Color(1, 1, 1, 0.5f);
            Happening.color = new Color(1, 1, 1, 0.5f);
        }

        public void SetProps(int dayNumber, HappeningType type, bool showDefault = false) {
            if (dayNumber == -1) {
                Clear();
                return;
            }

            Date.sprite = DateSprites[dayNumber];
            SetHappening(type, showDefault);
        }

        public void SetProps(CalendarDayView calendarDayView) {
            Date.sprite = calendarDayView.Date.sprite;
            Happening.sprite = calendarDayView.Happening.sprite;
        }

        public void SetHappening(HappeningType type, bool showDefault = false) {
            Happening.gameObject.SetActive(true);
            Happening.sprite = WeatherTable.WeatherByType(type).DaySprite;

            if (type == HappeningType.None && !showDefault) {
                Happening.gameObject.SetActive(false);
                return;
            }

            Happening.gameObject.SetActive(true);
            Happening.sprite = WeatherTable.WeatherByType(type).DaySprite;
        }
    }
}