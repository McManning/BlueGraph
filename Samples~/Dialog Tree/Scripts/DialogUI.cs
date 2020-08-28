using BlueGraph;
using BlueGraphSamples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueGraphSamples
{
    public enum PortraitPosition
    {
        None,
        Left,
        Right,
        Center
    }

    /// <summary>
    /// Queries for components of the Dialog UI prefab
    /// </summary>
    public class DialogUI : MonoBehaviour
    {
        public Button ContinueButton { get; private set; }

        private readonly Dictionary<PortraitPosition, Image> portraits = new Dictionary<PortraitPosition, Image>();
        
        private GameObject choices;
        private Text message;
        private Text namePlate;

        void Start()
        {
            portraits[PortraitPosition.Left] = transform.Find("Left").GetComponent<Image>();
            portraits[PortraitPosition.Right] = transform.Find("Right").GetComponent<Image>();
            portraits[PortraitPosition.Center] = transform.Find("Center").GetComponent<Image>();

            namePlate = transform.Find("NamePlate").gameObject.GetComponentInChildren<Text>();
            message = transform.Find("Message").gameObject.GetComponent<Text>();
            choices = transform.Find("Choices").gameObject;

            ContinueButton = transform.Find("ContinueButton").GetComponent<Button>();

            // Instance each material separately
            foreach (var image in portraits)
            {
                image.Value.material = new Material(image.Value.material);
            }
        }

        public void SetPortrait(PortraitPosition position, Texture2D portrait = null)
        {
            portraits[position].material.mainTexture = portrait;
            portraits[position].SetAllDirty();
            portraits[position].gameObject.SetActive(portrait != null);
        }

        public void ShowMessage(string text, string name = null)
        {
            message.gameObject.SetActive(true);
            message.text = text;
            
            namePlate.text = name;
            namePlate.transform.parent.gameObject.SetActive(name != null);
        }

        public void ClearMessage()
        {
            message.gameObject.SetActive(false);
        }

        public Button GetChoiceButton(int index)
        {
            return choices.transform.Find($"Choice {index}").GetComponent<Button>();
        }

        /// <summary>
        /// Display a specific choice button with the text
        /// </summary>
        public Button ShowChoice(int index, string text)
        {
            choices.SetActive(true);

            var button = GetChoiceButton(index);
            button.GetComponentInChildren<Text>().text = text;

            button.gameObject.SetActive(true);
            return button;
        }

        /// <summary>
        /// Clear the choice buttons 
        /// </summary>
        public void ClearChoices()
        {
            choices.SetActive(false);

            for (int i = 0; i < 3; i++)
            {
                var button = GetChoiceButton(i);
                button.GetComponentInChildren<Text>().text = "";
                button.gameObject.SetActive(false);
            }
        }
    }
}
