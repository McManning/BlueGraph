using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BlueGraphSamples
{
    /// <summary>
    /// This is a simple CustomYieldInstruction that allows you to wait in a coroutine 
    /// for a click on one UI.Button out of a list of buttons. 
    /// 
    /// Author: http://wiki.unity3d.com/index.php/UI/WaitForUIButtons
    /// </summary>
    public class WaitForUIButtons : CustomYieldInstruction, System.IDisposable
    {
        private struct ButtonCallback
        {
            public Button button;
            public UnityAction listener;
        }

        private List<ButtonCallback> buttons = new List<ButtonCallback>();

        private Action<Button> buttonCallback = null;
 
        public override bool keepWaiting { get { return PressedButton == null; }}

        public Button PressedButton { get; private set; } = null;
 
        public WaitForUIButtons(Action<Button> aCallback, params Button[] aButtons)
        {
            buttonCallback = aCallback;
            buttons.Capacity = aButtons.Length;

            foreach(var b in aButtons)
            {
                if (b == null)
                    continue;
                var bc = new ButtonCallback { button = b };
                bc.listener = () => OnButtonPressed(bc.button);
                buttons.Add(bc);
            }

            Reset();
        }

        public WaitForUIButtons(params Button[] aButtons) : this(null, aButtons) { }
 
        private void OnButtonPressed(Button button)
        {
            PressedButton = button;
            RemoveListeners();
            buttonCallback?.Invoke(button);
        }

        private void InstallListeners()
        {
            foreach (var bc in buttons)
                if (bc.button != null)
                    bc.button.onClick.AddListener(bc.listener);
        }

        private void RemoveListeners()
        {
            foreach (var bc in buttons)
                if (bc.button != null)
                    bc.button.onClick.RemoveListener(bc.listener);
        }

        public new WaitForUIButtons Reset()
        {
            RemoveListeners();
            PressedButton = null;
            InstallListeners();
            base.Reset();
            return this;
        }

        public WaitForUIButtons ReplaceCallback(Action<Button> aCallback)
        {
            buttonCallback = aCallback;
            return this;
        }
 
        public void Dispose()
        {
            RemoveListeners();
            buttonCallback = null;
            buttons.Clear();
        }
    }
}