using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class CarDAL
    {
        private readonly CarRentalEntities context;

        // Parameterized constructor for Dependency Injection
        public CarDAL(CarRentalEntities dbContext)
        {
            context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Default constructor creates a new instance (for normal runtime)
        public CarDAL() : this(new CarRentalEntities())
        {
        }

        public List<Car> getcar()
        {
            return context.Cars.ToList();
        }

        public bool addcar(Car c)
        {
            try
            {                
                context.Cars.Add(c);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as necessary
                throw new ApplicationException("Unable to add car.", ex);
            }
        }

        public Car find(int id)
        {
            return context.Cars.FirstOrDefault(x => x.CarID == id);
        }

        public bool delete(int id)
        {
            try
            {
                Car carToDelete = context.Cars.FirstOrDefault(x => x.CarID == id);
                if (carToDelete == null)
                {
                    throw new ArgumentException("Car with the specified ID not found.");
                }

                context.Cars.Remove(carToDelete);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to delete car.", ex);
            }
        }

        public void update(int id, Car c)
        {
            try
            {                
                Car carToUpdate = context.Cars.Find(id);
                if (carToUpdate == null)
                {
                    throw new ArgumentException("Car with the specified ID not found.");
                }
                carToUpdate.CarName = c.CarName;
                carToUpdate.PerDayCharge = c.PerDayCharge;
                carToUpdate.ChargePerKm = c.ChargePerKm;
                carToUpdate.Photo = c.Photo;
                carToUpdate.CarType = c.CarType;
                carToUpdate.Available = c.Available;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to update car.", ex);
            }
        }

        public void locked(int id)
        {
            Car carToLock = context.Cars.Find(id);
            if (carToLock == null)
            {
                throw new ArgumentException("Car with the specified ID not found.");
            }
            carToLock.Available = false; 
            context.SaveChanges();
        }

        public void unlocked(int id)
        {
            Car carToUnlock = context.Cars.Find(id);
            if (carToUnlock == null)
            {
                throw new ArgumentException("Car with the specified ID not found.");
            }
            carToUnlock.Available = true; 
            context.SaveChanges();
        }



    }
}
