using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using GameProtocol;

namespace GamePlay
{
    public class GameInput : MonoBehaviour 
    {
        public static UnityAction<int,float> onVKey;

        private float DefaultSpeedValue = 1;

        [SerializeField, Range(2, 5)]
        private int SpeedUpValue = 2;


        private Dictionary<KeyCode, bool> _keyState = new Dictionary<KeyCode, bool>();

        private static EasyTouch _easyTouch;
        private static EasyJoystick _joystick;
        private static EasyButton _easyButton;

        private static bool _Enable = false;


        public static void EnableInput()
        {
            _Enable = true;

            if( _joystick != null ){
                _joystick.enable = true;
                _joystick.isActivated = true;
            }
            if( _easyButton != null ){
                _easyButton.enable = true;
                _easyButton.isActivated = true;
            }
            if( _easyTouch != null )
                _easyTouch.enable = true;
        }
        public static void DisableInput()
        {
            _Enable = false;

            if( _joystick != null ){
                _joystick.enable = false;
                _joystick.isActivated = false;
            }
            if( _easyButton != null ){
                _easyButton.enable = false;
                _easyButton.isActivated = false;
            }
            if( _easyTouch != null )
                _easyTouch.enable = false;
        }

        void Start()
        {
            _keyState.Clear();

            _easyTouch = GetComponentInChildren<EasyTouch>();
            _joystick = GetComponentInChildren<EasyJoystick>();
            _easyButton = GetComponentInChildren<EasyButton>();
        }

        void OnEnable()
        {
            RegisterInputKeys();
        }
        void OnDisable()
        {
            UnregisterInputKeys();
        }

        void OnDestroy()
        {
            _keyState.Clear();
            onVKey = null;

            UnregisterInputKeys();
        }


        void RegisterInputKeys()
        {
            EasyJoystick.On_JoystickMove += EasyJoystick_On_JoystickMove;
            EasyJoystick.On_JoystickMoveEnd += EasyJoystick_On_JoystickMoveEnd;
            EasyButton.On_ButtonDown += EasyButton_On_ButtonDown;
            EasyButton.On_ButtonUp += EasyButton_On_ButtonUp;
        }

        void UnregisterInputKeys()
        {
            EasyJoystick.On_JoystickMove -= EasyJoystick_On_JoystickMove;
            EasyJoystick.On_JoystickMoveEnd -= EasyJoystick_On_JoystickMoveEnd;
            EasyButton.On_ButtonDown -= EasyButton_On_ButtonDown;
            EasyButton.On_ButtonUp -= EasyButton_On_ButtonUp;
        }


        void EasyJoystick_On_JoystickMove (MovingJoystick move)
        {
            HandleVKey(GameVKey.MoveX, move.joystickValue.x);
            HandleVKey(GameVKey.MoveY, move.joystickValue.y);
        }
        void EasyJoystick_On_JoystickMoveEnd (MovingJoystick move)
        {
            HandleVKey(GameVKey.MoveX, 0f);
            HandleVKey(GameVKey.MoveY, 0f);
        }

        void EasyButton_On_ButtonDown (string buttonName)
        {
            HandleVKey(GameVKey.SpeedUp, SpeedUpValue);
        }
        void EasyButton_On_ButtonUp (string buttonName)
        {
            HandleVKey(GameVKey.SpeedUp, DefaultSpeedValue);
        }


        void Update()
        {
            if( _Enable == false )
                return;

            HandleKey(KeyCode.W, GameVKey.MoveY, 1f, 0f);
            HandleKey(KeyCode.A, GameVKey.MoveX, -1f, 0f);
            HandleKey(KeyCode.S, GameVKey.MoveY, -1f, 0f);
            HandleKey(KeyCode.D, GameVKey.MoveX, 1f, 0f);

            HandleKey(KeyCode.UpArrow, GameVKey.MoveY, 1f, 0f);
            HandleKey(KeyCode.LeftArrow, GameVKey.MoveX, -1f, 0f);
            HandleKey(KeyCode.DownArrow, GameVKey.MoveY, -1f, 0f);
            HandleKey(KeyCode.RightArrow, GameVKey.MoveX, 1f, 0f);

            HandleKey(KeyCode.Space, GameVKey.SpeedUp, SpeedUpValue, DefaultSpeedValue);
        }

        void HandleKey(KeyCode key, GameVKey vkey, float pressArg, float releaseArg)
        {
            if( Input.GetKey(key) ){
                if( !_keyState.ContainsKey(key) || !_keyState[key] )
                {
                    _keyState[key] = true;
                    HandleVKey(vkey, pressArg);
                }
            }
            else{
                if( _keyState.ContainsKey(key) && _keyState[key] )
                {
                    _keyState[key] = false;
                    HandleVKey(vkey, releaseArg);
                }
            }
        }

        void HandleVKey(GameVKey vkey, float arg)
        {
            if( onVKey != null )
                onVKey.Invoke((int)vkey, arg);
        }
    }
}
