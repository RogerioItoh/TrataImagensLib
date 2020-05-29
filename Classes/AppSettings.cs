using System.Configuration;

namespace Classes
{
    public class AppSettings
    {

        Configuration config;


        public AppSettings()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }


        public string GetConnectionString(string strKey)
        {
            //strkey = nome da connectionstring
            return config.ConnectionStrings.ConnectionStrings[strKey].ConnectionString;
        }

        public void UpdateConnectionString(string strKey, string strConexao)
        {
            config.ConnectionStrings.ConnectionStrings[strKey].ConnectionString = strConexao;
            config.ConnectionStrings.ConnectionStrings[strKey].ProviderName = "Oracle.ManagedDataAccess.Client;";
            config.Save(ConfigurationSaveMode.Modified);
        }





    }
}
