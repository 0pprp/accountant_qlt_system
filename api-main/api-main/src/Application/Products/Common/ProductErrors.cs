namespace Application.Products.Common;

public static class ProductErrors
{
    public static Error ProductCategoryNotFound = new("لم يتم العثور على فئة المنتج", "ProductCategory_Not_Found");
    public static Error WarehouseNotFound = new("لم يتم العثور على الخزينة", "Warehouse_Not_Found");
    public static Error ProductNotFound = new("لم يتم العثور على المنتج", "Product_Not_Found");
    public static Error ProductStillReferencedByOrderItems = new("لا يمكن حذف المنتج: لا تزال تتم الإشارة إليها بواسطة الطلب", "Product_Still_Referenced_By_OrdersItems");
    public static Error ProductStillReferencedByPurchaseItems = new("المنتج لا يزال مذكورًا في المشتريات", "Product_Still_Referenced_By_PurchaseItems");
}