namespace Application.ProductCategories.Common;

public static class ProductCategoryErrors
{
    public static Error ProductCategoryAlreadyExist = new("هذا الفئة موجود", "ProductCategory_Already_Exist");
    public static Error ProductCategoryNotFound = new("لم يتم العثور على الفئة", "ProductCategory_Not_Found");
    public static Error ProductCategoryStillReferencedByProducts = new("لا يمكن حذف الفئة: لا تزال تتم الإشارة إليها بواسطة المنتج", "ProductCategory_Still_Referenced_By_Products");
}