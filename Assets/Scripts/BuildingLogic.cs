using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingLogic : MonoBehaviour
{
    // Mitä rakennusta pelaaja on rakentamassa?
    // previewBuilding on prefabista luotu GameObject
    // jota liikutetaan hiirellä rakentamisen aikana
    private GameObject selectedBuildingPrefab = null;
    private GameObject previewBuilding = null;

    // Tämä muuttuja tallentaa voiko valittuna olevan
    // rakennuksen rakentaa hiiren osoittamaan kohtaan
    // joka on buildingPos
    private bool canBuildHere = false;
    private Vector3 buildingPos = Vector3.zero;
    
    // Viite olioon joka hallitsee valikoita
    private MenuController menuController;

    /// <summary>
    /// Missä tilassa ollaan, määrittää sen miten hiiren klikkaukset
    /// tulkitaan ja mitä ne tekevät
    /// </summary>
    public enum InputMode
    {
       Selecting, // Selecting unit or building type from menu
       Building, // Placing a building
       Ordering  // Giving orders to a vehicle
    }
    private InputMode activeMode;
    
    // Valittu asia, voi olla rakennus tai ajoneuvo(vehicle)
    ISelectable selectedItem;
    
    void Start()
    {
        // Hae valikkojen ohjaaja ja kuuntele 
        // milloin painetaan rakennusnappia tai ohjeen antamisen
        // nappia valikossa
        menuController = GameObject.Find("UI Canvas").GetComponent<MenuController>();
        menuController.buildingButtonPressed += OnBuildingButton;
        menuController.orderButtonPressed+= OnOrderButton;
        
        // Alussa ollaan valitsemistilassa ja mitään ei ole valittuna
        activeMode =  InputMode.Selecting;
        selectedItem = null;
    }

    /// <summary>
    /// Tällä funktiolla muut skriptit voivat kysyä missä
    /// tilassa rakentamislogiikka on
    /// </summary>
    /// <returns></returns>
    public InputMode GetActiveInputMode()
    {
        return activeMode;
    }

    /// <summary>
    /// Tämä funktio ampuu säteen hiiren sijainnista kohti pelimaailmaa
    /// Sille annetaan maski, joka kertoo mihin tasoihin säde saa osua
    /// </summary>
    /// <param name="raymask"></param>
    /// <returns>Ensimmäinen osuma. Jos ei osu mihinkään niin hit.collision on null</returns>
    private RaycastHit RayToMask(int raymask)
    {
        Vector3 pos = Mouse.current.position.ReadValue();
        Ray selectRay = Camera.main.ScreenPointToRay(pos);
        // Säde ei osu Collidereihin jotka ovat Trigger
        if (Physics.Raycast(selectRay.origin, selectRay.direction,  out RaycastHit hit, 1000, raymask, QueryTriggerInteraction.Ignore))
        {
            return hit;
        }
        // Jos ei osu mihinkään, palauta tyhjä osuma
        return new RaycastHit();
    }

    private RaycastHit RayToGroundOrBuilding()
    {
        int buildingMask = LayerMask.GetMask("Building", "Ground");
        return RayToMask(buildingMask);
    }

    private RaycastHit RayToBuilding()
    {
        int buildingMask = LayerMask.GetMask("Building");
        return RayToMask(buildingMask);
    }

    // Kun hiirtä klikataan, katsotaan missä tilassa ollaan
    // OnSelect on kytketty Input Action:iin Select
    void OnSelect(InputValue value) 
    {
        switch (activeMode)
        {
            case InputMode.Selecting:
                HandleSelectingInput();
                break;
            case InputMode.Building:
                HandleBuildingInput();
                break;
            case InputMode.Ordering:
                HandleOrderInput();
                break;
        }
    }

    /// <summary>
    /// Pelaaja voi valita rakennuksen tai ajoneuvon
    /// </summary>
    void HandleSelectingInput()
    {
        int buildingVehicleMask = LayerMask.GetMask("Building", "Vehicle");
        RaycastHit hit = RayToMask(buildingVehicleMask);
        
        // Tyhjennä aina tämänhetkinen valinta
        ClearSelection();
        if (hit.collider != null)
        {
            selectedItem = hit.transform.gameObject.GetComponent<ISelectable>();
            if (selectedItem != null)
            {
                selectedItem.SetSelected(true);
                
                // Jos valittiin ajoneuvo, vaihda tilaksi käskyjen antaminen
                Vehicle selectedVehicle = selectedItem as Vehicle;
                if (selectedVehicle)
                {
                    activeMode = InputMode.Ordering;
                }
            }
        }
    }

    /// <summary>
    /// Jos jokin rakennus prefab on valittu, hiiren painallus
    /// yrittää sijoittaa sen kenttään jos kursorin kohdalla on
    /// tyhjää, eli säde osuu maahan
    /// </summary>
    void HandleBuildingInput()
    {
        if (selectedBuildingPrefab != null)
        {
           RaycastHit hit = RayToGroundOrBuilding();
           
           // Jos osuu maahan...
           if (hit.collider != null && hit.collider.CompareTag("Ground"))
           {
               if (previewBuilding != null)
               {
                   // Aseta rakennuksen paikka ja vaihda sen tasoksi Building
                   previewBuilding.transform.position = hit.point;
                   previewBuilding.layer = LayerMask.NameToLayer("Building");
                   previewBuilding.SetActive(true);
                   // Lopeta esikatselu poistamalla viite rakennukseen
                   previewBuilding = null;
               }
           }
        }
    }

    /// <summary>
    /// Tämä tila on päällä kun pelaaja on antamassa ohjeita
    /// ajoneuvolle. Jos pelaaja klikkaa rakennusta ajoneuvo
    /// saa ohjeen mennä sen luokse
    /// </summary>
    void HandleOrderInput()
    {
        RaycastHit hit = RayToBuilding();
        if (hit.collider != null)
        {
            VehicleOrder order = new VehicleOrder();
            order.building = hit.collider.gameObject;
            
            // Varmista että valittu asia on ajoneuvo ja anna sille
            // uusi ohje. Lisäksi päivitä ohjeiden lista käyttöliittymässä
            Vehicle vehicle = selectedItem as Vehicle;
            if (vehicle)
            {
                vehicle.AddOrder(order);
                menuController.SetOrderListText(vehicle.OrdersToString());
            }
        }
    }

    /// <summary>
    /// Tätä funktiota kutsutaan kun hiiren oikeaa näppäintä painetaan
    /// Kytketty toimintoon Deselect
    /// </summary>
    /// <param name="value"></param>
    void OnDeselect(InputValue value)
    {
        switch (activeMode)
        {
            case InputMode.Selecting:
                ClearSelection();
                break;
            
            case InputMode.Building:
                // Jos ollaan rakentamassa, tuhoa esikatselurakennus
                // ja lopeta esikatselu
                if (previewBuilding != null)
                {
                    previewBuilding.SetActive(true);
                    GameObject.Destroy(previewBuilding);
                }

                previewBuilding = null;
                selectedBuildingPrefab = null;
                activeMode = InputMode.Selecting;
            break;
            
            // Tyhjennä ohjeiden lista jotta se ei ole täynnä edellisen
            // ajoneuvon ohjeita kun seuraava ajoneuvo valitaan
            case InputMode.Ordering:
                ClearSelection();
                activeMode = InputMode.Selecting;
                menuController.SetOrderListText(String.Empty);
                break;
        }
    }

    private void ClearSelection()
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }
    }

    // Tilasta riippuen pelaajan hiiren liikuttaminen voi tehdä jotain
    void Update()
    {
        switch (activeMode)
        {
            case InputMode.Selecting: break;
            
            case InputMode.Building:
            {
                // Tarkista jatkuvasti voiko hiiren osoittamaan
                // kohtaan rakentaa.
                // Mutta tarkista vain jos on valittu rakennustyyppi
                if (selectedBuildingPrefab != null && previewBuilding != null)
                {
                    RaycastHit hit = RayToGroundOrBuilding();
                    if (hit.collider != null)
                    {
                        // Tallenna piste
                        buildingPos = hit.point;

                        // Jos säde osui maahan, voi rakentaa
                        // siirrä esikatselurakennusta
                        if (hit.collider.CompareTag("Ground"))
                        {
                            previewBuilding.SetActive(true);
                            previewBuilding.transform.position = buildingPos;
                            canBuildHere = true;
                        }
                        else
                        {
                            // Jos osui olemassa olevaan rakennukseen,
                            // laita esikatselurakennus pois päältä
                            previewBuilding.SetActive(false);
                            canBuildHere = false;
                        }
                    }
                }
            }
            break;
            case InputMode.Ordering: break;
        }
    }

    /// <summary>
    /// Kerro pelaajalle voiko rakentaa vai ei
    /// </summary>
    private void OnGUI()
    {
        Vector3 textpos = Camera.main.WorldToScreenPoint(buildingPos);
        string text;
        if (previewBuilding != null && selectedBuildingPrefab != null)
        {
            text = "Cannot build";
            if (canBuildHere)
            {
                text = "BUILD OK";
            }
        }
        else
        {
            text = "";
        }
        GUI.Label(new Rect(textpos.x, Screen.height - textpos.y, 1000, 3000), text);

        // Näytä aktiivinen tila
        string modeText = "";
        switch (activeMode)
        {
            case InputMode.Selecting: modeText = "Selecting"; break;
            case InputMode.Building: modeText = "Building"; break;
            case InputMode.Ordering: modeText = "Ordering"; break;
        }

        GUI.Label(new Rect(10, Screen.height - 40, 1000, 3000), $"Mode: {modeText}");
    }

    // Pelaaja klikkaa rakennustyyppiä rakennusvalikosta
    void OnBuildingButton(GameObject buildingPrefab)
    {
        selectedBuildingPrefab = buildingPrefab;
        CreatePreview(selectedBuildingPrefab);
        activeMode = InputMode.Building;
    }

    void CreatePreview(GameObject prefabBuilding)
    {
        // Jos jokin on jo valittuna, poista se ensin
        if (previewBuilding != null)
        {
            GameObject.Destroy(previewBuilding);
        }
        previewBuilding = Instantiate(prefabBuilding, Vector3.zero, Quaternion.identity);
        // Put preview on different layer so it does not interfere with placement preview rays
        previewBuilding.layer = LayerMask.NameToLayer("Default");
    }

    void OnOrderButton(MenuController.OrderButtonType type)
    {
        Debug.Log("Order button pressed!");
        switch (type)
        {
            // Kun pelaaja painaa [Go To] nappia, siirry
            // ohjeiden antamisen tilaan
            case MenuController.OrderButtonType.GotoOrder:
                activeMode = InputMode.Ordering;
                break;
            case MenuController.OrderButtonType.ClearOrders:
                Vehicle vehicle = (Vehicle)selectedItem;
                if (vehicle)
                {
                    vehicle.ClearOrders();
                    menuController.SetOrderListText(String.Empty);
                }
                break;
        }
    }
}
