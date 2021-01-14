using System.Collections.Generic;

namespace Nice3point.Revit.ADSK.MEP.Commands.CopyADSK
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