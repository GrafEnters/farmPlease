using System;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Events;
using GooglePlayGames.OurUtils;
using UnityEngine;
using Event = GooglePlayGames.BasicApi.Events.Event;

#if UNITY_ANDROID
namespace GooglePlayGames.Android {
    internal class AndroidEventsClient : IEventsClient {
        private volatile AndroidJavaObject mEventsClient;

        public AndroidEventsClient() {
            using (AndroidJavaClass gamesClass = new AndroidJavaClass("com.google.android.gms.games.PlayGames")) {
                mEventsClient = gamesClass.CallStatic<AndroidJavaObject>("getEventsClient",
                    AndroidHelperFragment.GetActivity());
            }
        }

        public void FetchAllEvents(DataSource source, Action<ResponseStatus, List<IEvent>> callback) {
            callback = ToOnGameThread(callback);
            using (AndroidJavaObject task =
                   mEventsClient.Call<AndroidJavaObject>("load", source == DataSource.ReadNetworkOnly ? true : false)) {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData => {
                        using (AndroidJavaObject buffer = annotatedData.Call<AndroidJavaObject>("get")) {
                            int count = buffer.Call<int>("getCount");
                            List<IEvent> result = new();
                            for (int i = 0; i < count; ++i)
                                using (AndroidJavaObject eventJava = buffer.Call<AndroidJavaObject>("get", i)) {
                                    result.Add(CreateEvent(eventJava));
                                }

                            buffer.Call("release");
                            callback.Invoke(
                                annotatedData.Call<bool>("isStale")
                                    ? ResponseStatus.SuccessWithStale
                                    : ResponseStatus.Success,
                                result
                            );
                        }
                    });
                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => {
                        Debug.Log("FetchAllEvents failed");
                        callback.Invoke(ResponseStatus.InternalError, null);
                    });
            }
        }

        public void FetchEvent(DataSource source, string eventId, Action<ResponseStatus, IEvent> callback) {
            callback = ToOnGameThread(callback);
            string[] ids = new string[1];
            ids[0] = eventId;
            using (AndroidJavaObject task = mEventsClient.Call<AndroidJavaObject>("loadByIds",
                       source == DataSource.ReadNetworkOnly ? true : false, ids)) {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData => {
                        using (AndroidJavaObject buffer = annotatedData.Call<AndroidJavaObject>("get")) {
                            int count = buffer.Call<int>("getCount");
                            if (count > 0)
                                using (AndroidJavaObject eventJava = buffer.Call<AndroidJavaObject>("get", 0)) {
                                    callback.Invoke(
                                        annotatedData.Call<bool>("isStale")
                                            ? ResponseStatus.SuccessWithStale
                                            : ResponseStatus.Success,
                                        CreateEvent(eventJava)
                                    );
                                }
                            else
                                callback.Invoke(
                                    annotatedData.Call<bool>("isStale")
                                        ? ResponseStatus.SuccessWithStale
                                        : ResponseStatus.Success,
                                    null
                                );

                            buffer.Call("release");
                        }
                    });
                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => {
                        Debug.Log("FetchEvent failed");
                        callback.Invoke(ResponseStatus.InternalError, null);
                    });
            }
        }

        public void IncrementEvent(string eventId, uint stepsToIncrement) {
            mEventsClient.Call("increment", eventId, (int) stepsToIncrement);
        }

        private static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert) {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        private static Event CreateEvent(AndroidJavaObject eventJava) {
            string id = eventJava.Call<string>("getEventId");
            string name = eventJava.Call<string>("getName");
            string description = eventJava.Call<string>("getDescription");
            string imageUrl = eventJava.Call<string>("getIconImageUrl");
            ulong currentCount = (ulong) eventJava.Call<long>("getValue");
            EventVisibility visibility = eventJava.Call<bool>("isVisible")
                ? EventVisibility.Revealed
                : EventVisibility.Hidden;
            return new Event(id, name, description, imageUrl, currentCount, visibility);
        }
    }
}
#endif