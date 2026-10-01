namespace lab1
{
    internal class Buyer
    {
        private Product[] _products;

        public Buyer(Product[] products)
        {
            _products = products;
        }

        public Product[] GetProductsCopy()
        {
            Product[] copyProducts = new Product[_products.Length];
            _products.CopyTo(copyProducts, 0);

            return copyProducts;
        }
    }
}