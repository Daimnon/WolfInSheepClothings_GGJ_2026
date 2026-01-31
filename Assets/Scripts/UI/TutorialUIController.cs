using System;
using System.Collections;
using Player;
using UnityEngine;

namespace UI
{
    public class TutorialUIController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup WasdPopup;
        [SerializeField] private CanvasGroup AttackPopup;
        [SerializeField] private CanvasGroup BushPopup;
        [SerializeField] private CanvasGroup HidingPopup;
        [SerializeField] private CanvasGroup StainPopup;
        [SerializeField] private CanvasGroup ShepherdPopup;
        
        

        private void Awake()
        {
            PlayerHandler.OnPlayerInProximityOf += PopUpAccordingToProximity;
            MainMenu.FinishedCameraPan += TriggerWasd;
            HideWasdPopup();
            HideAttackPopup();
            HideBushPopup();
            HideHidingPopup();
            HideStainPopup();
            HideShepherdPopup();
        }

        private void TriggerWasd()
        {
            
        }

        private void PopUpAccordingToProximity(TutorialEntetyType tet)
        {
            switch (tet)
            {
                
            }
        }

        public void ShowWasdPopup()
        {
            WasdPopup.alpha = 1;
            WasdPopup.interactable = true;
            WasdPopup.blocksRaycasts = true;
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
            AttackPopup.interactable = true;
            AttackPopup.blocksRaycasts = true;
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
            BushPopup.interactable = true;
            BushPopup.blocksRaycasts = true;
        }
        public void HideBushPopup()
        {
            BushPopup.alpha = 0;
            BushPopup.interactable = false;
            BushPopup.blocksRaycasts = false;
        }
        public void ShowHidingPopup()
        {
            HidingPopup.alpha = 1;
            HidingPopup.interactable = true;
            HidingPopup.blocksRaycasts = true;
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
            StainPopup.interactable = true;
            StainPopup.blocksRaycasts = true;
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
            ShepherdPopup.interactable = true;
            ShepherdPopup.blocksRaycasts = true;
        }
        public void HideShepherdPopup()
        {
            ShepherdPopup.alpha = 0;
            ShepherdPopup.interactable = false;
            ShepherdPopup.blocksRaycasts = false;
        }

        private IEnumerator WaitABitAndPopup(Action action)
        {
            
        }
    }
}

public enum TutorialStage
{
    Wasd,
    FirstSheep,
    SecondSheep,
    Shepherd
}