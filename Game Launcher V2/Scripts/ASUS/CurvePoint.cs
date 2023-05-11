using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Launcher_V2.Scripts.ASUS
{
    public class CurvePoint
    {

        private int x;
        private int y;
        public int Temperature
        {
            get => x;
            set
            {
                x = Math.Max(Math.Min(value, 110), 0);
            }
        }
        public int Fan
        {
            get => y;
            set
            {
                y = Math.Max(Math.Min(value, 100), 0);
            }
        }
    }
}
