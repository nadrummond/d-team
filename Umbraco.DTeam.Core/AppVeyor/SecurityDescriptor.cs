using System.Collections.Generic;

namespace ZBuildMon.AppVeyor
{
    public class SecurityDescriptor
    {
        public List<AccessRightDefinition> AccessRightDefinitions { get; set; }
        public List<RoleAce> RoleAces { get; set; }
    }
}