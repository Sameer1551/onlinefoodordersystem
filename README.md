# Online Food Ordering System

An enterprise-ready **Online Food Ordering System** built with **ASP.NET Core 8.0 MVC**. This full-stack web application bridges the gap between hungry customers and local restaurants by providing a seamless, robust platform for ordering food online and managing restaurant operations.

## 🚀 Features

### For Customers (Public Portal)
* **Secure Authentication:** User registration and login powered by ASP.NET Core Identity.
* **Browse & Discover:** Explore a variety of restaurants, menus, and categorized food items.
* **Shopping Cart & Checkout:** Add items to your cart, apply discount coupons, and securely place orders.
* **Wishlist:** Save your favorite food items for quick access later.
* **Order Tracking:** Track the real-time status of your active orders.
* **Address Management:** Easily manage multiple delivery addresses within your profile.

### For Administrators (Admin Area)
* **Central Dashboard:** Monitor recent orders, overall system health, and key metrics.
* **Catalog Management:** Full CRUD (Create, Read, Update, Delete) operations for restaurants, food items, and categories.
* **Order Fulfillment:** Manage, track, and update the status of all incoming orders.
* **User Management:** Oversee user accounts and access roles.

## 🛠️ Technology Stack
* **Framework:** ASP.NET Core 8.0 MVC
* **Language:** C#
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **Authentication:** ASP.NET Core Identity
* **Frontend:** HTML5, CSS3, JavaScript, Razor Views (.cshtml), Bootstrap

## 🏗️ Architecture
The system follows the **Model-View-Controller (MVC)** architectural pattern to ensure clean separation of concerns:
* **Models:** Define the database schema (Entities like `Order`, `FoodItem`, `Restaurant`).
* **Views:** Render the user interface (using Razor syntax).
* **Controllers:** Handle user input and business logic, leveraging Dependency Injection (DI) for integrating services like `IOrderService` and `ICartService`.

## ⚙️ Setup & Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Sameer1551/onlinefoodordersystem.git
   cd onlinefoodordersystem
   ```

2. **Configure Database Connection**
   Update the `DefaultConnection` string in `appsettings.json` to point to your local or remote SQL Server instance.

3. **Run Migrations**
   To create the database tables, open the Package Manager Console or your terminal and run:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

## 🔮 Future Enhancements
- Integration with third-party payment gateways (Stripe, PayPal).
- Real-time notifications and order tracking using SignalR.
- Dedicated REST API for mobile app support.
