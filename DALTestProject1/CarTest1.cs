using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DAL;

[TestFixture]
public class CarDALTests
{
    private Mock<CarRentalEntities> _contextMock;
    private Mock<DbSet<Car>> _carSetMock;
    private CarDAL _carDal;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<CarRentalEntities>();
        _carSetMock = new Mock<DbSet<Car>>();
        _contextMock.Setup(c => c.Cars).Returns(_carSetMock.Object);

        _carDal = new CarDAL(_contextMock.Object);
    }

    [Test]
    public void AddCar_ValidCar_AddsToDatabase()
    {
        var car = new Car();

        _carDal.addcar(car);

        _carSetMock.Verify(m => m.Add(It.IsAny<Car>()), Times.Once);
        _contextMock.Verify(m => m.SaveChanges(), Times.Once);
    }

    [Test]
    public void GetCar_ReturnsAllCars()
    {
        var car1 = new Car { CarID = 1 };
        var car2 = new Car { CarID = 2 };
        var carsData = new List<Car> { car1, car2 }.AsQueryable();

        var mockSet = new Mock<DbSet<Car>>();
        mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(carsData.Provider);
        mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(carsData.Expression);
        mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(carsData.ElementType);
        mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(carsData.GetEnumerator());

        _contextMock.Setup(c => c.Cars).Returns(mockSet.Object);

        var result = _carDal.getcar();

        Assert.AreEqual(2, result.Count);
        Assert.Contains(car1, result);
        Assert.Contains(car2, result);
    }


    [Test]
    public void Find_ExistingId_ReturnsCar()
    {
        var car = new Car { CarID = 1 };
        var carsData = new List<Car> { car }.AsQueryable();

        var mockSet = new Mock<DbSet<Car>>();
        mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(carsData.Provider);
        mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(carsData.Expression);
        mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(carsData.ElementType);
        mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(carsData.GetEnumerator());

        _contextMock.Setup(c => c.Cars).Returns(mockSet.Object);

        var result = _carDal.find(1);

        Assert.AreEqual(car, result);
    }


    [Test]
    public void Delete_ExistingId_DeletesFromDatabase()
    {
        var car = new Car { CarID = 1 };
        var carsData = new List<Car> { car }.AsQueryable();

        var mockSet = new Mock<DbSet<Car>>();
        mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(carsData.Provider);
        mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(carsData.Expression);
        mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(carsData.ElementType);
        mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(carsData.GetEnumerator());

        _contextMock.Setup(c => c.Cars).Returns(mockSet.Object);

        var result = _carDal.delete(1);

        mockSet.Verify(m => m.Remove(It.IsAny<Car>()), Times.Once);
        _contextMock.Verify(m => m.SaveChanges(), Times.Once);
    }

    [Test]
    public void Update_ExistingId_UpdatesCar()
    {
        var car = new Car { CarID = 1, CarName = "Old Name" };
        var updatedCar = new Car { CarID = 1, CarName = "New Name" };

        _carSetMock.Setup(m => m.Find(It.IsAny<int>())).Returns(car);

        _carDal.update(1, updatedCar);

        Assert.AreEqual(updatedCar.CarName, car.CarName);
        _contextMock.Verify(m => m.SaveChanges(), Times.Once);
    }

    [Test]
    public void Lock_ExistingId_LocksCar()
    {
        var car = new Car { CarID = 1, Available = true };

        _carSetMock.Setup(m => m.Find(It.IsAny<int>())).Returns(car);

        _carDal.locked(1);

        Assert.IsFalse(car.Available);
        _contextMock.Verify(m => m.SaveChanges(), Times.Once);
    }

    [Test]
    public void Unlock_ExistingId_UnlocksCar()
    {
        var car = new Car { CarID = 1, Available = false };

        _carSetMock.Setup(m => m.Find(It.IsAny<int>())).Returns(car);

        _carDal.unlocked(1);

        Assert.IsTrue(car.Available);
        _contextMock.Verify(m => m.SaveChanges(), Times.Once);
    }
}
