using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.ChargeAfter.Domain;

namespace Nop.Plugin.Payments.ChargeAfter.Migrations
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/06/28 10:27:55:1687532", "Payments.ChargeAfter base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<OrderPaymentInfoRecord>(Create);
        }
    }
}
