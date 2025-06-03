using Microsoft.Data.SqlClient;
using System;
using System.Configuration;

namespace Mailer.Sql
{
    public class SqlConnectionHelper
    {
        #region GetConnection
        public static SqlConnection GetConnection(string NameOrConnectionString=null)
        {
            SqlConnection connection = null;
            ConnectionStringSettings connectionStringSettings = null;
            string connectionString = null;

            var connectionStrings = ConfigurationManager.ConnectionStrings;

            //if name or connectionstring is provided
            if (!String.IsNullOrEmpty(NameOrConnectionString))
            {

                string[] segments = NameOrConnectionString.Split('=');

                //see how many segments we have in the provided name or connectionstring
                if (segments.Length > 0)
                {
                    var firstKey = segments[0].Trim();
                    //if only 2 segment and it is called name 
                    if ((segments.Length == 2) && (firstKey.ToLower() == "name"))
                    {
                        //then get the connectionstring from the connectionstrings node in config
                        connectionStringSettings = connectionStrings[segments[1]];
                    }
                    else
                    {
                        //otherwise this is a connectionstring
                        connectionString = NameOrConnectionString;
                    }
                }
                else
                {
                    //if not segment (ie - no equal sign) then retrieve from config by name
                    connectionStringSettings = connectionStrings[NameOrConnectionString];
                }

            }
            else
            {
                //nothing was specified - so use the first connection in connectionstrings
                if (connectionStrings.Count > 0)
                {
                    //take the first connection as default
                    connectionStringSettings = connectionStrings[0];

                }
            }

            if (connectionStringSettings != null)
            {
                connectionString = connectionStringSettings.ConnectionString;
            }

            //build connection
            connection = new SqlConnection(connectionString);

            return connection;
        }
        #endregion GetConnection
    }
}
