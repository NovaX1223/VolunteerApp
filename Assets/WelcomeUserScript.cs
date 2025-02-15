using UnityEngine;
using UnityEngine;
using TMPro;
public class WelcomeUserScript : MonoBehaviour
{

    public  TextMeshProUGUI welcomeusertext;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WelcomeUser(string username) 
    { 
    
    welcomeusertext.text= "Welcome" + username; 
    
    
    
    }




}
