using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Database;
using System.Collections;
using UnityEditor;


public class GameControllingScript : MonoBehaviour
{
    public GameObject MessagePage;
    public GameObject InitialPage;
    public GameObject LoginPage;
    public GameObject WelcomePage;
    public TMP_InputField LoginEmailInput;
    public TMP_InputField LoginPasswordInput;
    public TMP_InputField InputMessage;
    public string selectedBase64Image = "empty"; // default value



    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    public string currentusername;
    public WelcomeUserScript welcomeUserScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartCoroutine(CallAfterDelay());

        InitializeFirebaseDatabase();
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();
                Debug.Log("Firebase is ready");

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase(); // Initialize Firebase Auth
                InitializeFirebaseDatabase(); // Initialize Firebase Database
                Debug.Log("Firebase is ready");
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });



    }


    IEnumerator CallAfterDelay()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Call your method
        SignOutUser();
    }

    // Update is called once per frame
    void Update()
    {

    }



    void SaveUserToDatabase(string email, string username, string base64Image)
    {
        string encodedEmail = email.Replace(".", "_").Replace("@", "_");

        UserData userData = new UserData(email, username, base64Image);
        string json = JsonUtility.ToJson(userData);

        dbReference.Child("user").Child(encodedEmail).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User data with image saved to database.");
            }
            else
            {
                Debug.LogError("Error saving user data: " + task.Exception);
            }
        });
    }


    [Serializable]
    public class UserData
    {
        public string email;
        public string username;
        public string profileImage;

        public UserData(string email, string username, string profileImage)
        {
            this.email = email;
            this.username = username;
            this.profileImage = profileImage;
        }
    }



    public void PressedUserLoginButton()
    {
        InitialPage.SetActive(false);
        LoginPage.SetActive(true);

    }


    public void LoginButtonPressed()
    {
        string enteredemailText = LoginEmailInput.text;
        string enteredpasswordText = LoginPasswordInput.text;

        Debug.Log("User typed email: " + enteredemailText + " This was password: " + enteredpasswordText);

        SignInUser(enteredemailText, enteredpasswordText);

        currentusername = enteredemailText;

    }

  

    public void PressedLoginPageSignUpButton()
    {
        string enteredemailText = LoginEmailInput.text;
        string enteredpasswordText = LoginPasswordInput.text;
       

        Debug.Log("User typed email: " + enteredemailText + " This was password: " + enteredpasswordText);

        CreateUser(enteredemailText, enteredpasswordText, enteredemailText, selectedBase64Image );

    }

    void CreateUser(string email, string password, string username, string base64Image)

    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });

        UpdateUserProfile(email);

        SaveUserToDatabase(email, username, base64Image);
    }



    public void SelectImageFromFile()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadAndCompressImage(path));
        }
#else
        Debug.LogWarning("File picker not yet implemented for mobile.");
#endif
    }



    IEnumerator LoadAndCompressImage(string path)
    {
        byte[] imageBytes = System.IO.File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes); // Load image from byte array

        // Compress the image (resize or lower quality)
        Texture2D resized = ResizeTexture(texture, 64, 64); // Resize to 128x128
        byte[] compressedBytes = resized.EncodeToJPG(10); // 50 = compression quality (0-100)

        selectedBase64Image = Convert.ToBase64String(compressedBytes); // <-- store in field
        Debug.Log($"Image compressed. Final size: {selectedBase64Image.Length / 1024} KB");

        yield break;
    }

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(newWidth, newHeight);
        newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTex;
    }




    void SaveImageStringToFirebase(string base64Image)
    {
        string email = auth.CurrentUser.Email;
        string encodedEmail = email.Replace(".", "_").Replace("@", "_");

        dbReference.Child("user").Child(encodedEmail).Child("profileImage").SetValueAsync(base64Image).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Image string saved successfully.");
            }
            else
            {
                Debug.LogError("Failed to save image string: " + task.Exception);
            }
        });
    }
 



































    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

           // currentusername = result.User.DisplayName;


        });
    }


    public void ShowMessageButtonPressed()
    {
        InitialPage.SetActive(false);
        LoginPage.SetActive(false);
        WelcomePage.SetActive(false);
        MessagePage.SetActive(true);


    }
    void WelcomeUser()
    {
        InitialPage.SetActive(false);
        LoginPage.SetActive(false);
        WelcomePage.SetActive(true);
        MessagePage.SetActive(false);

    }

    public void StartFromBeginning()
    {
        InitialPage.SetActive(true);
        LoginPage.SetActive(false);
        WelcomePage.SetActive(false);
        MessagePage.SetActive(false);
    }




    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);

                StartFromBeginning();

            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                //currentusername = user.DisplayName; 

                WelcomeUser();

            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void UpdateUserProfile(string UserName)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = UserName,
                PhotoUrl = new System.Uri("https://makerworld.bblmw.com/makerworld/model/US6c39e1cc457c18/design/2024-02-28_0ff994fea2df8.jpeg"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }
    }

    public void SignOutUser()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("User signed out successfully.");

            // Reset the UI to the initial page
            StartFromBeginning();
        }
        else
        {
            Debug.LogError("Auth instance is null, cannot sign out.");
        }
    }


    private DatabaseReference dbReference; // Reference to Firebase Realtime Database




    void InitializeFirebaseDatabase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Realtime Database initialized");
    }

    public void PostStringToDatabase()
    {
        string key = GetSecondsSince2024().ToString();
        string value = InputMessage.text;
        string formattedDate = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");

        Task nameTask = dbReference.Child("messages").Child(key).Child("Name").SetValueAsync(currentusername);
        Task timeTask = dbReference.Child("messages").Child(key).Child("Time").SetValueAsync(formattedDate);
        Task messageTask = dbReference.Child("messages").Child(key).Child("Message").SetValueAsync(value);

        Task.WhenAll(nameTask, timeTask, messageTask).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("All messages posted successfully!");

          
            }
            else
            {
                Debug.LogError("Failed to post all messages: " + task.Exception);
            }
        });

    }




    int GetSecondsSince2024()
    {
        // Define midnight UTC on January 1, 2024
        DateTime startOf2024 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Get the current time in UTC
        DateTime now = DateTime.UtcNow;

        // Get the difference
        TimeSpan elapsed = now - startOf2024;

        // Return total seconds as an int (could overflow if it's too large)
        return (int)elapsed.TotalSeconds;
    }

}
