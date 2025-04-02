using Microsoft.EntityFrameworkCore;
using WebStore.Entities;

namespace WebStore.Assignments
{
    /// Additional tutorial materials https://dotnettutorials.net/lesson/linq-to-entities-in-entity-framework-core/

    /// <summary>
    /// This class demonstrates various LINQ query tasks 
    /// to practice querying an EF Core database.
    /// 
    /// ASSIGNMENT INSTRUCTIONS:
    ///   1. For each method labeled "TODO", write the necessary
    ///      LINQ query to return or display the required data.
    ///      
    ///   2. Print meaningful output to the console (or return
    ///      collections, as needed).
    ///      
    ///   3. Test each method by calling it from your Program.cs
    ///      or test harness.
    /// </summary>
    public class LinqQueriesAssignment
    {

        //TODO: Uncomment this code after generating the entity models

        private readonly Assignment3Context _dbContext;

        public LinqQueriesAssignment(Assignment3Context context)
        {
            _dbContext = context;
        }


        /// <summary>
        /// 1. List all customers in the database:
        ///    - Print each customer's full name (First + Last) and Email.
        /// </summary>
        public async Task Task01_ListAllCustomers()
        {
            // TODO: Write a LINQ query that fetches all customers
            //       and prints their names + emails to the console.
            // HINT: context.Customers
            
            var customers = await _dbContext.Customers
               // .AsNoTracking() // optional for read-only
               .ToListAsync();

            Console.WriteLine("=== TASK 01: List All Customers ===");

            foreach (var c in customers)
            {
                Console.WriteLine($"{c.FirstName} {c.LastName} - {c.Email}");
            }

            
        }

        /// <summary>
        /// 2. Fetch all orders along with:
        ///    - Customer Name
        ///    - Order ID
        ///    - Order Status
        ///    - Number of items in each order (the sum of OrderItems.Quantity)
        /// </summary>
        public async Task Task02_ListOrdersWithItemCount()
        {
            // Fetch all orders with related customer and order items
            var orders = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Select(o => new
            {
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
                o.OrderId,
                ItemCount = o.OrderItems.Sum(oi => oi.Quantity)
            })
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== TASK 02: List Orders With Item Count ===");

            foreach (var order in orders)
            {
            Console.WriteLine($"Order ID: {order.OrderId}, Customer: {order.CustomerName}, Item Count: {order.ItemCount}");
            }
        }

        /// <summary>
        /// 3. List all products (ProductName, Price),
        ///    sorted by price descending (highest first).
        /// </summary>
        public async Task Task03_ListProductsByDescendingPrice()
        {
            // Fetch all products and sort them by descending price
            var products = await _dbContext.Products
            .OrderByDescending(p => p.Price)
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 03: List Products By Descending Price ===");

            foreach (var product in products)
            {
            Console.WriteLine($"Product: {product.ProductName}, Price: {product.Price:C}");
            }
        }

        /// <summary>
        /// 4. Find all "Pending" orders (order status = "Pending")
        ///    and display:
        ///      - Customer Name
        ///      - Order ID
        ///      - Order Date
        ///      - Total price (sum of unit_price * quantity - discount) for each order
        /// </summary>
        public async Task Task04_ListPendingOrdersWithTotalPrice()
        {
            // Fetch all pending orders and calculate their total price
            var pendingOrders = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Select(o => new
            {
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
                o.OrderId,
                o.OrderDate,
                TotalPrice = o.OrderItems.Sum(oi => (oi.UnitPrice * oi.Quantity) - oi.Discount)
            })
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 04: List Pending Orders With Total Price ===");

            foreach (var order in pendingOrders)
            {
            Console.WriteLine($"Order ID: {order.OrderId}, Customer: {order.CustomerName}, Date: {order.OrderDate}, Total Price: {order.TotalPrice:C}");
            }
        }

        /// <summary>
        /// 5. List the total number of orders each customer has placed.
        ///    Output should show:
        ///      - Customer Full Name
        ///      - Number of Orders
        /// </summary>
        public async Task Task05_OrderCountPerCustomer()
        {
            // Group orders by customer and count the number of orders for each
            var customerOrderCounts = await _dbContext.Orders
            .GroupBy(o => new { o.Customer.FirstName, o.Customer.LastName })
            .Select(g => new
            {
                CustomerName = $"{g.Key.FirstName} {g.Key.LastName}",
                OrderCount = g.Count()
            })
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 05: Order Count Per Customer ===");

            foreach (var customer in customerOrderCounts)
            {
            Console.WriteLine($"Customer: {customer.CustomerName}, Orders: {customer.OrderCount}");
            }
        }

        /// <summary>
        /// 6. Show the top 3 customers who have placed the highest total order value overall.
        ///    - For each customer, calculate SUM of (OrderItems * Price).
        ///      Then pick the top 3.
        /// </summary>
        public async Task Task06_Top3CustomersByOrderValue()
        {
            // Calculate each customer's total order value
            var topCustomers = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .GroupBy(o => new { o.Customer.FirstName, o.Customer.LastName })
            .Select(g => new
            {
                CustomerName = $"{g.Key.FirstName} {g.Key.LastName}",
                TotalOrderValue = g.Sum(o => o.OrderItems.Sum(oi => (oi.UnitPrice * oi.Quantity) - oi.Discount))
            })
            .OrderByDescending(c => c.TotalOrderValue)
            .Take(3)
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 06: Top 3 Customers By Order Value ===");

            foreach (var customer in topCustomers)
            {
            Console.WriteLine($"Customer: {customer.CustomerName}, Total Order Value: {customer.TotalOrderValue:C}");
            }
        }

            /// <summary>
            /// 7. Show all orders placed in the last 30 days (relative to now).
            ///    - Display order ID, date, and customer name.
            /// </summary>
            public async Task Task07_RecentOrders()
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);

                var recentOrders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= thirtyDaysAgo)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}"
                })
                .ToListAsync();

                Console.WriteLine(" ");
                Console.WriteLine("=== Task 07: Recent Orders ===");

                foreach (var order in recentOrders)
                {
                Console.WriteLine($"Order ID: {order.OrderId}, Date: {order.OrderDate}, Customer: {order.CustomerName}");
                }
            }

                /// <summary>
                /// 8. For each product, display how many total items have been sold
                ///    across all orders.
                ///    - Product name, total sold quantity.
                ///    - Sort by total sold descending.
                /// </summary>
                public async Task Task08_TotalSoldPerProduct()
                {
                    // Group OrderItems by Product and calculate total quantity sold
                    var productSales = await _dbContext.OrderItems
                    .GroupBy(oi => oi.Product)
                    .Select(g => new
                    {
                        ProductName = g.Key.ProductName,
                        TotalSold = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(p => p.TotalSold)
                    .ToListAsync();

                    Console.WriteLine(" ");
                    Console.WriteLine("=== Task 08: Total Sold Per Product ===");

                    foreach (var product in productSales)
                    {
                    Console.WriteLine($"Product: {product.ProductName}, Total Sold: {product.TotalSold}");
                    }
                }

        /// <summary>
        /// 9. List any orders that have at least one OrderItem with a Discount > 0.
        ///    - Show Order ID, Customer name, and which products were discounted.
        /// </summary>
        public async Task Task09_DiscountedOrders()
        {
            // Identify orders with any OrderItem having (Discount > 0)
            var discountedOrders = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderItems.Any(oi => oi.Discount > 0))
            .Select(o => new
            {
                o.OrderId,
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
                DiscountedProducts = o.OrderItems
                .Where(oi => oi.Discount > 0)
                .Select(oi => new
                {
                    oi.Product.ProductName,
                    oi.Discount
                })
                .ToList()
            })
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 09: Discounted Orders ===");

            foreach (var order in discountedOrders)
            {
            Console.WriteLine($"Order ID: {order.OrderId}, Customer: {order.CustomerName}");
            foreach (var product in order.DiscountedProducts)
            {
                Console.WriteLine($"  - Product: {product.ProductName}, Discount: {product.Discount:C}");
            }
            }
        }

        /// <summary>
        /// 10. (Open-ended) Combine multiple joins or navigation properties
        ///     to retrieve a more complex set of data. For example:
        ///     - All orders that contain products in a certain category
        ///       (e.g., "Electronics"), including the store where each product
        ///       is stocked most. (Requires `Stocks`, `Store`, `ProductCategory`, etc.)
        /// </summary>
        public async Task Task10_AdvancedQueryExample()
        {
            // Filter products by category name "Electronics"
            var electronicsCategoryProducts = await _dbContext.Products
            .Where(p => p.Categories.Any(c => c.CategoryName == "Electronics"))
            .ToListAsync();

            // Find orders that contain these products
            var ordersWithElectronics = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderItems.Any(oi => electronicsCategoryProducts.Contains(oi.Product)))
            .Select(o => new
            {
                o.OrderId,
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
                Products = o.OrderItems
                .Where(oi => electronicsCategoryProducts.Contains(oi.Product))
                .Select(oi => new
                {
                    oi.Product.ProductName,
                    oi.Quantity
                })
                .ToList()
            })
            .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 10: Advanced Query Example ===");

            foreach (var order in ordersWithElectronics)
            {
            Console.WriteLine($"Order ID: {order.OrderId}, Customer: {order.CustomerName}");
            foreach (var product in order.Products)
            {
                Console.WriteLine($"  - Product: {product.ProductName}, Quantity: {product.Quantity}");
            }
            }
        }
    }
}
