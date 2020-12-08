using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nice3point.Revit.ADSK.MEP.ViewModel
{
    public class CopyAdskPickScheduleViewModel
    {
        public List<string> ProjectUnusedSpecification{ get; set; }

        public CopyAdskPickScheduleViewModel(List<string> schedules)
        {
            ProjectUnusedSpecification = schedules;
        }
    }
}