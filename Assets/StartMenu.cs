using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// https://www.youtube.com/watch?v=zc8ac_qUXQY
public class StartMenu : MonoBehaviour
{
    private int n = 4;
    private int k = 5;

    void Awake()
    {
        // Allow the game scene to get n and k before deactivating menu scene
        DontDestroyOnLoad(this);
    }

    public void PlayGame()
    {
        if (validNK()) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            // GameObject.Find("StartMenu").SetActive(false);
        }
    }

    public void inputN(string input)
    {
        if (input == "")
            n = 4;
        else
            int.TryParse(input, out n);
    }

    public void inputK(string input)
    {
        if (input == "")
            k = 5;
        else
            int.TryParse(input, out k);
    }

    private bool validNK()
    {
        if (n < 3 || k < 3)
            return false;
        if (2*n + 2*k >= n*k)
            return false;
        return true;
    }

    public List<int> getNK()
    {
        return new List<int>() {n, k};
    }
}
