using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.Domain.Entities;
using CleanCut.Infrastructure.Data.Context;
using System.Reflection;

namespace CleanCut.Infrastructure.Data.Seeding;

/// <summary>
/// Database seeder for development data
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CleanCutDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanCutDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Customers.AnyAsync())
            {
                logger.LogInformation("Database already seeded");
                return;
            }

            logger.LogInformation("Seeding database with comprehensive test data");

            // Seed Countries FIRST
            if (!await context.Countries.AnyAsync())
            {
                var countries = new List<Country>
                {
                    new Country("United States", "US"),
                    new Country("Canada", "CA"),
                    new Country("United Kingdom", "GB"),
                    new Country("Australia", "AU"),
                    new Country("Germany", "DE")
                };
                await context.Countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} countries", countries.Count);
            }

            // Seed Users (50 users for better pagination testing)
            var users = GenerateUsers();
            await context.Customers.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} users", users.Count);

            // Seed Products (200+ products for comprehensive pagination testing)
            var products = GenerateProducts(users);
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} products", products.Count);

            // Seed Orders (sample orders for testing)
            var orders = GenerateOrders(users, products);
            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} orders", orders.Count);

            logger.LogInformation("Database seeded successfully with {UserCount} users, {ProductCount} products, and {OrderCount} orders", 
                users.Count, products.Count, orders.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static List<Customer> GenerateUsers()
    {
        var users = new List<Customer>();

        // Add the original users with known GUIDs for testing
        var originalUser = new Customer("John", "Doe", "john.doe@example.com");
        SetEntityId(originalUser, Guid.Parse("11111111-1111-1111-1111-111111111111"));
        users.Add(originalUser);

        users.Add(new Customer("Jane", "Smith", "jane.smith@example.com"));
        users.Add(new Customer("Bob", "Johnson", "bob.johnson@example.com"));

        // Add comprehensive user data
        var userTemplates = new[]
        {
            ("Alice", "Anderson", "alice.anderson@techcorp.com"),
            ("Michael", "Brown", "michael.brown@innovate.com"),
            ("Sarah", "Davis", "sarah.davis@startup.io"),
            ("David", "Wilson", "david.wilson@enterprise.com"),
            ("Emily", "Miller", "emily.miller@consulting.com"),
            ("James", "Moore", "james.moore@development.org"),
            ("Jessica", "Taylor", "jessica.taylor@design.studio"),
            ("Christopher", "Martinez", "chris.martinez@tech.solutions"),
            ("Ashley", "Garcia", "ashley.garcia@digital.agency"),
            ("Matthew", "Rodriguez", "matt.rodriguez@software.inc"),
            ("Amanda", "Lewis", "amanda.lewis@creative.works"),
            ("Joshua", "Lee", "joshua.lee@innovation.lab"),
            ("Stephanie", "Walker", "stephanie.walker@product.hub"),
            ("Andrew", "Hall", "andrew.hall@development.team"),
            ("Nicole", "Allen", "nicole.allen@marketing.pro"),
            ("Ryan", "Young", "ryan.young@business.solutions"),
            ("Lauren", "Hernandez", "lauren.hernandez@ecommerce.store"),
            ("Kevin", "King", "kevin.king@logistics.company"),
            ("Michelle", "Wright", "michelle.wright@healthcare.system"),
            ("Brandon", "Lopez", "brandon.lopez@financial.services"),
            ("Megan", "Hill", "megan.hill@education.platform"),
            ("Tyler", "Scott", "tyler.scott@manufacturing.corp"),
            ("Rachel", "Green", "rachel.green@retail.chain"),
            ("Jason", "Adams", "jason.adams@transportation.inc"),
            ("Jennifer", "Baker", "jennifer.baker@hospitality.group"),
            ("Daniel", "Gonzalez", "daniel.gonzalez@media.company"),
            ("Samantha", "Nelson", "samantha.nelson@publishing.house"),
            ("Jonathan", "Carter", "jonathan.carter@consulting.firm"),
            ("Katherine", "Mitchell", "katherine.mitchell@research.institute"),
            ("Nicholas", "Perez", "nicholas.perez@technology.ventures"),
            ("Angela", "Roberts", "angela.roberts@design.collective"),
            ("Benjamin", "Turner", "benjamin.turner@software.factory"),
            ("Rebecca", "Phillips", "rebecca.phillips@data.analytics"),
            ("Anthony", "Campbell", "anthony.campbell@cloud.services"),
            ("Brittany", "Parker", "brittany.parker@mobile.apps"),
            ("Jacob", "Evans", "jacob.evans@web.development"),
            ("Melissa", "Edwards", "melissa.edwards@digital.marketing"),
            ("William", "Collins", "william.collins@artificial.intelligence"),
            ("Heather", "Stewart", "heather.stewart@cybersecurity.solutions"),
            ("Alexander", "Sanchez", "alexander.sanchez@blockchain.tech"),
            ("Crystal", "Morris", "crystal.morris@machine.learning"),
            ("Joseph", "Rogers", "joseph.rogers@quantum.computing"),
            ("Vanessa", "Reed", "vanessa.reed@biotech.research"),
            ("Aaron", "Cook", "aaron.cook@renewable.energy"),
            ("Lisa", "Morgan", "lisa.morgan@space.technology"),
            ("Robert", "Bell", "robert.bell@robotics.engineering"),
            ("Maria", "Murphy", "maria.murphy@virtual.reality")
        };

        foreach (var (firstName, lastName, email) in userTemplates)
        {
            try
            {
                users.Add(new Customer(firstName, lastName, email));
            }
            catch (Exception ex)
            {
                // Skip invalid users but log the issue
                Console.WriteLine($"Skipping user {firstName} {lastName}: {ex.Message}");
            }
        }

        return users;
    }

    private static List<Product> GenerateProducts(List<Customer> users)
    {
        var products = new List<Product>();
        var random = new Random(42); // Fixed seed for consistent test data

        // Add the original product with known GUID for testing
        var originalProduct = new Product("Gaming Laptop", "High-performance gaming laptop with RTX graphics", 1299.99m, users[0].Id);
        SetEntityId(originalProduct, Guid.Parse("b6cc1306-75ea-45f0-902e-6cdf34260651"));
        products.Add(originalProduct);

        // Technology Products
        var techProducts = new[]
        {
            ("Wireless Mouse", "Ergonomic wireless mouse with precision tracking", 49.99m),
            ("Mechanical Keyboard", "RGB mechanical keyboard with blue switches", 129.99m),
            ("4K Monitor", "27-inch 4K UHD monitor with HDR support", 399.99m),
            ("Desk Chair", "Comfortable ergonomic office chair", 249.99m),
            ("Webcam", "1080p HD webcam with auto-focus", 89.99m),
            ("Headphones", "Noise-cancelling wireless headphones", 199.99m),
            ("Microphone", "Professional USB microphone for streaming", 119.99m),
            ("Graphics Card", "High-end graphics card for gaming and AI", 899.99m),
            ("SSD Drive", "1TB NVMe SSD with high-speed data transfer", 149.99m),
            ("RAM Module", "32GB DDR4 RAM for high-performance computing", 179.99m),
            ("CPU Processor", "Latest generation multi-core processor", 449.99m),
            ("Motherboard", "Gaming motherboard with RGB lighting", 299.99m),
            ("Power Supply", "80+ Gold certified modular power supply", 159.99m),
            ("Case Fan", "Silent RGB case fan with PWM control", 29.99m),
            ("Thermal Paste", "High-performance thermal compound", 12.99m),
            ("USB Hub", "7-port USB 3.0 hub with fast charging", 39.99m),
            ("External HDD", "2TB portable external hard drive", 79.99m),
            ("Network Card", "Gigabit Ethernet PCI network adapter", 24.99m),
            ("Sound Card", "Professional audio interface for music production", 189.99m),
            ("Optical Drive", "Blu-ray writer with M-DISC support", 69.99m)
        };

        // Office & Productivity
        var officeProducts = new[]
        {
            ("Standing Desk", "Height-adjustable standing desk", 349.99m),
            ("Monitor Arm", "Dual monitor mounting arm", 89.99m),
            ("Desk Lamp", "LED desk lamp with wireless charging base", 79.99m),
            ("Wireless Charger", "Fast wireless charging pad", 34.99m),
            ("Document Camera", "HD document camera for presentations", 159.99m),
            ("Label Printer", "Thermal label printer for shipping", 119.99m),
            ("Barcode Scanner", "Wireless 2D barcode scanner", 89.99m),
            ("Digital Whiteboard", "Interactive digital whiteboard", 899.99m),
            ("Conference Camera", "360-degree conference room camera", 299.99m),
            ("Presentation Remote", "Wireless presentation remote with laser pointer", 29.99m)
        };

        // Smart Home & IoT
        var smartHomeProducts = new[]
        {
            ("Smart Speaker", "Voice-controlled smart speaker with AI assistant", 99.99m),
            ("Smart Thermostat", "WiFi-enabled programmable thermostat", 199.99m),
            ("Security Camera", "Wireless outdoor security camera with night vision", 149.99m),
            ("Smart Doorbell", "Video doorbell with motion detection", 179.99m),
            ("Smart Light Bulb", "Color-changing WiFi smart bulb", 24.99m),
            ("Smart Switch", "WiFi smart wall switch", 34.99m),
            ("Smart Outlet", "WiFi smart plug with energy monitoring", 19.99m),
            ("Motion Sensor", "Wireless motion sensor for automation", 29.99m),
            ("Smart Lock", "Keyless entry smart door lock", 229.99m),
            ("Air Quality Monitor", "Smart air quality sensor", 119.99m)
        };

        // Gaming & Entertainment
        var gamingProducts = new[]
        {
            ("Gaming Chair", "Racing-style gaming chair with RGB lighting", 299.99m),
            ("Gaming Headset", "7.1 surround sound gaming headset", 149.99m),
            ("Mechanical Gaming Keyboard", "Compact mechanical keyboard for gaming", 159.99m),
            ("Gaming Mouse Pad", "Extended RGB gaming mouse pad", 39.99m),
            ("Streaming Deck", "Customizable macro keyboard for streamers", 149.99m),
            ("Capture Card", "4K HDR capture card for streaming", 199.99m),
            ("Green Screen", "Collapsible green screen for streaming", 89.99m),
            ("Ring Light", "LED ring light for video calls and streaming", 59.99m),
            ("Controller", "Wireless gaming controller", 69.99m),
            ("VR Headset", "Virtual reality headset with controllers", 399.99m)
        };

        // Mobile & Accessories
        var mobileProducts = new[]
        {
            ("Phone Case", "Protective case for latest smartphone", 24.99m),
            ("Screen Protector", "Tempered glass screen protector", 14.99m),
            ("Car Mount", "Magnetic car phone mount", 19.99m),
            ("Portable Battery", "20,000mAh portable power bank", 49.99m),
            ("Wireless Earbuds", "True wireless earbuds with noise cancellation", 179.99m),
            ("Phone Stand", "Adjustable desktop phone stand", 16.99m),
            ("Bluetooth Speaker", "Waterproof portable Bluetooth speaker", 79.99m),
            ("Car Charger", "Fast charging dual-port car charger", 22.99m),
            ("Tablet Stand", "Adjustable tablet and iPad stand", 34.99m),
            ("Stylus Pen", "Precision stylus for tablets and phones", 29.99m)
        };

        // Networking & Communication
        var networkProducts = new[]
        {
            ("WiFi Router", "Dual-band WiFi 6 router", 129.99m),
            ("Mesh System", "Whole-home mesh WiFi system", 299.99m),
            ("Network Switch", "8-port gigabit Ethernet switch", 49.99m),
            ("WiFi Extender", "WiFi range extender and booster", 39.99m),
            ("Modem", "DOCSIS 3.1 cable modem", 89.99m),
            ("Ethernet Cable", "Cat 8 Ethernet cable 10ft", 19.99m),
            ("Powerline Adapter", "Gigabit powerline network adapter kit", 69.99m),
            ("Access Point", "Business-grade wireless access point", 159.99m),
            ("Network Analyzer", "WiFi analyzer and spectrum scanner", 299.99m),
            ("Firewall", "Next-generation security firewall", 499.99m)
        };

        // Software & Digital Products
        var softwareProducts = new[]
        {
            ("Antivirus Software", "Premium antivirus and security suite", 59.99m),
            ("Photo Editor", "Professional photo editing software", 99.99m),
            ("Video Editor", "4K video editing and production software", 199.99m),
            ("Office Suite", "Complete productivity office suite", 149.99m),
            ("Project Manager", "Professional project management software", 79.99m),
            ("Database Software", "Enterprise database management system", 299.99m),
            ("Design Software", "Vector graphics and design software", 239.99m),
            ("CAD Software", "3D computer-aided design software", 399.99m),
            ("Audio Editor", "Professional audio editing and mastering", 129.99m),
            ("Virtual Machine", "Desktop virtualization software", 79.99m)
        };

        // Combine all product categories
        var allProductTemplates = techProducts
            .Concat(officeProducts)
            .Concat(smartHomeProducts)
            .Concat(gamingProducts)
            .Concat(mobileProducts)
            .Concat(networkProducts)
            .Concat(softwareProducts)
            .ToArray();

        // Generate products with random users and slight price variations
        foreach (var (name, description, basePrice) in allProductTemplates)
        {
            // Create 2-4 instances of each product with different users and slight price variations
            var instanceCount = random.Next(2, 5);
            
            for (int i = 0; i < instanceCount; i++)
            {
                var randomUser = users[random.Next(users.Count)];
                var priceVariation = 1.0m + (decimal)(random.NextDouble() * 0.4 - 0.2); // ±20% price variation
                var adjustedPrice = Math.Round(basePrice * priceVariation, 2);
                
                var productName = instanceCount > 1 ? $"{name} {GetProductVariant(i)}" : name;
                var isAvailable = random.NextDouble() > 0.15; // 85% of products are available
                
                try
                {
                    var product = new Product(productName, description, adjustedPrice, randomUser.Id);
                    if (!isAvailable)
                    {
                        product.MakeUnavailable();
                    }
                    products.Add(product);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Skipping product {productName}: {ex.Message}");
                }
            }
        }

        return products;
    }

    private static List<Order> GenerateOrders(List<Customer> customers, List<Product> products)
    {
        var orders = new List<Order>();
        var random = new Random(42); // Fixed seed for consistent test data

        // Create sample orders for testing
        var orderCount = Math.Min(customers.Count, 20); // Create up to 20 sample orders

        for (int i = 0; i < orderCount; i++)
        {
            var customer = customers[i];
            var shippingAddress = GenerateAddress(random);
            var billingAddress = random.NextDouble() > 0.3 ? shippingAddress : GenerateAddress(random); // 70% same address

            var notes = random.NextDouble() > 0.6 ? GenerateOrderNotes(random) : null; // 40% have notes

            try
            {
                var order = new Order(customer.Id, shippingAddress, billingAddress, notes);

                // Add random products to the order (1-5 items)
                var itemCount = random.Next(1, 6);
                var availableProducts = products.Where(p => p.IsAvailable).ToList();

                for (int j = 0; j < itemCount && availableProducts.Count > 0; j++)
                {
                    var productIndex = random.Next(availableProducts.Count);
                    var product = availableProducts[productIndex];
                    var quantity = random.Next(1, 4); // 1-3 quantity

                    order.AddLineItem(product.Id, product.Name, product.Price, quantity);
                    
                    // Remove product from available list to avoid duplicates in same order
                    availableProducts.RemoveAt(productIndex);
                }

                // Randomly set order status
                var statusRoll = random.NextDouble();
                if (statusRoll > 0.8) // 20% confirmed
                {
                    order.ConfirmOrder();
                    if (statusRoll > 0.9) // 10% shipped
                    {
                        order.ShipOrder();
                        if (statusRoll > 0.95) // 5% delivered
                        {
                            order.DeliverOrder();
                        }
                    }
                }
                else if (statusRoll < 0.05) // 5% cancelled
                {
                    order.CancelOrder();
                }

                orders.Add(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Skipping order for customer {customer.GetFullName()}: {ex.Message}");
            }
        }

        return orders;
    }

    private static string GenerateAddress(Random random)
    {
        var streetNumbers = new[] { "123", "456", "789", "321", "654", "987", "147", "258", "369" };
        var streetNames = new[] { "Main St", "Oak Ave", "Pine Rd", "Elm St", "Cedar Ln", "Maple Dr", "First Ave", "Second St", "Park Blvd" };
        var cities = new[] { "Springfield", "Riverside", "Franklin", "Georgetown", "Madison", "Salem", "Bristol", "Clinton", "Fairview" };
        var states = new[] { "CA", "NY", "TX", "FL", "IL", "PA", "OH", "GA", "NC", "MI" };

        var streetNumber = streetNumbers[random.Next(streetNumbers.Length)];
        var streetName = streetNames[random.Next(streetNames.Length)];
        var city = cities[random.Next(cities.Length)];
        var state = states[random.Next(states.Length)];
        var zipCode = random.Next(10000, 99999).ToString();

        return $"{streetNumber} {streetName}, {city}, {state} {zipCode}";
    }

    private static string GenerateOrderNotes(Random random)
    {
        var notes = new[]
        {
            "Please deliver to the back door",
            "Leave package with neighbor if not home",
            "Fragile items - handle with care",
            "Gift wrapping requested",
            "Rush delivery needed",
            "Customer requested expedited shipping",
            "Special handling instructions attached",
            "Delivery during business hours only"
        };

        return notes[random.Next(notes.Length)];
    }

    private static string GetProductVariant(int index)
    {
        return index switch
        {
            0 => "Standard",
            1 => "Pro",
            2 => "Premium",
            3 => "Elite",
            _ => $"V{index + 1}"
        };
    }

    // Helper method to set specific GUID for test entities using reflection
    private static void SetEntityId(object entity, Guid id)
    {
        var entityType = entity.GetType();
        var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        
        if (idProperty != null)
        {
            // Get the backing field for the protected setter
            var idField = entityType.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (idField != null)
            {
                idField.SetValue(entity, id);
            }
            else
            {
                // Fallback: try to set via property if it has a setter
                try
                {
                    idProperty.SetValue(entity, id);
                }
                catch
                {
                    // If we can't set the ID, just let it use the auto-generated one
                    Console.WriteLine($"Could not set specific ID for entity type {entityType.Name}");
                }
            }
        }
    }
}