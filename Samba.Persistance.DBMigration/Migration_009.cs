using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(9)]
    public class Migration_009 : Migration
    {
        public override void Up()
        {
            Create.Column("Tag").OnTable("ScreenMenuItems").AsString(128).Nullable();
        }

        public override void Down()
        {
            //do nothing
        }
    }
}
