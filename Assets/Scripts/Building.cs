using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// Rakennukset tuottavat ja kuluttavat resursseja
/// sekä säilövät niitä inventaariossaan
/// Rakennukset voi valita.
/// </summary>
public class Building : MonoBehaviour, ISelectable
{
    bool isSelected = false;

    // Mitä rakennus tuottaa ja kuluttaa ja mitä
    // sillä on varastossaan
    [SerializeField] protected List<ResourceType> Inputs;
    [SerializeField] protected List<ResourceType> Outputs;
    [SerializeField] protected Dictionary<ResourceType, float> resources;
    
    // Ruudulla näkyvät tiedot rakennuksesta
    protected string buildingName;
    protected string debugInfo;
    protected string resourceInfo;
    protected string inventoryInfo;

    // Rakennuksen vaikutusalue jos on
    protected GameObject? AreaIndicator;

    protected void OnStart()
    {
        Transform AreaTrans = gameObject.transform.Find("Area");
        if (AreaTrans)
        {
            AreaIndicator = AreaTrans.gameObject;
        }
        else
        {
            AreaIndicator = null;
        }
            SphereCollider coll = GetComponentInChildren<SphereCollider>();
		if (AreaIndicator && coll)
		{
			AreaIndicator.transform.localScale = Vector3.one * (coll.radius * 2.0f);
		}
		if (AreaIndicator)
        {
            AreaIndicator.SetActive(false);
        }
        buildingName = gameObject.name;
        resources = new Dictionary<ResourceType, float>(); 
        StringBuilder sb = new StringBuilder();
        
        // Lisää kaikki tuotetut ja kulutettavat resurssit
        // inventaarioon ja ruudulla näkyvään tekstiin
        foreach (ResourceType type in Inputs)
        {
            sb.Append($"in > {type}\n");
            // Jos resurssia ei vielä ole inventaariossa
            // lisää se sinne ja laita määräksi 0
            if (resources.ContainsKey(type) == false)
            {
                resources.Add(type, 0);
            }
        }
        foreach (ResourceType type in Outputs)
        {
            sb.Append($"{type} > out\n");
            if (resources.ContainsKey(type) == false)
            {
                resources.Add(type, 0);
            }
        }
        resourceInfo = sb.ToString();
    }

    // Päivitä tekstiä joka kertoo inventaarion tilanteen
    // tätä kutsutaan kun rakennus saa tai menettää resursseja
    void UpdateInventory()
    {
        StringBuilder sb = new StringBuilder();
        foreach (ResourceType type in resources.Keys)
        {
            sb.Append($"[{ type }]:{(int)resources[type]} ");
        }
        inventoryInfo = sb.ToString();
    }
    
    public virtual void SetSelected(bool selected)
    {
        isSelected = selected;
        if (AreaIndicator)
        {
            AreaIndicator.SetActive(selected);
        }
    }

    // This is called when the building is placed on the map
    public virtual void OnConstructionComplete()
    {
        // Nop
    }    

    // Apufunktiot resurssien käsittelyyn
    public bool IsProducing(ResourceType type)
    {
        return Outputs.Contains(type);
    }

    public bool IsConsuming(ResourceType type)
    {
        return Inputs.Contains(type);
    }

    protected void AddInput(ResourceType type)
    {
        if (IsConsuming(type) == false)
        {
            Inputs.Add(type);
        }
    }
    protected void RemoveInput(ResourceType type)
    {
        if (IsConsuming(type))
        {
            Inputs.Remove(type);
        }
    }
    protected void AddOutput(ResourceType type)
    {
        if (IsProducing(type) == false)
        {
            Outputs.Add(type);
        }
    }
    protected void RemoveOutput(ResourceType type)
    {
        if (IsProducing(type))
        {
            Outputs.Remove(type);
        }
    }

    protected void AddResource(ResourceType type, float amount)
    {
        if (amount > 0)
        {
            resources[type] += amount;
            UpdateInventory();
        }
    }

    public void Receive(ResourceType type, float amount)
    {
        if (IsConsuming(type))
        {
            AddResource(type, amount);
        }
    }

    /// <summary>
    /// Varmista että resurssia on ja että sitä ei oteta enempää kuin sitä on varastossa
    /// </summary>
    /// <param name="type">Mitä otetaan</param>
    /// <param name="amount">Kuinka paljon pyydetään</param>
    /// <returns>Kuinka paljon annettiin</returns>
    public float Take(ResourceType type, float amount)
    {
        if (amount > 0 && IsProducing(type) && resources[type] > 1.0f)
        {
            float amountTaken = Math.Min(amount, resources[type]);
            resources[type] -= amountTaken;
            UpdateInventory();
            return amountTaken;
        }
        // Jos resurssia ei ole tai se on loppu, palauta 0
        return 0;
    }
    
    private void OnGUI()
    {
        Vector3 textpos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1);
        string text = buildingName;
        if (isSelected)
        {
            text = $"[ {buildingName } ]";
        }  
        
        // Ota Label elementin oletustyyli ja vaihda sen teksti isommaksi
        GUIStyle labelBig = new GUIStyle(GUI.skin.label);
        labelBig.fontSize = 14;
        // Käytä tätä tyyliä Label elementissä
        GUILayout.BeginArea(new Rect(textpos.x, Screen.height - textpos.y, 1000, 3000));
        GUILayout.Label(text, labelBig);
        GUILayout.Label(debugInfo, labelBig);
        GUILayout.Label(resourceInfo, labelBig);
        GUILayout.Label(inventoryInfo, labelBig);
        GUILayout.EndArea();
    }
}
