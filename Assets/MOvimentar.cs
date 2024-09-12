using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MOvimentar : MonoBehaviour
{
    [SerializeField] Transform cubeTransform;
    public float distacne=0f;
    // Start is called before the first frame update
    void Start()
    {
        cubeTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        cubeTransform.position = new Vector3(0, 0, distacne);
        
    }

    public void Mover(float newDistance)
    {
        distacne = newDistance*-5;
    }


}
