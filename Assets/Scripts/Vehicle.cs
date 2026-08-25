using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Unity.VisualScripting;

public enum VehicleAction
{
    Load,
    Unload
}

// Yksittäinen käsky jonka ajoneuvo voi toteuttaa
public class VehicleOrder
{
    public GameObject building; // Käskyn kohde
    public VehicleAction action; // Toiminta kohteessa

    public bool IsSameAs(VehicleOrder other)
    {
        return (building == other.building && action == other.action);
    }
}

/// <summary>
/// Ajoneuvot kuljettavat resursseja rakennusten välillä
/// </summary>
public class Vehicle : MonoBehaviour, ISelectable
{
    bool isSelected = false;
    private Rigidbody rb;
    
    private int loadAmount = 0; // Kuinka paljon lastia on
    [SerializeField] private int loadCapacity; // Kuinka paljon ajoneuvoon mahtuu
    [SerializeField] private ResourceType cargoType;
    [SerializeField] private float moveSpeed;
    
    // Mitä käskyä ajoneuvo on nyt suorittamassa;
    // toimii indeksi käskyjen listaan
    private int activeOrderIndex = -1;
    List<VehicleOrder> orders;
    
    private MenuController menuController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        menuController = MenuController.GetController();
        orders = new List<VehicleOrder>();
    }

    [CanBeNull]
    VehicleOrder GetActiveOrder()
    {
        if (activeOrderIndex >= 0 && activeOrderIndex < orders.Count)
        {
            VehicleOrder order = orders[activeOrderIndex];
            return order;
        }
        return null;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // Liiku kohti käskyn määrittämää kohdetta
        VehicleOrder order = GetActiveOrder(); 
        if (order != null)
        {
            Vector3 dir = (order.building.transform.position - transform.position).normalized;
            rb.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Yritä toteuttaa käsky kohteena olevassa rakennuksessa.
    /// Ajoneuvo päättelee rakennuksen tuottamien tai
    /// kuluttamien resurssien ja oman rahtityyppinsä
    /// perusteella pitääkö resursseja ottaa vai luovuttaa
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    bool TryDoOrder(Building building)
    {
        if (building != null)
        {
            // Jos rakennus tuottaa lastina olevaa 
            // resurssia, lastaa täyteen:
            if (building.IsProducing(cargoType)) 
            {
                if (loadAmount < loadCapacity)
                {
                    // Pyydä vain sen verran mitä mahtuu
                    // kyytiin
                    loadAmount += (int)building.Take(cargoType, loadCapacity - loadAmount);
                }
                if (loadAmount >= loadCapacity)
                {
                    // Lasti on täysi, käsky on suoritettu
                    return true;
                }
            }
            // Jos rakennus kuluttaa lastina olevaa
            // resurssia luovuta koko lasti:
            else if (building.IsConsuming(cargoType))
            {
                building.Receive(cargoType, loadAmount);
                loadAmount = 0;
                // Lasti luovutettu: käsky on suoritettu
                return true;
            }
        }
        // Ei voinut suorittaa käskyä
        return false;
    }
    // Tutki onko Triggerin havaitsema rakennus
    // aktiivisen käskyn kohde, eli ollaanko perillä.
    void OnTriggerStay(Collider other)
    {
        // Älä huomioi rakennusten Trigger alueita
        if (other.CompareTag("Building") && other.isTrigger == false)
        {
            VehicleOrder order = GetActiveOrder(); 
            if (order != null)
            {
                if (order.building == other.gameObject)
                {
                    Building building = other.GetComponent<Building>();
                    if (TryDoOrder(building))
                    {
                        // Jos käskyn suoritus onnistui
                        // siirry seuraavaan käskyyn
                        activeOrderIndex += 1;
                        // Jos kaikki käskyt on suoritettu
                        // palaa ensimmäiseen käskyyn
                        activeOrderIndex %= orders.Count;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Add order to vehicle. Duplicate orders next to each other are not accepted
    /// </summary>
    /// <param name="order"></param>
    /// <returns>True if order was added</returns>
    public bool AddOrder(VehicleOrder order)
    {
        if (orders.Count > 0)
        {
            VehicleOrder lastOrder = orders[orders.Count - 1];
            if (lastOrder.IsSameAs(order))
            {
                return false;
            }
        }

        orders.Add(order);
        // Odota kunnes ajoneuvolla on vähintään kaksi
        // ohjetta ennen kuin niitä aletaan suorittaa
        if (orders.Count > 1 && activeOrderIndex < 0)
        {
            activeOrderIndex = 0;
        }
        return true;
    }

    public void ClearOrders()
    {
        orders.Clear();
        activeOrderIndex = -1;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            // Show orders menu
            menuController.ShowOrdersMenu(this);
        }
        else
        {
            menuController.HideOrdersMenu();
        }
        isSelected = selected;
    }

    public string OrdersToString()
    {
        StringBuilder orderList = new StringBuilder();
        for (int i = 0; i < orders.Count; i++)
        {
            VehicleOrder order = orders[i];
            orderList.Append($"{i} Goto: {order.building.name}\n");
        }
        return orderList.ToString();
    }

    // Näytä ajoneuvon tiedot ja käskyt
    private void OnGUI()
    {
        Vector3 textpos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1);
        string text = name;
        if (isSelected)
        {
            text = $"[{ name }]";
        }  
        GUILayout.BeginArea(new Rect(textpos.x, Screen.height - textpos.y, 400, 600));
        GUILayout.Label(text);
        GUILayout.Label($"{loadAmount} / {loadCapacity}");
        if (orders != null && orders.Count > 0)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                VehicleOrder order = orders[i];
                if (activeOrderIndex == i)
                {
                    GUILayout.Label($"> {i} Goto: {order.building.name}");
                }
                else
                {
                    GUILayout.Label($"  {i} Goto: {order.building.name}");
                }
            }
        }

        GUILayout.EndArea();
    }
}
