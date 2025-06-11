using System;

namespace Marten.Testing
{
    public class ConnectionSource : ConnectionFactory
    {
        public static readonly string ConnectionString = "Command Timeout=0;Database=marten-test-db;MaxPoolSize=10;Password=postgres;Port=5446;Server=localhost;Timeout=60;User Id=postgres";

        static ConnectionSource()
        {
            if (ConnectionString.IsEmpty())
                throw new Exception(
                    "You need to set the connection string for your local Postgresql database in the environment variable 'marten_testing_database'");
        }


        public ConnectionSource() : base(ConnectionString)
        {
        }
    }
}