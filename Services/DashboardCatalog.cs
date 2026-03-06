using OpenBoxesMobile.Blazor.Models.Ui;

namespace OpenBoxesMobile.Blazor.Services;

public static class DashboardCatalog
{
    public static readonly IReadOnlyList<DashboardEntryDefinition> Entries =
    [
        new("sortation", "Sortation", "Manage sortation tasks and workflows", "/sortation"),
        new("putaway", "Putaway", "Manage putaway tasks and workflows", "/sortation-putaway"),
        new("orders", "Orders Picking", "Manage orders and picking tasks", "/orders-picking", false),
        new("pickUpAllocation", "Pick-Up Allocation", "Manage pick-up allocations and tasks", "/pickup-allocation"),
        new("packing", "Packing", "Manage packing tasks and shipments", "/packing"),
        new("moveToStaging", "Move To Staging", "Manage moving picked items to staging area.", "/move-to-staging"),
        new("loading", "Loading", "Manage loading tasks and shipments", "/loading"),
        new("receiving", "Receiving", "Manage inbound orders and receiving tasks", "/receiving"),
        new("putawayCandidates", "Putaway Candidates", "View and manage putaway candidates", "/putaway-candidates"),
        new("pendingPutaways", "Pending Putaways", "View and manage pending putaway tasks", "/pending-putaways"),
        new("products", "Products", "Manage products and product details", "/products"),
        new("legacy-inventory", "Legacy Inventory", "View and manage current inventory", "/legacy-inventory"),
        new("createLPN", "Create LPN", "Create a new License Plate Number (LPN)", "/create-lpn"),
        new("transfers", "Transfers", "Manage pending internal transfers", "/transfers"),
        new("inventory", "Inventory", "Manage inventory and cycle counts", "/inventory"),
        new("scan", "Scan", "Scan barcodes and QR codes for quick access", "/scan"),
        new("picking", "Picking", "Manage and group picking tasks", "/picking")
    ];
}
