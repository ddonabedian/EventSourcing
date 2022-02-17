using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Console
{
    public class Projection
    {
        private readonly ProductDbContext _dbContext;

        public Projection(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void ReceiveEvent(IEvent evnt)
        {
            switch (evnt)
            {
                case ProductShipped shippedProduct:
                    Apply(shippedProduct);
                    break;
                case ProductReceived productReceived:
                    Apply(productReceived);
                    break;
                default:
                    break;
            }
        }

        public Product GetProduct(string sku)
        {
            var product = _dbContext.Products.SingleOrDefault(x => x.Sku == sku);
            if (product == null)
            {
                product = new Product
                {
                    Sku = sku
                };
                _dbContext.Products.Add(product);
            }

            return product;
        }

        private void Apply(ProductShipped shippedProduct)
        {
            var product = GetProduct(shippedProduct.Sku);
            product.Shipped += shippedProduct.Quantity;
            _dbContext.SaveChanges();
        }
        private void Apply(ProductReceived productReceived)
        {
            var product = GetProduct(productReceived.Sku);
            product.Received += productReceived.Quantity;
            _dbContext.SaveChanges();
        }
    }
}
