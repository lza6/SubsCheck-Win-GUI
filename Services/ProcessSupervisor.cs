using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace subs_check.win.gui
{
    /// <summary>
    /// 进程监管：将 subs-check.exe 绑定到 Windows Job Object，
    /// GUI 崩溃/被任务管理器强杀时内核进程随之退出，杜绝孤儿进程。
    /// 通过开关可回退（DisableJobObject=true 时行为与旧版一致）。
    /// </summary>
    public static class ProcessSupervisor
    {
        // 设为 true 可一键回退旧行为（不绑定 Job Object）
        public static bool DisableJobObject = false;

        [Flags]
        private enum JOBOBJECTLIMITS : uint
        {
            JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public JOBOBJECTLIMITS LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, int infoClass,
            IntPtr lpInfo, int cbInfoSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int JobObjectExtendedLimitInformation = 9;
        private static readonly object lockObj = new object();
        private static IntPtr jobHandle = IntPtr.Zero;

        /// <summary>
        /// 将进程绑定到 Kill-on-Close 的 Job Object。
        /// 返回是否绑定成功（失败仅降级为旧行为，不抛异常）。
        /// </summary>
        public static bool Assign(Process process)
        {
            if (process == null || DisableJobObject) return false;
            try
            {
                lock (lockObj)
                {
                    if (jobHandle == IntPtr.Zero)
                    {
                        // 匿名 Job（name=null）：每个 GUI 实例独立持有 Kill-on-Close 句柄；
                        // 命名 job 会被第二实例打开复用，导致先崩溃实例的孤儿内核不被回收
                        jobHandle = CreateJobObject(IntPtr.Zero, null);
                        if (jobHandle == IntPtr.Zero) return false;

                        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                        info.BasicLimitInformation.LimitFlags =
                            JOBOBJECTLIMITS.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
                        try
                        {
                            Marshal.StructureToPtr(info, ptr, false);
                            if (!SetInformationJobObject(jobHandle,
                                JobObjectExtendedLimitInformation, ptr, Marshal.SizeOf(info)))
                            {
                                CloseHandle(jobHandle);
                                jobHandle = IntPtr.Zero;
                                return false;
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                    }

                    return AssignProcessToJobObject(jobHandle, process.Handle);
                }
            }
            catch
            {
                return false; // 绑定失败降级：不影响正常启动
            }
        }
    }
}
