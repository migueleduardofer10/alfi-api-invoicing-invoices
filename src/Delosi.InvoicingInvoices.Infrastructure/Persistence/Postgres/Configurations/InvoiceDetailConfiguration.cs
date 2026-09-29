using Delosi.InvoicingInvoices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres.Configurations;

public sealed class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> b)
    {
        b.ToTable("invoice_detail", table =>
        {
            table.HasCheckConstraint("ck_invoice_detail_qty", "quantity > 0");
            table.HasCheckConstraint("ck_invoice_detail_line", "line_number > 0");
            table.HasCheckConstraint("ck_invoice_detail_amounts", "unit_price >= 0 and discount >= 0 and subtotal >= 0 and service_charge >= 0 and tax_amount >= 0 and total >= 0");
        });
        b.HasKey(x => x.InvoiceDetailId).HasName("pk_invoice_detail");
        b.Property(x => x.InvoiceDetailId).HasColumnName("invoice_detail_id").HasColumnType("bigint").UseIdentityByDefaultColumn().IsRequired();
        b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.LineNumber).HasColumnName("line_number").HasColumnType("integer").IsRequired();
        b.Property(x => x.ConceptId).HasColumnName("concept_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.ConceptCode).HasColumnName("concept_code").HasColumnType("varchar(20)").IsRequired().HasMaxLength(20);
        b.Property(x => x.MaterialId).HasColumnName("material_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.MaterialCode).HasColumnName("material_code").HasColumnType("varchar(20)").IsRequired().HasMaxLength(20);
        b.Property(x => x.AccountingAccountId).HasColumnName("accounting_account_id").HasColumnType("bigint");
        b.Property(x => x.Description).HasColumnName("description").HasColumnType("varchar(250)").IsRequired().HasMaxLength(250);
        b.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("numeric(14,4)").IsRequired().HasPrecision(14, 4);
        b.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,6)").IsRequired().HasPrecision(18, 6);
        b.Property(x => x.Discount).HasColumnName("discount").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.ServiceCharge).HasColumnName("service_charge").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.HasIndex(x => new { x.InvoiceId, x.LineNumber }).IsUnique().HasDatabaseName("uq_invoice_detail_line");
        b.HasIndex(x => x.ConceptId).HasDatabaseName("ix_invoice_detail_concept");
        b.HasIndex(x => x.MaterialId).HasDatabaseName("ix_invoice_detail_material");
        b.HasIndex(x => x.AccountingAccountId).HasFilter("accounting_account_id is not null").HasDatabaseName("ix_invoice_detail_account");
        b.HasMany(x => x.Taxes).WithOne(x => x.InvoiceDetail).HasForeignKey(x => x.InvoiceDetailId).OnDelete(DeleteBehavior.ClientNoAction);
    }
}
