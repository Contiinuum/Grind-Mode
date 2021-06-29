using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AudicaModding
{
    internal static class GrindModeButton
    {
        private static GameObject grindModeButton;
        private static bool IsCreated => grindModeButton != null;
        private static Vector3 buttonPosition = new Vector3(-12.28f, 9.5f, -6.38f);
        private static Vector3 buttonRotation = new Vector3(0f, -51.978f, 0f);

        public static void CreateGrindModeButton()
        {
            if (IsCreated)
            {
                grindModeButton.SetActive(true);
                return;
            }

            string name = "menu/ShellPage_Launch/page/backParent/back";
            Action listener = new Action(() => { OnGrindModeButtonShot(); });
            Vector3 localPosition = buttonPosition;
            Vector3 rotation = buttonRotation;
            

            var refButton = GameObject.Find(name);
            GameObject button = GameObject.Instantiate(refButton, refButton.transform.parent.transform);
            grindModeButton = button;
            
            InitButton(button, "Grind Mode", listener, localPosition, rotation);
        }

        public static void InitButton(GameObject button, string label, Action listener, Vector3 localPosition,
                                      Vector3 rotation)
        {
            GameObject.Destroy(button.GetComponentInChildren<Localizer>());

            UpdateButtonLabel(button, label);

            GunButton gb = button.GetComponentInChildren<GunButton>();
            gb.destroyOnShot = false;
            gb.doMeshExplosion = false;
            gb.doParticles = false;
            gb.onHitEvent = new UnityEvent();
            gb.onHitEvent.AddListener(listener);

            button.transform.localPosition = localPosition;
            button.transform.Rotate(rotation);
        }

        public static void UpdateButtonLabel(GameObject button, string label)
        {
            TextMeshPro buttonText = button.GetComponentInChildren<TextMeshPro>();
            buttonText.text = label;
        }

        private static void OnGrindModeButtonShot()
        {
            GrindMode.ShowPanel = true;
            MenuState.I.GoToSettingsPage();
        }
    }
}
