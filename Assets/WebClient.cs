using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class WebClient : MonoBehaviour
{
    // private string SERVER_URL = "https://inbound-bee-381420.ue.r.appspot.com/get_image";
    private string SERVER_URL = "http://127.0.0.1:5555/get_image";

    SpriteCreater sc;

    void Start()
    {
        sc = GameObject.Find("SpriteCreater").GetComponent<SpriteCreater>();
    }

    public IEnumerator SendRequest(Dictionary<string, List<List<float>>> data, List<Tile> megatile)
    {
        string uri = SERVER_URL;

        using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
        {   
            // Request and wait for the desired page.
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonConvert.SerializeObject(data));
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            Dictionary<string, string> response = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text).ToObject<Dictionary<string, string>>();
            // Debug.Log(response["images"]);
            // Debug.Log(response["vectors"]);

            // response["images"] format: "im1_bytes im2_bytes im3_bytes"
            string[] images = response["images"].Split(' ');

            List<List<float>> latent_vectors = string_to_vectors(response["vectors"]);

            Debug.Assert(images.Length == latent_vectors.Count, "SendRequest: Number of latent vectors should match number of new images");

            for (int i = 0; i < images.Length; i++) {
                Texture2D myTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false); // the arguments after 2,2 are likely unnecessary
                myTexture.LoadImage(System.Convert.FromBase64String(images[i]));
                myTexture.Apply();
                GameObject go = sc.createSprite(myTexture);
                GameObject prev = megatile[i].image;
                Destroy(prev);
                megatile[i].image = go;
                megatile[i].generated = true;
                megatile[i].latent_vector = latent_vectors[i];
            }
            
            
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

    // Convert string of latent vectors to list of List<float>
    List<List<float>> string_to_vectors(string s)
    {
        // s example: "[[1.6, 2, 3, 4], [5, 6], [7, 8]]"
        s = s.Substring(1, s.Length - 2); // "[1.6, 2, 3, 4], [5, 6], [7, 8]"
        s = s.Replace("],", "").Replace("]", ""); // "[1.6, 2, 3, 4 [5, 6 [7, 8"
    
        string[] s_split = s.Split('[');
        
        List<List<float>> latent_vectors = new List<List<float>>();
        for (int i = 1; i < s_split.Length; i++) { // First substring is empty
            string sub = s_split[i];
            latent_vectors.Add(sub.Split(',').Select(float.Parse).ToList());
        }
        
        return latent_vectors;
    }
}
