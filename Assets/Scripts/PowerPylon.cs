using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tämä on erikoinen rakennus joka välittää sähköä
/// eteenpäin yhdelle tai useammalla rakennukselle joka
/// kuluttaa sitä.
/// Se välittää sähköä myös muille sähköpylväille
/// </summary>
public class PowerPylon : Building
{
    [SerializeField] Sprite powered;
    [SerializeField] Sprite unpowered;
    SpriteRenderer spriteRenderer;
    
    [SerializeField] float transferRate;
    
    // Mistä sähkö tulee
    [SerializeField] private Building input;
    
    // Mihin sähkö menee
    [SerializeField] private List<Building> outputs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        input = null;
        outputs = new List<Building>();
        resources.Add(ResourceType.Power, 0);
    }

    // Update is called once per frame
    void Update()
    {
        debugInfo = "";
        if (input != null && outputs.Count > 0)
        {
            debugInfo += "Transferring";
            // Kuinka paljon sähköä siirretään
            float fullAmount = transferRate * Time.deltaTime;
            // Yritä ottaa koko määrä
            float taken = input.Take(ResourceType.Power, fullAmount);
            AddResource(ResourceType.Power, taken);
            
            // Jaa saatu sähkö tasan siirtokohteiden kesken
            float amount =  taken /  outputs.Count;
            foreach (Building building in outputs)
            {
                if (building != input)
                {
                    building.Receive(ResourceType.Power, Take(ResourceType.Power, amount));
                }
            }
        }
        else if (input != null)
        {
            // Sähköä tulee, mutta ei mene minnekään
            debugInfo += "Receiving";
        }
    }

    /// <summary>
    /// Kytkeydy rakennukseen, yritä ottaa tai antaa sähköä
    /// </summary>
    /// <param name="otherBuilding"></param>
    void TryToConnect(Building otherBuilding)
    {
        // Jos sähköä ei vielä tule mistään ja toinen
        // rakennus tuotta sitä, merkitse se tulolähteeksi
        if (input == null && otherBuilding.IsProducing(ResourceType.Power))
        {
            // Kytke sähkön siirto päälle että
            // toiset pylväät voivat ottaa sähköä 
            // tästä pylväästä
            AddInput(ResourceType.Power);
            AddOutput(ResourceType.Power);
            spriteRenderer.sprite = powered;
            input = otherBuilding;
        }
        // Jos toinen rakennus kuluttaa sähköä,
        // lisää se siirtokohteisiin
        else if (otherBuilding.IsConsuming(ResourceType.Power))
        {
            AddInput(ResourceType.Power);
            AddOutput(ResourceType.Power);
            
            // Varmista että rakennus ei ole kohteissa kahdesti
            // eikä kohde ole sama kuin tulo: muuten pylväät tuottavat loputtamasti sähköä :O
            if (outputs.Contains(otherBuilding) == false && (input != null && otherBuilding != input))
            {
                outputs.Add(otherBuilding);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            Building otherBuilding = other.gameObject.GetComponent<Building>();
            TryToConnect(otherBuilding);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            Building otherBuilding = other.gameObject.GetComponent<Building>();
            TryToConnect(otherBuilding);
        }
    }

    // Jos rakennus katoaa Trigger alueelta
    // katkaise yhteys siihen
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            Building otherBuilding = other.gameObject.GetComponent<Building>();
            if (otherBuilding.IsProducing(ResourceType.Power))
            {
                spriteRenderer.sprite = unpowered;
                if (otherBuilding == input)
                {
                    input = null;
                }
            }
            else if (otherBuilding.IsConsuming(ResourceType.Power))
            {
                outputs.Remove(otherBuilding);
            }

            // Jos kaikki kohteet ja tulo ovat poikki
            // lopeta sähkön siirtäminen ja vastaanottaminen
            if (input == null && outputs.Count == 0)
            {
                RemoveOutput(ResourceType.Power);
                RemoveInput(ResourceType.Power);
            }
        }
    }
}
