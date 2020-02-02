using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Grounded2.MiniGame.Snake
{
    public class SnakeButtonController : MonoBehaviour
    {
        [SerializeField] private AllButtonTypes buttonType;
        [SerializeField] private int buttonId;

        SnakeController m_snakeController;

        //private GlowObjectCmd m_glowObjectCmd;
        private bool canTakeInput = false;

        [SerializeField] float moveOffSet = 0.01f;
        float targetPos;
        float iniTialPos;
        //GameData m_data;
        private void Start()
        {
            iniTialPos = transform.localPosition.y;
            targetPos = iniTialPos - moveOffSet;
            m_snakeController = FindObjectOfType<SnakeController>();
            
        }
        


        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                m_snakeController.OnButtonClicked(AllButtonTypes.turnLeft_button);
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_snakeController.OnButtonClicked(AllButtonTypes.turnRight_button);
            }
        }


        
        
    }
}
