using System;
using System.Collections;
using Abstract;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SpotlightWithText : HasAnimationAndCallback {
        [SerializeField]
        protected TextMeshProUGUI _hintText;

        
        [SerializeField]
        private CanvasGroup _centerImageCanvasGroup;

        [SerializeField]
        protected RectTransform _shadowTransform, _shadowCenter, _headShift, _textBubble, _headFinPosAnchor;


        [SerializeField]
        protected CanvasGroup _textBubbleCanvas;
        private const string SHOW = "SpotlightShow";
        private const string HIDE = "SpotlightHide";

        private Button _target;
        private Action _onButtonPressed;
        private bool _isHidingByAnyTap;
        private bool _isShown;
        private bool _isHidingAfter;

        public void ShowSpotlightOnButton(Button target, SpotlightAnimConfig animDataConfig, Action onButtonPressed = null,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);
            if (_isShown) {
                StartCoroutine(JumpSpotlight(target.transform.position, animDataConfig));
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            _target = target;
            target.onClick.AddListener(OnTargetButtonPressed);
            OnAnimationEnded = onButtonPressed;
            _isHidingByAnyTap = false;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        public void ShowSpotlight(Transform target, SpotlightAnimConfig animDataConfig, Action onHideEnded = null, bool isHidingByAnyTap = true,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            if (_isShown) {
                StartCoroutine(JumpSpotlight(target.transform.position, animDataConfig));
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            OnAnimationEnded = onHideEnded;
            _isHidingByAnyTap = isHidingByAnyTap;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        public void MoveHead(Transform target, SpotlightAnimConfig animDataConfig, Action onHideEnded = null, bool isHidingByAnyTap = true,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            if (_isShown) {
                StartCoroutine(MoveHeadOnly(animDataConfig));
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            OnAnimationEnded = onHideEnded;
            _isHidingByAnyTap = isHidingByAnyTap;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }
        public void JumpSpotlightFromVeryBigOnButton(Button target, SpotlightAnimConfig animDataConfig, Action onButtonPressed = null,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            JumpSpotlightFromVeryBig(target.transform, animDataConfig);

            _target = target;
            target.onClick.AddListener(OnTargetButtonPressed);
            OnAnimationEnded = onButtonPressed;
            _isHidingByAnyTap = false;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        private void JumpSpotlightFromVeryBig(Transform target, SpotlightAnimConfig animDataConfig) {
            _headFinPosAnchor.position = target.transform.position;
            _shadowTransform.position = target.transform.position;
            _hintText.text = animDataConfig.HintText;
            StartCoroutine(JumpSpotlightEnd( animDataConfig));
        }

        private void OnTargetButtonPressed() {
            if (_isHidingAfter) {
                HideSpotlight();
            } else {
                OnAnimationEnded?.Invoke();
            }

            _target.onClick.RemoveListener(OnTargetButtonPressed);
            _target = null;
        }

        private void ShowSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
            _textBubbleCanvas.alpha = 1;
            _shadowTransform.position = targetPos;
            _headFinPosAnchor.position = targetPos;
            _headShift.anchoredPosition = _shadowTransform.anchoredPosition + config.HeadShift;
            _shadowCenter.sizeDelta = config.SpotlightSize;
            _hintText.text = config.HintText;
            _animation.Play(SHOW);
            _isShown = true;
        }

        private void ChangeCenterBlockRaycast(bool isBlock) {
            _centerImageCanvasGroup.blocksRaycasts = isBlock;
        }

        public void Hide() {
            if (_isHidingAfter) {
                HideSpotlight();
            } else {
                OnAnimationEnded?.Invoke();
            }
        }

        public void HideButton() {
            if (_isHidingByAnyTap) {
                HideSpotlight();
            }
        }

        public void HideSpotlight() {
            _animation.Play(HIDE);
            StartCoroutine(WaitForAnimationEnded());
            _isShown = false;
        }

        private IEnumerator MoveSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
            _hintText.text = config.HintText;

            Vector3 startpos = _shadowTransform.position;
            Vector2 headPos = _headShift.anchoredPosition;
            Vector2 shadowSize = _shadowCenter.sizeDelta;

            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;
                _shadowTransform.position = Vector3.Slerp(startpos, targetPos, Mathf.SmoothStep(0, 1, percent / 2));
                _headShift.anchoredPosition = Vector2.Lerp(headPos, config.HeadShift, Mathf.SmoothStep(0, 1, percent / 2));

                _shadowCenter.sizeDelta = Vector2.Lerp(shadowSize, Vector2.zero, Mathf.SmoothStep(0, 1, percent ));
                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);

            curTime = 0;
            do {
                float percent = curTime / maxTime;
                float globalPercent = 0.5f + percent / 2;
                _shadowTransform.position = Vector3.Slerp(startpos, targetPos,Mathf.SmoothStep(0, 1, globalPercent) );
                _headShift.anchoredPosition = Vector2.Lerp(headPos, config.HeadShift, Mathf.SmoothStep(0, 1, globalPercent));

                _shadowCenter.sizeDelta = Vector2.Lerp(Vector2.zero, config.SpotlightSize, Mathf.SmoothStep(0, 1, percent));
                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));

                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private IEnumerator JumpSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
           
            _headFinPosAnchor.position = targetPos;

            StartCoroutine(MoveHeadPos(config));
            yield return StartCoroutine(JumpSpotlightStart(config));
           

            _shadowTransform.position = targetPos;
            _hintText.text = config.HintText;

            yield return StartCoroutine(JumpSpotlightEnd( config));
        }

        private IEnumerator JumpSpotlightStart(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            Vector2 shadowSize = _shadowCenter.sizeDelta;
            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;
                //_shadowTransform.position = Vector3.Slerp(startpos, targetPos, Mathf.SmoothStep(0, 1, percent / 2));
               // _headShift.anchoredPosition =
                 //   Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift, Mathf.SmoothStep(0, 1, percent / 2));

                _shadowCenter.sizeDelta = Vector2.Lerp(shadowSize, Vector2.zero, Mathf.SmoothStep(0, 1, percent));
                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(1, 0, percent * 2f);

                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private IEnumerator JumpSpotlightEnd(SpotlightAnimConfig config) {
            Vector2 startShadowSize = _shadowCenter.sizeDelta;
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 0.5f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;
                
                _shadowCenter.sizeDelta = Vector2.Lerp(startShadowSize, config.SpotlightSize, Mathf.SmoothStep(0, 1, percent));
                
                //_headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift, Mathf.SmoothStep(0, 1, 0.5f +percent/2));
                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(0,1,(percent-0.5f)*2f);
                
                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private IEnumerator MoveHeadOnly( SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;

                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(1, 0, percent * 2f);

                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);

            _hintText.text = config.HintText;
            yield return StartCoroutine(MoveHeadEnd(config));
        }

        private IEnumerator MoveHeadEnd(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 0.5f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;
                _headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift,
                    Mathf.SmoothStep(0, 1, 0.5f + percent/2));

                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(0, 1, (percent - 0.5f) * 2f);

                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private IEnumerator MoveHeadPos(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 1f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;
                _headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift,
                    Mathf.SmoothStep(0, 1, percent));
                curTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }
    }
}