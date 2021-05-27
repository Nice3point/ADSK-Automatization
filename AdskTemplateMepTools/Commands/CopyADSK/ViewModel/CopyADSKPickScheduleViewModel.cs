using System.Collections.Generic;

namespace AdskTemplateMepTools.Commands.CopyADSK.ViewModel
{
    public class CopyAdskPickScheduleViewModel
    {
        public List<string> ProjectUnusedSpecification{ get; }

        public CopyAdskPickScheduleViewModel(List<string> schedules)
        {
            ProjectUnusedSpecification = schedules;
        }
    }
}