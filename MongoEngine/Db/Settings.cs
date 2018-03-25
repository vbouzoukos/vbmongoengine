using System;
using System.Collections.Generic;
using System.Text;

namespace Vb.Mongo.Engine.Db
{
    public class Settings
    {
        internal String ConnectionString { get; set; }
        static Settings _instance = null;

        internal static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("Mongo Db is not set you need to call Start Up");
                }
                return _instance;
            }
        }

        public static void StartUp(string connectionString)
        {
            _instance = new Settings() { ConnectionString = connectionString };
        }
    }
}
