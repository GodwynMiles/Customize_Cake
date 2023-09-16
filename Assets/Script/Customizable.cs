using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;   

public class MaterialPrice : MonoBehaviour {
    public float Price;
}

public class Customizable : MonoBehaviour {
    public List<Customization> Customizations;
    int _currentCustomizationIndex;
    public Customization CurrentCustomization { get; private set; }
    public int captureWidth = 1920; 
    public int captureHeight = 1080; 
    public GameObject messageBoxPanel;

    private Camera cam;
    public Dropdown flavorDropdown;
    public Dropdown sugarDropdown;
    public Text TotalPriceText;
    public Button SaveButton;
    public InputField NameInput;
    private string selectedFlavor = "";
    private string selectedSugarLevel = "";

    private float totalPrice;
    private float flavorPrice;
    private float sugarPrice;
    private byte [] imageData;

     private void Start() {
        cam = GetComponent<Camera>();
        SaveButton.onClick.AddListener(SaveCustomization);

        flavorDropdown.onValueChanged.AddListener(delegate { FlavorChanged(flavorDropdown); });

        sugarDropdown.onValueChanged.AddListener(delegate { SugarChanged(sugarDropdown); });
    }

    private void FlavorChanged(Dropdown dropdown1) {

    switch (dropdown1.value) {
        case 0:
            flavorPrice = 150f;
            selectedFlavor = "Chocolate Moist";
            break;
        case 1:
            flavorPrice = 250f;
            selectedFlavor = "Ube Moist"; 
            break;
        case 2:
            flavorPrice = 350f;
            selectedFlavor = "Red Velvet"; 
            break;
    }

    totalPrice += flavorPrice;

    UpdateTotalPrice();
}

  private void SugarChanged(Dropdown dropdown) {

    switch (dropdown.value) {
        case 0:
            sugarPrice = 50f;
            selectedSugarLevel = "Low";
            break;
        case 1:
            sugarPrice = 80f;
            selectedSugarLevel = "Medium";
            break;
        case 2:
            sugarPrice = 100f;
            selectedSugarLevel = "High";
            break;
    }

    totalPrice += sugarPrice;

    UpdateTotalPrice();
}

    private void SaveCustomization() {
           
    string flavor = flavorDropdown.captionText.text;
    string sugarLevel = sugarDropdown.captionText.text;

    string userName = NameInput.text;

    RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
    cam.targetTexture = rt;
    Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    cam.Render();
    RenderTexture.active = rt;
    screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

    cam.targetTexture = null;
    RenderTexture.active = null;
    
    string filePath = Application.dataPath + "/Screenshot.jpg"; // Change file extension to ".jpg"
    ScreenCapture.CaptureScreenshot(filePath);

    byte[] imageData = screenShot.EncodeToJPG(); // Use JPEG format instead of PNG

    StartCoroutine(InsertCustomizationData(userName, selectedFlavor, selectedSugarLevel, TotalPriceText.text, imageData));;
}

private IEnumerator InsertCustomizationData(string userName, string flavor, string sugarLevel, string totalPrice, byte[] imageData) {

    WWWForm form = new WWWForm();
    form.AddField("user", userName);
    form.AddField("flavor", flavor);
    form.AddField("sugar", sugarLevel);
    form.AddField("total", totalPrice);
    form.AddBinaryData("image", imageData, "screenshot.jpg", "image/jpeg");

    using (UnityWebRequest www = UnityWebRequest.Post("https://thaliasweetooth.shop/uCustomize.php", form)) {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log("Error inserting customization data: " + www.error);
        } else {
            Debug.Log("Customization data inserted successfully.");
            // Activate the pop-up message box
            messageBoxPanel.SetActive(true);
            // Wait for 3 seconds
            yield return new WaitForSeconds(3);
            // Deactivate the pop-up message box
            messageBoxPanel.SetActive(false);
        }
    }
}


private void UpdateTotalPrice() {
    TotalPriceText.text = "Total Price: ₱" + totalPrice.ToString();
}

    void Awake() {
        foreach (var customization in Customizations) {
            customization.UpdateRenderers();
            customization.UpdateSubObjects();
        }
    }

    void Update() {
        SelectCustomizationWithUpDownArrows();
    }

public void NextMaterialButton() {
    CurrentCustomization.NextMaterial();
    totalPrice += CurrentCustomization.MaterialPrice;
    UpdateTotalPrice();
}

public void NextSubObjectButton() {
    CurrentCustomization.NextSubObject();
    totalPrice += CurrentCustomization.SubObjectPrice;
    UpdateTotalPrice();
}

    public void SelectNextCustomizationButton() {
        _currentCustomizationIndex++;
        if (_currentCustomizationIndex >= Customizations.Count)
            _currentCustomizationIndex = 0;
        CurrentCustomization = Customizations[_currentCustomizationIndex];
UpdateUIElements();
}

    public void SelectPreviousCustomizationButton() {
    _currentCustomizationIndex--;
    if (_currentCustomizationIndex < 0)
        _currentCustomizationIndex = Customizations.Count - 1;
    CurrentCustomization = Customizations[_currentCustomizationIndex];
    UpdateUIElements();
}

    void SelectCustomizationWithUpDownArrows() {
    if (Input.GetKeyDown(KeyCode.UpArrow))
        SelectPreviousCustomizationButton();
    else if (Input.GetKeyDown(KeyCode.DownArrow))
        SelectNextCustomizationButton();
}

    void UpdateUIElements() {
           
    if (CurrentCustomization.MaterialText)
        CurrentCustomization.MaterialText.text = CurrentCustomization.Materials[CurrentCustomization.MaterialIndex].name + " = ₱" + CurrentCustomization.MaterialPrice.ToString() + " ";
    if (CurrentCustomization.SubObjectText)
        CurrentCustomization.SubObjectText.text = CurrentCustomization.SubObjects[CurrentCustomization.SubObjectIndex].name + " = ₱" + CurrentCustomization.SubObjectPrice.ToString() + " ";
}
    
    
}

[Serializable]
    public class Customization {
public string DisplayName;
public List<Renderer> Renderers;
public List<Material> Materials;
public List<GameObject> SubObjects;

public int MaterialIndex { get; private set; }
public int SubObjectIndex { get; private set; }

public float MaterialPrice;
public float SubObjectPrice;

public Text MaterialText;
public Text SubObjectText;

    public void NextMaterial() {
    MaterialIndex++;
    if (MaterialIndex >= Materials.Count)
        MaterialIndex = 0;

    UpdateRenderers();
    if (MaterialText)
        MaterialText.text = Materials[MaterialIndex].name + " = ₱" + MaterialPrice.ToString() + " ";
}

    public void NextSubObject() {
    SubObjectIndex++;
    if (SubObjectIndex >= SubObjects.Count)
        SubObjectIndex = 0;

    UpdateSubObjects();
    if (SubObjectText)
        SubObjectText.text = SubObjects[SubObjectIndex].name + " = ₱" + SubObjectPrice.ToString() + " ";
}

    public void UpdateSubObjects() {
    for (var i = 0; i < SubObjects.Count; i++)
        if (SubObjects[i])
            SubObjects[i].SetActive(i == SubObjectIndex);
}

    public void UpdateRenderers() {
    foreach (var renderer in Renderers)
        if (renderer)
            renderer.material = Materials[MaterialIndex];
}
}