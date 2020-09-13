using System;
using System.Collections.Generic;
using GameProtocol;

namespace Framework.Network.FSP.Client
{
    /// <summary>
    /// 帧控制器，决定逻辑帧是否需要加速还是缓冲
    /// </summary>
    public class FSPController
    {
        public string LOG_TAG = "FSPController";

        //缓冲控制
        private int m_LatestFrameId;  //收到的最新帧
        private int m_ClientFrameRateMultiple = 2;  // 客户端的帧序列其实是：0 1 0 1 0 1 0 1... 中间的0就是空白帧，该变量决定
        private bool m_IsInBuffing = false;
        private int m_BuffSize = 0;   //缓冲一些帧，因为可能网络会变差，一段时间内收不到新的帧，能正常消耗掉一些
        
        //加速控制
        private bool m_EnableSpeedUp = true;
        private int m_DefaultSpeed = 1;
        private bool m_IsInSpeedUp = false;
        
        //自动缓冲
        private bool m_EnableAutoBuff = true;
        private int m_AutoBuffInterval = 15;  //每一轮自动缓冲持续多久，像播放器一样，不要来一点数据就播放
        private int m_AutoBuffCount = 0;


        public int LatestFrameId { get { return m_LatestFrameId;} }
        public bool IsInBuffing { get { return m_IsInBuffing; } }
        public bool IsInSpeedUp { get { return m_IsInSpeedUp; } }
        public int FrameBufferSize { get { return m_BuffSize; } set { m_BuffSize = value; } }
        

        public bool Start(FSPParam param)
        {
            SetParam(param);
            return true;
        }

        public void Stop()
        {
            // do nothing
        }


        public void SetParam(FSPParam param)
        {
            m_ClientFrameRateMultiple = param.clientFrameRateMultiple;
            
            m_EnableSpeedUp = param.enableSpeedUp;
            m_DefaultSpeed = param.defaultSpeed;

            m_EnableAutoBuff = param.enableAutoBuffer;
            m_BuffSize = param.frameBufferSize;
        }

        public void AddFrameId(int frameId)
        {
            m_LatestFrameId = frameId;
        }

        public int GetFrameSpeed(int curFrameId)
        {
            int speed = 0;

            // 缓存的帧数
            int newFrameNum = m_LatestFrameId - curFrameId;

            //如果没在缓冲中
            if (!m_IsInBuffing)
            {
                // 那就开始缓冲
                if (newFrameNum == 0)
                {
                    //需要缓冲一下
                    m_IsInBuffing = true;
                    m_AutoBuffCount = m_AutoBuffInterval;
                }
                else
                {
                    //因为即将播去这么多帧
                    newFrameNum -= m_DefaultSpeed;

                    //剩下的可加速的帧数
                    int speedUpFrameNum = newFrameNum - m_BuffSize;

                    // 嗯，不是大于m_DefaultSpeed
                    if (speedUpFrameNum >= m_ClientFrameRateMultiple)
                    {
                        //可以加速
                        if (m_EnableSpeedUp)
                        {
                            if (speedUpFrameNum > 100)
                            {
                                speed = 8;
                            }
                            else if (speedUpFrameNum > 50)
                            {
                                speed = 4;
                            }
                            else
                            {
                                speed = 2;
                            }
                        }
                        else
                        {
                            speed = m_DefaultSpeed;
                        }
                    }
                    else
                    {
                        //还达不到可加速的帧数
                        speed = m_DefaultSpeed;

                        //主动缓冲，当帧数过少时，
                        //与其每一帧都卡，不如先卡久一点，然后就不卡了
                        if (m_EnableAutoBuff)
                        {
                            m_AutoBuffCount--;
                            if (m_AutoBuffCount <= 0)
                            {
                                m_AutoBuffCount = m_AutoBuffInterval;
                                if (speedUpFrameNum < m_ClientFrameRateMultiple - 1)
                                {
                                    speed = 0;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //正在缓冲中

                //当缓冲的数量足够时，结束缓冲
                int speedUpFrameNum = newFrameNum - m_BuffSize;
                if (speedUpFrameNum > 0)
                {
                    m_IsInBuffing = false;
                }
            }

            // 调试用
            m_IsInSpeedUp = speed > m_DefaultSpeed;

            return speed;
        }
    }
}