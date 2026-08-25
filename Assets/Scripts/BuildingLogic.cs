using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingLogic : MonoBehaviour
{
	// Mitä rakennusta pelaaja on rakentamassa?
	// previewBuilding on prefabista luotu GameObject
	// jota liikutetaan hiirellä rakentamisen aikana
	private GameObject selectedPrefab = null;
	private GameObject preview = null;

	// Tämä muuttuja tallentaa voiko valittuna olevan
	// rakennuksen rakentaa hiiren osoittamaan kohtaan
	// joka on buildingPos
	private bool canBuildHere = false;
	private Vector3 buildingPos = Vector3.zero;

	// Viite olioon joka hallitsee valikoita
	private MenuController menuController;

	// Viite inputtiin
	private InputAction selectAction;
	private InputAction deselectAction;

	/// <summary>
	/// Missä tilassa ollaan, määrittää sen miten hiiren klikkaukset
	/// tulkitaan ja mitä ne tekevät
	/// </summary>
	public enum InputMode
	{
		Selecting, // Selecting unit or building type from menu
		Building, // Placing a building
		Vehicle, // Placing a vehicle
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
		menuController.orderButtonPressed += OnOrderButton;
		menuController.vehicleButtonPressed += OnVehicleButton;

		// Alussa ollaan valitsemistilassa ja mitään ei ole valittuna
		activeMode = InputMode.Selecting;
		selectedItem = null;



		selectAction = InputSystem.actions.FindAction("Select", true);
		deselectAction = InputSystem.actions.FindAction("Deselect", true);
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
		if (Physics.Raycast(selectRay.origin, selectRay.direction, out RaycastHit hit, 1000, raymask, QueryTriggerInteraction.Ignore))
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
	void DoSelect(bool clicked)
	{
		if (!clicked)
		{
			return;
		}
		switch (activeMode)
		{
			case InputMode.Selecting:
				HandleSelectingInput();
				break;
			case InputMode.Vehicle:
				goto case InputMode.Building;
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
	/// Jos jokin rakennus tai ajoneuvo prefab on valittu, hiiren painallus
	/// yrittää sijoittaa sen kenttään jos kursorin kohdalla on
	/// tyhjää, eli säde osuu maahan
	/// </summary>
	void HandleBuildingInput()
	{
		if (selectedPrefab != null)
		{
			RaycastHit hit = RayToGroundOrBuilding();

			// Jos osuu maahan...
			if (hit.collider != null && hit.collider.CompareTag("Ground"))
			{
				if (preview != null)
				{
					// Aseta rakennuksen paikka ja vaihda sen tasoksi Building
					preview.transform.position = hit.point;
					if (activeMode == InputMode.Building)
					{
						preview.layer = LayerMask.NameToLayer("Building");
					}
					else if (activeMode == InputMode.Vehicle)
					{
						preview.layer = LayerMask.NameToLayer("Vehicle");
					}
					preview.SetActive(true);
					// Ilmoita rakennukselle että se on valmis
					Building bcomp = preview.GetComponent<Building>();
					if (bcomp)
					{
						bcomp.OnConstructionComplete();
					}
					// Lopeta esikatselu poistamalla viite rakennukseen
					preview = null;
					activeMode = InputMode.Selecting;
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
				bool added = vehicle.AddOrder(order);
				if (added)
				{
					menuController.SetOrderListText(vehicle.OrdersToString());
				}
			}
		}
	}

	/// <summary>
	/// Tätä funktiota kutsutaan kun hiiren oikeaa näppäintä painetaan
	/// Kytketty toimintoon Deselect
	/// </summary>
	/// <param name="value"></param>
	void DoDeselect(bool clicked)
	{
		if (!clicked)
		{
			return;
		}
		switch (activeMode)
		{
			case InputMode.Selecting:
				ClearSelection();
				break;

			case InputMode.Building:
				// Jos ollaan rakentamassa, tuhoa esikatselurakennus
				// ja lopeta esikatselu
				if (preview != null)
				{
					preview.SetActive(true);
					GameObject.Destroy(preview);
				}

				preview = null;
				selectedPrefab = null;
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
		DoSelect(selectAction.ReadValue<float>()>0);
		DoDeselect(deselectAction.ReadValue<float>()>0);

		switch (activeMode)
		{
			case InputMode.Selecting: break;

			case InputMode.Vehicle:
				goto case InputMode.Building;
			case InputMode.Building:
				{
					// Tarkista jatkuvasti voiko hiiren osoittamaan
					// kohtaan rakentaa.
					// Mutta tarkista vain jos on valittu rakennustyyppi
					if (selectedPrefab != null && preview != null)
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
								preview.SetActive(true);
								preview.transform.position = buildingPos;
								canBuildHere = true;
							}
							else
							{
								// Jos osui olemassa olevaan rakennukseen,
								// laita esikatselurakennus pois päältä
								preview.SetActive(false);
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
		if (preview != null && selectedPrefab != null)
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
		selectedPrefab = buildingPrefab;
		CreatePreview(selectedPrefab);
		activeMode = InputMode.Building;
	}

	private void OnVehicleButton(GameObject vehiclePrefab)
	{
		selectedPrefab = vehiclePrefab;
		CreatePreview(selectedPrefab);
		activeMode = InputMode.Building;
	}

	void CreatePreview(GameObject prefabBuilding)
	{
		// Jos jokin on jo valittuna, poista se ensin
		if (preview != null)
		{
			GameObject.Destroy(preview);
		}
		preview = Instantiate(prefabBuilding, Vector3.zero, Quaternion.identity);
		// Put preview on different layer so it does not interfere with placement preview rays
		preview.layer = LayerMask.NameToLayer("Default");
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
