using UnityEngine;

namespace Default.Ctrl
{
    public class LeagueCtrl : Ctrl
    {
        public static void Send_CApply(string msg, int value)
        {
            Debug.LogError($"{msg} {value}");
        }

        public static void Send_CGoTo(int log, string msg, double dur)
        {
            Debug.LogError($"{log} {msg} {dur}");
        }

        public static int NormalMethod()
        {
            var sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += i;
            }
            Debug.LogError(sum);
            return sum;
        }

        public int GoTo(int log)
        {
            var sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += i;
            }
            Debug.LogError(sum);
            return sum;
        }
    }
}