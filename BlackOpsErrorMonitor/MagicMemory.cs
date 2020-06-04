using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace BlackOpsErrorMonitor
{
    public class MM_RESULT
    {
        public readonly bool BoolResult = false;
        public readonly int Int32Result = 0;
        public readonly uint UInt32Result = 0;
        public readonly long Int64Result = 0;
        public readonly ulong UInt64Result = 0;
        public readonly string StringResult = null;

        private bool GetLastErrorSet = false;
        public int GetLastError { get; private set; } = 0;
        public MagicMemory.MM_ERROR ErrorCode { get; private set; } = MagicMemory.MM_ERROR.MM_NOERROR;

        public bool Success
        {
            get
            {
                return ErrorCode == MagicMemory.MM_ERROR.MM_NOERROR;
            }
        }

        public MM_RESULT() { }
        public MM_RESULT(MagicMemory.MM_ERROR ErrorCode)
        {
            this.ErrorCode = ErrorCode;
        }
        public MM_RESULT(MagicMemory.MM_ERROR ErrorCode, int GetLastErrorResult)
        {
            this.ErrorCode = ErrorCode;
            GetLastError = GetLastErrorResult;
            GetLastErrorSet = true;
        }
        public MM_RESULT(bool Result)
        {
            this.BoolResult = Result;
        }
        public MM_RESULT(int Result)
        {
            this.Int32Result = Result;
        }
        public MM_RESULT(uint Result)
        {
            this.UInt32Result = Result;
        }
        public MM_RESULT(long Result)
        {
            this.Int64Result = Result;
        }
        public MM_RESULT(ulong Result)
        {
            this.UInt64Result = Result;
        }
        public MM_RESULT(string Result)
        {
            this.StringResult = Result;
        }

        public string ErrorString()
        {
            return ErrorCode.ToString() + (GetLastErrorSet ? " (" + GetLastError + ")" : "");
        }

        public override string ToString()
        {
            return StringResult;
        }
    }

    public static class MagicMemory
    {
        //Imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern int VirtualAllocEx(int hProcess, int lpAddress, int dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll")]
        private static extern int CreateRemoteThread(int hProcess, int lpThreadAttributes, int dwStackSize, int lpStartAddress, int lpParameter, int dwCreationFlags, int lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern int VirtualFreeEx(int hProcess, int lpAddress, int dwSize, int dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern int WaitForSingleObjectEx(int hHandle, int dwMilliseconds, bool bAlertable);

        [DllImport("kernel32.dll")]
        private static extern int VirtualProtectEx(int hProcess, int lpAddress, int dwSize, int flNewProtect, out int lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern int GetExitCodeThread(int hThread, out int lpExitCode);

        [DllImport("kernel32.dll")]
        private static extern int GetLastError();

        [DllImport("kernel32.dll")]
        private static extern int GetProcAddress();

        [DllImport("kernel32.dll")]
        private static extern int GetModuleHandle();

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(int Handle);
        [DllImport("User32.dll")]
        private static extern int SetWindowLongA(int hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll")]
        private static extern int GetWindowLongA(int hWnd, int nIndex);
        [DllImport("User32.dll")]
        private static extern int SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

        //Consts for use with imports
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        const int MEM_COMMIT = 0x00001000;
        const int MEM_RELEASE = 0x8000;

        const int PAGE_EXECUTE_READWRITE = 0x40;

        public enum MM_ERROR
        {
            MM_NOERROR = 0,
            MM_CANTOPERATE = 1,
            MM_UNKNOWN = 2,
            MM_ARGERROR = 3,
            //MM_OTHER = 4,
            
            MM_ALLOCFAIL = 4,
            MM_FREEFAIL = 5,
            MM_CRTFAIL = 6,
            MM_WAITTHREADFAIL = 7,
            MM_PROTECTFAIL = 8,
            MM_GETTHREADEXITCODEFAIL = 9,
            MM_CLOSEHANDLEFAIL = 10,

            MM_WRITEWORDFAIL = 11,
            MM_WRITEINTFAIL = 12,
            MM_WRITEUINTFAIL = 13,
            MM_WRITEINT64FAIL = 14,
            MM_WRITEBYTEFAIL = 15,
            MM_WRITEBYTESFAIL = 16,
            MM_WRITESTRINGFAIL = 17,
            MM_WRITEFLOATFAIL = 18,

            MM_READPOINTERFAIL = 19,
            MM_READWORDFAIL = 20,
            MM_READINTFAIL = 21,
            MM_READUINTFAIL = 22,
            MM_READINT64FAIL = 23,
            MM_READFLOATFAIL = 24,
            MM_READSTRINGFAIL = 25,
            MM_READBYTEFAIL = 26,
            MM_READBYTESFAIL = 27,
        };

        public static string GetMM_ERRORString(MM_ERROR Code)
        {
            return Enum.GetName(typeof(MM_ERROR), Code);
        }

        //Checks
        public static bool GetProcessExists(string ProcessName)
        {
            Process[] ProcessList = Process.GetProcessesByName(ProcessName);

            if (ProcessList.Length != 1)
            {
                return false;
            }

            return true;
        }

        public static bool GetProcessExists(int ProcessID)
        {
            Process TargetProcess;

            try
            {
                TargetProcess = Process.GetProcessById(ProcessID);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (TargetProcess.Id == 0)
            {
                return false;
            }

            return true;
        }

        //Actual Functions

        public static int GetProcessID(string ProcessName)
        {
            Process[] ProcessList = Process.GetProcessesByName(ProcessName);

            if (ProcessList.Length != 1)
            {
                return 0;
            }

            return ProcessList[0].Id;
        }

        public static IntPtr GetProcessHandle(string ProcessName)
        {
            IntPtr Handle = IntPtr.Zero;

            Process[] ProcessList = Process.GetProcessesByName(ProcessName);

            if (ProcessList.Length != 1)
            {
                //Failed, process not found.
                return Handle;
            }

            Handle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION, false, ProcessList[0].Id);

            return Handle;
        }

        public static IntPtr GetProcessHandle(int ProcessID)
        {
            IntPtr Handle = IntPtr.Zero;

            Handle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION, false, ProcessID);

            return Handle;
        }

        public static bool CloseObjectHandle(int ObjectHandle)
        {
            return CloseHandle(ObjectHandle) == 0 ? false : true;
        }

        public static MM_RESULT GetAModuleHandle(IntPtr ProcessHandle, string ModuleName)
        {
            //Attempt to allocate memory
            int Address = AllocateMemory(ProcessHandle, ModuleName.Length);

            //Check if the memory was allocated successfully
            if (Address == 0)
            {
                //Something went wrong!
                return new MM_RESULT(MM_ERROR.MM_ALLOCFAIL, GetLastError());
            }

            //Write the ModuleName string to memory
            if (!WriteString(ProcessHandle, Address, ModuleName))
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_WRITESTRINGFAIL, GetLastError());
            }

            //Get GetModuleHandle's address
            int GetModuleHandleAddress = ReadInt(ProcessHandle, 0x009A30F0);

            //Create a thread and point it to GetModuleHandle. Pass the address of the ModuleName string over as the parameter.
            int ThreadHandle = CreateARemoteThread(ProcessHandle, GetModuleHandleAddress, Address);

            //Check if the thread handle is valid
            if (ThreadHandle == 0)
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_CRTFAIL, GetLastError());
            }

            //Attempt to wait for the thread to finish
            if (!WaitForThread(ThreadHandle))
            {
                //Something went wrong!

                //Close the thread handle
                CloseHandle(ThreadHandle);

                //Don't unallocate the space, if its still using it, it could cause a crash

                return new MM_RESULT(MM_ERROR.MM_WAITTHREADFAIL);
            }

            //Get the result (the result of GetModuleHandle)
            int Result = GetThreadExitCode(ThreadHandle);

            //Close the thread handle
            CloseHandle(ThreadHandle);

            //Unallocate the space for the module name
            UnAllocateMemory(ProcessHandle, Address);

            //Return the result of the thread 
            return new MM_RESULT(Result);
        }

        public static MM_RESULT GetProcessAddress(IntPtr ProcessHandle, int ModuleHandle, string FunctionName)
        {
            //Check if the passed parameters are valid
            if (FunctionName == null || ModuleHandle == 0 || FunctionName.Length == 0)
            {
                return new MM_RESULT(MM_ERROR.MM_ARGERROR);
            }

            //Declare our "payload"
            byte[] ASM = new byte[] { 0x8B, 0x44, 0x24, 0x04, 0xFF, 0x30, 0x83, 0xC0, 0x04, 0xFF, 0x30, 0xE8, 0xFF, 0xFF, 0xFF, 0xFF, 0xC3 };

            //Attempt to allocate some space in the target process
            int Address = AllocateMemory(ProcessHandle, ASM.Length + FunctionName.Length + 1 + 8);

            //Check if the memory was allocated successfully
            if (Address == 0)
            {
                //It wasn't

                return new MM_RESULT(MM_ERROR.MM_ALLOCFAIL, GetLastError());
            }

            //Place our relative jump into the asm string
            Buffer.BlockCopy(BitConverter.GetBytes(CalculateRelativeAddress(ReadInt(ProcessHandle, 0x009A30EC), (Address + FunctionName.Length + 1 + 16))), 0, ASM, 12, 4);

            //Write the FunctionName string to memory
            if (!WriteString(ProcessHandle, Address, FunctionName))
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_WRITESTRINGFAIL, GetLastError());
            }

            //Next, write the ASM string.
            if (!WriteBytes(ProcessHandle, Address + FunctionName.Length + 1, ASM))
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_WRITEBYTESFAIL, GetLastError());
            }

            //Address of FunctionName string
            if (!WriteInt(ProcessHandle, Address + FunctionName.Length + 1 + ASM.Length, Address))
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_WRITEINTFAIL, GetLastError());
            }

            //Write the ModuleHandle to memory
            if (!WriteInt(ProcessHandle, Address + FunctionName.Length + 1 + ASM.Length + 4, ModuleHandle))
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_WRITEINTFAIL, GetLastError());
            }

            //Create a remote thread and point it to the start of the ASM. Pass the pointer to the addresses as the parameter.
            int ThreadHandle = CreateARemoteThread(ProcessHandle, Address + FunctionName.Length + 1, Address + FunctionName.Length + 1 + ASM.Length);

            //Check if the thread handle is valid
            if (ThreadHandle == 0)
            {
                //Something went wrong!

                //Unallocate memory and return
                UnAllocateMemory(ProcessHandle, Address);

                return new MM_RESULT(MM_ERROR.MM_CRTFAIL, GetLastError());
            }

            //Attempt to wait for the thread to finish
            if (!WaitForThread(ThreadHandle))
            {
                //Something went wrong!

                //Close the thread handle
                CloseHandle(ThreadHandle);

                return new MM_RESULT(MM_ERROR.MM_WAITTHREADFAIL);
            }

            //Get the result (the result of getprocaddress)
            int Result = GetThreadExitCode(ThreadHandle);

            //Close the thread handle
            CloseHandle(ThreadHandle);

            //Unallocate memory
            UnAllocateMemory(ProcessHandle, Address);

            //Return the result of the thread 
            return new MM_RESULT(Result);
        }

        public static bool WriteFloat(IntPtr ProcessHandle, int Address, float Value)
        {
            int OutBytesWritten = 0;

            WriteProcessMemory((int)ProcessHandle, Address, BitConverter.GetBytes(Value), 4, ref OutBytesWritten);

            if (OutBytesWritten == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes an int (4 bytes) to the specified location.
        /// </summary>
        /// <param name="ProcessHandle">The handle to the target process.</param>
        /// <param name="Address">The base address to write the int to.</param>
        /// <param name="Value">The int to be written.</param>
        /// <returns>Returns whether or not the int was successfully written to memory.</returns>
        public static bool WriteUInt(IntPtr ProcessHandle, int Address, UInt32 Value)
        {
            int OutBytesWritten = 0;

            WriteProcessMemory((int)ProcessHandle, Address, BitConverter.GetBytes(Value), 4, ref OutBytesWritten);

            if (OutBytesWritten == 0)
            {
                return false;
            }

            return true;
        }

        public static bool WriteInt(IntPtr ProcessHandle, int Address, int Value)
        {
            int OutBytesWritten = 0;

            byte[] Bytes = BitConverter.GetBytes(Value);

            WriteProcessMemory((int)ProcessHandle, Address, Bytes, 4, ref OutBytesWritten);

            if (OutBytesWritten == 0)
            {
                return false;
            }

            return true;
        }

        public static bool WriteString(IntPtr ProcessHandle, int Address, string StringToWrite)
        {
            int OutBytesWritten = 0;

            WriteProcessMemory((int)ProcessHandle, Address, Encoding.ASCII.GetBytes(StringToWrite), StringToWrite.Length + 1, ref OutBytesWritten);

            if (OutBytesWritten == 0)
            {
                return false;
            }

            return true;
        }

        public static bool WriteBytes(IntPtr ProcessHandle, int Address, byte[] buffer)
        {
            int OutBytesWritten = 0;

            WriteProcessMemory((int)ProcessHandle, Address, buffer, buffer.Length, ref OutBytesWritten);

            if (OutBytesWritten == 0)
            {
                return false;
            }

            return true;
        }

        public static float ReadFloat(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[4] { 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 4, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0.0f;
            }

            return BitConverter.ToSingle(buffer, 0);
        }

        public static UInt32 ReadUInt(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[4] { 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 4, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return BitConverter.ToUInt32(buffer, 0);
        }

        public static int ReadInt(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[4] { 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 4, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return BitConverter.ToInt32(buffer, 0);
        }

        public static long ReadLong(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 8, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return BitConverter.ToInt64(buffer, 0);
        }

        public static ulong ReadULong(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 8, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return BitConverter.ToUInt64(buffer, 0);
        }

        public static UInt16 ReadWord(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[2] { 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 2, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return BitConverter.ToUInt16(buffer, 0);
        }

        public static byte ReadByte(IntPtr ProcessHandle, int Address)
        {
            byte[] buffer = new byte[1] { 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 1, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return buffer[0];
        }

        public static byte[] ReadBytes(IntPtr ProcessHandle, int Address, int AmountOfBytesToRead)
        {
            if (AmountOfBytesToRead == 0)
            {
                AmountOfBytesToRead = 1;
            }

            byte[] buffer = new byte[AmountOfBytesToRead];

            for (int i = 0; i < AmountOfBytesToRead; i++)
            {
                buffer[i] = 0;
            }

            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, AmountOfBytesToRead, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return buffer;
            }

            return buffer;
        }

        public static int ReadPointer(IntPtr ProcessHandle, int Address, int Offset)
        {
            byte[] buffer = new byte[4] { 0, 0, 0, 0 };
            int OutBytesRead = 0;

            ReadProcessMemory((int)ProcessHandle, Address, buffer, 4, ref OutBytesRead);

            if (OutBytesRead == 0)
            {
                return 0;
            }

            return (BitConverter.ToInt32(buffer, 0) + Offset);
        }

        /// <summary>
        /// Allocates memory in the target process.
        /// </summary>
        /// <returns>Returns a pointer to the begining of the allocated memory</returns>
        public static int AllocateMemory(IntPtr ProcessHandle, int SizeOfMemoryBlockNeeded)
        {
            return VirtualAllocEx((int)ProcessHandle, 0, SizeOfMemoryBlockNeeded, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        }

        public static int AllocateAndZeroMemory(IntPtr ProcessHandle, int SizeOfMemoryBlockNeeded)
        {
            int Result = VirtualAllocEx((int)ProcessHandle, 0, SizeOfMemoryBlockNeeded, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            if (Result != 0)
            {
                WriteBytes(ProcessHandle, Result, new byte[SizeOfMemoryBlockNeeded]);
            }

            return Result;
        }

        public static int UnAllocateMemory(IntPtr ProcessHandle, int Address)
        {
            return VirtualFreeEx((int)ProcessHandle, Address, 0, MEM_RELEASE);
        }

        public static int CreateARemoteThread(IntPtr ProcessHandle, int Address, int Parameter = 0)
        {
            return CreateRemoteThread((int)ProcessHandle, 0, 0, Address, Parameter, 0, 0);
        }

        public static bool WaitForThread(int ObjectReference, int Timeout = 100)
        {
            int Result = WaitForSingleObjectEx(ObjectReference, Timeout, false);

            if (Result == -1)
            {
                return false;
            }

            return true;
        }

        public static int CalculateRelativeAddress(int Target, int NextInstructionEIP)
        {
            return Target - NextInstructionEIP;
        }

        public static bool UnprotectMemory(IntPtr ProcessHandle, int Address, int Size, out int OldProtection)
        {
            OldProtection = 0;

            if (Size <= 0)
            {
                return false;
            }

            if (VirtualProtectEx((int)ProcessHandle, Address, Size, PAGE_EXECUTE_READWRITE, out OldProtection) == 0)
            {
                return false;
            }

            return true;
        }

        public static bool ReprotectMemory(IntPtr ProcessHandle, int Address, int Size, int NewProtection, out int CurrentProtection)
        {
            CurrentProtection = 0;

            if (Size <= 0)
            {
                return false;
            }

            if (VirtualProtectEx((int)ProcessHandle, Address, Size, NewProtection, out CurrentProtection) == 0)
            {
                return false;
            }

            return true;
        }

        public static int GetThreadExitCode(int ThreadHandle)
        {
            int Result = 0;

            int FunctionReturned = GetExitCodeThread(ThreadHandle, out Result);

            return Result;
        }

        public static string ReadString(IntPtr ProcessHandle, int Address)
        {
            List<byte> buffer = new List<byte>();

            int Counter = 0;

            byte CurrentByte = ReadBytes(ProcessHandle, Address + Counter, 1)[0];

            while (CurrentByte != 0x00 && Counter < 65500)
            {
                buffer.Add(CurrentByte);
                Counter++;
                CurrentByte = ReadBytes(ProcessHandle, Address + Counter, 1)[0];
            }

            return Encoding.ASCII.GetString(buffer.ToArray());
        }

        public static int GetTheLastError()
        {
            return GetLastError();
        }
    }
}
