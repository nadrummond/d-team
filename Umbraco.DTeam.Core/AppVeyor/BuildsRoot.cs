using System.Collections.Generic;

namespace ZBuildMon.AppVeyor
{
    public class BuildsRoot
    {
        public Project Project { get; set; }
        public List<Build> Builds { get; set; }
    }
}