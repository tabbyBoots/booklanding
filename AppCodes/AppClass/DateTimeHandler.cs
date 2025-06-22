


namespace mvcDapper3.AppCodes.AppClass;

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime?>
{
    public override void SetValue(IDbDataParameter parameter, DateTime? value)
    {
        parameter.Value = value;
    }

    public override DateTime? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        if (value is string strValue)
        {
            return DateTime.Parse(strValue, CultureInfo.CurrentCulture);
        }
        return Convert.ToDateTime(value);
    }
    
}
