using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VkusnoAndSochno.API.Models;

public partial class VkusnoDbContext : DbContext
{
    public VkusnoDbContext()
    {
    }

    public VkusnoDbContext(DbContextOptions<VkusnoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Dish> Dishes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<OperationItem> OperationItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<ShiftEmployee> ShiftEmployees { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VActiveOrder> VActiveOrders { get; set; }

    public virtual DbSet<VLowStock> VLowStocks { get; set; }

    public virtual DbSet<VStockBalance> VStockBalances { get; set; }

    public virtual DbSet<WarehouseOperation> WarehouseOperations { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.IdBranch).HasName("Branch_pkey");

            entity.ToTable("Branch");

            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(e => e.IdDish).HasName("Dish_pkey");

            entity.ToTable("Dish");

            entity.Property(e => e.IdDish).HasColumnName("id_dish");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.DishName)
                .HasMaxLength(150)
                .HasColumnName("dish_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("Employee_pkey");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.Phone, "Employee_phone_key").IsUnique();

            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdPosition).HasColumnName("id_position");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            entity.HasOne(d => d.IdBranchNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.IdBranch)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Employee_id_branch_fkey");

            entity.HasOne(d => d.IdPositionNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.IdPosition)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Employee_id_position_fkey");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IdIngredient).HasName("Ingredient_pkey");

            entity.ToTable("Ingredient");

            entity.Property(e => e.IdIngredient).HasColumnName("id_ingredient");
            entity.Property(e => e.CostPrice)
                .HasPrecision(10, 2)
                .HasColumnName("cost_price");
            entity.Property(e => e.IngredientName)
                .HasMaxLength(150)
                .HasColumnName("ingredient_name");
            entity.Property(e => e.MinRemaining)
                .HasPrecision(10, 3)
                .HasColumnName("min_remaining");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasColumnName("unit_of_measure");
        });

        modelBuilder.Entity<OperationItem>(entity =>
        {
            entity.HasKey(e => new { e.IdOperation, e.IdIngredient }).HasName("OperationItem_pkey");

            entity.ToTable("OperationItem");

            entity.Property(e => e.IdOperation).HasColumnName("id_operation");
            entity.Property(e => e.IdIngredient).HasColumnName("id_ingredient");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 3)
                .HasColumnName("quantity");

            entity.HasOne(d => d.IdIngredientNavigation).WithMany(p => p.OperationItems)
                .HasForeignKey(d => d.IdIngredient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OperationItem_id_ingredient_fkey");

            entity.HasOne(d => d.IdOperationNavigation).WithMany(p => p.OperationItems)
                .HasForeignKey(d => d.IdOperation)
                .HasConstraintName("OperationItem_id_operation_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.OrderTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_time");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");

            entity.HasOne(d => d.IdBranchNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdBranch)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_id_branch_fkey");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_id_employee_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.IdOrder, e.IdDish }).HasName("OrderItem_pkey");

            entity.ToTable("OrderItem");

            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdDish).HasColumnName("id_dish");
            entity.Property(e => e.PriceAtSale)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_sale");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdDishNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdDish)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderItem_id_dish_fkey");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("OrderItem_id_order_fkey");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.IdPosition).HasName("Position_pkey");

            entity.ToTable("Position");

            entity.Property(e => e.IdPosition).HasColumnName("id_position");
            entity.Property(e => e.PositionName)
                .HasMaxLength(100)
                .HasColumnName("position_name");
            entity.Property(e => e.Salary)
                .HasPrecision(10, 2)
                .HasColumnName("salary");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.IdRecipe).HasName("Recipe_pkey");

            entity.ToTable("Recipe");

            entity.Property(e => e.IdRecipe).HasColumnName("id_recipe");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IdDish).HasColumnName("id_dish");

            entity.HasOne(d => d.IdDishNavigation).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.IdDish)
                .HasConstraintName("Recipe_id_dish_fkey");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IdRecipe, e.IdIngredient }).HasName("RecipeIngredient_pkey");

            entity.ToTable("RecipeIngredient");

            entity.Property(e => e.IdRecipe).HasColumnName("id_recipe");
            entity.Property(e => e.IdIngredient).HasColumnName("id_ingredient");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 3)
                .HasColumnName("quantity");

            entity.HasOne(d => d.IdIngredientNavigation).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.IdIngredient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RecipeIngredient_id_ingredient_fkey");

            entity.HasOne(d => d.IdRecipeNavigation).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.IdRecipe)
                .HasConstraintName("RecipeIngredient_id_recipe_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "Role_role_name_key").IsUnique();

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.IdShift).HasName("Shift_pkey");

            entity.ToTable("Shift");

            entity.Property(e => e.IdShift).HasColumnName("id_shift");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.ShiftDate).HasColumnName("shift_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.IdBranchNavigation).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.IdBranch)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Shift_id_branch_fkey");
        });

        modelBuilder.Entity<ShiftEmployee>(entity =>
        {
            entity.HasKey(e => new { e.IdShift, e.IdEmployee }).HasName("ShiftEmployee_pkey");

            entity.ToTable("ShiftEmployee");

            entity.Property(e => e.IdShift).HasColumnName("id_shift");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.HoursWorked)
                .HasPrecision(5, 2)
                .HasColumnName("hours_worked");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.ShiftEmployees)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ShiftEmployee_id_employee_fkey");

            entity.HasOne(d => d.IdShiftNavigation).WithMany(p => p.ShiftEmployees)
                .HasForeignKey(d => d.IdShift)
                .HasConstraintName("ShiftEmployee_id_shift_fkey");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier).HasName("Supplier_pkey");

            entity.ToTable("Supplier");

            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(150)
                .HasColumnName("contact_person");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(255)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.IdEmployee, "User_id_employee_key").IsUnique();

            entity.HasIndex(e => e.Login, "User_login_key").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            entity.HasOne(d => d.IdEmployeeNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.IdEmployee)
                .HasConstraintName("User_id_employee_fkey");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_id_role_fkey");
        });

        modelBuilder.Entity<VActiveOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_active_orders");

            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(255)
                .HasColumnName("employee_name");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.OrderTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
        });

        modelBuilder.Entity<VLowStock>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_low_stock");

            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdIngredient).HasColumnName("id_ingredient");
            entity.Property(e => e.IngredientName)
                .HasMaxLength(150)
                .HasColumnName("ingredient_name");
            entity.Property(e => e.MinRemaining)
                .HasPrecision(10, 3)
                .HasColumnName("min_remaining");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasColumnName("unit_of_measure");
        });

        modelBuilder.Entity<VStockBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_stock_balance");

            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdIngredient).HasColumnName("id_ingredient");
            entity.Property(e => e.IngredientName)
                .HasMaxLength(150)
                .HasColumnName("ingredient_name");
            entity.Property(e => e.MinRemaining)
                .HasPrecision(10, 3)
                .HasColumnName("min_remaining");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasColumnName("unit_of_measure");
        });

        modelBuilder.Entity<WarehouseOperation>(entity =>
        {
            entity.HasKey(e => e.IdOperation).HasName("WarehouseOperation_pkey");

            entity.ToTable("WarehouseOperation");

            entity.Property(e => e.IdOperation).HasColumnName("id_operation");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.IdBranch).HasColumnName("id_branch");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.OperationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("operation_date");
            entity.Property(e => e.OperationType)
                .HasMaxLength(50)
                .HasColumnName("operation_type");

            entity.HasOne(d => d.IdBranchNavigation).WithMany(p => p.WarehouseOperations)
                .HasForeignKey(d => d.IdBranch)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("WarehouseOperation_id_branch_fkey");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.WarehouseOperations)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("WarehouseOperation_id_employee_fkey");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.WarehouseOperations)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("WarehouseOperation_id_order_fkey");

            entity.HasOne(d => d.IdSupplierNavigation).WithMany(p => p.WarehouseOperations)
                .HasForeignKey(d => d.IdSupplier)
                .HasConstraintName("WarehouseOperation_id_supplier_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
