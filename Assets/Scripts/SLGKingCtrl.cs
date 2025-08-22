using UnityEngine;

namespace Default.Ctrl
{
    public class SLGKingCtrl : Ctrl
    {
        public static void Send_CNotice(string msg)
        {
            Debug.LogError($"{msg} ");
        }

        public static void Send_CWaring(int log, string msg, double dur)
        {
            Debug.LogError($"{log} {msg} {dur}");
        }

        public static void NormalMethod()
        {
            
        }
    }
}