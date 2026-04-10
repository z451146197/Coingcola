using System.Security.Principal;

namespace Coingcola.Helpers
{
    /// <summary>
    /// 管理员权限辅助类。
    /// 
    /// 当前只负责一件事：
    /// 判断当前程序是否以管理员身份运行。
    /// 
    /// 后续如果你要增加：
    /// - 主动提升权限
    /// - 判断 UAC 状态
    /// - 检测是否关闭 UAC
    /// 都可以继续放在这里。
    /// </summary>
    public static class AdminHelper
    {
        /// <summary>
        /// 判断当前程序是否为管理员运行。
        /// </summary>
        public static bool IsRunAsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}