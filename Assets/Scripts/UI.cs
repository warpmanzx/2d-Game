using UnityEngine;

public class Ui : MonoBehaviour
{
    public static Ui instance;

    private void Awake()
    {
            instance = this;
    } // F�r att kunna calla funktions fr�n den h�r
      // klassen kan man kalla genom att skriva Ui.instance.FunktionsNamn();

    private void Update()
    {
        
    }
}
