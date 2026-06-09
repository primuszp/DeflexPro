using System;

namespace DeflexPro.ViewModel
{
    public class EnumValueViewModel : ViewModelBase
    {
        public Enum Value { get; private set; }
        public string DisplayName { get; private set; }

        public EnumValueViewModel(Enum enumValue)
        {
            this.Value = enumValue;
            this.DisplayName = enumValue.ToString();
        }
    }
}