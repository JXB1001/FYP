using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    public static class Constants
    {
        
        //Vehicle
        public const int MASS           = 500;
        public const float SAFEDISTANCE = 5f;
        public const float EMERGENCYDISTANCE = 2f;
        public const int sensoryArraySize = 113;
        public const int sensoryArraySize2 = 22;
        public const int numberOfData = 5;

        //Bots
        public const float SPEEDFLOOR   = 1f;
        public const float SPEEDCEIL    = 5f;
        public const float TURNING = 0.05f;

        //
        public const float GOALSPEED    = 4f;

        //Enviroment
        public const float ROADLENGTH   = 50f;
        public const float LANEDIFFERENCE = 1.28f;
        public static int NoOfLanes { get; set; }

        public static int[] ListOfAngle = new int[22];
        public static int LengthOfAngleList { get; set; }

        public static class SteeringValues
        {
            public const float FLOOR = 10f;
            public const float CEIL = 90f;
        }
    }
}
