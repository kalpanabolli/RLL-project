using DAL;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace DALTests
{
    [TestFixture]
    public class CustomerDALTests
    {
        private Mock<CarRentalEntities> _mockContext;
        private Mock<DbSet<Customer>> _mockDbSet;
        private CustomerDAL _dal;
        private List<Customer> data;

        [SetUp]
        public void Setup()
        {
            List<Customer> data = new List<Customer> { new Customer(), new Customer() };

            _mockDbSet = new Mock<DbSet<Customer>>();
            _mockDbSet.As<IDbSet<Customer>>().Setup(m => m.Add(It.IsAny<Customer>())).Returns((Customer c) =>
            {
                data.Add(c);
                return c;
            });

            // Setting up IQueryable properties
            _mockDbSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            _mockDbSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            _mockDbSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            _mockDbSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext = new Mock<CarRentalEntities>();
            _mockContext.Setup(c => c.Customers).Returns(_mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Callback(() => { });

            _dal = new CustomerDAL();
            _dal.context = _mockContext.Object;
        }
        [Test]
        public void GetCustomers_ReturnsAllCustomers()
        {
            var allCustomers = _dal.GetCustomers();
            Assert.AreEqual(2, allCustomers.Count);
        }


        [Test]
        public void GetCustomer_ReturnsCustomer()
        {
            var testCustomer = new Customer { CustomerID = 1, CustomerName = "John Doe" };

            _mockDbSet.Setup(m => m.Find(1)).Returns(testCustomer);

            var result = _dal.GetCustomer(1);

            Assert.AreEqual(testCustomer, result);
        }

        [Test]
        public void AddCustomer_AddsSuccessfully()
        {
            // Arrange
            var newCustomer = new Customer
            {
                CustomerName = "Test Name",
                Email = "test@email.com",
                Password = "TestPassword123",  // Setting the password to a valid value
                LoyaltyPoints = 0
            };

            // Act
            var result = _dal.AddCustomer(newCustomer);

            // Assert
            Assert.IsTrue(result);
            _mockDbSet.Verify(m => m.Add(It.IsAny<Customer>()), Times.Once());
            _mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
        [Test]
        public void UpdateCustomer_UpdatesSuccessfully()
        {
            var existingCustomer = new Customer
            {
                CustomerID = 1,
                CustomerName = "John Doe",
                Email = "john.doe@example.com",
                LoyaltyPoints = 10,
                Password = "samplePassword"  // added password
            };

            var updatedData = new Customer
            {
                CustomerID = 1,
                CustomerName = "John D.",
                Email = "john.d@example.com",
                LoyaltyPoints = 15,
                Password = "newPassword"  // added password
            };

            _mockDbSet.Setup(m => m.Find(1)).Returns(existingCustomer);

            _dal.UpdateCustomer(1, updatedData);

            Assert.AreEqual(updatedData.CustomerName, existingCustomer.CustomerName);
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }


        [Test]
        public void DeleteCustomer_DeletesSuccessfully()
        {
            var customerToDelete = new Customer { CustomerID = 1 };

            _mockDbSet.Setup(m => m.Find(1)).Returns(customerToDelete);

            _dal.DeleteCustomer(1);

            _mockDbSet.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Once);
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Test]
        public void AddLoyalty_AddsLoyaltyPointsSuccessfully()
        {
            var customer = new Customer { CustomerID = 1, LoyaltyPoints = 10 };

            _mockDbSet.Setup(m => m.Find(1)).Returns(customer);

            _dal.AddLoyalty(100, 1); // Adding 2 points

            Assert.AreEqual(12, customer.LoyaltyPoints);
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Test]
        public void MinusLoyalty_DeductsLoyaltyPointsSuccessfully()
        {
            var customer = new Customer { CustomerID = 1, LoyaltyPoints = 10 };

            _mockDbSet.Setup(m => m.Find(1)).Returns(customer);

            _dal.MinusLoyalty(1);

            Assert.AreEqual(10 - 25, customer.LoyaltyPoints);  // Deducting 25 points
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [TestCase("", "validEmail@example.com", "password123", 0, ExpectedResult = false)]
        [TestCase("Valid Name", "", "password123", 0, ExpectedResult = false)]
        [TestCase("Valid Name", "invalidEmail", "password123", 0, ExpectedResult = false)]
        [TestCase("Valid Name", "validEmail@example.com", "", 0, ExpectedResult = false)]
        [TestCase("Valid Name", "validEmail@example.com", "password123", -10, ExpectedResult = false)]
        public bool AddCustomer_ValidatesCorrectly(string name, string email, string password, int loyaltyPoints)
        {
            var customer = new Customer { CustomerName = name, Email = email, Password = password, LoyaltyPoints = loyaltyPoints };

            try
            {
                _dal.AddCustomer(customer);
                return true; // if it succeeds without any exception
            }
            catch
            {
                return false; // if any exception occurs
            }
        }

        [TestCase("test@example.com", ExpectedResult = true)]
        [TestCase("test.example", ExpectedResult = false)]
        public bool IsValidEmailTests(string email)
        {
            try
            {
                return (bool)_dal.GetType().GetMethod("IsValidEmail", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_dal, new object[] { email });
            }
            catch (Exception)
            {
                // Handle or throw an exception based on how you want to manage this situation.
                throw new InvalidOperationException("Failed to invoke IsValidEmail method.");
            }

        }
    }
}
