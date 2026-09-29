using Delosi.InvoicingInvoices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres.Configurations;

public sealed class InvoiceTaxConfiguration : IEntityTypeConfiguration<InvoiceTax>
{
    public void Configure(EntityTypeBuilder<InvoiceTax> b)
    {
        b.ToTable("invoice_tax", table =>
        {
            table.HasCheckConstraint("ck_invoice_tax_type", "tax_type in ('IGV', 'ISC', 'DETRACTION', 'SERVICE_CHARGE', 'OTHER')");
            table.HasCheckConstraint("ck_invoice_tax_rate", "rate >= 0 and rate <= 100");
            table.HasCheckConstraint("ck_invoice_tax_base", "taxable_base >= 0 and amount >= 0");
        });
        b.HasKey(x => x.InvoiceTaxId).HasName("pk_invoice_tax");
        b.Property(x => x.InvoiceTaxId).HasColumnName("invoice_tax_id").HasColumnType("bigint").UseIdentityByDefaultColumn().IsRequired();
        b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.InvoiceDetailId).HasColumnName("invoice_detail_id").HasColumnType("bigint");
        b.Property(x => x.TaxType).HasColumnName("tax_type").HasColumnType("varchar(20)").IsRequired().HasMaxLength(20);
        b.Property(x => x.TaxCode).HasColumnName("tax_code").HasColumnType("varchar(10)").IsRequired().HasMaxLength(10);
        b.Property(x => x.Rate).HasColumnName("rate").HasColumnType("numeric(7,4)").IsRequired().HasPrecision(7, 4);
        b.Property(x => x.TaxableBase).HasColumnName("taxable_base").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2);
        b.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2);
        b.HasIndex(x => x.InvoiceId).HasDatabaseName("ix_invoice_tax_invoice");
        b.HasIndex(x => x.InvoiceDetailId).HasFilter("invoice_detail_id is not null").HasDatabaseName("ix_invoice_tax_detail");
    }
}
