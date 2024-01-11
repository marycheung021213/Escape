using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using myGabber;

public class ChangeMaterials : MonoBehaviour
{
    // Start is called before the first frame update
    public Grabber grabber;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        print(grabber.m_grabMaxDistance);
    }
}
