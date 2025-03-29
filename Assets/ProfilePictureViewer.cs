using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePictureViewer : MonoBehaviour
{

    [Header("Profile Picture")]
    [SerializeField] private Image profileImageUI;
    private DatabaseReference dbReference;


    public GameControllingScript gameControllingScript;


    public string img = "empty";
    bool flagimageloaded = false;
    public string username = "empty"; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        LoadUserProfileImage(gameControllingScript.currentusername); 

    } 



    // Update is called once per frame
    void Update()
    {
        if (img == "empty" )
        {
            LoadUserProfileImage(gameControllingScript.currentusername);
            username = gameControllingScript.currentusername; 
        }


    }



    public void LoadUserProfileImage(string email)
    {
        string encodedEmail = email.Replace(".", "_").Replace("@", "_");
        //Debug.Log ("Testing Email" + encodedEmail);
        dbReference.Child("user").Child(encodedEmail).Child("profileImage")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.IsCompleted || task.Result == null || task.Result.Value == null)
                {
                    Debug.LogWarning("Could not load profile image.");
                    return;
                }

                string base64Image = task.Result.Value.ToString();
                img = base64Image;
                byte[] imageBytes = System.Convert.FromBase64String(base64Image);

                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imageBytes))
                {
                    Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    profileImageUI.sprite = newSprite;
                }
                else
                {
                    Debug.LogWarning("Failed to convert Base64 to texture.");
                }
            });
    }

}
