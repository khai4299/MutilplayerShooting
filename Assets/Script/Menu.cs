using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public HostGame host;

    public  void JoinMatch()
    {
        host.Join();
    }
    public void CreateMatch()
    {
        host.Create();
    }
    public void Quit()
    {
        Application.Quit();
    }
}
