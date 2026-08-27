using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Tämä on kaikkien rakennusten abstracti yläluokka
/// 
/// Rakennukset tuottavat ja kuluttavat resursseja
/// sekä säilövät niitä inventaariossaan
/// 
/// Rakennukset voi valita ISelectable rajapinnan kautta.
/// </summary>
public abstract class Building : MonoBehaviour, ISelectable
{
    // Onko rakennus tällä hetkellä valittuna
    bool isSelected = false;

    // Mitä rakennus tuottaa ja kuluttaa ja mitä
    // sillä on varastossaan
    [SerializeField] protected List<ResourceType> Inputs;
    [SerializeField] protected List<ResourceType> Outputs;
    [SerializeField] protected Dictionary<ResourceType, float> storedResources;
    
    // Ruudulla näkyvät tiedot rakennuksesta
    protected string buildingName;
    protected string debugInfo;
    protected string resourceInfo;
    protected string inventoryInfo;

    // Rakennuksen vaikutusalue jos on. Voi olla myös null
    protected GameObject AreaIndicator = null;

    protected void OnStart()
    {
        // Etsi mahdollinen vaikutusalueen objekti
        Transform AreaTrans = gameObject.transform.Find("Area");
        if (AreaTrans)
        {
            AreaIndicator = AreaTrans.gameObject;
        }
        else
        {
            AreaIndicator = null;
        }

        // Jos vaikutusalue on olemassa, tee siitä samankokoinen kuin
        // Spherecolliderista
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

        // Luo resurssien sanakirja : Resurssin tyyppi -> Määrä varastossa
        storedResources = new Dictionary<ResourceType, float>(); 
        StringBuilder sb = new StringBuilder();
        
        // Lisää kaikki tuotetut ja kulutettavat resurssit
        // inventaarioon ja ruudulla näkyvään tekstiin
        foreach (ResourceType type in Inputs)
        {
            sb.Append($"in > {type}\n");
            // Jos resurssia ei vielä ole inventaariossa
            // lisää se sinne ja laita määräksi 0
            if (storedResources.ContainsKey(type) == false)
            {
                storedResources.Add(type, 0);
            }
        }
        foreach (ResourceType type in Outputs)
        {
            sb.Append($"{type} > out\n");
            if (storedResources.ContainsKey(type) == false)
            {
                storedResources.Add(type, 0);
            }
        }
        resourceInfo = sb.ToString();
    }

    // Päivitä tekstiä joka kertoo inventaarion tilanteen
    // tätä kutsutaan kun rakennus saa tai menettää resursseja
    void UpdateInventory()
    {
        StringBuilder sb = new StringBuilder();
        foreach (ResourceType type in storedResources.Keys)
        {
            sb.Append($"[{ type }]:{(int)storedResources[type]} ");
        }
        inventoryInfo = sb.ToString();
    }
    
    // Tätä kutsutaan kun rakennus valitaan
    public virtual void SetSelected(bool selected)
    {
        isSelected = selected;
        if (AreaIndicator)
        {
            AreaIndicator.SetActive(selected);
        }
    }

    // Tätä kutsutaan kun rakennus laitetaan kartalle.
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
            storedResources[type] += amount;
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
        if (amount > 0 && IsProducing(type) && storedResources[type] > 1.0f)
        {
            float amountTaken = Math.Min(amount, storedResources[type]);
            storedResources[type] -= amountTaken;
            UpdateInventory();
            return amountTaken;
        }
        // Jos resurssia ei ole tai se on loppu, palauta 0
        return 0;
    }
    
    // Näyttää rakennuksen tiedot
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
