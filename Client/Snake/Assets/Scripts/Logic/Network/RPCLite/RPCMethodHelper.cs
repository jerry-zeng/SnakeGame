////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 作者：slicol
*/

using System.Net;

namespace Framework.Network.RPC
{
    public delegate void RPCMethod(IPEndPoint target);
    public delegate void RPCMethod<T0>(IPEndPoint target, T0 arg0);
    public delegate void RPCMethod<T0, T1>(IPEndPoint target, T0 arg0, T1 arg1);
    public delegate void RPCMethod<T0, T1, T2>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2);
    public delegate void RPCMethod<T0, T1, T2, T3>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate void RPCMethod<T0, T1, T2, T3, T4>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(IPEndPoint target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    public abstract class RPCMethodHelperBase
    {
        public abstract void Invoke(IPEndPoint target, object[] args);
    }

    public class RPCMethodHelper : RPCMethodHelperBase
    {
        public RPCMethod method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method(target);
        }
    }

    public class RPCMethodHelper<T0> : RPCMethodHelperBase
    {
        public RPCMethod<T0> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method(target, (T0)args[0]);
        }
    }

    public class RPCMethodHelper<T0, T1> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1]);
            
        }
    }

    public class RPCMethodHelper<T0, T1, T2> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4, T5> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4, T5, T6> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7]);
        }
    }
    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8]);
        }
    }

    public class RPCMethodHelper<T0, T1, T2,T3,T4,T5,T6,T7,T8,T9> : RPCMethodHelperBase
    {
        public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8,T9> method;

        public override void Invoke(IPEndPoint target, object[] args)
        {
            method.Invoke(target, (T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9]);
        }
    }
}
