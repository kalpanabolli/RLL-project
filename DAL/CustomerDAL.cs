using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class CustomerDAL
    {
        
        public CarRentalEntities context { get; set; }

        public CustomerDAL()
        {
            context = new CarRentalEntities();
        }
        // Overloaded constructor that accepts an instance of CarRentalEntities. 
        // This will be used for testing.
        public CustomerDAL(CarRentalEntities dbContext)
        {
            context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public List<Customer> GetCustomers()
        {
            return context.Customers.ToList();
        }
        
        public Customer GetCustomer(int id)
        {
            var customer = context.Customers.Find(id);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {id} not found.");
            return customer;
        }

        public bool AddCustomer(Customer c)
        {
            ValidateCustomer(c);
            // Check if a customer with the same email already exists
            var existingCustomer = context.Customers.FirstOrDefault(customer => customer.Email == c.Email);
            if (existingCustomer != null)
            {
                throw new ApplicationException("A customer with this email address already exists.");
            }

            // Explicitly set CustomerID to 0, so it's clear we're not assigning a manual ID
            c.CustomerID = 0;

            try
            {
                context.Customers.Add(c);
                context.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // This exception will give details on which fields failed validation in EF
                var errorMessages = dbEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(dbEx.Message, " The validation errors are: ", fullErrorMessage);

                throw new ApplicationException(exceptionMessage, dbEx);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpEx)
            {               
                throw new ApplicationException("Error adding customer.", dbUpEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding customer.", ex);
            } 
        }


        public bool UpdateCustomer(int id, Customer c)
        {
            ValidateCustomer(c);

            try
            {
                Customer existingCustomer = context.Customers.Find(id);
                if (existingCustomer == null)
                    throw new ArgumentException($"Customer with ID {id} not found.");

                existingCustomer.CustomerName = c.CustomerName;
                existingCustomer.Email = c.Email;
                existingCustomer.LoyaltyPoints = c.LoyaltyPoints;
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating customer.", ex);
            }
        }

        public bool DeleteCustomer(int id)
        {
            try
            {
                Customer customerToDelete = context.Customers.Find(id);
                if (customerToDelete == null)
                    throw new ArgumentException($"Customer with ID {id} not found.");

                context.Customers.Remove(customerToDelete);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error deleting customer.", ex);
            }
        }

        public void AddLoyalty(int km, int id)
        {
            var customer = context.Customers.Find(id);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {id} not found.");

            customer.LoyaltyPoints += km / 50;
            context.SaveChanges();
        }

        public void MinusLoyalty(int id)  
        {
            var customer = context.Customers.Find(id);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {id} not found.");

            customer.LoyaltyPoints -= 10;
            context.SaveChanges(); 
        }

        private void ValidateCustomer(Customer customer)
        {
            // Validate Customer Name
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                throw new ArgumentException("Customer name cannot be empty or null.");
            if (customer.CustomerName.Length > 255)
                throw new ArgumentException("Customer name is too long.");

            // Validate Email
            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new ArgumentException("Email cannot be empty or null.");
            if (customer.Email.Length > 255)
                throw new ArgumentException("Email is too long.");
            if (!IsValidEmail(customer.Email))
                throw new ArgumentException("Invalid email format.");

            // Validate Password 
            if (string.IsNullOrWhiteSpace(customer.Password))
                throw new ArgumentException("Password cannot be empty or null.");
            

            // Validate Loyalty Points
            if (customer.LoyaltyPoints < 0)
                customer.LoyaltyPoints=0    ;
        }

        // Helper method to validate email format using regex
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
