using System;
using UnityEngine;
using UnityEngine.UI;

public class DisplayNameUpdater : MonoBehaviour
{
    public Text DisplayNameText;
    public Customizable Customizable;


    void Update()
    {
        // Update the UI Text element with the DisplayName of the current customization
        if (DisplayNameText != null)
            DisplayNameText.text = Customizable.CurrentCustomization.DisplayName;

    }
}
