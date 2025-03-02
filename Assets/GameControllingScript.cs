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


public class GameControllingScript : MonoBehaviour
{

    public GameObject InitialPage;
    public GameObject LoginPage;
    public GameObject WelcomePage;
    public TMP_InputField LoginEmailInput;
    public TMP_InputField LoginPasswordInput;
    public TMP_InputField InputMessage;



    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    public string currentusername;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {

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

    }



    public void PressedLoginPageSignUpButton()
    {
        string enteredemailText = LoginEmailInput.text;
        string enteredpasswordText = LoginPasswordInput.text;

        Debug.Log("User typed email: " + enteredemailText + " This was password: " + enteredpasswordText);

        CreateUser(enteredemailText, enteredpasswordText, enteredemailText);

    }

    void CreateUser(string email, string password, string Username)
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

            currentusername = result.User.DisplayName;


        });
    }



    void WelcomeUser()
    {
        InitialPage.SetActive(false);
        LoginPage.SetActive(false);
        WelcomePage.SetActive(true);


    }

    public void StartFromBeginning()
    {
        InitialPage.SetActive(true);
        LoginPage.SetActive(false);
        WelcomePage.SetActive(false);
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
                currentusername = user.DisplayName; 

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
        string key ="" + GetSecondsSince2024();
        string value = InputMessage.text;
        string formattedDate = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");

        dbReference.Child("messages").Child(key).Child("Name").SetValueAsync(currentusername)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("String posted successfully!");
                }
                else
                {
                    Debug.LogError("Failed to post string: " + task.Exception);
                }
            });



        dbReference.Child("messages").Child(key).Child("Time").SetValueAsync(formattedDate)
      .ContinueWith(task =>
      {
          if (task.IsCompleted)
          {
              Debug.Log("String posted successfully!");
          }
          else
          {
              Debug.LogError("Failed to post string: " + task.Exception);
          }
      });

        dbReference.Child("messages").Child(key).Child("Message").SetValueAsync(value)
      .ContinueWith(task =>
      {
          if (task.IsCompleted)
          {
              Debug.Log("String posted successfully!");
          }
          else
          {
              Debug.LogError("Failed to post string: " + task.Exception);
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
