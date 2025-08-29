using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OJTToolKit.Utils
{
    public static class UtilEx
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// byte + byte 
        /// 파라미터 : [in] left	-  left
        ///            [in] right	-  right
        /// 반 환 값 : -
        /// 작 성 자 : 강현우
        /// 작 성 일 : 2025년 08월 22일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] ToAddbyteEx(this byte left, byte right)
        {
            byte[] result = new byte[]
            {
                left,right
            };

            return result;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// byte arr + byte arr 
        /// 파라미터 : [in] left  arr	-  left arr
        ///            [in] right arr	-  right arr
        /// 반 환 값 : -
        /// 작 성 자 : 강현우
        /// 작 성 일 : 2025년 08월 22일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] ToAddbytearrEx(this byte[] left, byte[] right)
        {
            byte[] result = new byte[left.Length + right.Length];

            Array.Copy(left, 0, result, 0, left.Length);
            Array.Copy(right, 0, result, left.Length, right.Length);

            return result;
        }

        public static byte[] ToUnionBytes(this List<byte[]> bytes)
        {
            byte[] result = new byte[bytes.Sum(i => i.Length)];

            int idx = 0;
            foreach (var item in bytes)
            {
                Array.Copy(item, 0, result, idx, item.Length);

                idx = idx + item.Length;
            }

            return result;
        }

        public static byte[] ToSubBytes(this byte[] bytes, int startidx, int length)
        {
            var subbytes = new byte[length];

            Array.Copy(bytes, startidx, subbytes, 0, length);

            return subbytes;
        }


    }
}
