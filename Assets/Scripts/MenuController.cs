using UnityEngine;
using Unity.UI;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
/// <summary>
/// Tämä luokka hallitsee sitä mikä valikko
/// on näkyvissä ja lähettää tapahtumia kun
/// valikon nappeja painetaan.
///
/// Muut luokat pyytävät sitä näyttämään tai piilottamaan
/// valikoita
/// </summary>
public class MenuController : MonoBehaviour
{
    // Eri valikkotyypit
    enum MenuName
    {
        Building,
        Vehicle,
        Order
    }

    // Erilaiset käskynapit
    public enum OrderButtonType
    {
        GotoOrder,
        ClearOrders
    }

    // Pinorakenteen avulla voidaan muistaa missä
    // valikossa oltiin viimeksi
    private Stack<MenuName> menuHistory;
    
    // Julkinen, staattinen funktio jonka avulla
    // muut luokat saavat helposti viitteen tähän olioon
    public static MenuController GetController()
    {
        return GameObject.Find("UI Canvas").GetComponent<MenuController>();
    }
    private GameObject vehicleMenu;
    private GameObject buildingMenu;
    private GameObject ordersMenu;
    
    // Ajoneuvo jolle ollaan antamassa käskyjä
    private Vehicle orderTarget = null;
    private TextMeshProUGUI orderText; // Ruudulla näkyvät käskyt

    // Erilaiset rakennukset joita voi rakentaa
    // nämä yhdistetään näppäimiin nimen perusteella
    [SerializeField] private GameObject[] unitPrefabs;
    
    // Tapahtumat eri valikoiden nappien painamiselle
    public delegate void BuildingButtonPress(GameObject unit);
    public event BuildingButtonPress buildingButtonPressed;
    
    public delegate void VehicleButtonPress(GameObject unit);
    public event VehicleButtonPress vehicleButtonPressed;

    public delegate void OrderButtonPress(OrderButtonType buttontype);
    public event OrderButtonPress orderButtonPressed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vehicleMenu = GameObject.Find("VehiclesPanel");
        buildingMenu = GameObject.Find("BuildingsPanel");
        ordersMenu = GameObject.Find("OrdersPanel");
       
        
        // Yhdistä kaikki yhden valikon napit
        // samaan funktioon joka saa parametrina
        // napin nimen
        Button[] vehicleButtons =  vehicleMenu.GetComponentsInChildren<Button>();
        foreach (Button b in vehicleButtons)
        {
            b.onClick.AddListener(() => OnVehicleButton(b.name));
        }
        Button[] buildingButtons =  buildingMenu.GetComponentsInChildren<Button>();
        foreach (Button b in buildingButtons)
        {
            b.onClick.AddListener(() => OnBuildingButton(b.name));
        }

        Button[] orderButtons = ordersMenu.GetComponentsInChildren<Button>();
        foreach (Button b in orderButtons)
        {
            b.onClick.AddListener(() => OnOrderButton(b.name));
        }

        GameObject orderList = GameObject.Find("OrderListText");
        orderText = orderList.GetComponent<TextMeshProUGUI>();
        
        // Aloita aina rakennusvalikosta:
        menuHistory = new Stack<MenuName>();
        menuHistory.Push(MenuName.Building);
        // Piilota muut valikot
        HideMenu(MenuName.Vehicle);
        HideMenu(MenuName.Order);
    }

    // Pelaaja valitsi ajoneuvon
    void OnVehicleButton(string name)
    {
        Debug.Log($"Clicked vehicle button for {name}");
        GameObject found = Array.Find(unitPrefabs, unit => unit.name == name);
        if (found != null)
        {
            vehicleButtonPressed?.Invoke(found);
        }
    }

    // Pelaaja valitsi rakennuksen
    void OnBuildingButton(string name)
    {
        Debug.Log($"Clicked building button for {name}");
        // Etsi samanniminen prefab rakennus
        GameObject found = Array.Find(unitPrefabs, unit => unit.name == name);
        // Lähetä tapahtuma vain jos prefab löytyi
        if (found != null)
        {
            buildingButtonPressed?.Invoke(found);
        }
    }

    // Pelaaja painoi käskynappia; tutki kumpaa painettiin
    void OnOrderButton(string buttonName)
    {
        Debug.Log("MenuController on Order Button " + buttonName);
        if (buttonName == "GoTo")
        {
            orderButtonPressed?.Invoke(OrderButtonType.GotoOrder);
        }
        else
        {
            orderButtonPressed?.Invoke(OrderButtonType.ClearOrders);
        }
    }

    private void HideMenu(MenuName menu)
    {
        switch (menu)
        {
            case MenuName.Building:
                buildingMenu.SetActive(false);
                break;
            case MenuName.Order:
                ordersMenu.SetActive(false);
                break;
            case MenuName.Vehicle:
                vehicleMenu.SetActive(false);
                break;
        }
    }

    // Näytä pinon päällimmäinen valikko
    // Jos sitä ei ole, näytä rakennusvalikko
    private void ShowTopMenu()
    {
        MenuName topMenu;
        if (menuHistory.TryPeek(out topMenu))
        {
            ShowMenu(topMenu); 
        }
        else
        {
            // History is empty: put Building menu
            ShowMenu(MenuName.Building);
            menuHistory.Push(MenuName.Building);
        }
    }

    // Piilota aktiivinen valikko
    void HideTopMenu()
    {
        MenuName topMenu;
        // Piilota vain jos pinossa on jotain
        if (menuHistory.TryPeek(out topMenu))
        {
            HideMenu(topMenu);
        }
    }

    private void ShowMenu(MenuName menu)
    {
        switch (menu)
        {
            case MenuName.Building: 
                buildingMenu.SetActive(true);
                break;
            case MenuName.Vehicle: 
                vehicleMenu.SetActive(true);
                break;
            case MenuName.Order: 
                ordersMenu.SetActive(true);
            break;
        }
    }

    // Muut luokat kutsuvat näitä funktioita
    public void ShowBuildingMenu()
    {
        ShowMenu(MenuName.Building);
    }

    public void HideBuildingMenu()
    {
        HideMenu(MenuName.Building);
    }

    public void ShowVehicleMenu()
    {
        HideTopMenu();
        menuHistory.Push(MenuName.Vehicle);
        ShowTopMenu();
    }

    public void HideVehicleMenu()
    {
        HideMenu(MenuName.Vehicle);
        menuHistory.Pop();
        ShowTopMenu();
    }

    public void ShowOrdersMenu(Vehicle target)
    {
        orderTarget = target;
        HideTopMenu();
        menuHistory.Push(MenuName.Order);
        ShowTopMenu();
    }
    public void HideOrdersMenu()
    {
        HideMenu(MenuName.Order);
        menuHistory.Pop();
        ShowTopMenu();
    }

    public void SetOrderListText(string text)
    {
        orderText.text = text;
    }
}
