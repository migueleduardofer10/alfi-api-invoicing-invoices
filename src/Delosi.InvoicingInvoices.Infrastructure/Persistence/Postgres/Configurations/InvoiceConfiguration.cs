using Delosi.InvoicingInvoices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("invoice", table =>
        {
            table.HasCheckConstraint("ck_invoice_status", "status in ('REGISTERED', 'ISSUED', 'SENT', 'ACCEPTED', 'REJECTED', 'CANCELLED')");
            table.HasCheckConstraint("ck_invoice_dates", "due_date is null or due_date >= issue_date");
            table.HasCheckConstraint("ck_invoice_rate", "exchange_rate > 0");
            table.HasCheckConstraint("ck_invoice_amounts", "subtotal >= 0 and discount >= 0 and service_charge_total >= 0 and tax_total >= 0 and total >= 0");
            table.HasCheckConstraint("ck_invoice_total", "abs(total - (subtotal - discount + service_charge_total + tax_total)) <= 0.01");
            table.HasTrigger("trg_invoice_modified_at");
        });
        b.HasKey(x => x.InvoiceId).HasName("pk_invoice");
        b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasColumnType("bigint").UseIdentityByDefaultColumn().IsRequired();
        b.Property(x => x.CompanyId).HasColumnName("company_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.CustomerId).HasColumnName("customer_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.DocumentTypeId).HasColumnName("document_type_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.SalesOrganizationId).HasColumnName("sales_organization_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.DistributionChannelId).HasColumnName("distribution_channel_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.PaymentTermId).HasColumnName("payment_term_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.PaymentTypeId).HasColumnName("payment_type_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.BrandId).HasColumnName("brand_id").HasColumnType("bigint").IsRequired();
        b.Property(x => x.BrandCode).HasColumnName("brand_code").HasColumnType("varchar(6)").IsRequired().HasMaxLength(6);
        b.Property(x => x.StoreId).HasColumnName("store_id").HasColumnType("bigint");
        b.Property(x => x.StoreCode).HasColumnName("store_code").HasColumnType("varchar(6)").HasMaxLength(6);
        b.Property(x => x.CustomerDocumentNumber).HasColumnName("customer_document_number").HasColumnType("varchar(15)").IsRequired().HasMaxLength(15);
        b.Property(x => x.CustomerLegalName).HasColumnName("customer_legal_name").HasColumnType("varchar(200)").IsRequired().HasMaxLength(200);
        b.Property(x => x.Series).HasColumnName("series").HasColumnType("varchar(4)").IsRequired().HasMaxLength(4);
        b.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasColumnType("varchar(8)").IsRequired().HasMaxLength(8);
        b.Property(x => x.IssueDate).HasColumnName("issue_date").HasColumnType("date").IsRequired();
        b.Property(x => x.DueDate).HasColumnName("due_date").HasColumnType("date");
        b.Property(x => x.Currency).HasColumnName("currency").HasColumnType("char(3)").IsRequired().HasMaxLength(3).IsFixedLength().HasDefaultValue("PEN");
        b.Property(x => x.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("numeric(12,6)").IsRequired().HasPrecision(12, 6).HasDefaultValue(1m);
        b.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.Discount).HasColumnName("discount").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.ServiceChargeTotal).HasColumnName("service_charge_total").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.TaxTotal).HasColumnName("tax_total").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(18,2)").IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        b.Property(x => x.Status).HasColumnName("status").HasColumnType("varchar(20)").IsRequired().HasMaxLength(20).HasDefaultValue("REGISTERED");
        b.Property(x => x.CreatedBy).HasColumnName("created_by").HasColumnType("varchar(50)").HasMaxLength(50);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone").IsRequired().HasDefaultValueSql("now()");
        b.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasColumnType("varchar(50)").HasMaxLength(50);
        b.Property(x => x.ModifiedAt).HasColumnName("modified_at").HasColumnType("timestamp without time zone");
        // PostgreSQL respalda UNIQUE con un índice del mismo nombre. HasIndex permite
        // editar la clave comercial, a diferencia de una alternate key inmutable en EF.
        b.HasIndex(x => new { x.CompanyId, x.DocumentTypeId, x.Series, x.InvoiceNumber })
            .IsUnique().HasDatabaseName("uq_invoice_document");
        b.HasIndex(x => new { x.CustomerId, x.IssueDate }).IsDescending(false, true).HasDatabaseName("ix_invoice_customer_date");
        b.HasIndex(x => new { x.CompanyId, x.IssueDate }).IsDescending(false, true).HasDatabaseName("ix_invoice_company_date");
        b.HasIndex(x => new { x.BrandId, x.IssueDate }).IsDescending(false, true).HasDatabaseName("ix_invoice_brand_date");
        b.HasIndex(x => new { x.StoreId, x.IssueDate }).IsDescending(false, true)
            .HasFilter("store_id is not null").HasDatabaseName("ix_invoice_store_date");
        b.HasIndex(x => new { x.Status, x.IssueDate })
            .HasFilter("status in ('REGISTERED', 'ISSUED', 'REJECTED')").HasDatabaseName("ix_invoice_pending");
        b.Property(x => x.CreatedAt).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        b.Property(x => x.CreatedAt).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        b.Property(x => x.ModifiedAt).ValueGeneratedOnAddOrUpdate();
        b.Property(x => x.ModifiedAt).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        b.Property(x => x.ModifiedAt).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        // Relaciones de navegación en EF. El SQL recibido NO define FOREIGN KEY.
        b.HasMany(x => x.Details).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.ClientNoAction);
        b.HasMany(x => x.Taxes).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.ClientNoAction);
    }
}
