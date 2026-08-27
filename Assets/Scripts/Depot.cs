using UnityEngine;
/// <summary>
/// Tämä rakennus rakentaa ajoneuvoja
/// ja määrittää milloin ajoneuvovalikko näkyy ruudulla
/// </summary>
public class Depot : Building
{
    private MenuController menuController;

    void Start()
    {
        OnStart();
        menuController = MenuController.GetController();
        name = "Depot";
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        if (selected)
        {
            // Show vehicle menu
            menuController.ShowVehicleMenu();
        }
        else 
        {
			// Show vehicle menu
			menuController.HideVehicleMenu();
		}
    }
}
