using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VaroniaBackOffice;

public class UITraduction : MonoBehaviour
{

    public Text txt;

    public string En_Trad;
    public string Es_Trad;

    IEnumerator  Start()
    {
        while (Config.VaroniaConfig== null)
        {
            yield return new WaitForSeconds(0.1f);
        }


        if (Config.VaroniaConfig.Language == "Fr")
        {

        }
        else if (Config.VaroniaConfig.Language == "Es")
        {
            txt.text = Es_Trad;
        }
        else 
        {
            txt.text = En_Trad;
        }


    }

}
