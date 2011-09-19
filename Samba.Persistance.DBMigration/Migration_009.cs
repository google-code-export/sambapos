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
            Create.Column("UsageCount").OnTable("ScreenMenuItems").AsInt32().WithDefaultValue(0);
            Create.Column("ItemPortion").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("SubButtonHeight").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(65);
            Create.Column("MaxItems").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
            Create.Column("SortType").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            //do nothing
        }
    }
}
