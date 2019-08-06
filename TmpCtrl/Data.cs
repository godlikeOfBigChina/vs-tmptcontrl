using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TmpCtrl
{
    public static class Data
    {

        //interval is one hour
        public static float[] time =new float[]{0,  72, 96, 132, 180, 220};
        public static float[] t0 = new float[]{ 80, 300,420,600, 600, 950};
        public static float[] k = new float[] {3.05f,5, 5,  0,   8.75f };

        public static float getTarget(int t)
        {
            float target = 0;
            for (int x=0;x<time.Length;x++)
            {
                if (60*2*time[x] <= t && t < 60*2*time[x+1])
                {
                    target = t0[x] + (t - 60*2 * time[x]) * k[x]/2;
                    return target;
                }
            }
            return 2222;
        }
    }
}
