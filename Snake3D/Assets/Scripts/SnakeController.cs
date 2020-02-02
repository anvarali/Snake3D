using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Random = UnityEngine.Random;


public enum AllButtonTypes
{
    turnLeft_button,
    turnRight_button

}
[Serializable]
class AdvancedSettings
{
    public int length = 5;
    public float size = 10;
    public float nextMoveTime = 0.3f;
    public Vector3 initialTailPos;
    public Vector2 maxX_MaxY;
    public Vector2 minX_MinY;
}

enum MovingDirection
{
    right,
    left,
    up,
    down
}
public class SnakeController : MonoBehaviour
{
    [SerializeField] private GameObject snakeHeadPrefab;
    [SerializeField] private GameObject snakeBodyPrefab;
    [SerializeField] private GameObject snakeTailPrefab;
    [SerializeField] private GameObject appleObj;
    [SerializeField] AdvancedSettings advancedSettings;

    private MovingDirection Direction { get { return m_direction; } set { ChangeDirection(value); } }
    private MovingDirection m_direction;
    private List<BodyPartMovemet> allBodyPartMovemetScripts;
    private List<GameObject> allBodyParts;
    private BodyPartMovemet m_headMovementScript;
    private GameObject m_snakeHeadPart;
    private GameObject m_snakeTailPart;
    private Vector3 m_headPos;
    private Vector3 m_headOldRot;
    private float lastUpdateTime = 0;
    private float lastDirectionUpdateTime = 0;
    private int m_totalColums = 0;
    private int m_totalRows = 0;
    private bool gameOver = true;
    internal bool isObjectiveCompleted = false;

    [SerializeField] private bool restartGame = false;

    [SerializeField] private FoodDetails availableFoods;
    [SerializeField] private Text scoreText, streakText,finalScoreText;
    [SerializeField] private GameObject resultObject;

    public static Action<Vector3> FindFood;

    public int score, scoreStreak=1;

    Food lastEatenFood;

    //GameData m_data;
    // Start is called before the first frame update
    void Start()
    {
        CalculateRowsAndcolums();
        FetchFoodDetails();
        StartGame();
    }


    private void OnDestroy()
    {

    }


    internal void StartGame()
    {
        gameOver = false;
        InIt();
    }

    private void FetchFoodDetails()
    {
        var rawData = Resources.Load<TextAsset>("foodlist");
        availableFoods = JsonUtility.FromJson<FoodDetails>(rawData.ToString());
    }

    /// <summary>
    /// Initializes the game
    /// </summary>
    public void InIt()
    {
        
        allBodyPartMovemetScripts = new List<BodyPartMovemet>();
        allBodyParts = new List<GameObject>();
        //Stores Last update time as current time
        lastUpdateTime = Time.time;
        lastDirectionUpdateTime = Time.time;
        //Calculating initial pos
        Vector3 initialTailPos = advancedSettings.initialTailPos;
        initialTailPos.x += (advancedSettings.size * (advancedSettings.length - 1));

        //Create head part first
        m_snakeHeadPart = Instantiate(snakeHeadPrefab, transform);
        m_snakeHeadPart.transform.localPosition = initialTailPos;
        m_headMovementScript = m_snakeHeadPart.GetComponent<BodyPartMovemet>();
        allBodyPartMovemetScripts.Add(m_headMovementScript);
        allBodyParts.Add(m_snakeHeadPart);
        m_headPos = m_snakeHeadPart.transform.localPosition;

        initialTailPos.x = initialTailPos.x - advancedSettings.size;
        //Create Snake Body part
        for (int i = 0; i < advancedSettings.length - 2; i++)
        {
            GameObject snakeBodyPart = Instantiate(snakeBodyPrefab, transform);
            snakeBodyPart.transform.localPosition = initialTailPos;
            initialTailPos.x = initialTailPos.x - advancedSettings.size;
            allBodyPartMovemetScripts.Add(snakeBodyPart.GetComponent<BodyPartMovemet>());
            allBodyParts.Add(snakeBodyPart);
        }

        //Create snake tail part
        m_snakeTailPart = Instantiate(snakeTailPrefab, transform);
        m_snakeTailPart.transform.localPosition = initialTailPos;
        initialTailPos.x = initialTailPos.x - advancedSettings.size;
        allBodyPartMovemetScripts.Add(m_snakeTailPart.GetComponent<BodyPartMovemet>());
        allBodyParts.Add(m_snakeTailPart);

        AssignScriptReferenceToEachPart();
        InvokeRepeating("ShowNewApple", 0, 3);
        Direction = MovingDirection.right;
        gameOver = false;
    }
    /// <summary>
    /// Connecting Each snake part each other
    /// </summary>
    private void AssignScriptReferenceToEachPart()
    {
        int toalParts = allBodyPartMovemetScripts.Count;
       
        for (int i = 0; i < toalParts - 1; i++)
        {
            allBodyPartMovemetScripts[i].previousPartMovemt = allBodyPartMovemetScripts[i + 1];
        }
    }

    /// <summary>
    /// Calculates total rows and columns depend on snake size
    /// </summary>
    private void CalculateRowsAndcolums()
    {
        //Inclusive of min and max X
        m_totalColums = (int)((advancedSettings.maxX_MaxY.x - advancedSettings.minX_MinY.x) / advancedSettings.size);

        //Inclusive of min and max Y
        m_totalRows = (int)((advancedSettings.maxX_MaxY.y - advancedSettings.minX_MinY.y) / advancedSettings.size);
        Debug.Log(m_totalColums + "   " + m_totalRows);
    }
    /// <summary>
    /// Restarts the game
    /// </summary>
    public void OnRestartButtonPressed()
    {
        DeleteAllUnwantedObjects();
        InIt();
    }
    /// <summary>
    /// Destroy all previous objects and create new
    /// </summary>
    public void DeleteAllUnwantedObjects()
    {
        if (allBodyParts != null)
        {
            if (allBodyParts.Count != 0)
            {
                foreach (var bodyPart in allBodyParts)
                {
                    Destroy(bodyPart);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        //For debug purpose
        if (restartGame)
        {
            restartGame = false;
            OnRestartButtonPressed();
        }
        
        //Compare last pos update time and move snake
        if (Time.time > (lastUpdateTime + advancedSettings.nextMoveTime))
        {
            Vector3 newPos = GetNewPos();
            if(!gameOver)
            {
            m_headPos = newPos;
            if (CheckCollision())
            {
                    OnCollision(); 
                return;
            }
            m_headMovementScript.Move(m_headPos, transform.localRotation.eulerAngles.z);
            lastUpdateTime = Time.time;
            }
        }

    }
    /// <summary>
    /// This will take input from button controller
    /// </summary>
    /// <param name="buttonType"></param>
    internal void OnButtonClicked(AllButtonTypes buttonType)
    {
        if (gameOver)
        {
            OnRestartButtonPressed();
            return;
        }

        if (Time.time <= (lastDirectionUpdateTime + advancedSettings.nextMoveTime))
        {
            return;
        }

        //Handle inputs
        if (buttonType == AllButtonTypes.turnLeft_button)
        {
            switch (Direction)
            {
                case MovingDirection.right: Direction = MovingDirection.up; break;
                case MovingDirection.left: Direction = MovingDirection.down; break;
                case MovingDirection.up: Direction = MovingDirection.left; break;
                case MovingDirection.down: Direction = MovingDirection.right; break;
            }
        }
        else if (buttonType == AllButtonTypes.turnRight_button)
        {
            switch (Direction)
            {
                case MovingDirection.right: Direction = MovingDirection.down; break;
                case MovingDirection.left: Direction = MovingDirection.up; break;
                case MovingDirection.up: Direction = MovingDirection.right; break;
                case MovingDirection.down: Direction = MovingDirection.left; break;
            }
        }
    }
    

    void OnCollision()
    {
        CancelInvoke("ShowNewApple");
        gameOver = true;
        finalScoreText.text = "Final score : " + score;
        var highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
        resultObject.SetActive(true);
    }


    /// <summary>
    /// Return new Position for the head
    /// </summary>
    /// <returns></returns>
    private Vector3 GetNewPos()
    {
        Vector3 newPos = m_headPos;
        Vector3 oldPos = m_headPos;
        //ChangeHeadRotation();
        switch (Direction)
        {
            case MovingDirection.right:
                newPos.x += advancedSettings.size;
                if (newPos.x > advancedSettings.maxX_MaxY.x)
                {
                    OnCollision();
                    newPos.x = advancedSettings.minX_MinY.x;
                }
                break;
            case MovingDirection.left:
                newPos.x -= advancedSettings.size;
                if (newPos.x < advancedSettings.minX_MinY.x)
                    {
                    OnCollision();
                    newPos.x = advancedSettings.maxX_MaxY.x;
                    }
                break;
            case MovingDirection.up:
                newPos.z += advancedSettings.size;
                if (newPos.z > advancedSettings.maxX_MaxY.y)
                    {
                    OnCollision();
                    newPos.z = advancedSettings.minX_MinY.y;
                    }
                break;
            case MovingDirection.down:
                newPos.z -= advancedSettings.size;
                if (newPos.z < advancedSettings.minX_MinY.y)
                    {
                    OnCollision();
                    newPos.z = advancedSettings.maxX_MaxY.y;
                    }
                break;
        }

        FindFood?.Invoke(newPos);

        newPos.y = 0.25f;
        return newPos;
    }
    /// <summary>
    /// Changing the direction
    /// </summary>
    /// <param name="newDirection"></param>
    private void ChangeDirection(MovingDirection newDirection)
    {
        if (newDirection == m_direction)
            return;
        m_direction = newDirection;
    }


    int availableFoodCount = 0;
    /// <summary>
    /// Create new apple in the game
    /// </summary>
    private void ShowNewApple()
    {
        if (availableFoodCount > 4) return;
        var food = GetFoodToSpawn();
        var foodObj = Instantiate(appleObj);
        foodObj.transform.position = GetNewFoodPos();
        foodObj.GetComponent<FoodItem>().Init(food, OnEatFood);
        availableFoodCount++;
       
    }

    Food GetFoodToSpawn()
    {
        return availableFoods.FoodList[Random.Range(0, availableFoods.FoodList.Count)];
    }



    void OnEatFood(Food food,GameObject foodObj)
    {
        Debug.Log("Inside OnEatFood");
        AddNewTailPartToSnakeBody();
        if (lastEatenFood != null && lastEatenFood == food)
        {
            scoreStreak++;
            score = score + (food.points * scoreStreak);
        }
        else
        {
            scoreStreak = 1;
            score += food.points;
        }

        scoreText.text = "Score : " + score;
        streakText.text = "x " + scoreStreak;
        availableFoodCount--;
        lastEatenFood = food;
        Destroy(foodObj);
    }

    public void RestartGame()
    {
        score = 0;
        availableFoodCount = 0;
        scoreStreak = 1;
        scoreText.text = "Score : " + score;
        streakText.text = "x " + scoreStreak;
        SceneManager.LoadScene("Main");
    }

    /// <summary>
    /// Returs apples new pos
    /// </summary>
    /// <returns></returns>
    private Vector3 GetNewFoodPos()
    {
        bool newPosGenerated = false;
        int randomColumn = 0;
        int randomRow = 0;
        Vector3 newApplePos = new Vector3(advancedSettings.minX_MinY.x, 0.25f, advancedSettings.minX_MinY.y);
        while (!newPosGenerated)
        {
            randomColumn = UnityEngine.Random.Range(0, m_totalColums);
            randomRow = UnityEngine.Random.Range(0, m_totalRows);

            newApplePos.x = advancedSettings.minX_MinY.x;
            newApplePos.z = advancedSettings.minX_MinY.y;

            newApplePos.x += (randomColumn * advancedSettings.size);
            newApplePos.z += (randomRow * advancedSettings.size);
            if (CheckSnakeOnTheNewPos(newApplePos))
                newPosGenerated = true;
        }
        return newApplePos;
    }
    /// <summary>
    /// Checks whether snake part is there in new apples position, if true create new pos
    /// </summary>
    /// <param name="newApplePos"></param>
    /// <returns></returns>
    private bool CheckSnakeOnTheNewPos(Vector3 newApplePos)
    {
        bool noPartFound = true;
        foreach (var bodyPart in allBodyParts)
        {
            if (bodyPart.transform.localPosition == newApplePos)
            {
                noPartFound = false;
                break;
            }
        }
        return noPartFound;
    }
    /// <summary>
    /// Adding new body part to snake
    /// </summary>
    private void AddNewTailPartToSnakeBody()
    {
        Debug.Log("Inside AddNewTailPartToSnakeBody");
        //Create new body part
        GameObject snakeBodyPart = Instantiate(snakeBodyPrefab, transform);
        //Set its postion as same as the last snake body part pos
        //In the list last item will be snake tail, so we need to fetch second last item from the list, that would be body part
        snakeBodyPart.transform.localPosition = allBodyParts[allBodyParts.Count - 2].transform.localPosition;
        allBodyPartMovemetScripts[allBodyParts.Count - 2].previousPartMovemt = snakeBodyPart.GetComponent<BodyPartMovemet>();
        //Replace new body part in tails poistion in the list and for script  reference also
        allBodyPartMovemetScripts[allBodyParts.Count - 1] = snakeBodyPart.GetComponent<BodyPartMovemet>();
        allBodyPartMovemetScripts[allBodyParts.Count - 1].previousPartMovemt = m_snakeTailPart.GetComponent<BodyPartMovemet>();
        //Add snakes tails in the list as new
        allBodyPartMovemetScripts.Add(m_snakeTailPart.GetComponent<BodyPartMovemet>());
        allBodyParts[allBodyParts.Count - 1] = snakeBodyPart;
        allBodyParts.Add(m_snakeTailPart);

    }

    private bool CheckCollision()
    {
        for (int i = 1; i < allBodyParts.Count; i++)
        {
            if (allBodyParts[i].transform.localPosition == m_headPos)
            {
                return true;
            }
        }
        return false;
    }
}
[Serializable]
public class FoodDetails
{
    public List<Food> FoodList;
}

[Serializable]
public class Food
{
    public string color;
    public int points;
}