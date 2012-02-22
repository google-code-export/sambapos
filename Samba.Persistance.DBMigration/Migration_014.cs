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
            Create.Column("DepartmentId").OnTable("Terminals").AsInt32().WithDefaultValue(0);
            Create.Column("DepartmentId").OnTable("Payments").AsInt32().WithDefaultValue(0);

            Execute.Sql("Update TicketItems set DepartmentId = (Select DepartmentId From Tickets Where Id = TicketItems.TicketId)");
            Execute.Sql("Update Payments set DepartmentId = (Select DepartmentId From Tickets Where Id = Payments.Ticket_Id)");
        }

        public override void Down()
        {
            //do nothing
        }
    }
}