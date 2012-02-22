using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(14)]
    public class Migration_014 : Migration
    {
        public override void Up()
        {
            Create.Column("DepartmentId").OnTable("TicketItems").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            //do nothing
        }
    }
}
