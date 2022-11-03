using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/// <summary>
/// This class handles all the requests and serialization and
/// deserialization of data.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    // reference to the BotUI class
    public BotUI botUI;
    // the url at which the bot's custom component is hosted
    private const string rasa_url = "http://127.0.0.1:5005/webhooks/unity/webhook";
    private const string rasa_url_public = "http://172.104.91.57:5678/posts";
    public bool isPublic = true;
    public ListStringEventChannelSO AcupuncturePointEvent;
    List<string> AcupuncturePointList = new List<string>();

    /// <summary>
    /// This method is called when user has entered their message and hits
    /// the send button. It calls the <see cref="NetworkManager.PostRequest"/> coroutine
    /// to send the user message to bot and also updates UI with the users message.
    /// </summary>
    public void SendNetMessage(string value)
    {
        // Get message from textbox and clear the input field
        string message = value;
        botUI.input.text = "";

        // Create a json object from user message
        PostData postMessage = new PostData
        {
            sender = "doku",
            message = message,
            // metadata = JsonUtility.ToJson("{'id': '000'}"),  // fail
        };

        string jsonBody = JsonUtility.ToJson(postMessage);
        //Debug.Log(jsonBody);

        // Update display
        botUI.UpdateDisplay("Doku", message, "text");

        // Create a post request with the data to send to Rasa server
        
        if(isPublic) StartCoroutine(PostRequest(rasa_url_public, jsonBody));
        else StartCoroutine(PostRequest(rasa_url, jsonBody));
    }

    /// <summary>
    /// This method updates the UI with the bot's response.
    /// </summary>
    /// <param name="response">The response json recieved from the bot</param>
    public void RecieveMessage(string response)
    {
        // Deserialize response recieved from the bot
        RootMessages recieveMessages =
            JsonUtility.FromJson<RootMessages>("{\"messages\":" + response + "}");

        //Debug.Log(response);
        //Debug.Log(ConvertJsonStringToSting(response));

        // show message based on message type on UI
        foreach (RecieveData message in recieveMessages.messages)
        {
            //Debug.Log(message.text);

            string[] word;
            if (message.text.Contains("建議按的穴位"))
            {
                word = message.text.Substring(8).Split('、');
                foreach(string s in word)
                {
                    AcupuncturePointList.Add(s);
                }
                Debug.Log(AcupuncturePointList.Count);
                AcupuncturePointEvent.RaiseEvent(AcupuncturePointList);
                AcupuncturePointList.Clear();
            }

            FieldInfo[] fields = typeof(RecieveData).GetFields();
            foreach (FieldInfo field in fields)
            {
                string data = null;

                // extract data from response in try-catch for handling null exceptions
                try
                {
                    data = field.GetValue(message).ToString();
                }
                catch (NullReferenceException) { }

                // print data
                if (data != null && field.Name != "recipient_id")
                {
                    botUI.UpdateDisplay("Bot", data, field.Name);
                }
            }
        }
    }

    

    /// <summary>
    /// This is a coroutine to asynchronously hit the server url with users message
    /// wrapped in request. The response is deserialized and rendered on the UI
    /// </summary>
    /// <param name="url">The url where the rasa server's custom connector is located</param>
    /// <param name="jsonBody">User message serialized into a json object</param>
    /// <returns></returns>
    private IEnumerator PostRequest(string url, string jsonBody)
    {
        // Create a request to hit the rasa custom connector
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);

        // Handlers
        request.uploadHandler = new UploadHandlerRaw(rawBody);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // make sure this gets cleaned up.  
        // ADD IN THESE TWO LINES
        request.disposeCertificateHandlerOnDispose = true;
        request.disposeDownloadHandlerOnDispose = true;

        // recieve the response asynchronously
        yield return request.SendWebRequest();

        // Show response on UI
        if (request.downloadHandler.text.Length == 0)
        {
            Debug.LogWarning("There's no response from server. Check if the server is ready!");
            yield break;
        }
        RecieveMessage(request.downloadHandler.text);
        // Dispose of this !!!
        request.Dispose();
    }

    /// <summary>
    /// This method gets url resource from link and applies it to the passed texture.
    /// </summary>
    /// <param name="url">url where the image resource is located</param>
    /// <param name="image">RawImage object on which the texture will be applied</param>
    /// <returns></returns>
    public IEnumerator SetImageTextureFromUrl(string url, Image image)
    {
        // Send request to get the image resource
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
            // image could not be retrieved
            Debug.Log(request.error);

        else
        {
            // Create Texture2D from Texture object
            Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Texture2D texture2D = texture.ToTexture2D();

            // set max size for image width and height based on chat size limits
            float imageWidth = 0, imageHeight = 0, texWidth = texture2D.width, texHeight = texture2D.height;
            if ((texture2D.width > texture2D.height) && texHeight > 0)
            {
                // Landscape image
                imageWidth = texWidth;
                if (imageWidth > 200) imageWidth = 200;
                float ratio = texWidth / imageWidth;
                imageHeight = texHeight / ratio;
            }
            if ((texture2D.width < texture2D.height) && texWidth > 0)
            {
                // Portrait image
                imageHeight = texHeight;
                if (imageHeight > 200) imageHeight = 200;
                float ratio = texHeight / imageHeight;
                imageWidth = texWidth / ratio;
            }

            // Resize texture to chat size limits and attach to message 
            // Image object as sprite
            TextureScale.Bilinear(texture2D, (int)imageWidth, (int)imageHeight);
            image.sprite = Sprite.Create(
                texture2D,
                new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f), 100.0f);

            // Resize and reposition all chat bubbles
            StartCoroutine(botUI.RefreshChatBubblePosition());
        }
    }

    private string ConvertJsonStringToSting(string json_string)
    {
        json_string.Replace("[", "'").Replace("]", "'");
        JObject json = JObject.Parse(json_string.Replace("]", "").Replace("[", ""));
        foreach (var e in json)
        {
            Debug.Log(e);
            if (e.Key == "text") return e.Value.ToString();
        }
        return json_string;
    }

    public void SetPublic()
    {
        isPublic = true;
    }
    public void SetPrivate()
    {
        isPublic = false;
    }
}