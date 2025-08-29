using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzNet
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// <br/> Endain(little - big) 변환 클래스
    /// <br/> 작 성 자 : 장봉석
    /// <br/> 작 성 일 : 2025년 06월 25일
    /// </summary>
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
    public class Endian
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> Endian(little - big) 변환
        /// <br/>
        /// <br/> 파라미터 : 
        /// <br/>       [in] Input        -  변환할 입력 데이터
        /// <br/>
        /// <br/> 반 환 값 : 변환된 데이터
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 06월 25일
        /// </summary>        
        /// <param name="sInput">   [in] 변환할 입력 데이터 </param>
        /// <returns>   변환된 데이터 </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static short Swap(short sInput)
        {
            short sReturn;

            byte[] byArray = BitConverter.GetBytes(sInput);
            Array.Reverse(byArray);

            sReturn = BitConverter.ToInt16(byArray, 0);

            return sReturn;
        }

        public static ushort Swap(ushort usInput)
        {
            ushort usReturn;

            byte[] byArray = BitConverter.GetBytes(usInput);
            Array.Reverse(byArray);

            usReturn = BitConverter.ToUInt16(byArray, 0);

            return usReturn;
        }

        public static int Swap(int nInput)
        {
            int nReturn;

            byte[] byArray = BitConverter.GetBytes(nInput);
            Array.Reverse(byArray);

            nReturn = BitConverter.ToInt32(byArray, 0);

            return nReturn;
        }

        public static uint Swap(uint unInput)
        {
            uint unReturn;

            byte[] byArray = BitConverter.GetBytes(unInput);
            Array.Reverse(byArray);

            unReturn = BitConverter.ToUInt32(byArray, 0);

            return unReturn;
        }

        public static float Swap(float fInput)
        {
            float fReturn;

            byte[] byArray = BitConverter.GetBytes(fInput);
            Array.Reverse(byArray);

            fReturn = BitConverter.ToSingle(byArray, 0);

            return fReturn;
        }

        public static double Swap(double dInput)
        {
            double dReturn;

            byte[] byArray = BitConverter.GetBytes(dInput);
            Array.Reverse(byArray);

            dReturn = BitConverter.ToSingle(byArray, 0);

            return dReturn;
        }
    }
}
