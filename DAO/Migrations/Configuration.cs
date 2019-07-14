
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace DAO.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DAO.EfContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
    } 
}