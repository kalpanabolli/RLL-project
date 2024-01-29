using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PROD.Models;

namespace PROD.Controllers
{
    public class CarController : Controller
    {
        // Data access layer instances and database context
        CarDAL cdal;
        CarRentalEntities cd;
        CustomerDAL csdal;
        RentDAL rd;

        // Default constructor
        public CarController()
        {
            cdal = new CarDAL();
            cd = new CarRentalEntities();
            csdal = new CustomerDAL();
            rd = new RentDAL();
        }

        // List all customers for employees
        public ActionResult EmpIndex()
        {
            List<CustomerModel> s = new List<CustomerModel>();
            List<Customer> s1 = csdal.GetCustomers();

            foreach (var item in s1)
            {
                CustomerModel m = new CustomerModel
                {
                    // Convert database entity to model
                    CustomerID = item.CustomerID,
                    CustomerName = item.CustomerName,
                    Password = item.Password,
                    LoyaltyPoints = Convert.ToInt32(item.LoyaltyPoints),
                    Email = item.Email.ToString()
                };
                s.Add(m);
            }
            return View(s);
        }

        // List all cars
        public ActionResult Index()
        {
            List<CarModel> cars = cdal.getcar().Select(c => new CarModel 
            {
                CarID = c.CarID,
                CarName = c.CarName,
                PerDayCharge = c.PerDayCharge,
                ChargePerKm = c.ChargePerKm,
                CarType = c.CarType,
                Available = c.Available,
                Photo = c.Photo
            }).ToList();
            ViewBag.ImagePath = "~/images/";
            return View(cars);
        }
        public ActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminLogin(FormCollection c)
        {
            // Validate admin credentials
            string username = c["Username"].ToString();
            string password = c["Password"].ToString();

            bool isValid = cd.admins.Any(a => a.Username == username && a.Password == password);

            if (isValid)
            {
                Session["u"] = username;
                TempData["u2"] = username;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Message = "Invalid Credentials..Try Again";
                return View();
            }
        }

        public ActionResult Details(int id)
        {
            // Fetch and convert car from database entity to view model
            Car carFromDb = cdal.find(id);

            CarModel viewModel = new CarModel
            {
                CarID = carFromDb.CarID,
                CarName = carFromDb.CarName,
                CarType = carFromDb.CarType,
                ChargePerKm = carFromDb.ChargePerKm,
                PerDayCharge = carFromDb.PerDayCharge,
                Available = carFromDb.Available,
                Photo = carFromDb.Photo
            };
            ViewBag.ImagePath = "~/images/"+viewModel.Photo;

            return View(viewModel);
        }

        

        // Render the create car page
        public ActionResult Create()
        {
            return View(new CarModel {  }); 
        }

        [HttpPost]
        public ActionResult Create(CarModel c)
        {
            try
            {
                // Convert view model to database entity and save
                Car carEntity = new Car
                {
                    CarID = c.CarID,
                    CarName = c.CarName,
                    PerDayCharge = c.PerDayCharge,
                    ChargePerKm = c.ChargePerKm,
                    CarType = c.CarType,
                    Available = c.Available,
                    Photo = c.Photo
                };

                cdal.addcar(carEntity);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Edit(int id)
        {
            // Fetch and convert car from database entity to view model
            Car carFromDb = cdal.find(id);

            CarModel viewModel = new CarModel
            {
                CarID = carFromDb.CarID,
                CarName = carFromDb.CarName,
                CarType = carFromDb.CarType,
                ChargePerKm = carFromDb.ChargePerKm,
                PerDayCharge = carFromDb.PerDayCharge,
                Available = carFromDb.Available,
                Photo = carFromDb.Photo
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(int id, CarModel c)
        {
            try
            {
                // Convert view model to database entity and save changes
                Car carEntity = new Car
                {
                    CarID = c.CarID,
                    CarName = c.CarName,
                    PerDayCharge = c.PerDayCharge,
                    ChargePerKm = c.ChargePerKm,
                    CarType = c.CarType,
                    Available = c.Available,
                    Photo = c.Photo
                };

                cdal.update(id, carEntity);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            // Fetch and convert car from database entity to view model
            Car carFromDb = cdal.find(id);

            CarModel viewModel = new CarModel
            {
                CarID = carFromDb.CarID,
                CarName = carFromDb.CarName,
                CarType = carFromDb.CarType,
                ChargePerKm = carFromDb.ChargePerKm,
                PerDayCharge = carFromDb.PerDayCharge,
                Available = carFromDb.Available,
                Photo = carFromDb.Photo
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id, CarModel c)
        {
            try
            {
                cdal.delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // List all rentals ordered by a specific customer
        public ActionResult OrderedRentals(int id)
        {
            List<Rental> rentalsFromDb = rd.GetAllRents().Where(x => x.CustomerID == id).ToList();

            List<RentModel> viewModelList = rentalsFromDb.Select(r => new RentModel
            {
                RentID = r.RentID,
                CarID = r.CarID,
                CustomerID = r.CustomerID,
                RentOrderDate = r.RentOrderDate,
                ReturnDate = r.ReturnDate,
                LicenseNumber = r.LicenseNumber,
                OdoReading = r.OdoReading,
                ReturnOdoReading = r.ReturnOdoReading
            }).ToList();

            return View(viewModelList);
        }

        // Prevent the back action after admin logs out
        public ActionResult preventBack()
        {
            // Clear admin session and redirect to home page
            string adminName = TempData["u2"].ToString();
            ViewBag.Message = "admin " + adminName;
            Session["u"] = null;
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
    
}
