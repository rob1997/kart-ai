using Core;
using TMPro;
using UnityEngine;

public class ScoreBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
 
    [Space]
    
    [SerializeField] private Driver driver;

    private int _score;
    
    private void Update()
    {
        if (_score != driver.Score)
        {
            scoreText.text = $"{driver.Score}";
        }
    }
}
