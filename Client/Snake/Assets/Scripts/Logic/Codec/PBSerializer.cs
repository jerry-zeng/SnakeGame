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

using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;


/// <summary>
/// 序列化帮助类.
/// </summary>
public class PBSerializer
{

    public static T Clone<T>(T data)
    {
        byte[] buffer = Serialize<T>(data);
        return Deserialize<T>(buffer);
    }


    /// <summary>
    /// 序列化pb数据.
    /// </summary>
    public static byte[] Serialize<T>(T t)
    {
        byte[] buffer = null;

        using (MemoryStream m = new MemoryStream())
        {
            Serializer.Serialize<T>(m, t);

            m.Position = 0;
            int length = (int)m.Length;
            buffer = new byte[length];
            m.Read(buffer, 0, length);
        }

        return buffer;
    }

    public static byte[] Serialize(object t)
    {
        byte[] buffer = null;

        using (MemoryStream m = new MemoryStream())
        {
            if (t != null)
            {
                RuntimeTypeModel.Default.Serialize(m, t);
            }

            m.Position = 0;
            int length = (int)m.Length;
            buffer = new byte[length];
            m.Read(buffer, 0, length);
        }

        return buffer;
    }

    public static int Serialize(object t, byte[] buffer)
    {
        using (MemoryStream m = new MemoryStream())
        {
            if (t != null)
            {
                RuntimeTypeModel.Default.Serialize(m, t);
            }

            m.Position = 0;
            int length = (int)m.Length;
            m.Read(buffer, 0, length);
            return length;
        }
    }


    /// <summary>
    /// 反序列化pb数据
    /// </summary>
    public static T Deserialize<T>(byte[] buffer)
    {
        T t = default(T);
        using (MemoryStream m = new MemoryStream(buffer))
        {
            t = Serializer.Deserialize<T>(m);
        }
        return t;
    }

    public static T Deserialize<T>(Stream stream)
    {
        T t = default(T);
        t = Serializer.Deserialize<T>(stream);
        return t;
    }

    public static object Deserialize(byte[] buffer, System.Type type)
    {
        object t = null;
        using (MemoryStream m = new MemoryStream(buffer))
        {
            t = RuntimeTypeModel.Default.Deserialize(m, null, type);
        }
        return t;
    }

    public static object Deserialize(byte[] buffer, int len, System.Type type)
    {
        object t = null;
        using (MemoryStream m = new MemoryStream(buffer))
        {
            t = RuntimeTypeModel.Default.Deserialize(m, null, type, len);
        }
        return t;
    }
}
