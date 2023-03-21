using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCreater : MonoBehaviour
{

    void Start()
    {
    }

    public GameObject createSprite(Texture2D tex)
    {
        GameObject go = new GameObject();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);

        go.transform.position = new Vector3(0.0f, 0.5f, 1.5f);

        sr.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        sr.sortingOrder = 1; // Default order is 0; want sprites to not render behind polygon meshes.

        return go;
    }

    /*
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Add sprite"))
        {
            sr.sprite = mySprite;
        }
    }
    */
}
