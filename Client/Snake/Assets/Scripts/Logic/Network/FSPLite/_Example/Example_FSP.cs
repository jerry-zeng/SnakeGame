using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Network.FSP.Client;
using Framework.Network.FSP.Server;
using GameProtocol;

namespace Framework.Network.FSP.Example
{
    public class Example_FSP : MonoBehaviour
    {
        FSPParam m_fspParam;
        FSPClient_Imp m_client1;
        FSPClient_Imp m_client2;


        void Start()
        {
            Debuger.EnableLog = true;
            Debuger.EnableSave = false;

            m_client1 = new FSPClient_Imp(1);
            m_client2 = new FSPClient_Imp(2);

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlayModeChanged;
            #endif
        }

        #if UNITY_EDITOR
        private void OnEditorPlayModeChanged()
        {
            if (UnityEngine.Application.isPlaying == false)
            {
                UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
                
                if (m_client1 != null)
                {
                    m_client1.Stop();
                }
                if (m_client2 != null)
                {
                    m_client2.Stop();
                }
            }
        }
        #endif

        void OnGUI()
        {
            // main menu
            if (!FSPServer.Instance.IsRunning)
            {
                if (GUILayout.Button("StartServer"))
                {
                    FSPServer.Instance.Start(0);
                    FSPServer.Instance.SetFrameInterval(66, 2);
                    FSPServer.Instance.SetServerTimeout(0);

                    FSPServer.Instance.StartGame();
                    m_client1.sid = m_client1.playerId;
                    m_client2.sid = m_client2.playerId;
                    FSPServer.Instance.Game.AddPlayer(m_client1.playerId, m_client1.sid);
                    FSPServer.Instance.Game.AddPlayer(m_client2.playerId, m_client2.sid);

                    m_fspParam = FSPServer.Instance.GetParam();
                }
            }
            else
            {
                if (GUILayout.Button("CloseServer"))
                {
                    FSPServer.Instance.Close();
                }
            }
            
            if (!FSPServer.Instance.IsRunning)
            {
                return;
            }


            GUILayout.Label("Server IP  : " + m_fspParam.host);
            GUILayout.Label("Server Port: " + m_fspParam.port);

            if ( FSPServer.Instance.Game != null )
                GUILayout.Label("GameState: " + FSPServer.Instance.Game.State.ToString());
            
            var game = FSPServer.Instance.Game;
            if (game != null)
            {
                GUILayout.Label("Frame: " + game.CurrentFrameId);
            }

            GUILayout.Space(10);

            // client gui
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    m_client1.OnGUI();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    m_client2.OnGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        void FixedUpdate()
        {
            m_client1.FixedUpdate();
            m_client2.FixedUpdate();
        }
    }

    public class FSPClient_Imp : IGameStateListener
    {
        public string LOG_TAG = "FSPGameClientImpl";

        uint m_playerId = 0;
        public uint playerId {get {return m_playerId;}}
        uint m_sid = 0;
        public uint sid {get {return m_sid;} set { m_sid = value;}}

        FSPManager m_mgrFSP;
        string m_lastVKey = "";
        int delayToStop = -1;


        public FSPClient_Imp(uint playerId)
        {
            m_playerId = playerId;
            LOG_TAG = LOG_TAG + "[" + m_playerId + "]";
        }

        public bool Start(FSPParam param)
        {
            m_mgrFSP = new FSPManager();
            m_mgrFSP.Start(param, m_playerId);

            m_mgrFSP.SetFrameListener(OnEnterFrame);
            m_mgrFSP.SetGameStateListener(this);

            return true;
        }

        public void Stop()
        {
            this.Log("Stop()");

            if (m_mgrFSP != null)
            {
                m_mgrFSP.Stop();
                m_mgrFSP = null;
            }
            m_lastVKey = "";

            delayToStop = -1;
        }


        public void OnGUI()
        {
            if (m_mgrFSP != null)
            {
                GUILayout.Label("Client[" + m_playerId + "]: " + m_mgrFSP.GameState);
                GUILayout.Label("Port: " + m_mgrFSP.Port);
            }
            else
            {
                GUILayout.Label("Client[" + m_playerId + "]");
            }
            GUILayout.Label("VKey：" + m_lastVKey);

            // 没开的话只能先开
            if (m_mgrFSP == null)
            {
                if (GUILayout.Button("Start"))
                {
                    // 这些变量不能共用
                    FSPParam param = FSPServer.Instance.GetParam();
                    param.sid = sid;
                    param.authId = (int)playerId;

                    Start(param);
                }
                return;
            }

            if (GUILayout.Button("SendGameBegin"))
            {
                m_mgrFSP.SendGameBegin();
            }

            if (GUILayout.Button("SendRoundBegin"))
            {
                m_mgrFSP.SendRoundBegin();
            }

            if (GUILayout.Button("SendControlStart"))
            {
                m_mgrFSP.SendControlStart();
            }

            if (GUILayout.Button("SendPing"))
            {
                m_mgrFSP.SendFSP(FSPVKeyBase.PING, UnityEngine.Random.Range(1, 1000));
            }

            if (GUILayout.Button("SendRoundEnd"))
            {
                m_mgrFSP.SendRoundEnd();
            }

            if (GUILayout.Button("SendGameEnd"))
            {
                m_mgrFSP.SendGameEnd();
            }

			if (GUILayout.Button("SendPlayerExit"))
            {
                m_mgrFSP.SendPlayerExit();
            }
        }

        public void FixedUpdate()
        {
            if (m_mgrFSP != null)
            {
                m_mgrFSP.EnterFrame();
            }

            if (delayToStop > 0)
            {
                delayToStop--;

                if (delayToStop == 0)
                    Stop();
            }
        }

        void OnEnterFrame(int frameId, FSPFrame frame)
        {
            if (frame != null && frame.vkeys != null)
            {
                for (int i = 0; i < frame.vkeys.Count; i++)
                {
                    FSPVKey cmd = frame.vkeys[i];
                    m_lastVKey = cmd.ToString();
                    Debuger.Log(LOG_TAG, "OnEnterFrame() frameId:{0}, cmd:{1}", frameId, cmd.ToString());
                }
            }
        }
    

        #region 接口
        public void OnGameBegin(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnGameBegin: {0}", cmd.args[0]);
        }
        public void OnRoundBegin(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnRoundBegin: {0}", cmd.args[0]);
        }
        public void OnControlStart(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnControlStart: {0}", cmd.args[0]);
        }
        public void OnRoundEnd(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnRoundEnd: {0}", cmd.args[0]);
        }
        public void OnGameEnd(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnGameEnd: {0}", (GameProtocol.FSPGameEndReason)cmd.args[0]);

            delayToStop = 60;  //2s
        }

        public void OnPlayerExit(FSPVKey cmd)
        {
            Debuger.Log(LOG_TAG, "OnGameExit: {0}", cmd.args[0]);

            if (cmd.playerId == m_playerId)
                delayToStop = 1;
        }
        #endregion
    }
}

