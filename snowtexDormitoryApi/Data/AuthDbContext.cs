using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Models;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.basicSetup.roomManagements;
using snowtexDormitoryApi.Models.admin.menu;
using snowtexDormitoryApi.Models.admin.RoleBasedUserMenu;
using snowtexDormitoryApi.Models.admin.roomBooking;
using snowtexDormitoryApi.Models.admin.settings;

namespace snowtexDormitoryApi.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }  //Roles is not defined aith perfect way
    public DbSet<TagModel> Tags { get; set; }
    public DbSet<MenuModel> Menus { get; set; }
    public DbSet<RoleBasedMenuModel> RoleBasedMenus { get; set; }
    public DbSet<RoleBasedUserModel> RoleBasedUsers { get; set; }
    public DbSet<BuildingInfoModel> buildingInfoModels { get; set; }
    public DbSet<FloorInfoModel> floorInfoModels { get; set; }
    public DbSet<RoomInfoModel> roomInfoModels { get; set; }
    public DbSet<RoomCommonFeaturesModel> roomCommonFeaturesModels { get; set; }
    public DbSet<RoomAvailableFurnitureModel> roomAFModels { get; set; }
    public DbSet<RoomBedSpecificationModel> roomBedModels { get; set; }
    public DbSet<RoomBathroomSpecificationModel> roomBathroomModels { get; set; }
    public DbSet<RoomDetailsModel> roomDetailsModels { get; set; }
    public DbSet<PaidItemsModels> paidItemModels { get; set; }
    public DbSet<NewPersonModel> newPersonModels { get; set; }
    public DbSet<RoomCategoryModel> roomCategoryModels { get; set; }
    public DbSet<RoomBookingModel> roomBookingModels { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RoleModel>().ToTable("RoleTable"); // Ensure this matches the actual table name in your database
        modelBuilder.Entity<TagModel>().ToTable("TagTable");
        modelBuilder.Entity<MenuModel>().ToTable("MenuTable");
        modelBuilder.Entity<RoleBasedMenuModel>().ToTable("RoleBasedMenuTable");
        modelBuilder.Entity<RoleBasedUserModel>().ToTable("RoleBasedUserTable");
        modelBuilder.Entity<BuildingInfoModel>().ToTable("BuildingTable");
        modelBuilder.Entity<FloorInfoModel>().ToTable("FloorTable");
        modelBuilder.Entity<RoomInfoModel>().ToTable("RoomTable");
        modelBuilder.Entity<RoomCommonFeaturesModel>().ToTable("RoomCommonFeatureTable");
        modelBuilder.Entity<RoomAvailableFurnitureModel>().ToTable("RoomAvailableFurnitureTable");
        modelBuilder.Entity<RoomBedSpecificationModel>().ToTable("RoomBedSpecificationTable");
        modelBuilder.Entity<RoomBathroomSpecificationModel>().ToTable("RoomBathroomSpecificationTable");
        modelBuilder.Entity<RoomDetailsModel>().ToTable("RoomDetailsTable");
        modelBuilder.Entity<PaidItemsModels>().ToTable("PaidItemsTable");
        modelBuilder.Entity<NewPersonModel>().ToTable("NewPersonTable");
        modelBuilder.Entity<RoomCategoryModel>().ToTable("RoomCategoryTable");
        modelBuilder.Entity<RoomBookingModel>().ToTable("RoomBookingTable");

        modelBuilder.Entity<UserModel>()
            .ToTable("userAuth")  // Specify the correct table name here
            .HasIndex(u => u.email)
            .IsUnique();
    }
}
