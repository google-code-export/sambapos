using Samba.Domain.Models.Settings;

namespace Samba.Domain
{
    public class SettingGetter
    {
        private readonly ProgramSetting _programSetting;

        public SettingGetter(ProgramSetting programSetting)
        {
            _programSetting = programSetting;
        }

        public string StringValue { get { return _programSetting.Value; } set { _programSetting.Value = value; } }

        public int IntegerValue
        {
            get
            {
                int result;
                int.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public decimal DecimalValue
        {
            get
            {
                decimal result;
                decimal.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public bool BoolValue
        {
            get
            {
                bool result;
                bool.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }
    }
}
