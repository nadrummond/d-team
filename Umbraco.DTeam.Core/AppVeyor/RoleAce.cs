using System.Collections.Generic;

namespace ZBuildMon.AppVeyor
{
    public class RoleAce
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public List<AccessRight> AccessRights { get; set; }
    }
}