using System;
using Core.FSM;
using Game.States;
using UnityEngine;

namespace Game.GameManager
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private readonly StateMachine _stateMachine = new();

        public IState CurrentState => _stateMachine.CurrentState;
        

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void Init()
        {
            ChangeState(new MenuState(this));
        }
        
        
        public void ChangeState(IState newState)
        {
            _stateMachine.ChangeState(newState);
        }
    }
}