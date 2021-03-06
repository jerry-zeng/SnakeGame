﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using Exception = System.Exception;
using System.Reflection;
using Framework.Network.Kcp;

namespace Framework.Network.RPC
{
    public class RPCService
    {
        protected string LOG_TAG = "RPCService";

        // fields
        private KCPSocket m_Socket;
        private bool m_IsRunning = false;
        private Dictionary<string, RPCMethodHelperBase> m_RPCBindMap;

        // properties
        public bool IsRunning { get{ return m_IsRunning;} }

        public IPEndPoint SelfEndPoint { get { return m_Socket != null? m_Socket.SelfEndPoint:null; } }
        public int SelfPort { get { return m_Socket != null? m_Socket.SelfPort:0; } }
        public string SelfIP { get { return m_Socket != null? m_Socket.SelfIP:""; } }

        //=====================================================================
        #region 构造和析构
        public RPCService(int port = 0)
        {
            //创建Socket
            m_Socket = new KCPSocket(port, 1);
            m_Socket.AddReceiveListener(OnReceive);
            m_Socket.EnableBroadcast = true;

            m_IsRunning = true;

            m_RPCBindMap = new Dictionary<string, RPCMethodHelperBase>();

            // ??? 为什么还要转换一次
            port = m_Socket.SelfPort;

            LOG_TAG = LOG_TAG + "[" + port + "]";
            Debuger.Log(LOG_TAG, "RPCSocket() port:{0}", port);
        }

        public virtual void Dispose()
        {
            Debuger.Log(LOG_TAG, "Dispose()");

            m_IsRunning = false;

            if (m_Socket != null)
            {
                m_Socket.Dispose();
                m_Socket = null;
            }

            m_RPCBindMap.Clear();
        }
        #endregion

        //=====================================================================
        #region 主线程驱动
        public void RPCTick()
        {
            if (m_IsRunning && m_Socket != null)
            {
                m_Socket.Update();
            }
        }
        #endregion

        //=====================================================================
        #region 消息接收: ACK, SYN, Broadcast
        void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            try
            {
                var msg = PBSerializer.Deserialize<RPCMessage>(buffer);
                HandleRPCMessage(msg, remotePoint);
            }
            catch (Exception e)
            {
                Debuger.LogError(LOG_TAG, "OnReceive()->HandleMessage->Error:" + e.Message + "\n" + e.StackTrace);
            }
        }

        void HandleRPCMessage(RPCMessage msg, IPEndPoint remote)
        {
            MethodInfo mi = this.GetType().GetMethod(msg.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                Debuger.Log(LOG_TAG, "HandleRPCMessage() DefaultRPC<{0}> Remote:{1}", msg.name, remote);

                List<object> args = new List<object>();
                args.Add(remote);  //放第一个参数
                args.AddRange(msg.args);

                try
                {
                    // 调用自己的方法
                    mi.Invoke(this, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, args.ToArray(), null);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "HandleRPCMessage() DefaultRPC<{0}>出错: {1}\n{2}\n 参数: {3}", msg.name, e.Message, e.StackTrace, ListToString(args));
                }
            }
            else
            {
                OnBindingRPCInvoke(remote, msg);
            }
        }

        void OnBindingRPCInvoke(IPEndPoint remote, RPCMessage msg)
        {
            if (m_RPCBindMap.ContainsKey(msg.name))
            {
                Debuger.Log(LOG_TAG, "OnBindingRPCInvoke() RPC:{0}, Remote:{1}", msg.name, remote);
                try
                {
                    RPCMethodHelperBase rpc = m_RPCBindMap[msg.name];
                    rpc.Invoke(remote, msg.args);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "OnBindingRPCInvoke() RPC<" + msg.name + ">响应出错:" + e.Message + "\n" + e.StackTrace + "\n");
                }
            }
            else
            {
                Debuger.LogWarning(LOG_TAG, "OnBindingRPCInvoke() 收到未知的RPC: {0}", msg.name);
            }
        }
        
        static string ListToString(List<object> args)
        {
            if (args == null)
                return "null";
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var arg in args)
            {
                if (arg != null){
                    sb.Append(arg.ToString());
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
        #endregion
    
        //=====================================================================
        #region 消息发送
        void SendMessage(IPEndPoint remote, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.Serialize(msg);
            m_Socket.SendTo(buffer, buffer.Length, remote);
        }

        void SendMessage(List<IPEndPoint> remoteList, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.Serialize(msg);

            for (int i = 0; i < remoteList.Count; i++)
            {
                IPEndPoint remote = remoteList[i];
                if (remote != null)
                {
                    m_Socket.SendTo(buffer, buffer.Length, remote);
                }
            }
        }

        void SendBroadcast(int beginPort, int endPort, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.Serialize(msg);

            for (int i = beginPort; i < endPort; i++)
            {
                m_Socket.SendTo(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, i));
            }
        }
        #endregion
    
        //=====================================================================
        #region RPC接口
        public void RPC(IPEndPoint remote, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 1对1调用, name:{0}, remote:{1}", name, remote);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendMessage(remote, msg);
        }

        public void RPC(List<IPEndPoint> remoteList, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 1对多调用, Begin, msg:{0}", name);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendMessage(remoteList, msg);

            Debuger.Log(LOG_TAG, "RPC() 1对多调用, End!");
        }

        public void RPC(int beginPort, int endPort, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 广播调用, PortRange:{0}-{1}, Begin, msg:{2}",  beginPort, endPort, name);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendBroadcast(beginPort, endPort, msg);
        }

        // 一般是发送给本地
        public void RPC(RPCService remote, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 调用具体对象, RPCService:{0}, msg:{1}",  remote.SelfPort, name);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            remote.HandleRPCMessage(msg, null);
        }
        #endregion

        //=====================================================================
        #region Bind
        public void Bind(string name, RPCMethod rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper()
            {
                method = rpc
            };
        }

        public void Bind<T0>(string name, RPCMethod<T0> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1>(string name, RPCMethod<T0, T1> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2>(string name, RPCMethod<T0, T1, T2> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3>(string name, RPCMethod<T0, T1, T2, T3> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4>(string name, RPCMethod<T0, T1, T2, T3, T4> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4, T5>(string name, RPCMethod<T0, T1, T2, T3, T4, T5> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4, T5>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8>()
            {
                method = rpc
            };
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> rpc)
        {
            m_RPCBindMap[name] = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
            {
                method = rpc
            };
        }
        #endregion
    }
}