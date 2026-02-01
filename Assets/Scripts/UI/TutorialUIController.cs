using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class TutorialUIController : MonoBehaviour
    {
        [SerializeField] private Button ParentButton;
        [SerializeField] private Image ParentImage;
        [SerializeField] private CanvasGroup WasdPopup;
        [SerializeField] private CanvasGroup AttackPopup;
        [SerializeField] private CanvasGroup BushPopup;
        [SerializeField] private CanvasGroup HidingPopup;
        [SerializeField] private CanvasGroup StainPopup;
        [SerializeField] private CanvasGroup ShepherdPopup;

        [SerializeField] private float generalWaitDuration = 0.2f;
        [SerializeField] private float waitDurationOnStartUp = 1.3f;

        private bool isTutorial = true;

        public Button tutorialSkipToggler;


        private Coroutine popupCoroutine;

        bool isBush = false;
        bool isShepherd = false;
        bool isWasd = false;
        bool isAttack = false;
        bool isHiding = false;
        bool isStaining = false;

        private int countSheep;

        private void Awake()
        {
            PlayerHandler.OnPlayerInProximityOf += PopUpAccordingToProximity;
            MainMenu.FinishedCameraPan += TriggerWasd;
            PlayerHandler.OnPlayerBloodyAndNextToSheep += TriggerStaining;

            HideWasdPopup();
            HideAttackPopup();
            HideBushPopup();
            HideHidingPopup();
            HideStainPopup();
            HideShepherdPopup();
            ParentButton.interactable = false;
            ParentImage.raycastTarget = false;

            ParentButton.interactable = false;
            ParentButton.onClick.AddListener(() =>
            {
                HideWasdPopup();
                HideAttackPopup();
                HideBushPopup();
                HideHidingPopup();
                HideStainPopup();
                HideShepherdPopup();

                ResumeGameTime();
                ParentButton.interactable = false;
                ParentButton.interactable = false;
                ParentImage.raycastTarget = false;
            });


            //tutorialSkipToggler.onClick.AddListener(() => { isTutorial = !isTutorial; });
        }

        private void TriggerStaining()
        {
            if (!isStaining && isAttack)
            {
                isStaining = true;
                popupCoroutine = StartCoroutine(WaitABitAndPopup(ShowStainPopup));
            }
        }

        private void TriggerWasd()
        {
            if (!isWasd)
            {
                isWasd = true;
                popupCoroutine = StartCoroutine(WaitABitAndPopup(ShowWasdPopup));
            }
        }

        private void PopUpAccordingToProximity(TutorialEntetyType entityInProxOfPlayer)
        {
            switch (entityInProxOfPlayer)
            {
                case TutorialEntetyType.Sheep:
                    if (isWasd)
                    {
                        if (!isAttack)
                        {
                            isAttack = true;
                            StartCoroutine(WaitABitAndPopup(ShowAttackPopup));
                        }
                    }

                    break;
                case TutorialEntetyType.Bush:
                    if (isAttack)
                    {
                        if (!isBush)
                        {
                            isBush = true;
                            StartCoroutine(WaitABitAndPopup(ShowBushPopup));
                        }
                    }

                    break;
                case TutorialEntetyType.Shepherd:
                    if (isBush)
                    {
                        if (!isShepherd)
                        {
                            isShepherd = true;
                            StartCoroutine(WaitABitAndPopup(ShowShepherdPopup));
                        }
                    }

                    break;
            }
        }

        public void ShowWasdPopup()
        {
            WasdPopup.alpha = 1;
            WasdPopup.interactable = false;
            WasdPopup.blocksRaycasts = false;
        }

        public void HideWasdPopup()
        {
            WasdPopup.alpha = 0;
            WasdPopup.interactable = false;
            WasdPopup.blocksRaycasts = false;
        }

        public void ShowAttackPopup()
        {
            AttackPopup.alpha = 1;
            AttackPopup.interactable = false;
            AttackPopup.blocksRaycasts = false;
        }

        public void HideAttackPopup()
        {
            AttackPopup.alpha = 0;
            AttackPopup.interactable = false;
            AttackPopup.blocksRaycasts = false;
        }

        public void ShowBushPopup()
        {
            BushPopup.alpha = 1;
            BushPopup.interactable = false;
            BushPopup.blocksRaycasts = false;
        }

        public void HideBushPopup()
        {
            BushPopup.alpha = 0;
            BushPopup.interactable = false;
            BushPopup.blocksRaycasts = false;
        }

        public void ShowHidingPopup()
        {
            isHiding = true;
            HidingPopup.alpha = 1;
            HidingPopup.interactable = false;
            HidingPopup.blocksRaycasts = false;
        }

        public void HideHidingPopup()
        {
            HidingPopup.alpha = 0;
            HidingPopup.interactable = false;
            HidingPopup.blocksRaycasts = false;
        }

        public void ShowStainPopup()
        {
            StainPopup.alpha = 1;
            StainPopup.interactable = false;
            StainPopup.blocksRaycasts = false;
        }

        public void HideStainPopup()
        {
            StainPopup.alpha = 0;
            StainPopup.interactable = false;
            StainPopup.blocksRaycasts = false;
        }

        public void ShowShepherdPopup()
        {
            ShepherdPopup.alpha = 1;
            ShepherdPopup.interactable = false;
            ShepherdPopup.blocksRaycasts = false;
        }

        public void HideShepherdPopup()
        {
            ShepherdPopup.alpha = 0;
            ShepherdPopup.interactable = false;
            ShepherdPopup.blocksRaycasts = false;
            //if (!isShepherd) return;

            //isHiding = true;
            //ShowHidingPopup();
        }

        private IEnumerator WaitABitAndPopup(Action action)
        {
            if (!isTutorial) yield break;

            if (!isWasd)
            {
                yield return new WaitForSeconds(waitDurationOnStartUp);
            }
            else
            {
                yield return new WaitForSeconds(generalWaitDuration);
            }


            StopGameTime();
            action.Invoke();
            ParentButton.interactable = true;
            ParentImage.raycastTarget = true;
        }

        private void StopGameTime()
        {
            Time.timeScale = 0f;
        }

        private void ResumeGameTime()
        {
            Time.timeScale = 1f;
        }
    }
}