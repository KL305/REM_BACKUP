using System.Configuration;

namespace REM_System.Data
{
    internal static class Database
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["RealEstateDb"]?.ConnectionString;
                return cs ?? string.Empty;
            }
        }
    }
}


