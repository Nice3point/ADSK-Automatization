using System.Collections.Generic;

namespace AdskTemplateMepTools.Commands.CopyADSK.ViewModel
{
    public class CopyAdskPickScheduleViewModel
    {
        private List<string> _selectedUnusedSpecification;

        public CopyAdskPickScheduleViewModel(List<string> schedules)
        {
            ProjectUnusedSpecification = schedules;
        }

        public List<string> ProjectUnusedSpecification { get; }

        public List<string> SelectedUnusedSpecification
        {
            get => _selectedUnusedSpecification ?? new List<string>();
            set => _selectedUnusedSpecification = value;
        }
    }
}