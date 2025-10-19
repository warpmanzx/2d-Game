using UnityEngine;

public class Ui : MonoBehaviour
{
    public static Ui instance;

    private void Awake()
    {
            instance = this;
    } // För att kunna calla funktions från den här
      // klassen kan man kalla genom att skriva Ui.instance.FunktionsNamn();

    private void Update()
    {
        
    }
}
