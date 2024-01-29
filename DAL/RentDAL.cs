using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class RentDAL
    {
        internal CarRentalEntities context = null;
        private CarDAL carDAL = null;

        public RentDAL()
        {
            context = new CarRentalEntities();
            carDAL = new CarDAL();
        }

        public bool RentCar(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            try
            {
                context.Rentals.Add(rental);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Add logging here if necessary
                throw new ApplicationException("Unable to rent the car.", ex);
            }
        }

        public void ReturnCar(int id, Rental rentalDetails)
        {
            var existingRental = context.Rentals.Find(id);
            if (existingRental == null)
                throw new ArgumentException($"No rental record found with ID {id}");

            try
            {
                existingRental.CarID = rentalDetails.CarID;
                existingRental.CustomerID = rentalDetails.CustomerID;
                existingRental.RentOrderDate = rentalDetails.RentOrderDate;
                existingRental.ReturnDate = rentalDetails.ReturnDate;
                existingRental.OdoReading = rentalDetails.OdoReading;
                existingRental.ReturnOdoReading = rentalDetails.ReturnOdoReading;
                existingRental.LicenseNumber = rentalDetails.LicenseNumber;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to return the car.", ex);
            }
        }


        public void CancelRent(int id)
        {
            var rental = context.Rentals.Find(id);
            if (rental == null)
                throw new ArgumentException($"No rental record found with ID {id}");

            try
            {
                context.Rentals.Remove(rental);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to cancel the rent.", ex);
            }
        }

        public Rental FindRent(int id)
        {
            try
            {
                return context.Rentals.FirstOrDefault(carRent => carRent.RentID == id);
            }
            catch
            {
                throw;
            }
        }

        public List<Rental> GetAllRents()
        {
            context.Database.Log = Console.Write; // Add this before the `return` statement in `GetAllRents()`.

            return context.Rentals.ToList();
        }
        public Rental GetRentalById(int rentId)
        {
            return context.Rentals.FirstOrDefault(r => r.RentID == rentId);
        }



        public Tuple<int, double> CalculateCharges(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            CarDAL carDAL = new CarDAL();
            Car rentedCar = carDAL.find(rental.CarID);

            if (rentedCar == null)
                throw new ArgumentException("No car found with the provided car ID.");

            DateTime rentalDate = rental.RentOrderDate;
            DateTime returnDate = rental.ReturnDate;
            int daysRented = (returnDate - rentalDate).Days;
            int kmsDriven = (rental.ReturnOdoReading ?? 0) - (rental.OdoReading ?? 0);
            double chargeForDays = daysRented * Convert.ToDouble(rentedCar.PerDayCharge);
            double chargeForKms = kmsDriven * Convert.ToDouble(rentedCar.ChargePerKm);
            
            double typeMultiplier = 1;
            switch (rentedCar.CarType)
            {
                case "Luxury":
                    typeMultiplier = 1.5;
                    break;
                case "SUV":
                    typeMultiplier = 1.4;
                    break;
                case "Sedan": 
                    typeMultiplier = 1.3;
                    break;
                case "Compact":
                    typeMultiplier = 1.2;
                    break;
                    
            }

            double totalCharge = (chargeForDays + chargeForKms) * typeMultiplier;

            return Tuple.Create(kmsDriven, totalCharge);
        }

    }
}
