﻿// Copyright (c) H. Ibrahim Penekli. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GameToolkit.Localization.Utilities {
    public class GoogleTranslator {
        private const string RequestUrlFormat = "https://translation.googleapis.com/language/translate/v2?key={0}";
        private const string RequestKeyInputText = "q";
        private const string RequestKeySourceLanguage = "source";
        private const string RequestKeyTargetLanguage = "target";

        public GoogleTranslator(TextAsset authFile) {
            AuthenticationFile = authFile;
        }

        /// <summary>
        ///     Gets or sets the google cloud API key.
        /// </summary>
        /// <seealso cref="https://cloud.google.com/docs/authentication/api-keys" />
        public TextAsset AuthenticationFile { get; set; }

        /// <summary>
        ///     Performs translation with given translate request asynchronous.
        /// </summary>
        /// <param name="request">Translate request.</param>
        /// <param name="onCompleted">Completed action.</param>
        /// <param name="onError">Error action.</param>
        public IEnumerator TranslateAsync(GoogleTranslateRequest request,
            Action<TranslationCompletedEventArgs> onCompleted = null,
            Action<TranslationErrorEventArgs> onError = null) {
            using (UnityWebRequest www = PrepareRequest(request)) {
#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif
                ProcessResponse(request, www, onCompleted, onError);
            }
        }

        /// <summary>
        ///     Useful for Editor scripts. Otherwise, recommended to use <see cref="TranslateAsync" />.
        /// </summary>
        /// <param name="request">Translate request.</param>
        /// <param name="onCompleted">Completed action.</param>
        /// <param name="onError">Error action.</param>
        public void Translate(GoogleTranslateRequest request,
            Action<TranslationCompletedEventArgs> onCompleted = null,
            Action<TranslationErrorEventArgs> onError = null) {
            using (UnityWebRequest www = PrepareRequest(request)) {
#if UNITY_2017_2_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif

                // Wait request completion.
                while (!www.isDone) {
                }

                ProcessResponse(request, www, onCompleted, onError);
            }
        }

        private UnityWebRequest PrepareRequest(GoogleTranslateRequest request) {
            if (request == null) throw new ArgumentNullException("request");

            if (!AuthenticationFile) throw new ArgumentNullException("AuthenticationFile", "Auth file not attached.");

            WWWForm form = new WWWForm();
            form.AddField(RequestKeyInputText, request.Text);
            form.AddField(RequestKeySourceLanguage, Localization.GetLanguageCode(request.Source));
            form.AddField(RequestKeyTargetLanguage, Localization.GetLanguageCode(request.Target));

            string url = string.Format(RequestUrlFormat, AuthenticationFile.text);
            return UnityWebRequest.Post(url, form);
        }

        private void ProcessResponse(GoogleTranslateRequest request, UnityWebRequest www,
            Action<TranslationCompletedEventArgs> onCompleted,
            Action<TranslationErrorEventArgs> onError) {
            if (www.isNetworkError || www.isHttpError) {
                if (onError != null) onError.Invoke(new TranslationErrorEventArgs(www.error));
            } else {
                JsonResponse response = JsonUtility.FromJson<JsonResponse>(www.downloadHandler.text);
                if (response != null && response.data != null && response.data.translations != null &&
                    response.data.translations.Length > 0) {
                    GoogleTranslateRequest[] requests = new[] {request};

                    GoogleTranslateResponse translateResponse = new GoogleTranslateResponse();
                    translateResponse.TranslatedText = response.data.translations[0].translatedText;
                    GoogleTranslateResponse[] responses = new[] {translateResponse};

                    if (onCompleted != null) onCompleted.Invoke(new TranslationCompletedEventArgs(requests, responses));
                } else {
                    if (onError != null)
                        onError.Invoke(new TranslationErrorEventArgs("Response data could not be read."));
                }
            }
        }

        [Serializable]
        private class JsonResponse {
            public JsonData data;
        }

        [Serializable]
        private class JsonData {
            public JsonTranslation[] translations;
        }

        [Serializable]
        private class JsonTranslation {
            public string translatedText = "";
            public string detectedSourceLanguage = "";
        }
    }

    /// <summary>
    ///     Provides the requests and translation responses.
    /// </summary>
    public class TranslationCompletedEventArgs : EventArgs {
        public TranslationCompletedEventArgs(GoogleTranslateRequest[] requests,
            GoogleTranslateResponse[] responses) {
            Debug.Assert(requests != null);
            Debug.Assert(responses != null);
            Requests = requests;
            Responses = responses;
        }

        /// <summary>
        ///     Translate requests.
        /// </summary>
        public GoogleTranslateRequest[] Requests { get; }

        /// <summary>
        ///     Translate responses.
        /// </summary>
        public GoogleTranslateResponse[] Responses { get; }
    }

    /// <summary>
    ///     Provides detailed information upon translation errors.
    /// </summary>
    public class TranslationErrorEventArgs : EventArgs {
        public TranslationErrorEventArgs(string message) {
            Message = message;
        }

        /// <summary>
        ///     Error message.
        /// </summary>
        public string Message { get; }
    }
}