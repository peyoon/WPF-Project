using OJTToolKit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfLogin.Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T_HeadModel
    {
        public byte Source;
        public byte Destition;
        public E_HeadType Type;
        public E_OPCode Opcode;
        public Int32 Size;
        public void ByteSwapLittleOrBig(bool IsBig)
        {
            if (IsBig)
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                    headtypebytes = headtypebytes.Reverse().ToArray();

                    this.Type = (E_HeadType)BitConverter.ToInt16(headtypebytes, 0);

                    byte[] opcodebytes = BitConverter.GetBytes((UInt16)this.Opcode);
                    opcodebytes = opcodebytes.Reverse().ToArray();

                    this.Opcode = (E_OPCode)BitConverter.ToUInt16(opcodebytes, 0);

                    byte[] sizebytes = BitConverter.GetBytes((Int32)this.Size);
                    sizebytes = sizebytes.Reverse().ToArray();

                    this.Size = BitConverter.ToInt32(sizebytes, 0);
                }
                else
                {

                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                    headtypebytes = headtypebytes.Reverse().ToArray();

                    this.Type = (E_HeadType)BitConverter.ToInt16(headtypebytes, 0);

                    byte[] opcodebytes = BitConverter.GetBytes((UInt16)this.Opcode);
                    opcodebytes = opcodebytes.Reverse().ToArray();

                    this.Opcode = (E_OPCode)BitConverter.ToUInt16(opcodebytes, 0);

                    byte[] sizebytes = BitConverter.GetBytes((Int32)this.Size);
                    sizebytes = sizebytes.Reverse().ToArray();

                    this.Size = BitConverter.ToInt32(sizebytes, 0);
                }
                else
                {

                }
            }
        }

        public byte[] GetSizeBytes(bool IsBigendian)
        {
            byte[] returnbytes = new byte[4];

            if (IsBigendian)
            {
                if (BitConverter.IsLittleEndian)
                {
                    returnbytes = BitConverter.GetBytes(this.Size);
                    returnbytes = returnbytes.Reverse().ToArray();
                }
                else
                {
                    returnbytes = BitConverter.GetBytes(this.Size);
                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    returnbytes = BitConverter.GetBytes(this.Size);
                }
                else
                {
                    returnbytes = BitConverter.GetBytes(this.Size);
                    returnbytes = returnbytes.Reverse().ToArray();
                }
            }


            return returnbytes;
        }
        public byte[] GetOpcodeBytes(bool IsBigendian)
        {
            byte[] headtypebytes = new byte[2];

            if (IsBigendian)
            {
                if (BitConverter.IsLittleEndian)
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Opcode);
                    headtypebytes = headtypebytes.Reverse().ToArray();
                }
                else
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Opcode);
                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Opcode);
                }
                else
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Opcode);
                    headtypebytes = headtypebytes.Reverse().ToArray();
                }
            }


            return headtypebytes;
        }
        public byte[] GetHeadTypeBytes(bool IsBigendian)
        {
            byte[] headtypebytes = new byte[2];

            if (IsBigendian)
            {
                if (BitConverter.IsLittleEndian)
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                    headtypebytes = headtypebytes.Reverse().ToArray();
                }
                else
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                }
                else
                {
                    headtypebytes = BitConverter.GetBytes((Int16)this.Type);
                    headtypebytes = headtypebytes.Reverse().ToArray();
                }
            }


            return headtypebytes;
        }
    }


    public static class HeadModel
    {
        public static byte[] HeadMessage(E_OPCode opcode,E_HeadType type,int size)
        {
            byte[] headbytes = new byte[0];
            T_HeadModel headmodel = new T_HeadModel();
            headmodel.Source = 0x01;
            headmodel.Destition = 0xFF;
            headmodel.Opcode = opcode;
            headmodel.Type = type;
            headmodel.Size = size;


            if (BitConverter.IsLittleEndian)
            {
                var t_headmodelsizeof = Marshal.SizeOf(typeof(T_HeadModel));
                headbytes = new byte[t_headmodelsizeof];
                byte[] sourceanddesctition = headmodel.Source.ToAddbyteEx(headmodel.Destition);
                byte[] headtypebytes = headmodel.GetHeadTypeBytes(true);
                byte[] opcodebytes = headmodel.GetOpcodeBytes(true);
                byte[] sizebytes = headmodel.GetSizeBytes(true);

               

                headbytes = sourceanddesctition.ToAddbytearrEx(headtypebytes).ToAddbytearrEx(opcodebytes).ToAddbytearrEx(sizebytes);
            }
            else
            {
                //10
                var t_headmodelsizeof = Marshal.SizeOf(typeof(T_HeadModel));
                //10
                headbytes = new byte[t_headmodelsizeof];

                IntPtr headptr = Marshal.AllocHGlobal(t_headmodelsizeof);
                try
                {
                    Marshal.StructureToPtr(headmodel, headptr, true);
                    Marshal.Copy(headptr, headbytes, 0, t_headmodelsizeof);
                }
                catch
                {

                }
                finally
                {
                    Marshal.FreeHGlobal(headptr);
                }
            }


            return headbytes;
        }


        public static T_HeadModel Frombytes(byte[] data)
        {
            T_HeadModel t_HeadModel = new T_HeadModel();
            int size = Marshal.SizeOf<T_HeadModel>();


            IntPtr headptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(data, 0, headptr, size);
                t_HeadModel = Marshal.PtrToStructure<T_HeadModel>(headptr);
                t_HeadModel.ByteSwapLittleOrBig(false);
            }
            catch
            {

            }

            return t_HeadModel;
        }
    }
}
