using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using System.Data;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Payments.ChargeAfter.Mapping.Builders
{
    public class OrderPaymentInfoRecordBuilder : NopEntityBuilder<OrderPaymentInfoRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OrderPaymentInfoRecord.Id))
                    .AsInt32()
                    .PrimaryKey()
                    .Identity()
                .WithColumn(nameof(OrderPaymentInfoRecord.StoreId))
                    .AsInt32()
                .WithColumn(nameof(OrderPaymentInfoRecord.OrderId))
                    .AsInt32()
                    .ForeignKey<Order>(onDelete: Rule.Cascade)
                .WithColumn(nameof(OrderPaymentInfoRecord.PaymentInfo))
                    .AsString(6000);
        }
    }
}
