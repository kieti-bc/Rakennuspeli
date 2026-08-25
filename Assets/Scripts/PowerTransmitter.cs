using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// Tämä on erikoinen rakennus joka välittää sähköä
/// eteenpäin yhdelle tai useammalla rakennukselle joka
/// kuluttaa sitä.
/// Se välittää sähköä myös muille sähköpylväille
/// </summary>
public class PowerTransmitter : Building
{
    [SerializeField] Sprite powered;
    [SerializeField] Sprite unpowered;
    SpriteRenderer spriteRenderer;
    [SerializeField] GameObject powerlinePrefab;
    
    [SerializeField] float transferRate;
    
    // Mistä sähkö tulee
    [SerializeField] private Building input;
    
    // Mihin sähkö menee
    [SerializeField] private List<Building> outputs;
    private Dictionary<Building, GameObject> powerlinesToOutputs;
    private GameObject inputPowerline;

    // Tässä listassa on rakennuksia jotka on havaittu triggerillä
    // Niihin ehkä voidaan lähettää sähköä.
    private List<Building> possibleOutputs;

    // This is a shared preview powerline for all transmitters
    // it exists to avoid spawning extra powerlines
    static GameObject g_previewPowerline = null;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        input = null;
        outputs = new List<Building>();
        resources.Add(ResourceType.Power, 0);
        if (g_previewPowerline == null)
        {
            g_previewPowerline = GameObject.Instantiate(powerlinePrefab);
            g_previewPowerline.SetActive(false);
        }
        powerlinesToOutputs = new Dictionary<Building, GameObject>();
        possibleOutputs = new List<Building>();

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
            foreach (Building connection in outputs)
            {
                if (connection != input)
                {
                    connection.Receive(ResourceType.Power, Take(ResourceType.Power, amount));
                }
            }
        }
        else if (input != null)
        {
            // Sähköä tulee, mutta ei mene minnekään
            debugInfo += "Receiving";
        }
    }

    enum ConnectionType
    {
        None,
        Input,
        Output
    }

    /// <summary>
    /// Kytkeydy rakennukseen, yritä ottaa tai antaa sähköä.
    /// Palauta millainen yhteys olisi mahdollinen
    /// </summary>
    /// <param name="otherBuilding"></param>
    ConnectionType TryToConnect(Building otherBuilding)
    {
        // Jos sähköä ei vielä tule mistään ja toinen
        // rakennus tuotta sitä, merkitse se tulolähteeksi
        if (otherBuilding.IsProducing(ResourceType.Power))
        {
            // Kytke sähkön siirto päälle että
            // toiset pylväät voivat ottaa sähköä 
            // tästä pylväästä
            AddInput(ResourceType.Power);
            AddOutput(ResourceType.Power);
            spriteRenderer.sprite = powered;
            if (input == null)
            {
                input = otherBuilding;
            }
            return ConnectionType.Input;
        }
        // Jos toinen rakennus kuluttaa sähköä,
        // lisää se siirtokohteisiin
        else if (otherBuilding.IsConsuming(ResourceType.Power))
        {
            AddInput(ResourceType.Power);
            AddOutput(ResourceType.Power);
            
            // Varmista että kohde ei ole sama kuin tulo: muuten pylväät tuottavat loputtamasti sähköä :O
            if (input != null && otherBuilding != input)
            {
				return ConnectionType.Output;
            }
        }
        return ConnectionType.None;
    }

    void ConnectPowerLineTo(GameObject powerline, Building target)
    {
		Vector3 sourcePos = this.transform.position + Vector3.up;
		Vector3 targetPos = target.transform.position + Vector3.up;
		Vector3 toTarget = (target.transform.position + Vector3.up) - sourcePos;

        powerline.transform.position = sourcePos;
		powerline.transform.rotation = Quaternion.LookRotation(toTarget);
		powerline.transform.localScale = new Vector3(1.0f, 1.0f, toTarget.magnitude);
	}

    bool HasConnectionTo(Building target)
    {
        foreach(Building c in outputs)
        {
            if (c == target)
            {
                return true;
            }
        }
        return false;
    }
    void RemoveConnectionTo(Building target)
    {
        foreach (Building c in outputs)
        {
            if (c == target)
            {
                outputs.Remove(c);
                break;
            }
        }
    }

	public override void OnConstructionComplete()
	{
        // Add powerline to input
        if (inputPowerline == null)
        {
			inputPowerline = GameObject.Instantiate(powerlinePrefab, transform);
			ConnectPowerLineTo(inputPowerline, input);
		}

		// Add powerlines to any new outputs
        foreach (Building b in possibleOutputs)
        {
			// Tarkista että ei ole vielä yhdistetty
            // eikä luotu johtoa
			if (HasConnectionTo(b) == false)
			{
				outputs.Add(b);
                if (powerlinesToOutputs.ContainsKey(b) == false)
                {
                    GameObject powerline = GameObject.Instantiate(powerlinePrefab, transform);
                    ConnectPowerLineTo(powerline, b);
                    powerlinesToOutputs.Add(b, powerline);
                }
			}
        }
        possibleOutputs.Clear();
		g_previewPowerline.SetActive(false);
	}

	void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            if (possibleOutputs.Count == 0)
            {
				g_previewPowerline.SetActive(true);
			}

            Building otherBuilding = other.gameObject.GetComponent<Building>();

            ConnectionType ctype = TryToConnect(otherBuilding);

			if (ctype == ConnectionType.Output)
            {
                if (possibleOutputs.Contains(otherBuilding) == false)
                {
                    possibleOutputs.Add(otherBuilding);
                }

				// Päivitä esikatselujohto osoittamaan tähän rakennukseen
				ConnectPowerLineTo(g_previewPowerline, otherBuilding);
			}
            else if (ctype == ConnectionType.Input)
            {
				// Päivitä esikatselujohto osoittamaan tähän rakennukseen
				ConnectPowerLineTo(g_previewPowerline, otherBuilding);
			}
		}
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            Building otherBuilding = other.gameObject.GetComponent<Building>();
            if (TryToConnect(otherBuilding) != ConnectionType.None)
            {
				// Päivitä esikatselujohto osoittamaan tähän rakennukseen
				ConnectPowerLineTo(g_previewPowerline, otherBuilding);
			}
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
                if (otherBuilding == input)
                {
					spriteRenderer.sprite = unpowered;
					input = null;
                }
            }
            else if (otherBuilding.IsConsuming(ResourceType.Power))
            {
                possibleOutputs.Remove(otherBuilding);
            }

            // Jos kaikki kohteet ja tulo ovat poikki
            // lopeta sähkön siirtäminen ja vastaanottaminen
            if (input == null && outputs.Count == 0)
            {
                RemoveOutput(ResourceType.Power);
                RemoveInput(ResourceType.Power);
				g_previewPowerline.SetActive(false);
			}
        }
    }
}
