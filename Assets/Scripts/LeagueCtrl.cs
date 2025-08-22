using UnityEngine;

namespace Default.Ctrl
{
    public class LeagueCtrl : Ctrl
    {
        public static void Send_CApply(string msg,int value)
        {
            Debug.LogError($"{msg} {value}");
        }

        public static void Send_CGoTo(int log, string msg, double dur)
        {
            Debug.LogError($"{log} {msg} {dur}");
        }

        public static void NormalMethod()
        {
            
        }
    }
}