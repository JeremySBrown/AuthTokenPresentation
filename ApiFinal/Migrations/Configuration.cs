using ApiFinal.Datasource;

namespace ApiFinal.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApiFinal.Datasource.SimpleDatasourceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            
        }

        protected override void Seed(ApiFinal.Datasource.SimpleDatasourceContext context)
        {
            SeedHelper.SeedDatabase(context);
        }
    }
}
