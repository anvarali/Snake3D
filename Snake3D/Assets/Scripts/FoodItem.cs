using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{

    Food _food;
    Color foodColor;
    [SerializeField]Renderer foodRenderer;

    Action<Food,GameObject> _onEatFood;

    private void Start()
    {
        SnakeController.FindFood += OnFindingFood;
    }

    public void Init(Food food,Action<Food,GameObject> onEatFood)
    {
        _onEatFood = onEatFood;
        ColorUtility.TryParseHtmlString(food.color, out foodColor);
        _food = food;
        foodRenderer.material.SetColor("_Color", foodColor);
    }

    private void OnDestroy()
    {
        SnakeController.FindFood -= OnFindingFood;
    }

    void OnFindingFood(Vector3 snakeHeadPos)
    {
        if (snakeHeadPos.x == transform.position.x && snakeHeadPos.z == transform.position.z)
        {
            _onEatFood.Invoke(_food,gameObject);
        }
    }

}
