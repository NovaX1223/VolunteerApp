using UnityEngine;
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Collections;
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

        // Fetch existing posts on startup
       // LoadAllMessages();

        // Attach a listener for real-time updates
        FirebaseDatabase.DefaultInstance
            .GetReference("messages")
            .ValueChanged += HandleDatabaseUpdate;
    }




    // Update is called once per frame
    void Update()
    {

    }

    public void WelcomeUser(string username) 
    { 
    
    welcomeusertext.text= "Welcome" + username; 
    
    
    
    }
    void HandleDatabaseUpdate(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        Debug.Log("Database updated! Waiting before loading messages...");

        // Start a coroutine to introduce a delay
        StartCoroutine(DelayedLoadMessages(0.1f)); // 1-second delay
    }

    // Coroutine to delay loading messages
    private IEnumerator DelayedLoadMessages(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Debug.Log("Delay over, reloading messages...");
        LoadAllMessages();
    }

    public void LoadAllMessages()
    {
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

                if (task.IsCompletedSuccessfully)
                {
                    DataSnapshot snapshot = task.Result;

                    //  Debugging: Print the entire Firebase JSON data
                    Debug.Log("Firebase Data: " + snapshot.GetRawJsonValue());

                    if (snapshot.ChildrenCount == 0)
                    {
                        Debug.LogWarning("No messages found in database.");
                        return;
                    }

                    // Clear previous messages
                    foreach (Transform child in contentParent)
                    {
                        Destroy(child.gameObject);
                    }

                    // Iterate through each message
                    foreach (DataSnapshot postSnapshot in snapshot.Children)
                    {
                        Dictionary<string, object> postDict = postSnapshot.Value as Dictionary<string, object>;

                        if (postDict == null)
                        {
                            Debug.LogWarning("Empty post data, skipping.");
                            continue;
                        }

                        string message = postDict.ContainsKey("Message") ? postDict["Message"].ToString() : "";
                        string name = postDict.ContainsKey("Name") ? postDict["Name"].ToString() : "";
                        string time = postDict.ContainsKey("Time") ? postDict["Time"].ToString() : "";

                        Debug.Log($"Loaded Post - Name: {name}, Time: {time}, Message: {message}");

                        GameObject newPostItem = Instantiate(postItemPrefab, contentParent);
                        TextMeshProUGUI postText = newPostItem.GetComponent<TextMeshProUGUI>();

                        postText.text = $"Name: {name}\nTime: {time}\nMessage: {message}";
                        postText.enabled = true;
                    }
                }
            });
    }



}
