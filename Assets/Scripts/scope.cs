using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scope : MonoBehaviour
{

    public Material scopeRenderMaterial;
    public Color defaultColor;
    // Start is called before the first frame update
    void Start()
    {
        scopeRenderMaterial.color = defaultColor;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
