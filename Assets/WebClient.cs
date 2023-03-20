using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class WebClient : MonoBehaviour
{   
    SpriteCreater sc;

    // Start is called before the first frame update
    void Start()
    {
        sc = GameObject.Find("SpriteCreater").GetComponent<SpriteCreater>();

        string body = "{'world': [], 'coords': [[0.0, 0.0], [0.899454, 0.899454], [-0.899454, 0.899454], [-0.899454, -0.899454], [0.899454, -0.899454]]}";
        StartCoroutine(SendRequest("http://127.0.0.1:5555/get_image", body));
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator SendRequest(string uri, string body)
    {
        Dictionary<string, List<List<float>>> mydict = new Dictionary<string, List<List<float>>>();
        
        List<List<float>> world = new List<List<float>>();

        List<List<float>> coords = new List<List<float>>();
        coords.Add(new List<float>() {0.0f, 0.0f});
        coords.Add(new List<float>() {0.899454f, 0.899454f});
        coords.Add(new List<float>() {-0.899454f, 0.899454f});
        coords.Add(new List<float>() {-0.899454f, -0.899454f});
        coords.Add(new List<float>() {0.899454f, -0.899454f});

        mydict.Add("world", world);
        mydict.Add("coords", coords);

        WWWForm form = new WWWForm();
        form.AddField("data", JsonConvert.SerializeObject(mydict));

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            // Set downloadHandler to handle textures (for single textures)
            // webRequest.downloadHandler = new DownloadHandlerTexture();
            
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            Dictionary<string, string> myDict2 = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text).ToObject<Dictionary<string, string>>();

            string result = myDict2["result"];

            string[] images = result.Split(' ');

            Texture2D myTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false); // the arguments after 2,2 are likely unnecessary
            myTexture.LoadImage(System.Convert.FromBase64String(images[3]));
            myTexture.Apply();

            
            // Single texture returned from flask (PIL image converted to bytes)
            // Texture2D myTexture = ((DownloadHandlerTexture) webRequest.downloadHandler).texture;
            
            GameObject go = sc.createSprite(myTexture);

            // Blank textures
            // sc.createSprite(Texture2D.whiteTexture);
            // sc.createSprite(new Texture2D(2, 2));
            
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
