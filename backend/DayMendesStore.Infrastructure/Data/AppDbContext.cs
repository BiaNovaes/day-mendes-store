using System.Linq.Expressions;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Loja> Lojas => Set<Loja>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<VariacaoProduto> VariacoesProduto => Set<VariacaoProduto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.Status));
                var filter = Expression.Lambda(
                    Expression.NotEqual(property, Expression.Constant(Status.Deletado)),
                    parameter
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        modelBuilder.Entity<Loja>(entity =>
        {
            entity.ToTable("Lojas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Senha).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Cnpj).HasMaxLength(20);
            entity.Property(e => e.Foto).HasMaxLength(500);
            entity.Property(e => e.CorPrimaria).HasMaxLength(20);
            entity.Property(e => e.CorSecundaria).HasMaxLength(20);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(300);
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Marca).HasMaxLength(100);
            entity.Property(e => e.Foto).HasMaxLength(500);
            entity.Property(e => e.ValorCompra).HasPrecision(10, 2);
            entity.Property(e => e.ValorVenda).HasPrecision(10, 2);
            entity.Property(e => e.EstoqueMinimo).IsRequired();

            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Variacoes)
                .WithOne(v => v.Produto)
                .HasForeignKey(v => v.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VariacaoProduto>(entity =>
        {
            entity.ToTable("VariacoesProduto", t =>
            {
                t.HasCheckConstraint("CK_VariacoesProduto_QuantidadeEstoque", "`QuantidadeEstoque` >= 0");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tamanho).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Cor).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CodigoBarras).HasMaxLength(80);
            entity.Property(e => e.QuantidadeEstoque).IsRequired();

            entity.HasOne(e => e.Produto)
                .WithMany(p => p.Variacoes)
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ProdutoId, e.Tamanho, e.Cor })
                .IsUnique();

            entity.HasIndex(e => e.CodigoBarras)
                .IsUnique();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Apelido).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Telefone).HasMaxLength(30);
            entity.Property(e => e.Endereco).HasMaxLength(300);
        });

        modelBuilder.Entity<Venda>(entity =>
        {
            entity.ToTable("Vendas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FormaPagamento).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ValorTotal).HasPrecision(10, 2);
            entity.Property(e => e.StatusVenda).IsRequired();
            entity.Property(e => e.MotivoCancelamento).HasMaxLength(500);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.ToTable("ItensVenda");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorUnitario).HasPrecision(10, 2);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);

            entity.HasOne(e => e.Venda)
                .WithMany(v => v.Itens)
                .HasForeignKey(e => e.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.VariacaoProduto)
                .WithMany()
                .HasForeignKey(e => e.VariacaoProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("MovimentacoesEstoque");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Motivo).HasMaxLength(255);
            entity.Property(e => e.Tipo).IsRequired();
            entity.Property(e => e.Quantidade).IsRequired();
            entity.Property(e => e.VariacaoProdutoId).IsRequired();

            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.VariacaoProduto)
                .WithMany()
                .HasForeignKey(e => e.VariacaoProdutoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
