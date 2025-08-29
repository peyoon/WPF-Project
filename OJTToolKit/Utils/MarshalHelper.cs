using System;
using System.Runtime.InteropServices;

namespace OzUtil
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// <br/> 마샬링용 Util
    /// <br/> 작 성 자 : 장봉석
    /// <br/> 작 성 일 : 2025년 05월 14일
    /// </summary>
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////	
    public class MarshalHelper
    {

        #region 템플릿 사용 - object 박싱/언박싱 속도에 대한 대안

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> byte 배열을 구조체로 변환 
        /// <br/> 
        /// <br/> 파라미터  
        /// <br/>   [in] data -  byte 배열
        /// <br/>   [in] size  - 변환 구조체 or 클래스 사이즈
        /// <br/> 
        /// <br/> 반 환 값 : 변환된 구조체 : T = 구조체 or 클래스
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>                
        /// <typeparam name="T">구조체 or 클래스 타입</typeparam>
        /// <param name="data">byte 배열</param>
        /// <param name="size">변환 구조체 or 클래스 사이즈</param>
        /// <returns>변환된 구조체 : T = 구조체 or 클래스</returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static T ByteToStructure<T>(byte[] data, int size)
        {
            IntPtr buff = Marshal.AllocHGlobal(size);       // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당.

            Marshal.Copy(data, 0, buff, size);              // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사.
            T st = Marshal.PtrToStructure<T>(buff);         // 복사된 데이터를 구조체 객체로 변환.

            Marshal.FreeHGlobal(buff);                      // 비관리 메모리 영역에 할당했던 메모리를 해제

            return st;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 구조체를 byte 배열로 변환
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>    [in] st -  변환할 구조체 or 클래스 : T = 구조체 or 클래스
        /// <br/> 
        /// <br/> 반 환 값 : 변환된 byte 배열
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일  2025년 05월 14일
        /// </summary>
        /// <typeparam name="T">구조체 or 클래스 타입</typeparam>
        /// <param name="st">[in] 변환할 구조체 or 클래스 : T = 구조체 or 클래스</param>
        /// <returns>변환된 byte 배열</returns>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] StructToByte<T>(T st)
        {
            int size = Marshal.SizeOf<T>();

            byte[] buffer = new byte[size];
            unsafe                                                  // /ERROR : 안전하지 않은 코드는 unsafe를 사용하여 컴파일하는 경우에만 나타날 수 있다.
                                                                    // 프로젝트 속성 - 빌드 - 안전하지 않은 코드 허용 체크박스
            {
                fixed (byte* fixed_buffer = buffer)                 //포인터 변수 선언 - 변수 직접 접근
                {
                    Marshal.StructureToPtr(st, (IntPtr)fixed_buffer, false);
                }
            }

            return buffer;
        }

        #endregion 템플릿 사용 - object 박싱/언박싱 속도에 대한 대안



        #region Object 사용

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> byte 배열을 구조체로 변환
        /// <br/> 
        /// <br/> 파라미터 :
        /// <br/>       [in] data -  byte 배열
        /// <br/>       [in] type -  변환 구조체 or 클래스 타입
        /// <br/>       [in] size  - 변환 구조체 or 클래스 사이즈
        /// <br/> 
        /// <br/> 반 환 값 : 변환된 구조체 (object로 박싱 되어 있음)
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="data">[in] byte 배열</param>
        /// <param name="type">[in] 변환 구조체 or 클래스 타입</param>
        /// <param name="size">[in] 변환 구조체 or 클래스 사이즈</param>
        /// <returns>변환된 구조체 (object로 박싱 되어 있음)</returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static object ByteToStructure(byte[] data, Type type, int size)
		{
			IntPtr buff = Marshal.AllocHGlobal(size);       // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당.

			Marshal.Copy(data, 0, buff, size);              // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사.
			object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환.

			Marshal.FreeHGlobal(buff);                      // 비관리 메모리 영역에 할당했던 메모리를 해제

			if (Marshal.SizeOf(obj) != size)                // 구조체와 원래의 데이터의 크기 비교
			{
                obj = null;                                 // 크기가 다르면 null 리턴
			}

			return obj; 
		}

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 구조체를 byte 배열로 변환
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] st -  변환할 구조체 or 클래스
        /// <br/> 
        /// <br/> 반 환 값 : 변환된 byte 배열
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="st">[in] 변환할 구조체 or 클래스</param>
        /// <returns>변환된 byte 배열</returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] StructToByte(object st)
		{
			byte[] buffer = new byte[Marshal.SizeOf(st)];
			unsafe                                                  // /ERROR : 안전하지 않은 코드는 unsafe를 사용하여 컴파일하는 경우에만 나타날 수 있다.
																	// 프로젝트 속성 - 빌드 - 안전하지 않은 코드 허용 체크박스
			{
				fixed (byte* fixed_buffer = buffer)					//포인터 변수 선언 - 변수 직접 접근
				{
					Marshal.StructureToPtr(st, (IntPtr)fixed_buffer, false);
				}
			}
			return buffer;
		}

        #endregion Object 사용


    }

}
