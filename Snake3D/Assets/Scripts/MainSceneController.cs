using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneController : MonoBehaviour
{

    [SerializeField] private Text highScoreText;

    // Start is called before the first frame update
    void Start()
    {
        highScoreText.text ="High Score : " + PlayerPrefs.GetInt("HighScore",0).ToString();
    }

    // Update is called once per frame
    public void PlayButtonClicked()
    {
        SceneManager.LoadScene("GameScene");
    }
}
