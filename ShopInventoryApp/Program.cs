using System;
using System.Data.SqlClient;

namespace ShopInventoryApp
{
    class Program
    {
        //IMPORTANT:
        // Update this with your SQL Server instance name; after Data Source enter the instance name.
        static string connStr = "Data Source=localhost\\SQLSERVER2022;Initial Catalog=ShopInventory;Integrated Security=True";

        static void Main()
        {
            while (true)
            {
                Console.WriteLine("\n--- Shop Inventory Menu ---");
                Console.WriteLine("1. List Products");
                Console.WriteLine("2. Look up product");
                Console.WriteLine("3. Order a product");
                Console.WriteLine("4. Display current inventory value");
                Console.WriteLine("5. Show all products in a single store");
                Console.WriteLine("6. Show all stores that contain a certain product");
                Console.WriteLine("7. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ListProducts(); break;
                    case "2": LookUpProduct(); break;
                    case "3": OrderProduct(); break;
                    case "4": DisplayInventoryValue(); break;
                    case "5": ShowProductsInStore(); break;
                    case "6": ShowStoresForProduct(); break;
                    case "7": return;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        static void ListProducts()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM StoreProducts ORDER BY StoreNumber, ProductCode", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Console.WriteLine("\nStoreNumber | ProductCode | Price | Quantity");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["StoreNumber"],-11} {reader["ProductCode"],-12} {reader["Price"],-8} {reader["Quantity"],-8}");
                }
            }
        }


        static void LookUpProduct()
        {
            Console.Write("Enter product code: ");
            int code = int.Parse(Console.ReadLine());

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM StoreProducts WHERE ProductCode=@code ORDER BY StoreNumber, ProductCode", conn);

                cmd.Parameters.AddWithValue("@code", code);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine($"Store: {reader["StoreNumber"]}, Product: {reader["ProductCode"]}, Price: {reader["Price"]}, Quantity: {reader["Quantity"]}");
                }
                else
                {
                    Console.WriteLine("Product not found.");
                }
            }
        }

        static void OrderProduct()
        {
            Console.Write("Enter store number: ");
            int store = int.Parse(Console.ReadLine());
            Console.Write("Enter product code: ");
            int code = int.Parse(Console.ReadLine());
            Console.Write("Enter quantity to order: ");
            int qty = int.Parse(Console.ReadLine());

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand checkCmd = new SqlCommand("SELECT * FROM StoreProducts WHERE StoreNumber=@store AND ProductCode=@code", conn);
                checkCmd.Parameters.AddWithValue("@store", store);
                checkCmd.Parameters.AddWithValue("@code", code);

                SqlDataReader reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    int currentQty = (int)reader["Quantity"];
                    reader.Close();

                    SqlCommand updateCmd = new SqlCommand("UPDATE StoreProducts SET Quantity=@qty WHERE StoreNumber=@store AND ProductCode=@code", conn);
                    updateCmd.Parameters.AddWithValue("@qty", currentQty + qty);
                    updateCmd.Parameters.AddWithValue("@store", store);
                    updateCmd.Parameters.AddWithValue("@code", code);
                    updateCmd.ExecuteNonQuery();

                    Console.WriteLine("Quantity updated.");
                }
                else
                {
                    reader.Close();
                    Console.Write("Enter price for new product: ");
                    decimal price = decimal.Parse(Console.ReadLine());

                    SqlCommand insertCmd = new SqlCommand("INSERT INTO StoreProducts (StoreNumber, ProductCode, Price, Quantity) VALUES (@store, @code, @price, @qty)", conn);
                    insertCmd.Parameters.AddWithValue("@store", store);
                    insertCmd.Parameters.AddWithValue("@code", code);
                    insertCmd.Parameters.AddWithValue("@price", price);
                    insertCmd.Parameters.AddWithValue("@qty", qty);
                    insertCmd.ExecuteNonQuery();

                    Console.WriteLine("New product added.");
                }
            }
        }

        static void DisplayInventoryValue()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT SUM(Price * Quantity) AS TotalValue FROM StoreProducts", conn);
                object result = cmd.ExecuteScalar();
                Console.WriteLine($"Current Inventory Value: {Convert.ToDecimal(result):C}");
            }
        }

        static void ShowProductsInStore()
        {
            Console.Write("Enter store number: ");
            int store = int.Parse(Console.ReadLine());

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM StoreProducts WHERE StoreNumber=@store", conn);
                cmd.Parameters.AddWithValue("@store", store);
                SqlDataReader reader = cmd.ExecuteReader();
                Console.WriteLine("\nStoreNumber | ProductCode | Price | Quantity");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["StoreNumber"],-11} {reader["ProductCode"],-12} {reader["Price"],-8} {reader["Quantity"],-8}");
                }
            }
        }

        static void ShowStoresForProduct()
        {
            Console.Write("Enter product code: ");
            int code = int.Parse(Console.ReadLine());

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT StoreNumber FROM StoreProducts WHERE ProductCode=@code", conn);
                cmd.Parameters.AddWithValue("@code", code);
                SqlDataReader reader = cmd.ExecuteReader();
                Console.WriteLine("\nStores containing product:");
                while (reader.Read())
                {
                    Console.WriteLine($"Store: {reader["StoreNumber"]}");
                }
            }
        }
    }
}
