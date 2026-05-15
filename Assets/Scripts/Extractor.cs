using System;
using UnityEngine;

/// <summary>
/// Tämä rakennus kerää maastosta jotakin resurssia
/// </summary>
public class Extractor : Building
{
    // Kuinka nopeasti resurssia kerätään
    [SerializeField] private float extractionSpeed;
    
    void Start()
    {
        OnStart(); // Building luokan Start
        name = "Extractor";
        debugInfo = "Not Extracting";
    }

    // Kun jokin resurssi on Trigger alueella, yritä
    // ottaa sitä talteen
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Resource"))
        {
            Resource resource = other.GetComponent<Resource>();
            // Jos resurssi on sellainen mitä tämä rakennus
            // tuottaa, ota sitä talteen
            if (IsProducing(resource.resourceType))
            {
                debugInfo = $"Extracting {resource.resourceType}";
                AddResource(resource.resourceType, extractionSpeed*Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Tätä tarvitaan siihen että rakennuksen sijoittelun
    /// esikatselussa näkyy onko paikka sopiva vai ei
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Resource"))
        {
            Resource resource = other.GetComponent<Resource>();
            if (IsProducing(resource.resourceType))
            {
                // TODO Multiple inputs possible
                debugInfo = $"Not Extracting";
            }
        }
    }
}
