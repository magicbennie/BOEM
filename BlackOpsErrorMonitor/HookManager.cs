using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackOpsErrorMonitor
{
    public class HookManager
    {
        public string LastError { get; private set; } = string.Empty;

        private string InjectDLL(IntPtr ProcessHandle, string DllToInject)
        {
            if (ProcessHandle == IntPtr.Zero)
            {
                return "InjectDLL: ProcessHandle is Zero";
            }

            MM_RESULT ModuleHandleResult = MagicMemory.GetAModuleHandle(ProcessHandle, "kernel32.dll");

            if (!ModuleHandleResult.Success || ModuleHandleResult.Int32Result == 0)
            {
                return "InjectDLL: Failed to get handle to kernel32";
            }

            MM_RESULT AddressOfLoadLibraryResult = MagicMemory.GetProcessAddress(ProcessHandle, ModuleHandleResult.Int32Result, "LoadLibraryA");

            if (!AddressOfLoadLibraryResult.Success || AddressOfLoadLibraryResult.Int32Result == 0)
            {
                return "InjectDLL: Failed to get address of LoadLibrary";
            }

            int AddressOfLoadLibrary = AddressOfLoadLibraryResult.Int32Result;

            int PointerToNewlyAllocatedMemory = MagicMemory.AllocateMemory(ProcessHandle, DllToInject.Length + 1);

            if (PointerToNewlyAllocatedMemory == 0)
            {
                return "InjectDLL: PointerToNewlyAllocatedMemory is Zero";
            }

            if (!MagicMemory.WriteString(ProcessHandle, PointerToNewlyAllocatedMemory, DllToInject)) // WriteProcessMemory(ProcessHandle, PointerToNewlyAllocatedMemory, Encoding.ASCII.GetBytes(DllToInject), DllToInject.Length, out BytesWritten);
            {
                MagicMemory.UnAllocateMemory(ProcessHandle, PointerToNewlyAllocatedMemory);
                return "InjectDLL: WriteString failed";
            }

            int ThreadID = MagicMemory.CreateARemoteThread(ProcessHandle, AddressOfLoadLibrary, PointerToNewlyAllocatedMemory);

            if (ThreadID == 0)
            {
                MagicMemory.UnAllocateMemory(ProcessHandle, PointerToNewlyAllocatedMemory);
                return "InjectDLL: ThreadID is zero";
            }

            if (!MagicMemory.WaitForThread(ThreadID))
            {
                MagicMemory.CloseObjectHandle(ThreadID);
                return "InjectDLL: Thread did not return in a timely manner";
            }

            MagicMemory.CloseObjectHandle(ThreadID);
            MagicMemory.UnAllocateMemory(ProcessHandle, PointerToNewlyAllocatedMemory);

            return null;
        }

        public int GetHookDllHandle(Process BlackOps)
        {
            IntPtr BlackOpsHandle = BlackOps.SafeHandle.DangerousGetHandle();

            MM_RESULT ModuleHandleResult = MagicMemory.GetAModuleHandle(BlackOpsHandle, "MagicbennieBO1InternalHooks.dll");

            if (!ModuleHandleResult.Success || ModuleHandleResult.Int32Result == 0)
            {
                //See if it has already been extracted or not
                string DLLLoc = null;

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] ...It failed, about to iterate through Black Ops' module list");

                foreach (ProcessModule CurrentModule in BlackOps.Modules)
                {
                    if (Path.GetFileNameWithoutExtension(CurrentModule.FileName) == "BlackOps")
                    {
                        DLLLoc = Path.GetDirectoryName(CurrentModule.FileName);

                        break;
                    }
                }

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Finished interation!");

                if (DLLLoc == null)
                {
                    //wut

                    LastError = "Failed to find games process, somehow.";

                    return 0;
                }

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Detecting old MagicbennieBO1InternalHooks.dll...");

                //MD5 and compare to stored one, only overwrite if you need to.

                if (File.Exists(DLLLoc + "\\MagicbennieBO1InternalHooks.dll"))
                {
                    //delete old and extract it (until we can work out version info)

                    try
                    {
                        File.Delete(DLLLoc + "\\MagicbennieBO1InternalHooks.dll");

                        //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Deleted old MagicbennieBO1InternalHooks.dll");
                    }
                    catch
                    {
                        LastError = "Unable to delete pre-existing MagicbennieBO1InternalHooks.dll! Is it being locked by another program? Probably.\n\nMake sure you are only running one error monitor? It shouldn't be locked by the game though, otherwise we would have gotten a handle to it eariler. Cheat Engine can also cause this, because it locks the DLL on the disk.";

                        return 0;
                    }

                }

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Writing new MagicbennieBO1InternalHooks.dll");

                File.WriteAllBytes(DLLLoc + "\\MagicbennieBO1InternalHooks.dll", Properties.Resources.MagicbennieBO1InternalHooks);

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Done writing new MagicbennieBO1InternalHooks.dll, about to inject dll...");

                //Inject DLL
                string ErrorMessage = InjectDLL(BlackOpsHandle, DLLLoc + "\\MagicbennieBO1InternalHooks.dll");

                if (ErrorMessage != null)
                {
                    LastError = ErrorMessage;

                    return 0;
                }

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Done injecting MagicbennieBO1InternalHooks.dll, going to get module handle...");

                //Give the compute enough time to load the dll
                Thread.Sleep(100);

                ModuleHandleResult = MagicMemory.GetAModuleHandle(BlackOpsHandle, "MagicbennieBO1InternalHooks.dll");

                if (!ModuleHandleResult.Success || ModuleHandleResult.Int32Result == 0)
                {
                    LastError = "Failed to get module MagicbennieBO1InternalHooks.dll!";
                }
            }

            return ModuleHandleResult.Int32Result;
        }
    }
}
