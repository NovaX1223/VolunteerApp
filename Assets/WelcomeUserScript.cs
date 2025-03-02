using UnityEngine;
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
public class WelcomeUserScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject postItemPrefab;  // The prefab with texts
    [SerializeField] private Transform contentParent;    // The "Content" Transform under your Scroll View

    private DatabaseReference dbReference;

    public TextMeshProUGUI welcomeusertext;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Fetch posts on startup
        LoadAllMessages();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WelcomeUser(string username) 
    { 
    
    welcomeusertext.text= "Welcome" + username; 
    
    
    
    }

    public void LoadAllMessages()
    {
        // Retrieve all children under "messages"
        FirebaseDatabase.DefaultInstance
            .GetReference("messages")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error fetching messages: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    // Optionally clear existing items in the list
                    foreach (Transform child in contentParent)
                    {
                        Destroy(child.gameObject);
                    }

                    // Iterate through each child (each post)
                    foreach (DataSnapshot postSnapshot in snapshot.Children)
                    {
                        // postSnapshot.Key might be "36815481", etc.

                        // Approach A: read as dictionary
                        Dictionary<string, object> postDict = (Dictionary<string, object>)postSnapshot.Value;

                        // Extract fields (assuming they match your structure)
                        string message = postDict.ContainsKey("Message") ? postDict["Message"].ToString() : "";
                        string name = postDict.ContainsKey("Name") ? postDict["Name"].ToString() : "";
                        string time = postDict.ContainsKey("Time") ? postDict["Time"].ToString() : "";

                        // Instantiate a prefab for this post
                        GameObject newPostItem = Instantiate(postItemPrefab, contentParent);

                        // Now set the text fields inside the prefab 
                        // (assuming they have child Text elements or a custom script)
                        TextMeshProUGUI postText = newPostItem.GetComponent<TextMeshProUGUI>();

                        // Make your combined text
                        string combinedText = $"Name: {name}\nTime: {time}\nMessage: {message}";

                        // Assign
                        postText.text = combinedText;
                        postText.enabled = true;
                        // The child names ("NameText", "MessageText", etc.) must match your prefab
                    }
                }
            });
    }


}
