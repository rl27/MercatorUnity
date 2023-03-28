using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// https://www.youtube.com/watch?v=zc8ac_qUXQY
public class StartMenu : MonoBehaviour
{
    private static int DEFAULT_N = 4;
    private static int DEFAULT_K = 5;
    private int n = DEFAULT_N;
    private int k = DEFAULT_K;

    private static float DEFAULT_SIGMA = 0.02f;
    private static float DEFAULT_LENGTHSCALE = 2.0f;
    private float sigma = DEFAULT_SIGMA;
    private float lengthscale = DEFAULT_LENGTHSCALE;

    private string sentence = "";

    void Awake()
    {
        // Allow the game scene to get n and k before deactivating menu scene
        DontDestroyOnLoad(this);
    }

    public void PlayGame()
    {
        if (validNK() && validSL()) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            // GameObject.Find("StartMenu").SetActive(false);
        }
    }

    public void inputN(string input)
    {
        if (input == "")
            n = DEFAULT_N;
        else
            int.TryParse(input, out n);
    }

    public void inputK(string input)
    {
        if (input == "")
            k = DEFAULT_K;
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

    public void inputSigma(string input)
    {
        if (input == "")
            sigma = DEFAULT_SIGMA;
        else
            float.TryParse(input, out sigma);
    }

    public void inputLengthscale(string input)
    {
        if (input == "")
            lengthscale = DEFAULT_LENGTHSCALE;
        else
            float.TryParse(input, out lengthscale);
    }

    private bool validSL()
    {
        if (sigma <= 0 || lengthscale <= 0)
            return false;
        return true;
    }

    public List<float> getSL()
    {
        return new List<float>() {sigma, lengthscale};
    }

    public void inputSentence(string input)
    {
        sentence = input;
    }

    public string getSentence()
    {
        return sentence;
    }
}
