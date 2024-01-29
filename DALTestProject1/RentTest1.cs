using NUnit.Framework;
using DAL;
using Moq;
using System.Data.Entity;
using System.Collections;

namespace DALTests
{
    [TestFixture]
    public class RentDALTests
    {
        private RentDAL rentDAL;
        private Mock<CarRentalEntities> mockContext;
        private Mock<DbSet<Rental>> mockRentalSet;
        private List<Rental> mockData;
        private Mock<CarDAL> mockCarDAL;

        [SetUp]
        public void SetUp()
        {
            mockContext = new Mock<CarRentalEntities>();
            mockRentalSet = new Mock<DbSet<Rental>>();

            mockData = new List<Rental>
            {
                new Rental { RentID = 1 },
                new Rental { RentID = 2 },
                new Rental { RentID = 3 }
            };

            mockRentalSet.As<IQueryable<Rental>>().Setup(m => m.Provider).Returns(mockData.AsQueryable().Provider);
            mockRentalSet.As<IQueryable<Rental>>().Setup(m => m.Expression).Returns(mockData.AsQueryable().Expression);
            mockRentalSet.As<IQueryable<Rental>>().Setup(m => m.ElementType).Returns(mockData.AsQueryable().ElementType);
            mockRentalSet.As<IQueryable<Rental>>().Setup(m => m.GetEnumerator()).Returns(mockData.GetEnumerator());

            rentDAL = new RentDAL();
            mockRentalSet = new Mock<DbSet<Rental>>();
            mockContext = new Mock<CarRentalEntities>();

            mockContext.Setup(m => m.Rentals).Returns(mockRentalSet.Object);
            var contextFieldInfo = rentDAL.GetType().GetField("context",
                                  System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);
            contextFieldInfo.SetValue(rentDAL, mockContext.Object);
            /*rentDAL.context = mockContext.Object;*/
        }
        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> items) where T : class
        {
            var queryable = items.AsQueryable();

            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Add these lines to handle the non-generic GetEnumerator method
            dbSetMock.As<IEnumerable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSetMock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            dbSetMock.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(items.Add);

            return dbSetMock;
        }


        [Test]
        public void RentCar_ValidRental_ReturnsTrue()
        {
            var rental = new Rental { /* initialize properties */ };

            bool result = rentDAL.RentCar(rental);

            mockRentalSet.Verify(m => m.Add(It.IsAny<Rental>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsTrue(result);
        }

        [Test]
        public void RentCar_NullRental_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => rentDAL.RentCar(null));
        }

        [Test]
        public void RentCar_ErrorDuringSave_ThrowsApplicationException()
        {
            var rental = new Rental();

            mockRentalSet.Setup(m => m.Add(It.IsAny<Rental>())).Returns(rental);
            mockContext.Setup(m => m.SaveChanges()).Throws<Exception>();

            Assert.Throws<ApplicationException>(() => rentDAL.RentCar(rental));
        }

       

        [Test]
        public void ReturnCar_ExistingRental_UpdatesRentalDetails()
        {
            var id = 1;
            var existingRental = new Rental { RentID = id };
            var updatedDetails = new Rental
            {
                CarID = 2,
                CustomerID = 2,
                RentOrderDate = DateTime.Now.AddDays(-3),
                ReturnDate = DateTime.Now,
                OdoReading = 500,
                ReturnOdoReading = 1000,
                LicenseNumber = "ABC123"
            };

            mockRentalSet.Setup(m => m.Find(id)).Returns(existingRental);

            rentDAL.ReturnCar(id, updatedDetails);

            Assert.AreEqual(existingRental.CarID, updatedDetails.CarID);
            Assert.AreEqual(existingRental.CustomerID, updatedDetails.CustomerID);
            Assert.AreEqual(existingRental.RentOrderDate, updatedDetails.RentOrderDate);
            // ... and so on for other properties ...

            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        [Test]
        public void ReturnCar_NonExistingRental_ThrowsArgumentException()
        {
            var id = 1;
            mockRentalSet.Setup(m => m.Find(id)).Returns((Rental)null);

            var rentalDetails = new Rental();
            Assert.Throws<ArgumentException>(() => rentDAL.ReturnCar(id, rentalDetails));
        }

        [Test]
        public void ReturnCar_ErrorDuringUpdate_ThrowsApplicationException()
        {
            var id = 1;
            var existingRental = new Rental { RentID = id };
            var updatedDetails = new Rental();

            mockRentalSet.Setup(m => m.Find(id)).Returns(existingRental);
            mockContext.Setup(m => m.SaveChanges()).Throws<Exception>();

            Assert.Throws<ApplicationException>(() => rentDAL.ReturnCar(id, updatedDetails));
        }

        [Test]
        public void CancelRent_ExistingRent_RentRemoved()
        {
            var rentId = 1;
            var rental = new Rental { RentID = rentId };
            mockRentalSet.Setup(m => m.Find(rentId)).Returns(rental);

            rentDAL.CancelRent(rentId);

            mockRentalSet.Verify(m => m.Remove(rental), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        [Test]
        public void CancelRent_NonExistingRent_ThrowsArgumentException()
        {
            var rentId = 5;
            mockRentalSet.Setup(m => m.Find(rentId)).Returns((Rental)null);

            Assert.Throws<ArgumentException>(() => rentDAL.CancelRent(rentId));
        }             

       

        [Test]
        public void CalculateCharges_NullRental_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => rentDAL.CalculateCharges(null));
        }

        
    }

}

